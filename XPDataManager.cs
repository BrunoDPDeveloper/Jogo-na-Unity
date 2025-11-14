using UnityEngine;
using System.Collections;

public class XPDataManager : MonoBehaviour
{
    // --- CHAVES DE PLAYERPREFS ---
    private const string XP_KEY = "Player_CurrentXP";
    private const string LEVEL_KEY = "Player_CurrentLevel";
    private const string PRESTIGE_KEY = "Player_CurrentPrestige";

    public static XPDataManager Instance { get; private set; }

    [Header("XP Settings")]
    public float CurrentXP = 0f;
    public int CurrentLevel = 1;
    public int CurrentPrestige = 0;

    [Header("XP Calculation")]
    [SerializeField] private float xpBaseAmount;
    [SerializeField] private float xpGrowthFactor = 1.15f;
    [SerializeField] private int maxLevelsPerPrestige = 55;

    public float RequiredXP { get; private set; }

    // Propriedade para calcular o nível efetivo (Total)
    public int EffectiveLevel => (CurrentPrestige * maxLevelsPerPrestige) + CurrentLevel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // ESSENCIAL: Permite que este objeto sobreviva à troca de cena
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadXPData();
        CalculateRequiredXP();
    }

    private void LoadXPData()
    {
        if (PlayerPrefs.HasKey(LEVEL_KEY))
        {
            CurrentXP = PlayerPrefs.GetFloat(XP_KEY, 0f);
            CurrentLevel = PlayerPrefs.GetInt(LEVEL_KEY, 1);
            CurrentPrestige = PlayerPrefs.GetInt(PRESTIGE_KEY, 0);

            Debug.Log($"[Data] Dados de XP carregados. Nível: {EffectiveLevel}, Prestígio: {CurrentPrestige}");
        }
        else
        {
            // Salva os valores iniciais se não houver dados
            SaveXPData();
        }
    }

    public void SaveXPData()
    {
        PlayerPrefs.SetFloat(XP_KEY, CurrentXP);
        PlayerPrefs.SetInt(LEVEL_KEY, CurrentLevel);
        PlayerPrefs.SetInt(PRESTIGE_KEY, CurrentPrestige);
        PlayerPrefs.Save();
        Debug.Log("[Data] Dados de XP salvos.");
    }

    private void CalculateRequiredXP()
    {
        // Usa o EffectiveLevel para calcular a XP necessária
        RequiredXP = xpBaseAmount * Mathf.Pow(xpGrowthFactor, EffectiveLevel - 1);
    }

    public void AddXP(float baseAmount)
    {
        CurrentXP += baseAmount;
        CheckForLevelUp();
        SaveXPData();

        // Informa o módulo de UI da cena atual para se atualizar
        if (PlayerXPManager.Instance != null)
        {
            PlayerXPManager.Instance.UpdateUI();
        }
    }

    private void CheckForLevelUp()
    {
        if (CurrentXP >= RequiredXP)
        {
            CurrentXP -= RequiredXP;
            CurrentLevel++;

            if (CurrentLevel > maxLevelsPerPrestige)
            {
                GainPrestige();
            }

            CalculateRequiredXP();
            Debug.Log($"[Data] Level Up! Nível efetivo: {EffectiveLevel}");

            // Informa o módulo de UI para exibir a animação
            if (PlayerXPManager.Instance != null)
            {
                PlayerXPManager.Instance.DisplayLevelUpUI();
            }

            SaveXPData();
        }
    }

    private void GainPrestige()
    {
        CurrentPrestige++;
        CurrentLevel = 1;
        CurrentXP = 0;

        Debug.Log($"[Data] Parabéns! Você alcançou o Prestígio {CurrentPrestige}!");
        CalculateRequiredXP();
    }

    // --- MÉTODOS PARA DEVS ---

    public void ResetPlayerProgress()
    {
        CurrentXP = 0f;
        CurrentLevel = 1;
        CurrentPrestige = 0;
        CalculateRequiredXP();
        SaveXPData();
        if (PlayerXPManager.Instance != null) PlayerXPManager.Instance.UpdateUI();
        Debug.Log("[Data] Progresso do jogador resetado para Nível 1, Prestígio 0.");
    }

    public void SetPrestigeLevel(int newPrestige)
    {
        newPrestige = Mathf.Clamp(newPrestige, 0, 999); // Use um limite apropriado
        if (CurrentPrestige != newPrestige)
        {
            CurrentPrestige = newPrestige;
            CurrentLevel = 1;
            CurrentXP = 0f;
            CalculateRequiredXP();
            SaveXPData();
            if (PlayerXPManager.Instance != null) PlayerXPManager.Instance.UpdateUI();
            Debug.Log($"[Data] Prestígio ajustado para: {CurrentPrestige}");
        }
    }

    public void SetCurrentLevel(int newLevel)
    {
        newLevel = Mathf.Clamp(newLevel, 1, maxLevelsPerPrestige);
        if (CurrentLevel != newLevel)
        {
            CurrentLevel = newLevel;
            CurrentXP = 0f;
            CalculateRequiredXP();
            SaveXPData();
            if (PlayerXPManager.Instance != null) PlayerXPManager.Instance.UpdateUI();
            Debug.Log($"[Data] Nível ajustado para: {CurrentLevel}");
        }
    }
}