using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class RaycastDebugger : MonoBehaviour
{
    GraphicRaycaster raycaster;
    EventSystem eventSystem;
    
    void Start()
    {
        // Debug.Log("========== RAYCAST DEBUGGER START ==========");
        
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
        
        if (raycaster == null)
            Debug.LogError("NO GRAPHIC RAYCASTER ON CANVAS!");
        else
            Debug.Log($"✓ GraphicRaycaster found, enabled: {raycaster.enabled}");
            
        if (eventSystem == null)
            Debug.LogError("NO EVENT SYSTEM IN SCENE!");
        else
        {
            Debug.Log($"✓ EventSystem found: {eventSystem.gameObject.name}");
            Debug.Log($"  EventSystem enabled: {eventSystem.enabled}");
            
            var inputModule = eventSystem.currentInputModule;
            if (inputModule == null)
            {
                Debug.LogError("  ❌ NO INPUT MODULE! EventSystem won't work!");
                Debug.LogError("  Fix: Select EventSystem → Remove 'Standalone Input Module' → Add 'Input System UI Input Module'");
            }
            else
            {
                Debug.Log($"  ✓ Current InputModule: {inputModule.GetType().Name}");
            }
        }
        
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"✓ Canvas Render Mode: {canvas.renderMode}");
        }
        
        Debug.Log("========== END START DIAGNOSTICS ==========");
    }
    
    void Update()
    {
        // Press I to get info about items
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            Debug.Log("========== ITEM INFO (I pressed) ==========");
            ItemDragHandler[] items = FindObjectsOfType<ItemDragHandler>();
            Debug.Log($"Found {items.Length} ItemDragHandler(s):");
            
            foreach (var item in items)
            {
                Debug.Log($"  → {item.gameObject.name}");
                Debug.Log($"     Active: {item.gameObject.activeInHierarchy}");
                Debug.Log($"     Position: {item.transform.position}");
                
                Image img = item.GetComponent<Image>();
                if (img != null)
                {
                    Debug.Log($"     Image: raycastTarget={img.raycastTarget}, enabled={img.enabled}, alpha={img.color.a}");
                }
                else
                {
                    Debug.LogError($"     ❌ NO IMAGE!");
                }
                
                CanvasGroup cg = item.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    Debug.Log($"     CanvasGroup: blocksRaycasts={cg.blocksRaycasts}, alpha={cg.alpha}");
                }
            }
        }
        
        // Mouse click raycast test
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            // Debug.Log($"========== MOUSE CLICKED at {mousePos} ==========");
            
            if (eventSystem == null || eventSystem.currentInputModule == null)
            {
                // Debug.LogError("EventSystem or InputModule is NULL! UI events won't work!");
                return;
            }
            
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = mousePos;
            
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);
            
            // Debug.Log($"Raycast hit {results.Count} objects:");
            foreach (RaycastResult result in results)
            {
                // Debug.Log($"  → {result.gameObject.name}");
                
                Image img = result.gameObject.GetComponent<Image>();
                if (img != null)
                {
                    // Debug.Log($"      Image.raycastTarget = {img.raycastTarget}");
                }
                
                CanvasGroup cg = result.gameObject.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    // Debug.Log($"      CanvasGroup.blocksRaycasts = {cg.blocksRaycasts}");
                }
            }
            
            if (results.Count == 0)
            {
                // Debug.LogWarning("NO OBJECTS HIT!");
            }
        }
    }
}