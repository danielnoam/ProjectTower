using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EffectEntryUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private UITooltipTrigger tooltipTrigger;
    
    [Header("Visual States")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.grey;
    [SerializeField] private Color unavailableColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 selectedScale = new Vector3(0.9f, 0.9f, 0.9f);
    
    private Type _effectType;
    private bool _isSelected;
    private bool _isAvailable;
    
    public event Action<Type, bool> StateChanged;
    
    public Type EffectType => _effectType;
    public bool IsSelected => _isSelected;
    public bool IsAvailable => _isAvailable;
    
    private void Awake()
    {
        button.onClick.AddListener(ToggleSelection);
    }
    
    public void Initialize(Type effectType)
    {
        _effectType = effectType;
        labelText.text = SpellTypeRegistry.GetEffectDisplayName(effectType);
        SetSelected(false);
        SetAvailable(true);
    }
    
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateVisuals();
    }
    
    public void SetAvailable(bool available)
    {
        _isAvailable = available;
        button.interactable = available;
        UpdateVisuals();
    }
    
    public void UpdateTooltip(string description)
    {
        if (tooltipTrigger)
        {
            tooltipTrigger.tooltipText = description;
        }
    }
    
    private void ToggleSelection()
    {
        if (!_isAvailable) return;
        StateChanged?.Invoke(_effectType, !_isSelected);
    }
    
    private void UpdateVisuals()
    {
        if (!_isAvailable)
        {
            backgroundImage.color = unavailableColor;
            transform.localScale = normalScale;
        }
        else
        {
            backgroundImage.color = _isSelected ? selectedColor : normalColor;
            transform.localScale = _isSelected ? selectedScale : normalScale;
        }
    }
}