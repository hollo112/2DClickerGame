using UnityEngine;
using DG.Tweening;

public class Monster : MonoBehaviour
{
    public enum State
    {
        Idle,
        Moving,
        Attacking
    }

    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Spawn Animation")]
    [SerializeField] private float _spawnDuration = 0.3f;

    private MonsterManager _manager;
    private MonsterData.TierInfo _tierInfo;
    private int _tier;
    private State _state = State.Idle;
    private Resource _targetResource;
    private float _attackTimer;
    private float _moveSpeed;

    public int Tier => _tier;
    public Vector2 Position => transform.position;

    // 몬스터가 공격 가능한 Resource 레벨 (도구 레벨 - 1, 최소 0)
    private int AttackableResourceLevel
    {
        get
        {
            int toolLevel = UpgradeManager.Instance?.ToolLevel ?? 0;
            return Mathf.Max(0, toolLevel - 1);
        }
    }

    public void Initialize(MonsterManager manager, int tier, MonsterData.TierInfo tierInfo)
    {
        _manager = manager;
        _tier = tier;
        _tierInfo = tierInfo;
        _moveSpeed = manager.Data.MoveSpeed;

        // 스프라이트 설정
        if (_spriteRenderer != null && tierInfo.Sprite != null)
        {
            _spriteRenderer.sprite = tierInfo.Sprite;
        }

        PlaySpawnAnimation();

        Debug.Log($"[Monster] {tierInfo.Name} 초기화 - 공격력: {tierInfo.AttackDamage}, 간격: {tierInfo.AttackInterval}s");
    }

    private void PlaySpawnAnimation()
    {
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;

        transform.DOScale(targetScale, _spawnDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                _state = State.Idle;
                if (TryGetComponent(out IdleAnimation idle))
                {
                    idle.Initialize(targetScale);
                }
            });
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Idle:
                FindTarget();
                break;
            case State.Moving:
                MoveToTarget();
                break;
            case State.Attacking:
                PerformAttack();
                break;
        }
    }

    private void FindTarget()
    {
        _targetResource = _manager.FindClosestResource(transform.position, AttackableResourceLevel);

        if (_targetResource != null)
        {
            _state = State.Moving;
        }
    }

    private void MoveToTarget()
    {
        if (_targetResource == null)
        {
            _state = State.Idle;
            return;
        }

        Vector2 direction = ((Vector2)_targetResource.transform.position - Position).normalized;
        float distance = Vector2.Distance(Position, _targetResource.transform.position);

        // 공격 범위 내에 들어왔으면 공격 상태로 전환
        if (distance <= _tierInfo.AttackRange)
        {
            _state = State.Attacking;
            _attackTimer = 0f;  // 즉시 첫 공격
            return;
        }

        // 이동
        Vector2 newPosition = Position + direction * _moveSpeed * Time.deltaTime;
        transform.position = newPosition;

        // 이동 방향에 따라 스프라이트 방향 조정
        if (_spriteRenderer != null && direction.x != 0)
        {
            _spriteRenderer.flipX = direction.x < 0;
        }

        // MonsterManager에 위치 업데이트
        _manager.UpdateMonsterPosition(this, newPosition);
    }

    private void PerformAttack()
    {
        // 타겟이 없어졌으면 Idle로
        if (_targetResource == null)
        {
            _state = State.Idle;
            return;
        }

        // 타겟이 범위를 벗어났으면 다시 이동
        float distance = Vector2.Distance(Position, _targetResource.transform.position);
        if (distance > _tierInfo.AttackRange * 1.2f)  // 약간의 여유
        {
            _state = State.Moving;
            return;
        }

        _attackTimer += Time.deltaTime;

        if (_attackTimer >= _tierInfo.AttackInterval)
        {
            _attackTimer = 0f;
            AttackTarget();
        }
    }

    private void AttackTarget()
    {
        if (_targetResource == null) return;

        // ClickInfo 생성하여 Resource 공격
        ClickInfo clickInfo = new ClickInfo
        {
            ToolLevel = AttackableResourceLevel,  // 도구 레벨 - 1 (최소 0)
            Damage = _tierInfo.AttackDamage,
            WorldPosition = _targetResource.transform.position
        };

        bool success = _targetResource.OnClick(clickInfo);

        if (success)
        {
            PlayAttackAnimation();
        }

        // 타겟이 파괴되었는지 확인
        if (_targetResource == null || _targetResource.CurrentHp <= 0)
        {
            _targetResource = null;
            _state = State.Idle;
        }
    }

    private void PlayAttackAnimation()
    {
        transform.DOKill(true);

        Vector3 originalScale = transform.localScale;
        transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 5, 0.5f)
            .OnComplete(() =>
            {
                if (TryGetComponent(out IdleAnimation idle))
                {
                    idle.Initialize(originalScale);
                }
            });
    }

    public void OnMerged()
    {
        transform.DOKill();

        // 머지 애니메이션 (축소 후 파괴)
        transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
