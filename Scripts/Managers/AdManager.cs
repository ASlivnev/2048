using UnityEngine;
using TMPro;
using YG;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    [Header("Ad Settings")]
    [SerializeField] private float interstitialInterval = 120f; // 2 minutes in seconds
    [SerializeField] private float countdownDuration = 2f; // 2 seconds countdown

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI countdownText;

    private float lastInterstitialTime;
    private bool isAdShowing = false;
    private bool isCountdownActive = false;
    private float countdownTime;

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
        Debug.Log("AdManager: Initializing");

        // ИНИЦИАЛИЗАЦИЯ ЯЗЫКА - как в StartGameManager
        InitializeLanguage();

        // Subscribe to ad events - ПРАВИЛЬНЫЕ МЕТОДЫ
        YG2.onOpenInterAdv += OnAdOpened;
        YG2.onCloseInterAdv += OnAdClosed;
        // YG2.onErrorInterAdv += OnAdError; // Убираем - нет такого события

        // Hide countdown text at start
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        // Show start ad after 1 second - ВОССТАНАВЛИВАЕМ
        Invoke("ShowStartAd", 1f);

        // Start timer for periodic ads
        InvokeRepeating("CheckInterstitialTimer", 1f, 1f);
    }

    void Update()
    {
        // Update countdown
        if (isCountdownActive)
        {
            countdownTime -= Time.unscaledDeltaTime; // НЕЗАВИСИМОЕ ВРЕМЯ!

            // Update countdown text in UI
            UpdateCountdownText();

            if (countdownTime <= 0f)
            {
                // Countdown finished, show ad
                StartAdAfterCountdown();
            }
        }
    }

    private void UpdateCountdownText()
    {
        if (countdownText != null)
        {
            countdownText.text = $"Advertisement in {countdownTime:F1}";
        }
        else
        {
            Debug.LogWarning("AdManager: Countdown Text is not assigned!");
        }
    }

    void OnEnable()
    {
        // Subscribe to Game Over events
        GameOverManager.OnGameOver += ShowInterstitialOnGameOver;
    }

    void OnDisable()
    {
        // Unsubscribe from events
        GameOverManager.OnGameOver -= ShowInterstitialOnGameOver;

        // Unsubscribe from ad events - ПРАВИЛЬНЫЕ МЕТОДЫ
        YG2.onOpenInterAdv -= OnAdOpened;
        YG2.onCloseInterAdv -= OnAdClosed;
        // YG2.onErrorInterAdv -= OnAdError; // Убираем - нет такого события
        
        // УБРАЛИ СОБЫТИЯ ЛОКАЛИЗАЦИИ - используем простой метод
        // YG2.onGetLanguage -= OnGetLanguage; // Не нужно - нет подписки
    }

    private void ShowStartAd()
    {
        ShowInterstitialAd("Game Start");
    }

    // ИНИЦИАЛИЗАЦИЯ ЯЗЫКА - ПРОСТОЙ МЕТОД
    private void InitializeLanguage()
    {
        Debug.Log("[AdManager] Инициализация языка");
        
        // Простая инициализация языка - используем русский по умолчанию
        string language = "ru"; // По умолчанию русский
        
        // Проверяем системный язык (простой способ)
        if (Application.systemLanguage == SystemLanguage.English)
        {
            language = "en";
        }
        else if (Application.systemLanguage == SystemLanguage.Russian)
        {
            language = "ru";
        }
        
        Debug.Log($"[AdManager] Системный язык: {Application.systemLanguage}, установленный язык: {language}");
        
        // Устанавливаем язык в PlayerPrefs
        PlayerPrefs.SetString("lang", language);
        PlayerPrefs.Save();
        
        Debug.Log($"[AdManager] Установлен язык: {language}");
    }

    private void ShowInterstitialOnGameOver()
    {
        // УБРАЛИ ПРОВЕРКУ isAdShowing - реклама должна показываться всегда
        Invoke("ShowGameOverAd", 0.5f);
    }

    private void ShowGameOverAd()
    {
        ShowInterstitialAd("Game Over");
    }

    private void CheckInterstitialTimer()
    {
        // Check if 2 minutes have passed since the last ad
        if (Time.time - lastInterstitialTime >= interstitialInterval)
        {
            ShowInterstitialAd("Timer");
            lastInterstitialTime = Time.time;
        }
    }

    public void ShowInterstitialAd(string source)
    {
        if (isCountdownActive)
        {
            Debug.Log($"AdManager: Countdown already active, skipping {source} request");
            return;
        }
        
        // УБРАЛИ ПРОВЕРКУ isAdShowing - реклама должна показываться постоянно

        Debug.Log($"AdManager: Starting countdown for interstitial ad from {source}");

        // Check ad availability (placeholder for Yandex Games)
        if (IsInterstitialAvailable())
        {
            StartCountdown(source);
        }
        else
        {
            Debug.LogWarning("AdManager: Interstitial ad not available");
        }
    }

    private void StartCountdown(string source)
    {
        // Start countdown
        isCountdownActive = true;
        countdownTime = countdownDuration;

        // Show countdown text
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            UpdateCountdownText();
        }

        // ПРОСТАЯ ПАУЗА - Time.timeScale = 0 (без модального окна)
        Time.timeScale = 0f;
        
        // БЛОКИРУЕМ УПРАВЛЕНИЕ ПРИ ОБРАТНОМ ОТСЧЁТЕ
        DisableGameControls();

        Debug.Log($"AdManager: Countdown started for {countdownDuration} seconds from {source}");
    }

    private void StartAdAfterCountdown()
    {
        isCountdownActive = false;
        isAdShowing = true;

        // Hide countdown text
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        Debug.Log("AdManager: Countdown completed, showing ad");

        // ПОКАЗЫВАЕМ МОДАЛЬНОЕ ОКНО (пауза через GameManager)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
            // Если GameManager недоступен, блокируем управление вручную
            DisableGameControls();
        }

        // Actual call to Yandex Games SDK - ПРАВИЛЬНЫЙ МЕТОД
        YG2.InterstitialAdvShow();
    }

    // Yandex Games SDK events
    private void OnAdOpened()
    {
        Debug.Log("AdManager: Yandex Games ad opened");
    }

    private void OnAdClosed()
    {
        Debug.Log("AdManager: Yandex Games ad closed");

        // Ad closed - complete the process
        isAdShowing = false;
        lastInterstitialTime = Time.time;
        
        // ИГРА ОСТАЕТСЯ НА ПАУЗЕ - НЕ ВОЗОБНОВЛЯЕМ УПРАВЛЕНИЕ
        // GameManager.Instance.ResumeGame(); // Закомментировано
        // Управление остается заблокированным - игрок должен нажать кнопку продолжения
    }

    
    private bool IsInterstitialAvailable()
    {
        // Check ad availability via Yandex Games SDK - ПРОСТАЯ ПРОВЕРКА
        return true; // Всегда доступно для Yandex Games
    }

    public bool IsAdShowing => isAdShowing;
    public bool IsCountdownActive => isCountdownActive;
    public float CountdownTime => countdownTime;

    public float TimeUntilNextAd => Mathf.Max(0f, interstitialInterval - (Time.time - lastInterstitialTime));
    
    // БЛОКИРОВКА УПРАВЛЕНИЯ - КОПИЯ ИЗ GAMEMANAGER
    private void DisableGameControls()
    {
        Debug.Log("AdManager: Disabling game controls during countdown");
        
        // Отключаем спаунер кубиков
        if (CubeSpawner.Instance != null)
        {
            CubeSpawner.Instance.enabled = false;
        }
        
        // Отключаем все скрипты управления на кубиках
        DisableCubeControls();
    }
    
    private void DisableCubeControls()
    {
        // Находим все скрипты управления на кубиках и отключаем их
        MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            if (script != null && (script.name.Contains("Control") || 
                script.name.Contains("Input") || 
                script.name.Contains("Mouse")))
            {
                script.enabled = false;
            }
        }
    }
}