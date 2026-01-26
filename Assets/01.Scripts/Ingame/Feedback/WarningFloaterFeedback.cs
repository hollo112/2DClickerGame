using UnityEngine;
using TMPro;
using DG.Tweening;

public class WarningFloaterFeedback : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject _floaterPrefab;

    [Header("Animation Settings")]
    [SerializeField] private float _spawnOffsetY = 0.5f;
    [SerializeField] private float _moveDistance = 0.8f;
    [SerializeField] private float _duration = 0.6f;

    [Header("Message")]
    [SerializeField] private string _warningMessage = "총을 강화하세요!";

    public void Play()
    {
        if (_floaterPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * _spawnOffsetY;
        GameObject floater = Instantiate(_floaterPrefab, spawnPos, Quaternion.identity);

        // 텍스트 설정
        if (floater.TryGetComponent(out TextMeshPro tmp))
        {
            tmp.text = _warningMessage;
        }
        else if (floater.TryGetComponent(out TextMeshProUGUI tmpUI))
        {
            tmpUI.text = _warningMessage;
        }

        // 일정하게 위로 이동
        Vector3 targetPos = spawnPos + Vector3.up * _moveDistance;

        floater.transform.DOMove(targetPos, _duration)
            .SetEase(Ease.OutCubic)
            .SetLink(floater);

        floater.transform.DOScale(0.5f, _duration)
            .SetEase(Ease.InCubic)
            .SetLink(floater);

        // 페이드 아웃
        if (floater.TryGetComponent(out TextMeshPro tmpFade))
        {
            tmpFade.DOFade(0f, _duration)
                .SetEase(Ease.InCubic)
                .SetLink(floater);
        }

        Destroy(floater, _duration + 0.1f);
    }
}