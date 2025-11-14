using UnityEngine;
using TMPro;
using System.Collections; // Necessário para Coroutines

public class PointManager : MonoBehaviour
{
    // A instância estática permite que outros scripts acessem este manager facilmente.
    public static PointManager Instance;

    public int currentPoints; // A pontuação atual do jogador

    // ⭐ Variáveis para a moeda e o texto flutuante
    [Header("Configurações da Moeda")]
    public string currencySymbol = " C "; // Ex: " C " para CipherCore

    [Header("UI Principal")]
    public TextMeshProUGUI pointsText; // Referência para o texto da UI que exibirá os pontos

    [Header("UI Temporária (Feedback)")]
    public TextMeshProUGUI floatingPointsText; // O texto base que servirá de modelo (prefab)
    public float fadeDuration = 0.8f;      // Tempo total para o texto desaparecer
    public float fadeDistance = 20f;       // Distância em pixels que o texto subirá

    // Variáveis internas para animação
    private Color floatingStartColor;
    private Vector2 floatingStartPos;

    // O padrão Singleton é útil para managers que só devem existir uma vez na cena.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Se quiser que o PointManager persista entre cenas:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Chamado no início do jogo.
    void Start()
    {
        currentPoints = 500; // Inicia a pontuação em 500
        UpdatePointsUI(); // Atualiza o texto inicial da UI

        // Inicialização da UI flutuante
        if (floatingPointsText != null)
        {
            floatingPointsText.text = ""; // Oculta o texto base
            floatingStartColor = floatingPointsText.color;
            floatingStartPos = floatingPointsText.rectTransform.anchoredPosition;
        }
    }

    // Adiciona pontos à pontuação atual.
    public void AddPoints(int amount)
    {
        currentPoints += amount;

        // Mostra texto flutuante de ganho
        ShowGainedPointsUI(amount);

        Debug.Log($"Pontos adicionados: {amount}. Pontuação total: {currencySymbol}{currentPoints}");
        UpdatePointsUI();
    }

    // Remove pontos (ex: compras)
    public void SubtractPoints(int amount)
    {
        currentPoints = Mathf.Max(0, currentPoints - amount);

        Debug.Log($"Pontos subtraídos: {amount}. Pontuação total: {currencySymbol}{currentPoints}");
        UpdatePointsUI();
    }

    // Atualiza a UI principal com a pontuação atual
    private void UpdatePointsUI()
    {
        if (pointsText != null)
        {
            pointsText.text = currencySymbol + currentPoints.ToString();
        }
    }

    // Cria e anima um novo texto flutuante
    private void ShowGainedPointsUI(int amount)
    {
        if (floatingPointsText != null)
        {
            // Cria uma cópia temporária do texto
            TextMeshProUGUI tempText = Instantiate(floatingPointsText, floatingPointsText.transform.parent);

            tempText.gameObject.SetActive(true);
            tempText.text = $"+{amount}{currencySymbol}";

            RectTransform rect = tempText.rectTransform;
            rect.anchoredPosition = floatingStartPos;
            tempText.color = floatingStartColor;

            // Inicia a animação independente
            StartCoroutine(AnimateFloatingText(tempText));
        }
    }

    // Coroutine para animar o texto flutuante
    IEnumerator AnimateFloatingText(TextMeshProUGUI textObj)
    {
        float elapsedTime = 0f;
        RectTransform rect = textObj.rectTransform;
        Color startColor = textObj.color;
        Vector2 startPos = rect.anchoredPosition;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;

            // Movimento: sobe
            float newY = Mathf.Lerp(startPos.y, startPos.y + fadeDistance, progress);
            rect.anchoredPosition = new Vector2(startPos.x, newY);

            // Fade-out
            float newAlpha = Mathf.Lerp(startColor.a, 0f, progress);
            textObj.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            yield return null;
        }

        // Destroi a cópia ao fim da animação
        Destroy(textObj.gameObject);
    }
}
