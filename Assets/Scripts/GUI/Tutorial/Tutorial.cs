using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DigitalRuby.Tween;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System;

public class Tutorial : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] Transform[] moveToFront;
    [SerializeField] bool hide = false;
    [SerializeField] float hideInSeconds = 3;

    [Header("Camera")]
    [SerializeField] GameObject cameraOverlay;
    [SerializeField] TextMeshProUGUI longTextObject;
    [SerializeField] string[] tagsMoveToFront;
    int spriteOverlayIndex = 25;
    Color overlayColor = new Color(0, 0, 0, .5f);

    // Mantención de los indices
    GameObject[] tagsObjects;
    List<int> originalSpriteIndex = new List<int>();
    List<SpriteRenderer> finalSpriteRenderers = new List<SpriteRenderer>();

    [Header("Referee")]
    [SerializeField] string text = "";
    [SerializeField] string longText = "";
    Referee referee;
    GameObject refereeTutorial;

    [Header("UI")]
    [SerializeField] ScoreDisplayer scoreDisplayer;
    [SerializeField] TimeDisplayer timeDisplayer;
    [SerializeField] RoundFloorsDisplayer roundFloorsDisplayer;

    // Niveles que tienen tutorial
    public static int[] classicModeWithTutorial = new int[] { 0, 5, 6, 7, 9, 10 };
    public static int[] survivalModeWithTutorial = new int[] { 0, 5, 7, 9, 10 };
    public static List<int> classicModeLevelsWithChangeAttack = new List<int>(){ 6, 7 };
    public static List<int> survivalModeLevelsWithChangeAttack = new List<int>(){ 0, 5, 7, 9 };
    private static int superPowerTutorialAtLevel = 7;

    // Actualizar correctamente la fuente a usar segun el idioma
    private void Start()
    {
        UpdateCurrentFont();
        LanguageController.onLanguageChangeDelegate += Localization;
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        TMP_FontAsset currentCPFont = FontManager._mainManager.GetMainFont();
        longTextObject.font = currentCPFont;
        longTextObject.fontStyle = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
    }

    // Mostrar el tutorial
    public void Show()
    {
        // Quitar el score o timer
        if(scoreDisplayer) scoreDisplayer.Toggle(false);
        if(timeDisplayer) timeDisplayer.Toggle(false);
        if(roundFloorsDisplayer) roundFloorsDisplayer.Toggle(false);

        // Activar el overlay de la camara
        ToggleCameraOverlay(true);

        // Obtener el objeto que esta en el canvas
        Transform parent = transform.parent;

        // Mover todos los demás al frente del padre
        foreach (Transform t in moveToFront)
        {
            t.SetSiblingIndex(parent.GetSiblingIndex());
        }

        // Mover los sprites en camara al frente
        foreach (string tag in tagsMoveToFront)
        {
            // Buscar los objetos que coincidan con los tags
            tagsObjects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject g in tagsObjects)
            {
                SpriteRenderer sp = g.GetComponent<SpriteRenderer>();
                if (sp)
                {
                    originalSpriteIndex.Add(sp.sortingOrder);
                    finalSpriteRenderers.Add(sp);
                    sp.sortingLayerID = SortingLayer.NameToID(JankenUp.SpritesSortingLayers.Foreground);
                }
            }
        }

        // Detectar a los PJ en pantalla y moverlos capa default
        CharacterInGameController[] characterInGameControllers = FindObjectsOfType<CharacterInGameController>();
        foreach(CharacterInGameController characterInGameController in characterInGameControllers)
        {
            characterInGameController.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Foreground);
        }

        // Además, activar el globo del referee
        referee = FindObjectOfType<Referee>();
        if (referee)
        {
            refereeTutorial = referee.transform.Find("Tutorial").gameObject;

            refereeTutorial.SetActive(true);
            // Cambiar texto
            GameObject refereeTutorialText = refereeTutorial.transform.Find("Text").gameObject;
            TextMeshPro refereeTMP = refereeTutorialText.GetComponent<TextMeshPro>();
            LocalizationHelper.Translate(refereeTMP, JankenUp.Localization.tables.Tutorial.tableName, text);
        }

        // Revisar si debe esconderse despues de X segundos
        if (hide)
        {
            Hide();
        }
    }

    // Quitar de pantalla
    public void Hide()
    {
        StartCoroutine(HideCoroutine());
    }

    public IEnumerator HideCoroutine()
    {
        if (hide) yield return new WaitForSeconds(hideInSeconds);

        // Mostrar el score
        if(scoreDisplayer) scoreDisplayer.Toggle(true);
        if(timeDisplayer) timeDisplayer.Toggle(true);
        if(roundFloorsDisplayer) roundFloorsDisplayer.Toggle(true);

        GameObject parent = transform.parent.gameObject;
        parent.SetActive(false);
        ToggleCameraOverlay(false);

        // Volver los elementos a su posición inicial
        int index = 0;
        foreach (SpriteRenderer sp in finalSpriteRenderers)
        {
            if(sp != null) sp.sortingLayerID = SortingLayer.NameToID( JankenUp.SpritesSortingLayers.Arena );
        }

        // Detectar a los PJ en pantalla y moverlos capa default
        CharacterInGameController[] characterInGameControllers = FindObjectsOfType<CharacterInGameController>();
        foreach (CharacterInGameController characterInGameController in characterInGameControllers)
        {
            characterInGameController.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Arena);
        }

        finalSpriteRenderers.Clear();
        originalSpriteIndex.Clear();

        // Quitar el mensaje tutorial
        if (refereeTutorial)
        {
            refereeTutorial.SetActive(false);
        }

        /* Nota: Esto estará muy en bruto pero es por que este tutorial es medio bruto
         * Se buscará el actual SingleModeController y se llamará a la función AfterTutorial
         */
        SingleModeController smc = FindObjectOfType<SingleModeController>();
        if (smc && hide) smc.AfterTutorial();
    }


    // Mostrar o esconder el overlay de tutorial
    private void ToggleCameraOverlay(bool show){

        if (show)
        {
            cameraOverlay.SetActive(show);
            longTextObject.gameObject.SetActive(show);
            LocalizationHelper.Translate(longTextObject, JankenUp.Localization.tables.Tutorial.tableName, longText);
        }
        Image imagePanelOverlay = cameraOverlay.transform.Find("Panel").GetComponent<Image>();

        System.Action<ITween<Color>> updateOverlay = (t) =>
        {
            imagePanelOverlay.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> updateText = (t) =>
        {
            longTextObject.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> completeOverlay = (t) =>
        {
            cameraOverlay.SetActive(show);
            longTextObject.gameObject.SetActive(show);
        };

        Color initialColor = show ? Color.clear : overlayColor;
        Color endColor = show ? overlayColor : Color.clear;

        Color textInitialColor = show ? JankenUp.JankenColors.clearWhite : JankenUp.JankenColors.white;
        Color textEndColor = show ? JankenUp.JankenColors.white : JankenUp.JankenColors.clearWhite;

        // Aumentar el color del fondo
        cameraOverlay.Tween(string.Format("Toggle{0}", cameraOverlay.GetInstanceID()), initialColor, endColor,
            .4f, TweenScaleFunctions.QuadraticEaseInOut, updateOverlay, completeOverlay);

        // Aumentar el color del texto
        cameraOverlay.Tween(string.Format("Toggle{0}", longTextObject.GetInstanceID()), textInitialColor, textEndColor,
            .4f, TweenScaleFunctions.QuadraticEaseInOut, updateText);

    }

    // Niveles modo clasico en los que necesita mostrarse el cambio de ataque
    public bool LevelsWithChangeAttack(int level)
    {
        return classicModeLevelsWithChangeAttack.Contains(level);
    }

    // Niveles modo survival en los que necesita mostrarse el cambio de ataque
    public bool SurvivalLevelsWithChangeAttack(int level)
    {
        return survivalModeLevelsWithChangeAttack.Contains(level);
    }

    // Obtener si el nivel es un tutorial en el modo clasico
    public static bool IsClassicModeTutorial(int level)
    {
        return Array.IndexOf(Tutorial.classicModeWithTutorial, level) > -1;
    }

    // Obtener si el nivel es un tutorial en el modo survival
    public static bool IsSurvivalModeTutorial(int level)
    {
        return Array.IndexOf(Tutorial.classicModeWithTutorial, level) > -1;
    }

    // Obtener el nivel que tiene el tutorial de superpoderes
    public static int GetSuperPowerLevel()
    {
        return superPowerTutorialAtLevel;
    }

    /// <summary>
    /// Indica si el tutorial esta o no completado
    /// </summary>
    /// <returns></returns>
    public static bool IsTutorialReady()
    {
        TutoriaslCompleted  tutorialsCompleted = GameController.LoadTutorials();

        int total = 0;
        foreach (int level in classicModeWithTutorial)
        {
            if (tutorialsCompleted.tutorials.Contains(level)) total++;
        }

        return total == classicModeWithTutorial.Length;
    }

    /// <summary>
    /// Ajuste del texto del tutorial
    /// </summary>
    private void Localization()
    {
        GameObject refereeTutorialText = refereeTutorial.transform.Find("Text").gameObject;
        TextMeshPro refereeTMP = refereeTutorialText.GetComponent<TextMeshPro>();

        UpdateCurrentFont();

        // Ajustes textos
        if (text != "" && refereeTMP != null) LocalizationHelper.Translate(refereeTMP, JankenUp.Localization.tables.Tutorial.tableName, text);
        if (longText != "" && longTextObject != null) LocalizationHelper.Translate(longTextObject, JankenUp.Localization.tables.Tutorial.tableName, longText);
    }

    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= Localization;
    }

}
