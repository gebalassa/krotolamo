using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JoystickUIElement : MonoBehaviour
{
    // Componentes
    Button button;
    JoystickButtonIcon joystickButtonIcon;

    private void Start()
    {
        CheckComponents();
    }

    /// <summary>
    /// Obtencion de los componentes
    /// </summary>
    private void CheckComponents()
    {
        if (joystickButtonIcon) return;
        button = GetComponent<Button>();
        joystickButtonIcon = GetComponentInChildren<JoystickButtonIcon>();
        if (joystickButtonIcon)
        {
            if (button) joystickButtonIcon.SetParent(button);
        }
    }

    /// <summary>
    /// Cambiar la posibilidad de interactuar con el elemento
    /// </summary>
    /// <param name="value"></param>
    public void SetInteractable(bool value = true)
    {
        CheckComponents();
        if (button) button.interactable = value;
        if (joystickButtonIcon) joystickButtonIcon.SetInteractable(value);
    }

    /// <summary>
    /// Obtencion del boton asociado
    /// </summary>
    /// <returns></returns>
    public Button GetButton()
    {
        return button;
    }

}