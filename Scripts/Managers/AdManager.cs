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
        
        // Подписываемся на событие готовности SDK
        YG2.onGetSDKData += OnSDKReady;
        
        InitializeLanguage();

        // Subscribe to ad events - ПРАВИЛЬНЫЕ МЕТОДЫ
        YG2.onOpenInterAdv += OnAdOpened;
        YG2.onCloseInterAdv += OnAdClosed;

        if (PlayerPrefs.HasKey("LastInterstitialTime"))
        {
            lastInterstitialTime = PlayerPrefs.GetFloat("LastInterstitialTime");
            Debug.Log($"AdManager: Loaded saved lastInterstitialTime={lastInterstitialTime:F1}");
        }
        else
        {
            lastInterstitialTime = Time.time;
            PlayerPrefs.SetFloat("LastInterstitialTime", lastInterstitialTime);
            PlayerPrefs.Save();
            Debug.Log($"AdManager: Set new lastInterstitialTime={lastInterstitialTime:F1}");
        }
        
        // ГАРАНТИРОВАННО останавливаем обратный отсчёт при старте
        isCountdownActive = false;
        countdownTime = 2f;
        
        // АБСОЛЮТНО ГАРАНТИРОВАННО скрываем текст обратного отсчёта
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
            Debug.Log("AdManager: countdownText ABSOLUTELY hidden at start");
        }
        
        // ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА - УНИЧТОЖАЕМ ТЕКСТ
        if (countdownText != null && countdownText.gameObject != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        Debug.Log($"AdManager: Initialized - lastInterstitialTime={lastInterstitialTime:F1}, countdown stopped");
        
        // Запускаем периодическую рекламу
        InvokeRepeating("CheckInterstitialTimer", 5f, 5f);
    }

    private void OnSDKReady()
    {
        Debug.Log("AdManager: SDK data received, showing start ad");
        
        // Отписываемся от события
        YG2.onGetSDKData -= OnSDKReady;
        
        // Показываем стартовую рекламу сразу без задержки
        //ShowStartAdWithoutCountdown();
    }

    void Update()
    {
        // АБСОЛЮТНАЯ БЛОКИРОВКА ОБРАТНОГО ОТСЧЁТА ПРИ РЕСТАРТЕ
        if (GameManager.Instance != null && GameOverManager.IsGameOver)
        {
            // Если Game Over - ничего не делаем
            return;
        }
        
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
        
        // АБСОЛЮТНАЯ ПРОВЕРКА - если countdownText видим, но отсчёт не активен
        if (countdownText != null && countdownText.gameObject.activeSelf && !isCountdownActive)
        {
            Debug.LogWarning("AdManager: countdownText is visible but countdown is not active - FORCE HIDING");
            countdownText.gameObject.SetActive(false);
        }
        
        // ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА - если countdownText видим вообще
        if (countdownText != null && countdownText.gameObject.activeSelf)
        {
            // Проверяем не было ли недавно рестарта (за последние 3 секунды)
            if (Time.time < 3.0f) // Если игра работает меньше 3 секунд
            {
                Debug.LogWarning("AdManager: Game recently restarted - FORCE HIDING countdown text");
                countdownText.gameObject.SetActive(false);
                isCountdownActive = false;
            }
        }
    }

    private void UpdateCountdownText()
    {
        if (countdownText != null)
        {
            // Используем LangManager.Instance
            if (LangManager.Instance != null)
            {
                countdownText.text = $"{LangManager.Instance.advertisementIn}{countdownTime:F1}";
            }
            else
            {
                countdownText.text = $"Advertisement in {countdownTime:F1}";
            }
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
        
        // ОТПИСЫВАЕМСЯ ОТ СОБЫТИЯ ГОТОВНОСТИ SDK
        YG2.onGetSDKData -= OnSDKReady;
        
        // УБРАЛИ СОБЫТИЯ ЛОКАЛИЗАЦИИ - используем простой метод
        // YG2.onGetLanguage -= OnGetLanguage; // Не нужно - нет подписки
    }

    // СТАРТОВАЯ РЕКЛАМА БЕЗ ОБРАТНОГО ОТСЧЁТА
    private void ShowStartAdWithoutCountdown()
    {
        Debug.Log("AdManager: Showing start ad WITHOUT countdown");
        
        // Просто вызываем рекламу как в рабочем примере
        YG2.InterstitialAdvShow();
        isAdShowing = true;
        
        // Только блокируем управление, НЕ показываем меню паузы
        DisableGameControls();
        
        Debug.Log("AdManager: Start ad initiated successfully");
    }
    
    // ПЕРИОДИЧЕСКАЯ РЕКЛАМА С ОБРАТНЫМ ОТСЧЁТОМ
    private void ShowStartAd()
    {
        ShowInterstitialAd("Game Start");
    }

    // ИНИЦИАЛИЗАЦИЯ ЯЗЫКА - ЧЕРЕЗ YG ПЛАГИН
    private void InitializeLanguage()
    {
        Debug.Log("[AdManager] Инициализация языка через YG плагин");
        
        // Используем YG2.SwitchLanguage для определения языка
        string ygLanguage = YG2.lang;
        Debug.Log($"[AdManager] Язык из YG плагина: {ygLanguage}");
        
        YG2.SwitchLanguage(ygLanguage);

        // Конвертируем язык YG в наш формат
        string language = "en"; // По умолчанию en
        
        if (ygLanguage == "en" || ygLanguage == "en-US" || ygLanguage == "en-GB")
        {
            language = "en";
        }
        else if (ygLanguage == "ru" || ygLanguage == "ru-RU")
        {
            language = "ru";
        }
        else
        {
            // Для других языков используем английский по умолчанию
            language = "en";
        }
        
        Debug.Log($"[AdManager] Конвертированный язык: {language}");
        
        // Устанавливаем язык в PlayerPrefs
        PlayerPrefs.SetString("language", language);
        PlayerPrefs.Save();
        
        Debug.Log($"[AdManager] Язык установлен в PlayerPrefs: {language}");
        
        // Принудительно обновляем LangManager после установки языка
        Invoke("UpdateLangManager", 0.1f);
    }
    
    private void UpdateLangManager()
    {
        if (LangManager.Instance != null)
        {
            LangManager.Instance.InitializeLanguage();
            Debug.Log("AdManager: LangManager updated after language change");
        }
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
        // Проверяем корректность lastInterstitialTime после рестарта
        if (lastInterstitialTime > Time.time)
        {
            // Time.time был сброшен, корректируем lastInterstitialTime
            lastInterstitialTime = Time.time;
            PlayerPrefs.SetFloat("LastInterstitialTime", lastInterstitialTime);
            PlayerPrefs.Save();
            Debug.Log($"AdManager: Fixed lastInterstitialTime after restart - new value: {lastInterstitialTime:F1}");
            return;
        }
        
        // Check if adInterval has passed since the last ad
        float timePassed = Time.time - lastInterstitialTime;
        Debug.Log($"AdManager: Timer check - Time.time={Time.time:F1}, lastInterstitialTime={lastInterstitialTime:F1}, timePassed={timePassed:F1}, interval={interstitialInterval}");
        
        if (timePassed >= interstitialInterval)
        {
            Debug.Log($"AdManager: Timer condition met! {timePassed:F1} >= {interstitialInterval}");
            ShowInterstitialAd("Timer");
            lastInterstitialTime = Time.time;
            
            // СОХРАНЯЕМ НОВОЕ ВРЕМЯ
            PlayerPrefs.SetFloat("LastInterstitialTime", lastInterstitialTime);
            PlayerPrefs.Save();
            
            Debug.Log($"AdManager: Timer ad shown, new lastInterstitialTime={lastInterstitialTime:F1} (SAVED)");
        }
        else
        {
            Debug.Log($"AdManager: Timer not ready yet, need {interstitialInterval - timePassed:F1} more seconds");
        }
    }

    public void ResetAdTimer()
    {
        // АБСОЛЮТНАЯ БЛОКИРОВКА ОБРАТНОГО ОТСЧЁТА
        isCountdownActive = false;
        countdownTime = 2f;
        
        // АБСОЛЮТНО СКРЫВАЕМ ТЕКСТ ОБРАТНОГО ОТСЧЁТА
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
            Debug.Log("AdManager: countdownText ABSOLUTELY hidden during reset");
        }
        
        // ДОПОЛНИТЕЛЬНОЕ УНИЧТОЖЕНИЕ ТЕКСТА ЕСЛИ ОН ЕСТЬ
        if (countdownText != null && countdownText.gameObject != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        Debug.Log("AdManager: ResetAdTimer called - COUNTDOWN ABSOLUTELY STOPPED");
    }
    
    public void ResetAdTimerFully()
    {
        // Вызываем обычный сброс
        ResetAdTimer();
        
        // НЕ СБРАСЫВАЕМ ВРЕМЯ ПОСЛЕДНЕЙ РЕКЛАМЫ - таймер должен продолжать работать
        // lastInterstitialTime = Time.time; // ЗАКОММЕНТИРОВАНО
        // PlayerPrefs.SetFloat("LastInterstitialTime", lastInterstitialTime); // ЗАКОММЕНТИРОВАНО
        // PlayerPrefs.Save(); // ЗАКОММЕНТИРОВАНО
        
        Debug.Log($"AdManager: ResetAdTimerFully called - lastInterstitialTime KEPT at {lastInterstitialTime:F1}");
    }
    
    public void StopAdTimer()
    {
        CancelInvoke("CheckInterstitialTimer");
        CancelInvoke("ShowGameOverAd"); // ОТМЕНЯЕМ ОТЛОЖЕННЫЙ ВЫЗОВ РЕКЛАМЫ!
        isCountdownActive = false;
        isAdShowing = false;

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        Debug.Log("AdManager: Ad timer STOPPED (including ShowGameOverAd invoke)");
    }
    
    public void StartAdTimer()
    {
        CancelInvoke("CheckInterstitialTimer");
        InvokeRepeating("CheckInterstitialTimer", 5f, 5f);
        Debug.Log("AdManager: Ad timer STARTED");
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
        // Проверяем доступность рекламы через Yandex Games SDK
        bool isReady = YG2.isTimerAdvCompleted;
        float timer = YG2.timerInterAdv;
        
        Debug.Log($"AdManager: Interstitial availability check - timer completed: {isReady}, timer: {timer:F1}s");
        
        return isReady;
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