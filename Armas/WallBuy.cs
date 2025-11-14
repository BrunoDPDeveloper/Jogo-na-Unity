using UnityEngine;
using TMPro;

public class WallBuy : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource wallBuyAudioSource;
    public AudioClip buyedWallBuyClip;

    [Header("Configurações da Arma")]
    public GameObject weaponToBuyPrefab;
    public int weaponCost;
    public int ammoCost; // <-- NOVO: Custo para comprar a munição

    [Header("UI e Interação")]
    public GameObject interactionUI;
    public string weaponName;

    [Header("Referências")]
    public WeaponSwitching weaponSwitching;

    private bool isPlayerNearby = false;
    private bool hasWeapon = false; // Flag para saber se o player já tem a arma

    void Start()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        if (weaponSwitching == null)
        {
            Debug.LogError("A referência ao WeaponSwitching não foi definida no Inspector!");
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            BuyItem(); // Chama a nova função principal de compra
        }
    }

    // Verifica se o jogador já possui a arma
    private void CheckWeaponPossession()
    {
        if (weaponSwitching != null && weaponToBuyPrefab != null)
        {
            // Passamos o nome do prefab para a função de verificação
            hasWeapon = weaponSwitching.HasWeapon(weaponToBuyPrefab.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            CheckWeaponPossession(); // Verifica posse ao entrar
            UpdateUI(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            UpdateUI(false);
        }
    }

    private void UpdateUI(bool show)
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(show);
            if (show)
            {
                CheckWeaponPossession(); // Atualiza o estado da posse antes de exibir

                TextMeshProUGUI uiText = interactionUI.GetComponentInChildren<TextMeshProUGUI>();
                if (uiText != null)
                {
                    string message;
                    if (hasWeapon)
                    {
                        message = $"Pressione F para comprar munição para {weaponName} ({ammoCost} pontos)";
                    }
                    else
                    {
                        message = $"Pressione F para comprar {weaponName} ({weaponCost} pontos)";
                    }
                    uiText.text = message;
                }
            }
        }
    }

    private void BuyItem()
    {
        if (hasWeapon)
        {
            BuyAmmo();
        }
        else
        {
            BuyWeapon();
        }
    }

    private void BuyAmmo()
    {
        if (PointManager.Instance == null) return;

        if (PointManager.Instance.currentPoints >= ammoCost)
        {
            // NOVO: Chama o método do WeaponSwitching para adicionar munição
            if (weaponSwitching.AddAmmoToWeapon(weaponToBuyPrefab.name))
            {
                PointManager.Instance.SubtractPoints(ammoCost);
                wallBuyAudioSource.PlayOneShot(buyedWallBuyClip);
                Debug.Log($"Munição comprada para '{weaponName}'!");
            }
            else
            {
                Debug.Log($"A arma '{weaponName}' já está com munição cheia ou não foi encontrada.");
            }
            weaponSwitching.UpdateAmmoUI(); // Atualiza a UI da munição
        }
        else
        {
            Debug.Log("Você não tem pontos suficientes para comprar munição.");
        }
    }

    private void BuyWeapon()
    {
        if (PointManager.Instance == null) return;

        if (PointManager.Instance.currentPoints >= weaponCost)
        {
            PointManager.Instance.SubtractPoints(weaponCost);

            if (weaponSwitching != null)
            {
                weaponSwitching.AddNewWeapon(weaponToBuyPrefab);
                wallBuyAudioSource.PlayOneShot(buyedWallBuyClip);
                Debug.Log($"Arma '{weaponName}' comprada com sucesso!");
                hasWeapon = true; // Atualiza o estado
                UpdateUI(true);   // Atualiza a UI para mostrar a opção de munição
            }
            else
            {
                Debug.LogError("A referência ao WeaponSwitching não foi definida no Inspector!");
            }
        }
        else
        {
            Debug.Log("Você não tem pontos suficientes para comprar esta arma.");
        }
    }
}