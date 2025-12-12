using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UITooltip : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 1f;
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private Vector2 tooltipOffset = new Vector2(10f, 10f);
    
    [Header("References")]
    [SerializeField] private CanvasGroup tooltipCanvas;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI tooltipText;

    private Sequence _visibilitySequence;
    private RectTransform _tooltipRect;
    private Canvas _rootCanvas;
    private bool _isVisible;

    private void Awake()
    {
        _tooltipRect = tooltipCanvas.GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        Hide(false);
    }

    private void Update()
    {
        if (_isVisible)
        {
            UpdateTooltipPosition();
        }
    }

    public void Show(string text, bool animate = true)
    {
        SetText(text);
        
        if (_visibilitySequence.isAlive) _visibilitySequence.Stop();

        if (animate)
        {
            _isVisible = true;
            _visibilitySequence = Sequence.Create()
                .Group(Tween.Alpha(tooltipCanvas, maxAlpha, animationDuration));
        }
        else
        {
            tooltipCanvas.alpha = maxAlpha;
            _isVisible = true;
        }
    }

    public void Hide(bool animate = true)
    {
        if (_visibilitySequence.isAlive) _visibilitySequence.Stop();

        if (animate)
        {
            _visibilitySequence = Sequence.Create()
                .Group(Tween.Alpha(tooltipCanvas, 0f, animationDuration))
                .OnComplete(() => _isVisible = false);
        }
        else
        {
            tooltipCanvas.alpha = 0f;
            _isVisible = false;
        }
    }

    private void UpdateTooltipPosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rootCanvas.transform as RectTransform,
            Input.mousePosition,
            null,
            out Vector2 localPoint
        );

        _tooltipRect.anchoredPosition = localPoint + tooltipOffset;
    }

    private void SetText(string text)
    {
        if (tooltipText) tooltipText.text = text;
    }
}