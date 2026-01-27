using UnityEngine;

/// <summary>
/// 몬스터의 상태를 관리하고 각 컴포넌트를 조율 (코디네이터)
/// </summary>
[RequireComponent(typeof(MonsterAnimator))]
[RequireComponent(typeof(MonsterMovement))]
[RequireComponent(typeof(MonsterAttack))]
public class Monster : MonoBehaviour
{
    public enum State
    {
        Spawning,
        Idle,
        Moving,
        Attacking
    }

    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    // 컴포넌트 참조
    private MonsterAnimator _animator;
    private MonsterMovement _movement;
    private MonsterAttack _attack;

    // 상태
    private MonsterManager _manager;
    private MonsterData.TierInfo _tierInfo;
    private int _tier;
    private State _state = State.Spawning;
    private Resource _targetResource;

    // 프로퍼티
    public int Tier => _tier;
    public Vector2 Position => transform.position;
    public Resource TargetResource => _targetResource;

    private int AttackableResourceLevel
    {
        get
        {
            // 플레이어와 동일한 레벨까지 공격 가능
            return UpgradeManager.Instance?.ToolLevel ?? 0;
        }
    }

    private void Awake()
    {
        _animator = GetComponent<MonsterAnimator>();
        _movement = GetComponent<MonsterMovement>();
        _attack = GetComponent<MonsterAttack>();
    }

    public void Initialize(MonsterManager manager, int tier, MonsterData.TierInfo tierInfo)
    {
        _manager = manager;
        _tier = tier;
        _tierInfo = tierInfo;

        // 스프라이트 설정
        if (_spriteRenderer != null && tierInfo.Sprite != null)
        {
            _spriteRenderer.sprite = tierInfo.Sprite;
        }

        // 컴포넌트 초기화
        _animator.Initialize(transform.localScale);
        _animator.OnSpawnComplete += OnSpawnComplete;
        _animator.OnMergeComplete += OnMergeComplete;
        _attack.OnAttackPerformed += OnAttackPerformed;

        _movement.Initialize(manager.Data.MoveSpeed);
        _movement.OnPositionChanged += OnPositionChanged;

        _attack.Initialize(tierInfo.AttackDamage, tierInfo.AttackInterval, tierInfo.AttackRange);

        // 스폰 애니메이션 시작
        _state = State.Spawning;
        _animator.PlaySpawn();

        Debug.Log($"[Monster] {tierInfo.Name} 초기화 - 공격력: {tierInfo.AttackDamage}, 간격: {tierInfo.AttackInterval}s");
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Spawning:
                // 애니메이션 완료 대기
                break;
            case State.Idle:
                HandleIdle();
                break;
            case State.Moving:
                HandleMoving();
                break;
            case State.Attacking:
                HandleAttacking();
                break;
        }
    }

    private void HandleIdle()
    {
        _targetResource = _manager.FindRandomResource(AttackableResourceLevel);

        if (_targetResource != null)
        {
            _movement.SetTarget(_targetResource.transform);
            _attack.SetAttackableLevel(AttackableResourceLevel);
            ChangeState(State.Moving);
        }
    }

    private void HandleMoving()
    {
        if (!_movement.HasTarget || _targetResource == null)
        {
            _movement.ClearTarget();
            ChangeState(State.Idle);
            return;
        }

        if (_movement.IsInRange(_attack.AttackRange))
        {
            _attack.ResetTimer();
            ChangeState(State.Attacking);
            return;
        }

        _movement.Move();
    }

    private void HandleAttacking()
    {
        if (_targetResource == null)
        {
            ChangeState(State.Idle);
            return;
        }

        // 타겟이 범위를 벗어났으면 다시 이동
        if (_movement.IsOutOfRange(_attack.AttackRange * 1.2f))
        {
            ChangeState(State.Moving);
            return;
        }

        bool targetDestroyed = _attack.TryAttack(_targetResource);
        if (targetDestroyed)
        {
            _targetResource = null;
            _movement.ClearTarget();
            ChangeState(State.Idle);
        }
    }

    private void ChangeState(State newState)
    {
        if (_state == newState) return;

        // 이전 상태 종료 처리
        switch (_state)
        {
            case State.Moving:
                _animator.StopMove();
                break;
        }

        _state = newState;

        // 새 상태 시작 처리
        switch (newState)
        {
            case State.Idle:
                _animator.PlayIdle();
                break;
            case State.Moving:
                _animator.PlayMove();
                break;
            case State.Attacking:
                _animator.PlayIdle();
                break;
        }
    }

    #region Event Handlers

    private void OnSpawnComplete()
    {
        _state = State.Idle;
    }

    private void OnMergeComplete()
    {
        Destroy(gameObject);
    }

    private void OnAttackPerformed()
    {
        // 타겟 방향 계산 (오른쪽 = 1, 왼쪽 = -1)
        float direction = 1f;
        if (_targetResource != null)
        {
            direction = _targetResource.transform.position.x > transform.position.x ? 1f : -1f;
        }
        _animator.PlayAttack(direction);
    }

    private void OnPositionChanged(Vector2 newPosition)
    {
        _manager.UpdateMonsterPosition(this, newPosition);
    }

    #endregion

    public void OnMerged()
    {
        _animator.PlayMerge();
    }

    private void OnDestroy()
    {
        if (_animator != null)
        {
            _animator.OnSpawnComplete -= OnSpawnComplete;
            _animator.OnMergeComplete -= OnMergeComplete;
        }
        if (_attack != null)
        {
            _attack.OnAttackPerformed -= OnAttackPerformed;
        }
        if (_movement != null)
        {
            _movement.OnPositionChanged -= OnPositionChanged;
        }
    }
}
