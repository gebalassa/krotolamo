using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialOptionsMenu : OptionsMenuRow
{
    bool tutorialIsNeeded = false;

    /// <summary>
    /// Cambiar el estado del turorial
    /// </summary>
    new void Start()
    {
        tutorialIsNeeded = !GameController.LoadTutorials().tutorials.Contains(0);
        localizationKey = tutorialIsNeeded ? JankenUp.Localization.tables.Options.Keys.disableTutorial : JankenUp.Localization.tables.Options.Keys.activateTutorial;
        base.Start();
    }

    /// <summary>
    /// Activar o desactivar el tutorial
    /// </summary>
    public void ToogleTutorial()
    {
        tutorialIsNeeded = !tutorialIsNeeded;
        GameController.ToggleTutorial(tutorialIsNeeded);
        Localize();
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected new void Localize()
    {
        localizationKey = tutorialIsNeeded ? JankenUp.Localization.tables.Options.Keys.disableTutorial : JankenUp.Localization.tables.Options.Keys.activateTutorial;
        base.Localize();
    }
}