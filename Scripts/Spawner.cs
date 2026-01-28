using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnForce = 5f;
    [SerializeField] private float maxDragDistance = 200f;
    
    [Header("Cube Values")]
    [SerializeField] private int[] possibleValues = {2, 2, 2, 4, 4, 8}; // Вероятности значений
    
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector2 dragStartPosition;
    private GameObject previewCube;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (spawnPoint == null)
            spawnPoint = transform;
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Обработка мыши для Unity Editor
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            UpdateDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag(Input.mousePosition);
        }
        
        // Обработка касаний для мобильных устройств
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                StartDrag(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                UpdateDrag(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                EndDrag(touch.position);
            }
        }
    }
    
    void StartDrag(Vector2 screenPosition)
    {
        Vector2 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        
        // Проверяем, находится ли касание в зоне спаунера (верхняя часть экрана)
        if (worldPosition.y > spawnPoint.position.y - 1f)
        {
            isDragging = true;
            dragStartPosition = screenPosition;
            
            // Создаем превью кубика
            CreatePreviewCube();
        }
    }
    
    void UpdateDrag(Vector2 screenPosition)
    {
        if (!isDragging || previewCube == null) return;
        
        // Ограничиваем расстояние перетаскивания
        Vector2 dragDirection = (screenPosition - dragStartPosition).normalized;
        float dragDistance = Vector2.Distance(screenPosition, dragStartPosition);
        dragDistance = Mathf.Min(dragDistance, maxDragDistance);
        
        Vector2 previewPosition = (Vector2)spawnPoint.position + dragDirection * (dragDistance / 50f);
        previewCube.transform.position = previewPosition;
    }
    
    void EndDrag(Vector2 screenPosition)
    {
        if (!isDragging) return;
        
        isDragging = false;
        
        // Уничтожаем превью
        if (previewCube != null)
        {
            Destroy(previewCube);
            previewCube = null;
        }
        
        // Вычисляем силу и направление броска
        Vector2 dragDirection = (screenPosition - dragStartPosition).normalized;
        float dragDistance = Vector2.Distance(screenPosition, dragStartPosition);
        
        if (dragDistance > 10f) // Минимальное расстояние для спауна
        {
            SpawnCube(dragDirection, dragDistance);
        }
    }
    
    void CreatePreviewCube()
    {
        if (cubePrefab != null)
        {
            previewCube = Instantiate(cubePrefab, spawnPoint.position, Quaternion.identity);
            
            // Делаем превью полупрозрачным
            SpriteRenderer renderer = previewCube.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = 0.5f;
                renderer.color = color;
            }
            
            // Отключаем физику для превью
            Rigidbody2D rb = previewCube.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = false;
            }
            
            // Отключаем слияния для превью
            Cube cube = previewCube.GetComponent<Cube>();
            if (cube != null)
            {
                cube.enabled = false;
            }
        }
    }
    
    void SpawnCube(Vector2 direction, float distance)
    {
        if (cubePrefab == null) return;
        
        GameObject newCube = Instantiate(cubePrefab, spawnPoint.position, Quaternion.identity);
        
        // Устанавливаем случайное значение
        Cube cubeScript = newCube.GetComponent<Cube>();
        if (cubeScript != null)
        {
            int randomValue = possibleValues[Random.Range(0, possibleValues.Length)];
            cubeScript.SetValue(randomValue);
        }
        
        // Применяем силу
        Rigidbody2D rb = newCube.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float force = spawnForce * (distance / maxDragDistance);
            rb.AddForce(-direction * force, ForceMode2D.Impulse);
        }
    }
}
