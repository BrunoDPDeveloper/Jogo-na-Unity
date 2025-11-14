using UnityEngine;
using TMPro;
using System.Collections.Generic;

// Este script deve ser anexado ao seu Player.
public class PlayerCollector : MonoBehaviour
{
    [Header("Configurações")]
    public string lootTag = "ComponenteLoot";

    [Header("UI de Contagem Individual")]
    [Tooltip("Arraste os TextMeshProUGUI na ordem correta: T1, T2, T3, T4, T5, T6.")]
    public TextMeshProUGUI[] componentCountTexts = new TextMeshProUGUI[6]; // Array para 6 textos

    // Dicionário para rastrear a contagem de componentes em tempo real (Runtime)
    private Dictionary<string, int> componentCounts =
        new Dictionary<string, int>();

    // Lista e mapeamento de todas as chaves de componentes (DEVE CORRESPONDER AO ARRAY DE TEXTOS)
    private readonly string[] allPossibleKeys = {
        "Componente_T1", "Componente_T2", "Componente_T3",
        "Componente_T4", "Componente_T5", "Componente_T6"
    };

    // Mapeia a chave de salvamento (string) para o seu respectivo TextMeshProUGUI
    private Dictionary<string, TextMeshProUGUI> keyToUITextMap =
        new Dictionary<string, TextMeshProUGUI>();


    void Awake()
    {
        // Garante que o número de chaves e o número de textos na UI sejam os mesmos.
        if (componentCountTexts.Length != allPossibleKeys.Length)
        {
            Debug.LogError("O número de Textos de UI não corresponde ao número de chaves de componentes no array allPossibleKeys! Verifique a configuração no Inspector.");
            // Preenche o mapeamento para evitar erros de índice, mas o erro de setup permanecerá.
            for (int i = 0; i < Mathf.Min(componentCountTexts.Length, allPossibleKeys.Length); i++)
            {
                keyToUITextMap.Add(allPossibleKeys[i], componentCountTexts[i]);
            }
        }
        else
        {
            for (int i = 0; i < allPossibleKeys.Length; i++)
            {
                keyToUITextMap.Add(allPossibleKeys[i], componentCountTexts[i]);
            }
        }
    }

    void Start()
    {
        // 1. Carrega todas as contagens salvas
        LoadAllComponentCounts();

        // 2. Atualiza a UI com os valores carregados
        UpdateAllComponentUIs();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(lootTag))
        {
            // Tenta obter o script de dados do componente
            TierComponentData componentData = other.GetComponent<TierComponentData>();

            if (componentData != null)
            {
                CollectLoot(other.gameObject, componentData.playerPrefsKey, componentData.componentName);
            }
            else
            {
                Debug.LogWarning("O item de loot '" + other.gameObject.name + "' não possui o script TierComponentData.");
            }
        }
    }

    void LoadAllComponentCounts()
    {
        // Carrega a contagem de todos os tipos de componentes
        foreach (string key in allPossibleKeys)
        {
            int count = PlayerPrefs.GetInt(key, 0);
            componentCounts[key] = count;
        }
    }


    void CollectLoot(GameObject collectedItem, string key, string name)
    {
        // 1. Atualiza a contagem em Runtime
        if (componentCounts.ContainsKey(key))
        {
            componentCounts[key]++;
        }
        else
        {
            componentCounts[key] = 1;
        }

        // 2. Salva a nova contagem no PlayerPrefs
        int newCount = componentCounts[key];
        PlayerPrefs.SetInt(key, newCount);
        PlayerPrefs.Save();

        // 3. Destrói o objeto
        Destroy(collectedItem);

        // 4. Atualiza APENAS o Texto da UI correspondente
        UpdateIndividualComponentUI(key, newCount);
    }

    /// <summary>
    /// Atualiza todos os 6 textos de componente ao iniciar o jogo.
    /// </summary>
    void UpdateAllComponentUIs()
    {
        foreach (var kvp in componentCounts)
        {
            UpdateIndividualComponentUI(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Atualiza o TextMeshProUGUI específico para uma dada chave/contagem.
    /// </summary>
    void UpdateIndividualComponentUI(string key, int count)
    {
        if (keyToUITextMap.ContainsKey(key))
        {
            TextMeshProUGUI textElement = keyToUITextMap[key];
            if (textElement != null)
            {
                // Exemplo de formato: "x 5"
                textElement.text = $"x {count}";
            }
        }
    }

    /// <summary>
    /// Método público para o Player acessar a contagem de um componente específico.
    /// </summary>
    public int GetComponentCount(string playerPrefsKey)
    {
        if (componentCounts.ContainsKey(playerPrefsKey))
        {
            return componentCounts[playerPrefsKey];
        }
        return 0;
    }

    // ⭐ NOVO: MÉTODO PARA CONSUMIR COMPONENTES NO UPGRADE
    public bool ConsumeComponent(string playerPrefsKey, int quantity)
    {
        if (componentCounts.ContainsKey(playerPrefsKey) && componentCounts[playerPrefsKey] >= quantity)
        {
            // 1. Atualiza em runtime
            componentCounts[playerPrefsKey] -= quantity;

            // 2. Salva no PlayerPrefs
            PlayerPrefs.SetInt(playerPrefsKey, componentCounts[playerPrefsKey]);
            PlayerPrefs.Save();

            // 3. Atualiza a UI individual
            UpdateIndividualComponentUI(playerPrefsKey, componentCounts[playerPrefsKey]);

            return true;
        }
        return false; // Falha (não tem o suficiente)
    }
}