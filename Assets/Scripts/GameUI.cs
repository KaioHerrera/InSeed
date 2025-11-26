using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;
    
    [Header("UI References")]
    public TextMeshProUGUI seedsText;
    public TextMeshProUGUI treesText;
    public TextMeshProUGUI messageText;
    public GameObject winPanel;
    
    private GameManager gameManager;
    private float messageTimer = 0f;
    private float messageDuration = 3f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        gameManager = GameManager.Instance;
        
        if (seedsText == null)
        {
            seedsText = GameObject.Find("SeedsText")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (treesText == null)
        {
            treesText = GameObject.Find("TreesText")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (messageText == null)
        {
            messageText = GameObject.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
            return;
        }
        
        // Atualizar texto de sementes
        if (seedsText != null)
        {
            int collected = gameManager.GetCollectedSeeds();
            int needed = gameManager.seedsNeededToPlant;
            seedsText.text = $"Sementes: {collected}/{needed}";
            
            // Mudar cor se tiver o suficiente
            if (collected >= needed)
            {
                seedsText.color = Color.green;
            }
            else
            {
                seedsText.color = Color.white;
            }
        }
        
        // Atualizar texto de árvores
        if (treesText != null)
        {
            int planted = gameManager.GetPlantedTrees();
            int total = gameManager.totalTreesToPlant;
            treesText.text = $"Árvores: {planted}/{total}";
        }
        
        // Verificar vitória
        if (gameManager.GetPlantedTrees() >= gameManager.totalTreesToPlant && winPanel != null)
        {
            winPanel.SetActive(true);
        }
        
        // Gerenciar mensagens temporárias
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0 && messageText != null)
            {
                messageText.gameObject.SetActive(false);
            }
        }
    }
    
    public void ShowMessage(string message, float duration = 3f)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
            messageTimer = duration;
            messageDuration = duration;
        }
    }
}
