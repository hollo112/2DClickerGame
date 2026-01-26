using UnityEngine;

public class ScaleTweeningFeedback : MonoBehaviour
{
    [SerializeField] private ClickTarget _owner;

    public void Play()
    {
        // _owner.transform.localScale.DoScale(1.2f, 1f).OnComplete(() =>
        // {
        //     _owner.transform.localScale = Vector3.one;
        // });
    }
}
