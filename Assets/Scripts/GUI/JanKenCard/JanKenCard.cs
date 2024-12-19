using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalRuby.Tween;
using UnityEngine.Video;
using System;
using System.IO;
using System.Collections.Generic;

public class JanKenCard : MonoBehaviour{

    [Header("Texts")]
    [SerializeField] TextMeshProUGUI characterName;
    [SerializeField] TextMeshProUGUI characterPlace;
    [SerializeField] TextMeshProUGUI characterNumber;
    [SerializeField] Text qlPoints;
    [SerializeField] TextMeshProUGUI rockStats;
    [SerializeField] TextMeshProUGUI paperStats;
    [SerializeField] TextMeshProUGUI scissorsStats;

    [Header("Components")]
    [SerializeField] Image characterIllustration;
    [SerializeField] Image videoFallbackImage;
    [SerializeField] RawImage videoImage;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] Image shineImage;
    [SerializeField] GameObject statsContainer;
    [SerializeField] Material lockMaterial;
    [SerializeField] Image rockImage;
    [SerializeField] Image paperImage;
    [SerializeField] Image scissorsImage;
    [SerializeField] Image qlPointsImage;

    [Header("Animation")]
    [SerializeField] [Range(-100, 100)] float movementX = 0;
    [SerializeField] [Range(-100, 100)] float movementY = 0;
    [SerializeField] [Range(-100, 100)] float rotation = 0;
    [SerializeField] [Range(0, 1)] float timeToAnimate = .5f;
    [SerializeField] bool rotateAllowed = true;
    int movementXFactor = 1;

    [Header("Screenshot")]
    [SerializeField] GameObject screenshotCanvasPrefab;

    [Header("Limits")]
    [SerializeField] [Range(0, 99999)] int maxQlPoints = 99999;
    [SerializeField] [Range(0, 100)] int maxRPSPoints = 100;

    [Header("Others")]
    [SerializeField] bool showPlayerName = false;
    int playerIndex = 0;

    CharacterConfiguration characterConfiguration;
    CanvasGroup canvasGroup;

    // Tweens
    Tween<Vector2> tweenMovement;
    Tween<Vector3> tweenRotation;
    Tween<float> tweenFade;
    Tween<float> tweenFadeVideoFallback;

    // B&W
    Color lockVideoColor = new Color(255, 255, 255, 1);
    Color lockShineColor = new Color(0, 0, 0, .27f);
    Material lockMaterialCopy;

    // Others
    bool firstConfiguration = true;
    Vector2 cardOriginalPosition;
    float cardOriginalRotation;
    bool animationReady = false;
    int minRPSDisplay = 0;
    int maxRPSDisplay = 99;

    // Ajustes para archivo
    string filesPath;
    string filesFolder = "screenshots";
    string filenamePrefix = "jankencard_";
    float timeToCapture = 0.5f;

    // Use this for initialization
    void Start()
    {
        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;
        videoPlayer.prepareCompleted += VideoPlayerPrepareCompleted;
    }

    /// <summary>
    /// Ajuste para cuando el video ya se encuentre cargado
    /// </summary>
    /// <param name="source"></param>
    private void VideoPlayerPrepareCompleted(VideoPlayer source)
    {
        videoPlayer.isLooping = true;
        CanvasGroup canvasGroup = videoImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null) return;

        System.Action<ITween<float>> fadeFn = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };
        System.Action<ITween<float>> fadeFnComplete = (t) =>
        {
            if (videoFallbackImage) videoFallbackImage.gameObject.SetActive(false);
        };
        tweenFadeVideoFallback = gameObject.Tween(string.Format("FadeVideo{0}", videoPlayer.gameObject.GetInstanceID()), 0, 1, timeToAnimate / 2, TweenScaleFunctions.QuadraticEaseInOut, fadeFn, fadeFnComplete);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Animacion de personaje
    /// </summary>
    void Animate()
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        if (firstConfiguration)
        {
            firstConfiguration = false;
            canvasGroup = GetComponent<CanvasGroup>();
            cardOriginalPosition = rectTransform.anchoredPosition;
            cardOriginalRotation = gameObject.transform.eulerAngles.z;
        }

        System.Action<ITween<Vector2>> moveFn = (t) =>
        {
            if (gameObject) rectTransform.anchoredPosition = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> moveFnComplete = (t) =>
        {
            if (gameObject) animationReady = true;
        };

        System.Action<ITween<Vector3>> rotateFn = (t) =>
        {
            if (gameObject) gameObject.transform.eulerAngles = t.CurrentValue;
        };

        System.Action<ITween<float>> fadeFn = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        Vector2 initialPosition = new Vector2(cardOriginalPosition.x + (movementX * movementXFactor), cardOriginalPosition.y + movementY);
        Vector2 finalPosition = cardOriginalPosition;

        tweenMovement = gameObject.Tween(string.Format("Move{0}", gameObject.GetInstanceID()), initialPosition, finalPosition, timeToAnimate, TweenScaleFunctions.QuadraticEaseInOut, moveFn, moveFnComplete);
        tweenFade = gameObject.Tween(string.Format("Fade{0}", gameObject.GetInstanceID()), 0, 1, timeToAnimate / 2, TweenScaleFunctions.QuadraticEaseInOut, fadeFn);

        if (rotateAllowed)
        {
            Vector3 initalRotation = new Vector3(0, 0, rotation);
            Vector3 finalRotation = new Vector3(0, 0, cardOriginalRotation);
            tweenRotation = gameObject.Tween(string.Format("Rotate{0}", gameObject.GetInstanceID()), initalRotation, finalRotation, timeToAnimate, TweenScaleFunctions.QuadraticEaseInOut, rotateFn);
        }
    }

    /// <summary>
    /// Configuracion de los distintos elementos de la card
    /// </summary>
    /// <param name="characterConfiguration"></param>
    /// <param name="animate"></param>
    /// <param name="forceUnlocked"></param>
    public void Setup(CharacterConfiguration characterConfiguration, bool animate = true, bool forceUnlocked = false, bool showPlayerName = false, int playerIndex = 0, bool animationInverse = false)
    {
        this.characterConfiguration = characterConfiguration;
        this.showPlayerName = showPlayerName;
        this.playerIndex = playerIndex;
        this.movementXFactor = animationInverse ? 1 : -1;
        Localize();

        // Textos
        characterName.text = characterConfiguration.GetName();
        SetCardNumber(characterConfiguration.GetCardNumber());
        SetQlPoints(characterConfiguration.GetCardQlPoints());
        rockStats.text = AddLeadingZerosToRPS(characterConfiguration.GetCardRockStats(), 2);
        paperStats.text = AddLeadingZerosToRPS(characterConfiguration.GetCardPaperStats(), 2);
        scissorsStats.text = AddLeadingZerosToRPS(characterConfiguration.GetCardScissorsStats(), 2);
        Localize();

        // Ilustracion
        characterIllustration.sprite = characterConfiguration.GetCardIllustration();
        characterIllustration.rectTransform.anchoredPosition = characterConfiguration.GetCardIllustrationPosition();
        characterIllustration.rectTransform.sizeDelta = characterConfiguration.GetCardIllustrationSize();
        Vector3 illustrationRotation = characterConfiguration.GetCardIllustrationRotation();
        illustrationRotation.z += transform.eulerAngles.z;
        characterIllustration.transform.eulerAngles = illustrationRotation;

        // Brillo
        shineImage.color = characterConfiguration.GetCardShinecolor();
        bool shineRotateX = characterConfiguration.GetCardShineRotateX();
        bool shineRotateY = characterConfiguration.GetCardShineRotateY();
        shineImage.transform.localScale = new Vector2(shineRotateX ? -1 : 1, shineRotateY ? -1 : 1);
        shineImage.transform.eulerAngles = characterConfiguration.GetCardShineRotation();

        // Otros
        videoImage.color = characterConfiguration.GetCardColor();
        if(videoFallbackImage.isActiveAndEnabled) videoFallbackImage.color = characterConfiguration.GetCardColor();
        statsContainer.GetComponent<RectTransform>().anchoredPosition = characterConfiguration.GetCardStatsPosition();

        // Si esta bloqueado, feedback visual
        BlackAndWhite(forceUnlocked || characterConfiguration.IsUnlocked());

        animationReady = false;
        if (animate) Animate();
        else animationReady = true;
    }

    /// <summary>
    /// Seteo del numero de la card
    /// </summary>
    /// <param name="number"></param>
    public void SetCardNumber(int number = 0) {
        characterNumber.text = string.Format("#{0}", number);
    }

    /// <summary>
    /// Seteo de los puntos de un jugador
    /// </summary>
    /// <param name="points"></param>
    public void SetQlPoints(int points = 0)
    {
        if (points > maxQlPoints) points = maxQlPoints;
        qlPoints.text = AddLeadingZeros(points.ToString(), 4);
    }

    /// <summary>
    /// Seteo de los ataques a partir de una secuencia
    /// </summary>
    /// <param name="attackList"></param>
    public void SetAttacks(List<Attacks> attackList)
    {
        if (attackList == null || attackList.Count == 0) return;
        float total = attackList.FindAll(a => a == Attacks.Rock || a == Attacks.Paper || a == Attacks.Scissors).Count;
        if (total == 0) return;
        int rock = (int)Math.Floor(attackList.FindAll(a => a == Attacks.Rock).Count / total * maxRPSPoints);
        int paper = (int) Math.Floor(attackList.FindAll(a => a == Attacks.Paper).Count / total * maxRPSPoints);
        int scissors = (int) Math.Floor(attackList.FindAll(a => a == Attacks.Scissors).Count / total * maxRPSPoints);
        int diff = maxRPSPoints - (rock + paper + scissors);
        if (diff != 0)
        {
            // Asignacion random de la diferencia
            int random = UnityEngine.Random.Range(0, 3);
            switch (random)
            {
                case 1:
                    paper += diff;
                    break;
                case 2:
                    scissors += diff;
                    break;
                default:
                    rock += diff;
                    break;
            }
        }
        rockStats.text = AddLeadingZerosToRPS(rock, 2);
        paperStats.text = AddLeadingZerosToRPS(paper, 2);
        scissorsStats.text = AddLeadingZerosToRPS(scissors, 2);
    }

    /// <summary>
    /// Agrega 0 al inicio de un texto
    /// </summary>
    /// <param name="text"></param>
    /// <param name="digits"></param>
    /// <returns></returns>
    private string AddLeadingZeros(string text = "", int digits = 1)
    {
        if (text.Length >= digits) return text;
        int diff = digits - text.Length;
        string finalText = "";
        for(int i = 0; i < diff; i++)
        {
            finalText += "0";
        }
        return finalText + text;
    }

    /// <summary>
    /// Agregar los 0 al inicio para los puntos RPS
    /// </summary>
    /// <param name="points"></param>
    /// <param name="digits"></param>
    /// <returns></returns>
    private string AddLeadingZerosToRPS(int points = 0, int digits =1)
    {
        points = Mathf.Clamp(points, minRPSDisplay, maxRPSDisplay);
        return AddLeadingZeros(points.ToString(), digits);
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        // Actualizar la fuente
        UpdateCurrentFont();

        // Si se esta mostrando el nombre del jugador
        if (showPlayerName)
        {
            var playerData = new[] { new { player = playerIndex + 1 } };
            LocalizationHelper.FormatTranslate(characterPlace, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.player_x, playerData);
        }
        else
        {
            LocalizationHelper.Translate(characterPlace, JankenUp.Localization.tables.Country.tableName, characterConfiguration.GetSubtitle());
        }
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        characterPlace.font = mainFont;

        // Cambiar el material
        Material mainMaterial = FontManager._mainManager.GetMainMaterial(FontManager.MainFontMaterial.White);
        characterPlace.fontSharedMaterial = mainMaterial;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        characterPlace.fontStyle = style;
    }

    /// <summary>
    /// Convertir a blanco y negro si esta bloqueado el personaje
    /// </summary>
    void BlackAndWhite(bool unlocked = false)
    {
        CheckLockMaterial();
        characterIllustration.material = unlocked? null : lockMaterialCopy;
        rockImage.material = unlocked ? null : lockMaterialCopy;
        paperImage.material = unlocked ? null : lockMaterialCopy;
        scissorsImage.material = unlocked ? null : lockMaterialCopy;
        qlPointsImage.material = unlocked ? null : lockMaterialCopy;
        if (!unlocked)
        {
            videoImage.color = lockVideoColor;
            shineImage.color = lockShineColor;
        }
    }

    /// <summary>
    /// Revision de si ya esta creado el material para personaje bloqueado
    /// </summary>
    void CheckLockMaterial()
    {
        if (lockMaterialCopy == null)
        {
            lockMaterialCopy = new Material(lockMaterial);
            lockMaterialCopy.SetFloat("_GrayscaleAmount", 1);
        }
    }

    /// <summary>
    /// Comprobar la existencia de la carpeta donde se guardaran los archivos
    /// </summary>
    private void CheckFilesPath()
    {
        filesPath = Path.Combine(Application.persistentDataPath, filesFolder);
        if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);
    }

    /// <summary>
    /// Compartir una captura de pantalla del perfil
    /// </summary>
    public void ShareScreenshot()
    {
        CheckFilesPath();
        StartCoroutine(TakeAndShare());
    }

    /// <summary>
    /// Toma una captura de pantalla y la comparte
    /// </summary>
    /// <returns></returns>
    private IEnumerator TakeAndShare()
    {
        // Crear el objeto overlay
        GameObject screenshotCanvas = null;
        GameObject cardCopy = null;
        if (screenshotCanvasPrefab != null)
        {
            screenshotCanvas = Instantiate(screenshotCanvasPrefab);
            JanKenCardPortraitBackground janKenCardPortraitBackground = screenshotCanvas.GetComponent<JanKenCardPortraitBackground>();
            janKenCardPortraitBackground.SetColor(characterConfiguration.GetCardColor());
            cardCopy = Instantiate(gameObject);
            cardCopy.transform.parent = screenshotCanvas.transform;
            RectTransform rectTransform = cardCopy.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, 0);
            rectTransform.localScale = new Vector3(.6f, .6f, 0);
            cardCopy.transform.eulerAngles = new Vector3(0, 0, 95);
            while (!janKenCardPortraitBackground.IsReady())
            {
                yield return null;
            }
        }

        // Crear una distincion por fecha y hora
        string date = DateTime.Now.ToString().Replace("/", "-").Replace(" ", "_").Replace(":", "-");
        string screenshotName = string.Format("{0}_{1}.png", filenamePrefix, date);
        yield return new WaitForEndOfFrame();

#if UNITY_EDITOR
        string finalPath = filesPath;
        string sharePath = filesPath;
        string screenShotPath = string.Format("{0}/{1}", finalPath, screenshotName);
#else
        string finalPath = filesFolder;
        string sharePath = string.Format("{0}/{1}/{2}", Application.persistentDataPath, finalPath, screenshotName);
        string screenShotPath = sharePath;
#endif
        //ScreenCapture.CaptureScreenshot(screenShotPath, 1);

        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        // Guardar la textura como PNG
        byte[] byteArray = RotateTexture(ss).EncodeToPNG();
        System.IO.File.WriteAllBytes(screenShotPath, byteArray);

        //yield return new WaitForSeconds(timeToCapture);

        if (cardCopy != null) Destroy(cardCopy);
        if (screenshotCanvas != null) Destroy(screenshotCanvas);
        Destroy(ss);

        // Compartir
        NativeShare nativeShare = new NativeShare();
        nativeShare.AddFile(sharePath);
        nativeShare.Share();
    }

    /// <summary>
    /// Rotatcion de textura
    /// </summary>
    /// <param name="originalTexture"></param>
    /// <param name="clockwise"></param>
    /// <returns></returns>
    Texture2D RotateTexture(Texture2D originalTexture, bool clockwise = true)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;
        int iRotated, iOriginal;
        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }
        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    /// <summary>
    /// Revision de si ya esta animada la card
    /// </summary>
    /// <returns></returns>
    public bool IsAnimationReady()
    {
        return animationReady;
    }

    private void OnDestroy()
    {
        // Desuscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate -= Localize;
        if (tweenMovement != null) tweenMovement.Stop(TweenStopBehavior.DoNotModify);
        if (tweenRotation != null) tweenRotation.Stop(TweenStopBehavior.DoNotModify);
        if (tweenFade != null) tweenFade.Stop(TweenStopBehavior.DoNotModify);
        videoPlayer.prepareCompleted -= VideoPlayerPrepareCompleted;
    }
}