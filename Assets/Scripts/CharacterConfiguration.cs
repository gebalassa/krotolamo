using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class CharacterConfiguration : MonoBehaviour, IElementItem
{
    [Header("Data")]
    [SerializeField] string name;
    [SerializeField] string identifier;
    [SerializeField] string parentIdentifier = "";
    [SerializeField] int priceToUnlock;
    [SerializeField] bool deluxeCharacter = false;
    [SerializeField] int order = 0;
    [SerializeField] bool secretCharacter = false;

    [Header("Animators")]
    [SerializeField] RuntimeAnimatorController inGame;

    [Header("SFX")]
    [SerializeField] AudioClip[] rock;
    [SerializeField] AudioClip[] paper;
    [SerializeField] AudioClip[] scissors;
    [SerializeField] AudioClip[] win;
    public List<SpecialSoundEffectsStruct> specialWinSFX;
    [SerializeField] AudioClip[] lose;
    public List<SpecialSoundEffectsStruct> specialLoseSFX;
    [SerializeField] AudioClip magicWandSFX;

    [Header("Avatar & Illustration")]
    [SerializeField] Sprite avatar;
    [SerializeField] PJISelectorllustration pjSelectorIllustration;

    [Header("Character")]
    [SerializeField] string origin = JankenUp.Localization.tables.Country.Keys.chile;
    [SerializeField] GameObject onUIPrefab;

    [Header("Music")]
    [SerializeField] Music characterMusic = Music.InGame;
    [SerializeField] bool customMusic = false;

    [Header("Card")]
    [SerializeField] Sprite cardIllustration;
    [SerializeField] Vector2 cardIllustrationPosition;
    [SerializeField] Vector2 cardShinePosition;
    [SerializeField] Vector2 cardStatsPosition;
    [SerializeField] Color cardColor;
    [SerializeField] Color cardShineColor;
    [SerializeField] int cardIllustrationWidth = 0;
    [SerializeField] int cardIllustrationHeight = 0;
    [SerializeField] bool cardShineRotateX = false;
    [SerializeField] bool cardShineRotateY = false;
    [SerializeField][Range(-360, 360)] float cardShineRotation = 0;
    [SerializeField][Range(-360, 360)] float cardIllustrationRotation = 0;
    [SerializeField][Range(0, 9999)] int cardIDNumber = 0;
    [SerializeField][Range(0, 9999)] int cardQLPoints = 0;
    [SerializeField][Range(0, 99)] int cardRockPoints = 0;
    [SerializeField][Range(0, 99)] int cardPaperPoints = 0;
    [SerializeField][Range(0, 99)] int cardScissorsPoints = 0;

    [Header("Selection")]
    [SerializeField] Vector2 selectionllustrationPosition;

    [Header("RRSS")]
    [SerializeField] string instagramURL = "";
    [SerializeField] string tiktokURL = "";
    [SerializeField] string twitterURL = "";
    [SerializeField] string webtoonURL = "";
    [SerializeField] string youtubeURL = "";
    [SerializeField] string medibangURL = "";

    [Header("Lang")]
    [SerializeField] bool descriptionFromConfiguration = false;
    [SerializeField] List<CharacterDescriptionStruct> descriptionLangs = new List<CharacterDescriptionStruct>();

    [Header("Music")]
    [SerializeField] public MusicAddresableStruct musicAddresableStruct;

    public string GetName()
    {
        return name;
    }

    /// <summary>
    /// Obtiene el identificador del personaje
    /// </summary>
    /// <returns></returns>
    public string GetIdentifier()
    {
        return identifier;
    }

    /// <summary>
    /// Obtiene el identificador padre (En caso de que sea una skin)
    /// </summary>
    /// <returns></returns>
    public string GetParentIdentifier()
    {
        return (parentIdentifier != "")? parentIdentifier : identifier;
    }

    // Las siguientes funciones se encargan de retornar el AudioClip asocioado a una acción
    public AudioClip SFXRock()
    {
        return rock.Length > 0? rock[Random.Range(0, rock.Length)] : null;
    }

    public AudioClip SFXPaper()
    {
        return paper.Length > 0 ? paper[Random.Range(0, paper.Length)] : null;
    }

    public AudioClip SFXScissors()
    {
        return scissors.Length > 0 ? scissors[Random.Range(0, scissors.Length)] : null;
    }

    public AudioClip SFXSMagicWand()
    {
        return magicWandSFX;
    }

    /// <summary>
    /// Retorno de un audio clip para cuando se gana la partida
    /// </summary>
    /// <returns></returns>
    public AudioClip SFXWin(string otherCharacterIdentifier = null)
    {
        if (otherCharacterIdentifier != null && specialWinSFX.Count > 0)
        {
            SpecialSoundEffectsStruct? specialSFX = specialWinSFX.Find(keyPair => keyPair.identifier == otherCharacterIdentifier);
            if (specialSFX.GetValueOrDefault().identifier != null)
            {
                bool doSpecial = Random.Range(0, 2) == 1;
                if(doSpecial) return specialSFX.Value.audioClips[Random.Range(0, specialSFX.Value.audioClips.Count)];
            }
        }

        return win.Length > 0? win[ Random.Range( 0, win.Length )] : null;
    }

    /// <summary>
    /// Retorno de un audio clip para cuando se pierde  la partida
    /// </summary>
    /// <returns></returns>
    public AudioClip SFXLose(string otherCharacterIdentifier = null)
    {
        if (otherCharacterIdentifier != null && specialLoseSFX.Count > 0)
        {
            SpecialSoundEffectsStruct? specialSFX = specialLoseSFX.Find(keyPair => keyPair.identifier == otherCharacterIdentifier);
            if (specialSFX.GetValueOrDefault().identifier != null)
            {
                bool doSpecial = Random.Range(0, 2) == 1;
                if (doSpecial) return specialSFX.Value.audioClips[Random.Range(0, specialSFX.Value.audioClips.Count)];
            }
        }

        return lose.Length > 0 ? lose[Random.Range(0, lose.Length)] : null;
    }

    // Funcion para obtener el animador para la selección
    public RuntimeAnimatorController GetAnimator()
    {
        return inGame;
    }

    // Obtener el avatar del jugador
    public Sprite GetAvatar()
    {
        return avatar;
    }

    // Revisar si personaje esta desbloqueado o no
    public bool IsUnlocked() {
        return UnlockedCharacterController.IsUnlocked(GetIdentifier());
    }

    // Desbloquear el personaje
    public void Unlocked()
    {
        UnlockedCharacterController.Unlock(GetIdentifier());
    }

    // Obtener el precio del personaje
    public int GetPrice()
    {
        return priceToUnlock;
    }

    // Obtener la informacion de si es un personaje deluxe
    public bool IsADeluxeCharacter()
    {
        return deluxeCharacter;
    }

    // Obtener el subtitulo para el personaje deluxe
    public string GetSubtitle()
    {
        return origin;
    }

    // Obtener si el personaje tiene una musica custom asignada
    public bool HasCustomMusic()
    {
        return customMusic;
    }

    // Obtener la musica que identifica al personaje para jugar con el
    public Music GetCharacterMusic()
    {
        return characterMusic;
    }

    // Obtener el objeto para mostrar en UI
    public GameObject GetCharacterOnUIPrefab()
    {
        return onUIPrefab;
    }

    /// <summary>
    /// Retorna el orden dentro del listado de personajes
    /// </summary>
    /// <returns></returns>
    public int GetOrder()
    {
        return order;
    }

    /// <summary>
    /// Indica si el personaje forma parte de los personajes secretos del juego
    /// </summary>
    /// <returns></returns>
    public bool IsSecretCharacter()
    {
        return secretCharacter;
    }

    /// <summary>
    /// Obtencion de la ilustracion del PJ para el selector
    /// </summary>
    /// <returns></returns>
    public PJISelectorllustration GetPJSelectorIllustration()
    {
        return pjSelectorIllustration;
    }

    #region Card data

    /// <summary>
    /// Obtener el numero identificador de carta
    /// </summary>
    /// <returns></returns>
    public int GetCardNumber()
    {
        return cardIDNumber;
    }

    /// <summary>
    /// Obtener los puntos de calidad de la carta
    /// </summary>
    /// <returns></returns>
    public int GetCardQlPoints()
    {
        return cardQLPoints;
    }

    /// <summary>
    /// Obtener los puntos de piedra de la carta
    /// </summary>
    /// <returns></returns>
    public int GetCardRockStats()
    {
        return cardRockPoints;
    }

    /// <summary>
    /// Obtener los puntos de papel de la carta
    /// </summary>
    /// <returns></returns>
    public int GetCardPaperStats()
    {
        return cardPaperPoints;
    }

    /// <summary>
    /// Obtener los puntos de tijeras de la carta
    /// </summary>
    /// <returns></returns>
    public int GetCardScissorsStats()
    {
        return cardScissorsPoints;
    }

    /// <summary>
    /// Obtener la ilustracion de la card
    /// </summary>
    /// <returns></returns>
    public Sprite GetCardIllustration()
    {
        return cardIllustration;
    }

    /// <summary>
    /// Posicion de la ilustracion en la card
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCardIllustrationPosition()
    {
        return cardIllustrationPosition;
    }

    /// <summary>
    /// Tamano de la imagen de la card
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCardIllustrationSize()
    {
        return new Vector2(cardIllustrationWidth, cardIllustrationHeight);
    }

    /// <summary>
    /// Rotacion de la imagen de la ilustracion de la card
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCardIllustrationRotation()
    {
        return cardIllustrationRotation != 0? new Vector3(0, 0, cardIllustrationRotation) : Vector3.zero;
    }

    /// <summary>
    /// Posicion de los stats en la card
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCardStatsPosition()
    {
        return cardStatsPosition;
    }

    /// <summary>
    /// Obtencion del color de la card
    /// </summary>
    /// <returns></returns>
    public Color GetCardColor()
    {
        return cardColor;
    }

    /// <summary>
    /// Obtencion del color del 'brillo' de la card
    /// </summary>
    /// <returns></returns>
    public Color GetCardShinecolor()
    {
        return cardShineColor;
    }

    /// <summary>
    /// Posicion del 'brillo' en la card
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCardShinePosition()
    {
        return cardShinePosition;
    }

    /// <summary>
    /// Rotacion del 'brillo' de la ilustracion de la card
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCardShineRotation()
    {
        return cardShineRotation != 0 ? new Vector3(0, 0, cardShineRotation) : Vector3.zero;
    }

    /// <summary>
    /// Indica si debe multiplicarse por 1 o -1 la escala del 'brillo' en el eje X
    /// </summary>
    /// <returns></returns>
    public bool GetCardShineRotateX()
    {
        return cardShineRotateX;
    }

    /// <summary>
    /// Indica si debe multiplicarse por 1 o -1 la escala del 'brillo' en el eje Y
    /// </summary>
    /// <returns></returns>
    public bool GetCardShineRotateY()
    {
        return cardShineRotateY;
    }

    #endregion

    #region RRSS
    /// <summary>
    /// Obtencion de la URL de instagram asociada al personaje
    /// </summary>
    /// <returns></returns>
    public string GetInstagram() {
        return instagramURL;
    }

    /// <summary>
    /// Obtencion de la URL de tiktok asociada al personaje
    /// </summary>
    /// <returns></returns>
    public string GetTiktok() {
        return tiktokURL;
    }

    /// <summary>
    /// Obtencion de la URL de X asociada al personaje
    /// </summary>
    /// <returns></returns>
    public string GetTwitter() {
        return twitterURL;
    }

    /// <summary>
    /// Obtencion de la URL de webtoon asociada al personaje
    /// </summary>
    /// <returns></returns>
    public string GetWebtoon() {
        return webtoonURL;
    }

    /// <summary>
    /// Obtencion de la URL de youtube asociada al personaje
    /// </summary>
    /// <returns></returns>
    public string GetYoutube() {
        return youtubeURL;
    }

    /// <summary>
    /// Obtencion de la URL de medibang asociada al personaje
    /// </summary>
    /// <returns></returns>
    public string GetMedibang() {
        return medibangURL;
    }

    #endregion

    #region Selection Data
    /// <summary>
    /// Posicion de la ilustracion en la caja de seleccion
    /// </summary>
    /// <returns></returns>
    public Vector2 GetSelectionIllustrationPosition()
    {
        return selectionllustrationPosition;
    }
    #endregion

    /// <summary>
    /// Se obtiene el valor para saber si la descripcion debe salir desde el componente de configuracion
    /// </summary>
    /// <returns></returns>
    public bool GetDescriptionFromConfiguration()
    {
        return descriptionFromConfiguration;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string GetDescriptionForLang(string lang = "en")
    {
        bool contains = descriptionLangs.Any(cds => cds.lang == lang);
        if (!contains) return "";
        return descriptionLangs.Find(cds => cds.lang == lang).description;
    }
}
