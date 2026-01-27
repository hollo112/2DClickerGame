using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance;

    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _clickClip;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayClick()
    {
        if (_clickClip == null) return;
        _audio.PlayOneShot(_clickClip);
    }
}