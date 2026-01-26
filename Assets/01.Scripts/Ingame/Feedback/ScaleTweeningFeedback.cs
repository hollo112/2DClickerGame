using UnityEngine;
using DG.Tweening;

public class ScaleTweeningFeedback : MonoBehaviour, IFeedback
{
    [SerializeField] private float _punchScale = 0.3f;
    [SerializeField] private float _duration = 0.3f;
    [SerializeField] private int _vibrato = 6;
    [SerializeField] private float _elasticity = 0.5f;

    public void Play(ClickInfo clickInfo)
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * _punchScale, _duration, _vibrato, _elasticity);
    }
}
