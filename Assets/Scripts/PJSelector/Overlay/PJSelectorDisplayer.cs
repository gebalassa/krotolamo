using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PJSelectorDisplayer : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] int playerIndex = 0;
    [SerializeField] Image background;
    [SerializeField] Image characterIllustration;
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] TextMeshProUGUI characterName;
    [SerializeField] Image shineImage;
    [SerializeField] Image borderImage;
    [SerializeField] CanvasGroup playerCanvasGroup;
    [SerializeField] TextMeshProUGUI pressToJoinLabel;
    [SerializeField] CanvasGroup pressToJoinCanvas;
    [SerializeField] [Range(0, 1)] float pressToJoinCanvasBlinkTime = 1f;
    [SerializeField] [Range(0, 1)] float borderColorTime = .5f;
    [SerializeField] Color unselectedColor = new Color(0, 0, 0);
    [SerializeField] Color selectedColor = new Color(36, 216, 134);

    // Delegados para cuando se una efectivamente un jugador
    public delegate void OnJoined();
    public static event OnJoined onJoinedDelegate;

    // Utiles
    List<GameObject> availablesCharacters = new List<GameObject>();
    CharacterConfiguration characterConfiguration;
    int selectedIndex = 0;
    bool joined = false;
    bool processLogin = false;
    float joinAfter = .2f;

    // Corutinas y Tweens
    Coroutine pressToJoinLabelCoroutine;
    Tween<float> tweenPressToJoin;
    Tween<Color> tweenBorderColor;

    private void Start()
    {
        // Obtener elementos recurrentes
        availablesCharacters = CharacterPool.Instance.GetAvailables();
        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;
        Localize();
    }

    /// <summary>
    /// Indicar el numero de jugador que corresponde
    /// </summary>
    /// <param name="playerIndex"></param>
    public void SetPlayerIndex(int playerIndex = 0)
    {
        this.playerIndex = playerIndex;
    }

    /// <summary>
    /// Obtencon del numero de jugador que corresponde
    /// </summary>
    public int GetPlayerIndex()
    {
        return playerIndex;
    }

    /// <summary>
    /// Activacion de la tarjeta de seleccion
    /// </summary>
    public bool Join()
    {
        if (joined || processLogin) return false;
        processLogin = true;
        if (tweenPressToJoin != null) tweenPressToJoin.Stop(TweenStopBehavior.Complete);
        if (pressToJoinLabelCoroutine != null) StopCoroutine(pressToJoinLabelCoroutine);
        pressToJoinCanvas.alpha = 0;
        playerCanvasGroup.alpha = 1;
        StartCoroutine(ProcessLogin());
        return true;
    }

    /// <summary>
    /// Marcar como unido despues de X segunods
    /// </summary>
    /// <returns></returns>
    private IEnumerator ProcessLogin()
    {
        yield return new WaitForSeconds(joinAfter);
        joined = true;
        if (onJoinedDelegate != null) onJoinedDelegate();
    }

    /// <summary>
    /// Activacion de la tarjeta de seleccion
    /// </summary>
    public void Left()
    {
        joined = false;
        pressToJoinCanvas.alpha = 1;
        playerCanvasGroup.alpha = 0;
        characterName.text = "";
        if (pressToJoinLabelCoroutine != null) StopCoroutine(pressToJoinLabelCoroutine);
        pressToJoinLabelCoroutine = StartCoroutine(PressToJoinLabelCoroutine());
    }

    /// <summary>
    /// Hacer parpadear el label de unirse
    /// </summary>
    /// <returns></returns>
    IEnumerator PressToJoinLabelCoroutine()
    {
        bool odd = true;
        while (true)
        {
            if (tweenPressToJoin != null) tweenPressToJoin.Stop(TweenStopBehavior.Complete);
            System.Action<ITween<float>> fadeFn = (t) =>
            {
                if (pressToJoinCanvas) pressToJoinCanvas.alpha = t.CurrentValue;
            };
            tweenPressToJoin = gameObject.Tween(string.Format("FadeJoin{0}", GetInstanceID()), odd? 0 : 1, odd? 1 : 0, pressToJoinCanvasBlinkTime / 2, TweenScaleFunctions.QuadraticEaseInOut, fadeFn);
            yield return new WaitForSeconds(pressToJoinCanvasBlinkTime);
            odd = !odd;
        }
    }

    /// <summary>
    /// Realizar un cambio de personaje
    /// </summary>
    /// <param name="gameObject"></param>
    public void ChangeCharacter(GameObject gameObject)
    {
        selectedIndex = availablesCharacters.FindIndex(ac => ac == gameObject);
        if(selectedIndex == -1)
        {
            selectedIndex = 0;
            gameObject = availablesCharacters[selectedIndex];
        }
        characterConfiguration = gameObject.GetComponent<CharacterConfiguration>();

        // Nombre de personaje
        characterName.text = characterConfiguration.GetName();

        // Configurar elementos
        characterIllustration.sprite = characterConfiguration.GetCardIllustration();
        characterIllustration.rectTransform.anchoredPosition = characterConfiguration.GetSelectionIllustrationPosition();
        characterIllustration.rectTransform.sizeDelta = characterConfiguration.GetCardIllustrationSize();
        background.color = characterConfiguration.GetCardColor();
        shineImage.color = characterConfiguration.GetCardShinecolor();
        bool shineRotateX = characterConfiguration.GetCardShineRotateX();
        bool shineRotateY = characterConfiguration.GetCardShineRotateY();
        shineImage.transform.localScale = new Vector2(shineRotateX ? -1 : 1, shineRotateY ? -1 : 1);
        shineImage.transform.eulerAngles = characterConfiguration.GetCardShineRotation();

        // Reproducir un sonido del adversario
        MasterSFXPlayer._player.PlayOneShot(characterConfiguration.SFXWin());

        // Localizar nombre jugador
        Localize();

        // Guardar personaje en CharacterPool
        CharacterPool.Instance.SetCurrentCharacterIdentifier(characterConfiguration.GetIdentifier(), playerIndex);
    }

    /// <summary>
    /// Selecciona un personaje al azar
    /// </summary>
    public void Surprise()
    {
        ChangeCharacter(availablesCharacters[Random.Range(0, availablesCharacters.Count)]);
    }

    /// <summary>
    /// Obtener el personaje seleccionado
    /// </summary>
    /// <returns></returns>
    public CharacterConfiguration GetCurrentCharacter()
    {
        return characterConfiguration;
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        // Actualizar la fuente
        UpdateCurrentFont();

        // Si se esta mostrando el nombre del jugador
        var playerData = new[] { new { player = playerIndex + 1 } };
        LocalizationHelper.FormatTranslate(playerName, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.player_x, playerData);

        LocalizationHelper.Translate(pressToJoinLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.press_to_join);
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        playerName.font = mainFont;
        pressToJoinLabel.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        playerName.fontStyle = style;
        pressToJoinLabel.fontStyle = style;
    }

    /// <summary>
    /// Seleccionar al siguiente personaje disponible
    /// </summary>
    public void NextCharacter()
    {
        selectedIndex++;
        if (selectedIndex >= availablesCharacters.Count) selectedIndex = 0;
        ChangeCharacter(availablesCharacters[selectedIndex]);
    }

    /// <summary>
    /// Seleccionar al personaje anterior
    /// </summary>
    public void PrevCharacter()
    {
        selectedIndex--;
        if (selectedIndex < 0) selectedIndex = availablesCharacters.Count - 1;
        ChangeCharacter(availablesCharacters[selectedIndex]);
    }

    /// <summary>
    /// Saber si la caja esta unida a un jugador
    /// </summary>
    /// <returns></returns>
    public bool IsJoined()
    {
        return joined;
    }

    /// <summary>
    /// Indicar visualmente seleccion de personaje
    /// </summary>
    /// <param name="selected"></param>
    public void Select(bool selected = true)
    {
        if (tweenBorderColor != null) tweenBorderColor.Stop(TweenStopBehavior.Complete);

        if (selected && characterConfiguration) MasterSFXPlayer._player.PlayOneShot(characterConfiguration.SFXWin());

        System.Action<ITween<Color>> fadeFn = (t) =>
        {
            borderImage.color = t.CurrentValue;
        };

        Color initialColor = borderImage.color;
        Color finalColor = selected ? selectedColor : unselectedColor;
        tweenBorderColor = gameObject.Tween(string.Format("FadeJoin{0}", GetInstanceID()), initialColor, finalColor, borderColorTime, TweenScaleFunctions.QuadraticEaseInOut, fadeFn);
    }

    /// <summary>
    /// Reinicio de corutina
    /// </summary>
    private void OnEnable()
    {
        if (!joined)
        {
            if (tweenPressToJoin != null) tweenPressToJoin.Stop(TweenStopBehavior.Complete);
            if (tweenBorderColor != null) tweenBorderColor.Stop(TweenStopBehavior.Complete);
            if (pressToJoinLabelCoroutine != null) StopCoroutine(pressToJoinLabelCoroutine);
            pressToJoinLabelCoroutine = StartCoroutine(PressToJoinLabelCoroutine());
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate -= Localize;
        if (tweenPressToJoin != null) tweenPressToJoin.Stop(TweenStopBehavior.Complete);
        if (tweenBorderColor != null) tweenBorderColor.Stop(TweenStopBehavior.Complete);
        if (pressToJoinLabelCoroutine != null) StopCoroutine(pressToJoinLabelCoroutine);
    }

}