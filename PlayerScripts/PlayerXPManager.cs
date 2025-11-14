using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerXPManager : MonoBehaviour
{
    // A ÚNICA REFERÊNCIA ESTÁTICA para que o DataManager possa encontrar o UI Manager.
    public static PlayerXPManager Instance;

    [Header("UI")]
    [SerializeField] public Image prestigeIconDisplay;
    [SerializeField] private TextMeshProUGUI levelTextDisplay;
    [SerializeField] private Image xpBarFill;
    [SerializeField] private TextMeshProUGUI xpValueText;

    [Header("Level Up UI")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private Image levelUpSpriteDisplay;
    [SerializeField] private TextMeshProUGUI levelUpTextDisplay;
    [SerializeField] private AudioSource levelUpSoundSource;
    [SerializeField] private AudioClip levelUpClip;
    [SerializeField] private float displayDuration = 3f;

    [Header("Prestige Icons")]
    public Sprite[] prestigeSprites; // MANTENHA AQUI, pois os sprites são específicos da cena/prefab da UI

    // Referência ao módulo persistente
    private XPDataManager dataManager;

    private void Awake()
    {
        // Singleton DE CENA: Garante que apenas uma UI está ativa na cena Lab
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Tenta encontrar a instância persistente
        dataManager = XPDataManager.Instance;

        if (dataManager == null)
        {
            Debug.LogError("[UI] XPDataManager não encontrado! A UI não será atualizada.");
            return;
        }

        // Puxa os dados iniciais e atualiza a UI
        UpdateUI();
    }

    public Sprite GetCurrentPrestigeSprite()
    {
        if (dataManager == null || prestigeSprites == null || prestigeSprites.Length == 0)
        {
            return null;
        }

        // Puxa o índice do DataManager
        int spriteIndex = Mathf.Min(dataManager.EffectiveLevel - 1, prestigeSprites.Length - 1);
        return prestigeSprites[spriteIndex];
    }

    // Este método é o que outros scripts (ex: inimigos) devem chamar
    public void AddXP(float baseAmount)
    {
        if (dataManager != null)
        {
            // Delega a ação para o módulo persistente
            dataManager.AddXP(baseAmount);
        }
    }

    // Método principal para atualizar a visualização da UI
    public void UpdateUI()
    {
        if (dataManager == null) return;

        // Puxa os dados do Manager Persistente
        float currentXP = dataManager.CurrentXP;
        float requiredXP = dataManager.RequiredXP;
        int effectiveLevel = dataManager.EffectiveLevel;

        if (xpBarFill != null)
        {
            xpBarFill.fillAmount = currentXP / requiredXP;
        }

        if (xpValueText != null)
        {
            xpValueText.text = $"{currentXP.ToString("F0")} / {requiredXP.ToString("F0")} XP";
        }

        if (levelTextDisplay != null)
        {
            levelTextDisplay.text = effectiveLevel.ToString();
        }

        if (prestigeIconDisplay != null && prestigeSprites != null && prestigeSprites.Length > 0)
        {
            prestigeIconDisplay.sprite = GetCurrentPrestigeSprite();
            prestigeIconDisplay.enabled = true;
        }
        else if (prestigeIconDisplay != null)
        {
            prestigeIconDisplay.enabled = false;
        }
    }

    // Chamado pelo XPDataManager quando o jogador sobe de nível
    public void DisplayLevelUpUI()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);

            if (levelUpTextDisplay != null && dataManager != null)
            {
                levelUpTextDisplay.text = $"LEVEL UP! Nível: {dataManager.EffectiveLevel}";
            }

            if (levelUpSpriteDisplay != null)
            {
                levelUpSpriteDisplay.sprite = GetCurrentPrestigeSprite();
            }

            if (levelUpSoundSource != null && levelUpClip != null)
            {
                levelUpSoundSource.PlayOneShot(levelUpClip);
            }

            StartCoroutine(HideLevelUpUI());
        }
    }

    private IEnumerator HideLevelUpUI()
    {
        yield return new WaitForSeconds(displayDuration);

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }
}