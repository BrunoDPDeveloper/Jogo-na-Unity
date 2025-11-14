using UnityEngine;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    // ARRASTE SEUS DROPDOWNS AQUI NO INSPECTOR DESTE OBJETO
    [SerializeField] private TMP_Dropdown qualityDropdownRef;
    [SerializeField] private TMP_Dropdown sensitivityDropdownRef;

    private void OnEnable()
    {
        // O OnEnable é chamado sempre que o painel de configurações é ativado

        // Verifica se o Singleton principal está pronto
        if (GameSettings.Instance != null)
        {
            // Passa as referências da UI que ACABARAM de ser carregadas para o Singleton
            GameSettings.Instance.SetQualityDropdown(qualityDropdownRef);
            GameSettings.Instance.SetSensitivityDropdown(sensitivityDropdownRef);

            Debug.Log("Referências da UI de Configurações re-estabelecidas com sucesso!");
        }
        else
        {
           // Debug.LogError("GameSettings.Instance não encontrado. Certifique-se de que o GameManager carregue primeiro.");
        }
    }
}