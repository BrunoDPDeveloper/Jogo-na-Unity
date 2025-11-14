using UnityEngine;
using TMPro; // É necessário para usar o TextMeshPro

/// <summary>
/// Exibe o valor de Frames Por Segundo (FPS) em um TextMeshProUGUI.
/// </summary>
public class FPSDisplay : MonoBehaviour
{
    // A referência para o componente de texto onde o FPS será exibido.
    // Atribua um objeto TextMeshProUGUI (do seu Canvas) a este campo no Inspector.
    [Header("Referência de UI")]
    public TextMeshProUGUI fpsText;

    [Header("Configurações")]
    // Intervalo de tempo para atualizar o texto (em segundos).
    // Atualizar a cada 0.5s é um bom equilíbrio entre precisão e desempenho.
    public float updateInterval = 0.5f;

    private float timeUntilUpdate = 0f;
    private int frameCount = 0;

    void Start()
    {
        // Define o tempo inicial para a primeira atualização.
        timeUntilUpdate = updateInterval;

        // Garante que o objeto de texto foi atribuído.
        if (fpsText == null)
        {
            Debug.LogError("O componente TextMeshProUGUI não está atribuído ao campo 'fpsText' no Inspector do FPSDisplay.");
            enabled = false; // Desabilita o script para evitar erros.
        }
    }

    void Update()
    {
        // 1. Contagem de Frames
        frameCount++;
        timeUntilUpdate -= Time.deltaTime;

        // 2. Verifica se é hora de atualizar o FPS
        if (timeUntilUpdate <= 0)
        {
            // Calcula o FPS: frames contados / tempo que levou para contá-los
            float fps = frameCount / updateInterval;

            // Define a cor com base no desempenho (Opcional)
            Color displayColor = Color.green;
            if (fps < 30) // Mau desempenho
            {
                displayColor = Color.red;
            }
            else if (fps < 60) // Desempenho razoável
            {
                displayColor = Color.yellow;
            }

            // Formata o texto para exibição (ex: "FPS: 60")
            // Usamos {0:0} para formatar o número com zero casas decimais.
            fpsText.text = $": {Mathf.RoundToInt(fps)}";
            fpsText.color = displayColor; // Aplica a cor

            // 3. Reseta os contadores para o próximo intervalo
            frameCount = 0;
            // timeUntilUpdate = updateInterval; 
            // CORREÇÃO: Isso evita acúmulo de tempo se o frame rate cair muito
            timeUntilUpdate += updateInterval;
        }
    }
}