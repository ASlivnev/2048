using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public static CubeSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int previewValue = 2;
    
    [Header("Input Settings")]
    [SerializeField] private RectTransform gameArea; // Игровая зона для кликов
    [SerializeField] private float fallForce = 2f;
    [SerializeField] private float spawnYPosition = 2.5f;
    [SerializeField] private float minXPosition = -2.1f;
    [SerializeField] private float maxXPosition = 2.1f;

    [Header("Destroy On Spawn")]
    [SerializeField] private GameObject destroyOnSpawnObject;

    [Header("Special Cubes")]
    [SerializeField] private int specialCubeInterval = 5;

    private Camera mainCamera;

    private GameObject previewCube;
    private SpriteRenderer previewRenderer;
    private float currentX;
    private float currentY;

    private bool isTouching;
    private bool isKeyboardControl;
    private bool isSpawningThisFrame = false;

    private int nextValue;
    private int spawnCounter;
    private int specialCubeIndex;
    
    // Массив для случайной последовательности спецкубиков
    private int[] specialCubeSequence;
    private int currentSpecialIndex = 0;

    private int maxCubeValue = 8;
    public int MaxCubeValue { get; private set; } = 2;

    #region Unity

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentX = 0f;
        currentY = 2;
        // spawnYPosition используется из инспектора (2.6f)
        CreatePreview();
    }

    // Метод для уведомления об уничтожении всех кубиков
    public void OnAllCubesDestroyed()
    {
        // Пересоздаем превью после уничтожения всех кубиков
        if (previewCube != null)
        {
            Destroy(previewCube);
            previewCube = null;
        }
        
        // Сбрасываем состояние спауна к начальному
        ResetSpawnerState();
        
        CreatePreview();
    }
    
    public void ResetSpawnerState()
    {
        // Сбрасываем все переменные к начальному состоянию
        currentX = 0f;
        currentY = 2;
        // spawnYPosition используется из инспектора (2.6f)
        spawnCounter = 0;
        specialCubeIndex = 0;
        nextValue = 2;
        MaxCubeValue = 2;
        
        // ПРАВИЛЬНОЕ УПРАВЛЕНИЕ - сбрасываем флаги управления
        isTouching = false;
        isKeyboardControl = false;
        
        // Перемешиваем последовательность спецкубиков
        ShuffleSpecialCubes();
        
    }
    
    private void ShuffleSpecialCubes()
    {
        // Создаем массив с 7 типами спецкубиков (1-7)
        specialCubeSequence = new int[7] { 1, 2, 3, 4, 5, 6, 7 };
        
        // Перемешиваем массив (Fisher-Yates shuffle)
        for (int i = specialCubeSequence.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = specialCubeSequence[i];
            specialCubeSequence[i] = specialCubeSequence[randomIndex];
            specialCubeSequence[randomIndex] = temp;
        }
        
        currentSpecialIndex = 0;
        
        // Логируем последовательность для отладки
        string sequenceStr = string.Join(", ", specialCubeSequence);
    }

    private void Update()
    {
        if (GameOverManager.IsGameOver)
        {
            SetPreviewAlpha(0f);
            return;
        }

        // Проверяем, существует ли превью, если нет - создаем
        if (previewCube == null)
        {
            CreatePreview();
        }

        HandlePointerInput();
        HandleKeyboardInput();
        UpdatePreviewPosition();
        
        // Сбрасываем флаг в КОНЦЕ кадра после всех обработок
        isSpawningThisFrame = false;
    }

    #endregion

    #region Input

    private void HandlePointerInput()
    {
        // Проверяем клик только по игровой зоне
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverGameArea())
            {
                isTouching = true;
                isKeyboardControl = false;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isTouching && !isSpawningThisFrame)
            {
                SpawnCube();
                isSpawningThisFrame = true;
                isTouching = false;
            }
        }

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                if (IsTouchOverGameArea(t.position))
                {
                    isTouching = true;
                    isKeyboardControl = false;
                }
            }
            else if (t.phase == TouchPhase.Ended)
            {
                if (isTouching && !isSpawningThisFrame)
                {
                    SpawnCube();
                    isSpawningThisFrame = true;
                    isTouching = false;
                }
            }
        }
    }
    
    private bool IsPointerOverGameArea()
    {
        if (gameArea == null) return true; // Если зона не назначена, работаем как раньше
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gameArea, 
            Input.mousePosition, 
            null, 
            out localPoint
        );
        
        return gameArea.rect.Contains(localPoint);
    }
    
    private bool IsTouchOverGameArea(Vector2 touchPosition)
    {
        if (gameArea == null) return true; // Если зона не назначена, работаем как раньше
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gameArea, 
            touchPosition, 
            null, 
            out localPoint
        );
        
        return gameArea.rect.Contains(localPoint);
    }

    private void HandleKeyboardInput()
    {
        // Движение влево-вправо
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            currentX -= 6f * Time.deltaTime;
            isKeyboardControl = true;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            currentX += 6f * Time.deltaTime;
            isKeyboardControl = true;
        }

        currentX = Mathf.Clamp(currentX, minXPosition, maxXPosition);

        // Спаун при нажатии кнопок
        if ((Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.Space)) && !isSpawningThisFrame)
        {
            SpawnCube();
            isSpawningThisFrame = true;
            isKeyboardControl = false;
        }
    }

    #endregion

    #region Preview

    private void CreatePreview()
    {
        if (previewCube != null)
            Destroy(previewCube);

        previewCube = Instantiate(
            cubePrefab,
            new Vector2(currentX, spawnYPosition),
            Quaternion.identity
        );

        previewRenderer = previewCube.GetComponent<SpriteRenderer>();
        previewRenderer.sortingOrder = -2; // Куб превью под спрайтом (-1) и текстом (0)
        
        // Настраиваем текст чтобы он был поверх спрайта
        TextMeshPro textMesh = previewCube.GetComponentInChildren<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.sortingOrder = 0; // Текст поверх спрайта
            // Отключаем подчеркивание чтобы избежать ошибки с шрифтом
            textMesh.fontStyle = FontStyles.Normal;
            // Дополнительно отключаем Rich Text чтобы избежать тегов <u>
            textMesh.richText = false;
        }
        
        SetPreviewAlpha(0.4f); // Увеличена прозрачность для лучшей видимости

        Rigidbody2D rb = previewCube.GetComponent<Rigidbody2D>();
        rb.simulated = false;

        Cube cube = previewCube.GetComponent<Cube>();
        cube.SetCanMerge(false);

        SetupPreviewValue(cube);
    }

    private void SetupPreviewValue(Cube cube)
    {
        if (spawnCounter >= specialCubeInterval - 1)
        {
            // Используем случайную последовательность спецкубиков
            if (specialCubeSequence == null || currentSpecialIndex >= specialCubeSequence.Length)
            {
                // Если последовательность закончилась, перемешиваем заново
                ShuffleSpecialCubes();
            }
            
            // Показываем следующий спецкубик
            int specialType = specialCubeSequence[currentSpecialIndex];
            cube.SetSpecialCube((Cube.SpecialCubeType)specialType);
            
            // Устанавливаем SpecialSprites sorting order = -3 для превью (ниже куба -2)
            cube.SetSpecialSpritesLayer(-3);
        }
        else
        {
            // Проверяем что nextValue не равно 0
            if (nextValue <= 0)
            {
                nextValue = 2; // Устанавливаем минимальное значение
            }
            cube.SetValue(nextValue);
        }
    }

    private void UpdatePreviewPosition()
    {
        // Проверяем, существует ли mainCamera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }
        
        if (previewCube == null)
            return;

        // ПРАВИЛЬНОЕ УПРАВЛЕНИЕ - двигаем превью только при зажатой мыши
        if (isTouching)
        {
            Vector2 screenPos =
                Input.touchCount > 0
                ? Input.GetTouch(0).position
                : (Vector2)Input.mousePosition;

            Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            currentX = Mathf.Clamp(worldPos.x, minXPosition, maxXPosition);
        }

        previewCube.transform.position =
            new Vector3(currentX, spawnYPosition);
    }

    private void SetPreviewAlpha(float a)
    {
        if (previewRenderer == null) return;
        Color c = previewRenderer.color;
        c.a = a;
        previewRenderer.color = c;
    }

    #endregion

    #region Spawn

    private void ReleaseCube()
    {
        // ПРАВИЛЬНОЕ УПРАВЛЕНИЕ - не используется, спаун напрямую в HandlePointerInput
        // Метод оставлен для совместимости
    }

    private void SpawnCube()
    {
        Vector2 spawnPos = new Vector2(currentX, spawnYPosition);
        GameObject cubeObj = Instantiate(cubePrefab, spawnPos, Quaternion.identity);

        cubeObj.AddComponent<FallingCubeTracker>();

        Rigidbody2D rb = cubeObj.GetComponent<Rigidbody2D>();
        rb.simulated = true;
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.down * fallForce, ForceMode2D.Impulse);

        Cube cube = cubeObj.GetComponent<Cube>();

        // Устанавливаем правильный sorting order чтобы куб был выше превью
        SpriteRenderer cubeRenderer = cubeObj.GetComponent<SpriteRenderer>();
        if (cubeRenderer != null)
        {
            cubeRenderer.sortingOrder = 1; // Выше превью (0) и текста (-1)
        }
        
        // Устанавливаем sorting order для текста на кубике
        TextMeshPro[] textMeshes = cubeObj.GetComponentsInChildren<TextMeshPro>();
        foreach (TextMeshPro textMesh in textMeshes)
        {
            if (textMesh != null)
            {
                textMesh.sortingOrder = 2; // Текст выше всего остального
                // Отключаем подчеркивание чтобы избежать ошибки с шрифтом
                textMesh.fontStyle = FontStyles.Normal;
                // Дополнительно отключаем Rich Text чтобы избежать тегов <u>
                textMesh.richText = false;
            }
        }

        spawnCounter++;

        if (spawnCounter >= specialCubeInterval)
        {
            spawnCounter = 0;
            
            // Используем случайную последовательность спецкубиков
            if (specialCubeSequence == null || currentSpecialIndex >= specialCubeSequence.Length)
            {
                // Если последовательность закончилась, перемешиваем заново
                ShuffleSpecialCubes();
            }
            
            int specialType = specialCubeSequence[currentSpecialIndex];
            cube.SetSpecialCube((Cube.SpecialCubeType)specialType);
            currentSpecialIndex++; // Переходим к следующему спецкубику
        }
        else
        {
            // Проверяем что nextValue не равно 0
            if (nextValue <= 0)
            {
                nextValue = 2; // Устанавливаем минимальное значение
            }
            cube.SetValue(nextValue);
        }

        // Проверяем таймер рекламы при спауне
        if (AdManager.Instance != null && AdManager.Instance.ShouldShowInterstitialOnSpawn())
        {
            AdManager.Instance.ShowInterstitialAd("Cube Spawn");
        }

        // Уничтожаем объект после спауна кубика
        if (destroyOnSpawnObject != null)
        {
            Destroy(destroyOnSpawnObject);
            destroyOnSpawnObject = null;
        }

        nextValue = GetRandomValue();
        CreatePreview();
    }

    #endregion

    #region Values

    private int GetRandomValue()
    {
        List<int> values = new List<int>();
        int v = 2;

        while (v <= maxCubeValue)
        {
            values.Add(v);
            v *= 2;
        }

        return values[Random.Range(0, values.Count)];
    }

    public void UpdateMaxCubeValue(int newValue)
    {
        if (newValue > maxCubeValue)
        {
            maxCubeValue = newValue;
            MaxCubeValue = newValue;
        }
    }

    #endregion

    public void OnCubeLanded()
    {
        // Вызывается из FallingCubeTracker
        // Сейчас не обязателен, но оставлен для совместимости
    }

    
}