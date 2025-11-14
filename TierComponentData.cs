using UnityEngine;

// Este script é anexado ao prefab de cada item de loot (TierComponent1, TierComponent2, etc.)
public class TierComponentData : MonoBehaviour
{
    [Tooltip("Nome do item para exibição e para PlayerPrefs.")]
    public string componentName = "Nome do Componente"; // Valor de exemplo, será sobrescrito

    [Tooltip("Chave única usada no PlayerPrefs. Ex: 'Componente_T1'")]
    public string playerPrefsKey = "Componente_Tx"; // Valor de exemplo, será sobrescrito
}