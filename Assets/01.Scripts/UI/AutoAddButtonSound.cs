using UnityEngine;
using UnityEngine.UI;

public class AutoAddButtonSound : MonoBehaviour
{
    private void Awake()
    {
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            if (!btn.TryGetComponent<ButtonSound>(out _))
                btn.gameObject.AddComponent<ButtonSound>();
        }
    }
}
