using DigitalRuby.Tween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class InGameSequence : MonoBehaviour
{
    [Header("Actors")]
    [SerializeField] GameObject referee;
    [SerializeField] GameObject leftPlayer;
    [SerializeField] GameObject rightPlayer;
    [SerializeField] ArenaController arena;

    [Header("Timing")]
    [SerializeField] [Range(0, 2)] float charge = 1f;
    [SerializeField] [Range(0, 2)] float refereeAttack = 0.25f;
    [SerializeField] [Range(0, 2)] float playersAttack = 0.25f;
    [SerializeField] [Range(0, 2)] float wait = 0.25f;
    [SerializeField] [Range(0, 2)] float refereeResult = 0.5f;
    [SerializeField] [Range(0, 2)] float playersResult = 1;
    [SerializeField] [Range(0, 2)] float JumpEveryBodyMinusTime = 0.25f;

     [Header("FX")]
    [SerializeField] GameObject FXExplosionDust;
    [SerializeField] float explosionDustShakeMagnitude = 2f;
    [SerializeField] GameObject FXAnimeSpeedLines;

    [Header("Force")]
    [SerializeField] [Range(0f, 5f)] float simplePunchMinForce = 2f;
    [SerializeField] [Range(0f, 5f)] float simplePunchMaxForce = 2.5f;
    [SerializeField] [Range(0f, 5f)] float strongPunchMinForce = 3f;
    [SerializeField] [Range(0f, 5f)] float strongPunchMaxForce = 5f;
    [SerializeField] [Range(0f, 5f)] float superJanKenUpMagnitude = 5f;

    [Header("Camera")]
    [SerializeField] [Range(0f, 2f)] float cameraSizeDelta = .8f;
    [SerializeField] [Range(0f, 180f)] float cameraRotationAngle = 10;
    [SerializeField] [Range(0f, 1f)] float cameraChangesTime = .1f;
    float cameraSizeOriginal = 0;
    int totalSpecialCameraTypes = 0;
    SpecialCameraType currentSpecialCameraType = SpecialCameraType.ZoomInRotation;

    [Header("Toing")]
    [SerializeField] [Range(0f, 180f)] float toingRotationAngle = 15;
    [SerializeField] [Range(0f, 2f)] float toingRotationFatorDelta = 0.2f;
    [SerializeField] [Range(0f, 2f)] float toingTimeToRotate = .05f;

    [Header("TimeScale")]
    [SerializeField] float timeNormalScale = 1;

    // Controladores de juego
    SingleModeController smc;

    // Componentes recurrentes
    Referee refereeComponent;
    CharacterInGameController leftActorComponent;
    CharacterInGameController rightActorComponent;
    Rigidbody2D refereeRigidBody;
    Rigidbody2D leftActorRigidBody;
    Rigidbody2D rightActorRigidBody;

    // Fuerza aplicada al atacar
    int forceFactor = 1;

    // Indicador de super
    bool superJankenUpActive = false;

    void Start()
    {
        refereeComponent = referee.GetComponent<Referee>();
        refereeRigidBody = referee.GetComponent<Rigidbody2D>();
        SetActors(leftPlayer, rightPlayer);
        arena = FindObjectOfType<ArenaController>();
        cameraSizeOriginal = Camera.main.orthographicSize;
        totalSpecialCameraTypes = Enum.GetNames(typeof(SpecialCameraType)).Length;

        // Controlador de scena
        smc = GetComponent<SingleModeController>();

        int playerPosition = PlayerPrefs.GetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, PlayersPreferencesController.CONTROLPOSITIONKEYDEFAULT);
        if (playerPosition == 1) forceFactor = -1;
    }

    /// <summary>
    /// Cambio en los actores en la escena. Sirve ademas para intercambiar informacion de su rival
    /// </summary>
    /// <param name="leftActor"></param>
    /// <param name="rightActor"></param>
    public void SetActors(GameObject leftActor, GameObject rightActor)
    {
        SetLeftActor(leftActor);
        SetRightActor(rightActor);

        // Indicar a cada actor su oponente
        this.SetOpponents();
    }

    public void SetLeftActor(GameObject leftActor)
    {
        this.leftPlayer = leftActor;
        leftActorComponent = leftActor.GetComponent<CharacterInGameController>();
        leftActorComponent.ChangeState(InGameStates.Stand);
        leftActorRigidBody = leftActor.GetComponent<Rigidbody2D>();
    }

    public void SetRightActor(GameObject rightActor)
    {
        this.rightPlayer = rightActor;
        rightActorComponent = rightActor.GetComponent<CharacterInGameController>();
        rightActorComponent.ChangeState(InGameStates.Stand);
        rightActorRigidBody = rightActor.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Indicar a cada actor su actual oponente
    /// </summary>
    public void SetOpponents()
    {
        if (!rightActorComponent || !rightActorComponent) return;
        leftActorComponent.SetOpponent(rightActorComponent.GetIdentifier());
        rightActorComponent.SetOpponent(leftActorComponent.GetIdentifier());
    }

    // Rutina de muestra y despliegue de ataque
    public IEnumerator GameCourutine()
    {
        Shake shake = FindObjectOfType<Shake>();

        // Indicar que estan en carga
        refereeComponent.ChangeState(InGameStates.Charge);
        leftActorComponent.ChangeState(InGameStates.Charge);
        rightActorComponent.ChangeState(InGameStates.Charge);
        yield return new WaitForSeconds(charge);

        // Realizar la animación del referee
        refereeComponent.ChangeState(InGameStates.Attack);
        yield return new WaitForSeconds(refereeAttack);

        if (shake) shake.ShakeIt();
        VibrationController.Instance.Vibrate();

        leftActorComponent.ChangeState(InGameStates.Attack);
        rightActorComponent.ChangeState(InGameStates.Attack);
        yield return new WaitForSeconds(playersAttack);

        if (shake) shake.StopIt();

        // Cambiar los estados a pausa
        refereeComponent.ChangeState(InGameStates.Wait);
        leftActorComponent.ChangeState(InGameStates.Wait);
        rightActorComponent.ChangeState(InGameStates.Wait);
        yield return new WaitForSeconds(wait);

    }


    // Rutina de muestra y despliegue de ataque parcelada. Debe ser complementada con FinishParcialGameRoutine
    public IEnumerator ParcialGameCourutine(float timeScale)
    {
        // Cambiar la escala de tiempo actual
        Time.timeScale = timeScale;

        // Indicar que estan en carga
        refereeComponent.ChangeState(InGameStates.Charge);
        leftActorComponent.ChangeState(InGameStates.Charge);
        rightActorComponent.ChangeState(InGameStates.Charge);
        yield return new WaitForSeconds(charge);

        // Realizar la animación del referee
        refereeComponent.ChangeState(InGameStates.Attack);
        yield return new WaitForSeconds(refereeAttack);

    }

    // Sobrecarga del metodo para indicar que debe mantener la escala de tiempo normal
    public IEnumerator ParcialGameCourutine()
    {
        yield return ParcialGameCourutine(timeNormalScale);
    }

    // Finalizacion de la rutina parcial de ataque
    public IEnumerator FinishParcialGameRoutine(bool strong, InGameStates result, int cameraStep) {

        // Si es el primer paso, calcular cual sera el efecto para la camara
        if(cameraStep == 1)
        {
            switch( Random.Range(0,totalSpecialCameraTypes))
            {
                case 1:
                    currentSpecialCameraType = SpecialCameraType.ZoomOut;
                    break;
                case 0:
                default:
                    currentSpecialCameraType = SpecialCameraType.ZoomInRotation;
                    break;
            }
        }

        // Si el juego actual tiene una rotacion de camara para dar efectos, solo usar el ZoomOut
        CameraLike[] camerasLike = FindObjectsOfType<CameraLike>();
        if(camerasLike.Length > 0)
        {
            foreach(CameraLike cameraLike in camerasLike)
            {
                if (cameraLike.gameObject.activeSelf && cameraLike.HasRotation())
                {
                    currentSpecialCameraType = SpecialCameraType.ZoomOut;
                    break;
                }
            }
        }

        Shake shake = FindObjectOfType<Shake>();

        if (shake) shake.ShakeIt(strong ? explosionDustShakeMagnitude : 0);
        VibrationController.Instance.Vibrate();

        // Realizar acercamiento y rotacion de la camara
        int cameraSizeFactor = cameraStep % 3 == 0 ? 0 : 1;
        int cameraRotationFactor = 0;
        switch (cameraStep % 3)
        {
            case 1:
                cameraRotationFactor = 1;
                break;
            case 2:
                cameraRotationFactor = -1;
                break;
        }

        // Revisar si es una camara por pasos o no
        if(cameraStep > 0)
        {
            // Definir los parametros segun el tipo de camara especial usada
            int postCameraStep = cameraStep;
            int postCameraRotationFactor = cameraRotationFactor;

            switch (currentSpecialCameraType)
            {
                case SpecialCameraType.ZoomOut:
                    postCameraStep = Mathf.Abs(-3 + cameraStep) * 2;
                    postCameraRotationFactor = 0;
                    break;
            }
        
            // Indicar que se quiere hacer la transformacion de la camara
            TransformCamera(postCameraStep, cameraSizeFactor, postCameraRotationFactor, cameraChangesTime,true);
        }

        // Si es una partida fuerte
        if (strong)
        {
            // Crear un efecto de anime
            CreateAnimeSpeedFX();

            // En caso de no haber pasos de camara, crear un segundo efecto directamente
            if (cameraStep == 0) CreateAnimeSpeedFX();

            CharacterInGameController target = result.Equals(InGameStates.Win) ? rightActorComponent : leftActorComponent;
            DissapearCharacter(target);
            
        }

        // Hacer saltar a los actores y a la multitud
        float force = strong ? Random.Range(strongPunchMinForce, strongPunchMaxForce) : Random.Range(simplePunchMinForce, simplePunchMaxForce);
        if(refereeRigidBody) refereeRigidBody.velocity = new Vector2(0, force);

        if (!strong)
        {
            switch (result)
            {
                case InGameStates.Win:
                    force = Random.Range(simplePunchMinForce, simplePunchMaxForce);
                    if(rightActorRigidBody) rightActorRigidBody.velocity = new Vector2(force * forceFactor, 0);
                    if(rightActorComponent) rightActorComponent.CreateDustHit(1 * forceFactor);
                    break;
                case InGameStates.Lose:
                    force = Random.Range(simplePunchMinForce, simplePunchMaxForce);
                    if(leftActorRigidBody) leftActorRigidBody.velocity = new Vector2(-force * forceFactor, 0);
                    if(leftActorComponent) leftActorComponent.CreateDustHit(-1 * forceFactor);
                    break;
                case InGameStates.Draw:
                    force = Random.Range(simplePunchMinForce, simplePunchMaxForce);
                    if(leftActorRigidBody) leftActorRigidBody.velocity = new Vector2(0, force);
                    if(rightActorRigidBody) rightActorRigidBody.velocity = new Vector2(0, force);
                    break;
            }
        }
        if(arena == null) arena = FindObjectOfType<ArenaController>();
        arena.Jump(strong? 2 : 1);

        // Ver si se debe aplicar la gravedad a los ataques
        if (leftActorComponent && leftActorComponent.GetCurrentAttack() == Attacks.MagicWand) rightActorComponent.CurrentAttackActivateGravity();
        if (rightActorComponent && rightActorComponent.GetCurrentAttack() == Attacks.MagicWand) leftActorComponent.CurrentAttackActivateGravity();

        // Cambiar de estado
        if(leftActorComponent) leftActorComponent.ChangeState(InGameStates.Attack);
        if(rightActorComponent) rightActorComponent.ChangeState(InGameStates.Attack);
        yield return new WaitForSeconds(playersAttack);

        if (shake) shake.StopIt();

        // Cambiar los estados a pausa
        if(refereeComponent) refereeComponent.ChangeState(InGameStates.Wait);
        if(leftActorComponent) leftActorComponent.ChangeState(InGameStates.Wait);
        if(rightActorComponent) rightActorComponent.ChangeState(InGameStates.Wait);
        yield return new WaitForSeconds(wait);

        // Cambiar la escala de tiempo normal
        Time.timeScale = timeNormalScale;

    }

    public IEnumerator ResultCourutine(InGameStates result, InGameStates refereeState)
    {
        // Definir los estados de cada actor
        if (result.Equals(InGameStates.Draw))
        {
            // Solo es necesario que el referee indique que hubo empate
            refereeComponent.ChangeState(InGameStates.Draw);
            yield return new WaitForSeconds(playersResult);

            // Cambiar al estado stand
            ResetToStand();
        }
        else {

            // El leftPlayer y el referee comparten el mismo estado
            refereeComponent.ChangeState(refereeState);
            yield return new WaitForSeconds(refereeResult);

            leftActorComponent.ChangeState(result);
            rightActorComponent.ChangeState(result.Equals(InGameStates.Win)?
                InGameStates.Lose : InGameStates.Win);
        }

    }

    public IEnumerator FinalWait()
    {
        yield return new WaitForSeconds(playersResult);
    }

    // Todos los actores vuelven a su animación de stand
    public void ResetToStand()
    {
        refereeComponent.ChangeState(InGameStates.Stand);
        leftActorComponent.ChangeState(InGameStates.Stand);
        rightActorComponent.ChangeState(InGameStates.Stand);
    }

    // Animacion para transforma la camara
    public void TransformCamera(int cameraStep, int cameraSizeFactor, int cameraRotationFactor, float cameraChangesTime, bool animeLines)
    {

        // Cambiar tamano de la camara
        System.Action<ITween<float>> zoomIn = (t) =>
        {
            Camera.main.orthographicSize = t.CurrentValue;
        };

        // Cambiar rotacion de la camara
        System.Action<ITween<Vector3>> rotateIn = (t) =>
        {
            Camera.main.transform.eulerAngles = t.CurrentValue;
        };

        // Tamano y rotacion final
        float finalSize = cameraSizeOriginal - (cameraSizeDelta * cameraStep * cameraSizeFactor);
        Vector3 rotation = new Vector3(0, 0, cameraRotationAngle * cameraRotationFactor);
        Vector3 initRotation = new Vector3(0, 0, 0);
        Vector3 finaRotation = initRotation - rotation;

        // Acercar la camara
        Camera.main.gameObject.Tween(string.Format("ZoomIn{0}", Camera.main.GetInstanceID()), Camera.main.orthographicSize, finalSize,
            cameraChangesTime, TweenScaleFunctions.QuadraticEaseInOut, zoomIn);

        // Girar la camara
        Camera.main.gameObject.Tween(string.Format("Rotate{0}", Camera.main.GetInstanceID()), initRotation, finaRotation,
            cameraChangesTime, TweenScaleFunctions.QuadraticEaseInOut, rotateIn);

        // Mostrar lineas de anime
        if(animeLines)
        {
            CreateAnimeSpeedFX();
        }
    }

    // Creacion de un efecto de velocidad/enfasis
    public void CreateAnimeSpeedFX()
    {
        AnimeSpeedParticle animeSpeedParticle = (Instantiate(FXAnimeSpeedLines)).GetComponent<AnimeSpeedParticle>();
        animeSpeedParticle.Play();
    }

    // Desaparicion de un personaje
    public void DissapearCharacter(CharacterInGameController target)
    {
        if (!target) return;

        GameObject dustObject = Instantiate(FXExplosionDust, target.transform.position, Quaternion.identity) as GameObject;
        DustParticle dustCopy = dustObject.GetComponent<DustParticle>();

        // Aplicar variaciones segun la velocidad del cuerpo
        dustCopy.SetXScaleMultiplier((int)target.transform.localScale.x);

        dustObject.transform.localScale = new Vector3(
            target.transform.localScale.x,
            target.transform.localScale.y,
            target.transform.localScale.z
            );

        dustCopy.Play();

        target.Disappear();
    }

    /* Ejecucion del super, en base a los datos enviados por algun controlador de escena
     * Parametros:
     * @charaterOne         : Controlador para uno de los jugadores. 
     * @charaterTwo         : Controlador para el otro jugador.
     * @characterOneSuper   : Indica si el primer personaje debe ejecutar su super
     * @characterTwoSuper   : Indica si el segundo personaje debe ejecutar su super
     * @byeByeReferee       : Ver si referee debe desaparecer de la plataforma
     */
    public IEnumerator ExecuteSuperJanKenUP(CharacterInGameController characterOne, CharacterInGameController characterTwo,
        bool characterOneSuper, bool characterTwoSuper, bool byeByeReferee, bool showResults = true)
    {
        superJankenUpActive = true;

        // Establecer que jugador debe ejecutar su super
        if (characterOneSuper && characterTwoSuper) yield return StartCoroutine(ExecuteSuperBothSides(characterOne));
        else yield return StartCoroutine(ExecuteSuperOneSide(characterOneSuper? characterOne : characterTwo, byeByeReferee, showResults));

        superJankenUpActive = false;
    }

    /* Ejecucion del super por parte de uno de los personajes
     * Parametros:
     * @charater            : Se debe contrastar con los actores en escena para saber cual es el personaje que ejecutara el super
     * @byeByeReferee       : Ver si referee debe desaparecer de la plataforma
     */
    public IEnumerator ExecuteSuperOneSide(CharacterInGameController character, bool byeByeReferee, bool showResult = true)
    {
        // Primero, realizar la preparacion del super del personaje
        yield return character.PrepareSuper(1, true, byeByeReferee);

        // Ejecutar el super como victoria
        yield return character.ExecuteSuperWin();

        // Se ejecuta la vibracion y las particulas de enfasis tipo anime
        Shake shake = FindObjectOfType<Shake>();
        CreateAnimeSpeedFX();
        if (shake) shake.ShakeIt(superJanKenUpMagnitude);
        VibrationController.Instance.Vibrate();

        // Hacer desaparecer al adversario
        DissapearCharacter(character == leftActorComponent ? rightActorComponent : leftActorComponent);

        // Hacer desaparecer al arbitro en caso de que se haya especificado
        if (byeByeReferee) refereeComponent.PlayDisappear(-character.transform.localScale.x);
        else
        {
            float force = Random.Range(strongPunchMinForce, strongPunchMaxForce);
            refereeRigidBody.velocity = new Vector2(0, force);
        }

        // Realizar el salto de toda la arena
        if (arena == null) arena = FindObjectOfType<ArenaController>();
        arena.Jump(3);

        // Volver camara a tamano original y esperar el tiempo indicado de ataque
        yield return new WaitForSeconds(playersAttack);

        // Detener el shake y volver a los estados de espera
        if (shake) shake.StopIt();
        yield return StartCoroutine(character.PostSuper());
        yield return new WaitForSeconds(wait);
        yield return new WaitForSeconds(refereeResult);

        // Volver camara a su estado original
        TransformCamera(0, 0, 0, cameraChangesTime, false);

        // Hacer ganar al jugador
        if (showResult) character.ChangeState(InGameStates.Win);
        
        // Eliminar super
        character.FinishSuper();
    }

    /* Ejecucion del super por parte de uno de los dos personajes */
    public IEnumerator ExecuteSuperBothSides(CharacterInGameController firstCharacter)
    {
        // El personaje enviado determina el primero en tirar la jugada
        CharacterInGameController characterOne = firstCharacter;
        CharacterInGameController characterTwo = firstCharacter == leftActorComponent ? rightActorComponent : leftActorComponent;

        // Primero, realizar la preparacion del super de los personajes
        yield return characterOne.PrepareSuper(1,false, false);
        yield return characterTwo.PrepareSuper(2,false, false);

        // Ejecutar el super como empate
        yield return characterOne.ExecuteSuperDraw();
        yield return characterTwo.ExecuteSuperDraw();

        // Se ejecuta la vibracion y las particulas de enfasis tipo anime
        Shake shake = FindObjectOfType<Shake>();
        CreateAnimeSpeedFX();
        if (shake) shake.ShakeIt(superJanKenUpMagnitude);
        VibrationController.Instance.Vibrate();

        // Realizar el salto de toda la arena
        if (arena == null) arena = FindObjectOfType<ArenaController>();
        arena.Jump(3);

        yield return new WaitForSeconds(playersAttack);

        if (shake) shake.StopIt();

        // Cambiar los estados a pausa
        StartCoroutine(characterOne.PostSuper());
        StartCoroutine(characterTwo.PostSuper());

        // Volver camara a tamano original
        TransformCamera(0, 0, 0, cameraChangesTime, false);

        // Realizar la animacion del referee rebotando
        yield return StartCoroutine(RefereeToing());

        // Solo es necesario que el referee indique que hubo empate
        refereeComponent.ChangeState(InGameStates.Draw);
        yield return new WaitForSeconds(playersResult);

        // Cambiar al estado stand
        ResetToStand();

        // Eliminar supers
        characterOne.FinishSuper();
        characterTwo.FinishSuper();
    }

    // Animacion del referee rebotando de lado a lado
    private IEnumerator RefereeToing()
    {
        float toingRotationFactor = 1;
        float toingOriginalTimeToRotate = toingTimeToRotate;
        float step = 0;

        // Desactivar el RigidBody del arbitro
        refereeRigidBody.bodyType = RigidbodyType2D.Static;

        // Cambio de rotacion
        System.Action<ITween<Vector3>> toingMe = (t) =>
        {
            referee.transform.eulerAngles = t.CurrentValue;
        };

        // Reproducir el sonido de rebote
        MasterSFXPlayer._player.Toing();

        while (toingRotationFactor > 0)
        {

            Vector3 rotation = new Vector3(0, 0, toingRotationAngle * (step % 2 == 0 ? (step == 0 ? 1 : 2) : -2));
            Vector3 initRotation = referee.transform.eulerAngles;
            Vector3 finaRotation = initRotation - rotation;

            // Que ataque aparezca
            referee.Tween(string.Format("Toing{0}", referee.GetInstanceID()), initRotation, finaRotation,
                toingTimeToRotate, TweenScaleFunctions.QuadraticEaseInOut, toingMe);

            yield return new WaitForSeconds(toingTimeToRotate);

            // Realizar los cambios para reducir la rotacion y aumentar el tiempo de rebote
            step++;
            toingRotationFactor -= toingRotationFatorDelta;
            toingTimeToRotate = toingOriginalTimeToRotate * step;
        }

        // Desactivar el RigidBody del arbitro
        refereeRigidBody.bodyType = RigidbodyType2D.Dynamic;
        toingTimeToRotate = toingOriginalTimeToRotate;

    }


    // Metodo que hace saltar a los actores menos al que se haya pasado como parametro
    public IEnumerator JumpEverybodyMinus(CharacterInGameController excluded, bool stopIt = true, float shakeMagnitude = -1)
    {
        // Realizar un shake
        if(shakeMagnitude >= 0) Shake._this.ShakeIt( shakeMagnitude );
        else Shake._this.ShakeIt();

        // Hacer saltar al referee
        float force = Random.Range(simplePunchMinForce, simplePunchMaxForce);

        // Solo hacer saltar si su velocidad es 0
        if (refereeRigidBody.velocity.y == 0)  refereeRigidBody.velocity = new Vector2(0, force);
        

        // Hacer saltar uno de los actores
        force = Random.Range(simplePunchMinForce, simplePunchMaxForce);
        if (excluded == leftActorComponent)
        {
            if (rightActorRigidBody.velocity.y == 0) rightActorRigidBody.velocity = new Vector2(0, force);
        }
        else
        {
            if (leftActorRigidBody.velocity.y == 0) leftActorRigidBody.velocity = new Vector2(0, force);
        }

        yield return new WaitForSeconds(JumpEveryBodyMinusTime);

        if(stopIt) Shake._this.StopIt();

    }


    /// <summary>
    /// Indica si se esta ejecutando un super
    /// </summary>
    /// <returns></returns>
    public bool IsSuperJankenUpActive()
    {
        return superJankenUpActive;
    }
}
