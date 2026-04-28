using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class pickUpItem : MonoBehaviour
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
