using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameSettings : MonoBehaviour
{
    // Padrão Singleton para acesso fácil e garantia de instância única
    public static GameSettings Instance { get; private set; }

    // --- CHAVES DE PLAYERPREFS ---
    private const string QualityKey = "Game_QualityLevel";
    private const string SensitivityKey = "MouseSensitivityIndex";

    [Header("CONFIGURAÇÕES DE MOUSE")]
    // A lista de valores que você usará no Dropdown
    public readonly float[] sensitivityValues = { 50f, 100f, 150f, 200f, 250f, 300f, 350f, 400f, 450f, 500f };
    // NÃO precisa de [SerializeField] se for setado pelo código (como recomendado)
    private TMP_Dropdown sensitivityDropdown;

    [Header("CONFIGURAÇÕES GRÁFICAS")]
    // NÃO precisa de [SerializeField] se for setado pelo código
    private TMP_Dropdown qualityDropdown;


    // --- VARIÁVEIS TEMPORÁRIAS PARA O BOTÃO APPLY ---
    private int pendingQualityIndex;
    private int pendingSensitivityIndex;


    // --- REFERÊNCIAS INTERNAS ---
    private PlayerController playerController;


    private void Awake()
    {
        // 1. Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        // Tenta encontrar o PlayerController
        playerController = FindFirstObjectByType<PlayerController>();

        if (playerController == null)
        {
            Debug.LogWarning("PlayerController não encontrado no Awake! As configurações serão carregadas quando ele aparecer.");
        }

        // 2. CARREGAR CONFIGURAÇÕES SALVAS
        LoadSettings();

        // Adiciona um listener para recarregar as configurações se a cena mudar
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Remove o listener para evitar erros quando o objeto for destruído
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Tenta encontrar o PlayerController (se ainda não encontrado)
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();

            if (playerController != null)
            {
                ApplySensitivity(pendingSensitivityIndex);
            }
        }

        // 2. APLICA A QUALIDADE SALVA
        // Garante que o URP Asset da qualidade correta seja carregado na nova cena.
        ApplyQuality(pendingQualityIndex);
    }


    private void Start()
    {
        // NOTE: A configuração do Dropdown (SetupQualityDropdown) agora é chamada
        // dentro de SetQualityDropdown, que é executado pelo SettingsPanelInitializer
        // quando a UI é carregada.
    }

    // --- FUNÇÃO CENTRAL PARA O BOTÃO 'APPLY' ---
    public void ApplySettings()
    {
        // 1. Aplica e Salva a Qualidade Gráfica
        ApplyQuality(pendingQualityIndex);
        PlayerPrefs.SetInt(QualityKey, pendingQualityIndex);

        // 2. Aplica e Salva a Sensibilidade do Mouse
        ApplySensitivity(pendingSensitivityIndex);
        PlayerPrefs.SetInt(SensitivityKey, pendingSensitivityIndex);

        PlayerPrefs.Save();

        Debug.Log("Configurações aplicadas e salvas com sucesso!");
    }


    // --- LÓGICA DE CARREGAMENTO INICIAL (CORRIGIDA) ---
    private void LoadSettings()
    {
        // Puxa o último salvo ou o padrão
        pendingQualityIndex = PlayerPrefs.GetInt(QualityKey, 0);
        pendingSensitivityIndex = PlayerPrefs.GetInt(SensitivityKey, 1);

        // Aplica as configurações salvas no jogo (importante para o Awake)
        ApplyQuality(pendingQualityIndex);

        // CORREÇÃO URP: Chamada DUPLA para forçar o URP a inicializar corretamente no Awake.
        ApplyQuality(pendingQualityIndex);

        ApplySensitivity(pendingSensitivityIndex);
    }


    // --- MÉTODOS PÚBLICOS PARA SETAR REFERÊNCIAS DA UI ---

    public void SetQualityDropdown(TMP_Dropdown dropdown)
    {
        // Se a referência nova é diferente da atual (ou a atual é nula), atualiza
        if (dropdown != null && qualityDropdown != dropdown)
        {
            qualityDropdown = dropdown;
            SetupQualityDropdown(); // Re-configura o Dropdown
            qualityDropdown.value = pendingQualityIndex; // Garante que o valor salvo seja exibido
        }
    }

    public void SetSensitivityDropdown(TMP_Dropdown dropdown)
    {
        // Se a referência nova é diferente da atual (ou a atual é nula), atualiza
        if (dropdown != null && sensitivityDropdown != dropdown)
        {
            sensitivityDropdown = dropdown;

            // Re-adiciona o Listener (remova se estiver em Start())
            sensitivityDropdown.onValueChanged.RemoveAllListeners(); // Limpa listeners antigos
            sensitivityDropdown.onValueChanged.AddListener(UpdatePendingSensitivity);

            sensitivityDropdown.value = pendingSensitivityIndex; // Garante que o valor salvo seja exibido
        }
    }


    // --- LÓGICA DE QUALIDADE GRÁFICA ---

    private void SetupQualityDropdown()
    {
        if (qualityDropdown != null)
        {
            string[] names = QualitySettings.names;
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(names));

            // Garante que o Listener não seja duplicado, depois adiciona
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.onValueChanged.AddListener(UpdatePendingQuality);
        }
    }

    public void UpdatePendingQuality(int qualityIndex)
    {
        pendingQualityIndex = qualityIndex;
        Debug.Log($"Qualidade PENDENTE: {QualitySettings.names[qualityIndex]}");
    }

    private void ApplyQuality(int qualityIndex)
    {
        // 1. Aplica o nível de qualidade global
        QualitySettings.SetQualityLevel(qualityIndex, true);

        // 2. Força a atualização do renderizador URP (Usando a propriedade CORRETA)
        RenderPipelineAsset activePipeline = QualitySettings.renderPipeline;

        // É necessário fazer o 'cast' para UniversalRenderPipelineAsset.
        if (activePipeline is UniversalRenderPipelineAsset urpAsset)
        {
            // Esta é a maneira correta de redefinir o pipeline.
            GraphicsSettings.defaultRenderPipeline = urpAsset;
        }
    }


    // --- LÓGICA DE SENSIBILIDADE ---

    public void UpdatePendingSensitivity(int index)
    {
        if (index >= 0 && index < sensitivityValues.Length)
        {
            pendingSensitivityIndex = index;
            Debug.Log($"Sensibilidade PENDENTE: {sensitivityValues[index]}");
        }
    }

    private void ApplySensitivity(int index)
    {
        if (playerController != null && index >= 0 && index < sensitivityValues.Length)
        {
            float newSensitivity = sensitivityValues[index];
            // OBSERVAÇÃO: Esta linha exige que o PlayerController tenha um método público chamado SetMouseSensitivity(float)
            playerController.SetMouseSensitivity(newSensitivity);
            Debug.Log($"Sensibilidade aplicada: {newSensitivity}");
        }
    }

    public float GetSavedSensitivityValue()
    {
        int savedIndex = PlayerPrefs.GetInt(SensitivityKey, 1);
        if (savedIndex >= 0 && savedIndex < sensitivityValues.Length)
        {
            return sensitivityValues[savedIndex];
        }
        return sensitivityValues[1];
    }
}