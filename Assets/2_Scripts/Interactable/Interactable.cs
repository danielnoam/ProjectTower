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
    
    [Header("References")]
    [SerializeField] private Outline outline;
    
    private Color _outlineColor = Color.mediumPurple;
    private float _outlineWidthOnHighlight = 7f;
    private float _outlineWidthOnUnHighlight;
    
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
            outline.OutlineWidth = _outlineWidthOnUnHighlight;
            outline.OutlineColor = _outlineColor;
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
                    endValue: _outlineWidthOnHighlight,
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
                    endValue: _outlineWidthOnUnHighlight,
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