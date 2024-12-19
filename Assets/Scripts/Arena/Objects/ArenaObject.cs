using DigitalRuby.Tween;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArenaObject : MonoBehaviour
{
    [Header("Prize")]
    [SerializeField] bool hasPrize = false;
    [SerializeField] bool randomPrize = false;
    [SerializeField] SuperPowers defaultPrize = SuperPowers.TimeMaster;
    [SerializeField] float timeToFadePostPrize = 0.5f;

    [Header("FX")]
    [SerializeField] ParticleSystem dust;
    [SerializeField] Color dustColor = Color.white;
    [SerializeField] bool useObjectColor = true;
    [SerializeField] bool generateDust = true;

    [Header("SFX")]
    [SerializeField] bool onClickEmitSound;
    [SerializeField] AudioClip onClickAudioClip;

    [Header("OnJump")]
    [SerializeField] bool horizontalJump = false;
    [SerializeField] bool torqueOnJump = false;
    [SerializeField] bool shake = false;
    [SerializeField] bool rotate = false;
    [SerializeField][Range(0,1)] float shakeTime = 0.01f;
    [SerializeField][Range(0,100)] float torqueMax = 5f;
    [SerializeField][Range(0,100)] float rotateMax = 5f;

    [Header("Children")]
    [SerializeField] ArenaObject[] nestedArenaObjects;

    // Utiles
    SpriteRenderer spriteRenderer;
    Rigidbody2D rigidBody;
    int sortingOrder = 0;
    int horizontalJumpReducerMax = 2;
    bool prizeGiven = false;
    bool soundInEmission = false;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer) sortingOrder = spriteRenderer.sortingOrder + 1;
        GetRigidBody();
    }

    // Efecto de particulas al entrar en colision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GetRigidBody();
        if (!rigidBody || !generateDust) return;

        GameObject dustObject = Instantiate(dust.gameObject, transform.position, Quaternion.identity) as GameObject;
        DustParticle dustCopy = dustObject.GetComponent<DustParticle>();

        // Aplicar variaciones segun la velocidad del cuerpo
        dustCopy.SetSpeed(rigidBody.velocity.y);
        dustCopy.SetSorting(sortingOrder);
        dustCopy.SetSortingLayer(JankenUp.SpritesSortingLayers.ArenaBackground);
        if(spriteRenderer) dustCopy.SetColor(useObjectColor? spriteRenderer.color : dustColor);
        dustCopy.Play();

    }

    /// <summary>
    /// Permite aplicar una fuerza vertical al objeto, dando el efecto de un salto
    /// </summary>
    /// <param name="minForce"></param>
    /// <param name="maxForce"></param>
    /// <param name="multiplier"></param>
    public virtual void Jump(float minForce, float maxForce, int multiplier)
    {
        if (shake)
        {
            Shake._this.ShakeThis(gameObject, shakeTime);
        }
        else if (rotate)
        {
            transform.Rotate(0, 0, rotateMax);
        }
        else
        {
            GetRigidBody();
            if (!rigidBody) return;

            if (rigidBody.bodyType != RigidbodyType2D.Kinematic)
            {
                float forceY = Random.Range(minForce * multiplier, maxForce * multiplier);
                float forceX = horizontalJump ? (Random.Range(minForce * multiplier, maxForce * multiplier) / horizontalJumpReducerMax) : 0;
                float torque = torqueOnJump ? (Random.Range(0, torqueMax * multiplier)) : 0;

                // Si hay salto horizontal, cambiar su direccion
                if (horizontalJump)
                {
                    forceX *= (Random.Range(0, 2) == 1 ? 1 : -1);
                }

                rigidBody.velocity = new Vector2(forceX, forceY);
                if (torqueOnJump) rigidBody.AddTorque(torque); 
            }

            // Revision de saltos para objetos hijos
            if (nestedArenaObjects != null && nestedArenaObjects.Length > 0)
            {
                foreach (ArenaObject arenaObject in nestedArenaObjects)
                {
                    if (arenaObject) arenaObject.Jump(minForce, maxForce, multiplier);
                }
            }
        }
        
    }

    /// <summary>
    /// Obtencion del RigidBody2D en caso de no estar listo
    /// </summary>
    private void GetRigidBody()
    {
        if (!rigidBody) rigidBody = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// En caso de ser presionado, revisar si cuenta con la posibilidad de dar un premio.
    /// </summary>
    public virtual void OnMouseDown()
    {
        if (onClickEmitSound) EmitSound();
        if (!hasPrize) return;
        if (prizeGiven) return;

        // De momento, solo se considera esta mecanica para el modo offline
        SingleModeSession sms = FindObjectOfType<SingleModeSession>();
        if (!sms) return;

        // Solo si el jugador conoce los superpoderes
        bool playerKnowsSuperPowers = GameController.LoadTutorials().tutorials.Contains(Tutorial.GetSuperPowerLevel());
        if (!playerKnowsSuperPowers) return;

        // Indicar de inmediato que el premio fue dado
        prizeGiven = true;

        // Detectar el premio a dar
        SuperPowers prize = randomPrize ? sms.GetSuperPower() : defaultPrize;
        StartCoroutine(sms.AnimateSuperPowerEarned((int) prize, false));

        // Iniciar la rutina para desaparecer
        StartCoroutine(FadeAndDestroy());
    }

    /// <summary>
    /// Rutina que esconde y luego elimina el objeto en cuestion
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeAndDestroy()
    {
        // Mover las ilustraciones
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if (spriteRenderer) spriteRenderer.color = t.CurrentValue;
        };

        Color clearWhite = new Color(1, 1, 1, 0);

        // Hacer fade in y mostrar elemento
        gameObject.Tween(string.Format("FadeAndDestroy_{0}", GetInstanceID()), Color.white, clearWhite, timeToFadePostPrize, TweenScaleFunctions.QuadraticEaseOut, updateColor);

        yield return new WaitForSeconds(timeToFadePostPrize * 1.1f);
        if(gameObject) Destroy(gameObject);
    }

    /// <summary>
    /// Emision de sonido si ha pasado el tiempo suficiente
    /// </summary>
    private void EmitSound() {
        if (!onClickEmitSound || onClickAudioClip == null || soundInEmission) return;
        MasterSFXPlayer._player.PlayOneShot(onClickAudioClip);
        soundInEmission = true;
        StartCoroutine(ResetOnClickSFX());
    }

    /// <summary>
    /// Resetear la posibilidad de emitir un sonido
    /// </summary>
    /// <returns></returns>
    private IEnumerator ResetOnClickSFX()
    {
        yield return new WaitForSeconds(onClickAudioClip.length);
        soundInEmission = false;
    }

    /// <summary>
    /// Volver a la normalidad variables necesarias
    /// </summary>
    private void OnEnable()
    {
        soundInEmission = false;
    }
}