using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DomainButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Image backgroundImage;
    
    [Header("Visual States")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.grey;
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 selectedScale = new Vector3(0.9f, 0.9f, 0.9f);
    
    private Domain _domain;
    private bool _isSelected;
    
    public event Action<Domain, bool> StateChanged;
    
    public Domain Domain => _domain;
    public bool IsSelected => _isSelected;
    
    private void Awake()
    {
        button.onClick.AddListener(ToggleSelection);
    }
    
    public void Initialize(Domain domain)
    {
        _domain = domain;
        labelText.text = domain.ToString();
        SetSelected(false);
    }
    
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateVisuals();
    }
    
    private void ToggleSelection()
    {
        StateChanged?.Invoke(_domain, !_isSelected);
    }
    
    private void UpdateVisuals()
    {
        backgroundImage.color = _isSelected ? selectedColor : normalColor;
        transform.localScale = _isSelected ? selectedScale : normalScale;
    }
}