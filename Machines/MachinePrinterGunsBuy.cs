using UnityEngine;
using TMPro;

public class MachinePrinterGunsBuy : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource wallBuyAudioSource;
    public AudioClip buyedWallBuyClip;

    [Header("UI e Interação")]
    public GameObject interactionUI; // UI para mostrar "Aperte F"

    [Header("UI de Compra")]
    public GameObject buyScreenUI; // O painel/tela de compra
    // A referência ao UI Controller é mantida porque o BuyButton precisa dela indiretamente.
    public WeaponUpgradeUIController upgradeUIController;

    [Header("Referências")]
    public WeaponSwitching weaponSwitching;

    // NOVO HEADER PARA O CONTROLE DO JOGADOR
    [Header("Player Control Scripts")]
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour playerLookScript;

    // VARIÁVEIS INTERNAS
    private bool isPlayerNearby = false;
    private bool hasWeapon = false;
    public static bool isBuyScreenOpen = false;
    private GameObject playerRef;

    void Start()
    {
        // Garante que ambas as UIs comecem desativadas
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        if (buyScreenUI != null)
        {
            buyScreenUI.SetActive(false);
        }

        if (weaponSwitching == null)
        {
            Debug.LogError("A referência ao WeaponSwitching não foi definida no Inspector!");
        }

        if (playerMovementScript == null || playerLookScript == null)
        {
            Debug.LogWarning("Os scripts de controle do Player não foram definidos no Inspector. A interação da UI pode falhar.");
        }

        if (upgradeUIController == null)
        {
            Debug.LogWarning("O Upgrade UI Controller não foi definido no Inspector! Os botões de upgrade não funcionarão.");
        }
    }

    void Update()
    {
        // Só verifica a interação se o player estiver por perto E a tela de compra não estiver aberta
        if (isPlayerNearby && !isBuyScreenOpen)
        {
            // Verifica se a tecla 'F' foi pressionada
            if (Input.GetKeyDown(KeyCode.F))
            {
                OpenBuyScreen();
            }
        }
        else if (isBuyScreenOpen)
        {
            // Opcional: Fechar a tela de compra ao apertar 'F' novamente
            if (Input.GetKeyDown(KeyCode.F))
            {
                CloseBuyScreen();
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
            playerRef = other.gameObject; // Guarda a referência do player

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
            playerRef = null; // Limpa a referência

            // Esconde o UI de interação ("Aperte F")
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }

            // Garante que a tela de compra feche se o jogador se afastar
            if (isBuyScreenOpen)
            {
                CloseBuyScreen();
            }
        }
    }

    // --- Métodos de UI de Compra ---

    private void OpenBuyScreen()
    {
        if (buyScreenUI != null)
        {
            buyScreenUI.SetActive(true); // Exibe a tela de compra
            isBuyScreenOpen = true;

            // BLOQUEIO DOS SCRIPTS DO PLAYER PARA PERMITIR CLIQUES NA UI
            if (playerMovementScript != null)
                playerMovementScript.enabled = false;
            if (playerLookScript != null)
                playerLookScript.enabled = false;

            // ❌ REMOVIDO: A inicialização da UI de Upgrade (que forçava a VCARF)
            // Agora a responsabilidade é do botão da arma clicado.

            // NOVO: Atualiza todos os botões de compra para checar o estado da arma/munição
            UpdateAllBuyButtonsUI();

            // Lógica de UI e Pausa
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // Esconde o UI de interação para não atrapalhar
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    public void CloseBuyScreen()
    {
        if (buyScreenUI != null)
        {
            buyScreenUI.SetActive(false); // Esconde a tela de compra
            isBuyScreenOpen = false;

            // Lógica de Retomada do Jogo
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // REATIVAÇÃO DOS SCRIPTS DO PLAYER
            if (playerMovementScript != null)
                playerMovementScript.enabled = true;
            if (playerLookScript != null)
                playerLookScript.enabled = true;
        }

        // Se o player ainda estiver perto, reativa o UI de interação
        if (isPlayerNearby && interactionUI != null)
        {
            interactionUI.SetActive(true);
        }
    }

    /// <summary>
    /// Procura e atualiza o estado de todos os botões de compra dentro do painel de compra.
    /// Chamado ao abrir a tela.
    /// </summary>
    private void UpdateAllBuyButtonsUI()
    {
        if (buyScreenUI == null) return;

        // Procura todos os BuyButton na tela de compra
        BuyButton[] buyButtons = buyScreenUI.GetComponentsInChildren<BuyButton>(true);

        foreach (BuyButton button in buyButtons)
        {
            button.UpdateBuyButtonUI();
        }
    }

    // Método de transação para ser chamado pelo botão da UI (BuyButton.cs)
    public void TryBuyWeaponOrAmmo(WeaponBuyData data, WeaponSwitching ws)
    {
        if (PointManager.Instance == null)
        {
            Debug.LogError("PointManager não encontrado!");
            return;
        }

        // 1. Verificar se o jogador JÁ TEM a arma
        bool playerHasWeapon = ws.HasWeapon(data.weaponPrefab.name);

        if (!playerHasWeapon)
        {
            // --- TENTAR COMPRAR A ARMA ---
            if (PointManager.Instance.currentPoints >= data.weaponCost)
            {
                // Verifica limite e substituição (a lógica está em WeaponSwitching)
                PointManager.Instance.SubtractPoints(data.weaponCost);
                ws.AddNewWeapon(data.weaponPrefab);

                if (wallBuyAudioSource != null && buyedWallBuyClip != null)
                {
                    wallBuyAudioSource.PlayOneShot(buyedWallBuyClip);
                }
                CloseBuyScreen(); // Fecha o menu após a compra da arma
                return;
            }
            else
            {
                Debug.Log("Pontos insuficientes para comprar a arma: " + data.weaponName);
                // Adicionar feedback visual aqui
                return;
            }
        }
        else
        {
            // --- TENTAR COMPRAR MUNIÇÃO ---
            if (PointManager.Instance.currentPoints >= data.ammoCost)
            {
                // Tenta adicionar a munição. A função AddAmmoToWeapon retorna false se a munição já estiver no máximo.
                if (ws.AddAmmoToWeapon(data.weaponPrefab.name))
                {
                    PointManager.Instance.SubtractPoints(data.ammoCost);

                    if (wallBuyAudioSource != null && buyedWallBuyClip != null)
                    {
                        wallBuyAudioSource.PlayOneShot(buyedWallBuyClip);
                    }
                    // O UpdateAmmoUI é chamado dentro de AddAmmoToWeapon
                    return;
                }
                else
                {
                    Debug.Log("Munição já está no máximo para: " + data.weaponName);
                    // Adicionar feedback visual aqui
                }
            }
            else
            {
                Debug.Log("Pontos insuficientes para recarga: " + data.weaponName);
                // Adicionar feedback visual aqui
                return;
            }
        }
    }
}