using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EffectEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown effectDropdown;
    [SerializeField] private Button removeButton;
    [SerializeField] private TextMeshProUGUI effectDescriptionText;
    
    private SpellCraftingUI _craftingUI;
    private int _index;
    private int _lastValidIndex;
    private List<Type> _availableEffects; 
    
    public int DropdownValue => effectDropdown.value;
    
    public void Initialize(SpellCraftingUI craftingUI, int index, List<Type> availableEffects)
    {
        _craftingUI = craftingUI;
        _index = index;
        _availableEffects = availableEffects;
        
        PopulateDropdown();
        
        effectDropdown.onValueChanged.AddListener(OnEffectChanged);
        removeButton.onClick.AddListener(OnRemoveClicked);
        
        effectDropdown.value = 0;
        OnEffectChanged(0);
    }
    
    private void PopulateDropdown()
    {
        effectDropdown.ClearOptions();
    
        var allEffects = SpellTypeRegistry.EffectTypes;
        var effectNames = allEffects.Select(effectType =>
        {
            string name = SpellTypeRegistry.GetEffectDisplayName(effectType);
            bool isAvailable = _availableEffects.Contains(effectType);
            
            return isAvailable ? name : $"<color=red>{name}</color>";
        }).ToList();
    
        effectDropdown.AddOptions(effectNames);
    }
    
    public void UpdateEffectDescription(Type effectType)
    {
        var effect = SpellTypeRegistry.CreateEffect(effectType);
    
        float strengthMultiplier = _craftingUI.GetCurrentStrengthMultiplier();
        effect.ApplyStrengthMultiplier(strengthMultiplier);
    
        effectDescriptionText.text = effect.GetDescription();
    }
    
    public void UpdateAvailableEffects(List<Type> availableEffects)
    {
        _availableEffects = availableEffects;
    
        // Get the currently selected effect from SpellTypeRegistry, not from availableEffects
        var allEffects = SpellTypeRegistry.EffectTypes;
        Type currentEffectType = null;
    
        if (effectDropdown.value >= 0 && effectDropdown.value < allEffects.Count)
        {
            currentEffectType = allEffects[effectDropdown.value];
        }
    
        PopulateDropdown();
    
        // Check if current effect is still available
        if (currentEffectType != null && availableEffects.Contains(currentEffectType))
        {
            // Keep the same effect selected (index might change but effect stays the same)
            int indexInAll = allEffects.IndexOf(currentEffectType);
            effectDropdown.value = indexInAll;
        }
        else
        {
            // Current effect not available, select first available
            effectDropdown.value = 0;
            OnEffectChanged(0);
        }
    }
    
    public void SetIndex(int newIndex)
    {
        _index = newIndex;
    }
    
    
    private void OnEffectChanged(int value)
    {
        var allEffects = SpellTypeRegistry.EffectTypes;

        if (value >= 0 && value < allEffects.Count)
        {
            var selectedEffectType = allEffects[value];
        
            if (!_availableEffects.Contains(selectedEffectType))
            {
                effectDropdown.value = _lastValidIndex;
                return;
            }
    
            _lastValidIndex = value;
            _craftingUI.OnEffectChanged(_index, value);
        
            UpdateEffectDescription(selectedEffectType);
        }
    }
    
    private void OnRemoveClicked()
    {
        _craftingUI.RemoveEffectEntry(_index);
    }
}