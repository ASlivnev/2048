using UnityEngine;
using YG;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    [Header("Ad Settings")]
    [SerializeField] private float interstitialInterval = 70f; // Интервал между рекламами в секундах
    
    [Header("Debug UI")]
    [SerializeField] private TMPro.TextMeshProUGUI debugText; // Текст для отладки

    private float lastInterstitialTime;
    private bool isAdShowing = false;
    private bool firstInterstitialShown = false;
    
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

        // Подписка на события SDK
        YG2.onGetSDKData += OnSDKReady;
        YG2.onOpenInterAdv += OnAdOpened;
        YG2.onCloseInterAdv += OnAdClosed;
        
        if (YG2.isSDKEnabled)
        {
            OnSDKReady();
        }

        InitializeLanguage();

        // Всегда начинаем с текущего времени для чистого старта
        lastInterstitialTime = Time.time;
        PlayerPrefs.SetFloat("LastInterstitialTime", lastInterstitialTime);
        PlayerPrefs.Save();
        
        // НЕ показываем рекламу здесь - ждем инициализации SDK
    }

    void Update()
    {
        // Обновляем отладочный текст
        if (debugText != null)
        {
            float timeSinceLastAd = Time.time - lastInterstitialTime;
            float remainingTime = interstitialInterval - timeSinceLastAd;
            
            debugText.text = $"Ad Status: {(isAdShowing ? "SHOWING" : "Ready")}\n" +
                           $"Timer: {remainingTime:F1}s\n" +
                           $"First Ad: {(firstInterstitialShown ? "Shown" : "Pending")}";
        }
    }

    private void ShowStartInterstitial()
    {
        
        if (firstInterstitialShown)
        {
            return;
        }


        firstInterstitialShown = true;
        ShowInterstitialAd("Game Start");
    }
    
    private void OnSDKReady()
    {
        YG2.onGetSDKData -= OnSDKReady;
        
        // Показываем стартовую рекламу только после полной инициализации SDK
        ShowStartInterstitial();
    }

    // ИНИЦИАЛИЗАЦИЯ ЯЗЫКА
    private void InitializeLanguage()
    {
        string ygLanguage = YG2.lang;
        YG2.SwitchLanguage(ygLanguage);
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
            language = "en";
        }
        
        PlayerPrefs.SetString("language", language);
        PlayerPrefs.Save();
        
        Invoke("UpdateLangManager", 0.1f);
    }
    
    private void UpdateLangManager()
    {
        if (LangManager.Instance != null)
        {
            LangManager.Instance.InitializeLanguage();
        }
    }

    public void ShowInterstitialAd(string source)
    {
        if (isAdShowing)
        {
            return;
        }


        // Check ad availability
        if (IsInterstitialAvailable())
        {
            try
            {
                isAdShowing = true;
                YG2.InterstitialAdvShow();
            }
            catch (System.Exception e)
            {
                isAdShowing = false; // Сбрасываем флаг при ошибке
                // НЕ блокируем управление при ошибке рекламы
            }
        }
        else
        {
        }
    }

    // Проверка таймера для спауна
    public bool ShouldShowInterstitialOnSpawn()
    {
        
        if (isAdShowing) 
        {
            return false; // Реклама уже показывается
        }
        
        float timeSinceLastAd = Time.time - lastInterstitialTime;
        bool shouldShow = timeSinceLastAd >= interstitialInterval;
        
        
        return shouldShow;
    }
    
    private bool IsInterstitialAvailable()
    {
        bool isReady = YG2.isTimerAdvCompleted;
        float timer = YG2.timerInterAdv;
        
        
        return isReady;
    }

    // Yandex Games SDK events
    private void OnAdOpened()
    {
        try
        {
            GameManager.Instance.PauseGame();
        }
        catch (System.Exception e)
        {
            // Если реклама не открылась правильно, не блокируем игру
            isAdShowing = false;
        }
    }

    private void OnAdClosed()
    {
        try
        {

            isAdShowing = false;
            lastInterstitialTime = Time.time; // Сбрасываем таймер
            
            // Сохраняем время последней рекламы
            PlayerPrefs.SetFloat("LastInterstitialTime", lastInterstitialTime);
            PlayerPrefs.Save();
            
            // НЕ ВОЗОБНОВЛЯЕМ игру - оставляем на паузе
            
        }
        catch (System.Exception e)
        {
            // При ошибке закрытия тоже не блокируем игру
            isAdShowing = false;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from events
        YG2.onGetSDKData -= OnSDKReady;
        YG2.onOpenInterAdv -= OnAdOpened;
        YG2.onCloseInterAdv -= OnAdClosed;
    }

    public bool IsAdShowing => isAdShowing;
}
