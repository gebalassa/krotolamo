using DigitalRuby.Tween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SuperGastonMiauffin : SuperJanKenUPBase
{

    [Header("Prepare Phase")]
    [SerializeField] [Range(0, 1f)] float timeToFadeIn = 0.5f;
    [SerializeField] [Range(0, 2f)] float timeToPrepare = 1.5f;
    [SerializeField] [Range(0, 2f)] float timeToExit = 1f;
    [SerializeField] [Range(0, 2f)] float timeToFinish = 1f;
    [SerializeField] [Range(0, 2f)] float timePostFinish = 0.5f;

    [Header("Camera Changes")]
    [SerializeField] [Range(0, 10f)] float cameraSize = 6;
    [SerializeField] [Range(0, 10f)] float cameraRotation = 5;
    [SerializeField] [Range(0, 10f)] float cameraX = 7;
    [SerializeField] [Range(0, 10f)] float cameraChangesTimeOne = 1f;
    [SerializeField] [Range(0, 10f)] float cameraChangesTimeTwo = .5f;

    [Header("SFX")]
    [SerializeField] AudioClip sfxTransformation;
    [SerializeField] AudioClip sfxFall;
    [SerializeField] AudioClip sfxInvocation;
    [SerializeField] AudioClip sfxSignal;

    [Header("FX")]
    [SerializeField] GameObject fxExplosion;

    [Header("Survival")]
    [SerializeField] Vector3 spawnPosition;
    [SerializeField] int spawnGravity = 12;
    [SerializeField] int spawnPostGravity = 1;
    [SerializeField] PhysicsMaterial2D spawnMaterial;
    [SerializeField] PhysicsMaterial2D spawnPostMaterial;

    [Header("BackToPosition")]
    [SerializeField] float moveXForce = 3f;
    [SerializeField] float moveYForce = 5f;

    [Header("Others")]
    [SerializeField] [Range(0, 50f)] float verticalForce = 50f;
    [SerializeField] [Range(0, 1f)] float timeToJump = .5f;
    [SerializeField] [Range(0, 1f)] float timeToFall = .5f;

    // Componentes
    Rigidbody2D pjRigidbody;
    Animator animator;

    // Resize de la camara que se usara para el super
    float cameraSizeOriginal = 0;
    Vector3 cameraPositionOriginal = new Vector3();
    float cameraOriginalZ = 10;
    float originalSpeed;
    bool win = true;
    int flippedMultiplier = 1;

    public override void Setup(CharacterInGameController character, Dictionary<string, object> data)
    {
        // Llamar a base
        base.Setup(character, data);

        // Obtener otros datos
        cameraSizeOriginal = Camera.main.orthographicSize;
        cameraPositionOriginal = Camera.main.transform.position;

        animator = character.GetComponent<Animator>();
        pjRigidbody = character.GetComponent<Rigidbody2D>();
        originalSpeed = animator.speed;

        if (character.transform.localScale.x < 0) flippedMultiplier = -1;

    }

    public override IEnumerator Prepare()
    {
        // Asignar correctamente la camara
        transform.Find("Overlay").GetComponent<Canvas>().worldCamera = Camera.main;

        character.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Foreground);

        // Revisar el sector del cuadrilatero en que se encuentra el personaje utilizando como referencia su xScale
        int scaleMultiplier = character.transform.localScale.x > 0? 1 : -1;

        // Realizar el fadeIn del overlay y colocar al PJ seleccionado sobre el overlay
        ToggleCameraOverlay(true);

        // Cambio de estado
        character.ChangeState(InGameStates.Super);
        animator.speed = 0;

        TransformCamera(cameraSize, cameraRotation * scaleMultiplier, cameraX * scaleMultiplier, cameraChangesTimeOne);

        yield return new WaitForSeconds(timeToFadeIn / 2);

        MasterSFXPlayer._player.PlayOneShot(sfxInvocation);

        // Instanciar el efecto de carga
        yield return new WaitForSeconds(timeToPrepare + timeToFadeIn / 2);

        // Enviar a volar al PJ
        JumpFX();

        // Devolver al PJ a su sortingOrder y quitar el overlay
        ToggleCameraOverlay(false);
        yield return new WaitForSeconds(timeToJump);

        // Detener el salto del PJ
        StopJumpFX();

        // Detener el shake
        Shake._this.StopIt();

        character.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Arena);

        TransformCamera(cameraSizeOriginal, -cameraRotation * scaleMultiplier, - cameraX * scaleMultiplier, cameraChangesTimeTwo);

        yield return new WaitForSeconds(timeToExit);
    }

    // Mostrar el salto del PJ hacia fuera de la pantalla
    private void JumpFX() {

        // Generar explosion y mandar a volar a los PJ
        GameObject explosionGameObject = Instantiate(fxExplosion, transform);
        SuperGastonMiauffinExplosion explosionParticles = explosionGameObject.GetComponent<SuperGastonMiauffinExplosion>();
        explosionParticles.Play();

        MasterSFXPlayer._player.PlayOneShot(sfxSignal);
        MasterSFXPlayer._player.PlayOneShot(sfxTransformation);

        animator.speed = originalSpeed;
        pjRigidbody.velocity = new Vector2(0, verticalForce);
        pjRigidbody.bodyType = RigidbodyType2D.Kinematic;
        Shake._this.ShakeIt();
    }

    // Detener el salto del PJ
    private void StopJumpFX()
    {
        // Detener el salto
        pjRigidbody.velocity = new Vector2(0, 0);

        // Mover a la posicion de ataque
        character.transform.position = new Vector2(character.transform.position.x + ( moveXForce * flippedMultiplier), character.transform.position.y);

        // Cambiar la animacion y detenerla
        animator.speed = 0;
        character.ChangeState(InGameStates.SuperLoop);
    }

    // Iniciar la caida del PJ
    private void StartFall()
    {
        pjRigidbody.bodyType = RigidbodyType2D.Dynamic;

        pjRigidbody.sharedMaterial = spawnMaterial;
        pjRigidbody.gravityScale = spawnGravity;

        // Indicar al controlador que debe hacer el cambio de material al tocar plataforma
        character.AlterRigidBodyOnContact(spawnPostMaterial, spawnPostGravity);

        MasterSFXPlayer._player.PlayOneShot(sfxFall);

    }

    public override IEnumerator WinExecute()
    {
        // Realizar la ejecucion del ataque
        StartFall();

        // Ejecutar sonido de golpe fuerte utilizando el MasterSFX
        MasterSFXPlayer._player.StrongHit();

        // Reproducir segundo sonido
        //MasterSFXPlayer._player.PlayOneShot(sfxZaaaaaa);

        yield return new WaitForSeconds(timeToFall);
    }

    public override IEnumerator DrawExecute()
    {
        // Para este caso en particular, el super debe tener colision
        win = false;
        yield return StartCoroutine(WinExecute());
    }

    // Finalizacion del super
    public override void Finish()
    {
        StartCoroutine(Autodestruction());
    }

    private IEnumerator Autodestruction()
    {
        yield return new WaitForSeconds(timeToFinish);
        Destroy(gameObject);
    }

    // Animacion para transforma la camara segun el super
    public void TransformCamera(float cameraSize, float cameraRotation, float cameraX, float cameraChangesTime)
    {

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
        };

        System.Action<ITween<Vector3>> moveInComplete = (t) =>
        {
            cameraPositionOriginal = finalPosition;
            Camera.main.transform.position = new Vector3(finalPosition.x, finalPosition.y, -cameraOriginalZ);
        };

        // Acercar la camara
        Camera.main.gameObject.Tween(string.Format("ZoomIn{0}", Camera.main.GetInstanceID()), Camera.main.orthographicSize, cameraSize,
            cameraChangesTime, TweenScaleFunctions.QuadraticEaseInOut, zoomIn);

        // Girar la camara
        if (!CheckCamerasLike()) Camera.main.gameObject.Tween(string.Format("Rotate{0}", Camera.main.GetInstanceID()), initRotation, finaRotation,
            cameraChangesTime, TweenScaleFunctions.QuadraticEaseInOut, rotateIn);

        // Mover la camara
        Camera.main.gameObject.Tween(string.Format("Move{0}", Camera.main.GetInstanceID()), cameraPositionOriginal, finalPosition,
            cameraChangesTime, TweenScaleFunctions.QuadraticEaseInOut, moveIn, moveInComplete);
    }

    // Volver al personaje a su posicion original al rebotar luego del superataque
    public void BouceToOriginalPosition()
    {
        pjRigidbody.velocity = new Vector2( ( -moveXForce * flippedMultiplier ), moveYForce);
        animator.speed = originalSpeed;
    }

    // Finalizar la destransformacion del PJ
    public void FinishDeTranformation()
    {
        // Explosion para destransformar
        GameObject explosionGameObject = Instantiate(fxExplosion, transform);
        explosionGameObject.transform.position = character.transform.position;
        SuperGastonMiauffinExplosion explosionParticles = explosionGameObject.GetComponent<SuperGastonMiauffinExplosion>();
        explosionParticles.Play();
        MasterSFXPlayer._player.PlayOneShot(sfxTransformation);
    }

}