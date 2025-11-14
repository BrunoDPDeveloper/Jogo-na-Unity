using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    [Header("Configurações de FPS")]
    [Tooltip("O FPS alvo para o jogo. 60 é o padrão para estabilidade.")]
    public int targetFPS = 60; // Você pode mudar isso para 75, 90, etc.

    void Awake()
    {
        // Garante que este objeto persista entre as cenas, se for um GameManager.
        // Se este script estiver no GameManager, você pode querer descomentar esta linha.
        // DontDestroyOnLoad(gameObject);

        // Aplica as configurações
        SetTargetFPS();
    }

    /// <summary>
    /// Aplica as configurações de FPS e VSync.
    /// </summary>
    public void SetTargetFPS()
    {
        // 1. DESATIVA O VSYNC (Sincronização Vertical)
        // Isso é crucial para que o 'Application.targetFrameRate' funcione corretamente.
        // VSync count 0 significa que não há sincronização com a taxa de atualização do monitor.
        QualitySettings.vSyncCount = 0;

        // 2. FIXA O FPS ALVO
        // Isso informa ao Unity a taxa de quadros máxima que ele deve tentar alcançar.
        Application.targetFrameRate = targetFPS;

        Debug.Log($"FPS alvo definido como {targetFPS}. VSync desativado.");
    }

    // Opcional: Se você quiser garantir que as configurações sejam aplicadas
    // mesmo que o jogo volte de uma pausa (Time.timeScale = 0), você pode chamar
    // o SetTargetFPS() em OnEnable ou OnDisable, mas Awake/Start já costuma ser suficiente.
}