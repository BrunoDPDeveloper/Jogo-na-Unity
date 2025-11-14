using UnityEngine;

public class Enemy : MonoBehaviour
{
    // --- VARIÁVEIS DE ESTATÍSTICAS ---

    [Header("Estatísticas Base (Round 1)")]
    public float baseHealth = 100f;

    [HideInInspector]
    public float currentHealth;

    [HideInInspector]
    public float xpValue; // XP para o NÍVEL DO JOGADOR (Player XP)

    [Header("Recompensas")]
    public float pointValue = 60f;
    public float headshotPointBonus = 40f;

    // --- VARIÁVEIS DE GERENCIAMENTO ---
    public static int currentActiveEnemies = 0;

    private PlayerXPManager xpManager;

    // --- MÉTODOS UNITY ---

    void Awake()
    {
        currentHealth = baseHealth;
    }

    void Start()
    {
        // ⭐ ATENÇÃO: Dependendo de como você gerencia o XP do Jogador, esta chamada pode mudar.
        xpManager = FindAnyObjectByType<PlayerXPManager>();

        if (xpManager == null)
        {
            Debug.LogError("PlayerXPManager não encontrado na cena! Garanta que ele esteja presente.");
        }

        // Aumenta a contagem global de inimigos ativos.
        currentActiveEnemies++;
    }

    // --- MÉTODOS DE ESCALABILIDADE ---

    /// <summary>
    /// Recebe e aplica as estatísticas escalonadas calculadas pelo RoundManager.
    /// </summary>
    public void SetStats(float newHealth, float newXP)
    {
        currentHealth = newHealth;
        xpValue = newXP;
    }

    // --- MÉTODOS DE COMBATE ---

    /// <summary>
    /// Aplica dano ao inimigo e verifica se ele deve morrer.
    /// </summary>
    public void TakeDamage(float amount, bool isHeadshot = false)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die(isHeadshot); // Passa o flag de headshot para a função Die
        }
    }

    // ⭐ ATUALIZADO: Lógica centralizada de morte.
    /// <summary>
    /// Lida com a morte do inimigo, recompensas, notificação do RoundManager e acionamento da animação.
    /// </summary>
    void Die(bool wasHeadshot = false)
    {
        // Garante que a morte só é processada uma vez
        if (currentHealth > 0f) currentHealth = 0f;

        // 1. Recompensas (XP do Jogador e Pontos)
        if (xpManager != null)
        {
            xpManager.AddXP(xpValue);
        }

        if (PointManager.Instance != null)
        {
            float totalPoints = pointValue;
            if (wasHeadshot)
            {
                totalPoints += headshotPointBonus;
            }
            PointManager.Instance.AddPoints((int)totalPoints);
        }

        // 2. ⭐ RECOMPENSA DE XP DA ARMA (LÓGICA CRÍTICA) ⭐
        // Verifica se o PlayerController e a Arma Ativa existem antes de tentar acessar.
        if (RoundManager.Instance != null &&
            PlayerController.Instance != null &&
            PlayerController.Instance.ActiveWeapon != null)
        {
            // Obtém o XP da arma ajustado para a Rodada Atual (com o cap de 1000)
            float weaponXP = RoundManager.Instance.GetWeaponXPForKill();

            // Concede o XP para a arma atualmente equipada
            PlayerController.Instance.ActiveWeapon.AddGunXP(weaponXP);
        }

        // 3. Notifica o RoundManager e atualiza a contagem de inimigos
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.EnemyDied();
        }

        currentActiveEnemies--;

        // 4. Chama o AI para tocar a animação e DESTRUIR o objeto com delay
        ZombieAI ai = GetComponent<ZombieAI>();
        if (ai != null)
        {
            ai.StartDeathSequence(); // Inicia a animação e a Coroutine que chamará Destroy(gameObject)
        }
        else
        {
            // Fallback: Destrói imediatamente se não houver ZombieAI
            Destroy(gameObject);
        }
    }
}