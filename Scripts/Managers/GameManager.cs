using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class GameManager : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    
    [Header("UI Elements")]
    [SerializeField] private Button muteButton;
    
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
        
        // Сбрасываем состояние игры при старте
        isPaused = false;
        isGameOver = false;
        
        // Гарантируем правильный TimeScale при старте
        Time.timeScale = 1f;
        
        
        // Скрываем меню паузы при старте
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        else
        {
        }
        
        // Скрываем меню Game Over при старте
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(false);
        }
        else
        {
        }
        
        // Включаем управление при старте
        EnableGameControls();
        
        // Вызываем GameReady API когда игра полностью готова
        YG2.GameReadyAPI();
        
        // Инициализируем цвет кнопки Mute с задержкой
        StartCoroutine(InitializeMuteButtonDelayed());
        
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
        
        if (isPaused) 
        {
            return;
        }
        
        if (isGameOver)
        {
            return;
        }
        
        isPaused = true;
        
        DisableGameControls();
        
        Time.timeScale = 0f;
        
        // Отключаем звук во время паузы
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(0f);
        }
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
        else
        {
        }
        
    }
    
    // Публичная функция: Снять с паузы
    public void ResumeGame()
    {
        
        if (isPaused) 
        {
        }
        else 
        {
            return;
        }
        
        if (isGameOver)
        {
            return;
        }
        
        isPaused = false;
        
        EnableGameControls();
        
        Time.timeScale = 1f;
        
        // Включаем звук после паузы
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(1f);
        }
        
        // Скрываем меню паузы
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        else
        {
        }
        
    }
    
    // Специальный метод для возобновления после рекламы (принудительный)
    public void ForceResumeGame()
    {
        
        if (isGameOver)
        {
            return;
        }
        
        // Принудительно сбрасываем состояние паузы
        isPaused = false;
        
        EnableGameControls();
        
        Time.timeScale = 1f;
        
        // Включаем звук после паузы
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(1f);
        }
        
        // Скрываем меню паузы
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        else
        {
        }
        
    }
    
    // Публичное свойство для проверки состояния паузы
    public bool IsPaused => isPaused;
    
    public bool IsGameOver => isGameOver;
    
    // БЛОКИРОВКА УПРАВЛЕНИЯ ПРИ ПАУЗЕ
    public void DisableGameControls()
    {
        
        // Отключаем спаунер кубиков
        if (CubeSpawner.Instance != null)
        {
            CubeSpawner.Instance.enabled = false;
        }
        
        // Отключаем все скрипты управления на кубиках
        DisableCubeControls();
    }
    
    // ВОССТАНОВЛЕНИЕ УПРАВЛЕНИЯ
    public void EnableGameControls()
    {
        
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
        }
        
        // Показываем меню Game Over
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(true);
        }
        else
        {
        }
        
    }
    
    // Публичная функция: Рестарт игры
    public void RestartGame()
    {
        
        // Сбрасываем состояние
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;
        
        // Удаляем все кубики
        DestroyAllCubes();
        
        // Очищаем score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        
        // Сбрасываем GameOverManager
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ResetGameOver();
        }
        
        // Сбрасываем спаунер к исходным значениям
        if (CubeSpawner.Instance != null)
        {
            CubeSpawner.Instance.ResetSpawnerState();
        }
        
        // Таймеры рекламы удалены - больше не нужно управлять ими
        
        // Скрываем меню Game Over
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(false);
        }
        
        // Скрываем меню паузы
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        
    }
    
    private void DestroyAllCubes()
    {
        // Находим все кубики и уничтожаем их
        Cube[] allCubes = FindObjectsOfType<Cube>();
        foreach (Cube cube in allCubes)
        {
            if (cube != null)
            {
                Destroy(cube.gameObject);
            }
        }
        
        
        // Уведомляем CubeSpawner что все кубики уничтожены
        if (CubeSpawner.Instance != null)
        {
            CubeSpawner.Instance.OnAllCubesDestroyed();
        }
    }
    
    // Публичная функция: Очистить все PlayerPrefs
    public void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
    
    // Публичная функция: Переключить звук
    public void ToggleSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ToggleMute();
            
            // Меняем цвет кнопки в зависимости от состояния
            if (muteButton != null)
            {
                Image buttonImage = muteButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    if (SoundManager.Instance.IsMuted())
                    {
                        // Темный цвет когда выключен
                        buttonImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    }
                    else
                    {
                        // Белый цвет когда включен
                        buttonImage.color = Color.white;
                    }
                }
            }
        }
    }
    
    IEnumerator InitializeMuteButtonDelayed()
    {
        // Ждем пока SoundManager инициализируется
        int attempts = 0;
        while (SoundManager.Instance == null && attempts < 10)
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }
        
        // Устанавливаем начальный цвет кнопки (всегда белый - звук включен)
        if (muteButton != null && SoundManager.Instance != null)
        {
            yield return new WaitForSeconds(0.1f);
            
            Image buttonImage = muteButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = Color.white; // Белый - звук включен
            }
        }
    }
    
    void InitializeMuteButtonColor()
    {
        if (muteButton != null && SoundManager.Instance != null)
        {
            Image buttonImage = muteButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (SoundManager.Instance.IsMuted())
                {
                    buttonImage.color = new Color(0.7f, 0.7f, 0.7f, 1f); // Светло-серый
                }
                else
                {
                    buttonImage.color = Color.white; // Белый
                }
            }
        }
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
