using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalRuby.Tween;

public class Referee : MonoBehaviour
{

    [Header("Animation")]
    [SerializeField] InGameStates state = InGameStates.Stand;
    InGameStates lastState = InGameStates.Stand;

    [Header("SFX")]
    [SerializeField] AudioClip timeStep;
    [SerializeField] AudioClip jankenSFX;
    [SerializeField] AudioClip resultSFX;

    [Header("Dialogs")]
    [SerializeField] GameObject dialogBubble;

    [Header("Announcement")]
    [SerializeField] GameObject announcement;
    [SerializeField] TextMeshPro announcementText;
    [SerializeField] [Range(0, 1)] float timeAnnouncement = 1f;
    [SerializeField] [Range(0, 1)] float shakeAnnouncement = 0.2f;
    [SerializeField] [Range(0, 1)] float announcementVibrateTime = 0.2f;

    [Header("Tutorial")]
    [SerializeField] TextMeshPro tutorialText;

    // Movimineto de la burbuja de countdown
    int stepsBase = 9;
    int nextDialogDirection = 1;
    [Header("Dialog animation")]
    [SerializeField] [Range(0, 360)] float rotationBase = 30f;
    [SerializeField] [Range(0, 1)] float distance = .2f;
    [SerializeField] [Range(0, 10)] float distanceXFactorStart = 7;
    [SerializeField] [Range(0, 10)] float distanceXFactorEnd = 9;
    [SerializeField] [Range(0, 10)] float distanceYFactorStart = 8;
    [SerializeField] [Range(0, 1)] float timeToFadeIn = .3f;
    [SerializeField] [Range(0, 1)] float timeToFadeOut = .3f;
    [SerializeField] [Range(0, 2)] float timeToMove = 2f;
    [SerializeField] [Range(0, 2)] float timeToMoveWait = 1.4f;
    [SerializeField] Color colorBase = Color.white;
    [SerializeField] Color colorLast = new Color(1, 1, 1, 1);
    [SerializeField] Color colorLastBase = new Color(1, 1, 1, 0);

    [Header("FX")]
    [SerializeField] ParticleSystem dust;
    [SerializeField] ParticleSystem dustHit;
    [SerializeField] float dustHitScale = 0.8f;

    [Header("Backup")]
    [SerializeField] GameObject backup;
    [SerializeField] bool backupActive = false;

    [Header("Other")]
    [SerializeField] float timeBetweenSFX = 1f;
    float currentTimeBetweenSFX = 1f;

    // Components
    Animator animator;

    // Backup
    Animator backupAnimator;
    SpriteRenderer backupSprite;

    // Útiles
    bool playedStateSFX = false;
    int sortingOrder = 0;
    bool isDisappeared = false;
    Vector3 initialPosition;
    bool initialPositionReady = false;
    bool immuneToDisappear = false;

    // Fuerzar para hacer desaparecer
    float disappearForceX = 2000;
    float disappearForceY = 1000;
    float disappearTorque = 50;

    void Start()
    {
        animator = GetComponent<Animator>();
        sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
        backupAnimator = backup.GetComponent<Animator>();
        backupSprite = backup.GetComponent<SpriteRenderer>();

        // Modificar las capas de los anuncios del referee
        tutorialText.sortingLayerID = SortingLayer.NameToID(JankenUp.SpritesSortingLayers.Foreground);
        tutorialText.sortingOrder = announcement.GetComponent<SpriteRenderer>().sortingOrder + 1;
        announcementText.sortingLayerID = SortingLayer.NameToID(JankenUp.SpritesSortingLayers.Foreground);
        announcementText.sortingOrder = announcement.GetComponent<SpriteRenderer>().sortingOrder + 1;

        // Obtener posicion inicial
        ResetPosition();

        UpdateCurrentFont();
        LanguageController.onLanguageChangeDelegate += UpdateCurrentFont;
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        TMP_FontAsset refereeFont = FontManager._mainManager.GetRefereeFont();
        TMP_FontAsset announcementFont = FontManager._mainManager.GetAnnouncementFont();
        tutorialText.font = refereeFont;
        announcementText.font = announcementFont;
        tutorialText.fontStyle = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        announcementText.fontStyle = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
    }

    // Update is called once per frame
    void Update()
    {
        if (state != lastState)
        {
            animator.SetInteger("State", (int)state);
            if (backupActive) backupAnimator.SetInteger("State", (int)state);
            lastState = state;
            playedStateSFX = false;
        }

        // Aumentar tiempo de SFX
        currentTimeBetweenSFX += Time.deltaTime;

    }

    // Cambia el estado del referee
    public void ChangeState(InGameStates newState)
    {
        state = newState;
    }

    // Reproduce el sonido del estado
    public void PlaySFX()
    {
        if (playedStateSFX || currentTimeBetweenSFX < timeBetweenSFX) return;

        playedStateSFX = true;
        currentTimeBetweenSFX = 0;

        if (MasterRefereeSFXPlayer._player == null) return;

        switch (state)
        {
            case InGameStates.Stand:
                MasterRefereeSFXPlayer._player.PlayOneShot(timeStep);
                playedStateSFX = false;
                break;
            case InGameStates.Attack:
                MasterRefereeSFXPlayer._player.PlayOneShot(jankenSFX);
                break;
            case InGameStates.Lose:
            case InGameStates.Win:
            case InGameStates.Draw:
                MasterRefereeSFXPlayer._player.PlayOneShot(resultSFX);
                break;

        }
    }

    // Transición en mostrar el indicador de tiempo
    public void Step(int step)
    {
        StartCoroutine(ShowStep(step));
    }

    // Muestra dialogo de tiempo animado
    private IEnumerator ShowStep(int step)
    {
        // Crear la copia y animar con Tween. Debe desaparecer al final
        GameObject copyDialog = Instantiate(dialogBubble, dialogBubble.transform.position, Quaternion.identity);
        copyDialog.transform.parent = dialogBubble.transform.parent;
        copyDialog.transform.localScale = new Vector3(1, 1, 1);
        copyDialog.SetActive(true);

        // Render de la imagen
        SpriteRenderer copyRenderer = copyDialog.GetComponent<SpriteRenderer>();
        copyRenderer.sortingOrder = copyRenderer.sortingOrder + stepsBase - step;

        // Texto a cambiar
        TextMeshPro copyText = copyDialog.transform.Find("Text").GetComponent<TextMeshPro>();
        copyText.sortingLayerID = SortingLayer.NameToID(JankenUp.SpritesSortingLayers.Arena);
        copyText.sortingOrder = copyRenderer.sortingOrder + 1;
        copyText.text = step.ToString();

        // Obtener la orientacion del personaje y cambiar los valores si es negativo
        float orientation = 0;
        float rotation = 0;
        switch (nextDialogDirection)
        {
            case 1:
                orientation = 1;
                rotation = -rotationBase;
                nextDialogDirection = -1;
                break;
            case -1:
                orientation = -1;
                rotation = rotationBase;
                nextDialogDirection = 0;
                break;
            case 0:
                nextDialogDirection = 1;
                break;
        }

        copyDialog.transform.Rotate(0, 0, rotation);

        System.Action<ITween<Color>> fadeGlobe = (t) =>
        {
            if(copyRenderer != null) copyRenderer.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> fadeText = (t) =>
        {
            if (copyText != null)  copyText.color = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> moveIn = (t) =>
        {
            if(copyDialog != null) copyDialog.transform.position = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> moveInComplete = (t) =>
        {
            Destroy(copyDialog);
        };
        
        Color initialColorGlobe = step > 1 ? new Color(1, 1, 1, 0) : colorLastBase;
        Color initialColorText = new Color(1, 1, 1, 0);
        Color endColorGlobe = step > 1 ? colorBase : colorLast;
        Color endColorText = Color.white;

        float xStartDelta = distance * orientation * distanceXFactorStart;
        float xEndDelta = distance * orientation * distanceXFactorEnd;
        float yStartDelta = distance * (Mathf.Abs(orientation) == 1 ? distanceYFactorStart : 1);
        float yEndDelta = distance * (distanceYFactorStart / 2);

        Vector2 startVector = new Vector2(copyDialog.transform.position.x + xStartDelta, copyDialog.transform.position.y - yStartDelta);
        Vector2 endVector = new Vector2(copyDialog.transform.position.x + xEndDelta, copyDialog.transform.position.y + yEndDelta);

        // Que ataque aparezca
        copyDialog.Tween(string.Format("FadeIn{0}", copyDialog.GetInstanceID()), initialColorGlobe, endColorGlobe,
            timeToFadeIn, TweenScaleFunctions.QuadraticEaseInOut, fadeGlobe);

        copyDialog.Tween(string.Format("FadeIn{0}", copyText.GetInstanceID()), initialColorText, endColorText,
            timeToFadeIn, TweenScaleFunctions.QuadraticEaseInOut, fadeText);

        copyDialog.Tween(string.Format("Move{0}", copyDialog.GetInstanceID()), startVector, endVector,
            timeToMove, TweenScaleFunctions.QuadraticEaseInOut, moveIn, moveInComplete);

        yield return new WaitForSeconds(timeToMoveWait);

        // Que ataque desaparezca
        copyDialog.Tween(string.Format("FadeOut{0}", copyDialog.GetInstanceID()), endColorGlobe, initialColorGlobe,
            timeToFadeOut, TweenScaleFunctions.QuadraticEaseInOut, fadeGlobe);

        copyDialog.Tween(string.Format("FadeIn{0}", copyText.GetInstanceID()), endColorText, initialColorText,
            timeToFadeOut, TweenScaleFunctions.QuadraticEaseInOut, fadeText);

    }
    public void ShowDialog(bool show)
    {
        //dialogBubble.SetActive(show);
    }

    // Co-rutina para mostrar el mensaje de triple shot
    public IEnumerator Announcement(string text, object[] args = null)
    {
        yield return StartCoroutine(Announcement(text, 1, args));
    }

        // Co-rutina para mostrar el mensaje de triple shot
    public IEnumerator Announcement(string text, int speed, object[] args = null) {

        // Si la velocidad es menor a 1, dejar en 1
        if (speed < 1) speed = 1;

        // Habilitar el mensaje de triple shot
        announcement.SetActive(true);
        if (args != null)
        {
            LocalizationHelper.FormatTranslate(announcementText, JankenUp.Localization.tables.InGame.tableName, text, args);
        }
        else
        {
            LocalizationHelper.Translate(announcementText, JankenUp.Localization.tables.InGame.tableName, text);
        }

        // Indicar al shake que debe moverlo por X tiempo
        Shake shake = FindObjectOfType<Shake>();
        if (shake) shake.ShakeThis(announcement, timeAnnouncement, shakeAnnouncement);
        VibrationController.Instance.Vibrate();

        // Reproducir sonido de silvato
        PlaySFX();

        yield return new WaitForSeconds(timeAnnouncement / speed);

        // Deshabilitar el triple show
        announcement.SetActive(false);

    }

    // Vibracion del dispositivo movil para hacer incapie en anuncio
    private IEnumerator Vibrate()
    {
        int steps = 3;
        while(steps > 0)
        {
            VibrationController.Instance.Vibrate();
            yield return new WaitForSeconds(announcementVibrateTime);
            steps--;
        }
    }

    // Ocultar dialogo de triple shoot
    public void HideAnnouncement() {
        announcement.SetActive(false);
    }

    // Efecto de particulas al entrar en colision
    private void OnCollisionEnter2D(Collision2D collision)
    {

        Rigidbody2D rigidBody = GetComponent<Rigidbody2D>();

        GameObject dustObject = Instantiate(dust.gameObject, transform.position, Quaternion.identity) as GameObject;
        DustParticle dustCopy = dustObject.GetComponent<DustParticle>();

        // Aplicar variaciones segun la velocidad del cuerpo
        dustCopy.SetSpeed(rigidBody.velocity.y);
        dustCopy.SetSorting(sortingOrder);
        dustCopy.Play();
    }

    /// <summary>
    /// Desaparicion del referee. Si se aplica fuerza, se vera como sale volando (Util para camaras lentas)
    /// </summary>
    /// <param name="xScale"></param>
    /// <param name="applyForce"></param>
    public void PlayDisappear(float xScale, bool applyForce = false, float customDisappearForceX = 0)
    {
        if (isDisappeared || immuneToDisappear) return;
        GameObject dustObject = Instantiate(dustHit.gameObject, transform.position, Quaternion.identity) as GameObject;
        DustParticle dustCopy = dustObject.GetComponent<DustParticle>();

        // Aplicar variaciones segun la velocidad del cuerpo
        dustCopy.SetXScaleMultiplier((int) (xScale * dustHitScale));

        dustObject.transform.localScale = new Vector3(
            xScale * dustHitScale,
            transform.localScale.y * dustHitScale,
            transform.localScale.z
            );

        dustCopy.Play();

        if (applyForce)
        {
            Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
            rigidbody2D.AddForce(new Vector2(xScale * -1 * ( customDisappearForceX != 0? customDisappearForceX : disappearForceX), disappearForceY));
            rigidbody2D.AddTorque(disappearTorque * xScale);
            Disappear(false);
        }
        else
        {
            Disappear();
        }
    }

    /// <summary>
    /// Metodo para simular la desaparicion del personaje
    /// </summary>
    public void Disappear(bool setActiveFalse = true)
    {
        HideAnnouncement();
        if (immuneToDisappear) return;
        isDisappeared = true;
        if(setActiveFalse) gameObject.SetActive(false);
    }

    // Volver al mostrar al personaje
    public void Reappear()
    {
        isDisappeared = false;
        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Static;
        ResetPosition();
        rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        gameObject.SetActive(true);
    }

    // Cambiar el estado del backup
    public void ToggleBackup(bool active)
    {
        backupActive = active;
        if(backupSprite == null) backupSprite = backup.GetComponent<SpriteRenderer>();
        backupSprite.color = active ? Color.white : Color.clear;

        if (backupActive)
        {
            if(animator == null) animator = GetComponent<Animator>();
            if(backupAnimator == null) backupAnimator = backup.GetComponent<Animator>();
            animator.SetInteger("State", (int)state);
            backupAnimator.SetInteger("State", (int)state);
        }
            
    }

    // Volver a la posicion original
    public void ResetPosition()
    {
        if (!initialPositionReady)
        {
            initialPosition = transform.localPosition;
            initialPositionReady = true;
        }
        transform.localPosition = initialPosition;
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    /// <summary>
    /// Cambiar la posibilidad de desaparecer
    /// </summary>
    /// <param name="value"></param>
    public void SetImmuneToDisappear(bool value) {
        immuneToDisappear = value;
    }

    /// <summary>
    /// Obtener la posibilidad de desaparecer
    /// </summary>
    public bool GetImmuneToDisappear()
    {
        return immuneToDisappear;
    }

    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= UpdateCurrentFont;
    }

}
