using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyButton : MonoBehaviour
{
    // Usa a classe serializada para configurar a compra
    public WeaponBuyData weaponData;

    [Header("Referências UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Image iconImage;
    public Button purchaseButton;
    [Header("Texto do Botão de Compra")]
    public TextMeshProUGUI buyButtonText; // Referência para o texto do botão

    [Header("Referências de Cena - PREENCHA MANUALMENTE")]
    // TORNADO PÚBLICO: Você DEVE arrastar os objetos da cena para estes campos no Inspector.
    public MachinePrinterGunsBuy machineRef;
    public WeaponSwitching weaponSwitchingRef;

    void Start()
    {
        // REMOVIDA a busca FindFirstObjectByType().
        // A checagem de nulidade será feita dentro de OnPurchaseClicked e UpdateBuyButtonUI.

        if (weaponData.weaponPrefab == null)
        {
            Debug.LogError($"O Prefab da arma não foi definido no BuyButton do {gameObject.name}!");
            // Não retorna para que a UI básica possa ser configurada
        }

        SetupButtonUI();

        // Configura o Listener do botão para chamar o método de compra
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }
    }

    void SetupButtonUI()
    {
        // Atualiza os elementos visuais do botão
        if (nameText != null)
        {
            nameText.text = weaponData.weaponName;
        }
        if (iconImage != null && weaponData.weaponIcon != null)
        {
            iconImage.sprite = weaponData.weaponIcon;
        }

        // O custo na UI mudará de acordo com o estado de posse da arma
        UpdateBuyButtonUI();
    }

    void OnPurchaseClicked()
    {
        // Checa se as referências de cena estão preenchidas antes de tentar a compra
        if (machineRef == null || weaponSwitchingRef == null)
        {
            Debug.LogError("Tentativa de compra falhou: Referências de cena (Machine ou WeaponSwitching) estão nulas no Inspector.");
            return;
        }

        // Chama o método de transação na Máquina.
        machineRef.TryBuyWeaponOrAmmo(weaponData, weaponSwitchingRef);

        // A UI precisa ser atualizada APÓS a compra
        UpdateBuyButtonUI();
    }

    /// <summary>
    /// Atualiza o texto ("BUY WEAPON" / "BUY AMMO") e o custo do botão.
    /// </summary>
    public void UpdateBuyButtonUI()
    {
        // Checagem de segurança para referências da UI e do Prefab (linha crítica)
        if (costText == null || buyButtonText == null || weaponData.weaponPrefab == null)
        {
            // O erro de NullReference é evitado aqui se a UI ou Prefab estiverem nulos.
            Debug.LogWarning($"UI ou Weapon Prefab não estão preenchidos no BuyButton: {gameObject.name}");
            return;
        }

        // NOVO: CHECAGEM CRÍTICA para a referência do WeaponSwitching.
        if (weaponSwitchingRef == null)
        {
            // Se esta referência estiver nula, não podemos checar a posse da arma.
            buyButtonText.text = "Error!";
            costText.text = "0";
            return;
        }

        // Verifica o estado de posse da arma
        bool playerHasWeapon = weaponSwitchingRef.HasWeapon(weaponData.weaponPrefab.name);

        if (playerHasWeapon)
        {
            // Se já tem a arma, o botão é para RECARREGAR (Munição)
            buyButtonText.text = "BUY AMMO";
            costText.text = weaponData.ammoCost.ToString();
            purchaseButton.interactable = true;
        }
        else
        {
            // Se não tem a arma, o botão é para COMPRAR (Weapon)
            buyButtonText.text = "BUY WEAPON";
            costText.text = weaponData.weaponCost.ToString();
            purchaseButton.interactable = true;
        }
    }

    // Garante que o estado do botão seja atualizado sempre que a tela for ativada
    void OnEnable()
    {
        // Chama a atualização da UI (a checagem de nulidade do weaponSwitchingRef acontece dentro do método)
        UpdateBuyButtonUI();
    }
}