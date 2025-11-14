using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script para o botão da lista de armas (VCRF, Shotgun, etc.).
/// Sua função é APENAS exibir as estatísticas da arma selecionada no painel lateral.
/// </summary>
public class WeaponSelectionButton : MonoBehaviour
{
    [Header("Dados da Arma")]
    // WeaponBuyData (a classe serializada) é usada para obter o Prefab da arma.
    public WeaponBuyData weaponData;

    [Header("Referências da UI")]
    // O componente Button principal do GameObject (ex: o botão VCRF)
    public Button selectionButton;

    [Header("Referência do Painel de Exibição")]
    // O script que realmente faz a atualização dos Sliders e Textos (WeaponStatsDisplay.cs).
    public WeaponStatsDisplay statsDisplayRef;

    // O GameObject do painel lateral (DANO/RANGE/MUNIÇÃO) - para ativá-lo ao clicar
    public GameObject statsPanelContainer;

    // Referência ao BuyButton final (REMOVIDA)

    void Start()
    {
        if (selectionButton != null)
        {
            selectionButton.onClick.RemoveAllListeners();
            selectionButton.onClick.AddListener(OnWeaponSelected);
        }

        // Verificação de segurança simplificada
        if (statsDisplayRef == null || statsPanelContainer == null)
        {
            // O debug está fora do bloco "if" no seu código anterior, vou corrigi-lo.
            Debug.LogError($"Referências críticas (Display ou Painel) não atribuídas no WeaponSelectionButton para {gameObject.name}. Verifique o Inspector.");
        }
    }

    /// <summary>
    /// Chamado quando o usuário clica no botão da lista para visualizar a arma.
    /// </summary>
    void OnWeaponSelected()
    {
        if (weaponData == null || weaponData.weaponPrefab == null)
        {
            Debug.LogWarning("WeaponData ou Prefab não definidos para este botão de seleção.");
            return;
        }

        // 1. Ativa o painel de estatísticas (se já não estiver ativo)
        if (statsPanelContainer != null)
        {
            statsPanelContainer.SetActive(true);
        }

        // 2. Atualiza a exibição visual (Sliders e Textos)
        if (statsDisplayRef != null)
        {
            // Puxa o script de stats do Prefab da arma e atualiza o display.
            statsDisplayRef.UpdateDisplay(weaponData.weaponPrefab);
        }

        // As etapas 3 (passar dados ao BuyButton) foram removidas.
    }
}
