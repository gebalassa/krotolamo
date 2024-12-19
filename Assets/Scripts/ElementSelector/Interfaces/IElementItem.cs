using System.Collections;
using UnityEngine;

public interface IElementItem
{
    /// <summary>
    /// Obtencion del identificador del elemento
    /// </summary>
    /// <returns></returns>
    string GetIdentifier();

    /// <summary>
    /// Obtencion del nombre del elemento
    /// </summary>
    /// <returns></returns>
    string GetName();

    /// <summary>
    /// Obtencion el avatar asociado al elemento
    /// </summary>
    /// <returns></returns>
    Sprite GetAvatar();

    /// <summary>
    /// Revisar si elemento esta bloqueado o no
    /// </summary>
    /// <returns></returns>
    bool IsUnlocked();
}