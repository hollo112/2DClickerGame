using System.Linq;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public enum State { Spawning, Idle, Moving, Attacking }

    [Header("--- Debug Info ---")]
    [SerializeField] private int _currentTier;
    [SerializeField] private bool _isRegisteredInManager;
    [SerializeField] private State _currentState;
    [SerializeField] private string _targetName;
    [SerializeField] private bool _isMerging = false; // 머지 중인지 확인

    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private MonsterAnimator _animator;
    private MonsterMovement _movement;
    private MonsterAttack _attack;
    private MonsterManager _manager;
    private MonsterData.TierInfo _tierInfo;
    private Resource _targetResource;

    public int Tier => _currentTier;
    public Resource TargetResource => _targetResource;

    private void Awake()
    {
        _animator = GetComponent<MonsterAnimator>();
        _movement = GetComponent<MonsterMovement>();
        _attack = GetComponent<MonsterAttack>();
    }

    public void Initialize(MonsterManager manager, int tier, MonsterData.TierInfo tierInfo)
    {
        _manager = manager;
        _currentTier = tier;
        _tierInfo = tierInfo;

        if (_spriteRenderer != null && tierInfo.Sprite != null)
            _spriteRenderer.sprite = tierInfo.Sprite;

        _animator.Initialize(transform.localScale);
        _animator.OnSpawnComplete += () => ChangeState(State.Idle);
        _animator.OnMergeComplete += () => Destroy(gameObject);
        
        _movement.Initialize(manager.Data.MoveSpeed);
        _attack.Initialize(tierInfo.AttackDamage, tierInfo.AttackInterval, tierInfo.AttackRange);
        _attack.OnAttackPerformed += OnAttackPerformed;

        RefreshDebugInfo();
        _currentState = State.Spawning;
        _animator.PlaySpawn();
    }

    private void Update()
    {
        // 머지 중이거나 스폰 중이면 모든 로직 중단
        if (_isMerging || _currentState == State.Spawning) return;

        if (MonsterManager.Instance != null)
        {
            bool currentlyRegistered = MonsterManager.Instance.Monsters.Contains(this);
            if (_isRegisteredInManager != currentlyRegistered)
            {
                _isRegisteredInManager = currentlyRegistered;
                string regStatus = _isRegisteredInManager ? "OK" : "MISSING";
                gameObject.name = $"Monster_T{_currentTier} [{regStatus}]";
            }
        }

        _targetName = _targetResource != null ? _targetResource.name : "None";

        switch (_currentState)
        {
            case State.Idle: HandleIdle(); break;
            case State.Moving: HandleMoving(); break;
            case State.Attacking: HandleAttacking(); break;
        }
    }

    private void RefreshDebugInfo()
    {
        if (_manager != null)
            _isRegisteredInManager = _manager.Monsters.Contains(this);
        
        string regStatus = _isRegisteredInManager ? "OK" : "MISSING";
        gameObject.name = $"Monster_T{_currentTier} [{regStatus}]";
    }

    private void ChangeState(State newState)
    {
        if (_isMerging || _currentState == newState) return;
        if (_currentState == State.Moving) _animator.StopMove();
        
        _currentState = newState;

        switch (newState)
        {
            case State.Idle: _animator.PlayIdle(); break;
            case State.Moving: _animator.PlayMove(); break;
            case State.Attacking: _animator.PlayIdle(); break;
        }
    }

    private void HandleIdle()
    {
        _targetResource = _manager.FindRandomResource(_manager.CurrentToolLevel);
        if (_targetResource != null)
        {
            _movement.SetTarget(_targetResource.transform);
            _attack.SetAttackableLevel(_manager.CurrentToolLevel);
            ChangeState(State.Moving);
        }
    }

    private void HandleMoving()
    {
        if (_targetResource == null) { ChangeState(State.Idle); return; }
        if (_movement.IsInRange(_attack.AttackRange)) { _attack.ResetTimer(); ChangeState(State.Attacking); return; }
        _movement.Move();
    }

    private void HandleAttacking()
    {
        if (_targetResource == null) { ChangeState(State.Idle); return; }
        if (_attack.TryAttack(_targetResource)) { _targetResource = null; _movement.ClearTarget(); ChangeState(State.Idle); }
    }

    private void OnAttackPerformed()
    {   
        if (_isMerging) return;
        float dir = (_targetResource != null && _targetResource.transform.position.x > transform.position.x) ? 1f : -1f;
        _animator.PlayAttack(dir);
    }

    public void OnMerged()
    {
        if (_isMerging) return;
        _isMerging = true;

        // 물리/타겟팅 즉시 차단
        if (TryGetComponent(out Collider2D col)) col.enabled = false;
        _targetResource = null;
        _movement.ClearTarget();

        _animator.PlayMerge();

        // [안전장치] 애니메이션 트윈이 끊겨도 0.5초 후에는 무조건 파괴
        Destroy(gameObject, 0.5f);
    }
}