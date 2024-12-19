using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GastonMiauffinInGameController : CharacterInGameController
{
    [Header("Sprites Renderers")]
    [SerializeField] SpriteRenderer[] spriteRenderers;

    [Header("Sprites Gaston Attack")]
    [SerializeField] GameObject spriteGastonRock;
    [SerializeField] GameObject spriteGastonPaper;
    [SerializeField] GameObject spriteGastonScissors;

    [Header("Others")]
    [SerializeField] GameObject woolWinPrefab;
    [SerializeField] Vector2 woolWinPosition = new Vector2(-2.97f, -0.3f);

    // Instancia de la lana producida al ganar
    GameObject woolWin;

    /* Preparacion del super ataque
     * Parametros:
     * @cameraScale : Factor de zoom para la camara durante la preparacion del super
     */
    public override IEnumerator PrepareSuper(int cameraResizeFactor, bool win = true, bool byeByeReferee = true)
    {

        GameObject instance = Instantiate(superJanKenUPPrefab, transform.position, Quaternion.identity) as GameObject;
        superAttack = null;
        superAttack = instance.GetComponent<SuperGastonMiauffin>();

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

    // Acciones posteriores a la ejecucion del super
    public override IEnumerator PostSuper()
    {
        yield return null;
    }

    // Cambio de la capa de orden para los spriteRenderer
    public override void ChangeSpritesLayerTo(string layerName)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingLayerID = SortingLayer.NameToID(layerName);
        }
    }

    // Cambia el estado del personaje
    public override void ChangeState(InGameStates newState)
    {
        base.ChangeState(newState);

        // Si se perdio y desaparecio, no cambiar de estado
        if (newState == InGameStates.Lose && isDisappeared) return;

        // Eliminar wool si existe
        if (newState == InGameStates.Stand && woolWin) Destroy(woolWin);

        // Al ganar, genera un wool
        if (newState == InGameStates.Win) GenerateWinWool();

        // Cambiar el despliegue de ataques de Gaston
        if( (state == InGameStates.Attack || state == InGameStates.Wait) && !isDisappeared )
        {
            switch (currentAttack)
            {
                case Attacks.Rock:
                case Attacks.MagicWand:
                    spriteGastonRock.SetActive(true);
                    spriteGastonPaper.SetActive(false);
                    spriteGastonScissors.SetActive(false);
                    break;

                case Attacks.Paper:
                    spriteGastonRock.SetActive(false);
                    spriteGastonPaper.SetActive(true);
                    spriteGastonScissors.SetActive(false);
                    break;

                case Attacks.Scissors:
                    spriteGastonRock.SetActive(false);
                    spriteGastonPaper.SetActive(false);
                    spriteGastonScissors.SetActive(true);
                    break;
            }
        }
        else
        {
            if(spriteGastonRock) spriteGastonRock.SetActive(false);
            if (spriteGastonPaper) spriteGastonPaper.SetActive(false);
            if (spriteGastonScissors) spriteGastonScissors.SetActive(false);
        }

    }

    // Realizar el rebote si se esta en un super
    protected override void ChangeRigidBodyProperties()
    {
        base.ChangeRigidBodyProperties();
        if (superJanKenUpActive && superAttack) (superAttack as SuperGastonMiauffin).BouceToOriginalPosition();
    }

    // Terminar la recuperacion del super
    public void FinishSuperAnimationDeTransformation()
    {
        if (superJanKenUpActive && superAttack) (superAttack as SuperGastonMiauffin).FinishDeTranformation();
    }

    // Generacion de la lana al ganar un combate
    private void GenerateWinWool()
    {
        woolWin = Instantiate(woolWinPrefab, transform);
        woolWin.transform.localPosition = woolWinPosition;
        woolWin.transform.parent = null;
    }

    /// <summary>
    /// Destruccion del objeto debe destruir la lana tambien
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (woolWin) Destroy(woolWin);
    }

}