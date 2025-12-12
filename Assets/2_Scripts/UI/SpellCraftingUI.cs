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
    [SerializeField] private TMP_Dropdown augmentDropdown;
    
    [Header("Domain Panel")]
    [SerializeField] private TextMeshProUGUI domainCountText;
    [SerializeField] private Transform domainButtonContainer;
    [SerializeField] private DomainButtonUI domainButtonPrefab;
    
    [Header("Effects Panel")]
    [SerializeField] private Transform effectListContainer;
    [SerializeField] private GameObject effectEntryPrefab;
    [SerializeField] private Button addEffectButton;
    
    [Header("Conjure Panel")]
    [SerializeField] private GameObject conjurePanel;
    [SerializeField] private TextMeshProUGUI conjureDurationText;
    [SerializeField] private TMP_Dropdown geometricDropdown;
    [SerializeField] private TMP_Dropdown motionDropdown;
    [SerializeField] private TMP_Dropdown impactDropdown;
    
    [Header("Bottom Panel")]
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private Button createSpellButton;
    [SerializeField] private Button cancelButton;
    
    [Header("References")]
    [SerializeField] private SpellCraftingStation spellCraftingStation;
    
    private SpellCraftingData _currentData = new();
    private readonly List<EffectEntryUI> _effectEntries = new();
    private readonly List<DomainButtonUI> _domainButtons = new();
    private readonly HashSet<Domain> _selectedDomains = new();

    public event Action Closed;
    
    private void Start()
    {
        castMethodDropdown.onValueChanged.AddListener(OnCastMethodChanged);
        spellFormDropdown.onValueChanged.AddListener(OnSpellFormChanged);
        augmentDropdown.onValueChanged.AddListener(OnAugmentChanged);
        geometricDropdown.onValueChanged.AddListener(OnGeometricChanged);
        motionDropdown.onValueChanged.AddListener(OnMotionChanged);
        impactDropdown.onValueChanged.AddListener(OnImpactChanged);
        addEffectButton.onClick.AddListener(AddEffectEntry);
        createSpellButton.onClick.AddListener(CreateSpell);
        cancelButton.onClick.AddListener(Close);
        
        PopulateDropdowns();
        InitializeDomainButtons();
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
        
        // Augment Types
        augmentDropdown.ClearOptions();
        var augmentNames = SpellTypeRegistry.AugmentTypes
            .Select(SpellTypeRegistry.GetAugmentDisplayName)
            .ToList();
        augmentDropdown.AddOptions(augmentNames);
        
        // Geometric Types
        geometricDropdown.ClearOptions();
        var geometricNames = spellCraftingStation.AvailableGeometrics
            .Select(g => g.label)
            .ToList();
        geometricDropdown.AddOptions(geometricNames);
        geometricDropdown.interactable = geometricNames.Count > 1;
        
        // Motion Types (renamed from Movement)
        motionDropdown.ClearOptions();
        var motionNames = SpellTypeRegistry.MotionTypes
            .Select(SpellTypeRegistry.GetMotionDisplayName)
            .ToList();
        motionDropdown.AddOptions(motionNames);
        motionDropdown.interactable = motionNames.Count > 1;
        
        // Impact Types (renamed from Collision)
        impactDropdown.ClearOptions();
        var impactNames = SpellTypeRegistry.ImpactTypes
            .Select(SpellTypeRegistry.GetImpactDisplayName)
            .ToList();
        impactDropdown.AddOptions(impactNames);
        impactDropdown.interactable = impactNames.Count > 1;
    }
    
    private void InitializeDomainButtons()
    {
        foreach (Domain domain in Enum.GetValues(typeof(Domain)))
        {
            DomainButtonUI button = Instantiate(domainButtonPrefab, domainButtonContainer);
            button.Initialize(domain);
            button.StateChanged += OnDomainButtonClicked;
            _domainButtons.Add(button);
        }
        
        UpdateDomainCountText();
    }
    
    private void OnDomainButtonClicked(Domain domain, bool wantsToSelect)
    {
        if (wantsToSelect)
        {
            if (_selectedDomains.Count >= spellCraftingStation.MaxDomains)
            {
                return; 
            }
            
            _selectedDomains.Add(domain);
            UpdateDomainButton(domain, true);
        }
        else
        {
            if (_selectedDomains.Count <= 1)
            {
                return;
            }
            _selectedDomains.Remove(domain);
            UpdateDomainButton(domain, false);
        }
        
        UpdateDomainCountText();
        RefreshEffectDropdowns();
        UpdateManaCost();
    }
    
    private void UpdateDomainButton(Domain domain, bool selected)
    {
        var button = _domainButtons.Find(b => b.Domain == domain);
        button?.SetSelected(selected);
    }
    
    private void UpdateDomainCountText()
    {
        domainCountText.text = $"Domains ({_selectedDomains.Count}/{spellCraftingStation.MaxDomains})";
    }
    
    private void AddEffectEntry()
    {
        if (_effectEntries.Count >= spellCraftingStation.MaxEffects) return;
    
        var availableEffects = GetAvailableEffectTypes();
        
        if (SpellTypeRegistry.EffectTypes.Count > 0)
        {
            _currentData.effectTypes.Add(SpellTypeRegistry.EffectTypes[0]);
        }
        else
        {
            Debug.LogError("No effect types available!");
            return;
        }
        
        GameObject entryGo = Instantiate(effectEntryPrefab, effectListContainer);
        EffectEntryUI entry = entryGo.GetComponent<EffectEntryUI>();
        entry.Initialize(this, _effectEntries.Count, availableEffects);
        _effectEntries.Add(entry);
    
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
        UpdateConjureDuration();
        RefreshAllEffectDescriptions();
    }
    

    private void OnSpellFormChanged(int value)
    {
        _currentData.spellForm = (SpellForm)value;
        UpdateUI();
        UpdateManaCost();
        UpdateConjureDuration();
    }
    
    private void OnAugmentChanged(int value)
    {
        if (value >= 0 && value < SpellTypeRegistry.AugmentTypes.Count)
        {
            _currentData.augmentType = SpellTypeRegistry.AugmentTypes[value];
            UpdateManaCost();
        }
    }
    
    private void OnGeometricChanged(int value)
    {
        if (value >= 0 && value < spellCraftingStation.AvailableGeometrics.Count)
        {
            _currentData.geometric = spellCraftingStation.AvailableGeometrics[value];
            UpdateManaCost();
            UpdateConjureDuration();
        }
    }
    
    private void OnMotionChanged(int value)
    {
        if (value >= 0 && value < SpellTypeRegistry.MotionTypes.Count)
        {
            _currentData.movementType = SpellTypeRegistry.MotionTypes[value];
        }
        
        UpdateConjureDuration();
    }
    
    private void OnImpactChanged(int value)
    {
        if (value >= 0 && value < SpellTypeRegistry.ImpactTypes.Count)
        {
            _currentData.collisionType = SpellTypeRegistry.ImpactTypes[value];
        }
        
        UpdateConjureDuration();
    }
    
    private void UpdateConjureDuration()
    {
        conjureDurationText.text = $"Duration: {spellCraftingStation.CalculateConjureDuration(_currentData):F1}s";
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
    
    private List<Type> GetAvailableEffectTypes()
    {
        return SpellTypeRegistry.EffectTypes.Where(effectType =>
        {
            var domains = SpellTypeRegistry.GetEffectDomains(effectType);
            
            if (domains.Length == 0) return true;
        
            return domains.Any(domain => _selectedDomains.Contains(domain));
        }).ToList();
    }
    
    private void RefreshEffectDropdowns()
    {
        var availableEffects = GetAvailableEffectTypes();
    
        for (int i = _effectEntries.Count - 1; i >= 0; i--)
        {
            // Check if current effect is still valid
            var currentEffectType = _currentData.effectTypes[i];
            if (!availableEffects.Contains(currentEffectType))
            {
                // Current effect no longer valid, reset to first available
                if (availableEffects.Count > 0)
                {
                    _currentData.effectTypes[i] = availableEffects[0];
                }
            }
        
            _effectEntries[i].UpdateAvailableEffects(availableEffects);
        }
    
        UpdateManaCost();
    }
    
    private void RefreshAllEffectDescriptions()
    {
        foreach (var effectEntry in _effectEntries)
        {
            int selectedIndex = effectEntry.DropdownValue;
            var allEffects = SpellTypeRegistry.EffectTypes;
        
            if (selectedIndex >= 0 && selectedIndex < allEffects.Count)
            {
                var effectType = allEffects[selectedIndex];
                effectEntry.UpdateEffectDescription(effectType);
            }
        }
    }
    
    public float GetCurrentStrengthMultiplier()
    {
        return spellCraftingStation.GetCastMethodStrengthMultiplier(_currentData.castMethod);
    }
    
    private void CreateSpell()
    {
        if (_currentData.effectTypes.Count == 0)
        {
            Debug.LogWarning("Cannot create spell with no effects!");
            return;
        }
        
        if (_selectedDomains.Count == 0)
        {
            Debug.LogWarning("Cannot create spell with no domains!");
            return;
        }
        
        if (_currentData.spellForm == SpellForm.Conjure && _currentData.geometric == null)
        {
            Debug.LogWarning("Cannot create Conjure spell without selecting Geometric!");
            return;
        }
        
        _currentData.domains.Clear();
        _currentData.domains.AddRange(_selectedDomains);
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
        

        _selectedDomains.Clear();
        foreach (var button in _domainButtons)
        {
            button?.SetSelected(false);
        }
        if (_domainButtons.Count > 0)
        {
            Domain firstDomain = _domainButtons[0].Domain;
            _selectedDomains.Add(firstDomain);
            _domainButtons[0].SetSelected(true);
        }
        
        castMethodDropdown.value = 0;
        spellFormDropdown.value = 0;
        augmentDropdown.value = 0;
        geometricDropdown.value = 0;
        motionDropdown.value = 0;
        impactDropdown.value = 0;
        
        _currentData = new SpellCraftingData
        {
            castMethod = 0,
            spellForm = 0,
            augmentType = SpellTypeRegistry.AugmentTypes[0],
            movementType = SpellTypeRegistry.MotionTypes[0],
            collisionType = SpellTypeRegistry.ImpactTypes[0],
            geometric = spellCraftingStation.AvailableGeometrics.Count > 0 
                ? spellCraftingStation.AvailableGeometrics[0] 
                : null
        };

        AddEffectEntry();
        UpdateUI();
        UpdateDomainCountText();
    }
}