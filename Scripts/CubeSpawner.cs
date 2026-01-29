using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private float spawnYPosition = 4.5f;
    [SerializeField] private float minXPosition = -3f;
    [SerializeField] private float maxXPosition = 3f;
    [SerializeField] private float fallForce = 2f;
    
    [Header("Cube Values")]
    [SerializeField] private int[] basePossibleValues = {2, 2, 2, 4, 4, 8};
    
    private Camera mainCamera;
    private GameObject previewCube;
    private bool isTouching = false;
    private int nextValue;
    private static int maxCubeValue = 8; // Начальный максимум
    
    [Header("Special Cubes")]
    [SerializeField] int specialCubeInterval = 5; // Через сколько ходов спецкубик
    private int spawnCounter = 0;
    private int specialCubeIndex = 0; // 0=Plus, 1=Minus, 2=Death
    
    [Header("Vortex System")]
    [SerializeField] GameObject vortexPrefab;
    [SerializeField] KeyCode vortexKey = KeyCode.V;
    private Vortex currentVortex;
    
    void Start()
    {
        mainCamera = Camera.main;
        nextValue = GetRandomValue();
        CreatePreviewCube();
    }
    
    void Update()
    {
        HandleInput();
        HandleVortexInput();
        UpdatePreviewPosition();
    }
    
    void HandleInput()
    {
        // Обработка мыши для Unity Editor
        if (Input.GetMouseButtonDown(0))
        {
            isTouching = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndTouch();
        }
        
        // Обработка касаний для мобильных устройств
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                EndTouch();
            }
        }
    }
    
    void EndTouch()
    {
        if (isTouching && previewCube != null)
        {
            SpawnFallingCube();
            isTouching = false;
        }
    }
    
    void UpdatePreviewPosition()
    {
        if (previewCube == null) return;
        
        Vector2 inputPosition;
        
        // Получаем позицию ввода (мышь или касание)
        if (Input.touchCount > 0)
        {
            inputPosition = Input.GetTouch(0).position;
        }
        else
        {
            inputPosition = Input.mousePosition;
        }
        
        // Конвертируем в мировые координаты
        Vector2 worldPosition = mainCamera.ScreenToWorldPoint(inputPosition);
        
        // Ограничиваем по оси X в пределах игрового поля
        float clampedX = Mathf.Clamp(worldPosition.x, minXPosition, maxXPosition);
        
        // Устанавливаем позицию превью - кубик всегда следует за мышью/пальцем
        previewCube.transform.position = new Vector2(clampedX, spawnYPosition);
    }
    
    void CreatePreviewCube()
    {
        // Создаем новый превью кубик
        previewCube = Instantiate(cubePrefab, new Vector2(0, spawnYPosition), Quaternion.identity);
        
        // Проверяем, будет ли следующий кубик спецкубиком
        if (spawnCounter + 1 >= specialCubeInterval)
        {
            // Следующий будет спецкубик - настраиваем превью как спецкубик
            Cube cubeScript = previewCube.GetComponent<Cube>();
            if (cubeScript != null)
            {
                Cube.SpecialCubeType type;
                switch (specialCubeIndex)
                {
                    case 0:
                        type = Cube.SpecialCubeType.Plus;
                        break;
                    case 1:
                        type = Cube.SpecialCubeType.Minus;
                        break;
                    case 2:
                        type = Cube.SpecialCubeType.Death;
                        break;
                    default:
                        type = Cube.SpecialCubeType.Plus;
                        break;
                }
                cubeScript.SetSpecialCube(type);
                
                // Применяем настройки внешнего вида и для превью
                SetupSpecialCubeAppearance(cubeScript);
            }
        }
        else
        {
            // Обычный кубик
            Cube cubeScript = previewCube.GetComponent<Cube>();
            if (cubeScript != null)
            {
                cubeScript.SetValue(nextValue);
            }
        }
        
        // Общая настройка превью для всех типов
        SetupPreviewCube();
    }
    
    void SetupPreviewCube()
    {
        if (previewCube == null) return;
        
        // Отключаем физику для превью
        Rigidbody2D rb = previewCube.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // Делаем превью полупрозрачным
        SpriteRenderer renderer = previewCube.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color color = renderer.color;
            color.a = 0.7f; // Полупрозрачный
            renderer.color = color;
        }
        
        // Отключаем слияния для превью
        Cube cube = previewCube.GetComponent<Cube>();
        if (cube != null)
        {
            cube.enabled = false; // Временно отключаем скрипт
            cube.enabled = true;  // Сразу включаем обратно
        }
        
        // Устанавливаем слой для превью если нужно
        // previewCube.layer = LayerMask.NameToLayer("Preview");
    }
    
    void SpawnFallingCube()
    {
        if (previewCube == null) return;
        
        // Сохраняем позицию превью
        Vector2 spawnPosition = previewCube.transform.position;
        
        // Уничтожаем превью
        Destroy(previewCube);
        
        // Создаем падающий кубик
        GameObject fallingCube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);
        
        Cube cubeScript = fallingCube.GetComponent<Cube>();
        if (cubeScript != null)
        {
            // Проверяем, нужно ли создать спецкубик
            spawnCounter++;
            Debug.Log($"Spawn counter: {spawnCounter}, interval: {specialCubeInterval}");
            
            if (spawnCounter >= specialCubeInterval)
            {
                spawnCounter = 0;
                Debug.Log($"Creating special cube, index: {specialCubeIndex}");
                CreateSpecialCube(cubeScript);
            }
            else
            {
                // Обычный кубик
                Debug.Log($"Creating normal cube with value: {nextValue}");
                cubeScript.SetValue(nextValue);
            }
        }
        
        // Настраиваем падающий кубик
        SetupFallingCube(fallingCube);
        
        // Генерируем следующее значение
        nextValue = GetRandomValue();
        
        // Сразу создаем новое превью
        CreatePreviewCube();
    }
    
    void CreateSpecialCube(Cube cubeScript)
    {
        // Создаем спецкубик по порядку: Plus, Minus, Death
        Cube.SpecialCubeType type;
        
        switch (specialCubeIndex)
        {
            case 0:
                type = Cube.SpecialCubeType.Plus;
                break;
            case 1:
                type = Cube.SpecialCubeType.Minus;
                break;
            case 2:
                type = Cube.SpecialCubeType.Death;
                break;
            default:
                type = Cube.SpecialCubeType.Plus;
                specialCubeIndex = 0;
                break;
        }
        
        Debug.Log($"About to call SetSpecialCube with type: {type} (index: {specialCubeIndex})");
        cubeScript.SetSpecialCube(type);
        
        // Дополнительная настройка внешнего вида для спецкубиков
        SetupSpecialCubeAppearance(cubeScript);
        
        // Переходим к следующему спецкубику
        specialCubeIndex = (specialCubeIndex + 1) % 3;
        
        Debug.Log($"Spawned special cube: {type}, next index: {specialCubeIndex}");
    }
    
    void SetupSpecialCubeAppearance(Cube cubeScript)
    {
        // Делаем кубик ромбовидным через поворот спрайта
        SpriteRenderer spriteRenderer = cubeScript.GetComponent<SpriteRenderer>();
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
            cubeScript.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            
            // Возвращаем текст в нормальное положение
            TextMeshPro textMesh = cubeScript.GetComponentInChildren<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        
        // Увеличиваем шрифт для спецкубиков - отключаем auto-size и устанавливаем размер
        TextMeshPro textMeshComponent = cubeScript.GetComponentInChildren<TextMeshPro>();
        if (textMeshComponent != null)
        {
            // Отключаем автоматический размер
            textMeshComponent.enableAutoSizing = false;
            
            // Устанавливаем большой размер шрифта
            float originalFontSize = textMeshComponent.fontSize;
            textMeshComponent.fontSize = Mathf.Max(8f, originalFontSize * 1.5f);
            textMeshComponent.fontStyle = TMPro.FontStyles.Bold;
            
            // Увеличиваем сам текстовый объект для лучшей видимости
            textMeshComponent.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            
            // Выравниваем текст по центру вертикально
            textMeshComponent.alignment = TextAlignmentOptions.Center;
            
            // Корректируем позицию текста для лучшего центрирования
            Vector3 textPosition = textMeshComponent.transform.localPosition;
            textPosition.y = 0f; // Устанавливаем точно в центр по Y
            textMeshComponent.transform.localPosition = textPosition;
        }
    }
    
    void SetupFallingCube(GameObject cube)
    {
        if (cube == null) return;
        
        // Включаем физику
        Rigidbody2D rb = cube.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            
            // Добавляем небольшую силу для падения
            rb.AddForce(Vector2.down * fallForce, ForceMode2D.Impulse);
        }
        
        // Делаем кубик непрозрачным, но сохраняем цвет
        SpriteRenderer renderer = cube.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color color = renderer.color;
            color.a = 1f; // Полностью непрозрачный
            renderer.color = color;
        }
        
        // Включаем слияния
        Cube cubeScript = cube.GetComponent<Cube>();
        if (cubeScript != null)
        {
            // cubeScript.enabled = true; // Если был отключен
            // Цвет уже установлен через SetValue выше
        }
    }
    
    int GetRandomValue()
    {
        // Создаем массив всех возможных значений от 2 до максимума
        List<int> possibleValues = new List<int>();
        
        // Добавляем все значения от 2 до максимума, удваивая каждый раз
        int currentValue = 2;
        while (currentValue <= maxCubeValue)
        {
            possibleValues.Add(currentValue);
            currentValue *= 2;
        }
        
        if (possibleValues.Count == 0)
            return 2;
            
        // Возвращаем случайное значение - все равновероятны
        return possibleValues[Random.Range(0, possibleValues.Count)];
    }
    
    // Статический метод для обновления максимального значения
    public static void UpdateMaxCubeValue(int newValue)
    {
        if (newValue > maxCubeValue)
        {
            maxCubeValue = newValue;
            Debug.Log($"New max cube value: {maxCubeValue}");
        }
    }
    
    void HandleVortexInput()
    {
        // Активация вихря по клавише V
        if (Input.GetKeyDown(vortexKey))
        {
            CreateVortexAtMousePosition();
        }
        
        // Активация вихря по правому клику мыши
        if (Input.GetMouseButtonDown(1))
        {
            CreateVortexAtMousePosition();
        }
    }
    
    void CreateVortexAtMousePosition()
    {
        if (vortexPrefab == null)
        {
            Debug.LogWarning("Vortex prefab not assigned!");
            return;
        }
        
        // Получаем позицию мыши в мире
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        // Проверяем что клик в пределах игровой области
        if (mousePosition.x >= minXPosition && mousePosition.x <= maxXPosition)
        {
            // Создаем вихрь
            GameObject vortexObject = Instantiate(vortexPrefab, mousePosition, Quaternion.identity);
            currentVortex = vortexObject.GetComponent<Vortex>();
            
            if (currentVortex != null)
            {
                currentVortex.ActivateVortex();
                Debug.Log($"Vortex created at position: {mousePosition}");
            }
            else
            {
                Debug.LogError("Vortex component not found on prefab!");
                Destroy(vortexObject);
            }
        }
        else
        {
            Debug.Log("Click outside game area - vortex not created");
        }
    }
    
    // Для визуализации границ в редакторе
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector2(minXPosition, spawnYPosition - 0.5f), 
                       new Vector2(maxXPosition, spawnYPosition - 0.5f));
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector2(minXPosition, spawnYPosition), 0.1f);
        Gizmos.DrawWireSphere(new Vector2(maxXPosition, spawnYPosition), 0.1f);
    }
}
