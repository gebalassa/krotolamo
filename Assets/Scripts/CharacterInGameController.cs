using DigitalRuby.Tween;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterInGameController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] bool control = false;

    [Header("Attacks")]
    [SerializeField] protected Attacks currentAttack = Attacks.Rock;

    [Header("Sprites")]
    [SerializeField] Sprite guuSpr;
    [SerializeField] Sprite paaSpr;
    [SerializeField] Sprite chookiSpr;
    [SerializeField] Sprite magicWandSpr;

    [Header("Animation")]
    [SerializeField] protected InGameStates state = InGameStates.Stand;
    InGameStates lastState = InGameStates.Stand;

    [Header("Illuminati")]
    [SerializeField] bool illuminati = false;
    [SerializeField] Sprite illuminatiSpr;

    [Header("Preattack")]
    [SerializeField] float deltaInvertedPosition = 0.4f;

    [Header("Superpowers")]
    [SerializeField] Sprite timeMaster;
    [SerializeField] Sprite magicWand;
    [SerializeField] Sprite janKenUp;

    [Header("Get Superpowers Animation")]
    [SerializeField] GameObject superAttackPrefab;
    [SerializeField] GameObject lifePrefab;
    [SerializeField] Vector3 superAttackPosition;
    [SerializeField] [Range(0, 1)] float superAttackDistance = .2f;
    [SerializeField] [Range(0, 10)] float superAttackDistanceYFactorStart = 8;
    [SerializeField] [Range(0, 1)] float superAttackTimeToFadeIn = .3f;
    [SerializeField] [Range(0, 1)] float superAttackTimeToFadeOut = .3f;
    [SerializeField] [Range(0, 2)] float superAttackTimeToMove = 2f;
    [SerializeField] [Range(0, 2)] float superAttackTimeToMoveWait = 1.4f;
    [SerializeField] Color colorBase = Color.white;

    [Header("FX")]
    [SerializeField] ParticleSystem dust;
    [SerializeField] ParticleSystem dustHit;
    [SerializeField] int sortingOrder = 0;

    [Header("SuperJanKenUp")]
    [SerializeField] protected Vector2 superJanKenUpFXPosition;
    [SerializeField] protected Color superJanKenUpColor = Color.white;
    [SerializeField] protected GameObject superJanKenUPPrefab;
    protected bool superJanKenUpActive = false;

    // Mantencion de la instancia relacionada al super
    protected SuperJanKenUPBase superAttack;

    [Header("Attack Sprites")]
    [SerializeField] protected GameObject guuSpriteRenderer;
    [SerializeField] protected GameObject paaSpriteRenderer;
    [SerializeField] protected GameObject chokiSpriteRenderer;
    [SerializeField] protected GameObject magicWandSpriteRenderer;

    [Header("Stand Sprites")]
    [SerializeField] SpriteRenderer[] standRenderers;

    [Header("SFX Options")]
    [SerializeField] bool onLosePlayInMaster = false;
    [SerializeField] bool onWinPlayInMaster = false;

    [Header("Support NPC")]
    [SerializeField] JankenUp.NPC.Identifiers supportNPCIdentifier;
    SupportCrowd supportNPC;

    [Header("Feint")]
    [SerializeField] GameObject feintControllerPrefab;
    [SerializeField] GameObject feintAttackPrefab;
    [SerializeField] Vector2 feintPosition = new Vector2();
    FeintController feintController;
    FeintAttack feintAttack;

    [Header("Emote")]
    [SerializeField] Emote emotePrefab;
    [SerializeField] Vector2 emotePosition;

    [SerializeField] Material lockMaterial;
    [SerializeField] Material unlockMaterial;

    [Header("Color")]
    [SerializeField] Color mainColor;


    // Movimiento de ataque
    float distance = .1f;
    float timeToFade = .5f;
    float timeToMove = 2f;
    float timeToMoveWait = 1f;
    float attackTorque = 10f;

    // Components
    protected Animator animator;
    protected AudioSource audioSource;
    AudioSourceSFX audioSourceFX;
    protected CharacterConfiguration configuration;
    protected SpriteRenderer bodyRenderer;
    Rigidbody2D mainRigidbody;

    // Sprite de ataque
    GameObject attackDisplayer;
    GameObject preAttackDisplayer;
    SpriteRenderer attackSprite;
    SpriteRenderer preAttackSprite;

    // Carga de ataque
    ChangeAttackFillBar changeAttackFillBar;

    // Utiles
    protected bool playedStateSFX = false;
    protected bool isDisappeared = false;
    protected bool isFlipped = false;

    // Gravedad para el ataque, util para MagicWand
    bool attackHasGravity = false;

    // Al chocar con la plataforma, puede alterar sus componentes de Rigidboy
    bool alterRigidBobyOnCollision = false;
    bool rigidBodyAlterPropertiesReady = false;
    PhysicsMaterial2D onCollisionMaterial;
    float onCollisionGravity;
    float changeAfterSeconds = .2f;
    float changeAfterSecondsCurrent = 0;

    // Finta para modo online
    bool showFeint = false;

    // Acceso directo a los sprite renderer
    protected SpriteRenderer rockSpriteRendererTarget;
    protected SpriteRenderer paperSpriteRendererTarget;
    protected SpriteRenderer scissorsSpriteRendererTarget;
    protected SpriteRenderer magicWandSpriteRendererTarget;

    // Oponente actual
    protected string opponentIdentifier;

    // Corutinas
    Coroutine showSuperPowerCoroutine;

    // CanRecieveHitsByObjects
    float canRecieveHitsByObjectsMinVelocity = -1;
    float canRecieveHitsByObjectsMaxVelocity = 1;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioSourceFX = GetComponent<AudioSourceSFX>();
        configuration = GetComponent<CharacterConfiguration>();
        mainRigidbody = GetComponent<Rigidbody2D>();

        // Obtencion de los sprite renderer
        rockSpriteRendererTarget = guuSpriteRenderer? guuSpriteRenderer.GetComponent<SpriteRenderer>() : null;
        paperSpriteRendererTarget = paaSpriteRenderer? paaSpriteRenderer.GetComponent<SpriteRenderer>() : null;
        scissorsSpriteRendererTarget = chokiSpriteRenderer? chokiSpriteRenderer.GetComponent<SpriteRenderer>() : null;
        magicWandSpriteRendererTarget = magicWandSpriteRendererTarget? magicWandSpriteRendererTarget.GetComponent<SpriteRenderer>() : null;

        // Obtener los displayer de ataque y preataque
        GetAttackDisplayer();

        preAttackDisplayer = transform.Find("PreAttack").gameObject;
        preAttackSprite = preAttackDisplayer.GetComponent<SpriteRenderer>();

        // Guardar el sortingOrder
        CheckSpriteRendered();
        if(sortingOrder == 0) sortingOrder = bodyRenderer.sortingOrder + 1;

        // Esconder el preataque si esta activada la opción illuminati
        if (illuminati)
        {
            preAttackSprite.sprite = illuminatiSpr;
        }

        // Girar el preataque segun preferencia de usuario
        FlipPreAttack();

        // No mostrar los ataques
        ShowPreAttack(false);

        // Prepararse para ignorar los objetos proyectiles que ya estan en la arena
        IgnoreSpecialObjectProjectile();
    }

    /// <summary>
    /// Ignorar los objetos de projectiles que ya estan creados
    /// </summary>
    private void IgnoreSpecialObjectProjectile()
    {
        Collider2D characterColider = GetComponent<Collider2D>();
        if (characterColider)
        {
            foreach(SpecialObjectProjectile specialObjectProjectile in SpecialObjectProjectile.specialObjectInstances)
            {
                Collider2D specialObjectCollider = specialObjectProjectile.GetComponent<Collider2D>();
                if(specialObjectCollider) Physics2D.IgnoreCollision(characterColider, specialObjectCollider);
            }
        }
    }

    // Cambia la orientacipn del preataque
    private void FlipPreAttack() {

        int position = PlayerPrefs.GetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, PlayersPreferencesController.CONTROLPOSITIONKEYDEFAULT);
        Transform rect = preAttackDisplayer.GetComponent<Transform>();

        float newX = rect.localPosition.x * (position == 0 ? 1 : -1) + (position == 0 ? 0 : deltaInvertedPosition);

        rect.localPosition = new Vector2(newX, rect.localPosition.y);
        rect.localScale = new Vector2(rect.localScale.x * (position == 0 ? 1 : -1), rect.localScale.y);

    }

    // Update is called once per frame
    void Update()
    {
        if (state != lastState)
        {
            animator.SetInteger("State", (int)state);
            lastState = state;
            playedStateSFX = false;
        }

        // Ver si alterar el rigidBody
        if (rigidBodyAlterPropertiesReady)
        {
            changeAfterSecondsCurrent += Time.deltaTime;
            if (changeAfterSecondsCurrent >= changeAfterSeconds) ChangeRigidBodyProperties();
        }
    }

    // Cambia el estado del personaje
    public virtual void ChangeState(InGameStates newState)
    {
        // Si se perdio y desaparecio, no cambiar de estado
        if (newState == InGameStates.Lose && isDisappeared) return;

        // Cambiar el despliegue de dialogos
        state = newState;
        switch (state)
        {
            case InGameStates.Stand:
                if (preAttackDisplayer && !superJanKenUpActive) ShowPreAttack(!control);
                // Volver al nivel normal de sonido
                if (audioSourceFX) audioSourceFX.UpdateVolume();
                break;
            case InGameStates.Attack:

                // Reproducir según ataque actual
                if (attackSprite)
                {
                    switch (currentAttack)
                    {
                        case Attacks.Rock:
                            attackSprite.sprite = guuSpr;
                            break;

                        case Attacks.Paper:
                            attackSprite.sprite = paaSpr;
                            break;

                        case Attacks.Scissors:
                            attackSprite.sprite = chookiSpr;
                            break;

                        case Attacks.MagicWand:
                            attackSprite.sprite = magicWandSpr;
                            break;
                    }
                }

                //if (!isDisappeared && !superJanKenUpActive) ShowAttackDisplayer();
                if (!superJanKenUpActive) ShowAttackDisplayer();
                ShowPreAttack(false);
                break;
            case InGameStates.Charge:
            case InGameStates.Wait:
            case InGameStates.Win:
            case InGameStates.Lose:
            case InGameStates.Draw:
            default:
                ShowPreAttack(false);
                break;
        }

    }

    // TODO: Borrar
    public InGameStates GetState()
    {
        return state;
    }

    // Reproduce el sonido del estado
    public virtual void PlaySFX()
    {
        if (playedStateSFX) return;
        playedStateSFX = true;

        switch (state)
        {

            case InGameStates.Attack:

                if (superJanKenUpActive)
                {
                    superJanKenUpActive = false;
                }
                else
                {
                    // Reproducir según ataque actual
                    switch (currentAttack)
                    {
                        case Attacks.Rock:
                            audioSource.PlayOneShot(configuration.SFXRock());
                            break;

                        case Attacks.Paper:
                            audioSource.PlayOneShot(configuration.SFXPaper());
                            break;

                        case Attacks.Scissors:
                            audioSource.PlayOneShot(configuration.SFXScissors());
                            break;

                        case Attacks.MagicWand:

                            AudioClip magicWandSFX = configuration.SFXSMagicWand();
                            if (magicWandSFX) audioSource.PlayOneShot(magicWandSFX);
                            else
                            {
                                audioSource.PlayOneShot(configuration.SFXRock());
                                audioSource.PlayOneShot(configuration.SFXPaper());
                                audioSource.PlayOneShot(configuration.SFXScissors());
                            }
                            break;
                    }
                }

                // Si desaparecio, reproducir ataque pero disminuir su volumen
                if (isDisappeared) GoneAudioSource();

                break;

            case InGameStates.Win:
                if (onWinPlayInMaster)
                {
                    if (MasterSFXPlayer._player) MasterSFXPlayer._player.PlayOneShot(configuration.SFXWin(opponentIdentifier));
                }
                else
                {
                    audioSource.PlayOneShot(configuration.SFXWin(opponentIdentifier));
                }
                break;

            case InGameStates.Lose:
            case InGameStates.Lose2:

                // Nota: aca hubo una mala planificacion entre diseno de audio y mecanicas.
                // El viejo al perder da el tremendo discurso, por lo que se corta al ser destruido. Solucion: Que lo reproduzca algo global. Lo mismo con matriara
                if (!isDisappeared)
                {
                    if (onLosePlayInMaster)
                    {
                        if(MasterSFXPlayer._player) MasterSFXPlayer._player.PlayOneShot(configuration.SFXLose(opponentIdentifier));
                    }
                    else
                    {
                        audioSource.PlayOneShot(configuration.SFXLose(opponentIdentifier));
                    }
                }

                break;

        }

    }

    // Cambiar el ataque del personaje
    public void ChangeCurrentAttack(Attacks attack) {

        currentAttack = attack;
        if (control) return;

        // Si no se tiene el preAttackDisplayer, buscarlo
        if (preAttackDisplayer == null)
        {
            preAttackDisplayer = transform.Find("PreAttack").gameObject;
            preAttackSprite = preAttackDisplayer.GetComponent<SpriteRenderer>();
        }

        // Si esta en estado de stand y se cambia el ataque, mostrar preAtaque
        if (state == InGameStates.Stand) ShowPreAttack(true);

        // Si illuminati esta activo, mostrar ese sprite
        if (illuminati)
        {
            preAttackSprite.sprite = illuminatiSpr;
        }
        else {
            switch (currentAttack)
            {
                case Attacks.Rock:
                    preAttackSprite.sprite = guuSpr;
                    break;

                case Attacks.Paper:
                    preAttackSprite.sprite = paaSpr;
                    break;

                case Attacks.Scissors:
                    preAttackSprite.sprite = chookiSpr;
                    break;

                case Attacks.MagicWand:
                    preAttackSprite.sprite = magicWandSpr;
                    break;
            }
        }

    }


    // Cambiar desde un int el ataque
    public void ChangeCurrentAttack(int attack)
    {
        Attacks transformed = Attacks.Rock;

        // Si es negativo, seleccionar de manera random
        if (attack == -1) attack = UnityEngine.Random.Range(0, Enum.GetNames(typeof(Attacks)).Length);

        // Transformar a ataque
        switch (attack)
        {
            case 0:
                transformed = Attacks.Rock;
                break;
            case 1:
                transformed = Attacks.Paper;
                break;
            case 2:
                transformed = Attacks.Scissors;
                break;
            case 3:
                transformed = Attacks.MagicWand;
                break;
        }

        ChangeCurrentAttack(transformed);
    }

    // Obtener el ataque actual
    public Attacks GetCurrentAttack()
    {
        return currentAttack;
    }

    // Indica que se debe mostrar el preataque
    public void ShowPreAttack(bool show)
    {
        if (control) return;

        if (preAttackDisplayer == null)
        {
            preAttackDisplayer = transform.Find("PreAttack").gameObject;
            preAttackSprite = preAttackDisplayer.GetComponent<SpriteRenderer>();
        }

        if (changeAttackFillBar == null)
        {
            // Obtener la barra de carga y asignarle la camara correspondiente
            changeAttackFillBar = transform.Find("ChangeAttackFillBar").gameObject.GetComponent<ChangeAttackFillBar>();
            changeAttackFillBar.AssignCamera();
        }

        preAttackDisplayer.SetActive(show);
        if (!show) changeAttackFillBar.StopFilling();
    }

    // Indica si el personaje es controlado o no por el jugador
    public void SetControl(bool control)
    {
        this.control = control;
    }

    // Indica que el personaje esta en estado illuminati
    public void Illuminati()
    {
        illuminati = true;
        if (!preAttackDisplayer) {
            preAttackDisplayer = transform.Find("PreAttack").gameObject;
            preAttackSprite = preAttackDisplayer.GetComponent<SpriteRenderer>();
        }
        if (preAttackSprite) preAttackSprite.sprite = illuminatiSpr;
        if (preAttackDisplayer) preAttackDisplayer.GetComponent<PreAttack>().ToggleJoystickIcon(true);
    }

    // Revisar si es Illiminati
    public bool IsIlluminati()
    {
        return illuminati;
    }

    // Al presionar al personaje, procesar si puede o no quitar el illuminati
    public void OnMouseDown()
    {
        if (!illuminati || state != InGameStates.Stand) return;

        // En bruto por ahora, se buscará el singleplayermode
        SingleModeController sMC = FindObjectOfType<SingleModeController>();
        SurvivalModeController survivalMC = FindObjectOfType<SurvivalModeController>();
        if (sMC)
        {
            bool destroyed = sMC.DestroyIlluminati();
            if (!destroyed) return;
        }
        else if (survivalMC) {
            bool destroyed = survivalMC.DestroyIlluminati();
            if (!destroyed) return;
        }

        // Quitar el illuminati
        illuminati = false;
        preAttackDisplayer.GetComponent<PreAttack>().ToggleJoystickIcon(false);

        switch (currentAttack)
        {
            case Attacks.Rock:
                preAttackSprite.sprite = guuSpr;
                break;

            case Attacks.Paper:
                preAttackSprite.sprite = paaSpr;
                break;

            case Attacks.Scissors:
                preAttackSprite.sprite = chookiSpr;
                break;

            case Attacks.MagicWand:
                preAttackSprite.sprite = magicWandSpr;
                break;
        }
    }

    // Carga de barra de cambio de ataque
    public IEnumerator FillChangeAttack(float time) {
        yield return StartCoroutine(changeAttackFillBar.Fill(time));
    }

    // Obtener el spriteRendered asociado al body del personaje
    protected virtual void CheckSpriteRendered()
    {
        if (bodyRenderer) return;
        bodyRenderer = transform.Find("Body").GetComponent<SpriteRenderer>();
    }

    // Obtener el spriteRenderer del body
    public SpriteRenderer GetBodyRenderer()
    {
        CheckSpriteRendered();
        return bodyRenderer;
    }

    /// <summary>
    /// Metodo para simular la desaparicion del personaje
    /// </summary>
    public virtual void Disappear() {
        isDisappeared = true;
        CheckSpriteRendered();

        foreach (SpriteRenderer spriteRenderer in standRenderers)
        {
            spriteRenderer.color = Color.clear;
        }
    }

    /// <summary>
    /// Volver al mostrar al personaje
    /// </summary>
    public virtual void Reappear() {
        isDisappeared = false;
        CheckSpriteRendered();

        foreach (SpriteRenderer spriteRenderer in standRenderers)
        {
            spriteRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// Obtener respuesta si esta o no desaparecido el personaje
    /// </summary>
    /// <returns></returns>
    public bool IsDisappeared()
    {
        return isDisappeared;
    }

    // Crear una copia del ataque y desplegarlo con animacion
    public void ShowAttackDisplayer()
    {
        if(gameObject) StartCoroutine(CloneAttackDisplayer());
    }

    private IEnumerator CloneAttackDisplayer()
    {
        // Crear la copia y animar con Tween. Debe desaparecer al final
        GameObject copyAttack = Instantiate(attackDisplayer, attackDisplayer.transform.position, Quaternion.identity);
        copyAttack.transform.parent = null;// attackDisplayer.transform.parent;
        copyAttack.transform.localScale = new Vector3(.8f * (attackDisplayer.transform.parent.transform.localScale.x > 0? 1 : -1), .8f, 1);
        copyAttack.SetActive(true);
        copyAttack.GetComponent<CloneAttackDisplayer>().Animate(attackHasGravity);
        attackHasGravity = false;
        yield return null;
    }

    /// <summary>
    /// Inicia la corutina de superpower
    /// </summary>
    /// <param name="superPower"></param>
    public void StartShowGetSuperPower(SuperPowers superPower, bool showLife = false)
    {
        if (showSuperPowerCoroutine != null) StopCoroutine(showSuperPowerCoroutine);
        showSuperPowerCoroutine = StartCoroutine(ShowGetSuperPower(superPower, showLife));
    }

    /// <summary>
    /// Muestra superpoder obtenido
    /// </summary>
    /// <param name="superPower"></param>
    /// <returns></returns>
    public IEnumerator ShowGetSuperPower(SuperPowers superPower, bool showLife = false)
    {
        yield return StartCoroutine(ShowGetSuperPower(superPower, null, showLife));
    }

    /// <summary>
    /// Muestra superpoder obtenido
    /// </summary>
    /// <param name="superPower"></param>
    /// <param name="newLayer"></param>
    /// <returns></returns>
    public IEnumerator ShowGetSuperPower(SuperPowers superPower, string newLayer, bool showLife = false)
    {
        // Crear la copia y animar con Tween. Debe desaparecer al final
        if((showLife && !lifePrefab) || (!showLife && !superAttackPrefab))
        {
            // No hacer na'. Puede ser mas elegante, pero quien vea este codigo, no me juzgue
        }
        else
        {
            GameObject copyDialog = Instantiate(showLife ? lifePrefab : superAttackPrefab, superAttackPosition, Quaternion.identity);
            copyDialog.transform.parent = transform;
            copyDialog.transform.localPosition = superAttackPosition;
            copyDialog.SetActive(true);

            // Render de la imagen
            SpriteRenderer copyRenderer = copyDialog.GetComponent<SpriteRenderer>();
            if (newLayer != null) copyRenderer.sortingLayerID = SortingLayer.NameToID(newLayer);

            if (!showLife)
            {
                switch (superPower)
                {
                    case SuperPowers.TimeMaster:
                        copyRenderer.sprite = timeMaster;
                        break;
                    case SuperPowers.MagicWand:
                        copyRenderer.sprite = magicWand;
                        break;
                    case SuperPowers.JanKenUp:
                        copyRenderer.sprite = janKenUp;
                        break;
                }
            }

            Color white = new Color(255, 255, 255, 1);
            Color clear = new Color(255, 255, 255, 0);

            System.Action<ITween<Color>> fadeGlobe = (t) =>
            {
                if (copyDialog != null) copyRenderer.color = t.CurrentValue;
            };

            System.Action<ITween<Vector2>> moveIn = (t) =>
            {
                if (copyDialog != null) copyDialog.transform.position = t.CurrentValue;
            };

            System.Action<ITween<Vector2>> moveInComplete = (t) =>
            {
                if (copyDialog != null) Destroy(copyDialog);
            };

            float yStartDelta = superAttackDistance;
            float yEndDelta = superAttackDistance * (superAttackDistanceYFactorStart / 2);

            Vector2 startVector = new Vector2(copyDialog.transform.position.x, copyDialog.transform.position.y - yStartDelta);
            Vector2 endVector = new Vector2(copyDialog.transform.position.x, copyDialog.transform.position.y + yEndDelta);

            // Que aparezca lo que se gano
            copyDialog.Tween(string.Format("FadeIn{0}", copyDialog.GetInstanceID()), clear, white,
                superAttackTimeToFadeIn, TweenScaleFunctions.QuadraticEaseInOut, fadeGlobe);

            copyDialog.Tween(string.Format("Move{0}", copyDialog.GetInstanceID()), startVector, endVector,
                superAttackTimeToMove, TweenScaleFunctions.QuadraticEaseInOut, moveIn, moveInComplete);

            yield return new WaitForSeconds(superAttackTimeToMoveWait);

            copyDialog.Tween(string.Format("FadeOut{0}", copyDialog.GetInstanceID()), white, clear,
                    superAttackTimeToFadeOut, TweenScaleFunctions.QuadraticEaseInOut, fadeGlobe);
        }

    }

    // Efecto de particulas al entrar en colision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rigidBody = GetComponent<Rigidbody2D>();
        CreateDust(rigidBody.velocity.y);

        // Ver si se debe cambiar propiedades de rigidbody
        if (alterRigidBobyOnCollision) rigidBodyAlterPropertiesReady = true;
    }

    // Esperar una actualizacion de frames para realizar el cambio
    protected virtual void ChangeRigidBodyProperties()
    {
        alterRigidBobyOnCollision = false;
        rigidBodyAlterPropertiesReady = false;
        changeAfterSecondsCurrent = 0;
        Rigidbody2D rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.sharedMaterial = onCollisionMaterial;
        rigidBody.gravityScale = onCollisionGravity;
    }

    // Indicador que se necesita crear un efecto de polvo en el personaje
    public void CreateDust(float velocity)
    {
        GameObject dustObject = Instantiate(dust.gameObject, transform.position, Quaternion.identity) as GameObject;
        DustParticle dustCopy = dustObject.GetComponent<DustParticle>();

        // Aplicar variaciones segun la velocidad del cuerpo
        dustCopy.SetSpeed(velocity);
        dustCopy.SetSorting(sortingOrder);
        dustCopy.Play();
    }

    // Indicador que se necesita crear un efecto de polvo en el personaje al ser golpeado
    public void CreateDustHit(int xScaleMultipler)
    {
        GameObject dustObject = Instantiate(dustHit.gameObject, transform.position, Quaternion.identity) as GameObject;
        DustParticle dustCopy = dustObject.GetComponent<DustParticle>();

        // Aplicar variaciones segun la velocidad del cuerpo
        dustCopy.SetXScaleMultiplier(xScaleMultipler);
        dustCopy.SetSorting(sortingOrder);
        dustCopy.Play();
    }

    // Disminucion del volumen del audiosource de manera paulatina
    public void GoneAudioSource()
    {
        // Obtener el volumen actual del audiosource
        float currentVolume = audioSource.volume;

        System.Action<ITween<float>> reduceVolume = (t) =>
        {
            if(audioSource) audioSource.volume = currentVolume - (currentVolume * t.CurrentValue);
        };

        // Que se vaya alejando
        gameObject.Tween(string.Format("ReduceVolume{0}", audioSource.GetInstanceID()), 0, 1,
            .2f, TweenScaleFunctions.Linear, reduceVolume);

    }

    /* Preparacion del super ataque
     * Parametros:
     * @cameraScale : Factor de zoom para la camara durante la preparacion del super
     */
    public virtual IEnumerator PrepareSuper(int cameraResizeFactor, bool win = true, bool byeByeReferee = true) {

        GameObject instance = Instantiate(superJanKenUPPrefab, transform.position, Quaternion.identity) as GameObject;
        superAttack = null;
        superAttack = instance.GetComponent<SuperJanKenUPGeneral>();

        // Configurar el super
        Dictionary<string, object> data = new Dictionary<string, object>();
        data.Add("cameraResizeFactor", cameraResizeFactor);
        data.Add("position", superJanKenUpFXPosition);
        data.Add("color", superJanKenUpColor);
        data.Add("win", win);
        data.Add("byeByeReferee", byeByeReferee);
        superAttack.Setup(this, data);

        // Activar el poder
        superJanKenUpActive = true;

        // Preparar el super
        yield return superAttack.Prepare();
    }

    // Terminar una ejecucion del super ataque que se gano
    public virtual IEnumerator ExecuteSuperWin()
    {
        if (superAttack) yield return superAttack.WinExecute();
    }

    // Terminar una ejecucion del super ataque que se empato
    public virtual IEnumerator ExecuteSuperDraw()
    {
        if (superAttack) yield return superAttack.DrawExecute();
    }

    // Llamada posterior a la realizacion del super
    public virtual IEnumerator PostSuper()
    {
        if (superAttack) yield return superAttack.PostSuper();
    }

    // Eliminacion del super prefab
    public virtual void FinishSuper()
    {
        if (superAttack) superAttack.Finish();
        superJanKenUpActive = false;
    }

    // Obtener el identificador del personaje
    public string GetIdentifier()
    {
        if (configuration == null) configuration = GetComponent<CharacterConfiguration>();
        return configuration.GetIdentifier();
    }

    // Indicar que el ataque actual debe verse afectado por la gravedad
    public void CurrentAttackActivateGravity()
    {
        attackHasGravity = true;
    }


    // Mostrar la mano adecuada segun ataque (Desde animacion)
    public virtual void ShowHandAttack()
    {
        ShowHandAttack(true);
    }

    // Ocultar la mano adecuada segun ataque (Desde animacion)
    public void HideHandAttack()
    {
        ShowHandAttack(false);
    }

    // Mostrar la mano adecuada segun ataque
    protected virtual void ShowHandAttack(bool show)
    {
        // Si el personaje debe desaparecer, no se debe mostrar el ataque
        if (isDisappeared) show = false;

        if (show)
        {
            switch (currentAttack)
            {
                case Attacks.Rock:
                case Attacks.JanKenUp:
                    guuSpriteRenderer.SetActive(true);
                    paaSpriteRenderer.SetActive(false);
                    chokiSpriteRenderer.SetActive(false);
                    if (magicWandSpriteRenderer) magicWandSpriteRenderer.SetActive(false);
                    break;
                case Attacks.Paper:
                    guuSpriteRenderer.SetActive(false);
                    paaSpriteRenderer.SetActive(true);
                    chokiSpriteRenderer.SetActive(false);
                    if (magicWandSpriteRenderer) magicWandSpriteRenderer.SetActive(false);
                    break;
                case Attacks.Scissors:
                    guuSpriteRenderer.SetActive(false);
                    paaSpriteRenderer.SetActive(false);
                    chokiSpriteRenderer.SetActive(true);
                    if (magicWandSpriteRenderer) magicWandSpriteRenderer.SetActive(false);
                    break;
                case Attacks.MagicWand:
                    if (magicWandSpriteRenderer)
                    {
                        magicWandSpriteRenderer.SetActive(true);
                        guuSpriteRenderer.SetActive(false);
                        paaSpriteRenderer.SetActive(false);
                        chokiSpriteRenderer.SetActive(false);
                    }
                    else
                    {
                        guuSpriteRenderer.SetActive(true);
                        paaSpriteRenderer.SetActive(false);
                        chokiSpriteRenderer.SetActive(false);
                    }
                    break;
            }
        }
        else
        {
            guuSpriteRenderer.SetActive(false);
            paaSpriteRenderer.SetActive(false);
            chokiSpriteRenderer.SetActive(false);
            if (magicWandSpriteRenderer) magicWandSpriteRenderer.SetActive(false);
        }
    }

    // Activar el cambio de material al detectar una colision
    public void AlterRigidBodyOnContact(PhysicsMaterial2D onCollisionMaterial, float onCollisionGravity)
    {
        alterRigidBobyOnCollision = true;
        this.onCollisionMaterial = onCollisionMaterial;
        this.onCollisionGravity = onCollisionGravity;
    }

    // Cambio de la capa de orden para los spriteRenderer
    public virtual void ChangeSpritesLayerTo(string layerName)
    {
        SpriteRenderer[] spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingLayerID = SortingLayer.NameToID(layerName);
        }
    }

    // Cambiar el color de los SpriteRendered
    public virtual void ChangeStandColor(Color color)
    {
        foreach (SpriteRenderer spriteRenderer in standRenderers)
        {
            spriteRenderer.color = color;
        }
    }

    // Desactivar/Activar el RidigBody
    public void ToggleRigidBody(bool state)
    {
        Rigidbody2D rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.isKinematic = state;
    }

    /**
     * Obtencion del identificador del NPC de soporte
     * @return {SupportCrowd} 
     */
    public JankenUp.NPC.Identifiers GetSupportNPCIdentifier()
    {
        return supportNPCIdentifier;
    }

    /**
     * Seteo del NPC de soporte asociado
     * @param {SupportNPC} supportCrowd: NPC que debe ser asociado
     * @return {SupportCrowd} 
     */
    public void SetSupportNPC(SupportCrowd supportCrowd)
    {
        supportNPC = supportCrowd;
    }

    /**
     * Obtencion del NPC de soporte asociado
     * @return {SupportCrowd} 
     */
    public SupportCrowd GetSupportNPC()
    {
        return supportNPC;
    }

    /**
     * Cambiar la velocidad del animator
     * @params {int speed} Nueva velocidad del animator
     */
    protected void ChangeAnimatorSpeed(int speed)
    {
        animator.speed = speed;
    }

    /// <summary>
    /// Se crea y da visibilidad al selector de finta
    /// </summary>
    public void ShowFeintController()
    {
        if (feintController == null)
        {
            GameObject gameObject = Instantiate(feintControllerPrefab, transform);
            gameObject.transform.localPosition = feintPosition;
            feintController = gameObject.GetComponent<FeintController>();
            feintController.FlipElements(transform.localScale.x);
            // Remover la transformacion padre para poder hacer detectar colisiones
            // Aca hay que tener en cuenta que el personaje este sobre una plataforma, de lo contrario al quitarlo, el controlador quedara ubicado en una posicion inalcanzable
            StartCoroutine(DeattachFeintController());
        }
        else
        {
            feintController.Show();
        }
    }

    /// <summary>
    /// Revision de condiciones hasta que se cumpla la posibilidad de crear un controlador de fintas
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeattachFeintController()
    {
        while (alterRigidBobyOnCollision || rigidBodyAlterPropertiesReady) yield return new WaitForSeconds(changeAfterSeconds);
        feintController.transform.SetParent(null);
    }

    /// <summary>
    /// Oculta el controlador de finta en caso de que exista
    /// </summary>
    public void HideFeintController()
    {
        if (feintController) feintController.Hide();
    }

    /// <summary>
    /// Muestra de la finta del jugador como el ataque que va a lanzar
    /// </summary>
    /// <param name="attack"></param>
    public void ShowFeint(int attack)
    {

        if (!feintAttack)
        {
            GameObject gameObject = Instantiate(feintAttackPrefab, transform);
            gameObject.transform.localPosition = feintPosition;
            feintAttack = gameObject.GetComponent<FeintAttack>();
        }

        // Si esta en estado de stand y se cambia el ataque, mostrar preAtaque
        if (state == InGameStates.Stand)
        {
            switch (IntToAttack(attack))
            {
                case Attacks.Rock:
                    feintAttack.SetSprite(guuSpr);
                    break;

                case Attacks.Paper:
                    feintAttack.SetSprite(paaSpr);
                    break;

                case Attacks.Scissors:
                    feintAttack.SetSprite(chookiSpr);
                    break;

                default:
                    feintAttack.SetSprite(null);
                    break;
            }
        }
        else
        {
            feintAttack.SetSprite(null);
        }
    }

    /// <summary>
    /// Oculta la finta seleccionada
    /// </summary>
    public void HideFeint()
    {
        if (feintAttack)
        {
            feintAttack.AnimationHide();
            feintAttack = null;
        }
    }

    /// <summary>
    /// Obtencion de los componentes para mostrar el ataque
    /// </summary>
    private void GetAttackDisplayer()
    {
        // Obtener los displayer de ataque y preataque
        attackDisplayer = transform.Find("Attack").gameObject;
        attackSprite = attackDisplayer.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Convierte un int a su equivalente de ataque
    /// </summary>
    /// <param name="attack"></param>
    /// <returns></returns>
    public Attacks IntToAttack(int attack)
    {
        Attacks transformed = Attacks.Rock;

        // Si es negativo, seleccionar de manera random
        if (attack == -1) attack = UnityEngine.Random.Range(0, Enum.GetNames(typeof(Attacks)).Length);

        // Transformar a ataque
        switch (attack)
        {
            case 0:
                transformed = Attacks.Rock;
                break;
            case 1:
                transformed = Attacks.Paper;
                break;
            case 2:
                transformed = Attacks.Scissors;
                break;
            case 3:
                transformed = Attacks.MagicWand;
                break;
        }

        return transformed;
    }

    /// <summary>
    /// Mostrar un emote a partir del personaje
    /// </summary>
    /// <param name="emoteButton"></param>
    public void ShowEmote(Emote emoteUI)
    {
        // Obtener el nuevo emote y el sprite del emoteUI
        Emote newEmote = Instantiate(emotePrefab,transform);
        Sprite sprite = emoteUI.GetSprite();

        // Asignar el emote e iniciar
        newEmote.SetSprite(sprite);
        newEmote.SetPartOfWorld();
        newEmote.SetLocalPosition(emotePosition, transform.localScale.x);
    }

    /// <summary>
    /// Eliminacion de elementos asociados al controlador
    /// </summary>
    public virtual void OnDestroy()
    {
        if (feintController) Destroy(feintController.gameObject);
        if (showSuperPowerCoroutine != null) StopCoroutine(showSuperPowerCoroutine);
    }

    /// <summary>
    /// Giro horizontal del personaje, aplicado a su escala local
    /// </summary>
    /// <param name="xFactor"></param>
    public virtual void Flip(int xFactor) {
        // Revisar si la escala actual coincide con el factor, que puede ser -1 o 1
        if (xFactor < 0) xFactor = -1;
        else xFactor = 1;

        isFlipped = xFactor == -1;

        // Aplicar el facor al valor absoluto de su escala en el eje X
        transform.localScale = new Vector3(
            xFactor * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );

    }

    /// <summary>
    /// Indicar si el personaje esta bloqueado o no. De estarlo, asignar material y cambiar valor
    /// </summary>
    /// <param name="lockValue"></param>
    public virtual void SetLock(bool lockValue)
    {
        Material newMaterial = new Material(lockValue ? lockMaterial : unlockMaterial);
        if (lockValue)
        {
            newMaterial.SetFloat("_GrayscaleAmount",1);
            //newMaterial.RecalculateMasking();
        }

        foreach (SpriteRenderer spriteRenderer in standRenderers)
        {
            spriteRenderer.material = newMaterial;
        }
    }

    /// <summary>
    /// Obtencion del color principal del personaje
    /// </summary>
    /// <returns></returns>
    public Color GetMainColor()
    {
        return mainColor;
    }

    /// <summary>
    /// Registro del oponente actual
    /// </summary>
    /// <param name="opponentIdentifier"></param>
    public void SetOpponent(string opponentIdentifier)
    {
        this.opponentIdentifier = opponentIdentifier;
    }


    /// <summary>
    /// Identificar si el personaje esta volteado
    /// </summary>
    /// <returns></returns>
    public bool IsFlipped()
    {
        return isFlipped;
    }

    /// <summary>
    /// Revisar si es posible que sea chocado por un objeto
    /// </summary>
    /// <returns></returns>
    public bool CanRecieveHitsByObjects()
    {
        bool velocityCondition = mainRigidbody && mainRigidbody.velocity.y >= canRecieveHitsByObjectsMinVelocity && mainRigidbody.velocity.y <= canRecieveHitsByObjectsMaxVelocity;
        return !isDisappeared && !alterRigidBobyOnCollision && velocityCondition;
    }
}
