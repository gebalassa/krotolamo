using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PJMultipleSelectorOverlay : OverlayObject
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI titleOverlay;
    [SerializeField] List<PJSelectorDisplayer> selectorsCharacters = new List<PJSelectorDisplayer>();
    [SerializeField] TextMeshProUGUI characterName;

    [Header("Others")]
    [SerializeField] bool joinAll = false;

    // Componentes y propiedades recurrentes
    CharacterConfiguration currentCharacterConfiguration;

    // Delegados para cuando se cambie de personaje
    public delegate void OnSelectDelegate(CharacterConfiguration characterConfiguration);
    public static event OnSelectDelegate onSelectDelegate;

    // Use this for initialization
    protected override void Start()
    {
        // Actualizar textos
        Localize();

        // Indicar que personaje deberia estar seleccionado
        for (int i = 0; i < selectorsCharacters.Count; i++)
        {
            selectorsCharacters[i].SetPlayerIndex(i);
        }

        // Suscribirse al evento de cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;

        // Revisar la cantidad de jugadores y asignar automaticamente los espacios correspondientes
        if (JoystickSupport.Instance.SupportActivated())
        {
            if (joinAll)
            {
                for (int i = 0; i < JoystickSupport.Instance.GetPlayerCount(); i++)
                {
                    Join(i);
                }
            }
            else
            {
                Join(0);
            }
            
        }
    }

    /// <summary>
    /// Indicar que se ha unido un jugador
    /// </summary>
    /// <param name="playerIndex"></param>
    public bool Join(int playerIndex = 0)
    {
        if (selectorsCharacters.Count - 1 < playerIndex) return true ;
        if (!selectorsCharacters[playerIndex].Join()) return true;

        // Revisar si existe personaje en el listado de characteres seleccionados
        GameObject character = CharacterPool.Instance.GetCurrentCharacter(playerIndex);
        if (character) selectorsCharacters[playerIndex].ChangeCharacter(character);
        else selectorsCharacters[playerIndex].Surprise();

        return false;
    }

    /// <summary>
    /// Indicar que se ha salido un jugador
    /// </summary>
    /// <param name="playerIndex"></param>
    public void Left(int playerIndex = 0)
    {
        if (selectorsCharacters.Count - 1 < playerIndex) return;
        // Quitar el control del ultimo cuadro con jugador unido
        int lastJoined = 0;
        int currentIndex = 0;
        foreach(PJSelectorDisplayer pJSelectorDisplayer in selectorsCharacters)
        {
            if (!pJSelectorDisplayer.IsJoined()) break;
            lastJoined = currentIndex++;
        }
        selectorsCharacters[lastJoined].Left();
    }

    /// <summary>
    /// Realizar un cambio de personaje
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    public void MoveTo(JoystickAction action, int playerIndex = 0)
    {
        if (selectorsCharacters.Count - 1 < playerIndex) return;

        switch (action)
        {
            case JoystickAction.Left:
                selectorsCharacters[playerIndex].PrevCharacter();
                break;
            case JoystickAction.Right:
                selectorsCharacters[playerIndex].NextCharacter();
                break;
        }
    }

    /// <summary>
    /// Localizacion de los textos del selector
    /// </summary>
    private void Localize()
    {
        LocalizationHelper.Translate(titleOverlay, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.selectYourCharacter);
        UpdateCurrentFont();
    }

    /// <summary>
    /// Actualizacion de la fuente del mensaje y de la localizacion del texto
    /// </summary>
    public void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        Material material = FontManager._mainManager.GetMainMaterial(FontManager.MainFontMaterial.White);
        titleOverlay.font = mainFont;
        titleOverlay.fontSharedMaterial = material;
    }

    /// <summary>
    /// Obtencion del personaje actualmente seleccionado
    /// </summary>
    /// <returns></returns>
    public CharacterConfiguration GetCurrentCharacter()
    {
        return currentCharacterConfiguration;
    }

    /// <summary>
    /// Obtener cuantos jugadores estan unidos a la partida
    /// </summary>
    /// <returns></returns>
    public int CountJoined()
    {
        return selectorsCharacters.FindAll(sc => sc.IsJoined()).Count;
    }

    /// <summary>
    /// Obtener un listado con los indices de los jugadores unidos a la partida
    /// </summary>
    /// <returns></returns>
    public List<int> GetJoined()
    {
        List<int> selectedIndex = selectorsCharacters.FindAll(sc => sc.IsJoined()).ConvertAll<int>(sc => sc.GetPlayerIndex());
        return selectedIndex;
    }

    /// <summary>
    /// Obtencion de selector en base al indice seleccionado
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public PJSelectorDisplayer GetSelectorsCharacters(int index = 0)
    {
        if (selectorsCharacters.Count > index) return selectorsCharacters[index];
        return null;
    }

    /// <summary>
    /// Seleccionar el PJ que corresponda
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    /// <summary>
    /// Desucripcion de eventos
    /// </summary>
    private void OnDestroy()
    {
        // Desuscribirse a los cambios en el selector de personajes
        LanguageController.onLanguageChangeDelegate -= Localize;
    }
}