using System;
using UnityEngine;

/// <summary>
/// 몬스터의 이동 로직을 담당 (SRP)
/// </summary>
public class MonsterMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private float _moveSpeed;
    private Transform _target;

    public event Action<Vector2> OnPositionChanged;
    public event Action OnTargetReached;

    public void Initialize(float moveSpeed)
    {
        _moveSpeed = moveSpeed;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void ClearTarget()
    {
        _target = null;
    }

    public bool HasTarget => _target != null;

    public bool IsInRange(float range)
    {
        if (_target == null) return false;
        return Vector2.Distance(transform.position, _target.position) <= range;
    }

    public bool IsOutOfRange(float range)
    {
        if (_target == null) return true;
        return Vector2.Distance(transform.position, _target.position) > range;
    }

    public void Move()
    {
        if (_target == null) return;

        Vector2 currentPos = transform.position;
        Vector2 targetPos = _target.position;
        Vector2 direction = (targetPos - currentPos).normalized;

        // 이동
        Vector2 newPosition = currentPos + direction * _moveSpeed * Time.deltaTime;
        transform.position = newPosition;

        // 스프라이트 방향 조정
        UpdateSpriteDirection(direction);

        // 위치 변경 알림
        OnPositionChanged?.Invoke(newPosition);
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (_spriteRenderer != null && direction.x != 0)
        {
            _spriteRenderer.flipX = direction.x < 0;
        }
    }
}
