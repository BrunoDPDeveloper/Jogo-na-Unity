using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIRaycastDebugger : MonoBehaviour
{
    [Tooltip("Se true, só loga quando clicar com o botão esquerdo do mouse")]
    public bool onlyOnClick = true;

    [Tooltip("Se true, mostra detalhes completos para cada hit")]
    public bool fullLog = true;

    void Update()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("[UIRaycastDebugger] Não há EventSystem na cena!");
            return;
        }

        // Trigger
        if (onlyOnClick)
        {
            if (!Input.GetMouseButtonDown(0)) return; // clique esquerdo
        }

        Vector2 pos = Input.mousePosition;
        PointerEventData ped = new PointerEventData(EventSystem.current) { position = pos };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        if (results.Count == 0)
        {
            Debug.Log($"[UIRaycastDebugger] Nada atingido em {pos}");
        }
        else
        {
            Debug.Log($"[UIRaycastDebugger] Hits: {results.Count} em {pos}. Top: {results[0].gameObject.name}");
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                var go = r.gameObject;
                var graphic = go.GetComponent<Graphic>();
                var canvas = go.GetComponentInParent<Canvas>();
                string canvasName = canvas ? canvas.name : "no-canvas";
                int sortOrder = canvas ? canvas.sortingOrder : 0;
                string info = $"[{i}] GO: {go.name} | Canvas: {canvasName} | sortOrder: {sortOrder} | depth: {r.depth} | module: {r.module} | dist: {r.distance}";

                if (graphic != null) info += $" | Graphic: {graphic.GetType().Name} | raycastTarget: {graphic.raycastTarget}";
                var cg = go.GetComponentInParent<CanvasGroup>();
                if (cg != null) info += $" | CanvasGroup.blocksRaycasts: {cg.blocksRaycasts} | interactable: {cg.interactable}";

                Debug.Log(info);
            }
        }

        // Lista todos os GraphicRaycasters na cena
        var raycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        Debug.Log($"[UIRaycastDebugger] {raycasters.Length} GraphicRaycasters encontrados:");
        foreach (var rc in raycasters)
        {
            var c = rc.GetComponent<Canvas>();
            Debug.Log($"- '{rc.gameObject.name}' (Canvas '{(c ? c.name : "no-canvas")}', sortOrder: {(c ? c.sortingOrder : 0)}, BlockingObjects: {rc.blockingObjects}, BlockingMask: {rc.blockingMask})");
        }

        // Info do EventSystem/input module
        Debug.Log($"[UIRaycastDebugger] EventSystem input module: {EventSystem.current.currentInputModule?.GetType().Name}");
    }
}
