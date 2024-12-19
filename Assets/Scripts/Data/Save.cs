using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JankenUp;

[System.Serializable]
public class Save
{
    // Datos de jugador
    public int coins = 0;
    public string name = "";
    public int difficultyLevel = 0;
    public int survivalLevel = 1;
    public int levelRecord = 0;
    public int comboRecord = 0;
    public int scoreRecord = 0;
    public float timeRecord = 0;
    public int playSessions = 0;
    public bool reviewPromptDisplayed = false;
    public bool publicModeRestructuredMessage = false;
    public bool competitiveModeRestructuredMessage = false;
    public bool firstLoad = true;

    // Datos del personaje seleccionado por última vez
    public int characterIndex = 0;
    public string characterIdentifier = "boylee";

    // Personajes desbloqueados
    public List<string> unlockedCharacters = new List<string>() { Characters.BOYLEE, Characters.GIRLSAN };

    // Overlay de deluxe
    public bool deluxeOverlaySinglePlayer = false;
    public bool deluxeOverlayShop = false;
    public bool deluxeOverlayShopCharacter = false;

    // Cantidad extra de superpowers comprados por el usuario
    public int timeMaster = 0;
    public int magicWand = 0;
    public int superJanKenUp = 0;

    // Ultimo modo de juego seleccionado
    public GameMode lastGameMode = GameMode.Classic;

}