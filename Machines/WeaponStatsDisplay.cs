using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gerencia a exibição visual das estatísticas de uma arma em um Menu de Compra.
/// Inclui Sliders de Dano e Alcance com limite visual e exibição de Munição.
/// Deve ser anexado ao painel lateral que contém os Sliders/Textos.
/// </summary>
public class WeaponStatsDisplay : MonoBehaviour
{
    // --- Configurações de UI (Arraste e Solte no Inspector) ---
    [Header("UI Componentes (Sliders e Textos)")]
    public Slider damageSlider;
    public Image damageFillImage;
    public Slider rangeSlider;
    public Image rangeFillImage;
    public Slider headshotdamageSlider;
    public Image headshotdamageFillImage;

    [Header("UI Componentes de Munição")]
    public TextMeshProUGUI magazineSizeText;
    public TextMeshProUGUI reserveAmmoText;

    // --- Valores Máximos de Comparação (Ajuste no Inspector) ---
    [Header("Valores Máximos de Comparação")]
    public float maxDamageWeapon = 100000f; // Dano máximo esperado (CORPO)
    public float maxheadshotDamage = 200000f; // Dano de Headshot máximo esperado
    public float maxRange = 500f;    // Alcance máximo esperado

    [Header("Configuração de Cores")]
    public Color normalColor = Color.white;
    public Color maxColor = Color.orange;

    // Classe auxiliar genérica para armazenar temporariamente as estatísticas
    private class TempWeaponStats
    {
        public float Damage;
        public float HeadshotDamage;
        public float Range;
        public int ClipSize;
        public int MaxReserveAmmo;
        public bool IsValid = false;
    }

    /// <summary>
    /// Atualiza todos os elementos de UI com base nos dados de um prefab de arma.
    /// </summary>
    /// <param name="weaponPrefab">O Prefab da arma (GameObject) contendo o script de Arma.</param>
    public void UpdateDisplay(GameObject weaponPrefab)
    {
        TempWeaponStats stats = new TempWeaponStats();

        // 1. Tenta obter AssaultRifle
        AssaultRifle arStats = weaponPrefab.GetComponent<AssaultRifle>();
        if (arStats != null)
        {
            stats.Damage = arStats.damage;
            stats.HeadshotDamage = arStats.headshotDamage;
            stats.Range = arStats.range;
            stats.ClipSize = arStats.clipSize;
            stats.MaxReserveAmmo = arStats.maxReserveAmmo;
            stats.IsValid = true;
        }

        // 2. Tenta obter Shotgun
        Shotgun sgStats = weaponPrefab.GetComponent<Shotgun>();
        if (sgStats != null)
        {
            // Para Shotgun, exibimos o dano TOTAL (damage * pelletsPerShot)
            // Lemos as PROPRIEDADES CALCULADAS que fazem dano * 6
            stats.Damage = sgStats.TotalBodyDamage;
            stats.HeadshotDamage = sgStats.TotalHeadshotDamage;
            stats.Range = sgStats.range;
            stats.ClipSize = sgStats.clipSize;
            stats.MaxReserveAmmo = sgStats.maxReserveAmmo;
            stats.IsValid = true;
        }

        // ➡️ 3. NOVO: Tenta obter Pistol
        Pistol pistolStats = weaponPrefab.GetComponent<Pistol>();
        if (pistolStats != null)
        {
            stats.Damage = pistolStats.damage;
            stats.HeadshotDamage = pistolStats.headshotDamage;
            stats.Range = pistolStats.range;
            stats.ClipSize = pistolStats.clipSize;
            stats.MaxReserveAmmo = pistolStats.maxReserveAmmo;
            stats.IsValid = true;
        }
        // ➡️ 4. NOVO: Tenta obter Sniper
        Sniper sniperStats = weaponPrefab.GetComponent<Sniper>();
        if (sniperStats != null)
        {
            stats.Damage = sniperStats.damage;
            stats.HeadshotDamage = sniperStats.headshotDamage;
            stats.Range = sniperStats.range;
            stats.ClipSize = sniperStats.clipSize;
            stats.MaxReserveAmmo = sniperStats.maxReserveAmmo;
            stats.IsValid = true;
        }

        if (!stats.IsValid)
        {
            Debug.LogError($"O Prefab '{weaponPrefab.name}' não possui o script 'AssaultRifle' nem 'Shotgun'.");
            // Limpa o display se nenhuma arma for encontrada
            ClearDisplay();
            return;
        }

        float currentDamage = stats.Damage;
        float currentRange = stats.Range;
        float currentheadshotDamage = stats.HeadshotDamage;


        // 1. ATUALIZAÇÃO DO SLIDER DE DANO (CORPO)
        if (damageSlider != null)
        {
            float damageRatio = Mathf.Clamp01(currentDamage / maxDamageWeapon);
            damageSlider.value = damageRatio;

            if (damageFillImage != null)
            {
                damageFillImage.color = Color.Lerp(normalColor, maxColor, damageRatio);
            }
        }

        // 2. ATUALIZAÇÃO DO SLIDER DE DANOS NA CABEÇA
        if (headshotdamageSlider != null)
        {
            float headshotdamageRatio = Mathf.Clamp01(currentheadshotDamage / maxheadshotDamage);
            headshotdamageSlider.value = headshotdamageRatio;

            if (headshotdamageFillImage != null)
            {
                headshotdamageFillImage.color = Color.Lerp(normalColor, maxColor, headshotdamageRatio);
            }
        }

        // 3. ATUALIZAÇÃO DO SLIDER DE ALCANCE
        if (rangeSlider != null)
        {
            float rangeRatio = Mathf.Clamp01(currentRange / maxRange);
            rangeSlider.value = rangeRatio;

            if (currentRange > maxRange)
            {
                Debug.LogWarning($"O Alcance da arma ({currentRange}) excede o limite visual ({maxRange}). O slider será preenchido 100%.");
            }

            if (rangeFillImage != null)
            {
                rangeFillImage.color = Color.Lerp(normalColor, maxColor, rangeRatio);
            }
        }

        // --- 4. ATUALIZAÇÃO DOS TEXTOS DE MUNIÇÃO ---

        if (magazineSizeText != null)
        {
            magazineSizeText.text = stats.ClipSize.ToString();
        }

        if (reserveAmmoText != null)
        {
            reserveAmmoText.text = stats.MaxReserveAmmo.ToString();
        }
    }

    // Método auxiliar para limpar o display em caso de erro
    private void ClearDisplay()
    {
        if (damageSlider != null) damageSlider.value = 0f;
        if (headshotdamageSlider != null) headshotdamageSlider.value = 0f;
        if (rangeSlider != null) rangeSlider.value = 0f;
        if (magazineSizeText != null) magazineSizeText.text = "N/A";
        if (reserveAmmoText != null) reserveAmmoText.text = "N/A";
    }
}