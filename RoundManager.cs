using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    public int totalKills = 0;
    public static RoundManager Instance;

    // --- NOVO: Variáveis para o limite de spawn ativo ---
    [HideInInspector] public int enemiesLeftToSpawn; // Inimigos restantes na fila total da rodada
    [HideInInspector] public int enemiesInScene;     // Inimigos ativos no mapa no momento
    // ---------------------------------------------------

    [Header("Limite de Spawn")]
    [Tooltip("Máximo de inimigos que podem estar ativos no mapa ao mesmo tempo.")]
    public int maxConcurrentEnemies = 25;
    [Header("Inimigos")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    [Header("Estatísticas por Rodada")]
    public int currentRound = 1;

    // ➡️ VARIÁVEL PÚBLICA PARA DEPURAR A VIDA DO ZUMBI
    public float ZombieCurrentHealth;

    public float baseEnemyHealth = 100f;
    public float healthMultiplier = 1.16f;

    // ⭐ Variáveis para o XP que o Zumbi DÁ à ARMA
    [Header("XP da Arma por Zumbi")]
    [Tooltip("XP base que o zumbi concede à arma ao morrer.")]
    public float baseWeaponXP = 25f;
    [Tooltip("Multiplicador para aumentar o XP base da arma a cada rodada (ex: 1.10f para 10%).")]
    public float weaponXpMultiplier = 1.05f;
    [Tooltip("XP MÁXIMA que um zumbi pode conceder à arma, conforme solicitado.")]
    public float maxWeaponXP = 10000f;

    // O XP de Pontos do Player
    public float baseEnemyXP = 50f;
    public float xpMultiplier = 1.15f;

    public int enemiesAlive = 0;
    private bool isSpawning = false;
    private bool isRoundEnding = false;
    private bool isFirstRound = true;

    [Header("Progressão de Loot")]
    [Tooltip("A cada X rounds, o peso de cada item raro será ajustado.")]
    public int roundsPerWeightAdjustment = 5;
    [Tooltip("Ajuste de peso por rodada para itens raros (ex: 5 significa +5 de peso).")]
    public int rareItemWeightIncrease = 2;

    [Header("UI")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI enemiesAliveText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip newRoundClip;

    void Awake()
    {
        // 1. Padrão Singleton: Garante apenas uma instância.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        ResetRoundState();
        StartNewRound();
        UpdateUI();
    }

    public void ResetRoundState()
    {
        currentRound = 1;
        totalKills = 0;
        enemiesAlive = 0;
        enemiesLeftToSpawn = 0; // Reset das variáveis novas
        enemiesInScene = 0;     // Reset das variáveis novas
        isFirstRound = true;
        isSpawning = false;
        isRoundEnding = false;

        CleanupEnemies();
    }


    void Update()
    {
        // Só verifica o fim da rodada quando não estamos spawnando ativamente.
        if (isSpawning || isRoundEnding) return;

        if (enemiesAlive <= 0)
        {
            isRoundEnding = true;
            StartCoroutine(RoundCompletedDelay());
        }
    }

    IEnumerator RoundCompletedDelay()
    {
        UpdateUI();
        yield return new WaitForSeconds(5f);
        currentRound++;
        isRoundEnding = false;
        StartNewRound();
        UpdateUI();
    }

    void StartNewRound()
    {
        if (!isFirstRound)
        {
            CleanupEnemies();
        }

        if (isFirstRound)
        {
            isFirstRound = false;
        }
        else if (audioSource != null && newRoundClip != null)
        {
            audioSource.PlayOneShot(newRoundClip);
        }

        ZombieCurrentHealth = CalculateEnemyHealth();

        int enemiesToSpawn = CalculateEnemyCount();
        StartCoroutine(SpawnEnemies(enemiesToSpawn));
    }

    // --- Métodos de Cálculo ---

    int CalculateEnemyCount()
    {
        const int zombiesAtR10 = 34;
        const int maxZombiesCap = 5000;
        const int maxRound = 255;
        const float exponentialGrowthRate = 1.025f;

        // 1. Fase Inicial (R1 a R5)
        if (currentRound <= 5)
        {
            return 5 + (currentRound * 4);
        }
        // 2. Fase Intermediária (R6 a R10)
        else if (currentRound <= 10)
        {
            return (int)Mathf.Round(22f + (currentRound * 1.2f));
        }
        // 3. Fase Exponencial (R11 a R255+)
        else
        {
            int cappedRound = Mathf.Min(currentRound, maxRound);
            int exponent = cappedRound - 10;

            double exponentialCount = zombiesAtR10 * Mathf.Pow(exponentialGrowthRate, exponent);

            int result = (int)Mathf.Round((float)exponentialCount);

            return Mathf.Min(result, maxZombiesCap);
        }
    }

    float CalculateEnemyHealth()
    {
        float newHealth;

        // FASE 1: R1 a R29 (Crescimento inicial com 1.16f)
        if (currentRound <= 29)
        {
            newHealth = baseEnemyHealth * Mathf.Pow(healthMultiplier, currentRound - 1);
        }
        // FASE 2: R30 a R60 (Progressão acelerada com 1.145f)
        else if (currentRound <= 60)
        {
            const float NEW_MULTIPLIER = 1.145f;

            float baseHealthAtR30Start = baseEnemyHealth * Mathf.Pow(healthMultiplier, 29f);

            int exponent = currentRound - 30;

            newHealth = baseHealthAtR30Start * Mathf.Pow(NEW_MULTIPLIER, exponent);
        }
        // FASE 3: R61 e Acima (Progressão LINEAR)
        else
        {
            const float LINEAR_MULTIPLIER = 1.145f;

            float baseHealthAtR30Start = baseEnemyHealth * Mathf.Pow(healthMultiplier, 29f);

            float healthAtR60 = baseHealthAtR30Start * Mathf.Pow(LINEAR_MULTIPLIER, 30f);

            const float LINEAR_INCREASE_RATE = 0.10f;
            float fixedIncreasePerRound = healthAtR60 * LINEAR_INCREASE_RATE;

            int roundsPastTransition = currentRound - 60;

            newHealth = healthAtR60 + (fixedIncreasePerRound * roundsPastTransition);
        }

        return newHealth;
    }

    private float CalculateWeaponXP()
    {
        float calculatedXP = baseWeaponXP * Mathf.Pow(weaponXpMultiplier, currentRound - 1);

        return Mathf.Min(calculatedXP, maxWeaponXP);
    }

    public float GetWeaponXPForKill()
    {
        return CalculateWeaponXP();
    }


    public int GetAdjustedDropWeight(int baseWeight, bool isRare)
    {
        if (!isRare || currentRound < roundsPerWeightAdjustment)
        {
            return baseWeight;
        }

        int adjustmentBlocks = currentRound / roundsPerWeightAdjustment;

        int weightIncrease = adjustmentBlocks * rareItemWeightIncrease;

        int adjustedWeight = Mathf.Max(baseWeight + weightIncrease, 1);

        return adjustedWeight;
    }


    // 🔄 MÉTODO ATUALIZADO: Inicializa o Spawn e a Fila
    IEnumerator SpawnEnemies(int count)
    {
        isSpawning = true;
        enemiesAlive = count; // Total que precisa morrer
        enemiesLeftToSpawn = count; // Total na fila de spawn
        enemiesInScene = 0; // Quantos estão ativos no mapa
        UpdateUI();

        yield return StartCoroutine(ContinueSpawning());

        isSpawning = false;
    }

    // ⭐ NOVO MÉTODO: Controla o fluxo de spawn para respeitar o limite ativo
    IEnumerator ContinueSpawning()
    {
        float newHealth = ZombieCurrentHealth;
        float newPointXP = baseEnemyXP * Mathf.Pow(xpMultiplier, currentRound - 1);
        newPointXP = Mathf.Min(newPointXP, 2000f);

        // Enquanto houver inimigos para spawnar E o limite de cena não foi atingido
        while (enemiesLeftToSpawn > 0 && enemiesInScene < maxConcurrentEnemies)
        {
            if (spawnPoints.Length == 0 || enemyPrefabs.Length == 0)
            {
                Debug.LogError("Pontos de spawn ou prefabs de inimigos não atribuídos no Inspector!");
                yield break;
            }

            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            GameObject newEnemy = Instantiate(enemyToSpawn, randomSpawnPoint.position, randomSpawnPoint.rotation);

            Enemy enemyScript = newEnemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.SetStats(newHealth, newPointXP);
            }

            enemiesLeftToSpawn--;
            enemiesInScene++; // Incrementa o contador de ativos

            // OTIMIZAÇÃO: Reduz o delay para o próximo frame
            yield return null;
        }
    }

    // 🔄 MÉTODO ATUALIZADO: Decrementa o contador ativo e continua o spawn da fila
    public void EnemyDied()
    {
        enemiesAlive--;
        enemiesInScene--; // Decrementa a contagem de inimigos no mapa
        UpdateUI();
        totalKills++;

        // Tenta continuar o spawn se houver inimigos na fila e espaço no mapa
        if (enemiesLeftToSpawn > 0 && enemiesInScene < maxConcurrentEnemies && !isSpawning)
        {
            // O spawn é reiniciado como uma nova corrotina, mas só vai spawnar o que
            // cabe até o limite ativo.
            StartCoroutine(ContinueSpawning());
        }
    }

    private void UpdateUI()
    {
        if (roundText != null)
        {
            roundText.text = currentRound.ToString();
        }

        if (enemiesAliveText != null)
        {
            // O UI de inimigos deve mostrar o TOTAL restante na rodada (ativos + na fila)
            // enemiesAlive é o total de mortes necessárias
            enemiesAliveText.text = enemiesAlive.ToString();
        }
    }

    private void CleanupEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
    }
}