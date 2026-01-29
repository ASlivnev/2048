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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Скрываем меню паузы при старте
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameManager: Pause menu not assigned!");
        }
        
        // Скрываем меню Game Over при старте
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameManager: Game Over menu not assigned!");
        }
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
    
    // Приватная функция для постановки на паузу
    void PauseGame()
    {
        if (isPaused || isGameOver) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        
        // Показываем меню паузы
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
            Debug.Log("GameManager: Game paused - menu shown");
        }
        else
        {
            Debug.LogError("GameManager: Pause menu is NULL! Cannot show pause menu.");
        }
    }
    
    // Публичная функция: Снять с паузы
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        
        // Скрываем меню паузы
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
            Debug.Log("GameManager: Game resumed - menu hidden");
        }
        else
        {
            Debug.LogError("GameManager: Pause menu is NULL! Cannot hide pause menu.");
        }
    }
    
    // Публичное свойство для проверки состояния паузы
    public bool IsPaused => isPaused;
    
    // Публичное свойство для проверки состояния Game Over
    public bool IsGameOver => isGameOver;
    
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
        // Просто перезагружаем текущую сцену
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        
        Debug.Log("GameManager: Scene restarted");
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
