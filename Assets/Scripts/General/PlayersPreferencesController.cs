using System.Collections;
using UnityEngine;

public class PlayersPreferencesController : MonoBehaviour
{
    // Llave para guardar y obtener la preferencia de control del jugador
    public static string CONTROLPOSITIONKEY = "ControlPosition";
    public static int CONTROLPOSITIONKEYDEFAULT = 0;
    public static string VIBRATE = "Vibrate";
    public static string LANGUAGE = "Language";

    // Revisar si es posible que vibre el celular
    public static bool CanVibrate()
    {
        return PlayerPrefs.GetInt(VIBRATE, 1) == 1;
    }

    // Obtener el idioma selecionado actualmente
    public static string GetCurrentLanguage()
    {
        return PlayerPrefs.GetString(LANGUAGE, "en");
    }

    // Guardar el idioma
    public static void SetCurrentLanguage(string code)
    {
        PlayerPrefs.SetString(LANGUAGE, code);
    }

    // Obtener si el jugador ha configurado su idioma o no
    public static bool PlayerSetCurrentLanguage()
    {
        return PlayerPrefs.GetString(LANGUAGE) != "";
    }

}