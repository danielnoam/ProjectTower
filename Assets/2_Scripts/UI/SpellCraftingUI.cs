using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellCraftingUI : MonoBehaviour
{
    [Header("Base Panel")]
    [SerializeField] private TMP_Dropdown castMethodDropdown;
    [SerializeField] private TMP_Dropdown spellFormDropdown;
    [SerializeField] private TMP_Dropdown domainDropdown;
    
    [Header("Effects Panel")]
    [SerializeField] private Transform effectListContainer;
    [SerializeField] private GameObject effectEntryPrefab;
    [SerializeField] private Button addEffectButton;
    
    [Header("Conjure Panel")]
    [SerializeField] private GameObject conjurePanel;
    [SerializeField] private TextMeshProUGUI conjureLifetimeText;
    [SerializeField] private TMP_Dropdown movementDropdown;
    [SerializeField] private TMP_Dropdown collisionDropdown;
    
    [Header("Bottom Panel")]
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private Button createSpellButton;
    [SerializeField] private Button cancelButton;
    
    [Header("References")]
    [SerializeField] private SpellCraftingStation spellCraftingStation;
    
    private SpellCraftingData _currentData = new();
    private readonly List<EffectEntryUI> _effectEntries = new();

    public event Action Closed;
    
    private void Start()
    {
        castMethodDropdown.onValueChanged.AddListener(OnCastMethodChanged);
        spellFormDropdown.onValueChanged.AddListener(OnSpellFormChanged);
        movementDropdown.onValueChanged.AddListener(OnMovementChanged);
        collisionDropdown.onValueChanged.AddListener(OnCollisionChanged);
        addEffectButton.onClick.AddListener(AddEffectEntry);
        createSpellButton.onClick.AddListener(CreateSpell);
        cancelButton.onClick.AddListener(Close);
        
        PopulateDropdowns();
        ResetData();
    }
    
    private void PopulateDropdowns()
    {
        // Cast Method 
        castMethodDropdown.ClearOptions();
        castMethodDropdown.AddOptions(Enum.GetNames(typeof(CastMethod)).ToList());
    
        // Spell Form
        spellFormDropdown.ClearOptions();
        spellFormDropdown.AddOptions(Enum.GetNames(typeof(SpellForm)).ToList());
    
        // Domain
        domainDropdown.ClearOptions();
        domainDropdown.AddOptions(Enum.GetNames(typeof(Domain)).ToList());
        domainDropdown.interactable = Enum.GetValues(typeof(Domain)).Length > 1;
        
        // Movement Types
        movementDropdown.ClearOptions();
        var movementNames = SpellTypeRegistry.MovementTypes
            .Select(SpellTypeRegistry.GetMovementDisplayName)
            .ToList();
        movementDropdown.AddOptions(movementNames);
        movementDropdown.interactable = movementNames.Count > 1;
        
        // Collision Types
        collisionDropdown.ClearOptions();
        var collisionNames = SpellTypeRegistry.CollisionTypes
            .Select(SpellTypeRegistry.GetCollisionDisplayName)
            .ToList();
        collisionDropdown.AddOptions(collisionNames);
        collisionDropdown.interactable = collisionNames.Count > 1;
    }
    
    private void AddEffectEntry()
    {
        if (_effectEntries.Count >= spellCraftingStation.MaxEffects) return;
        
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
        if (_effectEntries.Count <= 1) return;
        
        Destroy(_effectEntries[index].gameObject);
        _effectEntries.RemoveAt(index);
        _currentData.effectTypes.RemoveAt(index);
        
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
        UpdateConjureLifetime();
    }

    private void UpdateConjureLifetime()
    {
        conjureLifetimeText.text = $"Lifetime: {spellCraftingStation.CalculateConjureLifeTime(_currentData)}s";
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
        
        UpdateConjureLifetime();
    }
    
    private void OnCollisionChanged(int value)
    {
        if (value >= 0 && value < SpellTypeRegistry.CollisionTypes.Count)
        {
            _currentData.collisionType = SpellTypeRegistry.CollisionTypes[value];
        }
        
        UpdateConjureLifetime();
    }
    
    private void UpdateUI()
    {
        conjurePanel.SetActive(_currentData.spellForm == SpellForm.Conjure);
    }
    
    private void UpdateManaCost()
    {
        var cost = spellCraftingStation.CalculateManaCost(_currentData).ToString("0.0");
        manaCostText.text = $"Mana Cost: {cost}";
    }
    
    private void CreateSpell()
    {
        if (_currentData.effectTypes.Count == 0)
        {
            Debug.LogWarning("Cannot create spell with no effects!");
            return;
        }
        
        spellCraftingStation.CreateSpell(_currentData);
    }
    
    public void Close()
    {
        Closed?.Invoke();
    }
    
    public void ResetData()
    {

        
        foreach (var entry in _effectEntries)
        {
            Destroy(entry.gameObject);
        }
        _effectEntries.Clear();
        
        castMethodDropdown.value = 0;
        spellFormDropdown.value = 0;
        movementDropdown.value = 0;
        collisionDropdown.value = 0;
        
        _currentData = new SpellCraftingData
        {
            castMethod = 0,
            spellForm = 0,
            domain = 0,
            movementType = SpellTypeRegistry.MovementTypes[0],
            collisionType = SpellTypeRegistry.CollisionTypes[0]
        };

        AddEffectEntry();
        UpdateUI();
    }
}