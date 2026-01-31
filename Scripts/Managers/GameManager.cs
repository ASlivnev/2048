using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu;
    
    [Header("Game Over Menu")]
    [SerializeField] private GameObject gameOverMenu;
    
    private bool isPaused = false;
    private bool isGameOver = false;
    
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Убираем DontDestroyOnLoad чтобы GameManager пересоздавался при рестарте
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        Debug.Log("GameManager: Start() called");
        
        // Гарантируем правильный TimeScale при старте
        Time.timeScale = 1f;
        
        Debug.Log($"GameManager: Initial state - IsPaused: {isPaused}, IsGameOver: {isGameOver}, TimeScale: {Time.timeScale}");
        
        // Скрываем меню паузы при старте
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
            Debug.Log("GameManager: Pause menu hidden on start");
        }
        else
        {
            Debug.LogWarning("GameManager: Pause menu not assigned!");
        }
        
        // Скрываем меню Game Over при старте
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(false);
            Debug.Log("GameManager: Game Over menu hidden on start");
        }
        else
        {
            Debug.LogWarning("GameManager: Game Over menu not assigned!");
        }
        
        Debug.Log("GameManager: Start() completed");
    }
    
    void Update()
    {
        // Проверяем фокус окна (только если не Game Over)
        if (!isPaused && !isGameOver)
        {
            if (Application.isFocused == false)
            {
                PauseGame();
            }
        }
    }
    
    // Публичная функция для постановки на паузу
    public void PauseGame()
    {
        if (isPaused || isGameOver) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        
        // БЛОКИРУЕМ УПРАВЛЕНИЕ - отключаем спаун и управление
        DisableGameControls();
        
        // Показываем меню паузы
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
            Debug.Log("GameManager: Game paused - menu shown, controls disabled");
        }
        else
        {
            Debug.Log("GameManager: Game paused - no menu to show, controls disabled");
        }
    }
    
    // Публичная функция: Снять с паузы
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        
        // ВОЗОБНОВЛЯЕМ УПРАВЛЕНИЕ - включаем спаун и управление
        EnableGameControls();
        
        // Скрываем меню паузы
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
            Debug.Log("GameManager: Game resumed - menu hidden, controls enabled");
        }
        else
        {
            Debug.LogError("GameManager: Pause menu is NULL! Cannot hide pause menu.");
        }
    }
    
    // Публичное свойство для проверки состояния паузы
    public bool IsPaused => isPaused;
    
    public bool IsGameOver => isGameOver;
    
    // БЛОКИРОВКА УПРАВЛЕНИЯ ПРИ ПАУЗЕ
    private void DisableGameControls()
    {
        Debug.Log("GameManager: Disabling game controls");
        
        // Отключаем спаунер кубиков
        if (CubeSpawner.Instance != null)
        {
            CubeSpawner.Instance.enabled = false;
        }
        
        // Отключаем все скрипты управления на кубиках
        DisableCubeControls();
    }
    
    private void EnableGameControls()
    {
        Debug.Log("GameManager: Enabling game controls");
        
        // Включаем спаунер кубиков
        if (CubeSpawner.Instance != null)
        {
            CubeSpawner.Instance.enabled = true;
        }
        
        // Включаем все скрипты управления на кубиках
        EnableCubeControls();
    }
    
    private void DisableCubeControls()
    {
        // Находим все скрипты управления на кубиках и отключаем их
        MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            if (script != null && script.name.Contains("Control") || script.name.Contains("Input") || script.name.Contains("Mouse"))
            {
                script.enabled = false;
            }
        }
    }
    
    private void EnableCubeControls()
    {
        // Находим все скрипты управления на кубиках и включаем их
        MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            if (script != null && script.name.Contains("Control") || script.name.Contains("Input") || script.name.Contains("Mouse"))
            {
                script.enabled = true;
            }
        }
    }
    
    // Публичная функция для вызова Game Over
    public void TriggerGameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        isPaused = false;
        Time.timeScale = 0f;
        
        // Скрываем меню паузы
        if (pauseMenu != null && pauseMenu.activeInHierarchy)
        {
            pauseMenu.SetActive(false);
            Debug.Log("GameManager: Pause menu hidden due to Game Over");
        }
        
        // Показываем меню Game Over
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(true);
            Debug.Log("GameManager: Game Over - menu shown");
        }
        else
        {
            Debug.LogError("GameManager: Game Over menu is NULL! Cannot show Game Over menu.");
        }
        
        Debug.Log("GAME OVER!");
    }
    
    // Публичная функция: Рестарт игры
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
    
    // Публичная функция: Очистить все PlayerPrefs
    public void ClearAllPlayerPrefs()
    {
        Debug.Log("GameManager: Clearing all PlayerPrefs");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("GameManager: All PlayerPrefs cleared successfully");
        RestartGame();
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !isPaused && !isGameOver)
        {
            PauseGame();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isPaused && !isGameOver)
        {
            PauseGame();
        }
    }
}
