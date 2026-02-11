using UnityEngine;

public class FallingCubeTracker : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isFalling = true;
    private bool hasCollided = false;
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
        
        // Если уже было столкновение - разрешаем спаун при минимальном движении
        if (hasCollided)
        {
            bool isAlmostStopped = Mathf.Abs(velocity.y) < 0.5f && 
                                   Mathf.Abs(velocity.x) < 0.5f;
            
            if (isAlmostStopped)
            {
                // Кубик достаточно замедлился после столкновения
                isFalling = false;
                
                if (CubeSpawner.Instance != null)
                {
                    CubeSpawner.Instance.OnCubeLanded();
                }
                
                Destroy(this);
                return;
            }
        }
        else
        {
            // Проверяем если кубик почти остановился (старая логика для первого кубика)
            bool isAlmostStopped = Mathf.Abs(velocity.y) < 0.1f && 
                                   Mathf.Abs(velocity.x) < 0.1f;
            
            bool isNotMoving = Vector2.Distance(currentPosition, lastPosition) < minMovementThreshold;
            
            if (isAlmostStopped && isNotMoving)
            {
                // Первый кубик приземлился без столкновений
                isFalling = false;
                
                if (CubeSpawner.Instance != null)
                {
                    CubeSpawner.Instance.OnCubeLanded();
                }
                
                Destroy(this);
                return;
            }
        }
        
        lastPosition = currentPosition;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Отмечаем что было столкновение
        if (!hasCollided && isFalling)
        {
            hasCollided = true;
        }
    }
}
