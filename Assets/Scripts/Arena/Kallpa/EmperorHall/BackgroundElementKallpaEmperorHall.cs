using System.Collections;
using UnityEngine;

public class BackgroundElementKallpaEmperorHall : BackgroundElements
{
    [Header("Extra Elements")]
    [SerializeField] MeshRenderer centralWall;

    /// <summary>
    /// Override del start para colocar bien la muralla central
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // Los fondos deben estar al fondo, como su nombre lo indica
        string backgroundSortingLayerName = JankenUp.SpritesSortingLayers.ArenaBackground;

        // Colocar en el mismo orden de la muralla principal
        centralWall.sortingLayerName = backgroundSortingLayerName;
        centralWall.sortingOrder = wall.sortingOrder;
    }
}
