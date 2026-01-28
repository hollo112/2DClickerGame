using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private bool useCenter = true;
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    private void Start()
    {
        if (cursorTexture != null)
        {
            Vector2 spot = useCenter
                ? new Vector2(cursorTexture.width / 2f, cursorTexture.height / 2f)
                : hotspot;
            Cursor.SetCursor(cursorTexture, spot, CursorMode.Auto);
        }
    }
}