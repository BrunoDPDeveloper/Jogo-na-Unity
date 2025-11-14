using UnityEngine;
using System.Collections;
using TMPro;

public class ArmaDeRajada : MonoBehaviour
{
    [Header("Configuração de Dano e Alcance")]
    public float damage;
    public float headshotDamage; // <--- NOVO: Dano de Headshot
    public float range;
    public float fireRate; // tiros por segundo dentro da rajada
    public int burstCount;     // quantos tiros por rajada
    public float burstCooldown = 0.5f; // tempo entre rajadas

    [Header("Munição")]
    // [REMOVIDO: TextMeshProUGUI Ammo;] - Gerenciado pelo WeaponSwitching
    public int clipSize;
    public int currentAmmo;
    public int reserveAmmo;
    public int maxReserveAmmo; // <--- NOVO: Limite de Munição de Reserva
    public float reloadTime;
    private bool isReloading = false;
    private Coroutine reloadCoroutine;

    [Header("UI")]
    // [REMOVIDO: TextMeshProUGUI weaponNameText;] - Gerenciado pelo WeaponSwitching
    public string weaponName;

    [Header("Disparo")]
    public Transform firePoint;
    public ParticleSystem muzzleFlash;
    public GameObject bulletImpactEffect;
    public GameObject bloodEffect;

    private float nextTimeToFire = 0f;

    [Header("Áudio")]
    public AudioClip shootSound;
    public AudioClip emptyClipSound;
    private AudioSource audioSource;
    public AudioClip hitmarkerSound;
    public AudioClip headshotSound; // <--- NOVO: Som de Headshot

    public Animator rajadaAnimator;

    private bool isAimed = false;

    [Header("Player Stats")]
    [Tooltip("Valor subtraído da velocidade base do jogador quando esta arma está ativa.")]
    public float moveSpeedPenalty = 0.2f; // Exemplo: penalidade de 0.5 na velocidade base

    void Start()
    {
        isAimed = false;
        currentAmmo = clipSize;
        audioSource = GetComponent<AudioSource>();
    }

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

    // <--- NOVO: Método para parar a recarga quando a arma for desativada (ao trocar) --->
    void OnDisable()
    {
        if (reloadCoroutine != null)
        {
            // Pára a Coroutine de recarga em andamento
            StopCoroutine(reloadCoroutine);

            // Reseta o estado para permitir a recarga quando a arma for reativada
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
        rajadaAnimator.SetBool("Aim", isAimed);

        if (currentAmmo <= 0 && reserveAmmo <= 0 && Input.GetButtonDown("Fire1"))
        {
            if (audioSource != null && emptyClipSound != null)
            {
                audioSource.PlayOneShot(emptyClipSound);
            }
            return;
        }

        // Esta lógica deve estar aqui para recarregar automaticamente quando esvazia o clipe
        if (currentAmmo <= 0 && reserveAmmo > 0)
        {
            reloadCoroutine = StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            StartCoroutine(BurstFire());
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < clipSize && reserveAmmo > 0)
        {
            reloadCoroutine = StartCoroutine(Reload());
            return;
        }
    }

    IEnumerator BurstFire()
    {
        // Trava o fireRate entre as rajadas
        nextTimeToFire = Time.time + 1f / fireRate;
        int shotsFired = 0;

        while (shotsFired < burstCount && currentAmmo > 0)
        {
            Shoot();
            shotsFired++;
            // Espera entre cada tiro dentro da rajada
            yield return new WaitForSeconds(1f / fireRate);
        }

        // Aplica o cooldown entre rajadas
        nextTimeToFire = Time.time + burstCooldown;
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("ArmaDeRajada Reloading...");
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
        reloadCoroutine = null; // Limpa a referência após a conclusão

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

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range))
        {
            // 1. DETECÇÃO DE HEADSHOT
            bool isHeadshot = hit.collider.CompareTag("Headshot");

            Enemy damageEnemy = hit.transform.GetComponent<Enemy>();
            if (damageEnemy == null)
            {
                damageEnemy = hit.transform.GetComponentInParent<Enemy>();
            }

            float finalDamage = damage;
            int hitPoints = 10; // Pontos base por hit

            if (isHeadshot)
            {
                // 2. DEFINE O DANO FINAL E PONTOS PARA HEADSHOT
                finalDamage = headshotDamage;
                hitPoints = 40; // Ex: 40 pontos por acerto na cabeça

                if (damageEnemy != null && headshotSound != null)
                {
                    AudioSource.PlayClipAtPoint(headshotSound, hit.point);
                }
            }

            // 3. APLICAÇÃO DO DANO
            if (damageEnemy != null)
            {
                if (PointManager.Instance != null)
                {
                    PointManager.Instance.AddPoints(hitPoints);
                }

                // Aplica o dano final (damage ou headshotDamage)
                damageEnemy.TakeDamage(finalDamage, isHeadshot);

                // Toca o hitmarker Sound APENAS se não for Headshot
                if (!isHeadshot && hitmarkerSound != null)
                {
                    AudioSource.PlayClipAtPoint(hitmarkerSound, hit.point);
                }

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
                // --- CÓDIGO EXECUTADO SE NÃO ACERTAR UM INIMIGO (ex: parede, caixa) ---
                if (bulletImpactEffect != null)
                {
                    GameObject impactEffect = Instantiate(bulletImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactEffect, 2f);
                }
            }
        }
    }
}
