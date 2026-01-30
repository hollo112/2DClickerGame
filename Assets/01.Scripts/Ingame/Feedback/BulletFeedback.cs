using UnityEngine;

public class BulletFeedback : MonoBehaviour, IFeedback
{
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _screenEdgeOffset = 1f;

    public void Play(ClickInfo clickInfo)
    {
        if (_bulletPrefab == null) return;
        if (clickInfo.Type == ClickerType.Monster) return;

        Vector3 spawnPos = GetRandomScreenEdgePosition();
        Vector3 targetPos = clickInfo.WorldPosition;

        Bullet bullet = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
        bullet.Initialize(targetPos, _speed);
    }

    private Vector3 GetRandomScreenEdgePosition()
    {
        Camera cam = Camera.main;

        // 화면 4변 중 랜덤 선택 (0: 상, 1: 하, 2: 좌, 3: 우)
        int edge = Random.Range(0, 4);
        float viewportX = 0f;
        float viewportY = 0f;

        switch (edge)
        {
            case 0: // 상
                viewportX = Random.Range(0f, 1f);
                viewportY = 1f;
                break;
            case 1: // 하
                viewportX = Random.Range(0f, 1f);
                viewportY = 0f;
                break;
            case 2: // 좌
                viewportX = 0f;
                viewportY = Random.Range(0f, 1f);
                break;
            case 3: // 우
                viewportX = 1f;
                viewportY = Random.Range(0f, 1f);
                break;
        }

        Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(viewportX, viewportY, cam.nearClipPlane));
        worldPos.z = 0f;

        // 화면 밖으로 오프셋 적용
        Vector3 screenCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cam.nearClipPlane));
        screenCenter.z = 0f;
        Vector3 outwardDir = (worldPos - screenCenter).normalized;
        worldPos += outwardDir * _screenEdgeOffset;

        return worldPos;
    }
}
