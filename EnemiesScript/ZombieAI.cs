using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic; // Necessário para List<int> e arrays

// CLASSE DE ESTRUTURA DE DADOS PARA O LOOT
[System.Serializable]
public class LootItem
{
    public GameObject componentPrefab;

    // Peso BASE de 0 a 100. (Este será ajustado pelo RoundManager se for 'isRare')
    [Range(0, 100)]
    public int baseDropWeight = 50;

    // NOVO: Indica se este item ganha bônus de peso em rodadas altas
    public bool isRare = false;
}


public class ZombieAI : MonoBehaviour
{
    [Header("Referências")]
    private Transform player;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private AudioSource audioSource;
    private PlayerHealth playerHealth;
    // ❌ REMOVIDO: public GameObject tierComponent;

    [Header("Configuração de Ataque")]
    public float attackDistance = 2.0f;
    public float attackDamage = 50f;
    public Collider rightHandCollider;
    public Collider leftHandCollider;

    [Header("Comportamento e Velocidades")]
    public float walkSpeed = 0.5f;
    public float runSpeed = 5.0f;
    public int roundToStartRunning = 5;
    public float maxRunnerPercentage = 0.8f;

    [Header("Drop de Itens")] // NOVO HEADER PARA DROPS
    public LootItem[] possibleLoot; // Lista de itens que o zumbi pode dropar
    [Range(0, 1)]
    public float baseDropChance = 0.5f; // Chance BASE de drop (ex: 50% de chance de dropar ALGO)


    [Header("Sons do Zumbi")]
    public AudioClip[] walkingSounds; // Sons de "gemido" aleatórios (Caminhada)
    public AudioClip[] runningSounds; // Sons de "correndo/mais raivoso"
    public AudioClip deathSound;
    public float minTimeBetweenIdleSounds = 5f;
    public float maxTimeBetweenIdleSounds = 15f;

    [Header("Estado do Zumbi")]
    public bool isWalking = true;
    public bool isRunning = false;
    public bool isAttacking = false;
    public bool isDead = false;
    private bool isRunnerZombie = false;

    private float nextIdleSoundTime;
    private bool hasDealtDamageInCurrentAttack = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        if (rightHandCollider != null) rightHandCollider.enabled = false;
        if (leftHandCollider != null) leftHandCollider.enabled = false;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogWarning("Nenhum objeto com a tag 'Player' foi encontrado na cena!");
        }

        SetNextIdleSoundTime();
        DetermineIfRunner();

        bool isRunningRound = RoundManager.Instance != null && RoundManager.Instance.currentRound >= roundToStartRunning;

        if (isRunnerZombie && isRunningRound)
        {
            // Zumbi Corredor (Inicia Correndo)
            navMeshAgent.speed = runSpeed;
            isRunning = true;
            isWalking = false;
        }
        else
        {
            // Zumbi Padrão (Inicia Caminhando)
            navMeshAgent.speed = walkSpeed;
            isRunning = false;
            isWalking = true;
        }

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
    }

    /// <summary>
    /// Determina se este zumbi em particular será um 'runner' com base na rodada atual e na probabilidade.
    /// </summary>
    void DetermineIfRunner()
    {
        if (RoundManager.Instance == null)
        {
            isRunnerZombie = false;
            return;
        }

        int currentRound = RoundManager.Instance.currentRound;

        if (currentRound < roundToStartRunning)
        {
            isRunnerZombie = false;
            return;
        }

        const int MAX_ROUND_FOR_FULL_PERCENTAGE = 50;
        float roundSpan = MAX_ROUND_FOR_FULL_PERCENTAGE - roundToStartRunning;
        float roundsPassed = currentRound - roundToStartRunning;
        float rawChance = roundsPassed / roundSpan;
        float finalChance = Mathf.Min(rawChance, 1f) * maxRunnerPercentage;

        if (currentRound == roundToStartRunning)
        {
            finalChance = Mathf.Max(finalChance, 0.1f);
        }

        if (Random.value < finalChance)
        {
            isRunnerZombie = true;
        }
        else
        {
            isRunnerZombie = false;
        }
    }

    void Update()
    {
        if (player == null || isDead) return;

        // Lógica de velocidade
        bool isRunningRound = RoundManager.Instance != null && RoundManager.Instance.currentRound >= roundToStartRunning;

        // LÓGICA DE ATUALIZAÇÃO DE ESTADO DE MOVIMENTO
        bool previousIsRunning = isRunning; // Salva o estado anterior

        if (isRunnerZombie && isRunningRound)
        {
            navMeshAgent.speed = runSpeed;
            isRunning = true;
            isWalking = false;
        }
        else
        {
            navMeshAgent.speed = walkSpeed;
            isRunning = false;
            isWalking = true;
        }

        // Se o estado de corrida mudou E o zumbi não está atacando, atualiza imediatamente o som.
        if (previousIsRunning != isRunning && !isAttacking)
        {
            PlayRandomMovementSound();
            SetNextIdleSoundTime();
        }
        // FIM DA LÓGICA DE ATUALIZAÇÃO DE ESTADO

        // Sons
        if (Time.time >= nextIdleSoundTime)
        {
            PlayRandomMovementSound();
            SetNextIdleSoundTime();
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Lógica de ataque e perseguição
        if (distanceToPlayer <= attackDistance)
        {
            AttackPlayer();
        }
        else
        {
            isAttacking = false;
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(player.position);
        }

        // Rotação suave em direção à direção de movimento do NavMeshAgent ou para o jogador
        if (!isAttacking && navMeshAgent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(navMeshAgent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else if (isAttacking)
        {
            // Rotaciona para o jogador durante o ataque
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // Atualiza os parâmetros do Animator
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isAttacking", isAttacking);
    }

    // --- Funções Chamadas por Animation Events (Mão no Player) ---

    public void EnableAttack()
    {
        Debug.Log("EVENTO: EnableAttack chamado. Colliders devem estar ativos.");

        hasDealtDamageInCurrentAttack = false;
        if (rightHandCollider != null) rightHandCollider.enabled = true;
        if (leftHandCollider != null) leftHandCollider.enabled = true;
    }

    public void DisableAttack()
    {
        if (rightHandCollider != null) rightHandCollider.enabled = false;
        if (leftHandCollider != null) leftHandCollider.enabled = false;
    }

    public void DealDamage()
    {
        if (playerHealth == null || hasDealtDamageInCurrentAttack)
        {
            if (playerHealth == null) Debug.LogWarning("DealDamage abortado: playerHealth é nulo!");
            if (hasDealtDamageInCurrentAttack) Debug.Log("DealDamage abortado: Dano já aplicado neste ataque.");
            return;
        }

        float finalDamage = attackDamage;

        // LÓGICA DE REDUÇÃO DE DANOS POR PERK
        if (playerHealth.hasDamageReductionPerk)
        {
            finalDamage = 25f;
            Debug.Log("Dano Reduzido! O zumbi causou " + finalDamage + " de dano.");
        }

        playerHealth.TakeDamage(finalDamage);

        Debug.Log("SUCESSO: Dano de " + finalDamage + " aplicado ao Player.");

        hasDealtDamageInCurrentAttack = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TRIGGER DETECTADO! Objeto colidido: " + other.gameObject.name);

        bool isColliderActive = (rightHandCollider != null && rightHandCollider.enabled) ||
                                (leftHandCollider != null && leftHandCollider.enabled);

        if (other.GetComponent<PlayerHealth>() != null && isColliderActive)
        {
            Debug.Log("TRIGER VÁLIDO. Chamando DealDamage().");
            DealDamage();
        }
        else
        {
            if (other.GetComponent<PlayerHealth>() == null)
            {
                Debug.Log("Colisão inválida: O objeto colidido (" + other.gameObject.name + ") não tem PlayerHealth.");
            }
            if (!isColliderActive)
            {
                Debug.Log("Colisão inválida: Collider da mão desativado (ataque não está ativo).");
            }
        }
    }

    void AttackPlayer()
    {
        isAttacking = true;
        isWalking = false;
        isRunning = false;
        navMeshAgent.isStopped = true;
    }

    /// <summary>
    /// Inicia a sequência de morte (animação, som) e chama a coroutine para aguardar a destruição.
    /// Chamado por Enemy.Die().
    /// </summary>
    public void StartDeathSequence()
    {
        if (isDead) return;
        isDead = true; // Mantém a variável de estado do script para travar o Update.

        // 1. Configurações Imediatas
        isWalking = false;
        isRunning = false;
        isAttacking = false;
        if (navMeshAgent != null) navMeshAgent.isStopped = true;

        DisableAttack(); // Desativa os colliders de ataque

        // ⭐ DROP DO ITEM: Chama antes de destruir os componentes físicos.
        DropTierComponent();

        // =========================================================================
        // DESTRÓI COMPONENTES FÍSICOS
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }

        Collider mainCollider = GetComponent<CapsuleCollider>(); // Assume que o collider principal é Capsule
        if (mainCollider == null)
        {
            // Tenta encontrar um Collider genérico se não for Capsule
            mainCollider = GetComponent<Collider>();
        }
        if (mainCollider != null)
        {
            Destroy(mainCollider);
        }

        if (navMeshAgent != null)
        {
            Destroy(navMeshAgent); // Já paramos ele, mas destruí-lo economiza recursos
        }
        // =========================================================================

        // 2. Animação e Som
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // ⭐ CORREÇÃO CRÍTICA: Usando SetTrigger para transição instantânea e única
        animator.SetTrigger("DieTrigger"); // *CERTIFIQUE-SE DE QUE ESTE PARAMETRO EXISTE NO ANIMATOR*

        // 3. Inicia o delay para a destruição
        StartCoroutine(DeathCoroutine(3.333f));
    }

    /// <summary>
    /// Coroutine para esperar a duração da animação de morte antes de destruir.
    /// </summary>
    private IEnumerator DeathCoroutine(float delay)
    {
        Debug.Log("Zumbi Morreu! Esperando " + delay + "s pela animação...");
        yield return new WaitForSeconds(delay);

        // 4. Destrói o objeto
        Destroy(gameObject);
    }

    // --- Funções de som ---
    void SetNextIdleSoundTime()
    {
        nextIdleSoundTime = Time.time + Random.Range(minTimeBetweenIdleSounds, maxTimeBetweenIdleSounds);
    }

    /// <summary>
    /// Toca um som aleatório baseado no estado de movimento (Walking ou Running).
    /// </summary>
    void PlayRandomMovementSound()
    {
        if (audioSource == null || isDead) return;

        AudioClip[] soundSet = null;

        if (isRunning && runningSounds.Length > 0)
        {
            // Zumbi está correndo: usa sons de corrida
            soundSet = runningSounds;
        }
        else if (isWalking && walkingSounds.Length > 0)
        {
            // Zumbi está caminhando: usa sons de caminhada (antigos Idle Sounds)
            soundSet = walkingSounds;
        }

        if (soundSet != null && soundSet.Length > 0)
        {
            int randomIndex = Random.Range(0, soundSet.Length);
            audioSource.PlayOneShot(soundSet[randomIndex]);
        }
    }

    // ⭐ MÉTODO ATUALIZADO: Implementa a progressão de loot por rodada
    void DropTierComponent()
    {
        // Verificação de segurança
        if (RoundManager.Instance == null || possibleLoot.Length == 0) return;

        // 1. Chance BASE de Drop
        if (Random.value > baseDropChance)
        {
            Debug.Log("Nenhum componente dropado desta vez (Chance Base falhou).");
            return;
        }

        // 2. Cálculo do Peso Total Ajustado
        int totalAdjustedWeight = 0;

        // Usamos uma lista temporária para armazenar os pesos ajustados
        List<int> adjustedWeights = new List<int>();

        foreach (var item in possibleLoot)
        {
            // ⭐ CHAMA O MÉTODO DO ROUNDMANAGER PARA OBTER O PESO AJUSTADO
            int weight = RoundManager.Instance.GetAdjustedDropWeight(item.baseDropWeight, item.isRare);

            adjustedWeights.Add(weight);
            totalAdjustedWeight += weight;
        }

        // Se o peso total for 0, não dropar.
        if (totalAdjustedWeight <= 0) return;

        // 3. Seleção do Item (Lógica da Roleta)
        int randomValue = Random.Range(0, totalAdjustedWeight);
        GameObject componentToDrop = null;
        int currentWeight = 0;

        // Itera sobre os pesos ajustados para encontrar o item
        for (int i = 0; i < possibleLoot.Length; i++)
        {
            currentWeight += adjustedWeights[i];
            if (randomValue < currentWeight)
            {
                componentToDrop = possibleLoot[i].componentPrefab;
                break;
            }
        }

        if (componentToDrop == null) return;

        // 4. Instanciação e Física
        Vector3 position = transform.position;

        // Adiciona um pequeno offset vertical para garantir que não comece colidindo com o chão.
        GameObject goTierComponent = Instantiate(componentToDrop, position + new Vector3(0, 0.25f, 0), Quaternion.identity);
        goTierComponent.SetActive(true);

        Debug.Log($"Dropado: {componentToDrop.name} na Rodada {RoundManager.Instance.currentRound}.");

        // Destrói o item após um longo tempo
        Destroy(goTierComponent, 60f);
    }
}