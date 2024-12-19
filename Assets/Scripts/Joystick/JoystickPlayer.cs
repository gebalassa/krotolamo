using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Users;
using static UnityEngine.InputSystem.InputAction;

public class JoystickPlayer : MonoBehaviour {

    [Header("Setup")]
    [SerializeField] [Range(0, .99f)] float threshold = .99f;
    [SerializeField] [Range(0, 60)] int waitUntilDestroy = 10;

    PlayerInput playerInput;

    // Corutines
    Coroutine destroyCoroutine;

    /// <summary>
    /// No destruir entre cambios de pantalla
    /// </summary>
    protected void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    /// <summary>
    /// Desconeccion del control del jugador
    /// </summary>
    /// <param name="player"></param>
    public void OnDeviceLost(PlayerInput player)
    {
        destroyCoroutine = StartCoroutine(DestroyPlayer());
    }

    /// <summary>
    /// Jugador reconectado
    /// </summary>
    /// <param name="player"></param>
    public void OnDeviceOnDeviceRegained(PlayerInput player)
    {
        if (destroyCoroutine != null) StopCoroutine(destroyCoroutine);
    }

    /// <summary>
    /// Destruccion del joystick asociado al jugador
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyPlayer()
    {
        yield return new WaitForSeconds(waitUntilDestroy);
        while (GameController.GetGameplayActive()) yield return null;
        Destroy(gameObject);
    }

    /// <summary>
    /// Revision de movimiento del jugador
    /// </summary>
    /// <param name="value"></param>
    private void OnMove(InputValue value)
    {
        Vector2 movement = value.Get<Vector2>();
        if (movement.x >= threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.Right, playerInput);
        else if (movement.x <= -threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.Left, playerInput);

        if (movement.y >= threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.Up, playerInput);
        else if (movement.y <= -threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.Down, playerInput);
    }

    /// <summary>
    /// Revision de movimiento del jugador con el dPad. Similar a OnMove, cambia el valor enviado de accion
    /// </summary>
    /// <param name="value"></param>
    private void OnMoveDPad(InputValue value)
    {
        Vector2 movement = value.Get<Vector2>();
        if (movement.x >= threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.DPadRight, playerInput);
        else if (movement.x <= -threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.DPadLeft, playerInput);

        if (movement.y >= threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.DPadUp, playerInput);
        else if (movement.y <= -threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.DPadDown, playerInput);
    }

    /// <summary>
    /// Revision de movimiento del jugador con el leftStick. Similar a OnMove, cambia el valor enviado de accion
    /// </summary>
    /// <param name="value"></param>
    private void OnMoveLeftStick(InputValue value)
    {
        Vector2 movement = value.Get<Vector2>();
        if (movement.x >= threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.LeftStickRight, playerInput);
        else if (movement.x <= -threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.LeftStickLeft, playerInput);

        if (movement.y >= threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.LeftStickUp, playerInput);
        else if (movement.y <= -threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.LeftStickDown, playerInput);
    }

    /// <summary>
    /// Revision de movimiento del jugador con el rightStick. Similar a OnMove, cambia el valor enviado de accion
    /// </summary>
    /// <param name="value"></param>
    private void OnMoveRightStick(InputValue value)
    {
        Vector2 movement = value.Get<Vector2>();
        if (movement.x >= threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.RightStickRight, playerInput);
        else if (movement.x <= -threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.RightStickLeft, playerInput);

        if (movement.y >= threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.RightStickUp, playerInput);
        else if (movement.y <= -threshold) JoystickSupport.Instance.JoystickActionTrigger(JoystickAction.RightStickDown, playerInput);
    }

    /// <summary>
    /// Indica que uno de los botones del jugador fue presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnButtonPressed(JoystickAction action)
    {
        JoystickSupport.Instance.JoystickActionTrigger(action, playerInput);
    }

    /// <summary>
    /// Boton X presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnX(InputValue value) { OnButtonPressed(JoystickAction.X); }

    /// <summary>
    /// Boton X presionado por periodo extendido
    /// </summary>
    /// <param name="action"></param>
    private void OnXHold(InputValue value) { OnButtonPressed(JoystickAction.XHold); }

    /// <summary>
    /// Boton Y presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnY(InputValue value) { OnButtonPressed(JoystickAction.Y); }

    /// <summary>
    /// Boton Y presionado por periodo extendido
    /// </summary>
    /// <param name="action"></param>
    private void OnYHold(InputValue value) { OnButtonPressed(JoystickAction.YHold); }

    /// <summary>
    /// Boton A presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnA(InputValue value) { OnButtonPressed(JoystickAction.A); }

    /// <summary>
    /// Boton A presionado por periodo extendido
    /// </summary>
    /// <param name="action"></param>
    private void OnAHold(InputValue value) { OnButtonPressed(JoystickAction.AHold); }

    /// <summary>
    /// Boton B presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnB(InputValue value) { OnButtonPressed(JoystickAction.B); }

    /// <summary>
    /// Boton B presionado por periodo extendido
    /// </summary>
    /// <param name="action"></param>
    private void OnBHold(InputValue value) { OnButtonPressed(JoystickAction.BHold); }

    /// <summary>
    /// Boton Start(+) presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnStart(InputValue value) { OnButtonPressed(JoystickAction.Start); }

    /// <summary>
    /// Boton Select(-) presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnSelect(InputValue value) { OnButtonPressed(JoystickAction.Select); }

    /// <summary>
    /// Boton Escape presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnEscape(InputValue value) { OnButtonPressed(JoystickAction.Escape); }

    /// <summary>
    /// Boton L presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnL(InputValue value) { OnButtonPressed(JoystickAction.L); }

    /// <summary>
    /// Boton L presionado por un tiempo determinado
    /// </summary>
    /// <param name="action"></param>
    private void OnLHold(InputValue value) { OnButtonPressed(JoystickAction.LHold); }

    /// <summary>
    /// Boton ZL presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnZL(InputValue value) { OnButtonPressed(JoystickAction.ZL); }

    /// <summary>
    /// Boton ZL presionado por un tiempo determinado
    /// </summary>
    /// <param name="action"></param>
    private void OnZLHold(InputValue value) { OnButtonPressed(JoystickAction.ZLHold); }

    /// <summary>
    /// Boton R presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnR(InputValue value) { OnButtonPressed(JoystickAction.R); }

    /// <summary>
    /// Boton R presionado por un tiempo determinado
    /// </summary>
    /// <param name="action"></param>
    private void OnRHold(InputValue value) { OnButtonPressed(JoystickAction.RHold); }

    /// <summary>
    /// Boton ZR presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnZR(InputValue value) { OnButtonPressed(JoystickAction.ZR); }

    /// <summary>
    /// Boton ZR presionado por un tiempo determinado
    /// </summary>
    /// <param name="action"></param>
    private void OnZRHold(InputValue value) { OnButtonPressed(JoystickAction.ZRHold); }

    /// <summary>
    /// Boton J presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnJ(InputValue value) { OnButtonPressed(JoystickAction.J); }

    /// <summary>
    /// Boton K presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnK(InputValue value) { OnButtonPressed(JoystickAction.K); }

    /// <summary>
    /// Boton LKeyboard presionado
    /// </summary>
    /// <param name="action"></param>
    private void OnLKeyboard(InputValue value) { OnButtonPressed(JoystickAction.LKeyboard); }
}