using UnityEngine;
using TMPro;

public class WeaponSwitching : MonoBehaviour
{
    // A arma atualmente selecionada (índice 0, 1, etc.)
    public int selectedWeapon = 0;

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI weaponNameText;

    [Header("Referências")]
    // O objeto que contém os GameObjects das armas.
    // Esta variável 'weaponHolder' é redundante, pois o próprio 'transform' do script está sendo usado.
    // Mantenha para evitar erros no Inspector, mas o 'transform' é usado no código.
    public Transform weaponHolder;

    // O valor que será adicionado à munição de reserva ao comprar recarga.
    private const int AMMO_REFILL_AMOUNT = 50;

    // O limite máximo de armas que o jogador pode carregar.
    private const int WEAPON_LIMIT = 2;

    void Start()
    {
        selectWeapon();
        UpdateWeaponUI();
    }

    void Update()
    {
        int previousSelectedWeapon = selectedWeapon;

        // Troca para a primeira arma
        if (Input.GetKeyDown(KeyCode.Alpha1) && transform.childCount >= 1)
        {
            selectedWeapon = 0;
        }
        // Troca para a segunda arma (agora limitada a 2)
        if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
        {
            selectedWeapon = 1;
        }

        if (previousSelectedWeapon != selectedWeapon)
        {
            selectWeapon();
            UpdateWeaponUI();
        }
    }

    void selectWeapon()
    {
        int i = 0;
        MonoBehaviour activeWeaponComponent = null; // ⭐ NOVO: Variável para armazenar o script da arma ativa

        // Percorre todos os filhos (armas) do WeaponHolder
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true); // Liga a arma selecionada

                // ⭐ NOVO: Tenta obter o script da arma ativa
                activeWeaponComponent = GetWeaponComponent(weapon);
            }
            else
                weapon.gameObject.SetActive(false); // Desliga as outras
            i++;
        }

        // ⭐ NOVO: Notifica o PlayerController sobre a penalidade de velocidade
        // Assumindo que PlayerController.Instance existe
        if (PlayerController.Instance != null)
        {
            if (activeWeaponComponent != null)
            {
                PlayerController.Instance.UpdateActiveWeaponPenalty(activeWeaponComponent);
            }
            else
            {
                PlayerController.Instance.ClearActiveWeaponPenalty();
            }
        }
    }

    // NOVA LÓGICA DE COMPRA E TROCA DE ARMA
    public void AddNewWeapon(GameObject weaponPrefab)
    {
        // NOTA: O 'weaponHolder' é redundante se você usa 'transform', mas mantive a checagem.
        if (weaponHolder == null)
        {
            // Se weaponHolder não estiver definido, assuma que é o próprio transform.
            weaponHolder = transform;
        }

        // 1. VERIFICAÇÃO E TROCA DO LIMITE DE ARMAS
        if (transform.childCount >= WEAPON_LIMIT)
        {
            // Destrua a arma antiga ANTES de instanciar a nova.
            Transform weaponToDestroy = transform.GetChild(selectedWeapon);

            // ⭐ NOVO: Limpa a penalidade antes de destruir a arma
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.ClearActiveWeaponPenalty();
            }

            // Usar DestroyImmediate é mais seguro aqui para garantir que o objeto seja removido
            // do transform.childCount antes da próxima instrução.
            DestroyImmediate(weaponToDestroy.gameObject);
        }

        // 2. INSTANCIAÇÃO DA NOVA ARMA
        // Instancia a nova arma como filha do WeaponHolder (que é 'transform' aqui).
        GameObject newWeapon = Instantiate(weaponPrefab, transform);
        newWeapon.transform.SetParent(transform);

        // 3. SELEÇÃO DA NOVA ARMA
        // A nova arma será o último filho da lista.
        selectedWeapon = transform.childCount - 1;

        // 4. ATUALIZAÇÃO DA UI E SELEÇÃO
        // selectWeapon() chamará a atualização da penalidade de velocidade.
        selectWeapon();
        UpdateWeaponUI();
    }

    // CORREÇÃO CRÍTICA: Verifica se a arma já é filha do WeaponSwitching
    // A verificação pelo nome completo do prefab é a forma mais segura.
    public bool HasWeapon(string weaponPrefabName)
    {
        // Itera sobre todas as armas que o jogador possui
        foreach (Transform weapon in transform)
        {
            // Compara o nome do objeto instanciado (que geralmente é "NomeDoPrefab(Clone)")
            // com o nome do Prefab que você está tentando comprar.
            if (weapon.gameObject.name.Contains(weaponPrefabName))
            {
                return true;
            }
        }
        return false;
    }

    // ⭐ NOVO: Método auxiliar para obter o script da arma como MonoBehaviour (para PlayerController)
    // Usado também nas funções de UI/Ammo para obter o componente
    private MonoBehaviour GetWeaponComponent(Transform weaponTransform)
    {
        if (weaponTransform.TryGetComponent<Pistol>(out Pistol pistolScript))
            return pistolScript;
        if (weaponTransform.TryGetComponent<Sniper>(out Sniper sniperScript))
            return sniperScript;
        if (weaponTransform.TryGetComponent<Shotgun>(out Shotgun shotgunScript))
            return shotgunScript;
        if (weaponTransform.TryGetComponent<ArmaDeRajada>(out ArmaDeRajada armaDeRajadaScript))
            return armaDeRajadaScript;
        if (weaponTransform.TryGetComponent<AssaultRifle>(out AssaultRifle assaultRifleScript))
            return assaultRifleScript;
        if (weaponTransform.TryGetComponent<PlasmaGun>(out PlasmaGun plasmaGunScript))
            return plasmaGunScript;

        return null;
    }

    // Adiciona munição à reserva da arma existente, respeitando o limite máximo.
    public bool AddAmmoToWeapon(string weaponPrefabName)
    {
        // CORREÇÃO: Usar transform.Find é arriscado devido ao "(Clone)".
        // Usaremos a iteração para encontrar a arma, tornando esta função mais robusta.
        Transform weaponTransform = null;
        foreach (Transform weapon in transform)
        {
            if (weapon.gameObject.name.Contains(weaponPrefabName))
            {
                weaponTransform = weapon;
                break;
            }
        }

        if (weaponTransform != null)
        {
            // Mantendo a lógica de checagem de GetComponent original:
            Pistol pistolScript = weaponTransform.GetComponent<Pistol>();
            Sniper sniperScript = weaponTransform.GetComponent<Sniper>();
            Shotgun shotgunScript = weaponTransform.GetComponent<Shotgun>();
            ArmaDeRajada armaDeRajadaScript = weaponTransform.GetComponent<ArmaDeRajada>();
            AssaultRifle assaultRifleScript = weaponTransform.GetComponent<AssaultRifle>();
            PlasmaGun plasmaGunScript = weaponTransform.GetComponent<PlasmaGun>();
            // Adicione outras armas aqui

            // Lógica para PISTOL
            if (pistolScript != null)
            {
                if (pistolScript.reserveAmmo >= pistolScript.maxReserveAmmo)
                    return false;

                pistolScript.reserveAmmo += AMMO_REFILL_AMOUNT;
                pistolScript.reserveAmmo = Mathf.Min(pistolScript.reserveAmmo, pistolScript.maxReserveAmmo);
                UpdateAmmoUI(); // Adicionar atualização da UI
                return true;
            }
            // Lógica para SNIPER
            else if (sniperScript != null)
            {
                if (sniperScript.reserveAmmo >= sniperScript.maxReserveAmmo)
                    return false;

                sniperScript.reserveAmmo += AMMO_REFILL_AMOUNT;
                sniperScript.reserveAmmo = Mathf.Min(sniperScript.reserveAmmo, sniperScript.maxReserveAmmo);
                UpdateAmmoUI(); // Adicionar atualização da UI
                return true;
            }

            else if (shotgunScript != null)
            {
                if (shotgunScript.reserveAmmo >= shotgunScript.maxReserveAmmo)
                    return false;

                shotgunScript.reserveAmmo += AMMO_REFILL_AMOUNT;
                shotgunScript.reserveAmmo = Mathf.Min(shotgunScript.reserveAmmo, shotgunScript.maxReserveAmmo);
                UpdateAmmoUI(); // Adicionar atualização da UI
                return true;
            }
            // Lógica para ARMA DE RAJADA
            else if (armaDeRajadaScript != null)
            {
                if (armaDeRajadaScript.reserveAmmo >= armaDeRajadaScript.maxReserveAmmo)
                    return false;

                armaDeRajadaScript.reserveAmmo += AMMO_REFILL_AMOUNT;
                armaDeRajadaScript.reserveAmmo = Mathf.Min(armaDeRajadaScript.reserveAmmo, armaDeRajadaScript.maxReserveAmmo);
                UpdateAmmoUI(); // Adicionar atualização da UI
                return true;
            }
            else if (assaultRifleScript != null)
            {
                if (assaultRifleScript.reserveAmmo >= assaultRifleScript.maxReserveAmmo)
                    return false;

                assaultRifleScript.reserveAmmo += AMMO_REFILL_AMOUNT;
                assaultRifleScript.reserveAmmo = Mathf.Min(assaultRifleScript.reserveAmmo, assaultRifleScript.maxReserveAmmo);
                UpdateAmmoUI(); // Adicionar atualização da UI
                return true;
            }
            else if (plasmaGunScript != null)
            {
                if (plasmaGunScript.reserveAmmo >= plasmaGunScript.maxReserveAmmo)
                    return false;

                plasmaGunScript.reserveAmmo += AMMO_REFILL_AMOUNT;
                plasmaGunScript.reserveAmmo = Mathf.Min(plasmaGunScript.reserveAmmo, plasmaGunScript.maxReserveAmmo);
                UpdateAmmoUI(); // Adicionar atualização da UI
                return true;
            }
            // Adicione aqui outros 'else if' para outras classes de arma
        }
        return false; // Arma não encontrada ou não possui script de munição
    }

    // Atualiza o nome da arma e a munição na UI
    public void UpdateWeaponUI()
    {
        if (weaponNameText == null || ammoText == null)
        {
            Debug.LogError("As referências de texto da UI não foram definidas no Inspector!");
            return;
        }

        if (selectedWeapon < 0 || selectedWeapon >= transform.childCount)
        {
            weaponNameText.text = "";
            ammoText.text = "";
            return;
        }

        Transform currentWeapon = transform.GetChild(selectedWeapon);

        // Mantendo a lógica de checagem de GetComponent original:
        Pistol pistolScript = currentWeapon.GetComponent<Pistol>();
        Sniper sniperScript = currentWeapon.GetComponent<Sniper>();
        Shotgun shotgunScript = currentWeapon.GetComponent<Shotgun>();
        ArmaDeRajada armaDeRajadaScript = currentWeapon.GetComponent<ArmaDeRajada>();
        AssaultRifle assaultRifleScript = currentWeapon.GetComponent<AssaultRifle>();
        PlasmaGun plasmaGunScript = currentWeapon.GetComponent<PlasmaGun>();

        // Lógica de exibição:
        if (pistolScript != null)
        {
            weaponNameText.text = pistolScript.weaponName;
            ammoText.text = pistolScript.currentAmmo.ToString() + " / " + pistolScript.reserveAmmo.ToString();
        }
        else if (shotgunScript != null)
        {
            weaponNameText.text = shotgunScript.weaponName;
            ammoText.text = shotgunScript.currentAmmo.ToString() + " / " + shotgunScript.reserveAmmo.ToString();
        }

        else if (sniperScript != null)
        {
            weaponNameText.text = sniperScript.weaponName;
            ammoText.text = sniperScript.currentAmmo.ToString() + " / " + sniperScript.reserveAmmo.ToString();
        }

        else if (armaDeRajadaScript != null)
        {
            weaponNameText.text = armaDeRajadaScript.weaponName;
            ammoText.text = armaDeRajadaScript.currentAmmo.ToString() + " / " + armaDeRajadaScript.reserveAmmo.ToString();
        }
        else if (assaultRifleScript != null)
        {
            weaponNameText.text = assaultRifleScript.weaponName;
            ammoText.text = assaultRifleScript.currentAmmo.ToString() + " / " + assaultRifleScript.reserveAmmo.ToString();
        }
        else if (plasmaGunScript != null)
        {
            weaponNameText.text = plasmaGunScript.weaponName;
            ammoText.text = plasmaGunScript.currentAmmo.ToString() + " / " + plasmaGunScript.reserveAmmo.ToString();
        }
        else
        {
            weaponNameText.text = "Arma Desconhecida";
            ammoText.text = "0 / 0"; // Melhor exibir 0/0 do que vazio
        }
    }

    // Atualiza apenas a munição na UI (mantendo a lógica de checagem de script)
    public void UpdateAmmoUI()
    {
        if (ammoText == null)
        {
            Debug.LogError("A referência ao texto de munição da UI não foi definida no Inspector!");
            return;
        }

        if (selectedWeapon < 0 || selectedWeapon >= transform.childCount)
        {
            ammoText.text = "";
            return;
        }

        Transform currentWeapon = transform.GetChild(selectedWeapon);

        // Mantendo a lógica de checagem de GetComponent original:
        Pistol pistolScript = currentWeapon.GetComponent<Pistol>();
        Sniper sniperScript = currentWeapon.GetComponent<Sniper>();
        Shotgun shotgunScript = currentWeapon.GetComponent<Shotgun>();
        ArmaDeRajada armaDeRajadaScript = currentWeapon.GetComponent<ArmaDeRajada>();
        AssaultRifle assaultRifleScript = currentWeapon.GetComponent<AssaultRifle>();
        PlasmaGun plasmaGunScript = currentWeapon.GetComponent<PlasmaGun>();

        // Lógica de exibição:
        if (pistolScript != null)
        {
            ammoText.text = pistolScript.currentAmmo.ToString() + " / " + pistolScript.reserveAmmo.ToString();
        }
        else if (shotgunScript != null)
        {
            ammoText.text = shotgunScript.currentAmmo.ToString() + " / " + shotgunScript.reserveAmmo.ToString();
        }

        else if (sniperScript != null)
        {
            ammoText.text = sniperScript.currentAmmo.ToString() + " / " + sniperScript.reserveAmmo.ToString();
        }

        else if (armaDeRajadaScript != null)
        {
            ammoText.text = armaDeRajadaScript.currentAmmo.ToString() + " / " + armaDeRajadaScript.reserveAmmo.ToString();
        }
        else if (assaultRifleScript != null)
        {
            ammoText.text = assaultRifleScript.currentAmmo.ToString() + " / " + assaultRifleScript.reserveAmmo.ToString();
        }
        else if (plasmaGunScript != null)
        {
            ammoText.text = plasmaGunScript.currentAmmo.ToString() + " / " + plasmaGunScript.reserveAmmo.ToString();
        }
        else
        {
            ammoText.text = "0 / 0";
        }
    }
}