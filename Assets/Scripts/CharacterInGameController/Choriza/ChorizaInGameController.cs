using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ChorizaInGameController : CharacterInGameController
{
    [Header("Sprites Renderers")]
    [SerializeField] SpriteRenderer[] spriteRenderers;
    [SerializeField] GameObject dialog;

    [Header("HitTheFloor")]
    [SerializeField] [Range(0, 10)] float shakeMagnitude = 0.2f;

    // Componentes
    InGameSequence inGameSequence;

    public override void Flip(int xFactor)
    {
        base.Flip(xFactor);
        dialog.transform.localScale = new Vector2(xFactor, 1);
    }

    // Obtener el spriteRendered asociado al body del personaje
    protected override void CheckSpriteRendered()
    {
        if (bodyRenderer) return;
        bodyRenderer = transform.Find("Parts").Find("Base").GetComponent<SpriteRenderer>();
    }

    /* Preparacion del super ataque
     * Parametros:
     * @cameraScale : Factor de zoom para la camara durante la preparacion del super
     */
    public override IEnumerator PrepareSuper(int cameraResizeFactor, bool win = true, bool byeByeReferee = true)
    {

        GameObject instance = Instantiate(superJanKenUPPrefab, transform.position, Quaternion.identity) as GameObject;
        superAttack = null;
        superAttack = instance.GetComponent<SuperChoriza>();

        // Configurar el super
        Dictionary<string, object> data = new Dictionary<string, object>();
        data.Add("cameraResizeFactor", cameraResizeFactor);
        data.Add("win", win);
        data.Add("byeByeReferee", byeByeReferee);
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

    // Cambio de la capa de orden para los spriteRenderer
    public override void ChangeSpritesLayerTo(string layerName)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingLayerID = SortingLayer.NameToID(layerName);
        }
    }

    // Llamada posterior a la realizacion del super
    public override IEnumerator PostSuper()
    {
        if (superAttack) yield return superAttack.PostSuper();
    }

    /// <summary>
    /// Metodo llamado al animar la perdida en un match. Debe indicarle al InGameSequence que anime un saltito de los otros actores
    /// </summary>
    public void JumpOnWin()
    {
        // Comprobar que el personaje este activo
        if (isDisappeared) return;

        // Hacer saltar uno de los actores
        float force = Random.Range(2, 4);
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        if (rigidbody.velocity.y == 0) rigidbody.velocity = new Vector2(0, force);
    }
}