using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class pickUpItem : MonoBehaviour
{
    public GameObject objectToPickUp;
    void OnMouseOver()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            //Может быть любая механика вместо исчезновения
            objectToPickUp.SetActive(false) ;
        }
    }
}
