using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellCraftingUI : MonoBehaviour
{
    [Header("Main Dropdowns")]
    [SerializeField] private TMP_Dropdown castMethodDropdown;
    [SerializeField] private TMP_Dropdown spellFormDropdown;
    [SerializeField] private TMP_Dropdown domainDropdown;
    
    [Header("Effects")]
    [SerializeField] private Transform effectListContainer;
    [SerializeField] private GameObject effectEntryPrefab;
    [SerializeField] private Button addEffectButton;
    
    [Header("Conjure Panel")]
    [SerializeField] private GameObject conjurePanel;
    [SerializeField] private TMP_Dropdown movementDropdown;
    [SerializeField] private TMP_Dropdown collisionDropdown;
    
    [Header("Bottom UI")]
    [SerializeField] private TMP_Text manaCostText;
    [SerializeField] private Button createSpellButton;
    [SerializeField] private Button cancelButton;
    
    [Header("References")]
    [SerializeField] private SpellCraftingStation spellCraftingStation;
    [SerializeField] private FPCCaster playerCaster;
    
    private SpellCraftingData _currentData = new();
    private readonly List<EffectEntryUI> _effectEntries = new();
    
    private void Start()
    {
        PopulateDropdowns();
        
        castMethodDropdown.onValueChanged.AddListener(OnCastMethodChanged);
        spellFormDropdown.onValueChanged.AddListener(OnSpellFormChanged);
        movementDropdown.onValueChanged.AddListener(OnMovementChanged);
        collisionDropdown.onValueChanged.AddListener(OnCollisionChanged);
        
        addEffectButton.onClick.AddListener(AddEffectEntry);
        createSpellButton.onClick.AddListener(CreateSpell);
        cancelButton.onClick.AddListener(Close);
        
        AddEffectEntry();
        UpdateUI();
    }
    
    private void PopulateDropdowns()
    {
        // Cast Method
        castMethodDropdown.ClearOptions();
        castMethodDropdown.AddOptions(new List<string> { "Instant", "Channel", "Charge" });
        
        // Spell Form
        spellFormDropdown.ClearOptions();
        spellFormDropdown.AddOptions(new List<string> { "Imbue", "Invoke", "Conjure" });
        
        // Domain (only Arcane for now)
        domainDropdown.ClearOptions();
        domainDropdown.AddOptions(new List<string> { "Arcane" });
        domainDropdown.interactable = false;
        
        // Movement Types - Dynamic
        movementDropdown.ClearOptions();
        var movementNames = SpellTypeRegistry.MovementTypes
            .Select(SpellTypeRegistry.GetMovementDisplayName)
            .ToList();
        movementDropdown.AddOptions(movementNames);
        movementDropdown.interactable = movementNames.Count > 1;
        
        // Collision Types - Dynamic
        collisionDropdown.ClearOptions();
        var collisionNames = SpellTypeRegistry.CollisionTypes
            .Select(SpellTypeRegistry.GetCollisionDisplayName)
            .ToList();
        collisionDropdown.AddOptions(collisionNames);
        collisionDropdown.interactable = collisionNames.Count > 1;
    }
    
    private void AddEffectEntry()
    {
        GameObject entryGo = Instantiate(effectEntryPrefab, effectListContainer);
        EffectEntryUI entry = entryGo.GetComponent<EffectEntryUI>();
        entry.Initialize(this, _effectEntries.Count);
        _effectEntries.Add(entry);
        
        if (SpellTypeRegistry.EffectTypes.Count > 0)
        {
            _currentData.effectTypes.Add(SpellTypeRegistry.EffectTypes[0]);
        }
        
        UpdateManaCost();
    }
    
    public void RemoveEffectEntry(int index)
    {
        if (_effectEntries.Count <= 1) return; // Keep at least one effect
        
        Destroy(_effectEntries[index].gameObject);
        _effectEntries.RemoveAt(index);
        _currentData.effectTypes.RemoveAt(index);
        
        // Re-index remaining entries
        for (int i = 0; i < _effectEntries.Count; i++)
        {
            _effectEntries[i].SetIndex(i);
        }
        
        UpdateManaCost();
    }
    
    public void OnEffectChanged(int index, int effectValue)
    {
        if (effectValue >= 0 && effectValue < SpellTypeRegistry.EffectTypes.Count)
        {
            _currentData.effectTypes[index] = SpellTypeRegistry.EffectTypes[effectValue];
            UpdateManaCost();
        }
    }
    
    private void OnCastMethodChanged(int value)
    {
        _currentData.castMethod = (CastMethod)value;
        UpdateManaCost();
    }
    
    private void OnSpellFormChanged(int value)
    {
        _currentData.spellForm = (SpellForm)value;
        UpdateUI();
        UpdateManaCost();
    }
    
    private void OnMovementChanged(int value)
    {
        if (value >= 0 && value < SpellTypeRegistry.MovementTypes.Count)
        {
            _currentData.movementType = SpellTypeRegistry.MovementTypes[value];
        }
    }
    
    private void OnCollisionChanged(int value)
    {
        if (value >= 0 && value < SpellTypeRegistry.CollisionTypes.Count)
        {
            _currentData.collisionType = SpellTypeRegistry.CollisionTypes[value];
        }
    }
    
    private void UpdateUI()
    {
        conjurePanel.SetActive(_currentData.spellForm == SpellForm.Conjure);
    }
    
    private void UpdateManaCost()
    {
        manaCostText.text = spellCraftingStation.CalculateManaCost(_currentData).ToString("F0");
    }
    
    private void CreateSpell()
    {
        if (_currentData.effectTypes.Count == 0)
        {
            Debug.LogWarning("Cannot create spell with no effects!");
            return;
        }
        
        SOSpell spell = spellCraftingStation.CreateSpell(_currentData);
        playerCaster?.AddSpell(spell);
        
        Debug.Log($"Created spell: {spell.label} with {spell.effects.Length} effects, cost: {spell.manaCost}");
    }
    
    private void Close()
    {
        gameObject.SetActive(false);
    }
    
    public void Open()
    {
        gameObject.SetActive(true);
        
        // Reset to defaults
        _currentData = new SpellCraftingData();
        
        // Clear existing effect entries
        foreach (var entry in _effectEntries)
        {
            Destroy(entry.gameObject);
        }
        _effectEntries.Clear();
        
        // Reset dropdowns
        castMethodDropdown.value = 0;
        spellFormDropdown.value = 0;
        movementDropdown.value = 0;
        collisionDropdown.value = 0;
        
        // Add first effect
        AddEffectEntry();
        UpdateUI();
    }
}