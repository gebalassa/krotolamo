using System.Collections;
using UnityEngine;

public class BackgroundElementSubway : BackgroundElements
{
    [Header("Extra Elements")]
    [SerializeField] MeshRenderer ceil;
    [SerializeField] MeshRenderer map;
    [SerializeField] int fierroSorting = 0;

    /// <summary>
    /// Override del start para colocar bien la muralla central
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // Los fondos deben estar al fondo, como su nombre lo indica
        string mapAnCeilSortingLayerName = JankenUp.SpritesSortingLayers.ArenaPlatform;

        // Colocar el techo por sobre la pared
        ceil.sortingLayerName = mapAnCeilSortingLayerName;
        ceil.sortingOrder = fierroSorting + 1;
        map.sortingLayerName = mapAnCeilSortingLayerName;
        map.sortingOrder = fierroSorting + 2;
    }
}
