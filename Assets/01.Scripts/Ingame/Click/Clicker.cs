using UnityEngine;

public class Clicker : MonoBehaviour
{
    private float _autoClickInterval = 1f;
    private double _damage = 1;
    private int _toolLevel = 0;

    private IClickable _currentTarget;
    private Vector2 _currentWorldPos;
    private float _holdTimer;
    private bool _isHolding;
    private bool _isAutoClickEnabled;
    private Camera _mainCamera;

    // 외부에서 업그레이드 시 호출
    public void SetAutoClickEnabled(bool value) => _isAutoClickEnabled = value;
    public void SetAutoClickInterval(float interval) => _autoClickInterval = interval;
    public void SetDamage(double damage) => _damage = damage;
    public void SetToolLevel(int level) => _toolLevel = level;

    private void Awake()
    {
        _mainCamera = Camera.main;
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
            damage: _damage,
            toolLevel: _toolLevel,
            isAutoClick: isAutoClick
        );
    }
}
