using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JoystickLayout : MonoBehaviour {

    [Header("Buttons")]
    [SerializeField] Button xButton;
    [SerializeField] Button aButton;
    [SerializeField] Button bButton;
    [SerializeField] Button yButton;
    [SerializeField] Button startButton;
    [SerializeField] Button selectButton;
    [SerializeField] Button homeButton;
    [SerializeField] Button screenshotButton;
    [SerializeField] Button upButton;
    [SerializeField] Button downButton;
    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;
    [SerializeField] Button leftStickButton;
    [SerializeField] Button rightStickButton;

    [Header("Images")]
    [SerializeField] Image xSpriteRenderer;
    [SerializeField] Image aSpriteRenderer;
    [SerializeField] Image bSpriteRenderer;
    [SerializeField] Image ySpriteRenderer;

    [Header("Specific Control Adjustments")]
    [SerializeField] Sprite dualshockX;
    [SerializeField] Sprite dualshockA;
    [SerializeField] Sprite dualshockB;
    [SerializeField] Sprite dualshockY;
    [SerializeField] Sprite xboxX;
    [SerializeField] Sprite xboxA;
    [SerializeField] Sprite xboxB;
    [SerializeField] Sprite xboxY;

    [Header("Others")]
    [SerializeField] [Range(0, 100)] float moveOffset = 10;
    [SerializeField] [Range(0, 1)] float timeMoveStick = .05f;

    Coroutine moveLeftStickCoroutine;
    Coroutine moveRightStickCoroutine;
    Vector3 leftStickOriginalPosition;
    Vector3 rightStickOriginalPosition;

    private void Start()
    {
        leftStickOriginalPosition = leftStickButton.transform.localPosition;
        rightStickOriginalPosition = rightStickButton.transform.localPosition;
        CheckLayout();
    }

    /// <summary>
    /// Cambio en los sprites de los botones dependiendo del Layout del primer joystick conectado
    /// </summary>
    private void CheckLayout()
    {
        Gamepad gamepad = JoystickSupport.Instance.FirstJoystickLayout();
        if (gamepad is UnityEngine.InputSystem.XInput.XInputController)
        {
            xSpriteRenderer.sprite = xboxX;
            aSpriteRenderer.sprite = xboxA;
            bSpriteRenderer.sprite = xboxB;
            ySpriteRenderer.sprite = xboxY;
        }
        else if (gamepad is UnityEngine.InputSystem.DualShock.DualShockGamepad)
        {
            xSpriteRenderer.sprite = dualshockX;
            aSpriteRenderer.sprite = dualshockA;
            bSpriteRenderer.sprite = dualshockB;
            ySpriteRenderer.sprite = dualshockY;
        }
    }

    /// <summary>
    /// Realiza el llamado para movimiento de boton
    /// </summary>
    /// <param name="button"></param>
    /// <param name="negative"></param>
    private void MoveStick(Button button, bool xMovement = false, bool yMovement = false, bool negative = false) {
        if (moveLeftStickCoroutine != null && button == leftStickButton) StopCoroutine(moveLeftStickCoroutine);
        if (moveRightStickCoroutine != null && button == rightStickButton) StopCoroutine(moveRightStickCoroutine);

        if(button == leftStickButton) moveLeftStickCoroutine = StartCoroutine(MoveStickCoroutine(button, leftStickOriginalPosition, xMovement, yMovement, negative));
        else moveRightStickCoroutine = StartCoroutine(MoveStickCoroutine(button, rightStickOriginalPosition, xMovement, yMovement, negative));
    }

    /// <summary>
    /// Realiza el movimiento de un boton
    /// </summary>
    /// <param name="button"></param>
    /// <param name="negative"></param>
    /// <returns></returns>
    private IEnumerator MoveStickCoroutine(Button button, Vector3 originalPosition, bool xMovement = false, bool yMovement = false, bool negative = false)
    {
        float newX = xMovement ? originalPosition.x + (negative ? (moveOffset * -1) : moveOffset) : originalPosition.x;
        float newY = yMovement ? originalPosition.y + (negative ? (moveOffset * -1) : moveOffset) : originalPosition.y;
        button.transform.localPosition = new Vector3(newX, newY, originalPosition.z);
        yield return new WaitForSeconds(timeMoveStick);
        button.transform.localPosition = originalPosition;
    }

    /// <summary>
    /// Detener corutinas activas
    /// </summary>
    private void OnDestroy()
    {
        if (moveLeftStickCoroutine != null) StopCoroutine(moveLeftStickCoroutine);
        if (moveRightStickCoroutine != null) StopCoroutine(moveRightStickCoroutine);
    }

    #region Joystick Support
    /// <summary>
    /// Soporte para controles y acciones de jugadores
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    public bool OnJoystick(JoystickAction action, int playerIndex)
    {
        bool canContinue = action == JoystickAction.Start;

        switch (action)
        {
            case JoystickAction.L:
            case JoystickAction.Escape:
                BackCanvas backCanvas = FindObjectOfType<BackCanvas>();
                if (backCanvas) backCanvas.Back();
                break;
            case JoystickAction.LeftStickUp:
                MoveStick(leftStickButton, false, true, false);
                break;
            case JoystickAction.LeftStickDown:
                MoveStick(leftStickButton, false, true, true);
                break;
            case JoystickAction.LeftStickLeft:
                MoveStick(leftStickButton, true, false, true);
                break;
            case JoystickAction.LeftStickRight:
                MoveStick(leftStickButton, true, false, false);
                break;
            case JoystickAction.RightStickUp:
                MoveStick(rightStickButton, false, true, false);
                break;
            case JoystickAction.RightStickDown:
                MoveStick(rightStickButton, false, true, true);
                break;
            case JoystickAction.RightStickLeft:
                MoveStick(rightStickButton, true, false, true);
                break;
            case JoystickAction.RightStickRight:
                MoveStick(rightStickButton, true, false, false);
                break;
            case JoystickAction.DPadUp:
                GameController.SimulateClick(upButton);
                break;
            case JoystickAction.DPadDown:
                GameController.SimulateClick(downButton);
                break;
            case JoystickAction.DPadLeft:
                GameController.SimulateClick(leftButton);
                break;
            case JoystickAction.DPadRight:
                GameController.SimulateClick(rightButton);
                break;
            case JoystickAction.X:
                GameController.SimulateClick(xButton);
                break;
            case JoystickAction.Y:
                GameController.SimulateClick(yButton);
                break;
            case JoystickAction.A:
                GameController.SimulateClick(aButton);
                break;
            case JoystickAction.B:
                GameController.SimulateClick(bButton);
                break;
        }

        return canContinue;
    }
    #endregion
}