using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EsticController : CharacterInGameController
{
    [Header("Sprites Renderers")]
    [SerializeField] SpriteRenderer[] spriteRenderers;

    [Header("FX")]
    [SerializeField] GameObject winDustPrefab;

    [Header("SFX")]
    [SerializeField] AudioClip[] monsterGrowl;
    [SerializeField] AudioClip[] magicCast;
    [SerializeField] AudioClip hitTheFloorSFX;

    [Header("HitTheFloor")]
    [SerializeField] [Range(0, 10)] float shakeMagnitude = 0.2f;

    // Componentes
    InGameSequence inGameSequence;
    DustParticle winDustParticle;

    // Obtener el spriteRendered asociado al body del personaje
    protected override void CheckSpriteRendered()
    {
        if (bodyRenderer) return;
        bodyRenderer = transform.Find("Parts").Find("Body").GetComponent<SpriteRenderer>();
    }

    /* Preparacion del super ataque
     * Parametros:
     * @cameraScale : Factor de zoom para la camara durante la preparacion del super
     */
    public override IEnumerator PrepareSuper(int cameraResizeFactor, bool win = true, bool byeByeReferee = true)
    {

        GameObject instance = Instantiate(superJanKenUPPrefab, transform.position, Quaternion.identity) as GameObject;
        superAttack = null;
        superAttack = instance.GetComponent<SuperEstic>();

        // Configurar el super
        Dictionary<string, object> data = new Dictionary<string, object>();
        data.Add("cameraResizeFactor", cameraResizeFactor);
        data.Add("byeByeReferee", byeByeReferee);
        data.Add("win", win);
        superAttack.Setup(this, data);

        // Activar el poder
        superJanKenUpActive = true;

        // Preparar el super
        yield return superAttack.Prepare();
    }

    // Terminar una ejecucion del super ataque que se gano
    public override IEnumerator ExecuteSuperWin()
    {
        if (superAttack) yield return superAttack.WinExecute();
    }

    // Terminar una ejecucion del super ataque que se empato
    public override IEnumerator ExecuteSuperDraw()
    {
        if (superAttack) yield return superAttack.DrawExecute();
    }

    // Eliminacion del super prefab
    public override void FinishSuper()
    {
        if (superAttack) superAttack.Finish();
        superJanKenUpActive = false;
    }

    /// <summary>
    /// Cambio de la capa de orden para los spriteRenderer
    /// </summary>
    /// <param name="layerName"></param>
    public override void ChangeSpritesLayerTo(string layerName)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingLayerID = SortingLayer.NameToID(layerName);
        }
    }

    /// <summary>
    /// Llamada posterior a la realizacion del super
    /// </summary>
    /// <returns></returns>
    public override IEnumerator PostSuper()
    {
        if (superAttack) yield return superAttack.PostSuper();
    }

    /// <summary>
    /// Creacion de polvo previo a aparicion del monstruo
    /// </summary>
    public void WinDust()
    {
        if (winDustParticle) Destroy(winDustParticle.gameObject);
        winDustParticle = Instantiate(winDustPrefab, transform).GetComponent<DustParticle>();
        winDustParticle.Play();
        AudioClip magicCastClip = magicCast[UnityEngine.Random.Range(0, magicCast.Length)];
        MasterSFXPlayer._player.PlayOneShot(magicCastClip);
        Shake._this.ShakeIt();
    }

    /// <summary>
    /// Aparicion del monstruo interdimensional 
    /// </summary>
    public void ShowMonster()
    {
        ChangeState(InGameStates.WinLoop);
        AudioClip monsterClip = monsterGrowl[UnityEngine.Random.Range(0, monsterGrowl.Length)];
        MasterSFXPlayer._player.PlayOneShot(monsterClip);
    }

    /// <summary>
    /// Celebracion en loop
    /// </summary>
    public void FinalWinState()
    {
        Shake._this.StopIt();
        ChangeState(InGameStates.WinLoop2);
    }

    /// <summary>
    /// Metodo llamado al animar la perdida en un match. Debe indicarle al InGameSequence que anime un saltito de los otros actores
    /// </summary>
    public void HitTheFloor()
    {
        // Comprobar que el personaje este activo
        if (isDisappeared) return;

        // Indicar al InGameSequence que deben saltar los otros PJ
        if (!inGameSequence) inGameSequence = FindObjectOfType<InGameSequence>();
        if (inGameSequence) StartCoroutine(inGameSequence.JumpEverybodyMinus(this, true, shakeMagnitude));

    }

    /// <summary>
    /// Funcion a llamar para reproducir el sonido de golpe al piso
    /// </summary>
    public void HitTheFloorSFX()
    {
        // Reproducir el sonido
        if (MasterSFXPlayer._player) MasterSFXPlayer._player.PlayOneShot(hitTheFloorSFX);
    }

    /// <summary>
    /// Metodo llamado para indicar al super que debe terminar de ejecutar el golpe
    /// </summary>
    public void Yoyazo() {
        if (superAttack) (superAttack as SuperEstic).Yoyazo();
    }
}