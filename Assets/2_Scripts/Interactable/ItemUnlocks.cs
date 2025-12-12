using System;
using System.Collections.Generic;
using System.Linq;
using DNExtensions;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Upgrade
{
    [SerializeField] private string upgradeName = "Upgrade";
    [SerializeField] private string description;
    [SerializeField] private List<ItemRequirement> requirements = new List<ItemRequirement>();
    [SerializeField] private UnityEvent onUnlock;
    
    [SerializeField, ReadOnly] private bool isUnlocked;
    
    public string UpgradeName => upgradeName;
    public string Description => description;
    public List<ItemRequirement> Requirements => requirements;
    public UnityEvent OnUnlock => onUnlock;
    public bool IsUnlocked => isUnlocked;
    
    public bool CanUnlock(InventoryComponent inventory)
    {
        if (isUnlocked) return false;
        if (requirements.Count == 0) return false;
        return requirements.All(req => inventory.GetItemAmount(req.item) >= req.amount);
    }
    
    public void Unlock(InventoryComponent inventory)
    {
        foreach (var req in requirements)
        {
            inventory.RemoveItem(req.item, req.amount);
        }
        isUnlocked = true;
        onUnlock?.Invoke();
    }
    
    public string GetRequirementsText(InventoryComponent inventory)
    {
        if (requirements.Count == 0) return "No requirements";
        
        var requirementTexts = requirements.Select(req =>
        {
            int current = inventory.GetItemAmount(req.item);
            bool hasEnough = current >= req.amount;
            string color = hasEnough ? "green" : "red";
            return $"<color={color}>{req.item.Label}: {current}/{req.amount}</color>";
        });
        
        return string.Join("\n", requirementTexts);
    }
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Interactable))]
public class ItemUnlocks : MonoBehaviour
{
    [Header("Upgrades")]
    [SerializeField] private bool showTooltipIfAllUnlocked;
    [SerializeField] private List<Upgrade> upgrades = new List<Upgrade>();
    [SerializeField] private UnityEvent allUnlockedEvents;
    [SerializeField] private UnityEvent anyUpgradeUnlockedEvents;
    [SerializeField] private UnityEvent startEvents;
    
    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Interactable interactable;
    [SerializeField] private InteractableTooltip tooltip;
    [SerializeField] private SOAudioEvent unlockSuccessSfx;
    [SerializeField] private SOAudioEvent unlockFailedSfx;
    
    [Separator]
    [SerializeField, ReadOnly] private bool allUnlocked;
    
    private void OnValidate()
    {
        if (!interactable) interactable = GetComponent<Interactable>();
        if (!tooltip) tooltip = GetComponentInChildren<InteractableTooltip>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        startEvents?.Invoke();
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
        var inventory = interactor.GetComponent<InventoryComponent>();
        if (!inventory) return;
        
        bool unlockedAny = false;
        
        foreach (var upgrade in upgrades)
        {
            if (upgrade.CanUnlock(inventory))
            {
                upgrade.Unlock(inventory);
                unlockedAny = true;
                anyUpgradeUnlockedEvents?.Invoke();
            }
        }
        
        
        if (unlockedAny)
        {
            unlockSuccessSfx?.Play(audioSource);
            
            if (!allUnlocked && AllUpgradesUnlocked())
            {
                if (showTooltipIfAllUnlocked) interactable.SetCanInteract(false);
                allUnlocked = true;
                allUnlockedEvents?.Invoke();
            }
        }
        else
        {
            unlockFailedSfx?.Play(audioSource);
            tooltip?.Punch(Color.red);
        }
        
        UpdateTooltip(interactor);
    }
    
    private bool AllUpgradesUnlocked()
    {
        return upgrades.Count > 0 && upgrades.All(u => u.IsUnlocked);
    }
    
    private void UpdateTooltip(FPCInteraction interactor)
    {
        if (!tooltip) return;
    
        var inventory = interactor.GetComponent<InventoryComponent>();
        if (!inventory) return;
    
        if (allUnlocked)
        {
            tooltip.SetText("All Upgrades Complete", "Everything unlocked!");
            return;
        }
        
        var lockedUpgrades = upgrades.Where(u => !u.IsUnlocked).ToList();
    
        if (lockedUpgrades.Count == 0)
        {
            tooltip.SetText("All Upgrades Complete", "Everything unlocked!");
            return;
        }
    
        string header = $"Items ({upgrades.Count(u => u.IsUnlocked)}/{upgrades.Count})";
    
        var upgradeTexts = lockedUpgrades.Select(upgrade =>
        {
            string requirements = upgrade.GetRequirementsText(inventory);
            return $"<b>{upgrade.UpgradeName}</b>\n{requirements}";
        });
    
        string description = string.Join("\n\n", upgradeTexts);
    
        tooltip.SetText(header, description);
    }
}