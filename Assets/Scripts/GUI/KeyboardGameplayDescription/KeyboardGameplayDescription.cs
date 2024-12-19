using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class KeyboardGameplayDescription : OverlayObject
{
    [Header("UI")]
    [SerializeField] Image panelImage;
    [SerializeField] KeyboardGameplayLayoutPlayer keyboardGameplayLayoutPlayer;
    [SerializeField] KeyboardGameplayLayoutSpectator keyboardGameplayLayoutSpectator;

    [Header("Prefabs")]
    [SerializeField] GameObject backCanvas;
    GameObject backCanvasInstance;

    [Header("Others")]
    [SerializeField] Color panelColorIfNoBackground = new Color(0, 0, 0, .5f);

    [Header("Layouts")]
    [SerializeField] List<CanvasGroup> layouts;
    [SerializeField] bool allowChangeLayout = false;
    [SerializeField] GameObject arrowCanvas;
    [SerializeField] Button nextLayoutButton;
    [SerializeField] Button prevLayoutButton;

    // Util
    int currentLayout = 0;

    protected override void Start()
    {
        base.Start();

        // Si no existe un BackCanvas, agregar
        if (!FindObjectOfType<BackCanvas>()) backCanvasInstance = Instantiate(backCanvas);

        // En caso de no existir fondo, cambiar color de panel
        if (!FindObjectOfType<MovementBackground>()) panelImage.color = panelColorIfNoBackground;

        arrowCanvas.SetActive(allowChangeLayout);
        if (allowChangeLayout)
        {
            prevLayoutButton.onClick.AddListener(delegate { PrevLayout(); });
            nextLayoutButton.onClick.AddListener(delegate { NextLayout(); });
        }
    }

    /// <summary>
    /// Mostrar el layout anterior
    /// </summary>
    private void PrevLayout()
    {
        currentLayout--;
        if (currentLayout < 0) currentLayout = layouts.Count - 1;
        ShowLayout();
    }

    /// <summary>
    /// Mostrar el siguiente layout
    /// </summary>
    private void NextLayout()
    {
        currentLayout++;
        if (currentLayout >= layouts.Count) currentLayout = 0;
        ShowLayout();
    }

    /// <summary>
    /// Cambio de layout activo
    /// </summary>
    private void ShowLayout()
    {
        MasterSFXPlayer._player.UISFX();
        for (int i = 0; i < layouts.Count; i++)
        {
            layouts[i].alpha = currentLayout == i ? 1 : 0;
        }
    }

    private void OnDestroy()
    {
        if (backCanvasInstance != null) Destroy(backCanvasInstance);
    }

    #region Joystick Support
    /// <summary>
    /// Soporte para controles y acciones de jugadores
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    public override bool OnJoystick(JoystickAction action, int playerIndex)
    {
        keyboardGameplayLayoutPlayer.OnJoystick(action, playerIndex);
        keyboardGameplayLayoutSpectator.OnJoystick(action, playerIndex);

        switch (action)
        {
            case JoystickAction.L:
            case JoystickAction.Escape:
                if (backCanvasInstance) backCanvasInstance.GetComponent<BackCanvas>().Back();
                break;
            case JoystickAction.Left:
                if (allowChangeLayout) GameController.SimulateClick(prevLayoutButton);
                break;
            case JoystickAction.Right:
                if (allowChangeLayout) GameController.SimulateClick(nextLayoutButton);
                break;
        }

        return false;
    }
    #endregion
}
