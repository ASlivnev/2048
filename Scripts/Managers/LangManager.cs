using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LangManager : MonoBehaviour
{
    public static LangManager Instance { get; private set; }
    
    public string scoreText;
    public string bestScoreText;
    public string advertisementIn;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("LangManager: Created new instance");
        }
        else
        {
            Debug.Log("LangManager: Destroying duplicate instance");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Дополнительная инициализация при перезагрузке сцены
        InitializeLanguage();
    }
    
    public void InitializeLanguage()
    {
        // По умолчанию английский
        scoreText = "Score: ";
        bestScoreText = "Best: ";
        advertisementIn = "Advertisement in ";
        
        // Проверяем язык из PlayerPrefs
        if (PlayerPrefs.HasKey("language"))
        {
            string lang = PlayerPrefs.GetString("language");
            Debug.Log($"LangManager: Found language in PlayerPrefs: {lang}");
            
            if (lang == "ru")
            {
                scoreText = "Счет: ";
                bestScoreText = "Лучший: ";
                advertisementIn = "Реклама через ";
                Debug.Log("LangManager: Set Russian language");
            }
        }
        else
        {
            Debug.Log("LangManager: No language found in PlayerPrefs, using default English");
        }
        
        Debug.Log($"LangManager: Initialized - scoreText='{scoreText}', bestScoreText='{bestScoreText}', advertisementIn='{advertisementIn}'");
        
        // Принудительно обновляем все тексты после инициализации
        Invoke("UpdateAllTexts", 0.1f);
    }
    
    private void UpdateAllTexts()
    {
        Debug.Log("LangManager: Updating all texts after language initialization");
        
        // Ждем немного чтобы все менеджеры инициализировались
        Invoke("DelayedUpdateAllTexts", 0.2f);
    }
    
    private void DelayedUpdateAllTexts()
    {
        Debug.Log("LangManager: Delayed update of all texts");
        
        // Обновляем ScoreManager если есть
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.UpdateScoreDisplay();
            Debug.Log("LangManager: Updated ScoreManager texts");
        }
        else
        {
            Debug.LogWarning("LangManager: ScoreManager.Instance is null!");
        }
        
        // Обновляем AdManager если есть
        if (AdManager.Instance != null)
        {
            // AdManager обновит текст при следующем вызове UpdateCountdownText
            Debug.Log("LangManager: AdManager will update texts on next countdown");
        }
        else
        {
            Debug.LogWarning("LangManager: AdManager.Instance is null!");
        }
        
        // Обновляем GameManager если есть
        if (GameManager.Instance != null)
        {
            Debug.Log("LangManager: GameManager.Instance found");
        }
        else
        {
            Debug.LogWarning("LangManager: GameManager.Instance is null!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
