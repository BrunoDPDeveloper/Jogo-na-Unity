using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ⭐ NOVO: Permite acesso global fácil.
    public static PlayerController Instance;

    [Header("References")]
    private CharacterController controller;
    [SerializeField] private Transform cameraHolder; // assign CameraPivot (the parent of the camera)

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float sprintTransitSpeed = 5f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpHeight = 0.7f;

    // ⭐ NOVO ESSENCIAL: Propriedade pública para a arma ativa (o Enemy.cs precisa disso)
    // Converte a referência interna (MonoBehaviour) para o tipo específico 'AssaultRifle' 
    // ou retorna null se não for compatível ou se não houver arma.
    public AssaultRifle ActiveWeapon => activeWeaponComponent as AssaultRifle;

    // ⭐ NOVO: Penalidade de velocidade da arma ativa.
    private float currentMovePenalty = 0f;
    // ⭐ NOVO: Referência interna à arma ativa.
    private MonoBehaviour activeWeaponComponent;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity = 350f; // tweak this (100-400 typical)
    [SerializeField] private bool invertY = false;

    [Header("Stamina Settings")]
    // ⭐ NOVO: Variável para guardar o valor original da Stamina Máxima.
    [SerializeField] private float baseMaxStamina = 100f;
    [SerializeField] public float maxStamina = 100f; // Agora pode ser alterada por perks
    [SerializeField] private float currentStamina;
    [SerializeField] private float staminaDrainRate = 15f; // Taxa de dreno por segundo
    [SerializeField] private float staminaRegenRate = 10f; // Taxa de regeneração por segundo
    [SerializeField] private float staminaRegenDelay = 1f; // Tempo de espera para regenerar após parar de correr
    private float lastSprintTime; // Armazena o último momento em que o jogador estava correndo


    private float verticalVelocity = 0f;
    private float currentSpeed = 0f;
    private float cameraPitch = 0f; // rotation X (up/down)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // ⭐ Garante que o valor base seja o valor inicial (se não for setado no Inspector)
        if (baseMaxStamina == 0f) baseMaxStamina = maxStamina;
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        // ✅ CORREÇÃO CS0117
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Inicializa a stamina
        currentStamina = maxStamina;

        if (GameSettings.Instance != null)
        {
            float savedSens = GameSettings.Instance.GetSavedSensitivityValue();
            SetMouseSensitivity(savedSens); // Aplica a sensibilidade salva
        }
    }

    public void SetMaxStamina(float newMaxStamina)
    {
        float oldMaxStamina = maxStamina;
        maxStamina = newMaxStamina;

        // Ao mudar o máximo, é uma boa prática manter a proporção da stamina atual
        if (oldMaxStamina > 0 && currentStamina > 0)
        {
            float ratio = currentStamina / oldMaxStamina;
            currentStamina = maxStamina * ratio;
        }
        else // Se a stamina atual for 0, ela continua 0
        {
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }

        Debug.Log($"Stamina Máxima atualizada para: {maxStamina}");
    }

    // ⭐ NOVO MÉTODO: Reseta a stamina máxima para o valor base
    public void ResetMaxStamina()
    {
        SetMaxStamina(baseMaxStamina);
        // Opcional: encher a stamina após o reset
        currentStamina = maxStamina;
    }

    private void Update()
    {
        // Se o jogo estiver pausado, pare o movimento e mostre o cursor
        if (PauseMenu.isPaused || PlayerHealth.isDead)
        {
            // ✅ CORREÇÃO CS0117
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return; // Interrompe o restante do método Update
        }

        // Se o jogo não estiver pausado, garanta que o cursor está travado e invisível
        // ✅ CORREÇÃO CS0117
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        HandleMouse();
        HandleMovement();
        HandleStamina();
    }

    private void HandleMouse()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mx);

        cameraPitch += invertY ? my : -my;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }


    private void HandleMovement()
    {
        float inputZ = Input.GetAxis("Vertical");
        float inputX = Input.GetAxis("Horizontal");
        bool isMoving = inputZ != 0 || inputX != 0;


        // ⭐ NOVO: Calcula a velocidade base, subtraindo a penalidade da arma
        float effectiveMoveSpeed = Mathf.Max(1.0f, moveSpeed - currentMovePenalty);

        // A velocidade de sprint agora depende da stamina
        bool canSprint = currentStamina > 0 && isMoving;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && canSprint;

        float targetMultiplier = isSprinting ? sprintSpeedMultiplier : 1f;

        // Usa a velocidade efetiva no Lerp
        currentSpeed = Mathf.Lerp(currentSpeed, effectiveMoveSpeed * targetMultiplier, sprintTransitSpeed * Time.deltaTime);

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        Vector3 move = (forward * inputZ + right * inputX);
        if (move.sqrMagnitude > 1f) move.Normalize();

        Vector3 velocity = move * currentSpeed;

        verticalVelocity = VerticalForceCalculation();
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private float VerticalForceCalculation()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // mantem no chão
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        return verticalVelocity;
    }

    public void SetMouseSensitivity(float newSensitivity)
    {
        // Garante que a sensibilidade não seja zero ou negativa
        mouseSensitivity = Mathf.Max(0.1f, newSensitivity);
        Debug.Log("Mouse Sensitivity set to: " + mouseSensitivity);
    }

    public float GetMouseSensitivity()
    {
        return mouseSensitivity;
    }

    private void HandleStamina()
    {
        // Verifica se o jogador está correndo
        bool isMoving = Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving;

        if (isSprinting && currentStamina > 0)
        {
            // Drena a stamina enquanto corre
            currentStamina -= staminaDrainRate * Time.deltaTime;
            lastSprintTime = Time.time; // Reseta o contador
        }
        else if (!isSprinting && Time.time - lastSprintTime >= staminaRegenDelay)
        {
            // Regenera a stamina após um pequeno atraso
            currentStamina += staminaRegenRate * Time.deltaTime;
        }

        // Garante que a stamina não passe dos limites
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

    }

    public void UpdateActiveWeaponPenalty(MonoBehaviour weaponComponent)
    {
        float newPenalty = 0f;

        if (weaponComponent is AssaultRifle ar)
        {
            newPenalty = ar.GetMoveSpeedPenalty();
        }

        else if (weaponComponent is Pistol pistol)
        {
            newPenalty = pistol.GetMoveSpeedPenalty();
        }

        else if (weaponComponent is Shotgun shotgun)
        {
            newPenalty = shotgun.GetMoveSpeedPenalty();
        }

        else if (weaponComponent is Sniper sniper)
        {
            newPenalty = sniper.GetMoveSpeedPenalty();
        }

        else if (weaponComponent is ArmaDeRajada armaDeRajada)
        {
            newPenalty = armaDeRajada.GetMoveSpeedPenalty();
        }

        else
        {
            var penaltyField = weaponComponent.GetType().GetField("moveSpeedPenalty");
            if (penaltyField != null && penaltyField.FieldType == typeof(float))
            {
                newPenalty = (float)penaltyField.GetValue(weaponComponent);
            }
        }

        currentMovePenalty = newPenalty;
        activeWeaponComponent = weaponComponent;

        Debug.Log($"Penalidade de movimento atualizada: -{currentMovePenalty} (Arma: {weaponComponent.GetType().Name})");
    }

    public void ClearActiveWeaponPenalty()
    {
        currentMovePenalty = 0f;
        activeWeaponComponent = null;
        Debug.Log("Penalidade de movimento removida (sem arma ativa).");
    }
}