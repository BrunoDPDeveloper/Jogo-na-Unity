using UnityEngine;

[System.Serializable]
public class WeaponXPData
{
    // --- CHAVES GENÉRICAS ---
    private const string LEVEL_SUFFIX = "_GunLevel";
    private const string XP_SUFFIX = "_GunXP";

    [Tooltip("Nível atual da arma.")]
    public int currentGunLevel = 1;

    [Tooltip("XP atual da arma para o próximo nível.")]
    public float currentGunXP = 0f;

    [Tooltip("XP base necessária para o Nível 1.")]
    public float xpBaseAmount = 500f;

    [Tooltip("Fator de crescimento (ex: 1.15f para 15% de aumento por nível).")]
    public float xpGrowthFactor = 1.15f;

    [Tooltip("Nível máximo da arma antes de atingir o Master.")]
    public int maxGunLevel = 50;

    // Calcula a XP necessária para o próximo nível
    public float RequiredXP
    {
        get
        {
            if (currentGunLevel >= maxGunLevel) return float.MaxValue;
            return xpBaseAmount * Mathf.Pow(xpGrowthFactor, currentGunLevel - 1);
        }
    }

    // CARREGA OS DADOS DE XP DA ARMA
    public void LoadXP(string weaponID)
    {
        string levelKey = weaponID + LEVEL_SUFFIX;
        string xpKey = weaponID + XP_SUFFIX;

        if (PlayerPrefs.HasKey(levelKey))
        {
            currentGunLevel = PlayerPrefs.GetInt(levelKey, 1);
            currentGunXP = PlayerPrefs.GetFloat(xpKey, 0f);

            Debug.Log($"[WeaponXP] Dados carregados para {weaponID}. Nível: {currentGunLevel}, XP: {currentGunXP}");
        }
        // Se não houver dados, o jogo usará os valores padrão definidos no Inspector.
    }

    // SALVA OS DADOS DE XP DA ARMA
    public void SaveXP(string weaponID)
    {
        string levelKey = weaponID + LEVEL_SUFFIX;
        string xpKey = weaponID + XP_SUFFIX;

        PlayerPrefs.SetInt(levelKey, currentGunLevel);
        PlayerPrefs.SetFloat(xpKey, currentGunXP);
        PlayerPrefs.Save();

        // Debug.Log($"[WeaponXP] Dados de XP salvos para {weaponID}.");
    }
}