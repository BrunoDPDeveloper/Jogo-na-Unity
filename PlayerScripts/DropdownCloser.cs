using UnityEngine;
using UnityEngine.EventSystems; // Necessário para detectar entrada/saída do mouse
using TMPro; // Necessário para a referência ao Dropdown

// Este script DEVE ser anexado ao painel (pai) que contém todos os elementos de UI do menu.
public class DropdownCloser : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    // Arraste o seu objeto Dropdown (TMP) de sensibilidade para este campo no Inspector
    [SerializeField] private TMP_Dropdown sensitivityDropdown;

    // Tempo em segundos que o mouse precisa ficar fora da área antes de fechar (0.5s é um bom valor)
    [SerializeField] private float closeDelay = 0.5f;

    private float exitTime = 0f;
    private bool isPointerInside = false;

    // --- Implementação da Interface ---

    // Chamado quando o ponteiro do mouse SAI da área do painel.
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        exitTime = Time.time; // Registra o momento da saída
    }

    // Chamado quando o ponteiro do mouse ENTRA na área do painel.
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
    }

    // --- Lógica de Fechamento ---

    private void Update()
    {
        // 1. Verifica se o Dropdown existe e está aberto na hierarquia
        if (sensitivityDropdown != null && sensitivityDropdown.gameObject.activeInHierarchy)
        {
            // 2. Verifica se o mouse saiu da área do painel e se o tempo de atraso já passou
            if (!isPointerInside && Time.time > exitTime + closeDelay)
            {
                // Para fechar o Dropdown via script, precisamos "desslecionar" ele.
                // Isso simula um clique em algum lugar fora.
                EventSystem.current.SetSelectedGameObject(null);

                // Opcional: Para debugar e garantir que está funcionando
                // Debug.Log("Dropdown fechado automaticamente após mouse sair da área.");
            }
        }
    }
}