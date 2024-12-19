using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SuperJanKenUPGeneral : SuperJanKenUPBase
{

    [Header("Prepare Phase")]
    [SerializeField] [Range(0, 1f)] float timeToFadeIn = 0.5f;
    [SerializeField] [Range(0, 2f)] float timeToPrepare = 2f;
    [SerializeField] [Range(0, 2f)] float timeToExit = 0.5f;

    [Header("FX")]
    [SerializeField] Vector2 fxPosition = new Vector2();
    [SerializeField] Color fxColor = Color.white;
    [SerializeField] ParticleSystem fxCharge;
    [SerializeField] ParticleSystem fxAccumulation;

    [Header("SFX")]
    [SerializeField] AudioClip sfxSuperJan;
    [SerializeField] AudioClip sfxSuperKen;
    [SerializeField] AudioClip sfxSuperUp;

    // Resize de la camara que se usara para el super
    int cameraResizeFactor = 1;

    public override void Setup(CharacterInGameController character, Dictionary<string, object> data)
    {
        // Llamar a base
        base.Setup(character, data);

        // Guardar el resize de la camara y otros datos para la ejecucion del super
        if (data.ContainsKey("cameraResizeFactor")) cameraResizeFactor = Convert.ToInt32(data["cameraResizeFactor"]);
        if (data.ContainsKey("position")) fxPosition = (Vector2)data["position"];
        if (data.ContainsKey("color")) fxColor = (Color)data["color"];

    }

    public override IEnumerator Prepare()
    {
        // Asignar correctamente la camara
        transform.Find("Overlay").GetComponent<Canvas>().worldCamera = Camera.main;

        character.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Foreground);

        // Realizar el fadeIn del overlay y colocar al PJ seleccionado sobre el overlay
        ToggleCameraOverlay(true);
        character.ChangeState(InGameStates.Charge);
        yield return new WaitForSeconds(timeToFadeIn);

        // Generar efectos de carga sobre la zona designada por el pj
        StartCoroutine(ChargeFX());

        // Acercar la camara haciendo uso de InGameSequence
        InGameSequence inGameSequence = FindObjectOfType<InGameSequence>();
        inGameSequence.TransformCamera(1, cameraResizeFactor, 0, timeToPrepare, false);

        // Instanciar el efecto de carga
        yield return new WaitForSeconds(timeToPrepare);

        // Devolver al PJ a su sortingOrder y quitar el overlay
        ToggleCameraOverlay(false);
        yield return new WaitForSeconds(timeToExit);
        character.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Arena);
    }

    // Mostrar el efecto de carga sobre el personaje
    private IEnumerator ChargeFX()
    {
        // Generar efectos de carga
        GameObject fxChargeObject = Instantiate(fxCharge.gameObject, transform.position, Quaternion.identity) as GameObject;
        GeneralParticle fxChargeCopy = fxChargeObject.GetComponent<GeneralParticle>();
        GameObject fxAccumulationObject = Instantiate(fxAccumulation.gameObject, transform.position, Quaternion.identity) as GameObject;
        GeneralParticle fxAccumulationCopy = fxAccumulationObject.GetComponent<GeneralParticle>();
        fxChargeCopy.SetLocalPosition(character.transform, fxPosition);
        fxChargeCopy.SetColor(fxColor);
        fxAccumulationCopy.SetLocalPosition(character.transform, fxPosition);
        fxAccumulationCopy.SetColor(fxColor);

        // Siempre mantener una scala positiva
        fxChargeObject.transform.localScale = new Vector2(1, 1);
        fxAccumulationObject.transform.localScale = new Vector2(1, 1);

        // Ejecutar los efectos de carga
        fxChargeCopy.Play();
        fxAccumulationCopy.Play();

        // Reproducir primer sonido
        MasterSFXPlayer._player.PlayOneShot(sfxSuperJan);

        yield return new WaitForSeconds(timeToPrepare / 2);

        // Reproducir segundo sonido
        MasterSFXPlayer._player.PlayOneShot(sfxSuperKen);

        yield return new WaitForSeconds(timeToPrepare / 2);
    }

    public override IEnumerator WinExecute()
    {
        // Realizar la ejecucion del ataque
        character.ChangeState(InGameStates.Attack);

        // Ejecutar sonido de golpe fuerte utilizando el MasterSFX
        MasterSFXPlayer._player.PlayOneShot(sfxSuperUp);
        MasterSFXPlayer._player.StrongHit();

        yield return null;
    }

    public override IEnumerator DrawExecute()
    {
        // Para este caso en particular, el ataque al ganar se comporta igual que el ataque al empatar
        yield return StartCoroutine(WinExecute());
    }

    // Acciones posteriores a la ejecucion del super
    public override IEnumerator PostSuper()
    {
        character.ChangeState(InGameStates.Wait);
        yield return null; 
    }

}