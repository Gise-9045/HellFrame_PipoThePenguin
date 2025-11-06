using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum ControlScheme
{
    Keyboard,
    Gamepad,
    Unknown
}

[DefaultExecutionOrder(-10)]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    // Required components
    private InputActions inputActions;
    private ControlScheme lastScheme = ControlScheme.Keyboard;
    
    // Events
    public event Action<Vector2> OnMovement;
    public event Action<bool> OnJump;
    public event Action<bool> OnDive;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (inputActions != null)
            {
                DisableInputs();
            }
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Start Input Actions
        inputActions = new InputActions();
    }

    /*
     * Each input has a method attached to an action.
     */
    private void OnEnable()
    {
        EnableInputs();
    }
    private void EnableInputs()
    {
        inputActions.Enable();
        inputActions.Pipo.Movement.performed += OnMovementHandler;
        inputActions.Pipo.Movement.canceled += OnMovementHandler;
        inputActions.Pipo.Jump.performed += OnJumpHandler;
        inputActions.Pipo.Jump.canceled += OnJumpHandler;
        inputActions.Pipo.Dive.performed += OnDiveHandler;
        inputActions.Pipo.Dive.canceled += OnDiveHandler;
    }
    private void OnDisable()
    {
        DisableInputs();
    }
    private void DisableInputs()
    {
        inputActions.Pipo.Movement.performed -= OnMovementHandler;
        inputActions.Pipo.Movement.canceled -= OnMovementHandler;
        inputActions.Pipo.Jump.performed -= OnJumpHandler;
        inputActions.Pipo.Jump.canceled -= OnJumpHandler;
        inputActions.Pipo.Dive.performed -= OnDiveHandler;
        inputActions.Pipo.Dive.canceled -= OnDiveHandler;
        inputActions.Disable();
    }
   
    #region Input Handlers
    private void OnMovementHandler(InputAction.CallbackContext obj)
    {
        if (obj.performed)
        {
            OnMovement?.Invoke(obj.ReadValue<Vector2>());
        }
        else
        {
            OnMovement?.Invoke(Vector2.zero);
        }
    }
    
    private void OnJumpHandler(InputAction.CallbackContext obj)
    {
        OnJump?.Invoke(obj.performed || obj.started);
    }

    private void OnDiveHandler(InputAction.CallbackContext obj)
    {
        OnDive?.Invoke(obj.performed || obj.started);
    }
    #endregion
    
    #region Control Scheme Methods
    public ControlScheme GetCurrentControlScheme()
    {
        if (IsKeyboardOrMouseActive())
        {
            lastScheme = ControlScheme.Keyboard;
        }
        else if (Gamepad.current != null && IsAnyGamepadButtonPressed(Gamepad.current)) 
        {
            lastScheme = ControlScheme.Gamepad;
        }
        return lastScheme;
    }
    private bool IsKeyboardOrMouseActive()
    {
        // Comprobamos si cualquier tecla del teclado está presionada
        if (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
        {
            return true;
        }

        // Comprobamos si se ha realizado alguna acción con el ratón (clic o movimiento de rueda)
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed ||
                Mouse.current.rightButton.isPressed ||
                Mouse.current.middleButton.isPressed ||
                Mouse.current.scroll.ReadValue().y != 0)
            {
                return true;
            }
        }

        return false;
    }
    private bool IsAnyGamepadButtonPressed(Gamepad gamepad)
    {
        foreach (var control in gamepad.allControls)
        {
            if (control is ButtonControl button && button.isPressed)
            {
                return true;
            }
        }
        return false;
    }
    public void VibrateController(float duration = 0.1f, float lowFrequency = 0.5f, float highFrequency = 0.5f)
    {
        VibrateGamepad(duration, 1, 1);
    }
    private void VibrateGamepad(float duration = 0.01f, float lowFrequency = 0.5f, float highFrequency = 0.5f)
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
            StartCoroutine(StopVibrationAfterDelay(duration));
        }
    }
    private System.Collections.IEnumerator StopVibrationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0, 0);
        }
    }
    #endregion
}
