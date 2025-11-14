using UnityEngine;
using TMPro; // Para usar TextMeshPro
using UnityEngine.SceneManagement; // Para reiniciar ou voltar ao menu
using UnityEngine.UI; // Adicione esta linha para usar a classe Image

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI do Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI roundsText;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI pointsText;

    // Campo para exibir o sprite do nível
    public Image prestigeIconDisplay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void ShowGameOverScreen(int finalRound, int finalKills, int finalLevel, int finalPoints)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;

            // Exibe as informações na tela
            if (roundsText != null)
            {
                roundsText.text = "You survived " + finalRound.ToString() + " rounds";
            }
            if (killsText != null)
            {
                killsText.text = finalKills.ToString();
            }
            if (levelText != null)
            {
                levelText.text = finalLevel.ToString();
            }
            if (pointsText != null)
            {
                pointsText.text = finalPoints.ToString();
            }

            // Exibe o sprite do prestígio
            if (prestigeIconDisplay != null && PlayerXPManager.Instance != null)
            {
                prestigeIconDisplay.sprite = PlayerXPManager.Instance.GetCurrentPrestigeSprite();
                prestigeIconDisplay.enabled = (prestigeIconDisplay.sprite != null);
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        PlayerHealth.isDead = false;

        // A cena será recarregada, e o RoundManager vai reiniciar
        // naturalmente com o método Start()
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}