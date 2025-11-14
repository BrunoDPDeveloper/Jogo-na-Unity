using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class Doorbuy : MonoBehaviour
{
    public int doorCost; // Custo do perk
    public TextMeshProUGUI promptText; // Texto de prompt para o jogador interagir

    private bool canBuy = false; // Flag para saber se o jogador está na área de compra
    private bool doorBought = false; // Flag para evitar que o jogador compre o perk mais de uma vez


    [Header("Audio")]
    public AudioSource doorAudioSource;
    public AudioClip buyedDoorClip;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que entrou no trigger é o jogador
        if (other.CompareTag("Player"))
        {
            canBuy = true;
            UpdatePromptText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Quando o jogador sai da área, remove o prompt
        if (other.CompareTag("Player"))
        {
            canBuy = false;
            promptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Se o jogador está na área de compra e apertar a tecla de interação (ex: 'E')
        if (canBuy && Input.GetKeyDown(KeyCode.E) && !doorBought)
        {
            TryBuyPerk();
        }
    }

    private void UpdatePromptText()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            if (doorBought)
            {
                promptText.text = "Porta já comprada!";
            }
            else
            {
                promptText.text = $"Pressione 'E' para comprar a porta ({doorCost} Pontos)";
            }
        }
    }

    private void TryBuyPerk()
    {
        // Garante que existe uma instância do PointManager
        if (PointManager.Instance == null)
        {
            Debug.LogError("PointManager não encontrado na cena.");
            return;
        }

        // Verifica se o jogador tem pontos suficientes
        if (PointManager.Instance.currentPoints >= doorCost)
        {
            // Encontra o script PlayerHealth no jogador
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                // Subtrai os pontos do jogador
                PointManager.Instance.SubtractPoints(doorCost);

                // Aplica o efeito do perk

                // Marca o perk como comprado
                doorBought = true;
                doorAudioSource.PlayOneShot(buyedDoorClip);
                Destroy(gameObject);


                // Atualiza o prompt para o jogador
                UpdatePromptText();
                Debug.Log("Porta comprado com sucesso!");
            }
            else
            {
                Debug.LogError("Script DoorBuy não encontrado no jogador.");
            }
        }
        else
        {
            Debug.Log("Pontos insuficientes para comprar a porta.");
            if (promptText != null)
            {
                promptText.text = "Pontos insuficientes!";
                // Opcional: Desative o prompt após um tempo
            }
        }
    }
}