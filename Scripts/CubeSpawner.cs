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
    private int specialCubeIndex = 0; // 0=Plus, 1=Minus, 2=Death, 3=Grow, 4=Shrink, 5=Freeze, 6=Vortex
    
    [Header("Vortex System")]
    [SerializeField] GameObject vortexPrefab;
    [SerializeField] KeyCode vortexKey = KeyCode.V;
    private Vortex currentVortex;
    
    private bool isKeyboardControl = false;
    
    void Start()
    {
        mainCamera = Camera.main;
        nextValue = GetRandomValue();
        CreatePreviewCube();
    }
    
    void Update()
    {
        HandleInput();
        HandleKeyboardInput();
        HandleVortexInput();
        UpdatePreviewPosition();
    }
    
    void HandleInput()
    {
        // Обработка мыши для Unity Editor
        if (Input.GetMouseButtonDown(0))
        {
            isTouching = true;
            isKeyboardControl = false; // Переключаемся на управление мышью
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
                isKeyboardControl = false; // Переключаемся на управление касанием
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
        
        // Если используется управление клавиатурой, не меняем позицию
        if (isKeyboardControl) return;
        
        if (isTouching)
        {
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
            
            // Устанавливаем позицию превью - следует за мышью/пальцем
            previewCube.transform.position = new Vector2(clampedX, spawnYPosition);
        }
        else
        {
            // Если кнопка не нажата, кубик стоит по центру
            float centerX = (minXPosition + maxXPosition) / 2f;
            previewCube.transform.position = new Vector2(centerX, spawnYPosition);
        }
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
                    case 3:
                        type = Cube.SpecialCubeType.Grow;
                        break;
                    case 4:
                        type = Cube.SpecialCubeType.Shrink;
                        break;
                    case 5:
                        type = Cube.SpecialCubeType.Freeze;
                        break;
                    case 6:
                        type = Cube.SpecialCubeType.Vortex;
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
        // Создаем спецкубик по порядку: Plus, Minus, Death, Grow, Shrink, Freeze
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
            case 3:
                type = Cube.SpecialCubeType.Grow;
                break;
            case 4:
                type = Cube.SpecialCubeType.Shrink;
                break;
            case 5:
                type = Cube.SpecialCubeType.Freeze;
                break;
            case 6:
                type = Cube.SpecialCubeType.Vortex;
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
        specialCubeIndex = (specialCubeIndex + 1) % 7;
        
        Debug.Log($"Spawned special cube: {type}, next index: {specialCubeIndex}");
    }
    
    void SetupSpecialCubeAppearance(Cube cubeScript)
    {
        // Спецкубики выглядят как обычные - только цвет отличается
        // Никаких поворотов, масштабов или изменений шрифта
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
        // Проверяем количество кубиков на сцене
        Cube[] allCubes = FindObjectsOfType<Cube>();
        int normalCubeCount = 0;
        
        // Собираем значения обычных кубиков (не спец)
        HashSet<int> existingValues = new HashSet<int>();
        foreach (Cube cube in allCubes)
        {
            if (!cube.IsSpecialCube)
            {
                normalCubeCount++;
                existingValues.Add(cube.Value);
            }
        }
        
        // Если кубиков больше 9, генерируем только из существующих значений
        if (normalCubeCount > 9 && existingValues.Count > 0)
        {
            List<int> possibleValues = new List<int>(existingValues);
            return possibleValues[Random.Range(0, possibleValues.Count)];
        }
        
        // Иначе генерируем как обычно - все значения от 2 до максимума
        List<int> allPossibleValues = new List<int>();
        
        // Добавляем все значения от 2 до максимума, удваивая каждый раз
        int currentValue = 2;
        while (currentValue <= maxCubeValue)
        {
            allPossibleValues.Add(currentValue);
            currentValue *= 2;
        }
        
        if (allPossibleValues.Count == 0)
            return 2;
            
        // Возвращаем случайное значение - все равновероятны
        return allPossibleValues[Random.Range(0, allPossibleValues.Count)];
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
    
    void HandleKeyboardInput()
    {
        // Проверяем нажатия клавиш движения
        bool isMoving = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ||
                       Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ||
                       Input.GetKey(KeyCode.W));
        
        if (isMoving)
        {
            isKeyboardControl = true;
        }
        
        // Движение превью кубика клавиатурой
        if (previewCube != null)
        {
            float moveSpeed = 5f; // Скорость движения
            Vector3 currentPos = previewCube.transform.position;
            
            // WASD и стрелки для движения
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                currentPos.x -= moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                currentPos.x += moveSpeed * Time.deltaTime;
            }
            
            // Ограничиваем движение в пределах игровой области
            currentPos.x = Mathf.Clamp(currentPos.x, minXPosition, maxXPosition);
            
            previewCube.transform.position = currentPos;
        }
        
        // Сброс кубика по пробелу, стрелке вниз или клавише S
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (previewCube != null)
            {
                SpawnFallingCube();
                isKeyboardControl = false; // Сбрасываем флаг после сброса
            }
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
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        
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
