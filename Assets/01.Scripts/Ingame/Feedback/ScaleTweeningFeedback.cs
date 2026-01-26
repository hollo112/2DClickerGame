using UnityEngine;
using DG.Tweening;

public class ScaleTweeningFeedback : MonoBehaviour, IFeedback
{
    [SerializeField] private float _punchScale = 0.3f;
    [SerializeField] private float _duration = 0.3f;
    [SerializeField] private int _vibrato = 6;
    [SerializeField] private float _elasticity = 0.5f;

    private Vector3 _originalScale;
    private IdleAnimation _idleAnimation;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _idleAnimation = GetComponent<IdleAnimation>();
    }

    public void SetOriginalScale(Vector3 scale)
    {
        _originalScale = scale;
    }

    public void Play(ClickInfo clickInfo)
    {
        _idleAnimation?.Pause();

        transform.DOKill();
        transform.localScale = _originalScale;

        Vector3 punch = new Vector3(
            _punchScale * Mathf.Sign(_originalScale.x),
            _punchScale,
            _punchScale
        );

        transform.DOPunchScale(punch, _duration, _vibrato, _elasticity)
            .OnComplete(() => _idleAnimation?.Resume());
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
