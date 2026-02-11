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
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
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
            
            if (lang == "ru")
            {
                scoreText = "Счет: ";
                bestScoreText = "Лучший: ";
                advertisementIn = "Реклама через ";
            }
        }
        else
        {
        }
        
        
        // Принудительно обновляем все тексты после инициализации
        Invoke("UpdateAllTexts", 0.1f);
    }
    
    private void UpdateAllTexts()
    {
        
        // Ждем немного чтобы все менеджеры инициализировались
        Invoke("DelayedUpdateAllTexts", 0.1f);
    }
    
    private void DelayedUpdateAllTexts()
    {
        
        // Обновляем ScoreManager если есть
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.UpdateScoreDisplay();
        }
        else
        {
        }
        
        // Обновляем AdManager если есть
        if (AdManager.Instance != null)
        {
            // AdManager обновит текст при следующем вызове UpdateCountdownText
        }
        else
        {
        }
        
        // Обновляем GameManager если есть
        if (GameManager.Instance != null)
        {
        }
        else
        {
        }
    }
    
}
