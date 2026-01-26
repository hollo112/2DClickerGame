using UnityEngine;

public class SoundFeedback : MonoBehaviour, IFeedback
{
    [SerializeField] private AudioSource _audio;
    
    public void Play(ClickInfo clickInfo)
    {
        if (_audio == null || !_audio.isActiveAndEnabled) return;

        _audio.pitch = Random.Range(0.95f, 1.05f);
        _audio.Play();
    }
}
