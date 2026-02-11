using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }
    
    [Header("Game Over Settings")]
    public float dangerTime = 3f;
    public Transform barTransform;
    public SpriteRenderer gameOverBar;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    
    [Header("Vortex Settings")]
    public string vortexCubeTag = "VortexCube";
    
    private bool isGameOver = false;
    private bool isTouchingBar = false;
    private int touchingCubesCount = 0;
    private float currentContactTime = 0f;
    private bool isVortexActive = false;
    
    // Сохраняем начальный лучший результат при старте игры
    private int initialBestScore;
    
    public event Action OnGameOver;
    
    public static bool IsGameOver => Instance != null ? Instance.isGameOver : false;
    
    public void ResetGameOver()
    {
        isGameOver = false;
        currentContactTime = 0f;
        isTouchingBar = false;
        touchingCubesCount = 0;
        isVortexActive = false;
        
        // Сбрасываем цвет Game Over бара
        if (gameOverBar != null)
        {
            gameOverBar.color = normalColor;
        }
        
    }
    
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
        // Загружаем начальный лучший результат из PlayerPrefs (но не сохраняем в процессе игры)
        initialBestScore = PlayerPrefs.GetInt("BestScore2048Cubes", 0);
        
        // Устанавливаем начальный цвет полосы
        if (gameOverBar != null)
        {
            gameOverBar.color = normalColor;
        }
        
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
        gameOverBar.color = Color.Lerp(normalColor, warningColor, t);
    }
    
    void TriggerGameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        
        // Воспроизводим звук в зависимости от рекорда ДО сохранения
        if (SoundManager.Instance != null && ScoreManager.Instance != null)
        {
            int currentScore = ScoreManager.Instance.CurrentScore;
            
            if (currentScore > initialBestScore)
            {
                // Новый рекорд - играем WOW
                // SoundManager.Instance.PlayWowSound();
            }
            else
            {
                // Рекорд не побит - играем FOO
                // SoundManager.Instance.PlayFooSound();
            }
        }
        
        // Вызываем событие Game Over
        OnGameOver?.Invoke();
        
        // Вызываем Game Over в GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
        else
        {
        }
        // Можно добавить дополнительные эффекты
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
            
        }
    }
    
    // Методы для отслеживания контактов
    public void OnCubeEnterBar()
    {
        touchingCubesCount++;
        isTouchingBar = true;
        
    }
    
    public void OnCubeExitBar()
    {
        touchingCubesCount--;
        if (touchingCubesCount <= 0)
        {
            touchingCubesCount = 0;
            isTouchingBar = false;
        }
        
    }
    
    // Методы для управления состоянием вихря
    public void OnVortexActivated()
    {
        isVortexActive = true;
        
        // Сбрасываем таймер контактов при активации вихря
        currentContactTime = 0f;
        isTouchingBar = false;
        touchingCubesCount = 0;
        
    }
    
    public void OnVortexDeactivated()
    {
        isVortexActive = false;
        
    }
    
    void OnDrawGizmosSelected()
    {
        if (gameOverBar != null)
        {
            // Рисуем границы полосы в редакторе
            Gizmos.color = isGameOver ? warningColor : normalColor;
            Gizmos.DrawWireCube(gameOverBar.transform.position, gameOverBar.transform.localScale);
        }
    }
}
