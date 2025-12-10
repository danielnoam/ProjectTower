using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EffectEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown effectDropdown;
    [SerializeField] private Button removeButton;
    
    private SpellCraftingUI _craftingUI;
    private int _index;
    
    public void Initialize(SpellCraftingUI craftingUI, int index)
    {
        _craftingUI = craftingUI;
        _index = index;
        
        // Populate dropdown with all available effects
        effectDropdown.ClearOptions();
        var effectNames = SpellTypeRegistry.EffectTypes
            .Select(SpellTypeRegistry.GetEffectDisplayName)
            .ToList();
        effectDropdown.AddOptions(effectNames);
        
        effectDropdown.onValueChanged.AddListener(OnEffectChanged);
        removeButton.onClick.AddListener(OnRemoveClicked);
    }
    
    public void SetIndex(int newIndex)
    {
        _index = newIndex;
    }
    
    public int GetSelectedEffectIndex()
    {
        return effectDropdown.value;
    }
    
    public void SetEffectIndex(int effectIndex)
    {
        effectDropdown.value = effectIndex;
    }
    
    private void OnEffectChanged(int value)
    {
        _craftingUI.OnEffectChanged(_index, value);
    }
    
    private void OnRemoveClicked()
    {
        _craftingUI.RemoveEffectEntry(_index);
    }
}