using UnityEngine;

public class AvatarBox
{
    private Sprite sprite;
    private bool blackAndWhite;

    public AvatarBox(Sprite sprite, bool blackAndWhite)
    {
        this.sprite = sprite;
        this.blackAndWhite = blackAndWhite;
    }

    /// <summary>
    /// Obtener el sprite asociado
    /// </summary>
    /// <returns></returns>
    public Sprite GetSprite()
    {
        return sprite;
    }

    /// <summary>
    /// Saber si el avatar debe ser desplegado en blanco y negro
    /// </summary>
    /// <returns></returns>
    public bool GetBlackAndWhite()
    {
        return blackAndWhite;
    }
}