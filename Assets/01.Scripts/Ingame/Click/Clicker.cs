using UnityEngine;

public class Clicker : MonoBehaviour
{
    public LayerMask ClickLayer;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Click(mousePos);
        }
    }

    private void Click(Vector2 mousePos)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, ClickLayer);
        if (hit == true)
        {
            Debug.Log("Click");
        }
    }
}
