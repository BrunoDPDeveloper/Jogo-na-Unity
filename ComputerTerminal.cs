using UnityEngine;
using TMPro;

public class ComputerTerminal : MonoBehaviour
{
    // --- UI e Interação (Você pode manter os mesmos nomes para facilitar) ---

    [Header("UI e Interação")]
    public GameObject interactionUI; // UI para mostrar "Aperte F"

    [Header("UI do Terminal")]
    public GameObject terminalScreenUI; // O painel/tela do terminal (seu menu Cyberpunk)

    // NOVO HEADER PARA O CONTROLE DO JOGADOR
    [Header("Player Control Scripts")]
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour playerLookScript;

    // VARIÁVEIS INTERNAS
    private bool isPlayerNearby = false;
    public static bool isTerminalOpen = false; // Flag ESTÁTICA para saber se o terminal está aberto

    void Start()
    {
        // Garante que ambas as UIs comecem desativadas
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        if (terminalScreenUI != null)
        {
            terminalScreenUI.SetActive(false);
        }

        // Aviso para garantir que os scripts de controle foram definidos
        if (playerMovementScript == null || playerLookScript == null)
        {
            Debug.LogWarning("Os scripts de controle do Player (Movement/Look) não foram definidos no Inspector!");
        }
    }

    void Update()
    {
        // 1. Lógica de Abertura (Se o Player estiver por perto e o terminal estiver fechado)
        if (isPlayerNearby && !isTerminalOpen)
        {
            // Verifica se a tecla 'F' foi pressionada
            if (Input.GetKeyDown(KeyCode.F))
            {
                OpenTerminal();
            }
        }
        // 2. Lógica de Fechamento (Se o terminal estiver aberto)
        else if (isTerminalOpen)
        {
            // Permite fechar a tela de compra com 'F' ou 'Escape'
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Escape))
            {
                CloseTerminal();
            }
        }
    }

    // Detecta quando o Player (com o Collider 'isTrigger') entra na área
    private void OnTriggerEnter(Collider other)
    {
        // Certifique-se de que a tag do Player é "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            // Exibe o UI de interação ("Aperte F")
            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }
        }
    }

    // Detecta quando o Player sai da área
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            // Esconde o UI de interação ("Aperte F")
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }

            // Garante que o terminal feche se o jogador se afastar
            if (isTerminalOpen)
            {
                CloseTerminal();
            }
        }
    }

    // --- Métodos de Controle do Terminal ---

    private void OpenTerminal()
    {
        if (terminalScreenUI != null)
        {
            terminalScreenUI.SetActive(true); // Exibe a tela do terminal
            isTerminalOpen = true;

            // ⭐ PAUSA E DESATIVA CONTROLES (CÓDIGO IDÊNTICO AO SEU) ⭐
            // BLOQUEIO DOS SCRIPTS DO PLAYER
            if (playerMovementScript != null)
                playerMovementScript.enabled = false;
            if (playerLookScript != null)
                playerLookScript.enabled = false;

            // Lógica de UI e Pausa: ZERA O TEMPO
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            // ⭐ FIM DA PAUSA ⭐

            // Esconde o UI de interação para não atrapalhar
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }

    public void CloseTerminal()
    {
        if (terminalScreenUI != null)
        {
            terminalScreenUI.SetActive(false); // Esconde a tela do terminal
            isTerminalOpen = false;

            // ⭐ RETOMADA E REATIVAÇÃO DE CONTROLES (CÓDIGO IDÊNTICO AO SEU) ⭐
            // Lógica de Retomada do Jogo
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // REATIVAÇÃO DOS SCRIPTS DO PLAYER
            if (playerMovementScript != null)
                playerMovementScript.enabled = true;
            if (playerLookScript != null)
                playerLookScript.enabled = true;
            // ⭐ FIM DA RETOMADA ⭐
        }

        // Se o player ainda estiver perto, reativa o UI de interação
        if (isPlayerNearby && interactionUI != null)
        {
            interactionUI.SetActive(true);
        }
    }
}