using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 3f; // Reduzido para pulo mais baixo
    public float gravity = -15f; // Aumentado para cair mais rápido
    
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 0.8f; // Reduzido para sensibilidade mais baixa
    public float upDownRange = 60f;
    
    [Header("Audio Settings")]
    public AudioClip walkingSound;
    public float walkingVolume = 0.5f;
    public float runningPitch = 1.2f; // Pitch mais alto quando correndo
    
    private CharacterController characterController;
    private Camera playerCamera;
    private AudioSource audioSource;
    private Vector3 velocity;
    private float verticalRotation = 0;
    private bool isGrounded;
    private bool isMoving = false;
    
    // Input System variables
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;
    private bool runPressed;
    private bool wasJumping = false; // Para evitar pulo infinito
    
#if ENABLE_INPUT_SYSTEM
    private PlayerInput playerInput;
    private InputActionMap playerActionMap;
#endif
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Configurar AudioSource para som de passos
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.loop = true;
        audioSource.volume = walkingVolume;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.playOnAwake = false;
        
        // Carregar som de passos se não foi atribuído
        if (walkingSound == null)
        {
            walkingSound = Resources.Load<AudioClip>("Sounds/Walking");
            if (walkingSound == null)
            {
                // Tentar carregar diretamente do caminho
#if UNITY_EDITOR
                walkingSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/Walking.wav");
#endif
            }
        }
        
        if (walkingSound != null)
        {
            audioSource.clip = walkingSound;
        }
        else
        {
            Debug.LogWarning("Som de passos não encontrado! Certifique-se de que o arquivo 'Walking.wav' está em Assets/Sounds/");
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
#if ENABLE_INPUT_SYSTEM
        // Tentar obter PlayerInput
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerActionMap = playerInput.actions.FindActionMap("Player");
        }
#endif
    }
    
    void Update()
    {
        // Não processar input se o jogo estiver pausado
        if (Time.timeScale == 0f)
        {
            return;
        }
        
        HandleMouseLook();
        HandleMovement();
    }
    
#if ENABLE_INPUT_SYSTEM
    // Input System callbacks
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
    
    public void OnJump(InputValue value)
    {
        // Só marcar como pressionado no frame que foi pressionado (não manter pressionado)
        if (value.isPressed && !jumpPressed)
        {
            jumpPressed = true;
        }
        else if (!value.isPressed)
        {
            jumpPressed = false;
        }
    }
    
    public void OnRun(InputValue value)
    {
        runPressed = value.isPressed;
    }
#endif
    
    void HandleMouseLook()
    {
        // Não processar movimento da câmera se o jogo estiver pausado ou cursor não estiver bloqueado
        if (Time.timeScale == 0f || Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }
        
        float mouseX, mouseY;
        
#if ENABLE_INPUT_SYSTEM
        // Tentar usar PlayerInput primeiro
        if (playerActionMap != null)
        {
            var lookAction = playerActionMap.FindAction("Look");
            if (lookAction != null)
            {
                Vector2 look = lookAction.ReadValue<Vector2>();
                mouseX = look.x * mouseSensitivity;
                mouseY = look.y * mouseSensitivity;
            }
            else
            {
                mouseX = lookInput.x * mouseSensitivity;
                mouseY = lookInput.y * mouseSensitivity;
            }
        }
        else if (Mouse.current != null)
        {
            // Fallback: usar mouse diretamente
            mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity * 0.1f;
            mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * 0.1f;
        }
        else
        {
            mouseX = lookInput.x * mouseSensitivity;
            mouseY = lookInput.y * mouseSensitivity;
        }
#else
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
#endif
        
        // Rotação horizontal (Y axis)
        transform.Rotate(0, mouseX, 0);
        
        // Rotação vertical (X axis) - limitada
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
    
    void HandleMovement()
    {
        // Verificar se está no chão
        isGrounded = characterController.isGrounded;
        
        // Raycast adicional para garantir detecção de chão (mais preciso)
        if (!isGrounded)
        {
            RaycastHit groundHit;
            Vector3 rayStart = transform.position + characterController.center;
            float rayDistance = characterController.height * 0.5f + 0.3f;
            
            if (Physics.Raycast(rayStart, Vector3.down, out groundHit, rayDistance))
            {
                // Verificar se não está muito longe do chão
                float distanceToGround = Vector3.Distance(rayStart, groundHit.point);
                if (distanceToGround < characterController.height * 0.5f + 0.1f)
                {
                    isGrounded = true;
                }
            }
        }
        
        // Resetar velocidade Y quando no chão
        if (isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
            wasJumping = false; // Resetar flag de pulo quando tocar o chão
        }
        
        // Input de movimento
        float horizontal, vertical;
        bool jump, run;
        
#if ENABLE_INPUT_SYSTEM
        // Tentar usar PlayerInput primeiro
        if (playerActionMap != null)
        {
            var moveAction = playerActionMap.FindAction("Move");
            var jumpAction = playerActionMap.FindAction("Jump");
            var sprintAction = playerActionMap.FindAction("Sprint");
            
            if (moveAction != null)
            {
                Vector2 moveInputValue = moveAction.ReadValue<Vector2>();
                horizontal = moveInputValue.x;
                vertical = moveInputValue.y;
            }
            else
            {
                horizontal = moveInput.x;
                vertical = moveInput.y;
            }
            
            // Só permitir pulo se foi pressionado neste frame
            bool jumpThisFrame = jumpAction != null && jumpAction.WasPressedThisFrame();
            jump = jumpThisFrame || (jumpPressed && !wasJumping);
            run = (sprintAction != null && sprintAction.IsPressed()) || runPressed;
        }
        else if (Keyboard.current != null)
        {
            // Fallback: usar teclado diretamente
            horizontal = (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0);
            vertical = (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0);
            // Só permitir pulo se foi pressionado neste frame
            jump = Keyboard.current.spaceKey.wasPressedThisFrame && !wasJumping;
            run = Keyboard.current.leftShiftKey.isPressed;
        }
        else
        {
            horizontal = moveInput.x;
            vertical = moveInput.y;
            jump = jumpPressed;
            run = runPressed;
        }
#else
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        jump = Input.GetButtonDown("Jump");
        run = Input.GetKey(KeyCode.LeftShift);
#endif
        
        // Calcular direção de movimento
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        
        // Determinar velocidade (correr ou andar)
        float currentSpeed = run ? runSpeed : walkSpeed;
        
        // Verificar se está se movendo
        bool wasMoving = isMoving;
        isMoving = isGrounded && move.magnitude > 0.1f; // Se está no chão e se movendo
        
        // Aplicar movimento
        characterController.Move(move * currentSpeed * Time.deltaTime);
        
        // Gerenciar som de passos
        HandleFootstepSound(isMoving, run);
        
        // Pulo - só permitir se estiver no chão E não estiver pulando E não estava pulando antes
        if (jump && isGrounded && !wasJumping && velocity.y <= 0.1f)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            wasJumping = true; // Marcar que está pulando
        }
        
        // Se não está no chão, está pulando
        if (!isGrounded)
        {
            wasJumping = true;
        }
        
        // Aplicar gravidade
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
        
        // Resetar jumpPressed no final do frame se não foi usado
        if (jump && isGrounded)
        {
            jumpPressed = false;
        }
    }
    
    void HandleFootstepSound(bool moving, bool running)
    {
        if (audioSource == null || walkingSound == null)
        {
            return;
        }
        
        if (moving && isGrounded)
        {
            // Ajustar pitch baseado na velocidade (correr = pitch mais alto)
            audioSource.pitch = running ? runningPitch : 1f;
            
            // Tocar som se não estiver tocando
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            // Parar som se não estiver se movendo
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
