using AirFishLab.ScrollingList;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PJSelector : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Button nextButton;
    [SerializeField] Button previousButton;
    [SerializeField] Button moreInformation;

    [Header("Characters")]
    [SerializeField] CharacterOnUI currentCharacter;
    [SerializeField] TextMeshProUGUI characterName;
    [SerializeField] GameObject characterOverlay;

    [Header("Text")]
    [SerializeField] TextMeshProUGUI extraMessage;

    [Header("QuickLinks")]
    [SerializeField] CircularScrollingList circularList;

    [Header("Options")]
    [SerializeField] bool showOnlyAvailable = false;
    [SerializeField] bool showFreeDeluxeMessage = true;
    [SerializeField] bool showCharacterMoreInformation = true;

    public static PJSelector _instance;
    public static PJSelector Instance { get { return _instance; } }

    // Index seleccionado
    int characterIndex = 0;
    int characterBoxesHalf = 0;
    string characterIdentifier = "";

    // Listado de personajes disponibles para este pool
    List<GameObject> charactersAvailables;

    // Util para determinar si cambia al centrar o no
    bool avatarWasClicked = false;
    Coroutine avatarWasUpdatedCoroutine;

    // Evento para notificar del cambio de personaje
    public delegate void OnCharacterChange();
    public event OnCharacterChange onCharacterChangeDelegate;
    private float timeBeforeNotifyCharacterChange = .25f;
    Coroutine coroutineNotifyChangeCharacterInstance;

    // Otros
    bool firstUpdate = true;

    // Configuracion de la instancia actual
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {

        // Obtener los personajes
        charactersAvailables = showOnlyAvailable ? CharacterPool.Instance.GetAvailables() : CharacterPool.Instance.GetAll();

        nextButton.onClick.AddListener(Next);
        previousButton.onClick.AddListener(Previous);

        if (!showCharacterMoreInformation) moreInformation.gameObject.SetActive(false);
        else moreInformation.onClick.AddListener(ShowCharacterMoreInformation);

        // Revisar y desbloquear los PJ que sea necesario. Ademas, rellenar la lista de avatares
        AvatarListBank bankList = circularList.GetComponent<AvatarListBank>();
        foreach (GameObject character in charactersAvailables)
        {
            CharacterConfiguration chInfo = character.GetComponent<CharacterConfiguration>();
            if (UnlockedCharacterController.IsUnlocked(chInfo.GetIdentifier()))
            {
                chInfo.Unlocked();
            }
            bankList.AddToContents(new AvatarBox(chInfo.GetAvatar(), !chInfo.IsUnlocked()));
        }


        // Obtener el indice del último PJ seleccionado
        string preIdentifier = GameController.Load().characterIdentifier;
        // Encontrar el identificador en el listado de personajes disponibles
        int preIndex = charactersAvailables.FindIndex(character => character.GetComponent<CharacterConfiguration>().GetIdentifier() == preIdentifier);

        if (preIndex < charactersAvailables.Count) circularList.SelectContentID(preIndex);

        // Inicializar el listado de PJ
        circularList.Initialize();

        // Obtener la cantidad de hijos que tiene el listado
        characterBoxesHalf = circularList.transform.childCount / 2;

        UpdateCurrentFont();

        // Realizar la primera actualizacion
        UpdateDisplayer(true, false);
    }

    /// <summary>
    /// Actualizacion del personaje que se muestra en el display
    /// </summary>
    void UpdateDisplayer(bool rightDirection = true, bool playSfx = true)
    {
        CharacterOnUI tempCharacter = currentCharacter;
        CharacterConfiguration newCharacterConfiguration = charactersAvailables[characterIndex].GetComponent<CharacterConfiguration>();
        currentCharacter = Instantiate(newCharacterConfiguration.GetCharacterOnUIPrefab(), tempCharacter.transform.parent).GetComponent<CharacterOnUI>();
        currentCharacter.SetLock(!newCharacterConfiguration.IsUnlocked());
        currentCharacter.transform.localPosition = tempCharacter.transform.localPosition;
        currentCharacter.Show(rightDirection);

        // Animar y destruir objeto temporal
        tempCharacter.Hide(rightDirection);

        // Cambiar nombre de personaje
        characterName.text = newCharacterConfiguration.GetName();
        characterIdentifier = newCharacterConfiguration.GetIdentifier();

        // Volver a habilitar el cambio del personaje por centrar el circular list
        if (avatarWasClicked)
        {
            if (avatarWasUpdatedCoroutine != null) StopCoroutine(avatarWasUpdatedCoroutine);
            avatarWasUpdatedCoroutine = StartCoroutine(DeactivateAvatarWasClicked(currentCharacter.GetFadeTime()));
        }

        // Si el personaje es de tipo deluxe y esta gratis, indicar fecha hasta la que se encuentra liberada (En caso de que jugador no lo tenga). Comprobar ademas que se deba mostrar el mensaje de gratuidad
        if (newCharacterConfiguration.IsUnlocked() && newCharacterConfiguration.IsADeluxeCharacter()
            && showFreeDeluxeMessage)
        {
            DateTime dateParse = new DateTime();
            bool validDate = RemoteConfigHandler._main ? DateTime.TryParse(RemoteConfigHandler._main.GetDeluxeUntil(), out dateParse) : false;
            if (validDate)
            {
                var untilDate = new[] { new { date = dateParse } };
                LocalizationHelper.FormatTranslate(extraMessage, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.freeDeluxeUntil, untilDate);
            }
            else
            {
                extraMessage.text = "";
            }
        }
        else
        {
            extraMessage.text = "";
        }

        if (firstUpdate)
        {
            firstUpdate = false;
        }
        else
        {
            // Guardar para proximos selectores
            GameController.SaveCharacterIdentifier(newCharacterConfiguration.GetIdentifier());

            // Reproducir un sonido del adversario
            if (playSfx) MasterSFXPlayer._player.PlayOneShot(newCharacterConfiguration.SFXWin());

            // Llamar a los metodos suscritos al cambio de personaje (Si ha transcurrido el tiempo)
            StartCoroutineNofityChangeCharacter();
        }

    }

    /// <summary>
    /// Elimina y da inicio a la rutina de notificacion de cambio de personaje
    /// </summary>
    private void StartCoroutineNofityChangeCharacter()
    {
        if (coroutineNotifyChangeCharacterInstance != null) StopCoroutine(coroutineNotifyChangeCharacterInstance);
        coroutineNotifyChangeCharacterInstance = StartCoroutine(CoroutineNotifyChangeCharacter());
    }

    /// <summary>
    /// Corutina de notificacion de cambio de personaje
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoroutineNotifyChangeCharacter()
    {
        yield return new WaitForSeconds(timeBeforeNotifyCharacterChange);
        if (onCharacterChangeDelegate != null) onCharacterChangeDelegate();
    }

    /// <summary>
    /// Desactiva la propiedad que indica que se presiono eligio PJ por click
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator DeactivateAvatarWasClicked(float time)
    {
        yield return new WaitForSeconds(time);
        avatarWasClicked = false;
    }

    /// <summary>
    /// Cambiar el personaje al indice indicado
    /// </summary>
    /// <param name="index"></param>
    public void SetIndex(int index) {
        if (index >= charactersAvailables.Count || index < 0 || index == characterIndex) return;
        avatarWasClicked = true;
        bool rightDirection = GetRightDirection(index);
        characterIndex = index;
        UpdateDisplayer(rightDirection);
    }

    /// <summary>
    /// Cambiar el personaje al indice indicado haciendo drag
    /// </summary>
    /// <param name="index"></param>
    public void SetIndexByCenter(int index)
    {
        if (index >= charactersAvailables.Count || index < 0 || index == characterIndex || avatarWasClicked) return;
        bool rightDirection = GetRightDirection(index);
        characterIndex = index;
        UpdateDisplayer(rightDirection);
    }

    /// <summary>
    /// Cambiar al siguiente personaje
    /// </summary>
    public void Next() {
        int _currentID =
            (int)Mathf.Repeat(circularList.GetCenteredContentID() + 1, circularList.listBank.GetListLength());
        circularList.SelectContentID(_currentID); 
    }

    /// <summary>
    /// Cambiar al personaje anterior
    /// </summary>
    public void Previous(){
        int _currentID =
            (int)Mathf.Repeat(circularList.GetCenteredContentID() - 1, circularList.listBank.GetListLength());
        circularList.SelectContentID(_currentID);
    }

    /// <summary>
    /// Cambia el indice tanto del personaje que se muestra como de selector circular
    /// </summary>
    /// <param name="index"></param>
    public void SetIndexOnDisplay(int index)
    {
        int _currentID =
           (int)Mathf.Repeat(circularList.GetCenteredContentID() + index, circularList.listBank.GetListLength());
        circularList.SelectContentID(_currentID);
    }

    /// <summary>
    /// Obtener si en base al indice debe moverse hacia la izq
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    bool GetRightDirection(int index)
    {
        // Codigo copiado desde CirculatScrollingList
        var difference = index - characterIndex;
        int characters = charactersAvailables.Count;

        if (Mathf.Abs(difference) > characters / 2)
        {
            difference -= (int)Mathf.Sign(difference) * characters;
        }

        return difference > 0;
    }

    /// <summary>
    /// Mostrar la informacion adicional del personaje seleccionado actualmente
    /// </summary>
    public void ShowCharacterMoreInformation()
    {
        if (!showCharacterMoreInformation) return;
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
        CharacterConfiguration configuration = charactersAvailables[characterIndex].GetComponent<CharacterConfiguration>();
        jksOverlay.Setup(configuration);
        yield return null;
    }

    /// <summary>
    /// Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    public void UpdateCurrentFont()
    {
        TMP_FontAsset currentCPFont = FontManager._mainManager.GetMainFont();
        extraMessage.font = currentCPFont;
    }

    /// <summary>
    /// Obtencion del personaje seleccionado actualmente
    /// </summary>
    /// <returns></returns>
    public string GetCurrentIdentifier()
    {
        return characterIdentifier;
    }

    /// <summary>
    /// Obtencion del personaje seleccionado actualmente
    /// </summary>
    /// <returns></returns>
    public GameObject GetCurrentCharacter()
    {
        return CharacterPool.Instance.Get(characterIdentifier);
    }

}
