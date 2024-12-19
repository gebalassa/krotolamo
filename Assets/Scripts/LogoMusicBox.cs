using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JankenUp;
using DigitalRuby.Tween;

public class LogoMusicBox : MonoBehaviour
{

    [Header("Keys")]
    [SerializeField] Button kro;
    [SerializeField] Button to;
    [SerializeField] Button la;
    [SerializeField] Button mo;
    [SerializeField] Button japanese;

    [Header("ImagesExtra")]
    [SerializeField] Image japaneseImage;

    [Header("SFX")]
    [SerializeField] List<AudioClip> aratakaSounds;
    [SerializeField] List<AudioClip> everyoneSounds;
    [SerializeField] AudioClip unlockEveryoneAudioClip;
    List<AudioClip> instrument;

    [Header("Animation")]
    [SerializeField] Color initialColor;
    [SerializeField] [Range(0f, 2f)] float timePerPiece = 1f;
    [SerializeField] float distance = 100;

    [Header("Animation SFX")]
    [SerializeField] AudioClip animationKrotolamoClip;

    [Header("Others")]
    [SerializeField] [Range(1f, 5f)] float waitTime = .7f;

    [Header("FX")]
    [SerializeField] GameObject notesParticles;

    // Evento cuando se presiona el logo
    public delegate void OnLogoClick();
    public static event OnLogoClick onLogoClickDelegate;

    // Indice del instrumento a usar con las notas
    int instrumentIndex = 0;

    // Secuencia de desbloqueo
    List<int> arakataSequence = new List<int>() { 0, 0, 1, 2 };
    List<int> chorizoSequence = new List<int>() { 0, 1, 2, 0, 1, 2};
    List<int> everyoneSequence = new List<int>() { 3, 3, 4, 4, 0, 2, 0, 2, 3, 3, 4, 4, 0, 2, 0, 2, 1, 1};
    List<int> selectedSequence;
    List<int> currentSequence = new List<int>();

    // Flag para saber si deluxe esta desbloqueado
    bool deluxeActive = true;//false;

    // Personajes disponibles para ser desbloqueados
    List<string> availableCharactersToUnlock = new List<string>() { JankenUp.Characters.REFEREE, JankenUp.Characters.CHORIZA, JankenUp.Characters.BOYLEE };
    string currentCharacterToUnlock = "";

    List<Vector2> notesPositions = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        // Ver si es deluxe
        //deluxeActive = GameController.IsDeluxe();

        // Escoger uno de los personajes a desbloquear
        int currentCharacterIndex = Random.Range(0, availableCharactersToUnlock.Count);
        currentCharacterToUnlock = availableCharactersToUnlock[currentCharacterIndex];

        // Elegir instrumento en base al personaje
        switch (currentCharacterToUnlock)
        {
            case JankenUp.Characters.CHORIZA:
                instrument = aratakaSounds;
                selectedSequence = chorizoSequence;
                break;
            case JankenUp.Characters.REFEREE:
                instrument = aratakaSounds;
                selectedSequence = arakataSequence;
                break;
            default:
                instrument = everyoneSounds;
                selectedSequence = everyoneSequence;
                break;
        }

        // Asignar las funciones a cada boton
        kro.onClick.AddListener(delegate {
            if (onLogoClickDelegate != null) onLogoClickDelegate();
            kro.GetComponent<AudioSource>().PlayOneShot(instrument[0]);
            AddToSequence(0);
        });

        to.onClick.AddListener(delegate {
            if (onLogoClickDelegate != null) onLogoClickDelegate();
            to.GetComponent<AudioSource>().PlayOneShot(instrument[1]);
            AddToSequence(1);
        });

        la.onClick.AddListener(delegate {
            if (onLogoClickDelegate != null) onLogoClickDelegate();
            la.GetComponent<AudioSource>().PlayOneShot(instrument[2]);
            AddToSequence(2);
        });

        if (instrument.Count > 3)
        {
            mo.onClick.AddListener(delegate {
                if (onLogoClickDelegate != null) onLogoClickDelegate();
                mo.GetComponent<AudioSource>().PlayOneShot(instrument[3]);
                AddToSequence(3);
            });
        }

        if (instrument.Count > 4)
        {
            japanese.onClick.AddListener(delegate {
                if (onLogoClickDelegate != null) onLogoClickDelegate();
                japanese.GetComponent<AudioSource>().PlayOneShot(instrument[4]);
                AddToSequence(4);
            });
        }

        // Posiciones de notas musicales
        notesPositions.Add(kro.transform.position);
        notesPositions.Add(to.transform.position);
        notesPositions.Add(la.transform.position);
        notesPositions.Add(mo.transform.position);
        notesPositions.Add(japanese.transform.position);
    }

    // Agregar a la secuencia de teclas tocadas
    private void AddToSequence(int key)
    {
        currentSequence.Add(key);
        CheckSequence();
        CreateNotesParticles(key);
    }

    // Mostrar de manera forzada el logo
    public void Show() {

        // Ver si es deluxe
        //deluxeActive = GameController.IsDeluxe();

        // Mostrar cada parte del logo
        kro.transform.Find("Image").GetComponent<Image>().color = Color.white;
        to.transform.Find("Image").GetComponent<Image>().color = Color.white;
        la.transform.Find("Image").GetComponent<Image>().color = Color.white;
        mo.transform.Find("Image").GetComponent<Image>().color = Color.white;
        ShowLogoExtras();
    }

    // Animacion de aparicion de logo
    public IEnumerator StartAnimation() {
        int currentComplete = 0;
        GameObject target = kro.gameObject;
        Image targetImage = target.transform.Find("Image").GetComponent<Image>();
        AudioSource audioSource = GetComponent<AudioSource>();

        // Aparicion de elemento
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            targetImage.color = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> updatePosition = (t) =>
        {
            target.transform.position = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> updatePositionComplete = (t) =>
        {
            currentComplete++;
        };

        List<GameObject> ordered = new List<GameObject>() { kro.gameObject, to.gameObject, la.gameObject, mo.gameObject };

        int orderedIndex = 1;

        audioSource.PlayOneShot(animationKrotolamoClip);
        foreach (GameObject gameObject in ordered)
        {
            target = gameObject;
            targetImage = gameObject.GetComponent<Image>()? gameObject.GetComponent<Image>() : gameObject.transform.Find("Image").GetComponent<Image>();
            Vector2 startVector = new Vector2(target.transform.position.x, target.transform.position.y - distance);
            target.gameObject.Tween("FadeIn", initialColor, Color.white, timePerPiece, TweenScaleFunctions.QuadraticEaseInOut, updateColor);
            target.Tween("FadeUp", startVector, target.transform.position, timePerPiece, TweenScaleFunctions.QuadraticEaseInOut, updatePosition, updatePositionComplete);

            while (currentComplete != orderedIndex) yield return null;
            orderedIndex++;
        }
    }

    // Mostrar los elementos extras del logo
    public void ShowLogoExtras()
    {
        japaneseImage.color = Color.white;
    }

    // Revision de la secuencia actual
    private void CheckSequence()
    {
        if (currentSequence.Count > selectedSequence.Count) currentSequence.Clear();
        if (currentSequence.Count == 0) return;

        // Flag para saber si se ejecuto correctamente
        bool correct = true;
        int index = 0;

        foreach (int note in currentSequence) {
            if (note != selectedSequence[index]) {
                correct = false;
            }
            index++;
        }

        if (!correct) {
            currentSequence.Clear();
            return;
        }

        // Secuencia completa
        if (correct && currentSequence.Count == selectedSequence.Count)
        {
            if (currentCharacterToUnlock == JankenUp.Characters.BOYLEE) AlrightLetsSetEveryoneFree();
            else if (!UnlockedCharacterController.IsUnlocked(currentCharacterToUnlock)) StartCoroutine(ShowTime());
        }

    }

    // Corutina para mostrar personaje
    private IEnumerator ShowTime()
    {
        yield return new WaitForSeconds(waitTime);
        if(GooglePlayGamesController._this && currentCharacterToUnlock == Characters.REFEREE) GooglePlayGamesController._this.ReportAchievement(JankenUp.Characters.achievements.GetAchievementId(Characters.REFEREE), 100.0f, true);
        UnlockedCharacterController.NewUnlock(currentCharacterToUnlock);

        // Ir a la pantalla de desbloqueo
        SceneLoaderManager slm = FindObjectOfType<SceneLoaderManager>();
        if (slm) slm.UnlockedCharacter(SceneLoaderManager.SceneNames.MainScreen);
    }

    /// <summary>
    /// Liberacion de todos los personajes
    /// </summary>
    private void AlrightLetsSetEveryoneFree() {
        GameObject[] characters = CharacterPool.Instance.GetAll().ToArray();
        foreach (GameObject character in characters)
        {
            UnlockedCharacterController.NewUnlockNotShow((character.GetComponent<CharacterConfiguration>().GetIdentifier()));
        }
        MasterSFXPlayer._player.PlayOneShot(unlockEveryoneAudioClip);
    }

    // Generar un efecto de particulas de notas
    private void CreateNotesParticles(int key = 0)
    {
        if (key > notesPositions.Count - 1) return;
        GeneralParticle notes = Instantiate(notesParticles).GetComponent<GeneralParticle>();
        notes.transform.position = JoystickSupport.Instance.SupportActivated() ? notesPositions[key] : Camera.main.ScreenToWorldPoint(Input.mousePosition);
        notes.Play();
    }

    /// <summary>
    /// Toca la tecla Kro del logo
    /// </summary>
    public void PlayKro()
    {
        kro.onClick.Invoke();
    }

    /// <summary>
    /// Toca la tecla To del logo
    /// </summary>
    public void PlayTo()
    {
        to.onClick.Invoke();
    }

    /// <summary>
    /// Toca la tecla La del logo
    /// </summary>
    public void PlayLa()
    {
        la.onClick.Invoke();
    }

    /// <summary>
    /// Toca la tecla Mo del logo
    /// </summary>
    public void PlayMo()
    {
        mo.onClick.Invoke();
    }

    /// <summary>
    /// Toca la tecla Japanese del logo
    /// </summary>
    public void PlayJapanese()
    {
        japanese.onClick.Invoke();
    }
}
