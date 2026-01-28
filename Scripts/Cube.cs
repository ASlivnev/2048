using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cube : MonoBehaviour
{
    [SerializeField] private int value = 2;
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D cubeCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Colors for different values")]
    [SerializeField] private Color[] valueColors;
    
    private bool canMerge = true;
    private float mergeCooldown = 0.1f;
    private static float mergeCheckInterval = 1f; // Проверка каждую секунду
    private float mergeCheckTimer = 0f;
    
    public int Value => value;
    
    void Start()
    {
        // Инициализируем цвета если массив пустой
        if (valueColors == null || valueColors.Length == 0)
        {
            valueColors = new Color[]
            {
                // 2 – 64 (яркий старт)
                new Color(0.95f, 0.85f, 0.35f), // 2  — тёплый жёлтый
                new Color(0.35f, 0.75f, 0.95f), // 4  — холодный голубой
                new Color(0.95f, 0.55f, 0.30f), // 8  — оранжевый
                new Color(0.45f, 0.55f, 0.95f), // 16 — синий
                new Color(0.90f, 0.35f, 0.25f), // 32 — красно-оранжевый
                new Color(0.35f, 0.85f, 0.70f), // 64 — мятный

                // 128 – 2048
                new Color(0.95f, 0.75f, 0.40f), // 128 — янтарный
                new Color(0.55f, 0.45f, 0.95f), // 256 — фиолетово-синий
                new Color(0.90f, 0.45f, 0.65f), // 512 — розово-коралловый
                new Color(0.30f, 0.75f, 0.95f), // 1024 — небесный
                new Color(0.85f, 0.30f, 0.55f), // 2048 — малиновый

                // 4096 – 65536 (более «редкие» значения)
                new Color(0.35f, 0.95f, 0.60f), // 4096 — салатово-зелёный
                new Color(0.65f, 0.35f, 0.95f), // 8192 — фиолетовый
                new Color(0.95f, 0.55f, 0.20f), // 16384 — насыщенный оранжевый
                new Color(0.20f, 0.65f, 0.95f), // 32768 — холодный синий
                new Color(0.95f, 0.30f, 0.30f), // 65536 — красный

                // 131072+ (почти «легендарные»)
                new Color(0.30f, 0.90f, 0.80f), // 131072 — бирюзовый
                new Color(0.80f, 0.30f, 0.95f), // 262144 — неоновый фиолетовый
                new Color(0.95f, 0.85f, 0.25f), // 524288 — золото
                new Color(0.25f, 0.45f, 0.95f), // 1048576 — глубокий синий
                new Color(0.95f, 0.25f, 0.70f), // 2097152 — яркий розовый
            };
            
            Debug.Log($"Initialized valueColors with {valueColors.Length} colors");
        }
        
        UpdateVisual();
        
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
            
        if (cubeCollider == null)
            cubeCollider = GetComponent<Collider2D>();
            
        if (textMesh == null)
            textMesh = GetComponentInChildren<TextMeshPro>();
            
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
    }
    
    void Update()
    {
        // Периодическая проверка слияний
        mergeCheckTimer += Time.deltaTime;
        if (mergeCheckTimer >= mergeCheckInterval)
        {
            mergeCheckTimer = 0f;
            CheckForMerges();
        }
    }
    
    void UpdateVisual()
    {
        // Обновляем текст
        if (textMesh != null)
        {
            textMesh.text = value.ToString();
        }
        
        // Устанавливаем цвет в зависимости от значения
        Color targetColor = Color.white;
        
        Debug.Log($"UpdateVisual: value={value}, valueColors.Length={valueColors?.Length}");
        
        if (valueColors != null && valueColors.Length > 0)
        {
            int colorIndex = GetColorIndex();
            Debug.Log($"UpdateVisual: colorIndex={colorIndex}, valueColors[colorIndex]={valueColors[colorIndex]}");
            
            if (colorIndex < valueColors.Length)
            {
                targetColor = valueColors[colorIndex];
            }
            else
            {
                // Если значение больше 2048, используем последний цвет
                targetColor = valueColors[valueColors.Length - 1];
                Debug.Log($"UpdateVisual: Using last color: {targetColor}");
            }
        }
        else
        {
            Debug.Log("UpdateVisual: valueColors is null or empty!");
        }
        
        // Применяем цвет к спрайту кубика
        if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
            Debug.Log($"Cube value: {value}, Final Color: {targetColor}");
        }
        else
        {
            Debug.Log("UpdateVisual: spriteRenderer is null!");
        }
        
        // Применяем контрастный цвет к тексту
        if (textMesh != null)
        {
            textMesh.color = GetContrastColor(targetColor);
        }
    }
    
    private Color GetContrastColor(Color backgroundColor)
    {
        // Вычисляем яркость фона
        float brightness = (backgroundColor.r * 0.299f + backgroundColor.g * 0.587f + backgroundColor.b * 0.114f);
        
        // Возвращаем черный текст для светлых фонов и белый для темных
        return brightness > 0.5f ? Color.black : Color.white;
    }
    
    private int GetColorIndex()
    {
        // Возвращает индекс цвета на основе значения (2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048)
        if (value < 2) return 0;
        
        int index = 0;
        int currentValue = 2;
        while (currentValue < value && index < 20)
        {
            currentValue *= 2;
            index++;
        }
        Debug.Log($"GetColorIndex for value {value}: index {index}");
        return index;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canMerge) return;
        
        Cube otherCube = collision.gameObject.GetComponent<Cube>();
        
        if (otherCube != null && otherCube.Value == this.value && otherCube.canMerge)
        {
            StartCoroutine(MergeCubes(otherCube));
        }
    }
    
    IEnumerator MergeCubes(Cube otherCube)
    {
        // Блокируем дальнейшие слияния
        canMerge = false;
        otherCube.canMerge = false;
        
        // Удваиваем значение
        value *= 2;
        UpdateVisual();
        
        // Обновляем максимальное значение в спаунере
        CubeSpawner.UpdateMaxCubeValue(value);
        
        // Уничтожаем другой кубик
        Destroy(otherCube.gameObject);
        
        // Небольшой эффект отталкивания для разделения кубиков
        rb.AddForce(Vector2.up * 1f, ForceMode2D.Impulse);
        
        // Короткая пауза для предотвращения мгновенных повторных слияний
        yield return new WaitForSeconds(mergeCooldown);
        canMerge = true;
    }
    
    public void SetValue(int newValue)
    {
        value = newValue;
        UpdateVisual();
    }
    
    
    void CheckForMerges()
    {
        if (cubeCollider == null || !canMerge) return;
        
        // Получаем все коллайдеры в точке контакта
        Collider2D[] contacts = new Collider2D[10];
        int contactCount = cubeCollider.OverlapCollider(new ContactFilter2D().NoFilter(), contacts);
        
        for (int i = 0; i < contactCount; i++)
        {
            Collider2D otherCollider = contacts[i];
            if (otherCollider == null || otherCollider == cubeCollider) continue;
            
            Cube otherCube = otherCollider.GetComponent<Cube>();
            if (otherCube != null && 
                otherCube.Value == this.value && 
                otherCube.canMerge && 
                otherCube != this)
            {
                // Находим кубик с которым можно слиться
                StartCoroutine(MergeCubes(otherCube));
                break; // Сливаемся только с одним за раз
            }
        }
    }
}
