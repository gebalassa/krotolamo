using System.Collections;
using UnityEngine;

public class BackgroundElementHumbleHouse : BackgroundElements
{
    [Header("Extra Elements")]
    [SerializeField] MeshRenderer centralWall;
    [SerializeField] MeshRenderer otherSideWall;
    [SerializeField] GameObject[] graffitiOptions;

    /// <summary>
    /// Override del start para colocar bien la muralla central
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // Los fondos deben estar al fondo, como su nombre lo indica
        string backgroundSortingLayerName = JankenUp.SpritesSortingLayers.ArenaBackground;

        // Colocar en el mismo orden de la muralla principal y la lateral
        centralWall.sortingLayerName = backgroundSortingLayerName;
        centralWall.sortingOrder = wall.sortingOrder;
        otherSideWall.sortingLayerName = backgroundSortingLayerName;
        otherSideWall.sortingOrder = wall.sortingOrder;

        // Colocar el piso sobre la muralla
        floor.sortingOrder = wall.sortingOrder + 1;
    }

    /// <summary>
    /// Override para aplicar configuraciones de graffitis
    /// </summary>
    /// <param name="backgroundColorIndex"></param>
    public override void Config(int backgroundColorIndex)
    {
        base.Config(backgroundColorIndex);
        int random = Random.Range(0, graffitiOptions.Length);
        int index = 0;
        foreach(GameObject graffitiOption in graffitiOptions)
        {
            graffitiOption.SetActive(index++ == random);
        }

    }
}
