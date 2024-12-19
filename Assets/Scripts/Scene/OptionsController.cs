using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsController : SceneController
{

    // Reproducir sonido de boton
    public void UIButtonSFX()
    {
        MasterSFXPlayer._player.UISFX();
    }

}
