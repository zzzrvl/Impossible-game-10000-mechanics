using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpItem : MonoBehaviour
{
    private TrackingCursor _trackingCursor;

    void Start()
    {
        _trackingCursor = FindFirstObjectByType<TrackingCursor>();
    }

    void OnMouseOver()
    {
        if (_trackingCursor == null) return;
        Vector3 playerPos = _trackingCursor.transform.position;
        Vector3 itemPos = transform.position;

        playerPos.y = 0;
        itemPos.y = 0;

        float distance = Vector3.Distance(playerPos, itemPos);

        if (distance <= _trackingCursor.maxRadius)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                PickUp();
            }
        }
    }

    private void PickUp()
    {
        Debug.Log("Предмет подобран!");
        gameObject.SetActive(false);
    }
}