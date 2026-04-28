using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PickUpItem : MonoBehaviour
{
    private GameObject _objectToPickUp;
    void OnMouseOver()
    {
        _objectToPickUp = gameObject;
        if (Mouse.current.leftButton.isPressed)
        {
            //Может быть любая механика вместо исчезновения
            _objectToPickUp.SetActive(false) ;
        }
    }
}
