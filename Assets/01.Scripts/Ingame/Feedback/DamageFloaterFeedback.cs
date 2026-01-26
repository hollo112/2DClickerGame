using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageFloaterFeedback : MonoBehaviour, IFeedback
{
    [Header("Prefab")]
    [SerializeField] private GameObject _floaterPrefab;

    [Header("Animation Settings")]
    [SerializeField] private float _spawnOffsetY = 0.5f;
    [SerializeField] private float _moveDistance = 1f;
    [SerializeField] private float _duration = 0.8f;
    [SerializeField] private float _randomAngle = 30f;

    public void Play(ClickInfo clickInfo)
    {
        if (_floaterPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * _spawnOffsetY;
        GameObject floater = Instantiate(_floaterPrefab, spawnPos, Quaternion.identity);

        // 텍스트 설정 (획득 재화 표시)
        if (floater.TryGetComponent(out TextMeshPro tmp))
        {
            tmp.text = $"+{clickInfo.Reward}";
        }
        else if (floater.TryGetComponent(out TextMeshProUGUI tmpUI))
        {
            tmpUI.text = $"+{clickInfo.Reward}";
        }

        // 랜덤 방향 계산 (위쪽 기준 ± randomAngle)
        float angle = 90f + Random.Range(-_randomAngle, _randomAngle);
        float rad = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        Vector3 targetPos = floater.transform.position + direction * _moveDistance;

        // 이동 + 페이드 아웃 (SetLink로 오브젝트 파괴 시 자동 Kill)
        floater.transform.DOMove(targetPos, _duration)
            .SetEase(Ease.OutCubic)
            .SetLink(floater);

        floater.transform.DOScale(0.5f, _duration)
            .SetEase(Ease.InCubic)
            .SetLink(floater);

        // CanvasGroup 또는 TMP 알파 페이드
        if (floater.TryGetComponent(out CanvasGroup canvasGroup))
        {
            canvasGroup.DOFade(0f, _duration)
                .SetEase(Ease.InCubic)
                .SetLink(floater);
        }
        else if (floater.TryGetComponent(out TextMeshPro tmpFade))
        {
            tmpFade.DOFade(0f, _duration)
                .SetEase(Ease.InCubic)
                .SetLink(floater);
        }

        Destroy(floater, _duration + 0.1f);
    }
}
