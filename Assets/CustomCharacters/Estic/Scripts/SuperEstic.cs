using DigitalRuby.Tween;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SuperEstic : SuperJanKenUPBase
{

    [Header("Prepare Phase")]
    [SerializeField] [Range(0, 1f)] float timeToFadeIn = 0.5f;
    [SerializeField] [Range(0, 2f)] float timeToPrepare = 1f;
    [SerializeField] [Range(0, 2f)] float timeToExit = 1f;
    [SerializeField] [Range(0, 2f)] float timeToFinish = .18f;
    [SerializeField] [Range(0, 2f)] float timePostFinish = 0.5f;

    [Header("Camera Changes")]
    [SerializeField] [Range(0, 10f)] float cameraSize = 8;
    [SerializeField] [Range(0, 10f)] float cameraRotation = 5;
    [SerializeField] [Range(0, 10f)] float cameraX = 7;
    [SerializeField] [Range(0, 10f)] float cameraChangesTimeOne = .5f;
    [SerializeField] [Range(0, 10f)] float cameraChangesTimeTwo = .5f;

    [Header("SFX")]
    [SerializeField] AudioClip sfxSuper1;
    [SerializeField] AudioClip sfxSuper2;
    [SerializeField] AudioClip sfxTeleport;
    [SerializeField] AudioClip sfxSwoosh;

    [Header("Destroy Phase")]
    [SerializeField] [Range(0, 1f)] float prepareDestroy = 0.3f;

    [Header("Others FX")]
    [SerializeField] GameObject animeLinesPrefab;
    [SerializeField] GameObject superTeleportPrefab;

    [Header("Super Execute Phase")]
    [SerializeField] [Range(0, 2f)] float timeToJump = .1f;
    [SerializeField] [Range(1, 100)] float xForce = 5f;
    [SerializeField] [Range(1, 100)] float yForce = 5f;
    [SerializeField] [Range(0, 2f)] float shakeMagnitude = 0.1f;
    [SerializeField] [Range(0, 100f)] float superExecuteCameraSize = 2;
    [SerializeField] [Range(0, 100f)] float superExecuteCameraRotation = 0;
    [SerializeField] [Range(0, 100f)] float superExecuteCameraTime = 0.5f;
    [SerializeField] [Range(0,10f)] float deltaFinalPosition = 6f;
    [SerializeField] [Range(0,10f)] float deltaFinalPositionY = 6f;
    [SerializeField] [Range(0, 1f)] float superExecuteInitSlowMotionAfer = 0.2f;
    [SerializeField] [Range(0, 1f)] float superExecuteTimeSlowMotionDecelerationTime = .15f;
    [SerializeField] [Range(0, 1f)] float superExecuteTimeSlowMotionWaitingTime = .005f;
    [SerializeField] [Range(0, 1f)] float superExecuteTimeSlowMotionAcelerationTime = .05f;
    [SerializeField] [Range(0, 1f)] float superExecuteTimeSlowMotionMinAceleration = 0.05f;
    [SerializeField] [Range(0, 1f)] float superExecuteCameraTimeTwo = .1f;
    [SerializeField] [Range(0, 10f)] float superExecuteKickRefereeAnimatorSpeed = 2.5f;
    [SerializeField] [Range(0, 10f)] float kickMaxYDelta = 1f;
    [SerializeField] [Range(0, 10f)] float gravityOnJump = .1f;


    [Header("Super Post Phase")]
    [SerializeField] [Range(0, 10f)] float superTeleporDeltaY = 5;
    [SerializeField] [Range(0, 2f)] float timePostTeleport = 0.1f;

    [Header("Special Fall")]
    [SerializeField] int spawnGravity = 12;
    [SerializeField] int spawnPostGravity = 1;
    [SerializeField] PhysicsMaterial2D spawnMaterial;
    [SerializeField] PhysicsMaterial2D spawnPostMaterial;

    // Resize de la camara que se usara para el super
    float cameraSizeOriginal = 0;
    Vector3 cameraPositionOriginal = new Vector3();
    float cameraOriginalZ = 10;
    bool flipped = false;
    bool win = true;

    // Fases del super
    bool yoyazoOnPoint = false;

    // El enemigo
    GameObject otherCharacter;

    // Particulas
    GameObject animeSpeedParticle;

    // Tweens de camara
    Tween<float> tweenCameraZoom;
    Tween<Vector3> tweenCameraMovement;
    Tween<Vector3> tweenCameraRotation;

    // Comportamiento especial de no velocidad X frente a ciertos PJ
    List<string> noXForceSuper = new List<string>() { JankenUp.Characters.GASTONMIAUFFIN };

    // Posicion inicial de las partes, para volver a ella tras el loop
    Vector3 characterOriginalPosition;

    // Otros utiles
    Rigidbody2D rigidbody;

    public override void Setup(CharacterInGameController character, Dictionary<string, object> data)
    {
        // Llamar a base
        base.Setup(character, data);

        // Indicar si gano
        win = data.ContainsKey("win") ? (bool)data["win"] : true;

        // Obtener otros datos
        cameraSizeOriginal = Camera.main.orthographicSize;
        cameraPositionOriginal = Camera.main.transform.position;

        // Obtener la posicion del personaje
        characterOriginalPosition = character.transform.localPosition;

        if (character.transform.localScale.x < 0) flipped = true;

        // Encontrar al otro personaje en juego
        CharacterInGameController[] characters = FindObjectsOfType<CharacterInGameController>();
        foreach (CharacterInGameController ch in characters)
        {
            if (!ch.Equals(character))
            {
                otherCharacter = ch.gameObject;
                break;
            }
        }

        rigidbody = character.GetComponent<Rigidbody2D>();
    }

    public override IEnumerator Prepare()
    {
        // Iniciar animacion de super
        MasterSFXPlayer._player.PlayOneShot(sfxSuper1);
        character.ChangeState(InGameStates.Super);

        // Asignar correctamente la camara
        transform.Find("Overlay").GetComponent<Canvas>().worldCamera = Camera.main;
        character.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Foreground);

        // Realizar el fadeIn del overlay y colocar al PJ seleccionado sobre el overlay
        ToggleCameraOverlay(true);

        TransformCamera(cameraSize, 0, 0, cameraChangesTimeOne);
        yield return new WaitForSeconds(timeToFadeIn + timeToPrepare);

        // Devolver al PJ a su sortingOrder y quitar el overlay
        ToggleCameraOverlay(false);
        yield return new WaitForSeconds(timeToFadeIn);

        character.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Arena);
        TransformCamera(cameraSizeOriginal, 0, 0, cameraChangesTimeTwo);

        yield return new WaitForSeconds(timeToExit);
    }

    public override IEnumerator WinExecute()
    {
        // Realizar el salto
        MasterSFXPlayer._player.PlayOneShot(sfxSuper2);
        yield return StartCoroutine(KickSomeAss());

        // Indicar que deben ser explotados
        yield return null;
    }

    public override IEnumerator DrawExecute()
    {
        yield return StartCoroutine(WinExecute());
    }

    /// <summary>
    /// Patear al otro jugador
    /// </summary>
    /// <returns></returns>
    private IEnumerator KickSomeAss()
    {
        bool complete = false;

        // Mover camara
        if(win) TransformCamera(superExecuteCameraSize, superExecuteCameraRotation, -otherCharacter.transform.localPosition.x, superExecuteCameraTime);

        animeSpeedParticle = Instantiate(animeLinesPrefab);
        animeSpeedParticle.transform.localPosition = new Vector2(otherCharacter.transform.localPosition.x, animeSpeedParticle.transform.localPosition.y);
        Shake._this.ShakeIt(shakeMagnitude);

        // Para que el efecto se vea mejor, patada sera dada en foreground
        character.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Foreground);
        bool slowMotionInitiated = false;

        // Rotacion final
        float finalDeltaPosition = flipped ? -1 * deltaFinalPosition : deltaFinalPosition;
        bool noXMovements = noXForceSuper.Contains(otherCharacter.GetComponent<CharacterConfiguration>().GetIdentifier());
        Vector3 initPosition = character.transform.localPosition;
        Vector3 finaPosition = otherCharacter ? new Vector3(noXMovements? 0 : otherCharacter.transform.localPosition.x - finalDeltaPosition, deltaFinalPositionY, otherCharacter.transform.localPosition.z)
            : character.transform.localPosition;

        // Cambiar la posicion de la camara
        System.Action<ITween<Vector3>> moveIn = (t) =>
        {
            float newY = kickMaxYDelta * Mathf.Sqrt((t.CurrentProgress > .5f ? 1 - t.CurrentProgress : t.CurrentProgress));
            character.transform.position = new Vector3(t.CurrentValue.x, t.CurrentValue.y + newY, t.CurrentValue.z);
            if(!slowMotionInitiated && t.CurrentProgress >= superExecuteInitSlowMotionAfer)
            {
                slowMotionInitiated = true;
                StartCoroutine(SlowMotion());
            }
        };

        System.Action<ITween<Vector3>> moveInComplete = (t) =>
        {
            complete = true;
        };

        // Acercar la camara
        gameObject.Tween(string.Format("moveIn{0}", GetInstanceID()), initPosition, finaPosition,
            timeToJump, TweenScaleFunctions.QuadraticEaseInOut, moveIn, moveInComplete);

        // Cambio de estado
        character.ChangeState(InGameStates.SuperComplete);
        MasterSFXPlayer._player.PlayOneShot(sfxTeleport);

        while (!complete) yield return null;

        // En caso de empate, considerar super del otro personaje para ver si aplicar fuerza en X
        float factor = flipped ? .5f : -1;

        // Cambiar velocidad y gravedad
        if (rigidbody){
            rigidbody.velocity = new Vector2(xForce * factor, yForce * factor);
            rigidbody.gravityScale = gravityOnJump;
        }

        bool specialVerticalCase = !win && otherCharacter && noXForceSuper.Contains(otherCharacter.GetComponent<CharacterConfiguration>().GetIdentifier());
        float xSpecialFactor = specialVerticalCase ? 0.2f : 1f;
        if (specialVerticalCase) StartFall();


        while (!yoyazoOnPoint) yield return null;
        Destroy(animeSpeedParticle);
        PlayStrongHit();
    }

    /// <summary>
    /// Indicar que se puede ejecutar el yoyazo
    /// </summary>
    public void Yoyazo()
    {
        yoyazoOnPoint = true;
    }

    // Acciones posteriores a la ejecucion del super
    public override IEnumerator PostSuper()
    {
        // Realizar la espera para aparecer
        character.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Arena);
        yield return new WaitForSeconds(timePostFinish);

        if (rigidbody) rigidbody.gravityScale = 1;
        
        // En caso de no haber ganado o que se trate de un modo brigido (Si el arbitro no ha desaparecido), devolver a su ubicacion
        Referee referee = FindObjectOfType<Referee>();
        if ( !win || (referee && referee.GetImmuneToDisappear())) {

            // Primer teleport
            SuperTeleport superTeleportInstancePostLoop = CreateTeleport();
            superTeleportInstancePostLoop.AutoDestroy(.5f);
            superTeleportInstancePostLoop.transform.localPosition = new Vector2(character.transform.localPosition.x, character.transform.localPosition.y + superTeleporDeltaY);
            superTeleportInstancePostLoop.transform.parent = transform;
            character.Disappear();

            yield return new WaitForSeconds(timePostTeleport);

            // Teleport en la zona original
            character.transform.localPosition = characterOriginalPosition;
            SuperTeleport superTeleportInstanceFinal = CreateTeleport();
            superTeleportInstanceFinal.ChangeState(SuperTeleport.animationStates.TeleportTwo);
            superTeleportInstanceFinal.transform.parent = transform;
            character.ChangeState(InGameStates.Stand);
            character.Reappear();
        }
            
    }

    // Animacion para transforma la camara segun el super
    public void TransformCamera(float cameraSize, float cameraRotation, float cameraX, float cameraChangesTime)
    {
        if (tweenCameraZoom != null) tweenCameraZoom.Stop(TweenStopBehavior.DoNotModify);
        if (tweenCameraRotation != null) tweenCameraRotation.Stop(TweenStopBehavior.DoNotModify);
        if (tweenCameraMovement != null) tweenCameraMovement.Stop(TweenStopBehavior.DoNotModify);

        // Cambiar tamano de la camara
        System.Action<ITween<float>> zoomIn = (t) =>
        {
            Camera.main.orthographicSize = t.CurrentValue;
        };

        // Rotacion final
        Vector3 rotation = new Vector3(0, 0, cameraRotation);
        Vector3 initRotation = Camera.main.transform.eulerAngles;
        Vector3 finaRotation = initRotation - rotation;

        // Cambiar rotacion de la camara
        System.Action<ITween<Vector3>> rotateIn = (t) =>
        {
            Camera.main.transform.eulerAngles = t.CurrentValue;
        };

        // Posicion final
        Vector3 position = new Vector3(cameraX, 0, cameraOriginalZ);
        Vector3 finalPosition = cameraPositionOriginal - position;

        // Cambiar la posicion de la camara
        System.Action<ITween<Vector3>> moveIn = (t) =>
        {
            Camera.main.transform.position = t.CurrentValue;
            cameraPositionOriginal = t.CurrentValue;
        };

        System.Action<ITween<Vector3>> moveInComplete = (t) =>
        {
            cameraPositionOriginal = finalPosition;
            Camera.main.transform.position = new Vector3(finalPosition.x, finalPosition.y, -cameraOriginalZ);
        };

        // Acercar la camara
        tweenCameraZoom = Camera.main.gameObject.Tween(string.Format("ZoomIn{0}", Camera.main.GetInstanceID()), Camera.main.orthographicSize, cameraSize,
            cameraChangesTime, TweenScaleFunctions.QuadraticEaseInOut, zoomIn);

        // Girar la camara
        if (!CheckCamerasLike()) tweenCameraRotation = Camera.main.gameObject.Tween(string.Format("Rotate{0}", Camera.main.GetInstanceID()), initRotation, finaRotation,
            cameraChangesTime, TweenScaleFunctions.QuadraticEaseInOut, rotateIn);

        // Mover la camara
        tweenCameraMovement = Camera.main.gameObject.Tween(string.Format("Move{0}", Camera.main.GetInstanceID()), cameraPositionOriginal, finalPosition,
            cameraChangesTime, TweenScaleFunctions.QuadraticEaseInOut, moveIn, moveInComplete);
    }

    /// <summary>
    /// Sonido de golpe duro
    /// </summary>
    public void PlayStrongHit()
    {
        MasterSFXPlayer._player.StrongHit();
    }

    /// <summary>
    /// Generar una camara lenta del juego por un corto perido de tiempo
    /// </summary>
    /// <returns></returns>
    public IEnumerator SlowMotion()
    {
        // Estas variables deben estar como slider para probar!!
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
        gameObject.Tween(string.Format("Deceleration{0}", GetInstanceID()), 1, superExecuteTimeSlowMotionMinAceleration, superExecuteTimeSlowMotionDecelerationTime, TweenScaleFunctions.QuadraticEaseOut, updateTimeAndPitch, completeDeceleration);

        while (!decelerationComplete) yield return null;

        // Esperar un momento
        yield return new WaitForSeconds(superExecuteTimeSlowMotionWaitingTime);

        // Atacar con el yoyo
        character.ChangeState(InGameStates.SuperLoop);

        // Ejecutar la aceleracion
        gameObject.Tween(string.Format("Aceleration{0}", GetInstanceID()), superExecuteTimeSlowMotionMinAceleration, 1, superExecuteTimeSlowMotionAcelerationTime, TweenScaleFunctions.QuadraticEaseIn, updateTimeAndPitch, completeAceleration);

        // Camara debe volver a sus valores iniciales
        TransformCamera(cameraSizeOriginal, 0, 0, superExecuteCameraTimeTwo);

        while (!acelerationComplete) yield return null;

    }

    /// <summary>
    /// Reproduccion del sonido de teletransportacion
    /// </summary>
    private SuperTeleport CreateTeleport()
    {
        // Realizar aparicion del teleport
        SuperTeleport teleport = Instantiate(superTeleportPrefab, character.transform).GetComponent<SuperTeleport>();
        teleport.SetSuperController(this);
        MasterSFXPlayer._player.PlayOneShot(sfxTeleport);
        return teleport;
    }

    /// <summary>
    /// Alterar la fisica del PJ para caidas con PJ especiales
    /// </summary>
    private void StartFall()
    {
        Rigidbody2D rigidbody = character.GetComponent<Rigidbody2D>();
        if (rigidbody)
        {
            rigidbody.bodyType = RigidbodyType2D.Dynamic;

            rigidbody.sharedMaterial = spawnMaterial;
            rigidbody.gravityScale = spawnGravity;

            // Indicar al controlador que debe hacer el cambio de material al tocar plataforma
            character.AlterRigidBodyOnContact(spawnPostMaterial, spawnPostGravity);
        }

    }

}