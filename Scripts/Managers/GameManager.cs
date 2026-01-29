using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu;
    
    private bool isPaused = false;
    
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
    }
    
    void Update()
    {
        // Проверяем фокус окна
        if (!isPaused)
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
        if (isPaused) return;
        
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
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !isPaused)
        {
            PauseGame();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isPaused)
        {
            PauseGame();
        }
    }
}
