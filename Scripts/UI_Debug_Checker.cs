using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Debug_Checker : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== UI DEBUG CHECKER START ===");
        
        // Проверяем EventSystem
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            Debug.Log("✅ EventSystem found");
        }
        else
        {
            Debug.LogError("❌ NO EventSystem found! UI buttons won't work!");
            Debug.LogWarning("Create EventSystem: GameObject -> UI -> Event System");
        }
        
        // Проверяем Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Debug.Log($"📊 Found {canvases.Length} Canvas(es)");
        
        foreach (Canvas canvas in canvases)
        {
            Debug.Log($"📱 Canvas: {canvas.name} - RenderMode: {canvas.renderMode} - SortOrder: {canvas.sortingOrder}");
        }
        
        // Проверяем все кнопки
        Button[] buttons = FindObjectsOfType<Button>();
        Debug.Log($"🔘 Found {buttons.Length} Button(s)");
        
        foreach (Button button in buttons)
        {
            bool hasCanvas = button.GetComponentInParent<Canvas>() != null;
            bool hasGraphic = button.GetComponent<UnityEngine.UI.Image>() != null || button.GetComponent<UnityEngine.UI.Text>() != null;
            
            Debug.Log($"🔘 Button: {button.name}");
            Debug.Log($"   - Has Canvas: {hasCanvas}");
            Debug.Log($"   - Has Graphic: {hasGraphic}");
            Debug.Log($"   - Interactable: {button.interactable}");
            
            if (!hasCanvas)
            {
                Debug.LogError($"❌ Button '{button.name}' has NO Canvas in parent!");
            }
            
            if (!hasGraphic)
            {
                Debug.LogWarning($"⚠️ Button '{button.name}' has NO Graphic component!");
            }
        }
        
        // Проверяем GraphicRaycaster
        GraphicRaycaster[] raycasters = FindObjectsOfType<GraphicRaycaster>();
        Debug.Log($"🎯 Found {raycasters.Length} GraphicRaycaster(s)");
        
        foreach (GraphicRaycaster raycaster in raycasters)
        {
            Debug.Log($"🎯 GraphicRaycaster: {raycaster.name}");
        }
        
        if (raycasters.Length == 0)
        {
            Debug.LogError("❌ NO GraphicRaycaster found! UI buttons won't detect clicks!");
            Debug.LogWarning("Add GraphicRaycaster to Canvas component");
        }
        
        Debug.Log("=== UI DEBUG CHECKER END ===");
    }
    
    void Update()
    {
        // Отладка кликов мыши
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Debug.Log($"🖱️ Left click at: {mousePos}");
            
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
                
                Debug.Log($"🎯 Found {results.Count} UI elements under cursor:");
                foreach (RaycastResult result in results)
                {
                    Debug.Log($"   - {result.gameObject.name} ({result.gameObject.GetType().Name})");
                }
            }
        }
    }
}
