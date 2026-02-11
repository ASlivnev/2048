using UnityEngine;

public class Vortex : MonoBehaviour
{
    [Header("Vortex Settings")]
    [SerializeField] float vortexRadius = 5f;
    [SerializeField] float vortexDuration = 3f;
    [SerializeField] float rotationSpeed = 360f; // градусов в секунду
    [SerializeField] float pullForce = 10f;
    [SerializeField] float scatterForce = 15f;
    [SerializeField] float upwardForce = 5f;
    
    [Header("Visual Effects")]
    [SerializeField] GameObject vortexEffect;
    [SerializeField] Color vortexColor = new Color(0.5f, 0.5f, 1f, 0.3f);
    
    private bool isActive = false;
    private float currentDuration = 0f;
    private GameObject visualEffect;
    private CircleCollider2D vortexCollider;
    private Rigidbody2D[] pulledCubes = new Rigidbody2D[50]; // Массив для хранения притягиваемых кубиков
    private int pulledCubeCount = 0;
    
    void Start()
    {
        // Создаем коллайдер для области вихря
        vortexCollider = gameObject.AddComponent<CircleCollider2D>();
        vortexCollider.isTrigger = true;
        vortexCollider.radius = vortexRadius;
        
        // Создаем визуальный эффект
        CreateVisualEffect();
    }
    
    void CreateVisualEffect()
    {
        if (vortexEffect != null)
        {
            visualEffect = Instantiate(vortexEffect, transform.position, Quaternion.identity);
            visualEffect.transform.SetParent(transform);
        }
        else
        {
            // Создаем простой визуальный эффект если не задан
            CreateSimpleVortexEffect();
        }
    }
    
    void CreateSimpleVortexEffect()
    {
        // Создаем SpriteRenderer для визуализации вихря
        GameObject vortexVisual = new GameObject("VortexVisual");
        vortexVisual.transform.SetParent(transform);
        
        SpriteRenderer spriteRenderer = vortexVisual.AddComponent<SpriteRenderer>();
        spriteRenderer.color = vortexColor;
        
        // Создаем круг спрайт
        spriteRenderer.sprite = CreateCircleSprite();
        spriteRenderer.transform.localScale = new Vector3(vortexRadius * 2, vortexRadius * 2, 1f);
        
        visualEffect = vortexVisual;
    }
    
    Sprite CreateCircleSprite()
    {
        // Создаем простой круг спрайт
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        
        Vector2 center = new Vector2(32, 32);
        float radius = 30f;
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance < radius)
                {
                    float alpha = 1f - (distance / radius);
                    pixels[y * 64 + x] = new Color(vortexColor.r, vortexColor.g, vortexColor.b, vortexColor.a * alpha);
                }
                else
                {
                    pixels[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }
    
    void Update()
    {
        if (isActive)
        {
            currentDuration += Time.deltaTime;
            
            // Вращаем визуальный эффект
            if (visualEffect != null)
            {
                visualEffect.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            }
            
            // Притягиваем все кубики в Update вместо корутины
            PullAllCubes();
            
            // Завершаем вихрь через заданное время
            if (currentDuration >= vortexDuration)
            {
                ScatterCubes();
                DeactivateVortex();
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isActive && other.CompareTag("Cube"))
        {
            Rigidbody2D cubeRb = other.GetComponent<Rigidbody2D>();
            if (cubeRb != null && pulledCubeCount < pulledCubes.Length)
            {
                // Добавляем кубик в массив для притяжения
                pulledCubes[pulledCubeCount] = cubeRb;
                pulledCubeCount++;
            }
        }
    }
    
    void PullAllCubes()
    {
        // Проходим по всем притягиваемым кубикам
        for (int i = pulledCubeCount - 1; i >= 0; i--)
        {
            Rigidbody2D cubeRb = pulledCubes[i];
            
            if (cubeRb == null)
            {
                // Удаляем null из массива
                pulledCubes[i] = pulledCubes[pulledCubeCount - 1];
                pulledCubeCount--;
                continue;
            }
            
            Transform cubeTransform = cubeRb.transform;
            Vector2 direction = (transform.position - cubeTransform.position).normalized;
            float distance = Vector2.Distance(transform.position, cubeTransform.position);
            
            if (distance > 0.5f)
            {
                // Тянем кубик к центру
                cubeRb.AddForce(direction * pullForce);
                
                // Добавляем вращение вокруг центра
                Vector2 tangent = new Vector2(-direction.y, direction.x);
                cubeRb.AddForce(tangent * pullForce * 0.5f);
            }
        }
    }
    
    void ScatterCubes()
    {
        // Находим все кубики в радиусе вихря
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, vortexRadius);
        
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Cube"))
            {
                Rigidbody2D cubeRb = hitCollider.GetComponent<Rigidbody2D>();
                if (cubeRb != null)
                {
                    // Разбрасываем кубики в случайных направлениях
                    Vector2 scatterDirection = Random.insideUnitCircle.normalized;
                    cubeRb.AddForce(scatterDirection * scatterForce, ForceMode2D.Impulse);
                    cubeRb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);
                }
            }
        }
    }
    
    public void ActivateVortex()
    {
        if (!isActive)
        {
            isActive = true;
            currentDuration = 0f;
            
            // Сообщаем GameOverManager что вихрь активирован
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.OnVortexActivated();
            }
            
            // Показываем визуальный эффект
            if (visualEffect != null)
            {
                visualEffect.SetActive(true);
            }
            
        }
    }
    
    void DeactivateVortex()
    {
        isActive = false;
        
        // Сообщаем GameOverManager что вихрь деактивирован
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.OnVortexDeactivated();
        }
        
        // Очищаем массив притягиваемых кубиков
        for (int i = 0; i < pulledCubeCount; i++)
        {
            pulledCubes[i] = null;
        }
        pulledCubeCount = 0;
        
        // Скрываем визуальный эффект
        if (visualEffect != null)
        {
            visualEffect.SetActive(false);
        }
        
    }
    
    void OnDrawGizmosSelected()
    {
        // Рисуем радиус вихря в редакторе
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, vortexRadius);
    }
    
    // Публичный метод для вызова извне
    public void TriggerVortex()
    {
        ActivateVortex();
    }
}
