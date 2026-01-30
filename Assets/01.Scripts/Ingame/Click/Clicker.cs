using UnityEngine;

public class Clicker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private double _baseClickDamage = 1;

    private float _autoClickInterval = 1f;
    private double _damage = 1;
    private double _autoDamage = 1;
    private int _toolLevel = 0;

    private IClickable _currentTarget;
    private Vector2 _currentWorldPos;
    private float _holdTimer;
    private bool _isHolding;
    private bool _isAutoClickEnabled;
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        UpgradeManager.OnDataChanged += RefreshStats;
        RefreshStats();
    }

    private void OnDestroy()
    {
        UpgradeManager.OnDataChanged -= RefreshStats;
    }

    private void RefreshStats()
    {
        if (UpgradeManager.Instance == null) return;
        var upgrade = UpgradeManager.Instance;

        // 도구 레벨
        _toolLevel = upgrade.Get(EUpgradeType.ToolLevel)?.Level ?? 0;

        // 클릭 데미지
        double clickDamageValue = upgrade.Get(EUpgradeType.ClickDamage)?.Value ?? 0;
        double totalDamage = _baseClickDamage + clickDamageValue;
        _damage = totalDamage;
        _autoDamage = totalDamage;

        // 오토클릭
        var autoClick = upgrade.Get(EUpgradeType.AutoClick);
        int autoClickLevel = autoClick?.Level ?? 0;
        _isAutoClickEnabled = autoClickLevel >= 1;
        if (autoClickLevel >= 1)
            _autoClickInterval = Mathf.Max((float)autoClick.Value, 0.1f);
    }

    private void Update()
    {
        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        // 클릭 시작
        if (Input.GetMouseButtonDown(0))
        {
            if (hit && hit.collider.TryGetComponent(out IClickable target))
            {
                _currentTarget = target;
                _currentWorldPos = worldPos;

                ClickInfo clickInfo = CreateClickInfo(worldPos, isAutoClick: false);
                _currentTarget.OnClick(clickInfo);

                _isHolding = true;
                _holdTimer = 0f;
            }
        }

        // 홀드 중 (오토클릭)
        if (_isAutoClickEnabled && _isHolding && Input.GetMouseButton(0))
        {
            if (hit && hit.collider.TryGetComponent(out IClickable target)
                && target == _currentTarget)
            {
                _holdTimer += Time.deltaTime;
                if (_holdTimer >= _autoClickInterval)
                {
                    ClickInfo clickInfo = CreateClickInfo(worldPos, isAutoClick: true);
                    _currentTarget.OnClick(clickInfo);
                    _holdTimer = 0f;
                }
            }
            else
            {
                _isHolding = false;
            }
        }

        // 클릭 해제
        if (Input.GetMouseButtonUp(0))
        {
            _isHolding = false;
            _currentTarget = null;
        }
    }

    private ClickInfo CreateClickInfo(Vector2 worldPos, bool isAutoClick)
    {
        return new ClickInfo(
            worldPosition: worldPos,
            damage: isAutoClick ? _autoDamage : _damage,
            toolLevel: _toolLevel,
            isAutoClick: isAutoClick
        );
    }
}
