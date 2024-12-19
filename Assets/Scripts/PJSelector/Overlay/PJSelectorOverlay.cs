using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PJSelectorOverlay : OverlayObject
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI titleOverlay;
    [SerializeField] ElementSelector elementSelector;
    [SerializeField] CharacterOnUI currentCharacter;
    [SerializeField] TextMeshProUGUI characterName;
    [SerializeField] PJISelectorllustration characterIllustration;
    [SerializeField] Button characterInformation;
    [SerializeField] JKButton buyButton;
    [SerializeField] CoinsCurrencyDisplayer coinsDisplayer;

    [Header("Others")]
    [SerializeField] GameObject characterOverlay;

    // Componentes y propiedades recurrentes
    Transform characterParent;
    Vector3 characterPosition;
    Transform illustrationParent;
    CharacterConfiguration currentCharacterConfiguration;
    string lastIdentifier = "";
    string lastSavedIdentifier = "";

    // Utiles
    bool firstChange = true;
    List<string> newUnlockedCharacters = new List<string>();

    // Delegados para cuando se cambie de personaje
    public delegate void OnValidSelection(CharacterConfiguration characterConfiguration);
    public static event OnValidSelection onValidSelectionDelegate;

    public delegate void OnSelection(CharacterConfiguration characterConfiguration);
    public static event OnSelection onSelectionDelegate;

    // Use this for initialization
    protected override void Start()
    {
        //base.Start();

        // Suscribirse a los cambios en el selector de personajes
        elementSelector.itemSelectedDelegate += onCharacterSelectionChange;

        // Obtener elementos recurrentes
        characterParent = currentCharacter.transform.parent;
        illustrationParent = characterIllustration.transform.parent;

        // Boton de mas info
        characterInformation.onClick.AddListener(ShowCharacterMoreInformation);

        // Actualizar textos
        Localize();

        // Indicar que personaje deberia estar seleccionado
        //StartCoroutine(SetInitialCharacter());

        // Suscribirse al evento de cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;

        // Habilitar compra de personajes
        buyButton.onClickDelegate += BuyCharacter;
        JanKenShopCharacterOverlay.onBuyDelegate += OnBuyThroughDetails;
    }

    /// <summary>
    /// Solicitud para cambiar al personaje seleccionado en un inicio
    /// </summary>
    /// <returns></returns>
    IEnumerator SetInitialCharacter(string identifier = "")
    {
        while (!elementSelector.IsReady) yield return null;
        if(identifier == "") identifier = GameController.Load().characterIdentifier;
        if(CharacterPool.Instance.Get(identifier) == null) identifier = JankenUp.Characters.BOYLEE;
        elementSelector.SelectByIdentifier(identifier);
    }

    /// <summary>
    /// El selector de elementos indica que hubo un cambio en el personaje seleccionado
    /// </summary>
    /// <param name="elementsSelected"></param>
    /// <param name="validChange"></param>
    void onCharacterSelectionChange(List<ElementItem> elementsSelected, bool validChange)
    {
        if (currentCharacterConfiguration != null && currentCharacterConfiguration.GetIdentifier() == elementsSelected[0].GetElementSource().GetComponent<CharacterConfiguration>().GetIdentifier()) return;

        // Al ser un selector de personajes unicos, siempre se considerara el primer elemento
        currentCharacterConfiguration = elementsSelected[0].GetElementSource().GetComponent<CharacterConfiguration>();
        lastIdentifier = currentCharacterConfiguration.GetIdentifier();

        // Realizar el cambio del PJ en UI
        CharacterOnUI tempCharacter = currentCharacter;
        currentCharacter = Instantiate(currentCharacterConfiguration.GetCharacterOnUIPrefab(), characterParent).GetComponent<CharacterOnUI>();
        currentCharacter.transform.localPosition = tempCharacter.transform.localPosition;
        currentCharacter.Show();
        currentCharacter.SetLock(!currentCharacterConfiguration.IsUnlocked());

        // Realizar cambios a la ilustracion del PJ que se esta mostrando
        PJISelectorllustration tempIllustration = characterIllustration;
        characterIllustration = Instantiate(currentCharacterConfiguration.GetPJSelectorIllustration(), illustrationParent).GetComponent<PJISelectorllustration>();
        characterIllustration.Show();
        characterIllustration.UpdateBlackAndWhite(!currentCharacterConfiguration.IsUnlocked());

        // Guardar para proximos selectores y llamar a eventos
        if (onSelectionDelegate != null) onSelectionDelegate(currentCharacterConfiguration);

        if (validChange)
        {
            lastSavedIdentifier = currentCharacterConfiguration.GetIdentifier();
            if (!firstChange) SaveCurrentCharacter();
        }

        // Animar y destruir objeto temporal
        if (firstChange){
            firstChange = false;
            Destroy(tempCharacter.gameObject);
            Destroy(tempIllustration.gameObject);
        }
        else{
            tempCharacter.Hide();
            tempIllustration.Hide();
        }

        // Compra
        if (currentCharacterConfiguration.IsUnlocked())
        {
            buyButton.Hide();
        }
        else{
            buyButton.gameObject.SetActive(true);
            buyButton.GetText().text = currentCharacterConfiguration.GetPrice().ToString();
        }

        // Cambiar nombre de personaje
        characterName.text = currentCharacterConfiguration.GetName();

        // Reproducir un sonido del adversario
        MasterSFXPlayer._player.PlayOneShot(currentCharacterConfiguration.SFXWin());
    }

    /// <summary>
    /// Guardado de informacion relacionada al personaje actual
    /// </summary>
    private void SaveCurrentCharacter()
    {
        GameController.SaveCharacterIdentifier(currentCharacterConfiguration.GetIdentifier());
        if (onValidSelectionDelegate != null) onValidSelectionDelegate(currentCharacterConfiguration);
        // Precargar la musica del personaje
        MasterAudioPlayer._player.PlayOrLoadThis(currentCharacterConfiguration.GetCharacterMusic(), false);
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
    /// Mostrar la informacion adicional del personaje seleccionado
    /// </summary>
    public void ShowCharacterMoreInformation()
    {
        // Obtener la escena actual
        SceneController sceneController = FindObjectOfType<SceneController>();
        sceneController.SwitchUI(OpenMoreInformation());
    }

    /// <summary>
    /// Corutina para mostrar el despligue de informacion del personaje seleccionado
    /// </summary>
    /// <returns></returns>
    IEnumerator OpenMoreInformation()
    {
        JanKenShopCharacterOverlay jksOverlay = Instantiate(characterOverlay).GetComponent<JanKenShopCharacterOverlay>();
        jksOverlay.Setup(currentCharacterConfiguration);
        yield return null;
    }

    /// <summary>
    /// Actualizacion del contenido cuando un nuevo personaje fue anadido al inventario
    /// </summary>
    /// <param name="identifier"></param>
    void OnAddCharacter(string identifier) {
        // Actualizar el personaje en UI si es el agregado
        if(currentCharacterConfiguration.GetIdentifier() == identifier)
        {
            currentCharacter.SetLock(false);
            characterIllustration.UpdateBlackAndWhite(false);
            buyButton.Hide();
            MasterSFXPlayer._player.PlayOneShot(currentCharacterConfiguration.SFXWin());
            SaveCurrentCharacter();
            if (onSelectionDelegate != null) onSelectionDelegate(currentCharacterConfiguration);
        }

        // Indicar al elemento seleccionador que debe actualizar el estado de uno de los elementos en base a su ID
        elementSelector.UpdateByIdentifier(identifier);
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
    /// Realizar la compra del personaje actualmente seleccionado
    /// </summary>
    public void BuyCharacter()
    {
        SingleModeSession singleModeSession = FindObjectOfType<SingleModeSession>();
        if (currentCharacterConfiguration.IsUnlocked() || !singleModeSession) return;
        if(currentCharacterConfiguration.GetPrice() <= singleModeSession.GetCoins())
        {
            // Gastar monedas y guardar los datos del jugador
            singleModeSession.SpendCoins(currentCharacterConfiguration.GetPrice());
            singleModeSession.SyncInitialCoins();
            GameController.Save(singleModeSession.GetCoins());
            UnlockedCharacterController.NewUnlockNotShow(currentCharacterConfiguration.GetIdentifier());
            OnAddCharacter(currentCharacterConfiguration.GetIdentifier());

            // Reproducir sonido de monedas
            MasterSFXPlayer._player.Coins();

            // Actualizar el UI de monedas
            if(coinsDisplayer) coinsDisplayer.OnCoinsCurrencyUpdate();
        }
        else
        {
            // Reproducir sonido de error
            MasterSFXPlayer._player.Error();
        }
    }

    /// <summary>
    /// Guardado de identificador comprado cuando se estaba en pantalla de detalle
    /// </summary>
    /// <param name="identifier"></param>
    private void OnBuyThroughDetails(string identifier)
    {
        newUnlockedCharacters.Add(identifier);
    }

    /// <summary>
    /// Seleccionar el PJ que corresponda
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        if (lastSavedIdentifier != GameController.Load().characterIdentifier) StartCoroutine(SetInitialCharacter());
        if (newUnlockedCharacters.Count > 0)
        {
            foreach(string newUnlockedCharacter in newUnlockedCharacters)
            {
                // Actualizar el elemento selector
                elementSelector.UpdateByIdentifier(newUnlockedCharacter);
            }
            newUnlockedCharacters.Clear();
            if (coinsDisplayer) coinsDisplayer.OnCoinsCurrencyUpdate();
        }
    }

    /// <summary>
    /// Realizar el cambio de personaje en base a la accion de un joystick
    /// </summary>
    /// <param name="action"></param>
    public bool MoveTo(JoystickAction action) {
        return elementSelector.MoveTo(action);
    }

    /// <summary>
    /// Desucripcion de eventos
    /// </summary>
    private void OnDestroy()
    {
        // Desuscribirse a los cambios en el selector de personajes
        elementSelector.itemSelectedDelegate -= onCharacterSelectionChange;
        LanguageController.onLanguageChangeDelegate -= Localize;
        JanKenShopCharacterOverlay.onBuyDelegate -= OnBuyThroughDetails;
    }
}