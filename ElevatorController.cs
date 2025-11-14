using UnityEngine;
using System.Collections;
using TMPro; // Para a UI de interação (opcional)

public class ElevatorController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    // Posições de referência que você define no Inspector
    public Transform pontoA; // Posição Inferior (Start)
    public Transform pontoB; // Posição Superior (End)
    public float tempoDeViagem = 3.0f; // Quanto tempo leva para ir de A para B
    public float tempoDeEspera = 2.0f; // Tempo que o elevador fica parado antes de retornar

    [Header("Configurações de Interação")]
    public KeyCode teclaDeAtivacao = KeyCode.F;
    public GameObject mensagemDeInteracao; // UI para mostrar "Pressione F"
    private bool estaEmTrigger = false;
    private bool estaEmMovimento = false;
    private bool isAtPositionA = true; // Começa na posição A (Inferior)

    private Transform playerTransform;

    void Start()
    {
        // VERIFICAÇÃO E CORREÇÃO CRUCIAL:
        // Garante que o elevador comece exatamente na posição A
        if (pontoA != null)
        {
            // Força a posição do elevador para a posição do Ponto A
            transform.position = pontoA.position;
        }
        else
        {
            Debug.LogError("O Ponto A (pontoA) não está configurado no Inspector! O elevador pode estar na posição errada.");
        }

        if (mensagemDeInteracao != null)
        {
            mensagemDeInteracao.SetActive(false);
        }
    }

    void Update()
    {
        if (estaEmMovimento) return;

        if (estaEmTrigger && Input.GetKeyDown(teclaDeAtivacao))
        {
            if (isAtPositionA)
            {
                StartCoroutine(MoveElevator(pontoA.position, pontoB.position));
            }
            else
            {
                StartCoroutine(MoveElevator(pontoB.position, pontoA.position));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto tem a tag "Player"
        if (other.CompareTag("Player"))
        {
            estaEmTrigger = true;
            playerTransform = other.transform;
            ShowInteractionUI(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            estaEmTrigger = false;
            // Remove o Player do Parentamento APENAS se ele estiver como filho (para evitar erros)
            if (playerTransform != null && playerTransform.parent == transform)
            {
                playerTransform.SetParent(null);
            }
            playerTransform = null;
            ShowInteractionUI(false);
        }
    }

    private void ShowInteractionUI(bool show)
    {
        if (mensagemDeInteracao != null)
        {
            mensagemDeInteracao.SetActive(show);
        }
    }

    IEnumerator MoveElevator(Vector3 startPos, Vector3 endPos)
    {
        estaEmMovimento = true;
        float elapsedTime = 0f;

        // Parentamento do Jogador
        // CRUCIAL: Parenta o Player no elevador ANTES de começar a se mover
        if (playerTransform != null)
        {
            playerTransform.SetParent(transform);
        }

        // Movimento Suave
        while (elapsedTime < tempoDeViagem)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / tempoDeViagem);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        isAtPositionA = !isAtPositionA;

        // Tempo de Espera
        yield return new WaitForSeconds(tempoDeEspera);

        // Remove o parentamento do jogador após o tempo de espera (ou quando o player sair)
        // Nota: O OnTriggerExit já lida com o desprentamento se o jogador sair por conta própria.

        estaEmMovimento = false;
    }
}