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
    
    [Header("FX")]
    [SerializeField] ParticleSystem mergeEffect;
    [SerializeField] GameObject deathEffectPrefab;
    
    [Header("Spec cubes sprites")]
    [SerializeField] GameObject deathCubeSprite;
    [SerializeField] GameObject growCubeSprite;
    [SerializeField] GameObject shrinkCubeSprite;
    [SerializeField] GameObject freezeCubeSprite;
    [SerializeField] GameObject vortexCubeSprite;
    
    
    private Rigidbody2D rb;
    private Collider2D cubeCollider;
    private TextMeshPro textMesh;
    private SpriteRenderer spriteRenderer;
    private bool canMerge = true;
    private float mergeCheckTimer = 0f;
    private float mergeCooldownTimer = 0f;
    private float originalFontSize;
    private Quaternion textOriginalRotation;
    
    public enum SpecialCubeType
    {
        None,
        Plus,   // x2
        Minus,  // x/2
        Death,  // destroy
        Grow,   // <> увеличивает кубик в 2 раза
        Shrink, // >< уменьшает кубик в 2 раза
        Freeze, // * замораживает кубик
        Vortex  // ~ создает вихрь
    }
    
    public int GetValue() => value;
    
    public bool IsSpecialCube() => isSpecialCube;
    
    public void SetCanMerge(bool canMerge)
    {
        this.canMerge = canMerge;
    }

    private void HideAllSpecCubesSprites()
    {
        deathCubeSprite.SetActive(false);
        growCubeSprite.SetActive(false);
        shrinkCubeSprite.SetActive(false);
        freezeCubeSprite.SetActive(false);
        vortexCubeSprite.SetActive(false); 
    }
    
    void Start()
    {
        HideAllSpecCubesSprites();
        Debug.Log($"Cube Start: value={value}, isSpecialCube={isSpecialCube}, specialType={specialType}");
        
        // Инициализируем цвета если массив пустой
        if (valueColors == null || valueColors.Length == 0)
        {
            valueColors = new Color[]
            {
                // 2 – 64
                new Color(0.92f, 0.84f, 0.52f), // 2  — мягкий тёплый жёлтый
                new Color(0.52f, 0.72f, 0.88f), // 4  — спокойный голубой
                new Color(0.92f, 0.62f, 0.42f), // 8  — приглушённый оранжевый
                new Color(0.48f, 0.58f, 0.82f), // 16 — холодный синий
                new Color(0.88f, 0.48f, 0.40f), // 32 — тёплый коралл
                new Color(0.46f, 0.78f, 0.68f), // 64 — мягкий мятный

                // 128 – 2048
                new Color(0.90f, 0.74f, 0.48f), // 128 — янтарный
                new Color(0.60f, 0.52f, 0.82f), // 256 — лавандово-синий
                new Color(0.88f, 0.56f, 0.64f), // 512 — розово-коралловый
                new Color(0.46f, 0.70f, 0.86f), // 1024 — небесный
                new Color(0.82f, 0.42f, 0.58f), // 2048 — мягкий малиновый

                // 4096 – 65536
                new Color(0.50f, 0.78f, 0.62f), // 4096 — зелёный чай
                new Color(0.66f, 0.52f, 0.84f), // 8192 — фиолетовый
                new Color(0.90f, 0.62f, 0.40f), // 16384 — тёплый апельсиновый
                new Color(0.42f, 0.62f, 0.84f), // 32768 — холодный синий
                new Color(0.86f, 0.46f, 0.46f), // 65536 — красный кирпич

                // 131072+
                new Color(0.48f, 0.78f, 0.74f), // 131072 — бирюза
                new Color(0.70f, 0.50f, 0.86f), // 262144 — фиолетовый
                new Color(0.92f, 0.82f, 0.50f), // 524288 — приглушённое золото
                new Color(0.40f, 0.52f, 0.78f), // 1048576 — глубокий синий
                new Color(0.88f, 0.48f, 0.70f), // 2097152 — розово-сливовый
            };
            
            Debug.Log($"Initialized valueColors with {valueColors.Length} colors");
        }
        
        cubeCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        textMesh = GetComponentInChildren<TextMeshPro>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        Debug.Log($"Cube.Awake: {gameObject.name} - SpriteRenderer: {spriteRenderer != null}, TextMesh: {textMesh != null}");
        
        // Проверяем обязательные компоненты
        if (spriteRenderer == null)
        {
            Debug.LogError("Cube: SpriteRenderer component is missing! Please add SpriteRenderer to the cube prefab.");
        }
        
        if (textMesh == null)
        {
            Debug.LogError("Cube: TextMeshPro component is missing! Please add TextMeshPro to the cube prefab.");
        }
        
        if (textMesh != null)
        {
            originalFontSize = textMesh.fontSize;
            textOriginalRotation = textMesh.transform.rotation;
        }
        
        // Устанавливаем тег для кубика
        if (gameObject.tag != "Cube")
        {
            gameObject.tag = "Cube";
        }
        
        UpdateVisual();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем контакт с полосой Game Over
        if (other.CompareTag("GameOverBar"))
        {
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.OnCubeEnterBar();
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        // Проверяем выход из контакта с полосой Game Over
        if (other.CompareTag("GameOverBar"))
        {
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.OnCubeExitBar();
            }
        }
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
        
        // Обновляем таймер кулдауна слияний
        if (mergeCooldownTimer > 0f)
        {
            mergeCooldownTimer -= Time.deltaTime;
            if (mergeCooldownTimer <= 0f)
            {
                canMerge = true;
                mergeCooldownTimer = 0f;
            }
        }
        
        // Держим текст в правильном положении
        KeepTextUpright();
    }
    
    void KeepTextUpright()
    {
        if (textMesh != null)
        {
            // Всегда держим текст в оригинальной ротации (не поворачиваем с кубиком)
            textMesh.transform.rotation = textOriginalRotation;
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
            HideAllSpecCubesSprites();
            Debug.LogWarning($"GetSpecialCubeText called on non-special cube! isSpecialCube={isSpecialCube}, specialType={specialType}");
            return "?";
        }
        
        switch (specialType)
        {
            case SpecialCubeType.Plus:
                return "X2";
            case SpecialCubeType.Minus:
                return "X / 2";
            case SpecialCubeType.Death:
                deathCubeSprite.SetActive(true);
                return "";
            case SpecialCubeType.Grow:
                growCubeSprite.SetActive(true);
                return "";
            case SpecialCubeType.Shrink:
                shrinkCubeSprite.SetActive(true);
                return "";
            case SpecialCubeType.Freeze:
                freezeCubeSprite.SetActive(true);
                return "";
            case SpecialCubeType.Vortex:
                vortexCubeSprite.SetActive(true);
                return "";
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
                return new Color(0.3f, 0.7f, 0.3f); // Менее яркий зеленый
            case SpecialCubeType.Minus:
                return new Color(0.7f, 0.3f, 0.3f); // Менее яркий красный
            case SpecialCubeType.Death:
                return Color.white; // Белый кубик смерти
            case SpecialCubeType.Grow:
                return new Color(0.3f, 0.3f, 0.8f); // Синий для увеличения
            case SpecialCubeType.Shrink:
                return new Color(0.8f, 0.6f, 0.2f); // Оранжевый для уменьшения
            case SpecialCubeType.Freeze:
                return new Color(0.7f, 0.9f, 1.0f); // Ледяной голубой
            case SpecialCubeType.Vortex:
                return new Color(0.5f, 0.3f, 0.8f); // Фиолетовый для вихря
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
        // Спецкубики выглядят как обычные - только цвет отличается
        // Никаких поворотов, масштабов или изменений шрифта
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
            // Проверка на разморозку при столкновении со спецкубиками Plus и Minus
            if ((this.specialType == SpecialCubeType.Plus || this.specialType == SpecialCubeType.Minus) && 
                !otherCube.canMerge)
            {
                UnfreezeCube(otherCube);
                return;
            }
            else if ((otherCube.specialType == SpecialCubeType.Plus || otherCube.specialType == SpecialCubeType.Minus) && 
                     !this.canMerge)
            {
                UnfreezeCube(this);
                return;
            }
            
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
            
            // Проверка на разморозку
            if (otherCube.GetValue() == this.value)
            {
                // Если один из кубиков заморожен, размораживаем его
                if (!this.canMerge && otherCube.canMerge)
                {
                    UnfreezeCube(this);
                    return;
                }
                else if (this.canMerge && !otherCube.canMerge)
                {
                    UnfreezeCube(otherCube);
                    return;
                }
            }
            
            // Обычное слияние
            if (otherCube.GetValue() == this.value && 
                otherCube.canMerge && 
                this.canMerge)
            {
                MergeCubes(otherCube);
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
                if (CubeSpawner.Instance != null)
            {
                CubeSpawner.Instance.UpdateMaxCubeValue(otherCube.value);
            }
                
                // Добавляем очки за спецкубик
                ScoreManager.AddSpecialCubeScore("Plus", otherCube.value);
                
                // Воспроизводим эффект на кубике, с которым взаимодействовали
                otherCube.PlayMergeEffect();
                
                Debug.Log($"Plus cube: {originalValue} -> {otherCube.value}");
                break;
                
            case SpecialCubeType.Minus:
                // Делим значение другого кубика на 2, но не меньше 2
                originalValue = otherCube.value;
                int newValue = otherCube.value / 2;
                
                // Добавляем очки за спецкубик
                ScoreManager.AddSpecialCubeScore("Minus", originalValue);
                
                if (newValue < 2)
                {
                    // Если результат меньше 2, уничтожаем кубик с эффектом смерти
                    Debug.Log($"Minus cube: {originalValue} -> destroyed (too small)");
                    PlayDeathEffect(otherCube);
                    Destroy(otherCube.gameObject);
                }
                else
                {
                    // Иначе устанавливаем новое значение
                    otherCube.value = newValue;
                    otherCube.UpdateVisual();
                    Debug.Log($"Minus cube: {originalValue} -> {otherCube.value}");
                    
                    // Воспроизводим эффект на кубике, с которым взаимодействовали
                    otherCube.PlayMergeEffect();
                }
                break;
                
            case SpecialCubeType.Death:
                // Добавляем очки за спецкубик
                ScoreManager.AddSpecialCubeScore("Death", otherCube.value);
                
                // Создаем эффект смерти между кубиками
                PlayDeathEffect(otherCube);
                
                // Уничтожаем другой кубик
                Debug.Log($"Death cube destroyed {otherCube.value}");
                Destroy(otherCube.gameObject);
                break;
                
            case SpecialCubeType.Grow:
                // Добавляем очки за спецкубик
                ScoreManager.AddSpecialCubeScore("Grow", otherCube.value);
                
                // Увеличиваем физический размер другого кубика в 2 раза
                Vector3 currentScale = otherCube.transform.localScale;
                Vector3 newScale = currentScale * 2f;
                
                // Ограничиваем максимальный размер чтобы кубик не стал слишком большим
                float maxScale = 3f;
                if (newScale.x > maxScale) newScale = Vector3.one * maxScale;
                
                otherCube.transform.localScale = newScale;
                Debug.Log($"Grow cube (<>): scale {currentScale} -> {newScale}");
                break;
                
            case SpecialCubeType.Shrink:
                // Добавляем очки за спецкубик
                ScoreManager.AddSpecialCubeScore("Shrink", otherCube.value);
                
                // Уменьшаем физический размер другого кубика в 2 раза
                currentScale = otherCube.transform.localScale;
                newScale = currentScale / 2f;
                
                // Ограничиваем минимальный размер чтобы кубик не стал слишком маленьким
                float minScale = 0.3f;
                if (newScale.x < minScale)
                {
                    // Если результат меньше минимума, уничтожаем кубик с эффектом смерти
                    Debug.Log($"Shrink cube (><): scale {currentScale} -> destroyed (too small)");
                    PlayDeathEffect(otherCube);
                    Destroy(otherCube.gameObject);
                }
                else
                {
                    // Иначе устанавливаем новый размер
                    otherCube.transform.localScale = newScale;
                    Debug.Log($"Shrink cube (><): scale {currentScale} -> {newScale}");
                }
                break;
                
            case SpecialCubeType.Freeze:
                // Добавляем очки за спецкубик
                ScoreManager.AddSpecialCubeScore("Freeze", otherCube.value);
                
                // Замораживаем другой кубик
                FreezeCube(otherCube);
                break;
                
            case SpecialCubeType.Vortex:
                // Добавляем очки за спецкубик
                ScoreManager.AddSpecialCubeScore("Vortex", otherCube.value);
                
                // Создаем вихрь между кубиками
                CreateVortexEffect(otherCube);
                break;
                
            default:
                Debug.LogWarning($"Unknown special cube type: {specialType}");
                canMerge = true; // Разблокируем если тип неизвестен
                return;
        }
        
        // Уничтожаем спецкубик после использования
        Destroy(gameObject);
    }
    
    void FreezeCube(Cube targetCube)
    {
        if (targetCube == null) return;
        
        // Делаем кубик ледяного цвета
        Color iceColor = new Color(0.7f, 0.9f, 1.0f); // Ледяной голубой
        if (targetCube.spriteRenderer != null)
        {
            targetCube.spriteRenderer.color = iceColor;
        }
        
        // Делаем текст немного темнее чем цвет кубика
        if (targetCube.textMesh != null)
        {
            Color darkerIceColor = Color.black; // Темнее ледяного цвета
            targetCube.textMesh.color = darkerIceColor;
        }
        
        // Делаем Rigidbody статичным
        if (targetCube.rb != null)
        {
            targetCube.rb.bodyType = RigidbodyType2D.Static;
        }
        
        // Отключаем возможность слияния для замороженного кубика
        targetCube.canMerge = false;
        
        // Включаем зацикленный эффект заморозки
        PlayFreezeEffect(targetCube);
        
        Debug.Log($"Freeze cube (*): froze cube with value {targetCube.value}");
    }
    
    void CreateVortexEffect(Cube otherCube)
    {
        if (otherCube == null) return;
        
        // Вычисляем позицию между двумя кубиками
        Vector3 vortexPosition = (transform.position + otherCube.transform.position) / 2f;
        
        // Создаем вихрь
        GameObject vortexObj = new GameObject("Vortex");
        vortexObj.transform.position = vortexPosition;
        
        Vortex vortex = vortexObj.AddComponent<Vortex>();
        
        // Активируем вихрь
        vortex.ActivateVortex();
        
        // Воспроизводим эффект создания вихря
        PlayMergeEffect();
        
        Debug.Log($"Vortex cube (~): created vortex at {vortexPosition}");
    }
    
    void UnfreezeCube(Cube targetCube)
    {
        if (targetCube == null) return;
        
        // Возвращаем обычный цвет в зависимости от значения
        targetCube.UpdateVisual();
        
        // Возвращаем динамическую физику
        if (targetCube.rb != null)
        {
            targetCube.rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // Включаем возможность слияния
        targetCube.canMerge = true;
        
        // Останавливаем эффект заморозки
        StopFreezeEffect(targetCube);
        
        Debug.Log($"Unfreeze cube: unfroze cube with value {targetCube.value}");
    }
    
    void PlayDeathEffect(Cube otherCube)
    {
        if (otherCube == null) return;
        
        // Вычисляем позицию между двумя кубиками
        Vector3 deathPosition = (transform.position + otherCube.transform.position) / 2f;
        
        if (deathEffectPrefab != null)
        {
            // Создаем эффект смерти из префаба
            GameObject deathEffect = Instantiate(deathEffectPrefab, deathPosition, Quaternion.identity);
            var deathEffectParticle = deathEffect.GetComponent<ParticleSystem>();
            deathEffectParticle.Play();
            // Уничтожаем эффект через 1 секунду
            Destroy(deathEffect, 1f);
            
            Debug.Log($"PlayDeathEffect: created death effect from prefab at {deathPosition}");
        }
        else
        {
            // Если префаб не задан, используем существующий mergeEffect
            if (mergeEffect != null)
            {
                // Создаем инстанс mergeEffect
                GameObject deathEffect = Instantiate(mergeEffect.gameObject, deathPosition, Quaternion.identity);
                
                // Уничтожаем через 1 секунду
                Destroy(deathEffect, 1f);
                
                Debug.Log($"PlayDeathEffect: created mergeEffect instance at {deathPosition}");
            }
            else
            {
                Debug.LogWarning("PlayDeathEffect: deathEffectPrefab is null and mergeEffect is also null");
            }
        }
    }
    
    void PlayFreezeEffect(Cube targetCube)
    {
        if (targetCube.mergeEffect != null)
        {
            // Устанавливаем ледяной цвет для эффекта
            Color iceColor = new Color(0.7f, 0.9f, 1.0f); // Ледяной голубой
            
            // Настраиваем эффект для заморозки
            var main = targetCube.mergeEffect.main;
            main.startColor = iceColor;
            main.loop = true; // Зацикливаем эффект
            
            // Воспроизводим эффект
            targetCube.mergeEffect.Play();
            
            Debug.Log($"PlayFreezeEffect: started loop effect with ice color");
        }
        else
        {
            Debug.LogWarning("PlayFreezeEffect: mergeEffect is null");
        }
    }
    
    void StopFreezeEffect(Cube targetCube)
    {
        if (targetCube.mergeEffect != null)
        {
            // Останавливаем эффект
            targetCube.mergeEffect.Stop();
            
            // Сбрасываем loop в false
            var main = targetCube.mergeEffect.main;
            main.loop = false;
            
            Debug.Log($"StopFreezeEffect: stopped freeze effect");
        }
        else
        {
            Debug.LogWarning("StopFreezeEffect: mergeEffect is null");
        }
    }
    
    void PlayMergeEffect()
    {
        if (mergeEffect != null)
        {
            // Получаем цвет текущего кубика
            Color cubeColor;
            if (isSpecialCube)
            {
                cubeColor = GetSpecialCubeColor();
            }
            else
            {
                if (valueColors != null && valueColors.Length > 0)
                {
                    int colorIndex = GetColorIndex();
                    if (colorIndex < valueColors.Length)
                    {
                        cubeColor = valueColors[colorIndex];
                    }
                    else
                    {
                        cubeColor = valueColors[valueColors.Length - 1];
                    }
                }
                else
                {
                    cubeColor = Color.white;
                }
            }
            
            // Устанавливаем цвет частиц
            var main = mergeEffect.main;
            main.startColor = cubeColor;
            
            // Воспроизводим эффект
            mergeEffect.Play();
            
            Debug.Log($"PlayMergeEffect: played with color {cubeColor}");
        }
        else
        {
            Debug.LogWarning("PlayMergeEffect: mergeEffect is null");
        }
    }
    
    void MergeCubes(Cube otherCube)
    {
        // Блокируем дальнейшие слияния
        canMerge = false;
        otherCube.canMerge = false;
        
        // Удваиваем значение
        value *= 2;
        UpdateVisual();
        
        // Добавляем очки за слияние
        ScoreManager.AddMergeScore(value);
        
        // Воспроизводим эффект слияния с цветом кубика
        PlayMergeEffect();
        
        // Обновляем максимальное значение в спаунере
        if (CubeSpawner.Instance != null)
        {
            CubeSpawner.Instance.UpdateMaxCubeValue(value);
        }
        
        // Уничтожаем другой кубик
        Destroy(otherCube.gameObject);
        
        // Небольшой эффект отталкивания для разделения кубиков
        rb.AddForce(Vector2.up * 1f, ForceMode2D.Impulse);
        
        // Устанавливаем таймер кулдауна вместо корутины
        mergeCooldownTimer = mergeCooldown;
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
        
        // Восстанавливаем оригинальный масштаб кубика
        transform.localScale = new Vector3(1f, 1f, 1f);
        
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
                otherCube.GetValue() == this.value && 
                otherCube.canMerge && 
                otherCube != this)
            {
                // Находим кубик с которым можно слиться
                MergeCubes(otherCube);
                break; // Сливаемся только с одним за раз
            }
        }
    }
}
