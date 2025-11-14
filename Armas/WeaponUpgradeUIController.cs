using UnityEngine;
using TMPro;

public class WeaponUpgradeUIController : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerCollector playerCollector;
    // Removida a referência ao defaultWeaponButton

    private AssaultRifle currentWeapon; // Será definido dinamicamente

    [Header("UI References")]
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI costText;
    public GameObject upgradeButton;
    public GameObject upgradePanelContainer;

    [Header("Damage Comparison UI")]
    public TextMeshProUGUI currentDamageText;
    public TextMeshProUGUI currentHeadshotText;
    public TextMeshProUGUI nextDamageText;
    public TextMeshProUGUI nextHeadshotText;

    void Awake()
    {
        if (playerCollector == null)
        {
            // Tenta encontrar o PlayerCollector
            playerCollector = FindFirstObjectByType<PlayerCollector>();
        }
    }

    // O método InitializePanel foi removido. A lógica de inicialização agora é tratada 
    // pelo OpenBuyScreen e pela primeira chamada de UpdateUpgradePanel.

    public void HideUpgradePanel()
    {
        currentWeapon = null;
        if (upgradePanelContainer != null)
        {
            upgradePanelContainer.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// MÉTODO PRINCIPAL: Atualiza o painel de upgrade com os dados da arma específica.
    /// Chamado pelo WeaponButtonReference (VCARF, IDC-43, etc.)
    /// </summary>
    /// <param name="weapon">A instância da arma ativa na cena.</param>
    public void UpdateUpgradePanel(AssaultRifle weapon)
    {
        currentWeapon = weapon;

        if (currentWeapon == null)
        {
            HideUpgradePanel();
            return;
        }

        // Se a UI estava escondida, mostre o container
        if (upgradePanelContainer != null)
        {
            upgradePanelContainer.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        // Obtém informações de upgrade
        var upgradeInfo = currentWeapon.GetNextUpgradeInfo();
        var simulatedStats = currentWeapon.GetSimulatedNextTierStats();

        // 1. NOME DA ARMA
        weaponNameText.text = currentWeapon.weaponName;

        // 2. EXIBIÇÃO DO CUSTO E BOTÃO
        if (upgradeInfo.cost > 0)
        {
            string componentName = upgradeInfo.componentKey.Replace("Componente_", "");
            string costDisplay = $"CUSTO: {upgradeInfo.cost}x ({componentName})";
            costText.text = costDisplay;

            // Assumimos que PlayerCollector tem GetComponentCount
            bool canAfford = playerCollector.GetComponentCount(upgradeInfo.componentKey) >= upgradeInfo.cost;
            costText.color = canAfford ? Color.white : Color.red;

            upgradeButton.SetActive(true);

            // 3. EXIBIÇÃO DA COMPARAÇÃO (UPGRADE DISPONÍVEL)
            currentDamageText.text = $"WD: {currentWeapon.damage:F0}";
            currentHeadshotText.text = $"HD: {currentWeapon.headshotDamage:F0}";

            nextDamageText.text = $"WD: {simulatedStats.newDamage:F0}";
            nextHeadshotText.text = $"HD: {simulatedStats.newHeadshotDamage:F0}";
        }
        else
        {
            // Tier Máximo
            costText.text = "Tier Máximo Atingido";
            costText.color = Color.yellow;
            upgradeButton.SetActive(false);

            // 3. EXIBIÇÃO DA COMPARAÇÃO (TIER MÁXIMO)
            currentDamageText.text = $"WD: {currentWeapon.damage:F0}";
            currentHeadshotText.text = $"HD: {currentWeapon.headshotDamage:F0}";

            nextDamageText.text = "MAX";
            nextHeadshotText.text = "MAX";
        }
    }

    /// <summary>
    /// MÉTODO CHAMADO QUANDO O JOGADOR CLICA NO BOTÃO DE UPGRADE
    /// </summary>
    public void OnUpgradeButtonClicked()
    {
        if (currentWeapon == null || playerCollector == null)
        {
            Debug.LogError("Erro: Arma ou PlayerCollector não definidos. O botão de upgrade não deve estar ativo.");
            return;
        }

        if (currentWeapon.TryUpgradeTier(playerCollector))
        {
            // Upgrade bem-sucedido: reatualiza a UI
            UpdateUpgradePanel(currentWeapon);
        }
        else
        {
            Debug.Log("Upgrade falhou! Componentes insuficientes.");
        }
    }
}