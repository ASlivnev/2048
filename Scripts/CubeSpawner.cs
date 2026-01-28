using System.Collections;
using System.Collections.Generic;
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
    
    void Start()
    {
        mainCamera = Camera.main;
        nextValue = GetRandomValue();
        CreatePreviewCube();
    }
    
    void Update()
    {
        HandleInput();
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
        if (cubePrefab == null) return;
        
        // Создаем превью кубик
        previewCube = Instantiate(cubePrefab, new Vector2(0, spawnYPosition), Quaternion.identity);
        
        // Настраиваем превью
        SetupPreviewCube();
        
        // Устанавливаем значение
        Cube cubeScript = previewCube.GetComponent<Cube>();
        if (cubeScript != null)
        {
            cubeScript.SetValue(nextValue);
        }
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
            // Можно добавить метод для отключения слияний если нужно
            // cube.enabled = false; // Или другой способ
        }
        
        // Устанавливаем слой для превью если нужно
        // previewCube.layer = LayerMask.NameToLayer("Preview");
    }
    
    void SpawnFallingCube()
    {
        if (previewCube == null) return;
        
        // Сохраняем позицию и значение превью
        Vector2 spawnPosition = previewCube.transform.position;
        
        // Уничтожаем превью
        Destroy(previewCube);
        
        // Создаем падающий кубик
        GameObject fallingCube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);
        
        // Устанавливаем значение
        Cube cubeScript = fallingCube.GetComponent<Cube>();
        if (cubeScript != null)
        {
            cubeScript.SetValue(nextValue);
        }
        
        // Настраиваем падающий кубик
        SetupFallingCube(fallingCube);
        
        // Генерируем следующее значение
        nextValue = GetRandomValue();
        
        // Сразу создаем новое превью
        CreatePreviewCube();
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
