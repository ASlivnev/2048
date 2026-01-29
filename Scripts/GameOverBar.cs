using UnityEngine;

public class GameOverBar : MonoBehaviour
{
    [Header("Bar Settings")]
    [SerializeField] private float barWidth = 10f;
    [SerializeField] private float barHeight = 0.5f;
    [SerializeField] private bool isTopBar = true;
    
    void Start()
    {
        // Устанавливаем тег для обнаружения
        if (gameObject.tag != "GameOverBar")
        {
            gameObject.tag = "GameOverBar";
        }
        
        // Настраиваем позицию если это верхняя полоса
        if (isTopBar)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                float topY = mainCamera.orthographicSize - barHeight / 2;
                transform.position = new Vector3(0, topY, 0);
            }
        }
        
        // Настраиваем размер
        transform.localScale = new Vector3(barWidth, barHeight, 1f);
        
        Debug.Log("GameOverBar: Initialized");
    }
    
    void OnDrawGizmosSelected()
    {
        // Рисуем границы полосы в редакторе
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
