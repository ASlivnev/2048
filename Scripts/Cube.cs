using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cube : MonoBehaviour
{
    [Header("Cube Settings")]
    [SerializeField] private int value = 2;
    [SerializeField] private Color[] valueColors;
    [SerializeField] private float mergeCooldown = 0.3f;
    [SerializeField] private float mergeCheckInterval = 1f;
    
    [Header("Special Cubes")]
    [SerializeField] private bool isSpecialCube = false;
    [SerializeField] private SpecialCubeType specialType = SpecialCubeType.None;
    
    private Rigidbody2D rb;
    private Collider2D cubeCollider;
    private TextMeshPro textMesh;
    private SpriteRenderer spriteRenderer;
    private bool canMerge = true;
    private float mergeCheckTimer = 0f;
    private float originalFontSize;
    
    public enum SpecialCubeType
    {
        None,
        Plus,   // x2
        Minus,  // x/2
        Death   // destroy
    }
    
    public int Value => value;
    
    void Start()
    {
        Debug.Log($"Cube Start: value={value}, isSpecialCube={isSpecialCube}, specialType={specialType}");
        
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
        
        cubeCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        textMesh = GetComponentInChildren<TextMeshPro>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (textMesh != null)
            originalFontSize = textMesh.fontSize;
            
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Устанавливаем тег для кубика
        if (gameObject.tag != "Cube")
        {
            gameObject.tag = "Cube";
        }
        
        UpdateVisual();
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
            if (isSpecialCube)
            {
                textMesh.text = GetSpecialCubeText();
            }
            else
            {
                textMesh.text = FormatValue(value);
            }
        }
        
        // Устанавливаем цвет
        Color targetColor;
        
        if (isSpecialCube)
        {
            targetColor = GetSpecialCubeColor();
        }
        else
        {
            // Для обычных кубиков используем цвет по умолчанию если массив пуст
            if (valueColors != null && valueColors.Length > 0)
            {
                int colorIndex = GetColorIndex();
                if (colorIndex < valueColors.Length)
                {
                    targetColor = valueColors[colorIndex];
                }
                else
                {
                    targetColor = valueColors[valueColors.Length - 1];
                }
            }
            else
            {
                // Стандартный цвет если массив не инициализирован
                targetColor = Color.white;
            }
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
        
        // Применяем цвет текста
        if (textMesh != null)
        {
            if (isSpecialCube)
            {
                // Разный цвет текста для разных спецкубиков
                if (specialType == SpecialCubeType.Death)
                {
                    textMesh.color = Color.black; // Черный текст для смерти
                }
                else
                {
                    textMesh.color = Color.white; // Белый текст для плюса и минуса
                }
            }
            else
            {
                textMesh.color = GetTextColor();
            }
        }
    }
    
    private Color GetTextColor()
    {
        // Системность цвета текста по группам значений
        if (value < 1000)
        {
            // Маленькие значения - черный текст
            return Color.black;
        }
        else if (value < 1000000)
        {
            // Тысячи 
            return Color.white;
        }
        else if (value < 1000000000)
        {
            // Миллионы - фиолетовый текст
            return Color.black;
        }
        else if (value < 1000000000000)
        {
            // Миллиарды - зеленый текст
            return  Color.white;
        }
        else if (value < 1000000000000000)
        {
            // Триллионы - оранжевый текст
            return Color.black;
        }
        else if (value < 1000000000000000000)
        {
            // Квадриллионы - красный текст
            return new Color(0.9f, 0.1f, 0.1f);
        }
        else
        {
            // Квинтиллионы и выше - золотой текст
            return new Color(1f, 0.8f, 0.2f);
        }
    }
    
    string GetSpecialCubeText()
    {
        if (!isSpecialCube)
        {
            Debug.LogWarning($"GetSpecialCubeText called on non-special cube! isSpecialCube={isSpecialCube}, specialType={specialType}");
            return "?";
        }
        
        switch (specialType)
        {
            case SpecialCubeType.Plus:
                return "+";
            case SpecialCubeType.Minus:
                return "-";
            case SpecialCubeType.Death:
                return "×";
            default:
                Debug.LogWarning($"GetSpecialCubeText: Unknown specialType={specialType}");
                return "?";
        }
    }
    
    Color GetSpecialCubeColor()
    {
        switch (specialType)
        {
            case SpecialCubeType.Plus:
                return Color.green;
            case SpecialCubeType.Minus:
                return Color.red;
            case SpecialCubeType.Death:
                return Color.white; // Белый кубик смерти
            default:
                return Color.white;
        }
    }
    
    public void SetSpecialCube(SpecialCubeType type)
    {
        isSpecialCube = true;
        specialType = type;
        Debug.Log($"SetSpecialCube: type={type}, isSpecialCube={isSpecialCube}");
        
        // Настраиваем внешний вид для спецкубиков
        SetupSpecialCubeAppearance();
        
        UpdateVisual();
    }
    
    void SetupSpecialCubeAppearance()
    {
        // Делаем кубик ромбовидным через поворот спрайта
        if (spriteRenderer != null)
        {
            // Создаем материал с поддержкой скругленных углов
            Material roundedMaterial = new Material(Shader.Find("Sprites/Default"));
            spriteRenderer.material = roundedMaterial;
            
            // Добавляем эффект скругления через цвет и прозрачность краев
            Color currentColor = spriteRenderer.color;
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.95f);
            
            // Поворачиваем спрайт на 45 градусов чтобы получился ромб
            spriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            
            // Немного уменьшаем масштаб чтобы ромб не был слишком большим
            transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            
            // Возвращаем текст в нормальное положение
            if (textMesh != null)
            {
                textMesh.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        
        // Увеличиваем шрифт для спецкубиков - отключаем auto-size и устанавливаем размер
        if (textMesh != null)
        {
            // Отключаем автоматический размер
            textMesh.enableAutoSizing = false;
            
            // Устанавливаем большой размер шрифта
            textMesh.fontSize = Mathf.Max(8f, originalFontSize * 1.5f);
            textMesh.fontStyle = FontStyles.Bold;
            
            // Увеличиваем сам текстовый объект для лучшей видимости
            textMesh.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            
            // Выравниваем текст по центру вертикально
            textMesh.alignment = TextAlignmentOptions.Center;
            
            // Корректируем позицию текста для лучшего центрирования
            Vector3 textPosition = textMesh.transform.localPosition;
            textPosition.y = 0f; // Устанавливаем точно в центр по Y
            textMesh.transform.localPosition = textPosition;
        }
    }
    
    private Color GetContrastColor(Color backgroundColor)
    {
        // Вычисляем яркость фона
        float brightness = (backgroundColor.r * 0.299f + backgroundColor.g * 0.587f + backgroundColor.b * 0.114f);
        
        // Возвращаем черный текст для светлых фонов и белый для темных
        return brightness > 0.5f ? Color.black : Color.white;
    }
    
    string FormatValue(int val)
    {
        if (val < 1000)
            return val.ToString();
        else if (val < 1000000)
            return (val / 1000).ToString() + " К";
        else if (val < 1000000000)
            return (val / 1000000).ToString() + " М";
        else if (val < 1000000000000)
            return (val / 1000000000).ToString() + " Б";
        else if (val < 1000000000000000)
            return (val / 1000000000000).ToString() + " Т";
        else if (val < 1000000000000000000)
            return (val / 1000000000000000).ToString() + " Кк";
        else
            return (val / 1000000000000000000).ToString() + " Ккк";
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
        Cube otherCube = collision.gameObject.GetComponent<Cube>();
        
        if (otherCube != null && otherCube != this)
        {
            // Если это спецкубик
            if (isSpecialCube)
            {
                HandleSpecialCubeCollision(otherCube);
                return;
            }
            
            // Если другой кубик - спецкубик
            if (otherCube.isSpecialCube)
            {
                otherCube.HandleSpecialCubeCollision(this);
                return;
            }
            
            // Обычное слияние
            if (otherCube.Value == this.value && 
                otherCube.canMerge && 
                this.canMerge)
            {
                StartCoroutine(MergeCubes(otherCube));
            }
        }
    }
    
    void HandleSpecialCubeCollision(Cube otherCube)
    {
        if (!canMerge || otherCube == null || isSpecialCube == false) return;
        
        // Блокируем повторные срабатывания
        canMerge = false;
        
        switch (specialType)
        {
            case SpecialCubeType.Plus:
                // Удваиваем значение другого кубика
                int originalValue = otherCube.value;
                otherCube.value *= 2;
                otherCube.UpdateVisual();
                CubeSpawner.UpdateMaxCubeValue(otherCube.value);
                Debug.Log($"Plus cube: {originalValue} -> {otherCube.value}");
                break;
                
            case SpecialCubeType.Minus:
                // Делим значение другого кубика на 2, но не меньше 2
                originalValue = otherCube.value;
                int newValue = otherCube.value / 2;
                
                if (newValue < 2)
                {
                    // Если результат меньше 2, уничтожаем кубик
                    Debug.Log($"Minus cube: {originalValue} -> destroyed (too small)");
                    Destroy(otherCube.gameObject);
                }
                else
                {
                    // Иначе устанавливаем новое значение
                    otherCube.value = newValue;
                    otherCube.UpdateVisual();
                    Debug.Log($"Minus cube: {originalValue} -> {otherCube.value}");
                }
                break;
                
            case SpecialCubeType.Death:
                // Уничтожаем другой кубик
                Debug.Log($"Death cube destroyed {otherCube.value}");
                Destroy(otherCube.gameObject);
                break;
                
            default:
                Debug.LogWarning($"Unknown special cube type: {specialType}");
                canMerge = true; // Разблокируем если тип неизвестен
                return;
        }
        
        // Уничтожаем спецкубик после использования
        Destroy(gameObject);
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
        isSpecialCube = false;
        specialType = SpecialCubeType.None;
        Debug.Log($"SetValue: value={newValue}, isSpecialCube={isSpecialCube}");
        
        // Восстанавливаем оригинальный размер шрифта и масштаб для обычных кубиков
        if (textMesh != null && originalFontSize > 0)
        {
            // Включаем обратно auto-size если был включен
            textMesh.enableAutoSizing = true;
            textMesh.fontSize = originalFontSize;
            textMesh.fontStyle = FontStyles.Normal;
            
            // Восстанавливаем масштаб текста
            textMesh.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        
        // Восстанавливаем оригинальный масштаб и поворот кубика
        transform.localScale = new Vector3(1f, 1f, 1f);
        if (spriteRenderer != null)
        {
            spriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        
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
