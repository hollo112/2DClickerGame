using UnityEngine;

public class SoundFeedback : MonoBehaviour, IFeedback
{
    [SerializeField] private AudioSource _audio;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip _playerClip;
    [SerializeField] private AudioClip _monsterClip;

    public void Play(ClickInfo clickInfo)
    {
        if (_audio == null || !_audio.isActiveAndEnabled) return;

        ChooseAudioClip(clickInfo);

        _audio.pitch = Random.Range(0.95f, 1.05f);
        _audio.Play();
    }

    private void ChooseAudioClip(ClickInfo clickInfo)
    {
        switch (clickInfo.Type)
        {
            case ClickerType.Player:
                _audio.clip = _playerClip;
                break;

            case ClickerType.Monster:
                _audio.clip = _monsterClip;
                break;

            default:
                return;
        }
    }
}