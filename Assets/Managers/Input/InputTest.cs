using System;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class InputTest : MonoBehaviour
{
    private void OnEnable()
    {
        InputManager.Instance.OnMovement += Display2D;
        InputManager.Instance.OnJump += DisplayBool;
        InputManager.Instance.OnDive += DisplayBool;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnMovement -= Display2D;
        InputManager.Instance.OnJump -= DisplayBool;
        InputManager.Instance.OnDive -= DisplayBool;
    }
    
    private void DisplayBool(bool obj)
    {
        Debug.Log(obj);
    }

    private void Display2D(Vector2 obj)
    {
        Debug.Log(obj);
    }
}