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
    [Min(1)] public int amount = 1;
}

[Serializable]
public class ProgressionStage
{
    [SerializeField] private string stageName = "Stage";
    [SerializeField] private string description;
    [SerializeField] private List<ItemRequirement> requiredItems = new List<ItemRequirement>();
    [SerializeField] private UnityEvent onStageReached;
    
    public string StageName => stageName;
    public string Description => description;
    public List<ItemRequirement> RequiredItems => requiredItems;
    public UnityEvent OnStageReached => onStageReached;
    
    public bool HasRequiredItems(InventoryComponent inventory)
    {
        if (requiredItems.Count == 0) return true;
        return requiredItems.All(req => inventory.GetItemAmount(req.item) >= req.amount);
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
        if (requiredItems.Count == 0) return "No requirements";
        
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
public class StageProgression : MonoBehaviour
{
    [Header("Stage Settings")]
    [SerializeField, Min(0)] private int startingStage;
    [SerializeField] private List<ProgressionStage> stages = new List<ProgressionStage>();

    
    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Interactable interactable;
    [SerializeField] private InteractableTooltip tooltip;
    [SerializeField] private SOAudioEvent stageSuccessSfx;
    [SerializeField] private SOAudioEvent stageFailedSfx;
    
    [Separator]
    [SerializeField, ReadOnly] private int currentStageIndex;
    
    private bool IsMaxStage => currentStageIndex >= stages.Count - 1;
    private int NextStageIndex => currentStageIndex + 1;
    
    private void OnValidate()
    {
        if (!interactable) interactable = GetComponent<Interactable>();
        if (!tooltip) tooltip = GetComponentInChildren<InteractableTooltip>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentStageIndex = Mathf.Clamp(startingStage, 0, stages.Count - 1);
        
        // Apply all stages up to current
        for (int i = 0; i <= currentStageIndex; i++)
        {
            stages[i].OnStageReached?.Invoke();
        }
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
        if (IsMaxStage) return;
        
        var inventory = interactor.GetComponent<InventoryComponent>();
        if (!inventory) return;
        
        var nextStage = stages[NextStageIndex];
        
        if (nextStage.HasRequiredItems(inventory))
        {
            nextStage.ConsumeItems(inventory);
            nextStage.OnStageReached?.Invoke();
            currentStageIndex++;
            stageSuccessSfx?.Play(audioSource);
            UpdateTooltip(interactor);
        }
        else
        {
            stageFailedSfx?.Play(audioSource);
            tooltip?.Punch(Color.red);
        }
    }
    
    private void UpdateTooltip(FPCInteraction interactor)  
    {
        if (!tooltip) return;
        
        var inventory = interactor.GetComponent<InventoryComponent>();
        if (!inventory) return;
        
        if (IsMaxStage)
        {
            var currentStage = stages[currentStageIndex];
            tooltip.SetText($"{currentStage.StageName} (Max)", "Fully Complete");
            return;
        }
        
        var current = stages[currentStageIndex];
        var next = stages[NextStageIndex];
        
        string header = $"{current.StageName} → {next.StageName}";
        string requirements = next.GetRequirementsText(inventory);
        string description = $"{next.Description}\n\n{requirements}";
        
        tooltip.SetText(header, description);
    }
}