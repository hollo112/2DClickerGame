using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class ColorFlashFeedback : MonoBehaviour, IFeedback
{
    private SpriteRenderer _spriteRenderer;
    private Tween _tween;

    [SerializeField] private Color _flashColor = Color.white;
    [SerializeField] private float _duration = 0.2f;

    private Color _originalColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    public void Play(ClickInfo clickInfo)
    {
        _tween?.Kill();

        _spriteRenderer.color = _originalColor;

        _tween = DOTween.Sequence()
            .Append(_spriteRenderer.DOColor(_flashColor, _duration * 0.5f))
            .Append(_spriteRenderer.DOColor(_originalColor, _duration * 0.5f));
    }

    private void OnDisable()
    {
        _tween?.Kill();
    }
}