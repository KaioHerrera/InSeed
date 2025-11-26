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

public class WinScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winPanel;
    public Button mainMenuButton;
    public Button quitButton;
    public TextMeshProUGUI winMessage;
    
    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu"; // Nome da cena do menu principal
    
    void Start()
    {
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
        
        if (winPanel == null)
        {
            winPanel = GameObject.Find("WinPanel");
        }
        
        if (mainMenuButton == null)
        {
            mainMenuButton = GameObject.Find("MainMenuButton")?.GetComponent<Button>();
        }
        
        if (quitButton == null)
        {
            quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();
        }
        
        if (winMessage == null)
        {
            winMessage = GameObject.Find("WinMessage")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }
        
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }
    
    public void ShowWinScreen()
    {
        Time.timeScale = 0f;
        
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    public void QuitGame()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
