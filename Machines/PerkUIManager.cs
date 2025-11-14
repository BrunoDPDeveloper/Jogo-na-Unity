using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PerkUIManager : MonoBehaviour
{
    public static PerkUIManager Instance;

    [Header("Configurações da UI")]
    public Transform perkIconsContainer;
    public GameObject perkIconPrefab;

    [Header("Ícones de Perks")]
    public Sprite juggernautIcon;
    public Sprite selfReviveIcon; // Adicione esta linha para o novo sprite
    public Sprite SuperStaminaIcon;

    private Dictionary<string, GameObject> activePerks = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Opcional: Não destruir na troca de cena se for um Singleton de jogo.
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPerkIcon(string perkName)
    {
        if (activePerks.ContainsKey(perkName))
        {
            Debug.Log($"Perk '{perkName}' já está ativo.");
            return;
        }

        Sprite iconSprite = GetPerkIconSprite(perkName);
        if (iconSprite == null)
        {
            Debug.LogError($"Sprite para o perk '{perkName}' não encontrado.");
            return;
        }

        GameObject newIcon = Instantiate(perkIconPrefab, perkIconsContainer);
        Image iconImage = newIcon.GetComponent<Image>();
        iconImage.sprite = iconSprite;

        // Adiciona o nome e a referência do ícone ao dicionário
        activePerks.Add(perkName, newIcon);
        Debug.Log($"Perk '{perkName}' adicionado à UI.");
    }

    // --- NOVO MÉTODO PARA VERIFICAR SE O PERK ESTÁ ATIVO (Resolvendo o erro CS1061) ---
    public bool HasPerk(string perkName)
    {
        // Retorna true se o nome do perk existe no dicionário de perks ativos
        return activePerks.ContainsKey(perkName);
    }
    // ----------------------------------------------------------------------------------

    // --- MÉTODO PARA REMOVER UM PERK ---
    public void RemovePerkIcon(string perkName)
    {
        if (activePerks.ContainsKey(perkName))
        {
            GameObject iconToRemove = activePerks[perkName];
            Destroy(iconToRemove);
            activePerks.Remove(perkName);
            Debug.Log($"Perk '{perkName}' removido da UI.");
        }
    }
    // ----------------------------------------

    private Sprite GetPerkIconSprite(string perkName)
    {
        switch (perkName)
        {
            case "Aço Sólido":
                return juggernautIcon;
            case "Ressureição Nanita":
                return selfReviveIcon;
            case "Super Stamina":
                return SuperStaminaIcon;
            default:
                return null;
        }
    }
}