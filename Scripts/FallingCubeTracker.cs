using UnityEngine;

public class FallingCubeTracker : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isFalling = true;
    private float checkInterval = 0.1f;
    private float checkTimer = 0f;
    private Vector2 lastPosition;
    private float minMovementThreshold = 0.01f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastPosition = rb.position;
    }
    
    void Update()
    {
        if (!isFalling) return;
        
        checkTimer += Time.deltaTime;
        
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckIfLanded();
        }
    }
    
    void CheckIfLanded()
    {
        if (rb == null) return;
        
        Vector2 currentPosition = rb.position;
        Vector2 velocity = rb.velocity;
        
        // Проверяем если кубик почти остановился (скорость очень мала)
        bool isAlmostStopped = Mathf.Abs(velocity.y) < 0.1f && 
                               Mathf.Abs(velocity.x) < 0.1f;
        
        // ИЛИ если позиция почти не меняется
        bool isNotMoving = Vector2.Distance(currentPosition, lastPosition) < minMovementThreshold;
        
        if (isAlmostStopped && isNotMoving)
        {
            // Кубик приземлился
            isFalling = false;
            
            // Сообщаем спаунеру что кубик приземлился
            if (CubeSpawner.Instance != null)
            {
                CubeSpawner.Instance.OnCubeLanded();
            }
            
            Debug.Log("FallingCubeTracker: Cube has landed");
            
            // Уничтожаем этот компонент так как он больше не нужен
            Destroy(this);
        }
        else
        {
            lastPosition = currentPosition;
        }
    }
}
