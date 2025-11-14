using UnityEngine;
using System.Collections;
using TMPro;

public class Pistol : MonoBehaviour
{
    public float damage;
    public float headshotDamage;
    public float range;
    public float fireRate;
    public GameObject bloodEffect;
    public GameObject bulletImpactEffect;

    // NOVO: Limite de zumbis que o projétil pode penetrar
    [Header("Penetração")]
    [Tooltip("Máximo de alvos inimigos que o projétil pode atravessar.")]
    public int maxPenetrationTargets = 2; // Valor padrão para Pistola

    // Munição
    public int clipSize;
    public int currentAmmo;
    public int reserveAmmo;
    public int maxReserveAmmo;
    public float reloadTime;
    private bool isReloading = false;
    private Coroutine reloadCoroutine;

    // Weapon Name
    public string weaponName;

    // Da onde a arma vai atirar
    public Transform firePoint;
    public ParticleSystem muzzleFlash;

    // Próxima hora para atirar
    private float nextTimeToFire = 0f;

    // Audio
    public AudioClip shootSound;
    public AudioClip emptyClipSound;
    private AudioSource pistolaudioSource;
    public AudioClip hitmarkerSound;
    public AudioClip headshotSound;

    public Animator pistolAnimator;

    private bool isAimed = false;

    // ➡️ NOVO: VARIÁVEL DE PENALIDADE DE VELOCIDADE
    [Header("Player Stats")]
    [Tooltip("Valor subtraído da velocidade base do jogador quando esta arma está ativa.")]
    public float moveSpeedPenalty = 0.5f; // Exemplo: penalidade de 0.5 na velocidade base

    void Start()
    {
        isAimed = false;
        currentAmmo = clipSize;
        // Tenta pegar o AudioSource no próprio objeto.
        if (GetComponent<AudioSource>() != null)
        {
            pistolaudioSource = GetComponent<AudioSource>();
        }
        else
        {
            // Cria um AudioSource se não existir
            pistolaudioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // ⭐ NOVO: Método para retornar a penalidade de velocidade
    public float GetMoveSpeedPenalty()
    {
        return moveSpeedPenalty;
    }
    void OnEnable()
    {
        // Garante que o estado de recarga seja resetado ao ativar a arma
        isReloading = false;

        // Tenta encontrar o WeaponSwitching para garantir que a UI seja exibida corretamente
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

        // Lógica de Mira (ADS)
        if (Input.GetButton("Fire2"))
        {
            isAimed = true;
        }
        else
        {
            isAimed = false;
        }

        pistolAnimator.SetBool("Aim", isAimed);

        // Som de clip vazio
        if (currentAmmo <= 0 && reserveAmmo <= 0 && Input.GetButtonDown("Fire1"))
        {
            if (pistolaudioSource != null && emptyClipSound != null)
            {
                pistolaudioSource.PlayOneShot(emptyClipSound);
            }
            return;
        }

        // Recarga automática
        if (currentAmmo <= 0 && reserveAmmo > 0 && reloadCoroutine == null)
        {
            reloadCoroutine = StartCoroutine(Reload());
            return;
        }

        // Disparo
        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0)
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

        // Atualização da UI
        WeaponSwitching weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching != null)
        {
            weaponSwitching.UpdateAmmoUI();
        }
    }


    void Shoot()
    {
        // Efeitos de tiro
        if (pistolaudioSource != null && shootSound != null)
        {
            pistolaudioSource.PlayOneShot(shootSound);
        }
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        currentAmmo--;

        // Atualização da UI
        WeaponSwitching weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching != null)
        {
            weaponSwitching.UpdateAmmoUI();
        }

        // 🎯 IMPLEMENTAÇÃO DA PENETRAÇÃO DE PROJÉTEIS
        RaycastHit[] hits = Physics.RaycastAll(firePoint.position, firePoint.forward, range);

        // Ordena os hits por distância
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        bool firstHitProcessed = false;
        int targetsHit = 0;

        foreach (RaycastHit hit in hits)
        {
            // Se atingimos o limite de penetração, paramos.
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
                // ⭐ CORREÇÃO CRÍTICA: Se o zumbi estiver morto, ignora este alvo para dano e pontuação.
                // (Requer que o script Enemy tenha 'isDead' ou a variável do ZombieAI possa ser acessada)
                ZombieAI zombieAI = damageEnemy.GetComponent<ZombieAI>();
                if (zombieAI != null && zombieAI.isDead)
                {
                    continue; // Pula este inimigo e tenta o próximo para penetração
                }

                targetsHit++; // Contamos o zumbi como um alvo penetrado

                float finalDamage = damage;
                int hitPoints = 10;

                if (isHeadshot)
                {
                    finalDamage = headshotDamage;
                    hitPoints = 40;
                }

                // 📢 TRATAMENTO DE ÁUDIO DO PRIMEIRO HIT
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

                // Aplica DANO e PONTOS para CADA zumbi atingido
                if (PointManager.Instance != null)
                {
                    PointManager.Instance.AddPoints(hitPoints);
                }
                damageEnemy.TakeDamage(finalDamage, isHeadshot);

                // Instancia o efeito de sangue
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
                // Se acertar uma superfície sólida, a penetração para.
                if (bulletImpactEffect != null)
                {
                    GameObject impactEffect = Instantiate(bulletImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactEffect, 2f);
                }
                break; // PÁRA o loop: A bala não atravessa paredes.
            }
        }
    }
}