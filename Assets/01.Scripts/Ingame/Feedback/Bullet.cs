using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 _targetPosition;
    private float _speed;
    private bool _initialized;

    public void Initialize(Vector3 targetPosition, float speed)
    {
        _targetPosition = targetPosition;
        _speed = speed;
        _initialized = true;

        // 이동 방향으로 회전
        Vector3 direction = (_targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        if (!_initialized) return;

        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
        {
            Destroy(gameObject);
        }
    }
}
