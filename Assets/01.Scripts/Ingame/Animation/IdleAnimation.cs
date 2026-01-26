using UnityEngine;
using DG.Tweening;

public class IdleAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField, Range(0.05f, 0.3f)] private float _squashRatio = 0.15f;
    [SerializeField] private float _duration = 0.8f;
    [SerializeField] private Ease _ease = Ease.InOutSine;

    private Vector3 _originalScale;
    private Tween _tween;
    private bool _initialized;

    public void Initialize(Vector3 originalScale)
    {
        _originalScale = originalScale;
        _initialized = true;
        Play();
    }

    public void SetOriginalScale(Vector3 scale)
    {
        _originalScale = scale;
        if (_initialized)
        {
            Stop();
            Play();
        }
    }

    public void Play()
    {
        Stop();

        float scaleX = Mathf.Abs(_originalScale.x);
        float scaleY = Mathf.Abs(_originalScale.y);
        float dirX = Mathf.Sign(_originalScale.x);

        // Squash: 납작 (x 15% 커지고, y 15% 작아짐)
        Vector3 squash = new Vector3(
            scaleX * (1f + _squashRatio) * dirX,
            scaleY * (1f - _squashRatio),
            _originalScale.z
        );

        // Stretch: 길쭉 (x 10% 작아지고, y 10% 커짐)
        Vector3 stretch = new Vector3(
            scaleX * (1f - _squashRatio * 0.6f) * dirX,
            scaleY * (1f + _squashRatio * 0.6f),
            _originalScale.z
        );

        _tween = DOTween.Sequence()
            .Append(transform.DOScale(squash, _duration * 0.25f).SetEase(_ease))
            .Append(transform.DOScale(_originalScale, _duration * 0.25f).SetEase(_ease))
            .Append(transform.DOScale(stretch, _duration * 0.25f).SetEase(_ease))
            .Append(transform.DOScale(_originalScale, _duration * 0.25f).SetEase(_ease))
            .SetLoops(-1, LoopType.Restart);
    }

    public void Stop()
    {
        _tween?.Kill();
        transform.localScale = _originalScale;
    }

    public void Pause()
    {
        _tween?.Pause();
        transform.localScale = _originalScale;
    }

    public void Resume()
    {
        if (_initialized)
            Play();
    }

    private void OnDestroy()
    {
        _tween?.Kill();
    }
}