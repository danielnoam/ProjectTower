using DNExtensions;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private SOItem itemData;
    [SerializeField] private int amount = 1;
    [SerializeField] private Interactable interactable;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SOAudioEvent collectSfx;
    
    
    private void OnValidate()
    {
        if (!interactable) interactable = GetComponent<Interactable>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }
    
    private void OnEnable()
    {
        interactable.OnInteract += OnCollect;
    }
    
    private void OnDisable()
    {
        interactable.OnInteract -= OnCollect;
    }
    
    private void OnCollect(FPCInteraction interactor)
    {
        var inventory = interactor.GetComponent<InventoryComponent>();
        if (!inventory) return;
        
        inventory.AddItem(itemData, amount);
        collectSfx?.Play(audioSource);
        
        Destroy(gameObject);
    }
    
    public void SetAmount(int amount)
    {
        this.amount = amount;
    }
}