using System;
using DNExtensions;
using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private float outlineWidthOnHighlight = 5f;
    [SerializeField] private float outlineWidthOnUnHighlight;
    
    [Header("References")]
    [SerializeField] private Outline outline;
    
    private bool _isHighlighted;
    private Sequence _highlightSequence;
    
    public bool CanInteract => canInteract;
    
    public event Action<FPCInteraction> OnInteract;
    public event Action<FPCInteraction> OnUnHighlight; 
    public event Action<FPCInteraction> OnHighlight;   


    private void Awake()
    {
        if (outline)
        {
            outline.OutlineWidth = outlineWidthOnUnHighlight;
        }
    }
    

    public void Highlight(FPCInteraction interactor) 
    {
        if (_isHighlighted) return;

        _isHighlighted = true;
        
        if (outline)
        {
            if (_highlightSequence.isAlive) _highlightSequence.Stop();
            _highlightSequence = Sequence.Create()
                .Group(Tween.Custom(
                    startValue: outline.OutlineWidth,
                    endValue: outlineWidthOnHighlight,
                    duration: 0.3f,
                    onValueChange: value => outline.OutlineWidth = value
                    ));
        }
        OnHighlight?.Invoke(interactor); 
    }
    
    public void UnHighlight(FPCInteraction interactor) 
    {
        if (!_isHighlighted) return;
        
        _isHighlighted = false;
        if (outline)
        {
            if (_highlightSequence.isAlive) _highlightSequence.Stop();
            _highlightSequence = Sequence.Create()
                .Group(Tween.Custom(
                    startValue: outline.OutlineWidth,
                    endValue: outlineWidthOnUnHighlight,
                    duration: 0.2f,
                    onValueChange: value => outline.OutlineWidth = value
                ));
        }
        OnUnHighlight?.Invoke(interactor); 
    }
    
    public void Interact(FPCInteraction interactor)
    {
        if (!canInteract) return;
        OnInteract?.Invoke(interactor);
    }
    
    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }
}