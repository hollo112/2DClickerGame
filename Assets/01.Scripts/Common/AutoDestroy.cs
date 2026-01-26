using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float _lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }
}