using UnityEngine;
using System.Collections; // Importante para Coroutines

public class DroppedWeapon : MonoBehaviour
{
    private WeaponSwitching weaponSwitching;
    private MysteryBox mysteryBox;
    private GameObject weaponPrefab;

    private bool isPlayerNearby = false;

    [Header("Configurações de Desaparecimento")]
    public float timeToDisappear = 15f; // Tempo em segundos antes de a arma sumir

    // Opcional: Efeito de desaparecimento (ex: animação de encerramento)
    // public Animator animator; 

    // Recebe a WeaponSwitching e a referência do Prefab original para a compra.
    public void SetWeaponData(GameObject originalPrefab, WeaponSwitching ws, MysteryBox mb)
    {
        weaponPrefab = originalPrefab;
        weaponSwitching = ws;
        mysteryBox = mb;

        // Inicia o contador de tempo limite assim que a arma é dropada
        StartCoroutine(WeaponTimeout());
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            PickUpWeapon();
        }
    }

    // ... (OnTriggerEnter, OnTriggerExit - MANTENHA IGUAIS) ...

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    private void PickUpWeapon()
    {
        if (weaponSwitching != null && weaponPrefab != null)
        {
            // 1. Para a Coroutine para que a arma não desapareça após ser pega
            StopAllCoroutines();

            // 2. ADICIONA/TROCA A ARMA
            weaponSwitching.AddNewWeapon(weaponPrefab);

            // 3. INFORMA A CAIXA E LIBERA PARA NOVO USO
            if (mysteryBox != null)
            {
                mysteryBox.WeaponWasTaken(gameObject);
            }

            // 4. DESTROI O OBJETO DA ARMA NO CHÃO
            Destroy(gameObject);
            Debug.Log($"Arma {weaponPrefab.name} pega!");
        }
        else
        {
            Debug.LogError("Erro ao pegar a arma: Referências WeaponSwitching ou Prefab ausentes.");
        }
    }

    // COROUTINE DE TEMPO LIMITE
    IEnumerator WeaponTimeout()
    {
        // Espera o tempo definido (ex: 15 segundos)
        yield return new WaitForSeconds(timeToDisappear);

        // Se a execução chegar até aqui, a arma não foi pega.

        Debug.Log($"Tempo esgotado. A arma {weaponPrefab.name} está desaparecendo!");

        // 1. Informa a Caixa de Mistério que a arma desapareceu
        if (mysteryBox != null)
        {
            // Isso libera a caixa para o próximo uso
            mysteryBox.WeaponWasTaken(gameObject);
        }

        // Opcional: Tocar animação de sumiço/efeito de teletransporte aqui
        // if (animator != null) animator.SetTrigger("Disappear");

        // 2. Destrói o objeto no mundo
        Destroy(gameObject);
    }
}