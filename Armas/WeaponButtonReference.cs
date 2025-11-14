using UnityEngine;

public class WeaponButtonReference : MonoBehaviour
{
    [Tooltip("O Prefab da arma a que este botão se refere.")]
    public GameObject weaponPrefab;

    [Tooltip("A referência direta ao WeaponUpgradeUIController.")]
    public WeaponUpgradeUIController uiController;

    /// <summary>
    /// Chamado pelo evento OnClick() do botão. 
    /// Encontra a instância da arma na cena e atualiza o painel de upgrade.
    /// </summary>
    public void SelectWeaponForUpgrade()
    {
        Debug.Log($"[BUTTON CLICK] Botão {weaponPrefab.name} clicado. Iniciando busca...");

        // 1. Encontra a instância real da arma na cena (incluindo inativas)
        AssaultRifle weaponInstance = FindWeaponInstanceInScene();

        if (uiController == null)
        {
            Debug.LogError("[BUTTON] UI Controller não está definido no botão. Não é possível atualizar o painel.");
            return;
        }

        if (weaponInstance != null)
        {
            // 2. A arma foi encontrada: passa a instância para o controlador de UI
            Debug.Log($"[SUCESSO] Instância da arma '{weaponInstance.weaponName}' encontrada na cena!");
            uiController.UpdateUpgradePanel(weaponInstance);
        }
        else
        {
            // 3. A arma NÃO foi encontrada: Se a arma ainda não foi comprada.
            string nameFromPrefab = GetWeaponNameFromPrefab();

            // ⭐ NOVO COMPORTAMENTO: Se a arma não existe, é porque ela não foi comprada.
            // Oculta o painel de upgrade e mostra, talvez, uma mensagem de "Arma não comprada".
            Debug.LogWarning($"[AÇÃO NECESSÁRIA] Arma '{nameFromPrefab}' não foi comprada/instanciada. Painel de upgrade oculto.");

            // Certifique-se que o HideUpgradePanel lida com o estado de 'não comprada'.
        }
    }

    private AssaultRifle FindWeaponInstanceInScene()
    {
        string targetWeaponName = GetWeaponNameFromPrefab();

        // ⭐ Corrigido o erro CS1503
        // Procura TODOS os scripts AssaultRifle (incluindo inativos)
        AssaultRifle[] allWeapons = FindObjectsByType<AssaultRifle>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (AssaultRifle weapon in allWeapons)
        {
            if (weapon.weaponName == targetWeaponName)
            {
                // Se a arma existe, o sistema pode aprimorá-la.
                return weapon;
            }
        }
        return null;
    }

    private string GetWeaponNameFromPrefab()
    {
        // ... (código inalterado) ...
        if (weaponPrefab == null)
        {
            Debug.LogError("[ERRO FATAL] weaponPrefab está nulo no WeaponButtonReference!");
            return "";
        }

        AssaultRifle prefabScript = weaponPrefab.GetComponent<AssaultRifle>();
        if (prefabScript != null)
        {
            return prefabScript.weaponName;
        }

        Debug.LogError($"[ERRO FATAL] Prefab {weaponPrefab.name} não tem o script AssaultRifle!");
        return weaponPrefab.name;
    }
}