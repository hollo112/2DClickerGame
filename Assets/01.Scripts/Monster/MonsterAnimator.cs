using System;
using UnityEngine;
using DG.Tweening;

public class MonsterAnimator : MonoBehaviour
{
    [Header("Spawn Animation")]
    [SerializeField] private float _spawnDuration = 0.3f;

    [Header("Idle Animation")]
    [SerializeField] private float _idleSquashRatio = 0.1f;
    [SerializeField] private float _idleDuration = 0.8f;

    [Header("Move Animation")]
    [SerializeField] private float _bounceHeight = 0.15f;
    [SerializeField] private float _bounceSpeed = 0.15f;

    [Header("Attack Animation")]
    [SerializeField] private float _attackWindupAngle = 15f;
    [SerializeField] private float _attackSwingAngle = 25f;
    [SerializeField] private float _attackWindupDuration = 0.1f;
    [SerializeField] private float _attackSwingDuration = 0.08f;
    [SerializeField] private float _attackReturnDuration = 0.15f;

    [Header("Merge Animation")]
    [SerializeField] private float _mergeDuration = 0.2f;

    private Vector3 _originalScale;
    private Tween _currentTween;
    private Tween _idleTween;
    private bool _isMerging = false; // 머지 중 여부

    public event Action OnSpawnComplete;
    public event Action OnMergeComplete;

    public void Initialize(Vector3 originalScale)
    {
        _originalScale = originalScale;
        _isMerging = false;
    }

    private bool CanPlay() => !_isMerging;

    public void PlaySpawn()
    {
        KillAllTweens();
        transform.localScale = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        _currentTween = transform.DOScale(_originalScale, _spawnDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                PlayIdle();
                OnSpawnComplete?.Invoke();
            });
    }

    public void PlayIdle()
    {
        if (!CanPlay()) return;
        KillAllTweens();
        
        transform.localScale = _originalScale;
        transform.localRotation = Quaternion.identity;

        float scaleX = Mathf.Abs(_originalScale.x);
        float scaleY = Mathf.Abs(_originalScale.y);
        float dirX = Mathf.Sign(_originalScale.x);

        Vector3 squash = new Vector3(scaleX * (1f + _idleSquashRatio) * dirX, scaleY * (1f - _idleSquashRatio), _originalScale.z);
        Vector3 stretch = new Vector3(scaleX * (1f - _idleSquashRatio * 0.6f) * dirX, scaleY * (1f + _idleSquashRatio * 0.6f), _originalScale.z);

        _idleTween = DOTween.Sequence()
            .Append(transform.DOScale(squash, _idleDuration * 0.25f).SetEase(Ease.InOutSine))
            .Append(transform.DOScale(_originalScale, _idleDuration * 0.25f).SetEase(Ease.InOutSine))
            .Append(transform.DOScale(stretch, _idleDuration * 0.25f).SetEase(Ease.InOutSine))
            .Append(transform.DOScale(_originalScale, _idleDuration * 0.25f).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Restart);
    }

    public void PlayMove()
    {
        if (!CanPlay()) return;
        KillAllTweens();

        Vector3 stretchScale = new Vector3(_originalScale.x * (1f - _bounceHeight * 0.5f), _originalScale.y * (1f + _bounceHeight), _originalScale.z);
        Vector3 squashScale = new Vector3(_originalScale.x * (1f + _bounceHeight * 0.5f), _originalScale.y * (1f - _bounceHeight * 0.5f), _originalScale.z);

        _currentTween = DOTween.Sequence()
            .Append(transform.DOScale(stretchScale, _bounceSpeed).SetEase(Ease.OutQuad))
            .Append(transform.DOScale(squashScale, _bounceSpeed).SetEase(Ease.InQuad))
            .SetLoops(-1);
    }

    public void StopMove()
    {
        if (!CanPlay()) return;
        PlayIdle();
    }

    public void PlayAttack(float targetDirection)
    {
        if (!CanPlay()) return;
        KillAllTweens();

        float windupAngle = _attackWindupAngle * targetDirection;
        float swingAngle = -_attackSwingAngle * targetDirection;

        _currentTween = DOTween.Sequence()
            .Append(transform.DORotate(new Vector3(0, 0, windupAngle), _attackWindupDuration).SetEase(Ease.OutQuad))
            .Append(transform.DORotate(new Vector3(0, 0, swingAngle), _attackSwingDuration).SetEase(Ease.OutQuad))
            .Append(transform.DORotate(Vector3.zero, _attackReturnDuration).SetEase(Ease.OutBack))
            .OnComplete(PlayIdle);
    }

    public void PlayMerge()
    {
        _isMerging = true; // 머지 시작 플래그 고정
        KillAllTweens();

        _currentTween = transform.DOScale(Vector3.zero, _mergeDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => OnMergeComplete?.Invoke());
    }

    private void KillAllTweens()
    {
        _currentTween?.Kill();
        _currentTween = null;
        _idleTween?.Kill();
        _idleTween = null;
    }

    private void OnDestroy()
    {
        KillAllTweens();
        transform.DOKill();
    }
}