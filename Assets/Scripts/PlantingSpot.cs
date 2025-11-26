using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlantingSpot : MonoBehaviour
{
    [Header("Planting Spot Settings")]
    public GameObject treePrefab;
    public bool isPlanted = false;
    public float interactionRange = 2f;
    
    private GameObject plantedTree;
    private Renderer markerRenderer;
    private Material originalMaterial;
    
    void Start()
    {
        // Encontrar o renderer do marcador (pode ser um filho ou o próprio objeto)
        markerRenderer = GetComponent<Renderer>();
        if (markerRenderer == null)
        {
            markerRenderer = GetComponentInChildren<Renderer>();
        }
        
        if (markerRenderer != null)
        {
            originalMaterial = markerRenderer.material;
        }
    }
    
    void Update()
    {
        // Verificar se o jogador está próximo e pode plantar
        if (!isPlanted && GameManager.Instance != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerPosition());
            
            if (distanceToPlayer <= interactionRange)
            {
                if (GameManager.Instance.CanPlant())
                {
                    // Destacar o marcador quando pode plantar
                    if (markerRenderer != null)
                    {
                        markerRenderer.material.color = Color.green;
                    }
                    
                    // Mostrar mensagem na UI
                    if (GameUI.Instance != null)
                    {
                        GameUI.Instance.ShowMessage("Pressione E para plantar uma árvore!", 0.1f);
                    }
                    
                    // Verificar input para plantar
                    bool plantInput = false;
#if ENABLE_INPUT_SYSTEM
                    plantInput = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
                    plantInput = Input.GetKeyDown(KeyCode.E);
#endif
                    if (plantInput)
                    {
                        PlantTree();
                    }
                }
                else
                {
                    // Manter cor original quando não pode plantar (não ficar amarelo)
                    if (markerRenderer != null && originalMaterial != null)
                    {
                        markerRenderer.material.color = originalMaterial.color;
                    }
                    
                    // Mostrar mensagem apenas se tiver algumas sementes
                    int collected = GameManager.Instance.GetCollectedSeeds();
                    if (collected > 0)
                    {
                        int needed = GameManager.Instance.seedsNeededToPlant;
                        if (GameUI.Instance != null)
                        {
                            GameUI.Instance.ShowMessage($"Você precisa de {needed} sementes para plantar! ({collected}/{needed})", 0.1f);
                        }
                    }
                }
            }
            else
            {
                // Voltar cor original quando longe
                if (markerRenderer != null && originalMaterial != null)
                {
                    markerRenderer.material.color = originalMaterial.color;
                }
            }
        }
    }
    
    public void PlantTree()
    {
        if (!isPlanted && GameManager.Instance.CanPlant())
        {
            isPlanted = true;
            GameManager.Instance.PlantTree();
            
            // Instanciar árvore
            if (treePrefab != null)
            {
                plantedTree = Instantiate(treePrefab, transform.position, Quaternion.identity);
                plantedTree.transform.SetParent(transform);
            }
            
            // Esconder ou desabilitar o marcador
            if (markerRenderer != null)
            {
                markerRenderer.enabled = false;
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualizar range de interação
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
