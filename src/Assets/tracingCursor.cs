using UnityEngine;
using UnityEngine.InputSystem;

public class tracingCursor : MonoBehaviour
{
    // Update is called once per frame
    public GameObject obj;
    void Update()
    {
        var mousePos = Mouse.current.position.ReadValue();

        var ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out var hitInfo))
        {
            var target = new Vector3(hitInfo.point.x, obj.transform.position.y, hitInfo.point.z);

            obj.transform.LookAt(target);
        }

        if (Keyboard.current.spaceKey.isPressed)
        {
            var newPos = new Vector2(0, 0);
            Mouse.current.WarpCursorPosition(newPos); 
        }

    }
}
