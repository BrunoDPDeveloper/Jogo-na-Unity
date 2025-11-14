using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject MapInfo;
    public void OnPointerEnter(PointerEventData eventData)
    {
        MapInfo.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MapInfo.SetActive(false);
    }
}
