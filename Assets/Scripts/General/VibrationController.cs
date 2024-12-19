using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VibrationController : MonoBehaviour
{
    [Header("Joysticks")]
    [SerializeField] [Range(0, 1)] float lowFrecuency = .2f;
    [SerializeField] [Range(0, 1)] float highFrecuency = 1f;
    [SerializeField] [Range(0, 1)] float duration = .25f;

    public static VibrationController _instance;
    public static VibrationController Instance { get { return _instance; } }

    // Listado de controles actuales para vibrar
    List<Gamepad> gamepadsToVibrate = new List<Gamepad>();

    // Corutina de vibracion
    Coroutine gamepadVibrationCoroutine;
    /// <summary>
    /// Singleton
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
        }
    }

    /// <summary>
    /// Realiza una vibracion en el dispositivo si es posible
    /// </summary>
    public void Vibrate() {
#if UNITY_ANDROID || UNITY_IOS
        if (PlayersPreferencesController.CanVibrate()) Handheld.Vibrate();
#endif
        if (PlayersPreferencesController.CanVibrate() && JoystickSupport.Instance.SupportActivated())
        {
            if (gamepadVibrationCoroutine != null) StopCoroutine(gamepadVibrationCoroutine);
            gamepadVibrationCoroutine = StartCoroutine(GamepadVibration());
        }
    }

    /// <summary>
    /// Vibracion de todos los pads conectados
    /// </summary>
    /// <returns></returns>
    private IEnumerator GamepadVibration()
    {
        // Detener vibracion de todos controles
        foreach (Gamepad gamepad in Gamepad.all) gamepad.SetMotorSpeeds(0, 0);

        // Solor hacer vibrar los controles permitidos
        if(gamepadsToVibrate.Count > 0) foreach (Gamepad gamepad in gamepadsToVibrate) gamepad.SetMotorSpeeds(lowFrecuency, highFrecuency);
        else foreach (Gamepad gamepad in Gamepad.all) gamepad.SetMotorSpeeds(lowFrecuency, highFrecuency);
        yield return new WaitForSeconds(duration);

        // Detener vibracion de todos controles
        foreach (Gamepad gamepad in Gamepad.all) gamepad.SetMotorSpeeds(0, 0);
    }

    /// <summary>
    /// Actualizar listado de controles permitidos para vibrar
    /// </summary>
    /// <param name="gamepads"></param>
    public void SetGamepadToVibrate(List<Gamepad> gamepads) {
        gamepadsToVibrate.Clear();
        gamepadsToVibrate.AddRange(gamepads);
    }

    /// <summary>
    /// Detener vibracion de cualquier joystick conectado
    /// </summary>
    private void OnDestroy()
    {
        foreach (Gamepad gamepad in Gamepad.all) gamepad.SetMotorSpeeds(0, 0);
    }
}