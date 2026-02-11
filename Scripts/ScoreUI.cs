using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    
    void Start()
    {
        // Находим ScoreManager и подписываемся на обновления
        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay();
        }
        else
        {
        }
    }
    
    void Update()
    {
        // Обновляем отображение счета каждый кадр (можно оптимизировать)
        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay();
        }
    }
    
    void UpdateScoreDisplay()
    {
        if (ScoreManager.Instance != null)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {FormatScore(ScoreManager.Instance.CurrentScore)}";
            }
            
            if (bestScoreText != null)
            {
                bestScoreText.text = $"Best: {FormatScore(ScoreManager.Instance.BestScore)}";
            }
        }
    }
    
    string FormatScore(int score)
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
