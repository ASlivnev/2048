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
    
    public int Value => value;
    
    void Start()
    {
        // Инициализируем цвета если массив пустой
        if (valueColors == null || valueColors.Length == 0)
        {
            valueColors = new Color[]
            {
                Color.white,    // 2
                new Color(1f, 0.8f, 0.4f), // 4
                new Color(1f, 0.6f, 0.2f), // 8
                new Color(1f, 0.4f, 0.1f), // 16
                new Color(1f, 0.2f, 0.1f), // 32
                new Color(0.8f, 0.1f, 0.1f), // 64
                new Color(0.6f, 0.1f, 0.8f), // 128
                new Color(0.4f, 0.1f, 1f), // 256
                new Color(0.2f, 0.1f, 1f), // 512
                new Color(0.1f, 0.1f, 1f), // 1024
                new Color(0.1f, 0f, 0.8f)  // 2048
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
}
