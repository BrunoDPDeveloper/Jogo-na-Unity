using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Necessário para o OnSceneLoaded e TimeScale
using TMPro; // Incluído caso você use o GameSettings dentro deste GameManager
using UnityEngine.Rendering; // Incluído caso você use o GameSettings dentro deste GameManager
using UnityEngine.Rendering.Universal; // Incluído caso você use o GameSettings dentro deste GameManager
// Adicione outros 'using' necessários para o seu GameSettings ou lógica de jogo

public class GameManager : MonoBehaviour
{
    // O Singleton para acesso fácil
    public static GameManager Instance { get; private set; }

    [Header("CONFIGURAÇÕES DE PAUSE MENU")]
    // ⚠️ Arraste o Prefab do seu PauseMenu aqui no Inspector
    [SerializeField] private GameObject pauseMenuPrefab;

    // A referência para a instância do menu criada na cena de jogo
    private GameObject currentPauseMenuInstance;

    // Se você tiver a lógica de GameSettings aqui, mantenha os campos:
    // [Header("CONFIGURAÇÕES GRÁFICAS")]
    // private TMP_Dropdown qualityDropdown; 
    // etc...

    private void Awake()
    {
        // Lógica do Singleton (DontDestroyOnLoad)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Se você tiver a lógica de GameSettings no mesmo script, chame a inicialização dele aqui
        // LoadSettings(); 
    }

    private void OnEnable()
    {
        // Adiciona um listener para saber quando uma nova cena é carregada
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Remove o listener para evitar vazamentos de memória ou erros
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Limpa a instância antiga (se houver, o que não deveria ocorrer se o menu não for DontDestroyOnLoad)
        if (currentPauseMenuInstance != null)
        {
            Destroy(currentPauseMenuInstance);
            currentPauseMenuInstance = null;
        }

        // Verifica se a cena carregada é uma cena de JOGO onde a pausa é necessária
        if (scene.name == "Lab") // 💡 Altere "Lab" para o nome da sua cena de jogo real!
        {
            InstantiatePauseMenu();
        }

        // Se você tiver a lógica de GameSettings no mesmo script, chame a reaplicação de configurações aqui
        // ApplySettings(); 
    }

    private void InstantiatePauseMenu()
    {
        if (pauseMenuPrefab == null)
        {
            Debug.LogError("Pause Menu Prefab não atribuído no GameManager! O menu de pausa não pode ser criado.");
            return;
        }

        // Tenta encontrar o Canvas na cena atual
        Canvas canvas = FindFirstObjectByType<Canvas>();

        // Instancia o menu dentro do Canvas para que a UI funcione corretamente
        if (canvas != null)
        {
            currentPauseMenuInstance = Instantiate(pauseMenuPrefab, canvas.transform);
        }
        else
        {
            // Se não houver Canvas, instancia no Root da cena (pode dar problemas de UI)
            currentPauseMenuInstance = Instantiate(pauseMenuPrefab);
            Debug.LogWarning("Nenhum Canvas encontrado na cena de jogo. O Pause Menu foi instanciado no Root.");
        }

        // Garante que o menu comece DESATIVADO
        currentPauseMenuInstance.SetActive(false);

        Debug.Log("Pause Menu instanciado e pronto para uso na cena de jogo.");
    }

    // --- LÓGICA DE PAUSA ---

    void Update()
    {
        // Exemplo: Tecla ESC para pausar/despausar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // Coloque aqui outros Updates importantes para o jogo, como a lógica do seu GameSettings
    }

    public void TogglePause()
    {
        if (currentPauseMenuInstance == null)
        {
            Debug.LogWarning("O menu de pausa não está disponível na cena atual.");
            return;
        }

        bool isPaused = currentPauseMenuInstance.activeSelf;

        // 1. Alterna o estado de ativação do menu
        currentPauseMenuInstance.SetActive(!isPaused);

        // 2. Altera o tempo do jogo
        Time.timeScale = isPaused ? 1f : 0f;

        // 3. Libera/Trava o cursor
        Cursor.lockState = isPaused ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isPaused;

        Debug.Log(isPaused ? "Jogo despausado." : "Jogo pausado.");
    }

    // ⚠️ Se o GameSettings estiver em um script separado, remova toda a lógica dele daqui.
    // Se o GameSettings estiver no mesmo script, mantenha as funções Apply, Load, etc.

    // Exemplo (Se o GameSettings estiver junto):
    // public void ApplySettings() { ... }
    // private void LoadSettings() { ... }
    // public void SetQualityDropdown(TMP_Dropdown dropdown) { ... }

}