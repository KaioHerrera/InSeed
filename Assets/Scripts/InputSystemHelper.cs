using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Helper script para garantir que o Input System funcione corretamente
/// Adicione este componente ao Player se estiver usando o novo Input System
/// </summary>
[RequireComponent(typeof(FirstPersonController))]
public class InputSystemHelper : MonoBehaviour
{
#if ENABLE_INPUT_SYSTEM
    private PlayerInput playerInput;
    private FirstPersonController controller;
    
    void Awake()
    {
        controller = GetComponent<FirstPersonController>();
        
        // Tentar encontrar PlayerInput existente
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput == null)
        {
            // Criar PlayerInput se não existir
            playerInput = gameObject.AddComponent<PlayerInput>();
            
            // Configurar ações básicas
            // Nota: Você precisará criar um Input Actions asset ou usar o padrão
            Debug.LogWarning("PlayerInput criado, mas você precisa configurar as ações de input. " +
                "Crie um Input Actions asset ou use o sistema de input antigo nas Player Settings.");
        }
    }
    
    void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }
    }
    
    void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }
    }
#else
    void Awake()
    {
        // Se não estiver usando Input System, este componente não é necessário
        Destroy(this);
    }
#endif
}
