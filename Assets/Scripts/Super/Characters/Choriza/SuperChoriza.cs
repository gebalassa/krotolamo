using DigitalRuby.Tween;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SuperChoriza : SuperJanKenUPBase
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
    [SerializeField] AudioClip sfxSuper3;
    [SerializeField] AudioClip sfxSwoosh;

    [Header("Destroy Phase")]
    [SerializeField] [Range(0, 1f)] float prepareDestroy = 0.3f;

    [Header("Super Execute Phase")]
    [SerializeField] [Range(0, 2f)] float timeToThrown = .25f;
    [SerializeField] [Range(1, 100)] float xForce = 5f;
    [SerializeField] [Range(1, 100)] float yForce = 5f;
    [SerializeField] [Range(0, 2f)] float shakeMagnitude = 0.1f;
    [SerializeField] [Range(0, 100f)] float superExecuteCameraSize = 2;
    [SerializeField] [Range(0, 100f)] float superExecuteCameraRotation = 0;
    [SerializeField] [Range(0, 100f)] float superExecuteCameraTime = 0.5f;

    [Header("Other")]
    [SerializeField] GameObject applePrefab;
    [SerializeField] Vector2 applePosition = new Vector2(1.96f, 4.86f);

    // Resize de la camara que se usara para el super
    float cameraSizeOriginal = 0;
    Vector3 cameraPositionOriginal = new Vector3();
    float cameraOriginalZ = 10;
    bool flipped = false;
    bool win = true;

    // Fases del super
    bool preparePhase = true;

    // El enemigo
    GameObject otherCharacter;

    // Tweens de camara
    Tween<float> tweenCameraZoom;
    Tween<Vector3> tweenCameraMovement;
    Tween<Vector3> tweenCameraRotation;

    public override void Setup(CharacterInGameController character, Dictionary<string, object> data)
    {
        // Llamar a base
        base.Setup(character, data);

        // Indicar si gano
        win = data.ContainsKey("win") ? (bool)data["win"] : true;

        // Obtener otros datos
        cameraSizeOriginal = Camera.main.orthographicSize;
        cameraPositionOriginal = Camera.main.transform.position;

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

        MasterSFXPlayer._player.PlayOneShot(sfxSuper2);

        // Devolver al PJ a su sortingOrder y quitar el overlay
        ToggleCameraOverlay(false);
        yield return new WaitForSeconds(timeToFadeIn);

        character.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Arena);

        TransformCamera(cameraSizeOriginal, 0, 0, cameraChangesTimeTwo);

        yield return new WaitForSeconds(timeToExit);
    }

    public override IEnumerator WinExecute()
    {
        // Instanciar todos los extras para los punos rapidos
        character.ChangeState(InGameStates.SuperComplete);
        yield return StartCoroutine(ThrowApple());

        // Indicar que deben ser explotados
        yield return null;
    }

    public override IEnumerator DrawExecute()
    {
        yield return StartCoroutine(WinExecute());
    }

    /// <summary>
    /// Lanza manzana al otro jugador
    /// </summary>
    /// <returns></returns>
    private IEnumerator ThrowApple()
    {
        bool complete = false;
        GameObject appleObject = Instantiate(applePrefab, character.transform);
        ChorizaApple appleChoriza = appleObject.GetComponent<ChorizaApple>();
        appleChoriza.Setup(character.gameObject, flipped);
        appleObject.transform.localPosition = applePosition;
        appleObject.transform.parent = null;
        MasterSFXPlayer._player.PlayOneShot(sfxSuper3);
        MasterSFXPlayer._player.PlayOneShot(sfxSwoosh);
        int count = 0;
        int maxCount = 100;
        while (!appleChoriza.EnemyHit() && count++ < maxCount) yield return null;
        PlayStrongHit();
    }

    // Acciones posteriores a la ejecucion del super
    public override IEnumerator PostSuper()
    {
        // Realizar la espera
        yield return new WaitForSeconds(timePostFinish);
            
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

}