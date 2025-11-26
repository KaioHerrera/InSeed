using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu"; // Nome da cena do menu principal
    public KeyCode pauseKey = KeyCode.Escape;
    
    private bool isPaused = false;
    private FirstPersonController playerController;
    
    void Start()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObj.AddComponent<StandaloneInputModule>();
#endif
        }
        else
        {
            GameObject eventSystemObj = eventSystem.gameObject;
            
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (eventSystemObj.GetComponent<InputSystemUIInputModule>() == null)
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
                eventSystemObj.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (eventSystemObj.GetComponent<StandaloneInputModule>() == null)
            {
                var newModule = eventSystemObj.GetComponent<InputSystemUIInputModule>();
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
        
        if (pausePanel == null)
        {
            pausePanel = GameObject.Find("PausePanel");
        }
        
        if (resumeButton == null)
        {
            resumeButton = GameObject.Find("ResumeButton")?.GetComponent<Button>();
        }
        
        if (mainMenuButton == null)
        {
            mainMenuButton = GameObject.Find("MainMenuButton")?.GetComponent<Button>();
        }
        
        if (quitButton == null)
        {
            quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();
        }
        
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
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
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        playerController = FindFirstObjectByType<FirstPersonController>();
    }
    
    void Update()
    {
        // Verificar input de pause
        bool pauseInput = false;
        
#if ENABLE_INPUT_SYSTEM
        pauseInput = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        pauseInput = Input.GetKeyDown(pauseKey);
#endif
        
        if (pauseInput)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
