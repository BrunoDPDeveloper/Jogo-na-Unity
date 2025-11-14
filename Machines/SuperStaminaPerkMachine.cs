// Script: SuperStaminaPerkMachine.cs

using UnityEngine;
using TMPro;
using UnityEngine.Audio;

public class SuperStaminaPerkMachine : MonoBehaviour
{
    public int perkCost = 3000; // Custo
    public float newMaxStaminaValue = 500f; // O valor fixo desejado

    public TextMeshProUGUI promptText;

    private bool canBuy = false;
    private bool perkBought = false; // Controla se foi comprado NESTA vida

    [Header("Audio")]
    public AudioSource machineAudioSource;
    public AudioClip buyedMachineClip;

    // ==============================
    // FUNÇÕES DE INTERAÇÃO 
    // ==============================

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
        // Usa 'canBuy' para permitir a compra apenas quando o jogador está perto
        if (canBuy && Input.GetKeyDown(KeyCode.E) && !perkBought)
        {
            TryBuyPerk();
        }
    }

    // ==============================
    // FUNÇÕES DE LÓGICA
    // ==============================

    private void UpdatePromptText()
    {
        string perkName = "Super Stamina";
        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            if (perkBought)
            {
                promptText.text = $"Perk '{perkName}' já adquirido!";
            }
            else
            {
                promptText.text = $"Pressione 'E' para comprar {perkName} ({perkCost} Pontos) (Nova Stamina Máxima: {newMaxStaminaValue:F0})";
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
            PlayerController playerController = FindAnyObjectByType<PlayerController>();

            if (playerController != null)
            {
                PointManager.Instance.SubtractPoints(perkCost);

                // APLICAÇÃO DO PERK: Muda a stamina máxima
                playerController.SetMaxStamina(newMaxStaminaValue);

                perkBought = true;

                if (machineAudioSource != null && buyedMachineClip != null)
                {
                    machineAudioSource.PlayOneShot(buyedMachineClip);
                }

                // Adiciona o ícone do perk na UI
                if (PerkUIManager.Instance != null)
                {
                    PerkUIManager.Instance.AddPerkIcon("Super Stamina");
                }

                UpdatePromptText();
                Debug.Log($"Perk Super Stamina comprado com sucesso! Stamina Máxima definida para {newMaxStaminaValue}.");
            }
            else
            {
                Debug.LogError("Script PlayerController não encontrado no jogador.");
            }
        }
        else
        {
            Debug.Log("Pontos insuficientes para comprar Super Stamina.");
            if (promptText != null)
            {
                promptText.text = "Pontos insuficientes!";
            }
        }
    }

    // ⭐ NOVO MÉTODO: Chamado pelo PlayerHealth para resetar o estado da máquina
    public void ResetMachineState()
    {
        perkBought = false;
        Debug.Log("Máquina Super Stamina resetada. Recompra disponível.");
        if (canBuy)
        {
            UpdatePromptText();
        }
    }
}