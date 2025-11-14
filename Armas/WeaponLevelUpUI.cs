using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class WeaponLevelUpUI : MonoBehaviour
{
    // A ÚNICA REFERÊNCIA ESTÁTICA para que as Armas possam encontrar este Manager.
    public static WeaponLevelUpUI Instance;

    [Header("Weapon Level Up UI")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TextMeshProUGUI weaponNameDisplay;
    [SerializeField] private TextMeshProUGUI levelTextDisplay;
    [SerializeField] private Image weaponSpriteDisplay;

    [Header("Settings")]
    [SerializeField] private AudioSource levelUpSoundSource;
    [SerializeField] private AudioClip levelUpClip;
    [SerializeField] private float displayDuration = 3f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        // Singleton: Garante que apenas uma instância deste UI Manager exista.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Garante que o painel está oculto no início
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Chamado pelo script da arma (ex: AssaultRifle) quando ela sobe de nível.
    /// </summary>
    /// <param name="weaponName">O nome da arma.</param>
    /// <param name="currentLevel">O novo nível da arma.</param>
    /// <param name="weaponIcon">O Sprite da arma (opcional, se a arma o fornecer).</param>
    public void DisplayWeaponLevelUp(string weaponName, int currentLevel, Sprite weaponIcon = null)
    {
        // 1. Interrompe a coroutine anterior para reiniciar o temporizador
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // 2. Define o Texto e o Nível
        if (weaponNameDisplay != null)
        {
            weaponNameDisplay.text = weaponName.ToUpper();
        }

        if (levelTextDisplay != null)
        {
            levelTextDisplay.text = $"Nível {currentLevel} Alcançado!";
        }

        // 3. Define o Sprite
        if (weaponSpriteDisplay != null)
        {
            if (weaponIcon != null)
            {
                weaponSpriteDisplay.sprite = weaponIcon;
                weaponSpriteDisplay.enabled = true;
            }
            else
            {
                // Se nenhum ícone for fornecido, desativa o Image (ou usa um ícone padrão)
                weaponSpriteDisplay.enabled = false;
            }
        }

        // 4. Ativa o painel e o som
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }

        if (levelUpSoundSource != null && levelUpClip != null)
        {
            levelUpSoundSource.PlayOneShot(levelUpClip);
        }

        // 5. Inicia o temporizador para ocultar
        hideCoroutine = StartCoroutine(HideLevelUpUI());
    }

    private IEnumerator HideLevelUpUI()
    {
        yield return new WaitForSeconds(displayDuration);

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
        hideCoroutine = null;
    }
}