using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DigitalRuby.Tween;

public class PlayerNotification : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] Image illustration;
    [SerializeField] TextMeshProUGUI playerName;

    [Header("Animation")]
    [SerializeField] bool fromLeft = false;
    [SerializeField] [Range(0, 100)] float xMovement = 40;
    [SerializeField] [Range(0, 1)] float timeToMove = 0.2f;
    [SerializeField] [Range(0, 1)] float timeToFade = 0.1f;
    [SerializeField] [Range(0, 1)] float timeToWait = 0.5f;

    // Components
    CanvasGroup canvasGroup;
    CharacterConfiguration characterConfiguration;

    // Coroutines + Tweens
    Coroutine animateCoroutine;
    Tween<float> tweenAlpha;
    Tween<Vector2> tweenPosition;

    private void Start()
    {
        UpdateCurrentFont();
        canvasGroup = GetComponent<CanvasGroup>();
        animateCoroutine = StartCoroutine(Animate());
    }

    /// <summary>
    /// Configuracion de los elementos
    /// </summary>
    /// <param name="characterConfiguration"></param>
    /// <param name="playerIndex"></param>
    public void Setup(CharacterConfiguration characterConfiguration, int playerIndex = 0)
    {
        // Configurar elementos
        this.characterConfiguration = characterConfiguration;
        illustration.sprite = this.characterConfiguration.GetCardIllustration();
        var playerData = new[] { new { player = playerIndex + 1 } };
        LocalizationHelper.FormatTranslate(playerName, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.player_x, playerData);
    }

    /// <summary>
    /// Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    public void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        playerName.font = mainFont;

        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        playerName.fontStyle = style;
    }

    /// <summary>
    /// Animacion del elemento
    /// </summary>
    /// <returns></returns>
    private IEnumerator Animate()
    {
        // Metodos de actualizacion
        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> updatePosition = (t) =>
        {
            if (transform) transform.localPosition = t.CurrentValue;
        };

        Vector2 initPosition = new Vector2(transform.localPosition.x + (fromLeft? -xMovement : xMovement), transform.localPosition.y);
        Vector2 finalPosition = transform.localPosition;

        // Mostrar y mover a posicion original
        tweenAlpha = gameObject.
            Tween(string.Format("Alpha{0}", GetInstanceID()), canvasGroup.alpha, 1, timeToFade, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);

        tweenPosition = gameObject.
            Tween(string.Format("Position{0}", GetInstanceID()), initPosition, finalPosition, timeToMove, TweenScaleFunctions.QuadraticEaseOut, updatePosition);

        // Emitir sonido
        MasterSFXPlayer._player.PlayOneShot(characterConfiguration.SFXWin());

        yield return new WaitForSeconds(timeToMove + timeToWait);

        // Ocultar y mover fuera de pantalla
        tweenAlpha = gameObject.
            Tween(string.Format("Alpha{0}", GetInstanceID()), canvasGroup.alpha, 0, timeToFade, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);

        tweenPosition = gameObject.
            Tween(string.Format("Position{0}", GetInstanceID()), finalPosition, initPosition, timeToMove, TweenScaleFunctions.QuadraticEaseOut, updatePosition);

        yield return new WaitForSeconds(timeToMove);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (animateCoroutine != null) StopCoroutine(animateCoroutine);
        if (tweenAlpha != null) tweenAlpha.Stop(TweenStopBehavior.DoNotModify);
        if (tweenPosition != null) tweenPosition.Stop(TweenStopBehavior.DoNotModify);
    }
}