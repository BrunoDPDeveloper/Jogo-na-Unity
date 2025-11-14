using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    // A única instância do SettingsManager (Singleton)
    public static SettingsManager Instance;

    [Header("UI References")]
    // Referência ao seu Dropdown no Inspector
    public TMP_Dropdown resolutionDropdown;

    // Variável para guardar o índice da resolução selecionada
    private int selectedResolutionIndex = 0;

    // Lista de resoluções fixas (baseada na sua UI)
    private List<(int width, int height)> fixedResolutions = new List<(int width, int height)>
    {
        (1280, 720),
        (1366, 768),
        (1600, 900),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160)
    };

    void Awake()
    {
        // 1. Implementação do Singleton: Garante que só há uma instância e a torna persistente
        if (Instance == null)
        {
            Instance = this;
            // Impede que o objeto seja destruído ao carregar novas cenas
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            // Destrói duplicatas
            Destroy(this.gameObject);
            return;
        }

        // As funções de configuração devem ser chamadas no Awake ou Start
        PopulateResolutionDropdown();

        // Carrega a resolução salva (e a aplica)
        LoadResolution();
    }

    void Start()
    {
        // O evento onValueChanged deve ser conectado após o Dropdown ser configurado (no Populate)
        // E só se o Dropdown estiver presente na cena (geralmente só na cena de Menu)
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(SetResolutionIndex);
        }
    }

    // Função para popular o Dropdown com as opções
    void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (var res in fixedResolutions)
        {
            options.Add($"{res.width} x {res.height}");
        }

        resolutionDropdown.AddOptions(options);
    }

    // Função para carregar a resolução salva e configurar o Dropdown e o jogo.
    void LoadResolution()
    {
        // Tenta carregar o índice salvo. Se não houver, usa 0 (primeira resolução).
        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);

        // Garante que o índice esteja dentro dos limites da lista
        if (savedIndex >= 0 && savedIndex < fixedResolutions.Count)
        {
            selectedResolutionIndex = savedIndex;
        }
        else
        {
            selectedResolutionIndex = 0;
        }

        // Aplica a resolução salva IMEDIATAMENTE no início do jogo
        var resToApply = fixedResolutions[selectedResolutionIndex];
        // O valor 'true' é para Fullscreen. Ajuste se houver opção Windowed/Fullscreen.
        Screen.SetResolution(resToApply.width, resToApply.height, true);

        // Configura o Dropdown para mostrar a resolução correta, se ele estiver ativo
        if (resolutionDropdown != null)
        {
            resolutionDropdown.value = selectedResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }

        Debug.Log($"Resolução carregada e aplicada: {resToApply.width} x {resToApply.height}");
    }

    // Função chamada pelo evento 'On Value Changed' do Dropdown
    public void SetResolutionIndex(int index)
    {
        // Armazena o índice da resolução selecionada (não aplicada ainda)
        selectedResolutionIndex = index;
    }

    // Função para aplicar a Resolução (Chamada pelo botão 'APPLY')
    public void ApplyResolution()
    {
        // Usa o índice que foi armazenado na variável 'selectedResolutionIndex'
        var resToApply = fixedResolutions[selectedResolutionIndex];

        // Aplica a resolução (o 'true' mantém o modo Fullscreen)
        Screen.SetResolution(resToApply.width, resToApply.height, true);


        // Salva a preferência do usuário no disco (PlayerPrefs)
        PlayerPrefs.SetInt("ResolutionIndex", selectedResolutionIndex);
        PlayerPrefs.Save();

        Debug.Log($"Resolução aplicada e salva: {resToApply.width} x {resToApply.height}");
    }
}