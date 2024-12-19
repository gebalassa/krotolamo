using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoystickSupport : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] GameObject joystickPlayerPrefab;
    [SerializeField] [Range(0, 8)] int maxPlayers = 8;
    [SerializeField] [Range(0, .99f)] float threshold = .99f;
    [SerializeField] bool debug = false;

    // Listado de jugadores
    private List<PlayerInput> players = new List<PlayerInput>();

    /// <summary>
    /// Eventos para botones del Joystick
    /// </summary>
    public delegate bool OnJoystick(JoystickAction action, int playerIndex);
    public static event OnJoystick onJoystick;

    /// <summary>
    /// Eventos para indicar cambio en el estado de soporte
    /// </summary>
    public delegate void OnSupportStatusChange(bool value);
    public static event OnSupportStatusChange onSupportStatusChange;

    /// <summary>
    /// Eventos para indicar entrada de jugador
    /// </summary>
    public delegate void OnPlayerJoinEvent(int playerIndex);
    public static event OnPlayerJoinEvent onPlayerJoinDelegate;

    /// <summary>
    /// Eventos para indicar salida de jugador
    /// </summary>
    public delegate void OnPlayerLeftEvent(int playerIndex);
    public static event OnPlayerLeftEvent onPlayerLeftDelegate;

    // Mantener singleton
    public static JoystickSupport Instance;

    // Esquemas de controles
    public enum PlayerControlScheme
    {
        Keyboard,
        Joystick
    }

    protected void Awake()
    {
        int length = FindObjectsOfType<JoystickSupport>().Length;
        if (length == 1)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Nuevo jugador conectado
    /// </summary>
    /// <param name="input"></param>
    public void OnPlayerJoined(PlayerInput input)
    {
        players.Add(input);
        if (players.Count == 1 && onSupportStatusChange != null) onSupportStatusChange(true);
        int playerIndex = players.FindIndex(p => p == input);
        if (onPlayerJoinDelegate != null) onPlayerJoinDelegate(playerIndex);
    }

    /// <summary>
    /// Jugador desconectado
    /// </summary>
    /// <param name="input"></param>
    public void OnPlayerLeft(PlayerInput input)
    {
        int playerIndex = players.FindIndex(p => p == input); 
        players.Remove(input);
        if (players.Count == 0 && onSupportStatusChange != null) onSupportStatusChange(false);
        if (onPlayerLeftDelegate != null) onPlayerLeftDelegate(playerIndex);
    }

    /// <summary>
    /// Indica si el soporte esta activo en base a los jugadores conectados
    /// </summary>
    /// <returns></returns>
    public bool SupportActivated()
    {
        return players.Count != 0;
    }

    /// <summary>
    /// Indica si el primer player esta usando teclado o joystick
    /// </summary>
    /// <returns></returns>
    public bool FirstPlayerSchemeIsKeyboard()
    {
        if(players.Count != 0)
        {
            return players[0].currentControlScheme == PlayerControlScheme.Keyboard.ToString();
        }
        return false;
    }

    /// <summary>
    /// Accion realizada en un de los joysticks. Llamado a metodos suscritos
    /// </summary>
    /// <param name="action"></param>
    /// <param name="player"></param>
    public void JoystickActionTrigger(JoystickAction action, PlayerInput player) {
        int playerIndex = players.FindIndex(p => p == player);
        if (playerIndex == -1) return;
        if (onJoystick != null) onJoystick(action, playerIndex);
        if (debug) Debug.Log(playerIndex + ": " + action.ToString());
    }

    /// <summary>
    /// Obtener la cantidad de jugadores conectados
    /// </summary>
    /// <returns></returns>
    public int GetPlayerCount()
    {
        return players.Count;
    }

    /// <summary>
    /// Indicar que joysticks deben vibrar
    /// </summary>
    /// <param name="list"></param>
    public void SetGamepadToVibrate(List<int> list)
    {
        List<Gamepad> gamepads = new List<Gamepad>();
        foreach(int playerIndex in list)
        {
            if (playerIndex < players.Count && players[playerIndex].GetDevice<Gamepad>() != null) gamepads.Add(players[playerIndex].GetDevice<Gamepad>());
        }
        VibrationController.Instance.SetGamepadToVibrate(gamepads);
    }

    /// <summary>
    /// Obtencion del primer joystick conectado
    /// </summary>
    /// <returns></returns>
    public Gamepad FirstJoystickLayout()
    {
        foreach (PlayerInput player in players)
        {
            if (player.GetDevice<Gamepad>() != null) return player.GetDevice<Gamepad>();
        }

        return null;
    }
}