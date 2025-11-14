using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MysteryBox : MonoBehaviour
{
    public GameObject[] armasDisponiveis;
    public int custoDaCaixa = 950;

    [Header("Drop Location")]
    // Ponto onde a arma instanciada deve aparecer
    public Transform dropLocation;

    [Header("UI e Interação")]
    public GameObject mensagemDeInteracao;
    private bool estaEmUso = false;
    private GameObject armaDropadaAtual = null; // Referência para a arma que está esperando para ser pega

    // Referência do WeaponSwitching (Pode ser definida no Inspector, mas FindFirstObjectByType é mais robusto aqui)
    private WeaponSwitching weaponSwitching;

    void Start()
    {
        if (mensagemDeInteracao != null)
        {
            mensagemDeInteracao.SetActive(false);
        }

        // Tenta encontrar a referência do WeaponSwitching no início.
        weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
        if (weaponSwitching == null)
        {
            Debug.LogError("WeaponSwitching não encontrado na cena. A funcionalidade da Caixa de Mistério não funcionará.");
        }
    }

    void Update()
    {
        if (estaEmUso && Input.GetKeyDown(KeyCode.F))
        {
            if (armaDropadaAtual == null)
            {
                AbrirCaixa();
            }
            else
            {
                Debug.Log("Pegue a arma atual antes de tentar usar a máquina novamente!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            estaEmUso = true;
            UpdateUI(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            estaEmUso = false;
            UpdateUI(false);
        }
    }

    private void UpdateUI(bool show)
    {
        if (mensagemDeInteracao != null)
        {
            mensagemDeInteracao.SetActive(show);
            if (show)
            {
                TextMeshProUGUI uiText = mensagemDeInteracao.GetComponentInChildren<TextMeshProUGUI>();
                if (uiText != null)
                {
                    string actionText = (armaDropadaAtual == null) ? $"Usar a máquina ({custoDaCaixa} pontos)" : "Pegar a Arma (F)";
                    uiText.text = $"Pressione F para {actionText}";
                }
            }
        }
    }

    private void AbrirCaixa()
    {
        if (weaponSwitching == null)
        {
            Debug.LogError("Não é possível usar a Máquina: WeaponSwitching ausente.");
            return;
        }

        // Assumo que PointManager.Instance e o custoDaCaixa estão funcionando corretamente
        // Se PointManager for nulo, a caixa não funciona
        if (PointManager.Instance != null && PointManager.Instance.currentPoints >= custoDaCaixa)
        {
            if (armaDropadaAtual != null) return;

            // 1. LÓGICA DE SORTEIO COM VERIFICAÇÃO

            // Cria uma lista de armas que o jogador AINDA NÃO POSSUI
            List<GameObject> armasNaoPossuidas = new List<GameObject>();
            foreach (GameObject armaPrefab in armasDisponiveis)
            {
                // Verifica se o jogador JÁ TEM a arma (usando o nome do prefab)
                if (!weaponSwitching.HasWeapon(armaPrefab.name))
                {
                    armasNaoPossuidas.Add(armaPrefab);
                }
            }

            // Se o jogador já tiver TODAS as armas na lista:
            if (armasNaoPossuidas.Count == 0)
            {
                Debug.Log("Parabéns! Você já possui todas as armas sorteadas. Nenhum prêmio concedido (ou adicione lógica de recompensa).");
                return;
            }

            // --- INÍCIO DA MUDANÇA (OPÇÃO 1) ---

            // Embaralha a lista de armas não possuídas. 
            // Isso garante que a ordem muda a cada uso, forçando o Random.Range a ter 
            // resultados diferentes mesmo que as armas restantes sejam as mesmas.
            armasNaoPossuidas.Shuffle();

            // 2. Sorteia APENAS entre as armas que o jogador NÃO POSSUI
            PointManager.Instance.SubtractPoints(custoDaCaixa);

            // Sorteia um índice da lista embaralhada
            int indiceAleatorio = Random.Range(0, armasNaoPossuidas.Count);
            GameObject armaEscolhidaPrefab = armasNaoPossuidas[indiceAleatorio];

            // --- FIM DA MUDANÇA (OPÇÃO 1) ---

            // 3. INSTANCIAÇÃO
            GameObject novaArmaDropada = Instantiate(armaEscolhidaPrefab, dropLocation.position, dropLocation.rotation);

            // Adiciona o script DroppedWeapon (assumindo que ele está na sua pasta de projeto)
            DroppedWeapon dropScript = novaArmaDropada.AddComponent<DroppedWeapon>();

            if (dropScript != null)
            {
                armaDropadaAtual = novaArmaDropada;

                // Passa o PREFAB original e a instância de WeaponSwitching e MysteryBox
                dropScript.SetWeaponData(armaEscolhidaPrefab, weaponSwitching, this);
            }

            Debug.Log($"Arma '{armaEscolhidaPrefab.name}' dropada da máquina!");

            UpdateUI(true);
        }
        else
        {
            Debug.Log("Você não tem pontos suficientes!");
        }
    }

    // Método chamado pelo DroppedWeapon quando a arma é pega ou desaparece.
    public void WeaponWasTaken(GameObject weaponObject)
    {
        if (armaDropadaAtual == weaponObject)
        {
            armaDropadaAtual = null; // Libera a caixa para abrir novamente
            if (estaEmUso)
            {
                UpdateUI(true);
            }
        }
    }
}