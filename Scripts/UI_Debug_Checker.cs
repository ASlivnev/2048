using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Debug_Checker : MonoBehaviour
{
    void Start()
    {
        
        // Проверяем EventSystem
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
        }
        else
        {
        }
        
        // Проверяем Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        
        foreach (Canvas canvas in canvases)
        {
        }
        
        // Проверяем все кнопки
        Button[] buttons = FindObjectsOfType<Button>();
        
        foreach (Button button in buttons)
        {
            bool hasCanvas = button.GetComponentInParent<Canvas>() != null;
            bool hasGraphic = button.GetComponent<UnityEngine.UI.Image>() != null || button.GetComponent<UnityEngine.UI.Text>() != null;
            
            
            if (!hasCanvas)
            {
            }
            
            if (!hasGraphic)
            {
            }
        }
        
        // Проверяем GraphicRaycaster
        GraphicRaycaster[] raycasters = FindObjectsOfType<GraphicRaycaster>();
        
        foreach (GraphicRaycaster raycaster in raycasters)
        {
        }
        
        if (raycasters.Length == 0)
        {
        }
        
    }
    
    void Update()
    {
        // Отладка кликов мыши
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            
            // Проверяем что под курсором
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };
            
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null)
            {
                var results = new System.Collections.Generic.List<RaycastResult>();
                eventSystem.RaycastAll(eventData, results);
                
                foreach (RaycastResult result in results)
                {
                }
            }
        }
    }
}
