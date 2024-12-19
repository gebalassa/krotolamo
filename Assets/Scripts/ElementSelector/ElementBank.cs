using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ElementBank 
{
    /// <summary>
    /// Obtencion de todos los elementos
    /// </summary>
    /// <returns></returns>
    List<GameObject> GetAll();

    /// <summary>
    /// Indica si el elemento permite o no ser seleccionado
    /// </summary>
    /// <returns></returns>
    bool CheckItem(GameObject itemSource);
}