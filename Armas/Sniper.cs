using UnityEngine;
using System.Collections;
using TMPro;

public class Sniper : MonoBehaviour
{
    public float damage;
    public float headshotDamage;
    public float range;
    public float fireRate;
    public GameObject bloodEffect;
    public GameObject bulletImpactEffect;
    public GameObject sniperScope;
    public GameObject weaponCamera;

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
    private AudioSource sniperAudioSource;
    public AudioClip hitmarkerSound;
    public AudioClip headshotSound;

    public Animator sniperAnimator;

    private bool isAimed = false;

    [Header("Player Stats")]
    [Tooltip("Valor subtraído da velocidade base do jogador quando esta arma está ativa.")]
    public float moveSpeedPenalty = 2f; // Exemplo: penalidade de 0.5 na velocidade base

    void Start()
    {
        isAimed = false;
        currentAmmo = clipSize;
        // Tenta pegar o AudioSource no próprio objeto.
        if (GetComponent<AudioSource>() != null)
        {
            sniperAudioSource = GetComponent<AudioSource>();
        }
        else
        {
            // Cria um AudioSource se não existir (opcional)
            sniperAudioSource = gameObject.AddComponent<AudioSource>();
        }
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

    // Método para parar a recarga quando a arma for desativada (ao trocar)
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
        // Certifique-se de que todas as classes estáticas estão corretamente referenciadas
        if (PauseMenu.isPaused || PlayerHealth.isDead || MachinePrinterGunsBuy.isBuyScreenOpen || ComputerTerminal.isTerminalOpen) return;
        if (isReloading) return;

        // Lógica de Mira (ADS)
        if (Input.GetButton("Fire2"))
        {
            isAimed = true;
            if (isAimed)
            {
                StartCoroutine(OnScoped());
            }
        }
        else
        {
            isAimed = false;
            OnUnoscoped();


        }
        sniperAnimator.SetBool("Aim", isAimed);

        void OnUnoscoped()
        {
            sniperScope.SetActive(false);
            weaponCamera.SetActive(true);

        }

        IEnumerator OnScoped()
        {
            yield return new WaitForSeconds(.15f);
            sniperScope.SetActive(true);
            weaponCamera.SetActive(false);
        }

        // Verifica munição vazia (som de clique)
        if (currentAmmo <= 0 && reserveAmmo <= 0 && Input.GetButtonDown("Fire1"))
        {
            if (sniperAudioSource != null && emptyClipSound != null)
            {
                sniperAudioSource.PlayOneShot(emptyClipSound);
            }
            return;
        }

        // Recarga automática (após o pente esvaziar)
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
        // Adicione animação de recarga aqui (sniperAnimator.SetTrigger("Reload");)

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

        // Chama o método de atualização da UI do WeaponSwitching
        WeaponSwitching weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching != null)
        {
            weaponSwitching.UpdateAmmoUI();
        }
    }


    void Shoot()
    {
        // Efeitos de tiro
        if (sniperAudioSource != null && shootSound != null)
        {
            sniperAudioSource.PlayOneShot(shootSound);
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

        // 🎯 IMPLEMENTAÇÃO DA PENETRAÇÃO DE PROJÉTEIS (LINE UP SHOTS)
        RaycastHit[] hits = Physics.RaycastAll(firePoint.position, firePoint.forward, range);

        // Ordena os hits por distância (crucial para processar o dano na ordem correta)
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        bool firstHitProcessed = false; // Flag para garantir que Headshot/Hitmarker toque apenas uma vez

        foreach (RaycastHit hit in hits)
        {
            bool isHeadshot = hit.collider.CompareTag("Headshot");

            Enemy damageEnemy = hit.transform.GetComponent<Enemy>();
            if (damageEnemy == null)
            {
                damageEnemy = hit.transform.GetComponentInParent<Enemy>();
            }

            if (damageEnemy != null)
            {
                // Variáveis de Dano/Pontos
                float finalDamage = damage;
                int hitPoints = 10;

                if (isHeadshot)
                {
                    finalDamage = headshotDamage;
                    hitPoints = 40;
                }

                // 📢 TRATAMENTO DE ÁUDIO DO PRIMEIRO HIT (Headshot ou Hitmarker)
                if (!firstHitProcessed)
                {
                    firstHitProcessed = true;
                    if (isHeadshot && headshotSound != null)
                    {
                        // Headshot: Toca o som e não toca o Hitmarker padrão.
                        AudioSource.PlayClipAtPoint(headshotSound, hit.point, 0.5f);
                    }
                    else if (hitmarkerSound != null)
                    {
                        // Acerto normal: Toca o Hitmarker.
                        AudioSource.PlayClipAtPoint(hitmarkerSound, hit.point, 0.5f);
                    }
                }

                // Aplica DANO e PONTOS para CADA zumbi atingido (Penetração)
                if (PointManager.Instance != null)
                {
                    PointManager.Instance.AddPoints(hitPoints);
                }
                damageEnemy.TakeDamage(finalDamage, isHeadshot);

                // Instancia o efeito de sangue (Blood Effect)
                if (bloodEffect != null)
                {
                    // Ligeiro offset para evitar z-fighting
                    Vector3 spawnPos = hit.point + hit.transform.forward * 0.1f;
                    GameObject blood = Instantiate(bloodEffect, spawnPos, Quaternion.LookRotation(hit.transform.forward));
                    blood.transform.SetParent(hit.transform);
                    Destroy(blood, 1f);
                }
            }
            else
            {
                // Se acertar uma superfície sólida (parede, chão, etc.), a penetração para.
                if (bulletImpactEffect != null)
                {
                    GameObject impactEffect = Instantiate(bulletImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactEffect, 2f);
                }

                // PÁRA o loop: A bala não atravessa objetos sólidos.
                break;
            }
        } // Fim do foreach (RaycastAll)
    }
}