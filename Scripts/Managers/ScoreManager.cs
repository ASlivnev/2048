using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    
    private int currentScore = 0;
    private int bestScore = 0;
    private const string BEST_SCORE_KEY = "BestScore2048Cubes";
    
    public static ScoreManager Instance { get; private set; }
    
    public int CurrentScore => currentScore;
    public int BestScore => bestScore;
    
    [Header("Record score")]
    [SerializeField] private GameObject recordModal;
    [SerializeField] private TextMeshProUGUI newRecordText;
    
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Убираем DontDestroyOnLoad чтобы ScoreManager пересоздавался при рестарте
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        LoadBestScore();
        currentScore = 0; // Всегда начинаем с нуля как при первом запуске
        
        Debug.Log("ScoreManager: Started - initializing texts");
        
        // Ждем LangManager и обновляем тексты
        Invoke("DelayedUpdateScoreDisplay", 0.3f);
    }
    
    private void DelayedUpdateScoreDisplay()
    {
        UpdateScoreDisplay();
    }
    
    void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
        Debug.Log("ScoreManager: Score reset to 0");
    }
    
    void SaveBestScore()
    {
        PlayerPrefs.SetInt(BEST_SCORE_KEY, bestScore);
        PlayerPrefs.Save();
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        
        UpdateScoreDisplay();
        
        // Проверяем и обновляем лучший результат
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            SaveBestScore();
        }
    }
    
    // Методы для добавления очков
    public static void AddMergeScore(int cubeValue)
    {
        if (Instance != null)
        {
            Instance.AddScore(cubeValue);
        }
    }
    
    public static void AddSpecialCubeScore(string specialType, int cubeValue)
    {
        if (Instance != null)
        {
            int points = cubeValue / 2; 
            
            switch (specialType)
            {
                case "Plus":
                    points = cubeValue;
                    break;
                case "Minus":
                    points = cubeValue / 4;
                    break;
                case "Death":
                    points = cubeValue * 2;
                    break;
                case "Grow":
                    points = cubeValue / 2;
                    break;
                case "Shrink":
                    points = cubeValue / 3;
                    break;
                case "Freeze":
                    points = cubeValue / 2;
                    break;
                case "Vortex":
                    points = cubeValue * 3;
                    break;
            }
            
            Instance.AddScore(points);
            Debug.Log($"ScoreManager: Added {points} points for {specialType} special cube");
        }
    }
    
    public void UpdateScoreDisplay()
    {
        // Используем LangManager.Instance вместо FindObjectOfType
        string scoreLabel = "Score: ";
        string bestLabel = "Best: ";
        
        if (LangManager.Instance != null)
        {
            scoreLabel = LangManager.Instance.scoreText;
            bestLabel = LangManager.Instance.bestScoreText;
            Debug.Log($"ScoreManager: LangManager found - scoreLabel='{scoreLabel}', bestLabel='{bestLabel}'");
        }
        else
        {
            Debug.LogWarning("ScoreManager: LangManager.Instance is null!");
        }
        
        Debug.Log($"ScoreManager: Updating texts - currentScore={currentScore}, bestScore={bestScore}");
        
        if (scoreText != null)
        {
            scoreText.text = $"{scoreLabel}{FormatScore(currentScore)}";
            Debug.Log($"ScoreManager: Updated scoreText to '{scoreText.text}'");
        }
        
        if (bestScoreText != null)
        {
            bestScoreText.text = $"{bestLabel}{FormatScore(bestScore)}";
            Debug.Log($"ScoreManager: Updated bestScoreText to '{bestScoreText.text}'");
        }
        
        if (newRecordText != null)
        {
            newRecordText.text = $"{scoreLabel}{FormatScore(currentScore)}";
            Debug.Log($"ScoreManager: Updated newRecordText to '{newRecordText.text}'");
        }
        
        if (currentScore > bestScore)
        {
            // Используем LangManager.Instance для локализации "Best score"
            string bestScoreLabel = "Best score: ";
            
            if (LangManager.Instance != null)
            {
                bestScoreLabel = LangManager.Instance.bestScoreText;
            }
            
            newRecordText.text = $"{bestScoreLabel}{FormatScore(currentScore)}";
            recordModal.SetActive(true);
            Debug.Log($"ScoreManager: New record! Updated to '{newRecordText.text}'");
        }
        else
        {
            // Рекорд НЕ побит - скрываем модальное окно
            recordModal.SetActive(false);
            Debug.Log($"ScoreManager: No new record (currentScore={currentScore} <= bestScore={bestScore}) - hiding record modal");
        }
    }
    
    static string FormatScore(int score)
    {
        if (score < 1000)
            return score.ToString();
        else if (score < 1000000)
            return $"{score / 1000}K";
        else if (score < 1000000000)
            return $"{score / 1000000}M";
        else if (score < 1000000000L)
            return $"{score / 1000000000}B";
        else
            return $"{score / 1000000000L}T";
    }
}
