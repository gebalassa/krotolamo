using UnityEngine;

public class PreAttack : MonoBehaviour
{
    [SerializeField] CharacterInGameController characterController;

    // Componentes
    JoystickButtonIcon joystickButtonIcon;

    /// <summary>
    /// Obtencion de componentes
    /// </summary>
    private void CheckComponents()
    {
        if (joystickButtonIcon) return;
        // Nota: El unico hijo del elemento deberia ser el icono
        if(transform.childCount > 0) joystickButtonIcon = transform.GetChild(0).GetComponent<JoystickButtonIcon>();
    }

    /// <summary>
    /// Indica si se debe mostrar o no el icono de juego
    /// </summary>
    /// <param name="show"></param>
    public void ToggleJoystickIcon(bool show = false)
    {
        CheckComponents();
        if (joystickButtonIcon) joystickButtonIcon.Toggle(show);
    }

    // Al presionar al personaje, procesar si puede o no quitar el illuminati
    private void OnMouseDown()
    {
        characterController.OnMouseDown();
    }
}
