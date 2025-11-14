using UnityEngine;
using TMPro;
using UnityEngine.Audio;

public class DamageReductionMachine : MonoBehaviour
{
    public int perkCost = 2500; // Defina o custo do seu perk
    public TextMeshProUGUI promptText;
    //public GameObject perkMachine; // Referência visual (opcional)

    private bool canBuy = false;
    // ⭐ ATUALIZADO: perkBought é resetado no revive, permitindo a recompra.
    private bool perkBought = false;

    [Header("Audio")]
    public AudioSource machineAudioSource;
    public AudioClip buyedMachineClip;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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
        if (canBuy && Input.GetKeyDown(KeyCode.E) && !perkBought)
        {
            TryBuyPerk();
        }
    }

    private void UpdatePromptText()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            if (perkBought)
            {
                promptText.text = "Perk 'Aço Sólido' já adquirido!";
            }
            else
            {
                promptText.text = $"Pressione 'E' para comprar Aço Sólido ({perkCost} Pontos)";
            }
        }
    }

    private void TryBuyPerk()
    {
        if (PointManager.Instance == null)
        {
            Debug.LogError("PointManager não encontrado na cena.");
            return;
        }

        if (PointManager.Instance.currentPoints >= perkCost)
        {
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                PointManager.Instance.SubtractPoints(perkCost);

                // Aplica o efeito do perk no PlayerHealth
                playerHealth.hasDamageReductionPerk = true;

                perkBought = true; // Impede que a MÁQUINA seja comprada novamente NESTA vida

                if (machineAudioSource != null && buyedMachineClip != null)
                {
                    machineAudioSource.PlayOneShot(buyedMachineClip);
                }

                if (PerkUIManager.Instance != null)
                {
                    PerkUIManager.Instance.AddPerkIcon("Aço Sólido");
                }

                UpdatePromptText();
                Debug.Log("Perk Aço Sólido comprado com sucesso! Dano reduzido.");
            }
            else
            {
                Debug.LogError("Script PlayerHealth não encontrado no jogador.");
            }
        }
        else
        {
            Debug.Log("Pontos insuficientes para comprar Aço Sólido.");
            if (promptText != null)
            {
                promptText.text = "Pontos insuficientes!";
            }
        }
    }

    // ⭐ NOVO MÉTODO: Chamado pelo PlayerHealth.Revive() para permitir recompra.
    public void ResetMachineState()
    {
        perkBought = false; // Define como não comprado, permitindo a interação de compra
        Debug.Log("Máquina Aço Sólido resetada. Recompra disponível após revive.");

        // Se o jogador estiver na área de trigger, atualize o prompt imediatamente
        if (canBuy)
        {
            UpdatePromptText();
        }
    }
}