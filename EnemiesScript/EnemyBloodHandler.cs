using UnityEngine;

public class EnemyBloodHandler : MonoBehaviour
{
    public GameObject bloodEffectPrefab;

    // Este método pode ser chamado por qualquer outro script
    public void SpawnBlood(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (bloodEffectPrefab != null)
        {
            // Instancia o sangue no ponto de impacto
            GameObject blood = Instantiate(bloodEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(blood, 1f); // destrói o efeito depois de 1s
        }
    }
}