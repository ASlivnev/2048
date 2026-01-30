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
        UpdateScoreDisplay();
        
        Debug.Log($"ScoreManager: Started - Current: {currentScore}, Best: {bestScore}");
    }
    
    void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        Debug.Log($"ScoreManager: Loaded best score: {bestScore}");
    }
    
    void SaveBestScore()
    {
        PlayerPrefs.SetInt(BEST_SCORE_KEY, bestScore);
        PlayerPrefs.Save();
        Debug.Log($"ScoreManager: Saved best score: {bestScore}");
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
            Debug.Log($"ScoreManager: New best score! {bestScore}");
        }
    }
    
    // Методы для добавления очков
    public static void AddMergeScore(int cubeValue)
    {
        if (Instance != null)
        {
            Instance.AddScore(cubeValue);
            Debug.Log($"ScoreManager: Added {cubeValue} points for merging {cubeValue} cube");
        }
    }
    
    public static void AddSpecialCubeScore(string specialType, int cubeValue)
    {
        if (Instance != null)
        {
            int points = cubeValue / 2; // Половина значения кубика
            
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
    
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {FormatScore(currentScore)}";
        }
        
        if (bestScoreText != null)
        {
            bestScoreText.text = $"Best: {FormatScore(bestScore)}";
        }
    }
    
    static string FormatScore(int score)
    {
        if (score < 1000)
            return score.ToString();
        else if (score < 1000000)
            return $"{score / 1000} K";
        else if (score < 1000000000)
            return $"{score / 1000000} M";
        else if (score < 1000000000L)
            return $"{score / 1000000000} B";
        else
            return $"{score / 1000000000L} T";
    }
}
