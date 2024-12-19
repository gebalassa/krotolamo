using System.Collections;
using UnityEngine;

public class BackgroundElementArcade : BackgroundElements
{
    [Header("Arcades")]
    [SerializeField] ArcadesController arcadesController;

    /// <summary>
    /// Al activar, realizar los cambios sobre los arcades
    /// </summary>
    /// <param name="active"></param>
    public override void SetActive(bool active)
    {
        base.SetActive(active);
        arcadesController.GenerateArcades();
    }

    /// <summary>
    /// Realizar los saltos sobre cada uno de los arcades
    /// </summary>
    /// <param name="minForce"></param>
    /// <param name="maxForce"></param>
    /// <param name="multiplier"></param>
    public override void Jump(float minForce, float maxForce, int multiplier)
    {
        base.Jump(minForce,maxForce,multiplier);
        arcadesController.Jump(minForce, maxForce, multiplier);
    }
}
