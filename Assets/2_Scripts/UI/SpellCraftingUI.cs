using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellCraftingUI : MonoBehaviour
{
    [Header("Base Panel")]
    [SerializeField] private TMP_Dropdown spellFormDropdown;
    [SerializeField] private TMP_Dropdown augmentDropdown;
    
    [Header("Domain Panel")]
    [SerializeField] private TextMeshProUGUI domainCountText;
    [SerializeField] private Transform domainButtonContainer;
    [SerializeField] private DomainButtonUI domainButtonPrefab;
    
    [Header("Effects Panel")]
    [SerializeField] private TextMeshProUGUI effectCountText;
    [SerializeField] private Transform effectButtonContainer;
    [SerializeField] private EffectEntryUI effectButtonPrefab;
    
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
    private readonly List<EffectEntryUI> _effectButtons = new();
    private readonly List<DomainButtonUI> _domainButtons = new();
    private readonly HashSet<Domain> _selectedDomains = new();
    private readonly HashSet<Type> _selectedEffects = new();

    public event Action Closed;
    
    private void Start()
    {
        spellFormDropdown.onValueChanged.AddListener(OnSpellFormChanged);
        augmentDropdown.onValueChanged.AddListener(OnAugmentChanged);
        geometricDropdown.onValueChanged.AddListener(OnGeometricChanged);
        motionDropdown.onValueChanged.AddListener(OnMotionChanged);
        impactDropdown.onValueChanged.AddListener(OnImpactChanged);
        createSpellButton.onClick.AddListener(CreateSpell);
        cancelButton.onClick.AddListener(Close);
        
        PopulateDropdowns();
        InitializeDomainButtons();
        InitializeEffectButtons();
        ResetData();
    }
    
    private void PopulateDropdowns()
    {
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
        
        // Motion Types
        motionDropdown.ClearOptions();
        var motionNames = SpellTypeRegistry.MotionTypes
            .Select(SpellTypeRegistry.GetMotionDisplayName)
            .ToList();
        motionDropdown.AddOptions(motionNames);
        motionDropdown.interactable = motionNames.Count > 1;
        
        // Impact Types
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
    
    private void InitializeEffectButtons()
    {
        foreach (Type effectType in SpellTypeRegistry.EffectTypes)
        {
            EffectEntryUI button = Instantiate(effectButtonPrefab, effectButtonContainer);
            button.Initialize(effectType);
            button.StateChanged += OnEffectButtonClicked;
            _effectButtons.Add(button);
            
            UpdateEffectButtonTooltip(button);
        }
        
        UpdateEffectCountText();
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
        UpdateEffectAvailability();
        UpdateManaCost();
    }
    
    private void OnEffectButtonClicked(Type effectType, bool wantsToSelect)
    {
        if (wantsToSelect)
        {
            if (_selectedEffects.Count >= spellCraftingStation.MaxEffects)
            {
                return;
            }
            
            _selectedEffects.Add(effectType);
            UpdateEffectButton(effectType, true);
        }
        else
        {
            if (_selectedEffects.Count <= 1)
            {
                return;
            }
            
            _selectedEffects.Remove(effectType);
            UpdateEffectButton(effectType, false);
        }
        
        UpdateEffectCountText();
        UpdateManaCost();
    }
    
    private void UpdateDomainButton(Domain domain, bool selected)
    {
        var button = _domainButtons.Find(b => b.Domain == domain);
        button?.SetSelected(selected);
    }
    
    private void UpdateEffectButton(Type effectType, bool selected)
    {
        var button = _effectButtons.Find(b => b.EffectType == effectType);
        button?.SetSelected(selected);
    }
    
    private void UpdateDomainCountText()
    {
        domainCountText.text = $"Domains ({_selectedDomains.Count}/{spellCraftingStation.MaxDomains})";
    }
    
    private void UpdateEffectCountText()
    {
        effectCountText.text = $"Effects ({_selectedEffects.Count}/{spellCraftingStation.MaxEffects})";
    }
    
    private void UpdateEffectAvailability()
    {
        var availableEffects = GetAvailableEffectTypes();
        
        foreach (var effectButton in _effectButtons)
        {
            bool isAvailable = availableEffects.Contains(effectButton.EffectType);
            effectButton.SetAvailable(isAvailable);
            
            if (!isAvailable && effectButton.IsSelected)
            {
                _selectedEffects.Remove(effectButton.EffectType);
                effectButton.SetSelected(false);
            }
        }
        
        UpdateEffectCountText();
    }
    
    private void UpdateEffectButtonTooltip(EffectEntryUI button)
    {
        var effect = SpellTypeRegistry.CreateEffect(button.EffectType);
        // Show base values in tooltip - player will see multipliers at cast time
        button.UpdateTooltip(effect.GetDescription());
    }
    
    private void UpdateAllEffectTooltips()
    {
        foreach (var effectButton in _effectButtons)
        {
            UpdateEffectButtonTooltip(effectButton);
        }
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
        manaCostText.text = $"Base Mana Cost: {cost}";
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
    
    private void CreateSpell()
    {
        if (_selectedEffects.Count == 0)
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
        
        _currentData.effectTypes.Clear();
        _currentData.effectTypes.AddRange(_selectedEffects);
        
        spellCraftingStation.CreateSpell(_currentData);
    }
    
    public void Close()
    {
        Closed?.Invoke();
    }
    
    public void ResetData()
    {
        _selectedEffects.Clear();
        foreach (var button in _effectButtons)
        {
            button?.SetSelected(false);
        }
        
        if (_effectButtons.Count > 0)
        {
            Type firstEffect = _effectButtons[0].EffectType;
            _selectedEffects.Add(firstEffect);
            _effectButtons[0].SetSelected(true);
        }
        
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
        
        spellFormDropdown.value = 0;
        augmentDropdown.value = 0;
        geometricDropdown.value = 0;
        motionDropdown.value = 0;
        impactDropdown.value = 0;
        
        _currentData = new SpellCraftingData
        {
            spellForm = 0,
            augmentType = SpellTypeRegistry.AugmentTypes[0],
            movementType = SpellTypeRegistry.MotionTypes[0],
            collisionType = SpellTypeRegistry.ImpactTypes[0],
            geometric = spellCraftingStation.AvailableGeometrics.Count > 0 
                ? spellCraftingStation.AvailableGeometrics[0] 
                : null
        };

        UpdateUI();
        UpdateDomainCountText();
        UpdateEffectCountText();
        UpdateEffectAvailability();
        UpdateAllEffectTooltips();
    }
}