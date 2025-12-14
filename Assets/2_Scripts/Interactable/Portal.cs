using UnityEngine;
using UnityEngine.Events;

public class Portal : MonoBehaviour
{
    [SerializeField] private Interactable interactable;
    [SerializeField] private UnityEvent onPortalActivated;

    private void OnEnable()
    {
        if (interactable != null)
        {
            interactable.OnInteract += HandleInteract;
        }
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.OnInteract -= HandleInteract;
        }
    }

    private void HandleInteract(FPCInteraction interactor)
    {
        onPortalActivated?.Invoke();
    }
}