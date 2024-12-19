using System.Collections;
using UnityEngine;

public class BackgroundElementMariaDaHouse : BackgroundElements
{
    [Header("Extra Elements")]
    [SerializeField] MeshRenderer WallWood;

    /// <summary>
    /// Override del start para colocar bien la muralla central
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // Los fondos deben estar al fondo, como su nombre lo indica
        string backgroundSortingLayerName = JankenUp.SpritesSortingLayers.ArenaBackground;

        // Colocar en el mismo orden de la muralla principal
        WallWood.sortingLayerName = backgroundSortingLayerName;
        WallWood.sortingOrder = wall.sortingOrder + 1;
    }
}
