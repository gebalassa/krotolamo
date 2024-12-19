using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttackOptions : MonoBehaviour
{
    // Llave para guardar y obtener la preferencia de control del jugador
    public static string CONTROLPOSITIONKEY = "ControlPosition";
    public static int CONTROLPOSITIONKEYDEFAULT = 0;

    [Header("Y Positions")]
    [SerializeField] float[] positions;

    [Header("Container")]
    [SerializeField] RectTransform attacksContainer;
    [SerializeField] RectTransform superAttacksContainer;

    [Header("Attacks")]
    [SerializeField] Button rockButton;
    [SerializeField] Button paperButton;
    [SerializeField] Button scissorsButton;

    [Header("AttackContainers")]
    [SerializeField] Transform topAttackContainer;
    [SerializeField] Transform middleAttackContainer;
    [SerializeField] Transform bottomAttackContainer;
    List<Transform> attacksButtonsContainer = new List<Transform>();

    [Header("SuperAttacks")]
    [SerializeField] Button timeMasterButton;
    [SerializeField] Button magicWandButton;
    [SerializeField] Button janKenUpButton;
    [SerializeField] TextMeshProUGUI timeMasterLabel;
    [SerializeField] TextMeshProUGUI magicWandLabel;
    [SerializeField] TextMeshProUGUI janKenUpLabel;
    [SerializeField] AudioClip spendSuperPowerFail;
    bool isSuperAttackExecuting = false;
    bool isTimeMasterExecuting = false;
    bool activeSuperPowers = true;
    bool canUseSuperPowers = false;

    [Header("SuperAttacksEnabled")]
    [SerializeField] bool timeMasterEnable = true;
    [SerializeField] bool magicWandEnable = true;
    [SerializeField] bool janKenUpEnable = true;

    // Componentes frecuentes
    AudioSource audioSource;
    CanvasGroup timeMasterCanvasGroup;
    CanvasGroup magicWandCanvasGroup;
    CanvasGroup jankenupCanvasGroup;

    // Variables especificas para TimeMaster
    FloatTween desacelerationTween;
    FloatTween acelerationTween;

    // Colores para los labels de superataques
    Color availableColor = Color.white;
    Color unavailableColor = new Color(1, 1, 1, .5f);

    // Sesion de juego
    JankenSession jankenSession;
    SingleModeSession singleModeSession;

    // Eventos
    public delegate void OnAttackSelect(int attack);
    public static event OnAttackSelect onAttackSelectDelegate;
    public delegate void OnTimeMaster();
    public static event OnTimeMaster onTimeMasterDelegate;
    public delegate void OnMagicWand(int attack);
    public static event OnMagicWand onMagicWandDelegate;
    public delegate void OnSuper();
    public static event OnSuper onSuperDelegate;

    /// <summary>
    /// Al comenzar, cambiar la ubicacion segun configuracion del usuario
    /// </summary>
    private void Start()
    {
        // Conseguir la preferencia del usuario. 0 izquierda 1 seria derecha
        int attacksPosition = PlayerPrefs.GetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, PlayersPreferencesController.CONTROLPOSITIONKEYDEFAULT);

        attacksContainer.anchoredPosition = new Vector2(attacksContainer.anchoredPosition.x * (attacksPosition == 0 ? 1 : -1), attacksContainer.anchoredPosition.y);
        attacksContainer.pivot = new Vector2(attacksPosition == 0 ? 0 : 1, .5f);
        attacksContainer.anchorMin = new Vector2(attacksPosition, 0.5f);
        attacksContainer.anchorMax = new Vector2(attacksPosition, 0.5f);

        superAttacksContainer.anchoredPosition = new Vector2(superAttacksContainer.anchoredPosition.x * (attacksPosition == 0 ? 1 : -1), superAttacksContainer.anchoredPosition.y);
        superAttacksContainer.pivot = new Vector2(attacksPosition == 0 ? 0 : 1, .5f);
        superAttacksContainer.anchorMin = new Vector2(attacksPosition == 0 ? 1 : 0, 0.5f);
        superAttacksContainer.anchorMax = new Vector2(attacksPosition == 0 ? 1 : 0, 0.5f);

        // Obtener sesion de juego
        jankenSession = FindObjectOfType<JankenSession>();
        singleModeSession = FindObjectOfType<SingleModeSession>();

        // Obtener el audiosource asociado
        audioSource = GetComponent<AudioSource>();

        // Actualizar todas las opciones
        UpdateUI();

        // Eventos de seleccion de ataque
        rockButton.onClick.AddListener(delegate { if (onAttackSelectDelegate != null) onAttackSelectDelegate((int) Attacks.Rock); });
        paperButton.onClick.AddListener(delegate { if (onAttackSelectDelegate != null) onAttackSelectDelegate((int) Attacks.Paper); });
        scissorsButton.onClick.AddListener(delegate { if (onAttackSelectDelegate != null) onAttackSelectDelegate((int) Attacks.Scissors); });

        timeMasterButton.onClick.AddListener(delegate { TimeMaster(); });
        magicWandButton.onClick.AddListener(delegate { MagicWandClick(); });
        janKenUpButton.onClick.AddListener(delegate { JanKenUp(); });
    }

    /// <summary>
    /// Actualizacion de la UI de los ataques y superataques
    /// </summary>
    public void UpdateUI()
    {
        // Revisar si es posible o no usar superpoderes
        if (canUseSuperPowers)
        {
            // Activar los elementos
            timeMasterButton.gameObject.SetActive(true && timeMasterEnable);
            magicWandButton.gameObject.SetActive(true && magicWandEnable);
            janKenUpButton.gameObject.SetActive(true && janKenUpEnable);

            // Obtener los poderes actuales
            int countTimeMaster = singleModeSession ? singleModeSession.GetTimeMaster() : (jankenSession ? jankenSession.GetTimeMaster() : 0);
            int countMagicWand = singleModeSession ? singleModeSession.GetMagicWand() : (jankenSession ? jankenSession.GetMagicWand() : 0);
            int countJanKenUp = singleModeSession ? singleModeSession.GetJanKenUp() : (jankenSession ? jankenSession.GetJanKenUp() : 0);

            bool timeMasterAvailable = activeSuperPowers && countTimeMaster > 0;
            bool magicWandAvailable = activeSuperPowers && countMagicWand > 0;
            bool janKenUpAvailable = activeSuperPowers && countJanKenUp > 0;

            // Actualizar la cantidad de monedas de cada elemento
            timeMasterLabel.text = string.Format("x{0}", countTimeMaster);
            magicWandLabel.text = string.Format("x{0}", countMagicWand);
            janKenUpLabel.text = string.Format("x{0}", countJanKenUp);

            timeMasterLabel.color = timeMasterAvailable ? availableColor : unavailableColor;
            magicWandLabel.color = magicWandAvailable ? availableColor : unavailableColor;
            janKenUpLabel.color = janKenUpAvailable ? availableColor : unavailableColor;

            // Habilitar botones
            timeMasterButton.interactable = timeMasterAvailable;
            magicWandButton.interactable = magicWandAvailable;
            janKenUpButton.interactable = janKenUpAvailable;
            UpdateSuperAttacksCanvasGroup();
        }
        else
        {
            timeMasterButton.gameObject.SetActive(false);
            magicWandButton.gameObject.SetActive(false);
            janKenUpButton.gameObject.SetActive(false);
        }

    }

    /// <summary>
    /// Actualizacion de los CanvasGroups de los superatauqes
    /// </summary>
    private void UpdateSuperAttacksCanvasGroup()
    {
        if (!jankenupCanvasGroup) jankenupCanvasGroup = janKenUpButton.GetComponent<CanvasGroup>();
        if (!magicWandCanvasGroup) magicWandCanvasGroup = magicWandButton.GetComponent<CanvasGroup>();
        if (!timeMasterCanvasGroup) timeMasterCanvasGroup = timeMasterButton.GetComponent<CanvasGroup>();

        if (jankenupCanvasGroup) jankenupCanvasGroup.alpha = janKenUpButton.interactable ? 1 : .5f;
        if (magicWandCanvasGroup) magicWandCanvasGroup.alpha = magicWandButton.interactable ? 1 : .5f;
        if (timeMasterCanvasGroup) timeMasterCanvasGroup.alpha = timeMasterButton.interactable ? 1 : .5f;
    }

    /// <summary>
    /// Habilitar al jugador a usar los superpoderes
    /// </summary>
    public void ReadyToUseSuperPowers()
    {
        canUseSuperPowers = true;
    }

    /// <summary>
    /// Oculta/Muestra los ataques normales
    /// </summary>
    /// <param name="show"></param>
    public void ToggleNormalAttacks(bool show)
    {
        attacksContainer.gameObject.SetActive(show);
    }

    /// <summary>
    /// Desordenar las opciones
    /// </summary>
    public void Shuffle()
    {
        // Desordenar los elementos y cambiar de contenedor
        CheckAttackContainers();
        int count = attacksButtonsContainer.Count;
        int last = count - 1;
        for (int i = 0; i < last; ++i){
            int r = Random.Range(i, count);
            Transform tmp = attacksButtonsContainer[i];
            attacksButtonsContainer[i] = attacksButtonsContainer[r];
            attacksButtonsContainer[r] = tmp;
        }

        rockButton.transform.SetParent(attacksButtonsContainer[0]);
        rockButton.transform.SetAsFirstSibling();
        paperButton.transform.SetParent(attacksButtonsContainer[1]);
        paperButton.transform.SetAsFirstSibling();
        scissorsButton.transform.SetParent(attacksButtonsContainer[2]);
        scissorsButton.transform.SetAsFirstSibling();
    }

    /// <summary>
    /// Se deshabilita uno de los ataques
    /// </summary>
    public void DisableAttacks()
    {
        // Dejar uno de los ataques deshabilitado
        CheckAttackContainers();
        int selectedIndex = Random.Range(0, attacksButtonsContainer.Count);
        int currentIndex = 0;
        foreach (Transform t in attacksButtonsContainer)
        {
            CanvasGroup canvasGroup = t.GetComponent<CanvasGroup>();
            if (currentIndex == selectedIndex)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
            }
            else
            {
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
            }
            currentIndex++;
        }

    }

    /// <summary>
    /// Revision de carga de listado de contenedores de ataque
    /// </summary>
    private void CheckAttackContainers()
    {
        if(attacksButtonsContainer.Count == 0)
        {
            attacksButtonsContainer.Add(topAttackContainer);
            attacksButtonsContainer.Add(middleAttackContainer);
            attacksButtonsContainer.Add(bottomAttackContainer);
        }
    }

    /// <summary>
    /// Habilitar todos los ataques
    /// </summary>
    public void EnableAttacks()
    {
        foreach (Transform t in attacksContainer.transform)
        {
            t.gameObject.SetActive(true);
        }

    }

    /// <summary>
    /// Desactivas los superataques
    /// </summary>
    private void InactiveSuperAttacks() {
        activeSuperPowers = false;
        UpdateUI();
    }

    /// <summary>
    /// Resetear la posibilidad de usar superataques
    /// </summary>
    public void Reset()
    {
        activeSuperPowers = true;
        UpdateUI();
    }

    /// <summary>
    /// Click en el poder TimeMaster
    /// </summary>
    public void TimeMaster()
    {
        if (!canUseSuperPowers || !activeSuperPowers || isSuperAttackExecuting || !TransitionDoors._this.IsTotallyOpen()) return;

        // Ver si es posible utilizar el TimeMaster
        if ((singleModeSession && singleModeSession.SpendTimeMaster()) || (jankenSession && jankenSession.SpendTimeMaster()))
        {
            MasterSFXPlayer._player.SelectAttack();

            isSuperAttackExecuting = true;
            isTimeMasterExecuting = true;

            // Hacer que superataques ya no puedan ser usados por la partida
            InactiveSuperAttacks();

            StartCoroutine(ExecuteTimeMaster());
        }
        else
        {
            // Reproducir sonido de erroneo
            audioSource.PlayOneShot(spendSuperPowerFail);
        }

    }

    /// <summary>
    /// Ejecucion del poder TimeMaster, que permite reducir el tiempo del juego por un periodo corto
    /// </summary>
    /// <returns></returns>
    public IEnumerator ExecuteTimeMaster()
    {
        // Estas variables deben estar como slider para probar!!
        float decelerationTime = .5f;
        float waitingTime = .1f;
        float acelerationTime = .2f;
        float minAceleration = .2f;

        bool decelerationComplete = false;
        bool acelerationComplete = false;

        // Tween para la aumentar y disminuir el tiempo
        System.Action<ITween<float>> updateTimeAndPitch = (t) =>
        {
            Time.timeScale = t.CurrentValue;
            MasterAudioPlayer._player.ChangePitchAudio(t.CurrentValue);
        };

        System.Action<ITween<float>> completeDeceleration = (t) =>
        {
            decelerationComplete = true;
        };

        System.Action<ITween<float>> completeAceleration = (t) =>
        {
            acelerationComplete = true;
        };

        // Ejecutar la desaceleracion
        desacelerationTween = gameObject.Tween(string.Format("Deceleration{0}", GetInstanceID()), 1, minAceleration, decelerationTime, TweenScaleFunctions.QuadraticEaseOut, updateTimeAndPitch, completeDeceleration);

        while (!decelerationComplete) yield return null;
        desacelerationTween = null;

        // Esperar un momento
        yield return new WaitForSeconds(waitingTime);

        // Ejecutar la aceleracion
        acelerationTween = gameObject.Tween(string.Format("Aceleration{0}", GetInstanceID()), minAceleration, 1, acelerationTime, TweenScaleFunctions.QuadraticEaseIn, updateTimeAndPitch, completeAceleration);

        while (!acelerationComplete) yield return null;
        acelerationTween = null;

        // Volver a permitir que se ejecuten superataques
        isSuperAttackExecuting = false;
        isTimeMasterExecuting = false;

    }

    /// <summary>
    /// Metodo para cancelar el superpoder de TimeMaster si se encuentra en ejecucion
    /// </summary>
    public void StopTimeMaster()
    {
        if (!isTimeMasterExecuting) return;

        float minAceleration = 0.2f;
        float acelerationTime = .2f;
        float currentAceleraion = minAceleration;

        // Si esta en etapa de desaceleracion
        if (desacelerationTween != null)
        {
            minAceleration = desacelerationTween.CurrentValue;
            desacelerationTween.Stop(TweenStopBehavior.DoNotModify);
            desacelerationTween = null;
        }
        else if (acelerationTween != null)
        {
            minAceleration = acelerationTween.CurrentValue;
            acelerationTween.Stop(TweenStopBehavior.DoNotModify);
            acelerationTween = null;
        }

        // Tween para la aumentar y disminuir el tiempo
        System.Action<ITween<float>> updateTimeAndPitch = (t) =>
        {
            Time.timeScale = t.CurrentValue;
            MasterAudioPlayer._player.ChangePitchAudio(t.CurrentValue);
        };

        // Quitar la ejecucion de los ataques
        System.Action<ITween<float>> complete = (t) =>
        {
            isSuperAttackExecuting = false;
            isTimeMasterExecuting = false;
        };

        // Hacer el subidon de velocidad
        gameObject.Tween(string.Format("Aceleration{0}", GetInstanceID()), currentAceleraion, 1, acelerationTime, TweenScaleFunctions.QuadraticEaseIn, updateTimeAndPitch, complete);

    }

    /// <summary>
    /// Click sobre el boton de varita magica
    /// </summary>
    public void MagicWandClick()
    {
        if (!canUseSuperPowers) return;
        if (onMagicWandDelegate != null) onMagicWandDelegate((int)Attacks.MagicWand);
    }

    /// <summary>
    /// Click en el poder MagicWand
    /// </summary>
    /// <returns></returns>
    public bool MagicWand()
    {
        if (!activeSuperPowers || isSuperAttackExecuting || !TransitionDoors._this.IsTotallyOpen() || (singleModeSession && singleModeSession.GetIsShowingResults())) return false;

        // Ver si es posible utilizar el MagicWand
        if ((singleModeSession && singleModeSession.SpendMagicWand()) || (jankenSession && jankenSession.SpendMagicWand()))
        {
            // Hacer que superataques ya no puedan ser usados por la partida
            InactiveSuperAttacks();
            return true;
        }
        else
        {
            // Reproducir sonido de erroneo
            audioSource.PlayOneShot(spendSuperPowerFail);
            return false;
        }

    }

    /// <summary>
    /// Click en el poder JanKenUP
    /// </summary>
    public void JanKenUp()
    {
        if (!canUseSuperPowers || !activeSuperPowers || isSuperAttackExecuting || !TransitionDoors._this.IsTotallyOpen() || (singleModeSession && singleModeSession.GetIsShowingResults())) return;

        // Ver si es posible utilizar el JanKenUp
        if ((singleModeSession && singleModeSession.SpendJanKenUp()) || (jankenSession && jankenSession.SpendJanKenUp()))
        {
            MasterSFXPlayer._player.SelectAttack();

            // Hacer que superataques ya no puedan ser usados por la partida
            InactiveSuperAttacks();

            isSuperAttackExecuting = true;

            // Notificar de ejecucion efectiva
            if (onSuperDelegate != null) onSuperDelegate();
        }
        else
        {
            // Reproducir sonido de erroneo
            audioSource.PlayOneShot(spendSuperPowerFail);
        }

    }

    /// <summary>
    /// Revisar si se esta ejecutando un superpoder
    /// </summary>
    /// <returns></returns>
    public bool IsSuperAttackExecuting()
    {
        return isSuperAttackExecuting;
    }

    /// <summary>
    /// Indica que un superataque fue ejecutado
    /// </summary>
    public void SuperAttackExecuted()
    {
        isSuperAttackExecuting = false;
    }

    /// <summary>
    /// Seleccion de ataque segun posicion en el stack de ataques
    /// </summary>
    /// <param name="position"></param>
    public void AttackByPosition(int position)
    {
        if (!attacksContainer.gameObject.activeSelf) return;
        Button attackButton = null;
        switch (position)
        {
            case 0:
                attackButton = topAttackContainer.GetComponent<CanvasGroup>().interactable? topAttackContainer.GetComponentInChildren<Button>() : null;
                break;
            case 1:
                attackButton = middleAttackContainer.GetComponent<CanvasGroup>().interactable ? middleAttackContainer.GetComponentInChildren<Button>() : null;
                break;
            case 2:
                attackButton = bottomAttackContainer.GetComponent<CanvasGroup>().interactable ? bottomAttackContainer.GetComponentInChildren<Button>() : null;
                break;
        }
        if (attackButton != null) attackButton.onClick.Invoke();
    }

    /// <summary>
    /// Obtener el nombre del ataque en el contenedor superior
    /// </summary>
    /// <returns></returns>
    public string GetTopAttackName()
    {
        return topAttackContainer.GetComponentInChildren<Button>()? topAttackContainer.GetComponentInChildren<Button>().name.ToLower() : "";
    }

    /// <summary>
    /// Obtener el nombre del ataque en el contenedor del medio
    /// </summary>
    /// <returns></returns>
    public string GetMiddleAttackName()
    {
        return middleAttackContainer.GetComponentInChildren<Button>() ? middleAttackContainer.GetComponentInChildren<Button>().name.ToLower() : "";
    }

    /// <summary>
    /// Obtener el nombre del ataque en el contenedor inferior
    /// </summary>
    /// <returns></returns>
    public string GetBottomAttackName()
    {
        return bottomAttackContainer.GetComponentInChildren<Button>() ? bottomAttackContainer.GetComponentInChildren<Button>().name.ToLower() : "";
    }
}
