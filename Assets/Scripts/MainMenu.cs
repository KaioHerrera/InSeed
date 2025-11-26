using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public Button instructionsButton;
    public Button quitButton;
    public GameObject instructionsPanel;
    public Button backButton;
    
    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene"; // Nome da cena do jogo
    
    void Start()
    {
        // Verificar se há EventSystem
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventSystemObj.AddComponent<StandaloneInputModule>();
#endif
        }
        else
        {
            GameObject eventSystemObj = eventSystem.gameObject;
            
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (eventSystemObj.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            {
                StandaloneInputModule oldModule = eventSystemObj.GetComponent<StandaloneInputModule>();
                if (oldModule != null)
                {
#if UNITY_EDITOR
                    DestroyImmediate(oldModule);
#else
                    Destroy(oldModule);
#endif
                }
                eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
#else
            if (eventSystemObj.GetComponent<StandaloneInputModule>() == null)
            {
                var newModule = eventSystemObj.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                if (newModule != null)
                {
#if UNITY_EDITOR
                    DestroyImmediate(newModule);
#else
                    Destroy(newModule);
#endif
                }
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }
#endif
        }
        
        if (playButton == null)
        {
            playButton = GameObject.Find("PlayButton")?.GetComponent<Button>();
        }
        
        if (instructionsButton == null)
        {
            instructionsButton = GameObject.Find("InstructionsButton")?.GetComponent<Button>();
        }
        
        if (quitButton == null)
        {
            quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();
        }
        
        if (backButton == null)
        {
            backButton = GameObject.Find("BackButton")?.GetComponent<Button>();
        }
        
        if (instructionsPanel == null)
        {
            instructionsPanel = GameObject.Find("InstructionsPanel");
        }
        
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(PlayGame);
        }
        
        if (instructionsButton != null)
        {
            instructionsButton.onClick.AddListener(ShowInstructions);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(HideInstructions);
        }
        
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void PlayGame()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("Nome da cena do jogo não configurado!");
            return;
        }
        
        bool sceneExists = false;
        string[] possiblePaths = {
            $"Assets/Scenes/{gameSceneName}.unity",
            $"Assets/{gameSceneName}.unity",
            gameSceneName
        };
        
        foreach (string path in possiblePaths)
        {
            if (SceneUtility.GetBuildIndexByScenePath(path) >= 0)
            {
                sceneExists = true;
                break;
            }
        }
        
        if (!sceneExists)
        {
            Debug.LogError($"Cena '{gameSceneName}' não encontrada no Build Profile!");
            return;
        }
        
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void ShowInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }
    }
    
    public void HideInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
