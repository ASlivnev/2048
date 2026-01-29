using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    
    private static int currentScore = 0;
    private static int bestScore = 0;
    private const string BEST_SCORE_KEY = "BestScore2048Cubes";
    
    public static ScoreManager Instance { get; private set; }
    
    public static int CurrentScore => currentScore;
    public static int BestScore => bestScore;
    
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
        LoadBestScore();
        UpdateScoreDisplay();
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
    
    public static void AddScore(int points)
    {
        currentScore += points;
        
        if (Instance != null)
        {
            Instance.UpdateScoreDisplay();
            
            // Проверяем и обновляем лучший результат
            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                Instance.SaveBestScore();
                Debug.Log($"ScoreManager: New best score! {bestScore}");
            }
        }
    }
    
    public static void ResetScore()
    {
        currentScore = 0;
        
        if (Instance != null)
        {
            Instance.UpdateScoreDisplay();
        }
        
        Debug.Log("ScoreManager: Score reset");
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
        else if (score < 1000000000000)
            return $"{score / 1000000000} B";
        else
            return $"{score / 1000000000000} T";
    }
    
    // Метод для добавления очков при слиянии кубиков
    public static void AddMergeScore(int cubeValue)
    {
        int points = cubeValue;
        AddScore(points);
        Debug.Log($"ScoreManager: Added {points} points for merging {cubeValue} cube");
    }
    
    // Метод для добавления очков за спецкубики
    public static void AddSpecialCubeScore(string specialType, int cubeValue)
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
        
        AddScore(points);
        Debug.Log($"ScoreManager: Added {points} points for {specialType} special cube");
    }
}
