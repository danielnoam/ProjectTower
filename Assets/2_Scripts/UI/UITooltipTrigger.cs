using System;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UITooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Content")]
    [SerializeField, Multiline(6)] private string tooltipText = "Text";
    
    [Header("References")]
    [SerializeField] private UITooltip tooltip;

    private void OnValidate()
    {
        if (!tooltip)
        {
            tooltip = FindFirstObjectByType<UITooltip>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip?.Show(tooltipText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.Hide();
    }
}