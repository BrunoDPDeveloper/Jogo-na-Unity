using UnityEngine;
using System.Collections;
using TMPro;

public class Shotgun : MonoBehaviour
{
    // --- VARIÁVEIS ORIGINAIS (Definidas diretamente no Inspetor) ---
    [Header("Weapon Stats")]
    public float damage; // Dano por pellet no corpo
    public float headshotDamage; // Dano por pellet na cabeça
    public float range;
    public float fireRate;

    [Header("Ammo & Reload")]
    public int clipSize;
    public int currentAmmo;
    public int reserveAmmo;
    public int maxReserveAmmo;
    public float reloadTime;
    private bool isReloading = false;
    private Coroutine reloadCoroutine;

    [Header("References")]
    public string weaponName;
    public Transform firePoint;
    public ParticleSystem muzzleFlash;
    private float nextTimeToFire = 0f;
    private AudioSource audioSource;
    public GameObject bloodEffect;
    public GameObject bulletImpactEffect;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip emptyClipSound;
    public AudioClip hitmarkerSound;
    public AudioClip headshotSound;

    [Header("Shotgun & Animation Settings")]
    public int pelletsPerShot = 6;
    public float spreadAngle = 10f;
    public Animator shotgunAnimator;
    private bool isAimed = false;

    [Header("Player Stats")]
    [Tooltip("Valor subtraído da velocidade base do jogador quando esta arma está ativa.")]
    public float moveSpeedPenalty = 1.2f; // Exemplo: penalidade de 0.5 na velocidade base

    // 🎯 NOVO: VARIÁVEL DE PENETRAÇÃO POR PELLET
    [Header("Penetração por Pellet")]
    [Tooltip("Máximo de alvos inimigos que CADA pellet pode atravessar. 1 significa sem penetração.")]
    public int maxPenetrationTargets = 2; // Padrão: cada pellet atinge 2 zumbis.

    // --- PROPRIEDADES DE CONVENIÊNCIA (Para a UI ou Debug) ---

    /// <summary> Dano Total Estimado por Tiro (Corpo) - Para UI. </summary>
    public float TotalBodyDamage
    {
        get { return damage * pelletsPerShot; }
    }

    /// <summary> Dano Total Estimado por Headshot - Para UI. </summary>
    public float TotalHeadshotDamage
    {
        get { return headshotDamage * pelletsPerShot; }
    }

    void Start()
    {
        isAimed = false;
        currentAmmo = clipSize;

        if (GetComponent<AudioSource>() == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Garante que o dano por Headshot é pelo menos o dano normal se não estiver configurado
        if (headshotDamage <= damage)
        {
            headshotDamage = damage * 2f; // Multiplicador padrão se for 0/não configurado
        }
    }

    public float GetMoveSpeedPenalty()
    {
        return moveSpeedPenalty;
    }
    void OnEnable()
    {
        isReloading = false;

        WeaponSwitching weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching != null)
        {
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

        // Lógica de Mira
        isAimed = Input.GetButton("Fire2");
        shotgunAnimator.SetBool("Aim", isAimed);

        // Lógica de Clip Vazio / Recarga Automática
        if (currentAmmo <= 0 && reserveAmmo <= 0 && Input.GetButton("Fire1"))
        {
            if (audioSource != null && emptyClipSound != null)
            {
                audioSource.PlayOneShot(emptyClipSound);
            }
            return;
        }

        if (currentAmmo <= 0 && reserveAmmo > 0)
        {
            if (reloadCoroutine == null) // Evita iniciar várias coroutines de recarga
            {
                reloadCoroutine = StartCoroutine(Reload());
            }
            return;
        }

        // Lógica de Tiro (Shotgun geralmente é tiro único: GetButtonDown)
        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

        // Lógica de Recarga Manual
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < clipSize && reserveAmmo > 0)
        {
            reloadCoroutine = StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading");
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

        WeaponSwitching weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching != null)
        {
            weaponSwitching.UpdateWeaponUI();
        }
    }

    void Shoot()
    {
        // 1. Efeitos e Munição
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        currentAmmo--;

        WeaponSwitching weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching != null)
        {
            weaponSwitching.UpdateAmmoUI();
        }

        bool headshotPlayed = false;
        bool hitmarkerPlayed = false;
        int totalPoints = 0;

        // 2. Loop de Pellets
        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 randomSpread = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0
            ) * firePoint.forward;

            // 🎯 USAMOS RAYCASTALL para detecção de penetração por pellet
            RaycastHit[] hits = Physics.RaycastAll(firePoint.position, randomSpread, range);
            System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

            int targetsHit = 0;

            // 3. Loop de Penetração para CADA Pellet
            foreach (RaycastHit hit in hits)
            {
                // Se atingiu o limite de penetração, para o pellet atual
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
                    targetsHit++; // Conta o zumbi como atingido por este pellet

                    float finalDamage = damage;
                    int hitPoints = 10;

                    if (isHeadshot)
                    {
                        finalDamage = headshotDamage;
                        hitPoints = 40;
                    }

                    totalPoints += hitPoints;
                    damageEnemy.TakeDamage(finalDamage, isHeadshot);

                    // 4. Tratamento de Áudio (Apenas o primeiro hit/pellet)
                    if (isHeadshot && !headshotPlayed && headshotSound != null)
                    {
                        AudioSource.PlayClipAtPoint(headshotSound, hit.point, 0.5f);
                        headshotPlayed = true;
                    }
                    else if (!headshotPlayed && !hitmarkerPlayed && hitmarkerSound != null)
                    {
                        AudioSource.PlayClipAtPoint(hitmarkerSound, hit.point, 0.5f);
                        hitmarkerPlayed = true;
                    }

                    // 5. Efeito de Sangue
                    if (bloodEffect != null)
                    {
                        Vector3 spawnPos = hit.point + hit.transform.forward * 0.1f;
                        GameObject blood = Instantiate(bloodEffect, spawnPos, Quaternion.LookRotation(hit.transform.forward));
                        blood.transform.SetParent(hit.transform);
                        Destroy(blood, 1f);
                    }
                }
                else
                {
                    // Se o pellet atingir uma superfície sólida, ele para.
                    if (bulletImpactEffect != null)
                    {
                        GameObject impactEffect = Instantiate(bulletImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(impactEffect, 2f);
                    }
                    break; // Pára a penetração para este pellet
                }
            }
        }

        // 6. Aplicação dos Pontos Totais
        if (PointManager.Instance != null && totalPoints > 0)
        {
            PointManager.Instance.AddPoints(totalPoints);
        }
    }
}