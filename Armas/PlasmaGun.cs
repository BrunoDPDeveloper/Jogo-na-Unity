using UnityEngine;
using System.Collections;
using TMPro;

public class PlasmaGun : MonoBehaviour
{
    public float damage;
    public float range;
    public float fireRate;
    public GameObject bulletImpactEffect;

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
    private AudioSource plasmaGunaudioSource;
    public AudioClip hitmarkerSound;

    //public Animator mak381Animator;

    void Start()
    {
        currentAmmo = clipSize;
        plasmaGunaudioSource = GetComponent<AudioSource>();
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

    // Este método é removido, pois a UI será gerenciada pelo WeaponSwitching.

    void Update()
    {
        if (PauseMenu.isPaused || PlayerHealth.isDead || MachinePrinterGunsBuy.isBuyScreenOpen) return;
        if (isReloading) return;

        //mak381Animator.SetBool("isADS", Input.GetMouseButton(1));

        if (currentAmmo <= 0 && reserveAmmo <= 0 && Input.GetButtonDown("Fire1"))
        {
            if (plasmaGunaudioSource != null && emptyClipSound != null)
            {
                plasmaGunaudioSource.PlayOneShot(emptyClipSound);
            }
            return;
        }

        if (currentAmmo <= 0 && reserveAmmo > 0)
        {
            reloadCoroutine = StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < clipSize && reserveAmmo > 0)
        {
            // Começa a Coroutine de recarga e armazena sua referência
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
        if (plasmaGunaudioSource != null && shootSound != null)
        {
            plasmaGunaudioSource.PlayOneShot(shootSound);
        }
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        currentAmmo--;

        // Chama o método de atualização da UI do WeaponSwitching
        WeaponSwitching weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching != null)
        {
            weaponSwitching.UpdateAmmoUI();
        }

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Enemy damageEnemy = hit.transform.GetComponent<Enemy>();
            if (damageEnemy != null)
            {
                if (PointManager.Instance != null)
                {
                    PointManager.Instance.AddPoints(10);
                }
                damageEnemy.TakeDamage(damage);
                if (hitmarkerSound != null)
                {
                    AudioSource.PlayClipAtPoint(hitmarkerSound, hit.point);
                }
                if (bulletImpactEffect != null)
                {
                    GameObject impactEffect = Instantiate(bulletImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactEffect, 2f);
                }
            }
            else
            {
                // --- CÓDIGO EXECUTADO SE NÃO ACERTAR UM INIMIGO (ex: parede, caixa) ---

                // Aqui está a sua lógica de impacto:
                if (bulletImpactEffect != null)
                {
                    GameObject impactEffect = Instantiate(bulletImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactEffect, 2f);
                }
            }
        }

    }
}
