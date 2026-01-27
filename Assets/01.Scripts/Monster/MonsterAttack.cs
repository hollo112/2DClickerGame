using System;
using UnityEngine;

/// <summary>
/// 몬스터의 공격 로직을 담당 (SRP)
/// </summary>
public class MonsterAttack : MonoBehaviour
{
    private double _damage;
    private float _attackInterval;
    private float _attackRange;
    private float _attackTimer;
    private int _attackableLevel;

    public event Action OnAttackPerformed;

    public float AttackRange => _attackRange;

    public void Initialize(double damage, float attackInterval, float attackRange)
    {
        _damage = damage;
        _attackInterval = attackInterval;
        _attackRange = attackRange;
        _attackTimer = 0f;  // 즉시 첫 공격 가능
    }

    public void SetAttackableLevel(int level)
    {
        _attackableLevel = level;
    }

    public void ResetTimer()
    {
        _attackTimer = 0f;
    }

    /// <summary>
    /// 공격 시도. 타이머가 차면 공격 실행.
    /// </summary>
    /// <returns>타겟이 파괴되었으면 true</returns>
    public bool TryAttack(Resource target)
    {
        if (target == null) return true;

        _attackTimer += Time.deltaTime;

        if (_attackTimer >= _attackInterval)
        {
            _attackTimer = 0f;
            return PerformAttack(target);
        }

        return false;
    }

    private bool PerformAttack(Resource target)
    {
        if (target == null) return true;

        ClickInfo clickInfo = new ClickInfo
        {
            ToolLevel = _attackableLevel,
            Damage = _damage,
            WorldPosition = target.transform.position
        };

        bool success = target.OnClick(clickInfo);

        if (success)
        {
            OnAttackPerformed?.Invoke();
        }

        // 타겟이 파괴되었는지 확인
        return target == null || target.CurrentHp <= 0;
    }
}
