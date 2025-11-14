using UnityEngine;
using TMPro;
using UnityEngine.Audio;

public class NaniteRessurection : MonoBehaviour
{
    public int perkCost = 500; // Custo do perk
    public TextMeshProUGUI promptText;
    public GameObject perkMachine;

    // Rastreia se a máquina foi comprada NESTA "vida" (permite recompra após revive)
    private bool machineUsedOnceThisLife = false;

    // Rastreia o estado permanente da máquina
    private bool isPermanentlyConsumed = false;

    private bool canBuy = false;
    // Variável local para exibir os usos restantes no prompt (sincronizada com PlayerHealth)
    private int currentTotalUses = 0;

    private PlayerHealth playerHealth;

    [Header("Audio")]
    public AudioSource machineAudioSource;
    public AudioClip buyedMachineClip;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth não encontrado. A máquina de perk não funcionará corretamente.");
            return;
        }

        // 🎯 Lógica de Sincronização e Estado Inicial (REVISADO)

        // 1. Obtém o número de usos totais persistentes
        currentTotalUses = playerHealth.quickReviveTotalUses;

        // 2. Define o estado inicial da máquina (se está consumida permanentemente)
        if (currentTotalUses <= 0)
        {
            // Se os usos persistentes forem 0, a máquina está permanentemente inativa
            isPermanentlyConsumed = true;

            // Opcional: Se esta é a PRIMEIRA vez (jogo novo) e currentTotalUses é 0, 
            // setamos a máquina para o estado inicial de compra (MAX_REVIVE_USES).
            // A compra inicial no TryBuyPerk definirá o valor no PlayerHealth.
            if (currentTotalUses == 0)
            {
                // Garante que o prompt mostra o valor máximo para a primeira compra
                currentTotalUses = PlayerHealth.MAX_REVIVE_USES;
                isPermanentlyConsumed = false; // Não está permanentemente inativa em um jogo novo.
            }
        }

        // Em um jogo novo ou após um reset completo, machineUsedOnceThisLife deve ser false.
        machineUsedOnceThisLife = false;

        // Se o jogo é carregado com usos restantes, a máquina ainda está disponível para RECOMPRA após um down.
        if (playerHealth.quickReviveTotalUses > 0)
        {
            // Garante que a máquina não é marcada como permanentemente consumida.
            isPermanentlyConsumed = false;
        }

        Debug.Log($"Nanite Start. Usos Totais: {playerHealth.quickReviveTotalUses}. Consumida Permanentemente: {isPermanentlyConsumed}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPermanentlyConsumed)
        {
            canBuy = true;
            UpdatePromptText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canBuy = false;
            promptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Se a máquina não foi comprada NESTA vida e não está permanentemente inativa
        if (canBuy && Input.GetKeyDown(KeyCode.E) && !machineUsedOnceThisLife && !isPermanentlyConsumed)
        {
            TryBuyPerk();
        }
        else if (canBuy)
        {
            // Atualiza o prompt para mostrar o estado (cargas ou inatividade)
            UpdatePromptText();
        }
    }

    private void UpdatePromptText()
    {
        if (promptText != null)
        {
            // Sincroniza o valor do PlayerHealth para exibir no prompt. 
            // Se for 0, mas não estiver permanentemente consumida (primeira compra), exibe MAX_REVIVE_USES.
            int displayUses = playerHealth.quickReviveTotalUses > 0 ?
                              playerHealth.quickReviveTotalUses :
                              (isPermanentlyConsumed ? 0 : PlayerHealth.MAX_REVIVE_USES);

            promptText.gameObject.SetActive(true);

            if (isPermanentlyConsumed)
            {
                promptText.text = "Ressureição Nanita foi totalmente consumida e está inativa.";
            }
            else if (machineUsedOnceThisLife) // Comprado NESTA vida
            {
                promptText.text = $"Ressureição Nanita adquirida. Usos restantes: {playerHealth.quickReviveTotalUses}.";
            }
            else // Pronto para comprar
            {
                promptText.text = $"Pressione 'E' para comprar Ressureição Nanita ({perkCost} Pontos) ({displayUses} Usos Restantes)";
            }
        }
    }

    private void TryBuyPerk()
    {
        if (PointManager.Instance == null || playerHealth == null) return;

        // Se já foi totalmente consumido, não pode comprar
        if (isPermanentlyConsumed)
        {
            promptText.text = "Inativo permanentemente!";
            return;
        }

        if (PointManager.Instance.currentPoints >= perkCost)
        {
            PointManager.Instance.SubtractPoints(perkCost);

            // Concede o perk ativo e ajusta as cargas totais no PlayerHealth
            playerHealth.GrantQuickRevivePerk(); // Isso define quickReviveTotalUses = MAX_REVIVE_USES se for 0.

            machineUsedOnceThisLife = true;

            if (machineAudioSource != null && buyedMachineClip != null)
            {
                machineAudioSource.PlayOneShot(buyedMachineClip);
            }

            UpdatePromptText();
            Debug.Log("Ressureição Nanita comprado com sucesso! Perk ativo.");
        }
        else
        {
            Debug.Log("Pontos insuficientes para comprar Ressureição Nanita.");
            if (promptText != null)
            {
                promptText.text = "Pontos insuficientes!";
            }
        }
    }

    // ⭐ MÉTODO: Chamado pelo PlayerHealth quando o jogador é revivido.
    public void ResetMachineState(int remainingUses)
    {
        currentTotalUses = remainingUses; // Sincroniza o número de usos totais

        // Verifica se a máquina deve ser permanentemente inativa
        if (currentTotalUses <= 0)
        {
            isPermanentlyConsumed = true;
            machineUsedOnceThisLife = true; // Impede recompra
            Debug.Log("Nanite Ressureição: Usos Esgotados! Inativo permanentemente.");
            // Opcional: Desativar visualmente o objeto da máquina aqui
            if (perkMachine != null)
            {
                perkMachine.SetActive(false); // Exemplo: Desativa o objeto da máquina
            }
        }
        else
        {
            machineUsedOnceThisLife = false; // Permite a recompra se ainda houver usos
            isPermanentlyConsumed = false; // Garante que a flag de permanente seja removida se houver usos
            Debug.Log("Nanite Ressureição: Máquina resetada devido ao revive. Recompra necessária.");
            if (perkMachine != null && !perkMachine.activeSelf)
            {
                perkMachine.SetActive(true); // Reativa o visual da máquina se foi desativado
            }
        }

        if (canBuy)
        {
            UpdatePromptText();
        }
    }
}