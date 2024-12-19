using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardLayout : MonoBehaviour {

    [Header("Buttons")]
    [SerializeField] Button aButton;
    [SerializeField] Button sButton;
    [SerializeField] Button dButton;
    [SerializeField] Button qButton;
    [SerializeField] Button wButton;
    [SerializeField] Button eButton;
    [SerializeField] Button rButton;
    [SerializeField] Button jButton;
    [SerializeField] Button kButton;
    [SerializeField] Button lButton;
    [SerializeField] Button upButton;
    [SerializeField] Button downButton;
    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;
    [SerializeField] Button humichanButton;
    [SerializeField] Button zweegButton;
    [SerializeField] Button instagramButton;
    [SerializeField] Button tiktokButton;
    [SerializeField] Button xButton;
    [SerializeField] Button chileButton;
    [SerializeField] Button youtubeButton;

    [Header("Others")]
    [SerializeField] [Range(0, 100)] float moveOffset = 10;
    [SerializeField] [Range(0, 1)] float timeMoveStick = .05f;

    private void Start(){
        humichanButton.onClick.AddListener(delegate { OpenLink("https://www.instagram.com/humichanbosu/"); });
        zweegButton.onClick.AddListener(delegate { OpenLink("https://www.instagram.com/zweeg.jpg/"); });
        instagramButton.onClick.AddListener(delegate { OpenLink("https://www.instagram.com/humitagames/"); });
        tiktokButton.onClick.AddListener(delegate { OpenLink("https://www.tiktok.com/@humitagames"); });
        xButton.onClick.AddListener(delegate { OpenLink("https://x.com/humitagames"); });
        chileButton.onClick.AddListener(delegate { OpenLink("https://www.youtube.com/watch?v=0gpDCaWBEHw"); });
        youtubeButton.onClick.AddListener(delegate { OpenLink("https://www.youtube.com/@humitagames"); });
    }

    /// <summary>
    /// Apertura de URL
    /// </summary>
    /// <param name="url"></param>
    private void OpenLink(string url) {
        Application.OpenURL(url);
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
            case JoystickAction.Up:
                GameController.SimulateClick(upButton);
                break;
            case JoystickAction.Down:
                GameController.SimulateClick(downButton);
                break;
            case JoystickAction.Left:
                GameController.SimulateClick(leftButton);
                break;
            case JoystickAction.Right:
                GameController.SimulateClick(rightButton);
                break;
            case JoystickAction.X:
                GameController.SimulateClick(aButton);
                break;
            case JoystickAction.Y:
                GameController.SimulateClick(wButton);
                break;
            case JoystickAction.A:
                GameController.SimulateClick(sButton);
                break;
            case JoystickAction.B:
                GameController.SimulateClick(dButton);
                break;
            case JoystickAction.J:
                GameController.SimulateClick(jButton);
                break;
            case JoystickAction.K:
                GameController.SimulateClick(kButton);
                break;
            case JoystickAction.LKeyboard:
                GameController.SimulateClick(lButton);
                break;
            case JoystickAction.R:
                GameController.SimulateClick(eButton);
                break;
            case JoystickAction.ZR:
                GameController.SimulateClick(rButton);
                break;
        }

        return canContinue;
    }
    #endregion
}