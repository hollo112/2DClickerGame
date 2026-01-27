using System;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 몬스터의 모든 애니메이션을 담당 (SRP)
/// </summary>
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
    [SerializeField] private float _attackWindupAngle = 15f;    // 힘 모으기 각도
    [SerializeField] private float _attackSwingAngle = 25f;     // 휘두르기 각도
    [SerializeField] private float _attackWindupDuration = 0.1f;
    [SerializeField] private float _attackSwingDuration = 0.08f;
    [SerializeField] private float _attackReturnDuration = 0.15f;

    [Header("Merge Animation")]
    [SerializeField] private float _mergeDuration = 0.2f;

    private Vector3 _originalScale;
    private Tween _currentTween;
    private Tween _idleTween;

    public event Action OnSpawnComplete;
    public event Action OnMergeComplete;

    public void Initialize(Vector3 originalScale)
    {
        _originalScale = originalScale;
    }

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
        KillAllTweens();
        transform.localScale = _originalScale;
        transform.localRotation = Quaternion.identity;

        // Squash & Stretch idle 애니메이션
        float scaleX = Mathf.Abs(_originalScale.x);
        float scaleY = Mathf.Abs(_originalScale.y);
        float dirX = Mathf.Sign(_originalScale.x);

        Vector3 squash = new Vector3(
            scaleX * (1f + _idleSquashRatio) * dirX,
            scaleY * (1f - _idleSquashRatio),
            _originalScale.z
        );
        Vector3 stretch = new Vector3(
            scaleX * (1f - _idleSquashRatio * 0.6f) * dirX,
            scaleY * (1f + _idleSquashRatio * 0.6f),
            _originalScale.z
        );

        _idleTween = DOTween.Sequence()
            .Append(transform.DOScale(squash, _idleDuration * 0.25f).SetEase(Ease.InOutSine))
            .Append(transform.DOScale(_originalScale, _idleDuration * 0.25f).SetEase(Ease.InOutSine))
            .Append(transform.DOScale(stretch, _idleDuration * 0.25f).SetEase(Ease.InOutSine))
            .Append(transform.DOScale(_originalScale, _idleDuration * 0.25f).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Restart);
    }

    public void PlayMove()
    {
        KillAllTweens();
        transform.localScale = _originalScale;
        transform.localRotation = Quaternion.identity;

        // Squash & Stretch 바운스 애니메이션
        Vector3 stretchScale = new Vector3(
            _originalScale.x * (1f - _bounceHeight * 0.5f),
            _originalScale.y * (1f + _bounceHeight),
            _originalScale.z
        );
        Vector3 squashScale = new Vector3(
            _originalScale.x * (1f + _bounceHeight * 0.5f),
            _originalScale.y * (1f - _bounceHeight * 0.5f),
            _originalScale.z
        );

        Sequence bounceSeq = DOTween.Sequence();
        bounceSeq.Append(transform.DOScale(stretchScale, _bounceSpeed).SetEase(Ease.OutQuad));
        bounceSeq.Append(transform.DOScale(squashScale, _bounceSpeed).SetEase(Ease.InQuad));
        bounceSeq.SetLoops(-1);

        _currentTween = bounceSeq;
    }

    public void StopMove()
    {
        KillAllTweens();
        transform.localScale = _originalScale;
        transform.localRotation = Quaternion.identity;
        PlayIdle();
    }

    /// <summary>
    /// 공격 애니메이션 - 타겟 방향으로 휘두르기
    /// </summary>
    /// <param name="targetDirection">타겟이 있는 방향 (right = 1, left = -1)</param>
    public void PlayAttack(float targetDirection)
    {
        KillAllTweens();
        transform.localScale = _originalScale;
        transform.localRotation = Quaternion.identity;

        // 타겟이 오른쪽이면: 왼쪽으로 기울였다가(+각도) → 오른쪽으로 휙(-각도) → 원래대로
        // 타겟이 왼쪽이면: 오른쪽으로 기울였다가(-각도) → 왼쪽으로 휙(+각도) → 원래대로
        float windupAngle = _attackWindupAngle * targetDirection;
        float swingAngle = -_attackSwingAngle * targetDirection;

        Sequence attackSeq = DOTween.Sequence();

        // 힘 모으기 (반대 방향으로 기울임)
        attackSeq.Append(transform.DORotate(new Vector3(0, 0, windupAngle), _attackWindupDuration)
            .SetEase(Ease.OutQuad));

        // 휘두르기 (타겟 방향으로)
        attackSeq.Append(transform.DORotate(new Vector3(0, 0, swingAngle), _attackSwingDuration)
            .SetEase(Ease.OutQuad));

        // 원래대로
        attackSeq.Append(transform.DORotate(Vector3.zero, _attackReturnDuration)
            .SetEase(Ease.OutBack));

        attackSeq.OnComplete(PlayIdle);

        _currentTween = attackSeq;
    }

    public void PlayMerge()
    {
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
