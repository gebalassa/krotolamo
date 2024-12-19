using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalRuby.Tween;
using System;
using System.Globalization;
using UnityEngine.Localization.Settings;

public class PJDisplayer : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] TextMeshProUGUI characterName;

    [Header("Displayers")]
    [SerializeField] GameObject left;
    [SerializeField] GameObject center;
    [SerializeField] GameObject right;

    [Header("UnlockCharacters")]
    [SerializeField] TextMeshProUGUI extraMessageLabel;
    [SerializeField] bool showHowToUnlock = false;
    [SerializeField] float timeToFade = .4f;

    [Header("Character Overlay")]
    [SerializeField] Text characterMoreInfoButtonText;
    [SerializeField] GameObject characterOverlay;

    [Header("BuyCharacters")]
    [SerializeField] TextMeshProUGUI priceLabel;
    [SerializeField] Image priceIcon;
    [SerializeField] Button priceButton;

    [Header("Camera Inputs")]
    [SerializeField] private LayerMask inputLayerMask;

    // Configuration
    int characterIndex = 0;
    bool firstUpdate = true;

    // Components & Childs
    AudioSource audioSource;
    Vector3 leftTransform, centerTransform, rightTransform;

    // Colores textos
    Color textWhiteColor = JankenUp.JankenColors.white;
    Color textClearWhiteColor = JankenUp.JankenColors.clearWhite;

    // Tweens
    ColorTween nameTween;
    ColorTween priceTween;
    ColorTween extraMessageTween;

    // Identificacion del personaje actual
    string currentCharacterIdentifier = "";
    int currentCharacterPrice = 0;
    bool currentCharacterIsADeluxe = false;

    // Revisar si es single player para mostrar mensaje de desbloqueo
    SingleModeSession singleModeSession;

    void Start()
    {

        // Configurar mascara de eventos para camara
        Camera.main.eventMask = inputLayerMask;

        singleModeSession = FindObjectOfType<SingleModeSession>();

        audioSource = GetComponent<AudioSource>();

        leftTransform = left.transform.position;
        centerTransform = center.transform.position;
        rightTransform = right.transform.position;

        // Obtener el indice del último PJ seleccionado
        int preIndex = GameController.Load().characterIndex;
        if (preIndex < CharacterPool.Instance.Length()) characterIndex = preIndex;

        foreach (GameObject character in CharacterPool.Instance.GetAll())
        {
            CharacterConfiguration chInfo = character.GetComponent<CharacterConfiguration>();
            if (UnlockedCharacterController.IsUnlocked(chInfo.GetIdentifier()))
            {
                chInfo.Unlocked();
            }
        }

        // Cambiar texto boton de mas info deluxes
        LocalizationHelper.Translate(characterMoreInfoButtonText, JankenUp.Localization.tables.Characters.tableName, JankenUp.Localization.tables.Characters.Keys.moreInfo);

        // Actualizar la fuente de todos los labels
        UpdateCurrentFont();

    }

    // Update is called once per frame
    void Update()
    {
        if (firstUpdate)
        {
            // Actualizar despliegue
            UpdateDisplayer();
            firstUpdate = false;
        }
    }

    // Se actualizan los distintos displayers de personajes
    public void UpdateDisplayer() {

        // El de la izquierda despliega el anterior al indice
        int preIndex = characterIndex - 1 < 0 ? CharacterPool.Instance.Length() - 1 : characterIndex - 1;
        GameObject newLeft = Instantiate(CharacterPool.Instance.Get(preIndex), transform);
        CharacterConfiguration leftConfigutarion = newLeft.GetComponent<CharacterConfiguration>();
        CharacterInGameController leftController = newLeft.GetComponent<CharacterInGameController>();

        Destroy(left);
        left = newLeft;

        newLeft.transform.position = leftTransform;

        leftController.SetLock(!leftConfigutarion.IsUnlocked());

        leftController.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Foreground);
        leftController.ToggleRigidBody(true);

        // El del centro desplega el indice actual seleccionado
        GameObject newCenter = Instantiate(CharacterPool.Instance.Get(characterIndex), transform);
        CharacterConfiguration centerConf = newCenter.GetComponent<CharacterConfiguration>();
        CharacterInGameController centerController = newCenter.GetComponent<CharacterInGameController>();

        Destroy(center);
        center = newCenter;

        newCenter.transform.position = centerTransform;
        centerController.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Foreground);
        centerController.ToggleRigidBody(true);

        currentCharacterIdentifier = centerConf.GetIdentifier();
        currentCharacterPrice = centerConf.GetPrice();
        currentCharacterIsADeluxe = centerConf.IsADeluxeCharacter();

        if (centerConf.IsUnlocked())
        {
            centerController.SetLock(false);

            // Comprobaciones para el UI
            ShowName(true, centerConf.GetName());
            ShowPrice(false, 0);
            if (currentCharacterIsADeluxe)
            {

                if (GameController.IsCharacterInPlayerAccount(currentCharacterIdentifier)) ShowMessage(false, null, null);
                else if(extraMessageLabel)
                {
                    ShowMessage(true, "", null);
                    DateTime dateParse = new DateTime();
                    bool validDate = RemoteConfigHandler._main? DateTime.TryParse(RemoteConfigHandler._main.GetDeluxeUntil(), out dateParse) : false;
                    if (validDate)
                    {
                        var untilDate = new[] { new { date = dateParse } };
                        LocalizationHelper.FormatTranslate(extraMessageLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.freeDeluxeUntil, untilDate);
                    }
                }
            }
            else
            {
                ShowMessage(false, null, null);
            }
        }
        else
        {
            centerController.SetLock(true);
            ShowName(false, null);

            if (showHowToUnlock && singleModeSession)
            {
                ShowMessage(true, null,
                !currentCharacterIsADeluxe ? centerConf.GetIdentifier() : JankenUp.Characters.DELUXEEXCLUSIVE);
            }
            else
            {
                ShowMessage(currentCharacterIsADeluxe, null,
                currentCharacterIsADeluxe ? JankenUp.Characters.DELUXEEXCLUSIVE : "");
            }

            // Si tiene precio para ser desbloqueado y no es de tipo deluxe
            if (currentCharacterPrice > 0 && !currentCharacterIsADeluxe) ShowPrice(true, currentCharacterPrice);
            else ShowPrice(false, 0);

        }

        if (!firstUpdate && IsCurrentCharacterUnlocked())
        {
            audioSource.PlayOneShot(centerConf.SFXWin());
            GameController.SaveCharacterIndex(characterIndex);
        }

        // Indicar a la session sobre el personaje seleccionado
        JankenSession session = FindObjectOfType<JankenSession>();
        if (session && IsCurrentCharacterUnlocked())
        {
            // Hacer distinción entres tipo de sesiones
            if(session as SingleModeSession)
                (session as SingleModeSession).SetPlayer(CharacterPool.Instance.Get(characterIndex));
        }

        // El de la derecha despliega el siguiente al indice
        int postIndex = characterIndex + 1 >= CharacterPool.Instance.Length()? 0 : characterIndex + 1;
        GameObject newRight = Instantiate(CharacterPool.Instance.Get(postIndex), transform);
        CharacterConfiguration rightConfigutarion = newRight.GetComponent<CharacterConfiguration>();
        CharacterInGameController rightController = newRight.GetComponent<CharacterInGameController>();

        Destroy(right);
        right = newRight;

        newRight.transform.position = rightTransform;
        rightController.SetLock(!rightConfigutarion.IsUnlocked());
        rightController.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Foreground);
        rightController.ToggleRigidBody(true);

    }

    // Mover hacia la derecha la selección de personajes
    public void Next() {
        characterIndex++;
        if (characterIndex >= CharacterPool.Instance.Length()) characterIndex = 0;
        UpdateDisplayer();
    }

    // Mover hacia la izquierda la selección de personajes
    public void Previous()
    {
        characterIndex--;
        if (characterIndex < 0) characterIndex = CharacterPool.Instance.Length() - 1;
        UpdateDisplayer();
    }

    // Se indica si el personaje seleccionado actualmente esta desbloqueado o no
    public bool IsCurrentCharacterUnlocked() {
        CharacterConfiguration centerConf = CharacterPool.Instance.Get(characterIndex).GetComponent<CharacterConfiguration>();
        return centerConf.IsUnlocked();
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        TMP_FontAsset currentCPFont = FontManager._mainManager.GetMainFont();
        if(extraMessageLabel != null) extraMessageLabel.font = currentCPFont;

        Font plainFont = FontManager._mainManager.GetPlainFont();
        characterMoreInfoButtonText.font = plainFont;

        FontStyle fontStyle = FontManager._mainManager.IsBold()? FontStyle.Bold : FontStyle.Normal;
        characterMoreInfoButtonText.fontStyle = fontStyle;
    }

    // Obtencion del identificador del personaje actual
    public string GetCurrentCharacterIdentifier()
    {
        return currentCharacterIdentifier;
    }

    // Obtencion del precio en monedas del personaje actual
    public int GetCurrentCharacterPrice()
    {
        return currentCharacterPrice;
    }

    // Obtencion de si el personaje es delujooo
    public bool GetCurrentCharacterIsADeluxe()
    {
        return currentCharacterIsADeluxe;
    }

    // Mostrar el nombre del PJ
    private void ShowName(bool show, string name)
    {
        // Ingresar nombre de inmediato
        if (characterName != null && name != null) characterName.text = name;

        // Revisar si se esta ejecutando la misma accion en base al tween
        if ( characterName == null || (nameTween != null && nameTween.EndValue == (show ? textWhiteColor : textClearWhiteColor))) return;

        // Activar de inmediato el nombre si es que que se desea mostrar
        if (show) characterName.gameObject.SetActive(true);

        // Realizar tween
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            characterName.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> updateColorComplete = (t) =>
        {
            characterName.gameObject.SetActive(show);
        };

        nameTween = characterName.gameObject.Tween(string.Format("FadeIn{0}", characterName.GetInstanceID()),
            (show ? textClearWhiteColor : textWhiteColor), (show ? textWhiteColor : textClearWhiteColor),
            timeToFade, TweenScaleFunctions.QuadraticEaseOut, updateColor, updateColorComplete);
    }

    // Mostrar el precio del PJ
    private void ShowPrice(bool show, int price)
    {
        // Ingresar precio de inmediato
        if (priceLabel != null && price > 0) priceLabel.text = price.ToString();

        // Revisar si se esta ejecutando la misma accion en base al tween
        if (priceLabel == null || (priceTween != null && priceTween.EndValue == (show ? textWhiteColor : textClearWhiteColor))) return;

        if (priceTween == null && price <= 0) return;

        // Activar de inmediato el precio si es que que se desea mostrar
        if (show)
        {
            priceLabel.gameObject.SetActive(true);
            priceIcon.gameObject.SetActive(true);
            priceButton.gameObject.SetActive(true);
        }

        // Realizar tween
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            priceLabel.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> updateButtonColor = (t) =>
        {
            priceIcon.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> updateColorComplete = (t) =>
        {
            priceLabel.gameObject.SetActive(show);
            priceIcon.gameObject.SetActive(show);
            priceButton.gameObject.SetActive(show);
        };

        priceIcon.gameObject.Tween(string.Format("FadeInButton{0}", priceIcon.GetInstanceID()),
            (show ? Color.clear : Color.white), (show ? Color.white : Color.clear),
            timeToFade, TweenScaleFunctions.QuadraticEaseOut, updateButtonColor);

        priceTween = priceLabel.gameObject.Tween(string.Format("FadeIn{0}", priceLabel.GetInstanceID()),
            (show ? textClearWhiteColor : textWhiteColor), (show ? textWhiteColor : textClearWhiteColor),
            timeToFade, TweenScaleFunctions.QuadraticEaseOut, updateColor, updateColorComplete);
    }

    // Mostrar texto adicional para el personaje
    private void ShowMessage(bool show, string message, string characterIdentifier)
    {
        // Ingresar mensaje de inmediato
        if (extraMessageLabel != null) {

            if (message != null)
            {
                extraMessageLabel.text = message;
            }
            else if(characterIdentifier != null)
            {
                LocalizationHelper.Translate(extraMessageLabel, JankenUp.Localization.tables.Achievements.tableName, characterIdentifier);
            }

        } 

        // Revisar si se esta ejecutando la misma accion en base al tween
        if (extraMessageLabel == null || (extraMessageTween != null && extraMessageTween.EndValue == (show ? textWhiteColor : textClearWhiteColor))) return;

        // Activar de inmediato el texto si es que que se desea mostrar
        if (show) extraMessageLabel.gameObject.SetActive(true);

        // Realizar tween
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            extraMessageLabel.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> updateColorComplete = (t) =>
        {
            extraMessageLabel.gameObject.SetActive(show);
        };

        extraMessageTween = extraMessageLabel.gameObject.Tween(string.Format("FadeIn{0}", extraMessageLabel.GetInstanceID()),
            (show ? textClearWhiteColor : textWhiteColor), (show ? textWhiteColor : textClearWhiteColor),
            timeToFade, TweenScaleFunctions.QuadraticEaseOut, updateColor, updateColorComplete);
    }

    // Mostrar la informacion adicional del personaje deluxe seleccionado actualmente
    public void ShowDeluxeCharacterMoreInfo()
    {
        // Obtener la escena actual
        SceneController sceneController = FindObjectOfType<SceneController>();

        // Mostrar el detalle del personaje
        sceneController.SwitchUI(OpenDetail(), true);

    }

    IEnumerator OpenDetail()
    {
        JanKenShopCharacterOverlay jksOverlay = Instantiate(characterOverlay).GetComponent<JanKenShopCharacterOverlay>();
        CharacterConfiguration configuration = CharacterPool.Instance.Get(characterIndex).GetComponent<CharacterConfiguration>();
        jksOverlay.Setup(configuration);
        yield return null;
    }

}
