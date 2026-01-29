using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
        
        // Проверяем контакты с полосой
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
        
        // Устанавливаем красный цвет
        if (gameOverBar != null)
        {
            gameOverBar.color = dangerColor;
        }
        
        // Ставим игру на паузу
        Time.timeScale = 0f;
        
        // Выводим в консоль
        Debug.Log("GAME OVER");
        
        // Можно добавить дополнительные эффекты
        Debug.LogWarning("GAME OVER - Кубики касались полосы более 3 секунд!");
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
    
    // Публичный метод для перезапуска игры
    public void RestartGame()
    {
        isGameOver = false;
        currentContactTime = 0f;
        touchingCubesCount = 0;
        isTouchingBar = false;
        
        // Возвращаем белый цвет
        if (gameOverBar != null)
        {
            gameOverBar.color = normalColor;
        }
        
        // Снимаем паузу
        Time.timeScale = 1f;
        
        // Сбрасываем счет
        ScoreManager.ResetScore();
        
        Debug.Log("GameOverManager: Game restarted");
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
