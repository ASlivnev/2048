using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Over Bar")]
    [SerializeField] private SpriteRenderer gameOverBar;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private float dangerTime = 3f;
    
    [Header("Game Over Settings")]
    [SerializeField] private bool isGameOver = false;
    
    private float currentContactTime = 0f;
    private bool isTouchingBar = false;
    private int touchingCubesCount = 0;
    private bool isVortexActive = false; // Флаг активного вихря
    
    // Событие Game Over
    public static event Action OnGameOver;
    
    public static GameOverManager Instance { get; private set; }
    
    public static bool IsGameOver => Instance != null ? Instance.isGameOver : false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Устанавливаем начальный цвет полосы
        if (gameOverBar != null)
        {
            gameOverBar.color = normalColor;
        }
        
        Debug.Log("GameOverManager: Initialized");
    }
    
    void Update()
    {
        if (isGameOver) return;
        
        // Управляем видимостью полосы в зависимости от вихря
        if (gameOverBar != null)
        {
            if (isVortexActive)
            {
                // Скрываем полосу при активном вихре
                if (gameOverBar.gameObject.activeInHierarchy)
                {
                    gameOverBar.gameObject.SetActive(false);
                }
            }
            else
            {
                // Показываем полосу когда вихрь неактивен
                if (!gameOverBar.gameObject.activeInHierarchy)
                {
                    gameOverBar.gameObject.SetActive(true);
                }
                
                // Проверяем контакты с полосой только если она видима
                if (isTouchingBar && touchingCubesCount > 0)
                {
                    currentContactTime += Time.deltaTime;
                    
                    // Изменяем цвет в зависимости от времени
                    UpdateBarColor();
                    
                    // Проверяем условие проигрыша
                    if (currentContactTime >= dangerTime)
                    {
                        TriggerGameOver();
                    }
                }
                else
                {
                    // Сбрасываем таймер если нет контакта
                    if (currentContactTime > 0f)
                    {
                        currentContactTime = 0f;
                        UpdateBarColor();
                    }
                }
            }
        }
    }
    
    void UpdateBarColor()
    {
        if (gameOverBar == null) return;
        
        // Плавно переходим от белого к красному
        float t = currentContactTime / dangerTime;
        gameOverBar.color = Color.Lerp(normalColor, dangerColor, t);
    }
    
    void TriggerGameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        
        Debug.Log("GAME OVER!");
        
        // Вызываем событие Game Over
        OnGameOver?.Invoke();
        
        // Вызываем Game Over в GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogError("GameOverManager: GameManager instance not found!");
        }
        // Можно добавить дополнительные эффекты
        Debug.LogWarning("GAME OVER - Кубики касались полосы более 3 секунд!");
    }
    
    // RestartGame убран - GameManager просто перезагружает сцену
    // Все объекты создадутся заново как при первом запуске
    
    // Публичный метод для принудительного сброса статических переменных
    public static void ForceReset()
    {
        if (Instance != null)
        {
            Instance.isGameOver = false;
            Instance.currentContactTime = 0f;
            Instance.touchingCubesCount = 0;
            Instance.isTouchingBar = false;
            Instance.isVortexActive = false;
            
            Debug.Log("GameOverManager: Force reset - all state cleared");
        }
    }
    
    // Методы для отслеживания контактов
    public void OnCubeEnterBar()
    {
        touchingCubesCount++;
        isTouchingBar = true;
        
        Debug.Log($"GameOverManager: Cube entered bar. Total touching: {touchingCubesCount}");
    }
    
    public void OnCubeExitBar()
    {
        touchingCubesCount--;
        if (touchingCubesCount <= 0)
        {
            touchingCubesCount = 0;
            isTouchingBar = false;
        }
        
        Debug.Log($"GameOverManager: Cube exited bar. Total touching: {touchingCubesCount}");
    }
    
    // Методы для управления состоянием вихря
    public void OnVortexActivated()
    {
        isVortexActive = true;
        
        // Сбрасываем таймер контактов при активации вихря
        currentContactTime = 0f;
        isTouchingBar = false;
        touchingCubesCount = 0;
        
        Debug.Log("GameOverManager: Vortex activated - hiding bar");
    }
    
    public void OnVortexDeactivated()
    {
        isVortexActive = false;
        
        Debug.Log("GameOverManager: Vortex deactivated - showing bar");
    }
    
    void OnDrawGizmosSelected()
    {
        if (gameOverBar != null)
        {
            // Рисуем границы полосы в редакторе
            Gizmos.color = isGameOver ? dangerColor : normalColor;
            Gizmos.DrawWireCube(gameOverBar.transform.position, gameOverBar.transform.localScale);
        }
    }
}
