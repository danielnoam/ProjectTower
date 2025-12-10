using System;
using System.Collections.Generic;
using System.Linq;
using DNExtensions;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ItemRequirement
{
    public SOItem item;
    public int amount = 1;
}

[Serializable]
public class UpgradeLevel
{
    [SerializeField] private string label = "Upgrade";
    [SerializeField] private string description;
    [SerializeField] private List<ItemRequirement> requiredItems = new List<ItemRequirement>();
    [SerializeField] private UnityEvent onUpgrade;
    
    public string Label => label;
    public string Description => description;
    public List<ItemRequirement> RequiredItems => requiredItems;
    public UnityEvent OnUpgrade => onUpgrade;
    
    public bool HasRequiredItems(InventoryComponent inventory)
    {
        if (requiredItems.Count == 0) return true;
        
        return requiredItems.All(req => 
            inventory.GetItemAmount(req.item) >= req.amount
        );
    }
    
    public void ConsumeItems(InventoryComponent inventory)
    {
        foreach (var req in requiredItems)
        {
            inventory.RemoveItem(req.item, req.amount);
        }
    }
    
    public string GetRequirementsText(InventoryComponent inventory)
    {
        if (requiredItems.Count == 0) return string.Empty;
        
        var requirements = requiredItems.Select(req =>
        {
            int current = inventory.GetItemAmount(req.item);
            bool hasEnough = current >= req.amount;
            string color = hasEnough ? "green" : "red";
            return $"<color={color}>{req.item.Label}: {current}/{req.amount}</color>";
        });
        
        return string.Join("\n", requirements);
    }
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Interactable))]
public class UpgradableObject : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [SerializeField, Min(0)] private int startLevel;
    [SerializeField] private List<UpgradeLevel> upgradeLevels = new List<UpgradeLevel>();
    [SerializeField] private SOAudioEvent upgradeSuccessSfx;
    [SerializeField] private SOAudioEvent upgradeFailedSfx;
    
    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Interactable interactable;
    [SerializeField] private InteractableTooltip tooltip;
    
    [Separator]
    [SerializeField, ReadOnly] private int currentLevel;
    
    private bool IsMaxLevel => currentLevel >= upgradeLevels.Count;
    
    
    private void OnValidate()
    {
        if (!interactable) interactable = GetComponent<Interactable>();
        if (!tooltip) tooltip = GetComponentInChildren<InteractableTooltip>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentLevel = Mathf.Clamp(startLevel, 0, upgradeLevels.Count);
        upgradeLevels[currentLevel]?.OnUpgrade?.Invoke();
    }

    private void OnEnable()
    {
        interactable.OnInteract += OnInteract;
        interactable.OnHighlight += OnHighlight;
    }
    
    private void OnDisable()
    {
        interactable.OnInteract -= OnInteract;
        interactable.OnHighlight -= OnHighlight;
    }
    
    private void OnHighlight(FPCInteraction interactor)  
    {
        UpdateTooltip(interactor);
    }
    
    private void OnInteract(FPCInteraction interactor)
    {
        if (IsMaxLevel) return;
        
        var inventory = interactor.GetComponent<InventoryComponent>();
        if (!inventory) return;
        
        var currentUpgrade = upgradeLevels[currentLevel];
        
        if (currentUpgrade.HasRequiredItems(inventory))
        {
            currentUpgrade.ConsumeItems(inventory);
            currentUpgrade.OnUpgrade?.Invoke();
            upgradeSuccessSfx?.Play(audioSource);
            currentLevel++;
            UpdateTooltip(interactor);
        }
        else
        {
            upgradeFailedSfx?.Play(audioSource);
            tooltip?.Punch(Color.red);
        }
    }
    
    private void UpdateTooltip(FPCInteraction interactor)  
    {
        if (!tooltip) return;
        
        var inventory = interactor.GetComponent<InventoryComponent>();
        if (!inventory) return;
        
        if (IsMaxLevel)
        {
            tooltip.SetText("Max Level", "Fully Upgraded");
            return;
        }
        
        var upgrade = upgradeLevels[currentLevel];
        string action = $"{upgrade.Label}";
        
        string requirements = upgrade.GetRequirementsText(inventory);
        string description = string.IsNullOrEmpty(requirements) 
            ? upgrade.Description 
            : $"{upgrade.Description}\n\n{requirements}";
        
        tooltip.SetText(action, description);
    }
    
}