using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public static CubeSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private float spawnYPosition = 4.5f;
    [SerializeField] private float minXPosition = -3f;
    [SerializeField] private float maxXPosition = 3f;
    [SerializeField] private float fallForce = 2f;

    [Header("Destroy On Spawn")]
    [SerializeField] private GameObject destroyOnSpawnObject;

    [Header("Special Cubes")]
    [SerializeField] private int specialCubeInterval = 5;

    private Camera mainCamera;

    private GameObject previewCube;
    private SpriteRenderer previewRenderer;
    private float currentX;

    private bool isTouching;
    private bool isKeyboardControl;

    private int nextValue;
    private int spawnCounter;
    private int specialCubeIndex;

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
        mainCamera = Camera.main;

        nextValue = GetRandomValue();
        currentX = (minXPosition + maxXPosition) * 0.5f;

        CreatePreview();
    }

    private void Update()
    {
        if (GameOverManager.IsGameOver)
        {
            SetPreviewAlpha(0f);
            return;
        }

        HandlePointerInput();
        HandleKeyboardInput();
        UpdatePreviewPosition();
    }

    #endregion

    #region Input

    private void HandlePointerInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isTouching = true;
            isKeyboardControl = false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseCube();
        }

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                isTouching = true;
                isKeyboardControl = false;
            }
            else if (t.phase == TouchPhase.Ended)
            {
                ReleaseCube();
            }
        }
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
        if (Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            SpawnCube();
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
        previewRenderer.sortingOrder = 0; // Спрайт под текстом
        
        // Настраиваем текст чтобы он был поверх спрайта
        TextMeshPro textMesh = previewCube.GetComponentInChildren<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.sortingOrder = 100; // Текст поверх спрайта
        }
        
        SetPreviewAlpha(0.7f); // Увеличена прозрачность для лучшей видимости

        Rigidbody2D rb = previewCube.GetComponent<Rigidbody2D>();
        rb.simulated = false;

        Cube cube = previewCube.GetComponent<Cube>();
        cube.SetCanMerge(false);

        SetupPreviewValue(cube);
    }

    private void SetupPreviewValue(Cube cube)
    {
        if (spawnCounter + 1 >= specialCubeInterval)
        {
            // Пропускаем None (0) и используем только рабочие спецкубики (1-7)
            int workingSpecialIndex = specialCubeIndex % 7 + 1; // 1-7 вместо 0-6
            cube.SetSpecialCube((Cube.SpecialCubeType)workingSpecialIndex);
        }
        else
        {
            cube.SetValue(nextValue);
        }
    }

    private void UpdatePreviewPosition()
    {
        if (previewCube == null)
            return;

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
            new Vector2(currentX, spawnYPosition);
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
        if (!isTouching && !isKeyboardControl) return;

        SpawnCube();
        isTouching = false;
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

        spawnCounter++;

        if (spawnCounter >= specialCubeInterval)
        {
            spawnCounter = 0;
            // Пропускаем None (0) и используем только рабочие спецкубики (1-7)
            int workingSpecialIndex = specialCubeIndex % 7 + 1; // 1-7 вместо 0-6
            cube.SetSpecialCube((Cube.SpecialCubeType)workingSpecialIndex);
            specialCubeIndex = (specialCubeIndex + 1) % 7; // 7 рабочих типов: Plus, Minus, Death, Grow, Shrink, Freeze, Vortex
        }
        else
        {
            cube.SetValue(nextValue);
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