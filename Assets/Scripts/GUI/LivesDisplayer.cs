using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalRuby.Tween;

public class LivesDisplayer : MonoBehaviour{

    [Header("Hearts")]
    [SerializeField] GameObject heartsContainer;
    [SerializeField] GameObject heartPrefab;
    [SerializeField] Color heartActiveColor;
    [SerializeField] Color heartInactiveColor;

    [Header("Container")]
    [SerializeField] GameObject dotsContainer;
    [SerializeField] GameObject dotPrefab;
    [SerializeField] Color dotActiveColor;
    [SerializeField] Color dotInactiveColor;

    [Header("AnimationUp")]
    [SerializeField] float timeFirstStep = 0.4f;
    [SerializeField] float timeTwoStep = 0.1f;

    [Header("V2")]
    [SerializeField] TextMeshProUGUI playerLives;
    [SerializeField] Image playerIllustration;
    [SerializeField] TextMeshProUGUI playerName;

    [Header("SuperPowers")]
    [SerializeField] GameObject superPowerContainer;
    [SerializeField] GameObject superPowerContainerBG;
    [SerializeField] CanvasGroup superPowerMagicWand;
    [SerializeField] CanvasGroup superPowerSuperJanken;
    [SerializeField] [Range(0, 1)] float superPowerDeactivateAlpha = .5f;

    [Header("ReadyLabel")]
    [SerializeField] TextMeshProUGUI readyLabel;
    [SerializeField] CanvasGroup readyLabelCanvasGroup;
    [SerializeField] float readyLabelShowY = -115;
    [SerializeField] float readyLabelHideY = -70;
    [SerializeField] float timeToShowReadyLabel = .2f;

    [Header("Others")]
    [SerializeField] bool updateFontName = false;

    // Utiles
    int totalLives = 0;
    int currentLives = 0;
    int totalStreak = 0;
    int currentStreak = 0;

    // Tweens
    Tween<float> tweenReadyLabelAlpha;
    Tween<Vector2> tweenReadyLabelPosition;

    private void Start()
    {
        LanguageController.onLanguageChangeDelegate += Localize;
        Localize();
    }

    /// <summary>
    /// Localizar elementos
    /// </summary>
    private void Localize() {
        LocalizationHelper._this.TranslateThis(readyLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.ready);
        UpdateCurrentFont();
    }

    /// <summary>
    /// Se crearan tantos corazones de vida como se indiquen. En caso de que existan mas de los necesarios, se eliminaran
    /// </summary>
    /// <param name="total">Cantidad total de vidas</param>
    /// <param name="current">Permite establecer la vida actual directamente.</param>
    public void SetTotalLives(int total, int current = -1) {

        if (total < JankenUp.Limits.minLives || JankenUp.Limits.maxLives < total) return;

        totalLives = total;

        // Procesar la cantidad de corazones que se estan mostrando
        /*int currentHearts = heartsContainer.transform.childCount;

        if(currentHearts < totalLives)
        {
            // Agregar corazones
            for(int i = currentHearts; i < totalLives; i++)
            {
                Instantiate(heartPrefab, heartsContainer.transform);
            }
        }
        else if(currentHearts > totalLives)
        {
            // Quitar corazones
            for(int i = totalLives; i < currentHearts; i++)
            {
                Destroy(heartsContainer.transform.GetChild(i).gameObject);
            }
        }*/

        // Actualizar cantidad de vidas actuales
        if( current != -1 && current > totalLives ) current = totalLives;
        if( current != -1 ) SetCurrentLives(current);

    }

    /// <summary>
    /// Se establece cuantas vidas actuales deben mostrarse como activas
    /// </summary>
    /// <param name="current">Vidas actuales</param>
    public void SetCurrentLives(int current, bool noLimits = false) {

        if ( !noLimits && (current < JankenUp.Limits.minLives - 1 || JankenUp.Limits.maxLives < current || current > totalLives)) return;

        currentLives = current;
        playerLives.text = string.Format("x{0}", currentLives == 0? "D" : currentLives);
        //StartCoroutine(LifeUpHeartAnimation());
    }

    /// <summary>
    /// Se crearan tantas puntos de racha como se indiquen. En caso de que existan mas de los necesarios, se eliminaran
    /// </summary>
    /// <param name="total">Cantidad total de racha</param>
    /// <param name="current">Permite establecer la racha actual directamente.</param>
    public void SetTotalStreak(int total, int current = -1) {
        if (total < JankenUp.Limits.minStrikes - 1 || JankenUp.Limits.maxStrikes < total) return;

        totalStreak = total;

        // Procesar la cantidad de puntos que se estan mostrando
        int currentDots = dotsContainer.transform.childCount;

        if (currentDots < totalStreak)
        {
            // Agregar puntos
            for (int i = currentDots; i < totalStreak; i++)
            {
                Instantiate(dotPrefab, dotsContainer.transform);
            }
        }
        else if (currentDots > totalStreak)
        {
            // Quitar corazones
            for (int i = totalStreak; i < currentDots; i++)
            {
                Destroy(dotsContainer.transform.GetChild(i).gameObject);
            }
        }

        // Actualizar cantidad de puntos actuales
        if (current != -1 && current > totalStreak) current = totalStreak;
        if (current != -1) SetCurrentStreak(current);
    }

    /// <summary>
    /// Se establece cuantas vidas actuales deben mostrarse como activas
    /// </summary>
    /// <param name="current">Vidas actuales</param>
    public void SetCurrentStreak(int current) {

        if (current < JankenUp.Limits.minStrikes - 1 || JankenUp.Limits.maxStrikes < current || current > totalStreak) return;

        currentStreak = current;

        // Pintar los puntos segun se requiera
        for (int i = 0; i < totalStreak; i++)
        {
            GameObject dot = dotsContainer.transform.GetChild(i).gameObject;
            if (dot)
            {
                Image image = dot.GetComponent<Image>();
                image.color = i < currentStreak ? dotActiveColor : dotInactiveColor;
            }
        }

    }

    /// <summary>
    /// Iniciar la secuencia de full strike
    /// </summary>
    public void StartFullStrike()
    {
        StartCoroutine(FullStrike());
    }

    /// <summary>
    /// Animar la completitud del strike
    /// </summary>
    /// <returns></returns>
    public IEnumerator FullStrike()
    {
        // Apagar todos los puntos luego de 'brillar'
        int index = 0;
        int total = dotsContainer.transform.childCount;
        int complete = 0;
        Color clearWhite = new Color(1,1,1,0);

        foreach (Transform tr in dotsContainer.transform)
        {
            Image dotImage = tr.GetComponent<Image>();
            Image image = tr.Find("Up").GetComponent<Image>();

            // Realizar aparicion de elemento
            System.Action<ITween<Color>> updateColor = (t) =>
            {
                if(image) image.color = t.CurrentValue;
            };

            System.Action<ITween<Color>> updateColorHalfComplete = (t) =>
            {
                if(dotImage) dotImage.color = dotInactiveColor;
            };

            System.Action<ITween<Color>> updateColorComplete = (t) =>
            {
                complete++;
            };

            // Hacer cambio de color de los iconos
            tr.gameObject.
                Tween(string.Format("FadeOut{0}{1}", image.GetInstanceID(), index), clearWhite, Color.white, timeFirstStep, TweenScaleFunctions.QuadraticEaseOut, updateColor, updateColorHalfComplete)
                .ContinueWith(new ColorTween().Setup(Color.white, clearWhite, timeTwoStep, TweenScaleFunctions.QuadraticEaseOut, updateColor, updateColorComplete));

            index++;
        }

        // Esperar 
        while (complete != total)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Animacion de aumento de vida
    /// </summary>
    /// <param name="newLife">Nueva cantidad de vidas</param>
    /// <returns></returns>
    public IEnumerator LifeUp(int newLife) {

        SetCurrentLives(newLife);
        MasterSFXPlayer._player.LifeUp();
        //yield return StartCoroutine(FullStrike());
        yield return null;
    }

    /// <summary>
    /// Animacion para animar el cambio de cantidad de vidas
    /// </summary>
    /// <returns></returns>
    private IEnumerator LifeUpHeartAnimation()
    {
        // Recorrer todos los corazones y aplicar animacion en caso de que sea necesario
        /*int index = 0;
        foreach (Transform heart in heartsContainer.transform)
        {
            // Si el indice es igual o menor a las vidas, significa que pudo haber perdido una vida
            Image baseHeart = heart.GetComponent<Image>();

            // Si las vidas son mayores al indice actual, significa que pudo haber ganado una vida
            if (index < currentLives)
            {
                if(baseHeart.color == heartInactiveColor)
                {
                    LifeUp(baseHeart);
                }
            }
            else
            {
                if (baseHeart.color == heartActiveColor)
                {
                    LifeDown(baseHeart);
                }
            }

            index++;
        }

        yield return new WaitForSeconds(timeFirstStep);*/
        yield return null;
    }

    // Animacion de subida de vidas
    public void LifeUp(Image heart)
    {
       // Obtener el icono de subida
       /*GameObject iconUp = heart.transform.Find("Up").gameObject;
       iconUp.SetActive(true);
       Image spriteRenderer = iconUp.GetComponent<Image>();

       // Realizar desaparicion del ataque
       System.Action<ITween<Color>> updateColor = (t) =>
       {
           if(spriteRenderer != null) spriteRenderer.color = t.CurrentValue;
       };

       System.Action<ITween<Color>> updateColorComplete = (t) =>
       {
           if(iconUp != null) iconUp.SetActive(false);
       };

       System.Action<ITween<Color>> updateColorHalfComplete = (t) =>
       {
           if (heart != null) heart.color = heartActiveColor;
       };

       // Hacer fade out
       iconUp.gameObject.
           Tween(string.Format("IconFadeIn{0}", iconUp.GetInstanceID()), Color.clear, Color.white, timeFirstStep, TweenScaleFunctions.QuadraticEaseOut,
           updateColor, updateColorHalfComplete)
           .ContinueWith(new ColorTween().Setup(Color.white, Color.clear, timeTwoStep, TweenScaleFunctions.QuadraticEaseOut, updateColor, updateColorComplete));*/
    }

    // Animacion de perdida de vidas
    public void LifeDown(Image heart)
    {
/*
        // Realizar desaparicion del ataque
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if(heart) heart.color = t.CurrentValue;
        };

        // Hacer fade out
        heart.gameObject.
            Tween(string.Format("IconFadeOut{0}", heart.GetInstanceID()), heartActiveColor, heartInactiveColor, timeFirstStep, TweenScaleFunctions.QuadraticEaseOut,
            updateColor);*/
    }

    /// <summary>
    /// Obtener las vidas actuales
    /// </summary>
    /// <returns>Cantidad de vidas actuales</returns>
    public int GetLives()
    {
        return currentLives;
    }

    /// <summary>
    /// Obtener la racha actual
    /// </summary>
    /// <returns>Cantidad de racha actuales</returns>
    public int GetStreak()
    {
        return currentStreak;
    }

    /// <summary>
    /// Cambio de la imagen del jugador en el livedisplayer
    /// </summary>
    /// <param name="illustration"></param>
    public void SetAvatar(Sprite illustration)
    {
        playerIllustration.sprite = illustration;
    }

    /// <summary>
    /// Cambio en el nombre de jugador
    /// </summary>
    /// <param name="name"></param>
    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }

    /// <summary>
    /// Obtencion del elemento del nombre del jugador
    /// </summary>
    /// <returns></returns>
    public TextMeshProUGUI GetPlayerLabel()
    {
        return playerName;
    }

    /// <summary>
    /// Actualizar la fuente segun el idioma actual
    /// </summary>
    public void UpdateCurrentFont() {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        if(updateFontName) playerName.font = mainFont;
        readyLabel.font = mainFont;

        Material material = FontManager._mainManager.GetMainMaterial(FontManager.MainFontMaterial.White);
        readyLabel.fontSharedMaterial = material;
    }

    /// <summary>
    /// Mostrar los superpoderes
    /// </summary>
    public void ShowSuperPowers()
    {
        if (superPowerContainer) superPowerContainer.SetActive(true);
        if (superPowerContainerBG) superPowerContainerBG.SetActive(true);
    }

    /// <summary>
    /// Activar/Desactivar el poder de varita magica
    /// </summary>
    /// <param name="show"></param>
    public void ActivateMagicWand(bool show = true)
    {
        superPowerMagicWand.alpha = show ? 1 : superPowerDeactivateAlpha;
    }

    /// <summary>
    /// Activar/Desactivar el poder de super janken
    /// </summary>
    /// <param name="show"></param>
    public void ActivateSuperJanken(bool show = true)
    {
        superPowerSuperJanken.alpha = show ? 1 : superPowerDeactivateAlpha;
    }

    /// <summary>
    /// Mostrar el label de jugadas listas
    /// </summary>
    public void ShowReady() {

        if (tweenReadyLabelAlpha != null) tweenReadyLabelAlpha.Stop(TweenStopBehavior.DoNotModify);
        if (tweenReadyLabelPosition != null) tweenReadyLabelPosition.Stop(TweenStopBehavior.DoNotModify);

        // Realizar aparicion de elemento
        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (readyLabel) readyLabel.alpha = t.CurrentValue;
        };

        // Realizar movimiento del elemento
        System.Action<ITween<Vector2>> updatePosition = (t) =>
        {
            if (readyLabel) readyLabel.transform.localPosition = t.CurrentValue;
        };

        // Hacer cambio de alpha y posicion
        tweenReadyLabelAlpha = readyLabel.gameObject.
            Tween(string.Format("ReadyAlpha{0}", readyLabel.GetInstanceID()), readyLabel.alpha, 1, timeToShowReadyLabel, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);

        Vector2 finalPosition = new Vector2(readyLabel.transform.localPosition.x, readyLabelShowY);

        tweenReadyLabelPosition = readyLabel.gameObject.
            Tween(string.Format("ReadyPosition{0}", readyLabel.GetInstanceID()), readyLabel.transform.localPosition, finalPosition, timeToShowReadyLabel, TweenScaleFunctions.QuadraticEaseOut, updatePosition);

    }

    /// <summary>
    /// Esconder el label de jugadas listas
    /// </summary>
    public void HideReady() {

        if (tweenReadyLabelAlpha != null) tweenReadyLabelAlpha.Stop(TweenStopBehavior.DoNotModify);
        if (tweenReadyLabelPosition != null) tweenReadyLabelPosition.Stop(TweenStopBehavior.DoNotModify);

        // Realizar aparicion de elemento
        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (readyLabel) readyLabel.alpha = t.CurrentValue;
        };

        // Realizar movimiento del elemento
        System.Action<ITween<Vector2>> updatePosition = (t) =>
        {
            if (readyLabel) readyLabel.transform.localPosition = t.CurrentValue;
        };

        // Hacer cambio de alpha y posicion
        tweenReadyLabelAlpha = readyLabel.gameObject.
            Tween(string.Format("ReadyAlpha{0}", readyLabel.GetInstanceID()), readyLabel.alpha, 0, timeToShowReadyLabel, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);

        Vector2 finalPosition = new Vector2(readyLabel.transform.localPosition.x, readyLabelHideY);

        tweenReadyLabelPosition = readyLabel.gameObject.
            Tween(string.Format("ReadyPosition{0}", readyLabel.GetInstanceID()), readyLabel.transform.localPosition, finalPosition, timeToShowReadyLabel, TweenScaleFunctions.QuadraticEaseOut, updatePosition);

    }

    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= Localize;
        if (tweenReadyLabelAlpha != null) tweenReadyLabelAlpha.Stop(TweenStopBehavior.DoNotModify);
        if (tweenReadyLabelPosition != null) tweenReadyLabelPosition.Stop(TweenStopBehavior.DoNotModify);
    }

}
