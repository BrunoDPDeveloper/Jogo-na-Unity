using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class AssaultRifle : MonoBehaviour
{
    // ⭐ NOVO: PROGRESSÃO DE XP/NÍVEL DA ARMA
    [Header("Weapon Progression (Level)")]
    public WeaponXPData weaponXP = new WeaponXPData();

    // Weapon Tier System
    public Color tierColor = Color.white;
    public string tierName;
    [Header("Tier Progression")]
    public WeaponTier[] tierProgression; // Assumimos que WeaponTier é uma struct/classe existente.
    private int currentTierIndex = 0;

    //Weapon Damage
    private float originalDamage;
    private float originalHeadshotDamage; // ⭐ NOVO: Variável para salvar o valor base
    private int originalClipSize;
    private float originalFireRate;

    public float damage;
    public float headshotDamage;
    public float range;
    public float fireRate;

    // Munição
    public int clipSize;
    public int currentAmmo;
    public int reserveAmmo;
    public int maxReserveAmmo;
    public float reloadTime;
    private bool isReloading = false;
    private Coroutine reloadCoroutine;

    // Arma
    public string weaponName; // ⭐ IMPORTANTE: Deve ser único para o PlayerPrefs
    [Header("UI Display")]
    public Sprite weaponIcon;

    // Da onde a arma vai atirar
    public Transform firePoint;
    public ParticleSystem muzzleFlash;

    // Próxima hora para atirar
    private float nextTimeToFire = 0f;

    // Audio
    public AudioClip shootSound;
    public AudioClip emptyClipSound;
    private AudioSource audioSource;
    public AudioClip hitmarkerSound;
    public AudioClip headshotSound;
    public GameObject bloodEffect;
    public GameObject bulletImpactEffect;

    // Mira
    public Animator assaultRifleAnimator;

    private bool isAimed = false;

    // ➡️ VARIÁVEL DE PENALIDADE DE VELOCIDADE
    [Header("Player Stats")]
    [Tooltip("Valor subtraído da velocidade base do jogador quando esta arma está ativa.")]
    public float moveSpeedPenalty = 0.5f;

    // ➡️ VARIÁVEL DE PENETRAÇÃO PARA AR
    [Header("Penetração")]
    [Tooltip("Máximo de alvos inimigos que o projétil pode atravessar.")]
    public int maxPenetrationTargets = 3;

    // ➡️ VARIÁVEIS PARA OBJECT POOLING
    private Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();
    [Header("Pooling")]
    public int poolSize = 10;

    void Start()
    {
        isAimed = false;
        currentAmmo = clipSize;

        if (GetComponent<AudioSource>() != null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ⭐ Salva todos os valores originais ANTES do Load
        originalDamage = damage;
        originalHeadshotDamage = headshotDamage; // ⭐ NOVO
        originalClipSize = clipSize;
        originalFireRate = fireRate;

        // ⭐ CARREGA OS DADOS DE XP DA ARMA
        weaponXP.LoadXP(weaponName);

        // ⭐ NOVO: Carregar Tier Index e Aplicar Tier Inicial
        currentTierIndex = PlayerPrefs.GetInt(weaponName + "_TierIndex", 0);
        UpdateTierStats(currentTierIndex);
        // ⭐ FIM LOAD

        // ➡️ INICIALIZA O POOL para os efeitos de sangue e impacto
        InitializePool(bloodEffect, poolSize);
        InitializePool(bulletImpactEffect, poolSize);
    }

    // ⭐ Implementação do método de atualização do Tier
    private void UpdateTierStats(int newTierIndex)
    {
        if (tierProgression == null || newTierIndex < 0 || newTierIndex >= tierProgression.Length)
        {
            return;
        }

        WeaponTier newTier = tierProgression[newTierIndex];

        // 1. Aplica as cores e nomes do Tier
        tierName = newTier.tierName;
        tierColor = newTier.tierColor;
        currentTierIndex = newTierIndex;

        // Salva o novo Tier Index
        PlayerPrefs.SetInt(weaponName + "_TierIndex", currentTierIndex);
        PlayerPrefs.Save();

        // 2. ⭐ APLICA OS NOVOS STATUS DA ARMA ⭐

        // Reseta os valores para os originais antes de aplicar o Tier atual
        damage = originalDamage;
        headshotDamage = originalHeadshotDamage;
        fireRate = originalFireRate;

        // Aplica os multiplicadores do Tier atual

        damage *= newTier.damageMultiplier;
        headshotDamage *= newTier.damageMultiplier;
        headshotDamage += newTier.bonusHeadshotDamage;

        fireRate *= newTier.fireRateMultiplier;

        // 3. Notifica a UI de que a arma mudou 
        WeaponSwitching weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching != null)
        {
            weaponSwitching.UpdateWeaponUI();
        }
    }

    // ⭐ Método para buscar os dados do PRÓXIMO upgrade.
    public (string tierName, Color tierColor, int cost, string componentKey) GetNextUpgradeInfo()
    {
        int nextTierIndex = currentTierIndex + 1;

        // Checa se já está no Tier Máximo
        if (nextTierIndex >= tierProgression.Length)
        {
            // Retorna um valor "Max Tier" para a UI exibir "Tier Máximo"
            return ("Tier Máximo", Color.yellow, 0, "");
        }

        WeaponTier nextTier = tierProgression[nextTierIndex];

        return (
            nextTier.tierName,
            nextTier.tierColor,
            nextTier.componentQuantityNeeded,
            nextTier.componentKeyNeeded
        );
    }

    /// <summary>
    /// Simula o upgrade para o próximo Tier e retorna os novos valores de dano para a UI.
    /// </summary>
    public (float newDamage, float newHeadshotDamage) GetSimulatedNextTierStats()
    {
        int nextTierIndex = currentTierIndex + 1;

        if (nextTierIndex >= tierProgression.Length)
        {
            // Retorna o dano atual se for Tier Máximo (para não mudar os números)
            return (damage, headshotDamage);
        }

        WeaponTier nextTier = tierProgression[nextTierIndex];

        // Simula a aplicação dos multiplicadores ao DANO ORIGINAL
        float simulatedDamage = originalDamage * nextTier.damageMultiplier;
        float simulatedHeadshotDamage = originalHeadshotDamage * nextTier.damageMultiplier;
        simulatedHeadshotDamage += nextTier.bonusHeadshotDamage;

        return (simulatedDamage, simulatedHeadshotDamage);
    }

    // ⭐ Método para fazer o Upgrade REAL (Chamado pelo botão da UI)
    public bool TryUpgradeTier(PlayerCollector collector)
    {
        int nextTierIndex = currentTierIndex + 1;

        if (nextTierIndex >= tierProgression.Length)
        {
            Debug.Log("Arma já está no Tier Máximo.");
            return false;
        }

        WeaponTier nextTier = tierProgression[nextTierIndex];

        // 1. Verifica e Consome os componentes
        if (collector.ConsumeComponent(nextTier.componentKeyNeeded, nextTier.componentQuantityNeeded))
        {
            // 2. Aplica o novo Tier
            UpdateTierStats(nextTierIndex);

            Debug.Log($"Upgrade da {weaponName} para o Tier {nextTier.tierName} bem-sucedido!");

            // Retorna TRUE para dizer que o upgrade foi bem-sucedido
            return true;
        }

        // Retorna FALSE para dizer que o upgrade falhou (falta componente)
        Debug.Log($"Falha no upgrade: Faltam {nextTier.componentQuantityNeeded}x {nextTier.componentKeyNeeded}.");
        return false;
    }

    public float GetMoveSpeedPenalty()
    {
        return moveSpeedPenalty;
    }

    // ➡️ Métodos de Object Pooling (InitializePool, GetPooledObject, SpawnPooledObject, ReturnToPoolAfterTime)
    void InitializePool(GameObject prefab, int size)
    {
        if (prefab == null) return;

        pooledObjects[prefab] = new List<GameObject>();
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);
            obj.SetActive(false);
            pooledObjects[prefab].Add(obj);
        }
    }

    GameObject GetPooledObject(GameObject prefab)
    {
        if (!pooledObjects.ContainsKey(prefab)) return null;

        List<GameObject> pool = pooledObjects[prefab];
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        GameObject newObj = Instantiate(prefab);
        newObj.SetActive(false);
        pool.Add(newObj);
        return newObj;
    }

    void SpawnPooledObject(GameObject prefab, Vector3 position, Quaternion rotation, float duration)
    {
        GameObject obj = GetPooledObject(prefab);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            StartCoroutine(ReturnToPoolAfterTime(obj, prefab, duration));
        }
    }

    IEnumerator ReturnToPoolAfterTime(GameObject obj, GameObject prefab, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            obj.SetActive(false);
        }
    }
    // ... (Fim dos Métodos de Object Pooling) ...

    void OnEnable()
    {
        isReloading = false;

        WeaponSwitching weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching != null)
        {
            // Chama a atualização da UI ao ativar a arma (para mostrar o Tier atual, etc.)
            weaponSwitching.UpdateWeaponUI();
        }
    }

    void OnDisable()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
            reloadCoroutine = null;
            Debug.Log($"Recarga da {weaponName} interrompida.");
        }
    }

    void Update()
    {
        if (PauseMenu.isPaused || PlayerHealth.isDead || MachinePrinterGunsBuy.isBuyScreenOpen || ComputerTerminal.isTerminalOpen) return;
        if (isReloading) return;

        if (Input.GetButton("Fire2"))
        {
            isAimed = true;
        }
        else
        {
            isAimed = false;
        }

        assaultRifleAnimator.SetBool("Aim", isAimed);

        // Som de clip vazio
        if (currentAmmo <= 0 && reserveAmmo <= 0 && Input.GetButton("Fire1"))
        {
            audioSource.PlayOneShot(emptyClipSound);
            return;
        }

        // Recarga automática
        if (currentAmmo <= 0 && reserveAmmo > 0 && reloadCoroutine == null)
        {
            reloadCoroutine = StartCoroutine(Reload());
            return;
        }

        // Disparo (AUTOMÁTICO)
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

        // Recarga manual
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < clipSize && reserveAmmo > 0)
        {
            reloadCoroutine = StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading");
        // Adicione Animação de recarga aqui
        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = clipSize - currentAmmo;

        if (reserveAmmo >= ammoNeeded)
        {
            currentAmmo += ammoNeeded;
            reserveAmmo -= ammoNeeded;
        }
        else
        {
            currentAmmo += reserveAmmo;
            reserveAmmo = 0;
        }

        isReloading = false;
        reloadCoroutine = null;

        // Notifica o WeaponSwitching
        WeaponSwitching weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching != null)
        {
            weaponSwitching.UpdateAmmoUI();
        }
    }

    void Shoot()
    {
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        currentAmmo--;

        // Notifica o WeaponSwitching
        WeaponSwitching weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching != null)
        {
            weaponSwitching.UpdateAmmoUI();
        }

        // 🎯 IMPLEMENTAÇÃO DA PENETRAÇÃO DE PROJÉTEIS
        RaycastHit[] hits = Physics.RaycastAll(firePoint.position, firePoint.forward, range);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        bool firstHitProcessed = false;
        int targetsHit = 0;

        foreach (RaycastHit hit in hits)
        {
            if (targetsHit >= maxPenetrationTargets)
            {
                break;
            }

            bool isHeadshot = hit.collider.CompareTag("Headshot");

            Enemy damageEnemy = hit.transform.GetComponent<Enemy>();
            if (damageEnemy == null)
            {
                damageEnemy = hit.transform.GetComponentInParent<Enemy>();
            }

            if (damageEnemy != null)
            {
                targetsHit++;

                float finalDamage = damage;
                int hitPoints = 10;
                float gunXP_Gained = 5f; // XP BASE POR TIRO

                if (isHeadshot)
                {
                    finalDamage = headshotDamage;
                    hitPoints = 40;
                    gunXP_Gained = 10f; // Bônus de XP por Headshot
                }

                // 📢 TRATAMENTO DE ÁUDIO DO PRIMEIRO HIT (Hitmarker/Headshot)
                if (!firstHitProcessed)
                {
                    firstHitProcessed = true;
                    if (isHeadshot && headshotSound != null)
                    {
                        AudioSource.PlayClipAtPoint(headshotSound, hit.point, 0.5f);
                    }
                    else if (hitmarkerSound != null)
                    {
                        AudioSource.PlayClipAtPoint(hitmarkerSound, hit.point, 0.5f);
                    }
                }

                if (PointManager.Instance != null)
                {
                    PointManager.Instance.AddPoints(hitPoints);
                }

                // ⭐ CONCEDE XP DA ARMA (Aqui está a chamada)
                AddGunXP(gunXP_Gained);
                // ⭐ FIM XP DA ARMA

                damageEnemy.TakeDamage(finalDamage, isHeadshot);

                // Instancia o efeito de sangue (USANDO POOLING)
                if (bloodEffect != null)
                {
                    Vector3 spawnPos = hit.point + hit.transform.forward * 0.1f;
                    SpawnPooledObject(
                        bloodEffect,
                        spawnPos,
                        Quaternion.LookRotation(hit.transform.forward),
                        1f
                    );
                }
            }
            else
            {
                // Se acertar uma superfície sólida, a bala não penetra mais.
                // Instancia o efeito de impacto (USANDO POOLING)
                if (bulletImpactEffect != null)
                {
                    SpawnPooledObject(
                        bulletImpactEffect,
                        hit.point,
                        Quaternion.LookRotation(hit.normal),
                        2f
                    );
                }
                break; // PÁRA o loop
            }
        } // Fim do foreach
    }

    // ⭐ MÉTODO PARA CONCEDER XP À ARMA
    public void AddGunXP(float amount)
    {
        if (weaponXP.currentGunLevel >= weaponXP.maxGunLevel) return;

        weaponXP.currentGunXP += amount;

        CheckForGunLevelUp();

        // ⭐ SALVA O PROGRESSO APÓS GANHAR XP
        weaponXP.SaveXP(weaponName);
    }

    // ⭐ MÉTODO PARA CHECAR O LEVEL UP DA ARMA
    private void CheckForGunLevelUp()
    {
        while (weaponXP.currentGunLevel < weaponXP.maxGunLevel && weaponXP.currentGunXP >= weaponXP.RequiredXP)
        {
            weaponXP.currentGunXP -= weaponXP.RequiredXP;
            weaponXP.currentGunLevel++;

            Debug.Log($"{weaponName} Level UP! Novo Nível: {weaponXP.currentGunLevel}");

            if (WeaponLevelUpUI.Instance != null)
            {
                // Assumindo que você tem este script
                WeaponLevelUpUI.Instance.DisplayWeaponLevelUp(
                    weaponName,
                    weaponXP.currentGunLevel,
                    weaponIcon
                );
            }
        }

        if (weaponXP.currentGunLevel >= weaponXP.maxGunLevel)
        {
            weaponXP.currentGunXP = 0f;
            Debug.Log($"Parabéns! {weaponName} atingiu o Nível Máximo ({weaponXP.maxGunLevel})!");
        }
    }
}