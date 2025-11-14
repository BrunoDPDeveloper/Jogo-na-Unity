using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static bool isDead = false;
    public float health = 150f;
    //public TextMeshProUGUI playerHealthText;
    public float maxHealth = 150f;
    public float regenerationRate = 10f;
    public float regenDelay = 0.5f;

    private float lastDamageTime;
    private bool isRegenerating = false;

    // Máquinas
    public bool hasDamageReductionPerk = false; // Estado do perk Aço Sólido

    // ⭐ ATUALIZADO: Variável para contar as CARGAS TOTAIS de Quick Revive
    [Header("Perk Quick Revive (Cargas Totais)")]
    // Inicia com 0. Será setado como MAX_REVIVE_USES na primeira compra.
    public int quickReviveTotalUses = 0;

    // ⭐ NOVO: Constante para o número máximo de usos (ex: 4)
    public const int MAX_REVIVE_USES = 4;
    private const string NANITE_PERK_NAME = "Ressureição Nanita"; // Nome do perk na UI

    [Header("Overlay de dano")]
    public Image screenDamageOverlay;
    public float overlayAlpha = 0.6f;

    // ⭐ Referência à máquina Nanite
    private NaniteRessurection naniteMachine;

    private void Start()
    {
        isDead = false;

        // Garante que o overlay começa invisível
        if (screenDamageOverlay != null)
        {
            Color color = screenDamageOverlay.color;
            color.a = 0f;
            screenDamageOverlay.color = color;
        }

        // Encontra a máquina Nanite no Start
        naniteMachine = FindAnyObjectByType<NaniteRessurection>();
    }

    void Update()
    {
        // Só tenta iniciar a regeneração se o jogador não estiver se curando
        if (!isRegenerating && health < maxHealth && Time.time >= lastDamageTime + regenDelay)
        {
            StartCoroutine(RegenerateHealth());
        }
    }

    public void TakeDamage(float amount)
    {
        // Aplicação da redução de dano (se hasDamageReductionPerk for true)
        if (hasDamageReductionPerk)
        {
            amount *= 0.7f;
        }

        health -= amount;
        Debug.Log("Vida do jogador: " + health);
        lastDamageTime = Time.time;
        //UpdatePlayerHealthText();

        StopAllCoroutines();
        isRegenerating = false;

        if (health <= 0)
        {
            Die();
        }

        UpdateOverlay();
    }

    // ⭐ ATUALIZADO: Chamado pela máquina Nanite quando o perk é comprado.
    public void GrantQuickRevivePerk()
    {
        // Se for a primeira compra, define o número total de usos
        if (quickReviveTotalUses == 0)
        {
            quickReviveTotalUses = MAX_REVIVE_USES;
        }

        // Ativa o ícone do perk na UI (o perk está ativo NESTA vida)
        if (PerkUIManager.Instance != null)
        {
            PerkUIManager.Instance.AddPerkIcon(NANITE_PERK_NAME);
        }
    }

    // ⭐ ATUALIZADO: Método que lida com o revive, remoção de todos os perks e reset da máquina
    public void Revive(float reviveHealth)
    {
        Debug.Log("O jogador foi revivido por Nanite! Todos os perks e a própria máquina Nanite foram resetados para recompra.");

        // Lógica de Revive
        health = reviveHealth;
        isDead = false;
        UpdateOverlay();

        // ❌ REMOÇÃO E RESET DE TODOS OS PERKS E MÁQUINAS

        // 1. Remove Aço Sólido (Damage Reduction)
        hasDamageReductionPerk = false;
        if (PerkUIManager.Instance != null)
        {
            PerkUIManager.Instance.RemovePerkIcon("Aço Sólido");
        }
        // Reseta a máquina de Aço Sólido para recompra
        DamageReductionMachine damageMachine = FindAnyObjectByType<DamageReductionMachine>();
        if (damageMachine != null)
        {
            // **IMPORTANTE**: Você precisa implementar este método na sua máquina.
            damageMachine.ResetMachineState();
        }

        // 2. Remove Super Stamina
        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            // Reseta a stamina máxima para o valor base
            playerController.ResetMaxStamina();

            if (PerkUIManager.Instance != null)
            {
                PerkUIManager.Instance.RemovePerkIcon("Super Stamina");
            }
        }
        // Reseta a máquina de Super Stamina para recompra
        SuperStaminaPerkMachine staminaMachine = FindAnyObjectByType<SuperStaminaPerkMachine>();
        if (staminaMachine != null)
        {
            // **IMPORTANTE**: Você precisa implementar este método na sua máquina.
            staminaMachine.ResetMachineState();
        }

        // 3. Remove o ÍCONE do Quick Revive e RESETA o estado da máquina Nanite para Recompra
        if (PerkUIManager.Instance != null)
        {
            PerkUIManager.Instance.RemovePerkIcon(NANITE_PERK_NAME);
        }

        if (naniteMachine != null)
        {
            // Notifica a máquina Nanite sobre o revive e a quantidade de usos restantes.
            naniteMachine.ResetMachineState(quickReviveTotalUses);
        }
    }

    void UpdateOverlay()
    {
        if (isDead || screenDamageOverlay == null) return;

        Color color = screenDamageOverlay.color;

        if (health >= 150f)
        {
            // Vida cheia → overlay invisível
            color.a = 0f;
        }
        else if (health <= 50f)
        {
            // Vida muito baixa → overlay máximo
            color.a = overlayAlpha;
        }
        else if (health < 150f && health > 50f)
        {
            // Entre 50 e 150 → escala proporcional
            float t = (health - 50f) / 100f; // varia de 0 (50 HP) até 1 (150 HP)
            color.a = Mathf.Lerp(overlayAlpha, 0f, t);
        }

        screenDamageOverlay.color = color;
    }

    private void DisableOverlay()
    {
        if (screenDamageOverlay != null)
        {
            Color color = screenDamageOverlay.color;
            color.a = 0f;
            screenDamageOverlay.color = color;
        }
    }

    private IEnumerator RegenerateHealth()
    {
        isRegenerating = true;
        while (health < maxHealth)
        {
            health += 5f; // quanto vai curar por tick
            health = Mathf.Min(health, maxHealth);
            UpdateOverlay(); // atualiza o overlay também durante a cura

            yield return new WaitForSeconds(0.1f); // intervalo entre ticks
        }
        isRegenerating = false;
    }

    private void Die()
    {
        // ⭐ ATUALIZADO: Verifica se o Nanite está ativo (ícone na UI) E se tem usos restantes
        // Assumimos que PerkUIManager.Instance.HasPerk(NANITE_PERK_NAME) verifica se o perk está ativo NESTA vida.
        bool isReviveActive = PerkUIManager.Instance != null && PerkUIManager.Instance.HasPerk(NANITE_PERK_NAME);

        if (isReviveActive && quickReviveTotalUses > 0)
        {
            quickReviveTotalUses--; // Consome uma carga TOTAL

            Debug.Log($"Quick Revive usado. Usos totais restantes: {quickReviveTotalUses}");

            // O revive reseta TUDO, incluindo o próprio Nanite (forçando recompra)
            Revive(maxHealth * 1f); // Revive com 100% da vida máxima

            return; // Sai da função para evitar a tela de Game Over
        }

        // Morte Final (Perk não ativo ou usos esgotados)
        Debug.Log("O jogador morreu!");
        isDead = true;
        DisableOverlay(); // Chama a função para desativar o overlay de dano

        // Você precisa de uma forma de acessar as informações globais
        // Exemplo: Usando a referência de Singletons
        int finalRound = 0;
        int finalKills = 0;
        int finalLevel = 0;
        int finalPoints = 0;

        // Acessa as informações de outros scripts globais
        if (RoundManager.Instance != null)
        {
            finalRound = RoundManager.Instance.currentRound;
            // Assumindo que você tem uma variável totalKills no RoundManager
            // finalKills = RoundManager.Instance.totalKills; 
        }

        if (PlayerXPManager.Instance != null)
        {
            // finalLevel = XPDataManager.Instance.EffectiveLevel; // Assumindo a classe XPDataManager
        }

        if (PointManager.Instance != null) // Se você já implementou o PointManager
        {
            finalPoints = PointManager.Instance.currentPoints;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (GameOverManager.Instance != null)
        {
            // GameOverManager.Instance.ShowGameOverScreen(finalRound, finalKills, finalLevel, finalPoints);
        }
    }
}