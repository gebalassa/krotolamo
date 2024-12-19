using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JoystickButtonIcon : MonoBehaviour {

    [Header("Setup")]
    [SerializeField] Text label;
    [SerializeField] SpriteRenderer spriteLabel;
    [SerializeField] Sprite joystickSpriteLabel;
    [SerializeField] Sprite keyboardSpriteLabel;
    [SerializeField] JoystickAction joystickAction;
    [SerializeField] JoystickAction keyboardAction;
    [SerializeField] bool reactToJoystickSupport = true;
    [SerializeField] bool showJoystickSupport = true;
    [SerializeField] bool showCustomLabel = false;
    [SerializeField] bool forceJoystickLabel = false;
    [SerializeField] bool forceKeyboardLabel = false;
    [SerializeField] bool userSpriteLabel = false;

    [Header("Specific Control Adjustments")]
    [SerializeField] Sprite dualshockSpriteLabel;
    [SerializeField] Sprite xboxSpriteLabel;

    // Componentes
    Button parentButton;

    private void Start()
    {
        // Asignar letra
        if(label && !showCustomLabel)
        {
            // Nota: La letra del joystick dependera del control utilizado. Se manejara el caso para Xbox y Playstation, por defecto esta la configuracion de Nintendo Switch
            string joystickLabel = joystickAction.ToString().ToUpper();
            bool setBold = false;
            Gamepad gamepad = JoystickSupport.Instance.FirstJoystickLayout();
            if(gamepad is UnityEngine.InputSystem.XInput.XInputController)
            {
                switch (joystickLabel)
                {
                    case "X":
                        joystickLabel = "Y";
                        break;
                    case "Y":
                        joystickLabel = "X";
                        break;
                    case "A":
                        joystickLabel = "B";
                        break;
                    case "B":
                        joystickLabel = "A";
                        break;
                    case "L":
                        joystickLabel = "LB";
                        break;
                    case "ZL":
                        joystickLabel = "LT";
                        break;
                    case "R":
                        joystickLabel = "RB";
                        break;
                    case "ZR":
                        joystickLabel = "RT";
                        break;
                }
            }
            else if (gamepad is UnityEngine.InputSystem.DualShock.DualShockGamepad)
            {
                switch (joystickLabel)
                {
                    case "X":
                        joystickLabel = "△";
                        setBold = true;
                        break;
                    case "Y":
                        joystickLabel = "□";
                        setBold = true;
                        break;
                    case "A":
                        joystickLabel = "○";
                        setBold = true;
                        break;
                    case "B":
                        joystickLabel = "X";
                        break;
                    case "L":
                        joystickLabel = "L1";
                        break;
                    case "ZL":
                        joystickLabel = "L2";
                        break;
                    case "R":
                        joystickLabel = "R1";
                        break;
                    case "ZR":
                        joystickLabel = "R2";
                        break;
                }
            }

            if (forceKeyboardLabel || (JoystickSupport.Instance.FirstPlayerSchemeIsKeyboard() && !forceJoystickLabel))
            {
                label.text = keyboardAction.ToString().ToUpper();
            }
            else
            {
                label.text = joystickLabel;
                if (setBold) label.fontStyle = FontStyle.Bold;
            }
        }
        else if(spriteLabel && userSpriteLabel)
        {
            Sprite _sprite = joystickSpriteLabel;
            Gamepad gamepad = JoystickSupport.Instance.FirstJoystickLayout();
            if (gamepad is UnityEngine.InputSystem.XInput.XInputController) _sprite = xboxSpriteLabel;
            else if (gamepad is UnityEngine.InputSystem.DualShock.DualShockGamepad) _sprite = dualshockSpriteLabel;

            spriteLabel.sprite = forceKeyboardLabel || (JoystickSupport.Instance.FirstPlayerSchemeIsKeyboard() && !forceJoystickLabel) ? keyboardSpriteLabel : _sprite;
        }

        // Si no hay soporte activo, no mostrar el icono
        Toggle(showJoystickSupport && JoystickSupport.Instance.SupportActivated());

        // Unirse al cambio de soporte de joystick
        if(showJoystickSupport && reactToJoystickSupport) JoystickSupport.onSupportStatusChange += Toggle;
    }

    /// <summary>
    /// Cambia el estado de activo del elemento
    /// </summary>
    /// <param name="show"></param>
    public void Toggle(bool show = true)
    {
        if (parentButton) gameObject.SetActive(showJoystickSupport && parentButton.interactable && show && JoystickSupport.Instance.SupportActivated());
        else gameObject.SetActive(showJoystickSupport && show && JoystickSupport.Instance.SupportActivated());
    }

    /// <summary>
    /// Se asocia un boton padre
    /// </summary>
    /// <param name="button"></param>
    public void SetParent(Button button)
    {
        parentButton = button;
    }

    /// <summary>
    /// Cambiar la posibilidad de interactuar con el elemento (En base a elemento padre)
    /// </summary>
    /// <param name="value"></param>
    public void SetInteractable(bool value = true) {
        if (parentButton) parentButton.interactable = value;
        Toggle(showJoystickSupport && JoystickSupport.Instance.SupportActivated());
    }

    private void OnDestroy()
    {
        // Quitar suscripcion al cambio de soporte de joystick
        if (showJoystickSupport && reactToJoystickSupport) JoystickSupport.onSupportStatusChange -= Toggle;
    }

}