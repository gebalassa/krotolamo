using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JanKenCardPortraitBackground : MonoBehaviour
{
    [SerializeField] Image background;

    // Utiles
    bool isReady = false;

    private void Start()
    {
        AdjustIllustration();
    }

    /// <summary>
    /// Ajuste de la ilustracion para mantener el tamano 16:9
    /// </summary>
    /// <returns></returns>
    protected virtual void AdjustIllustration()
    {
        float ratioX = 16f;
        float ratioY = 9f;
        float scaleFactor = 1.01f;
        int screenWidth = (int)Math.Ceiling((background.transform.parent as RectTransform).rect.width * scaleFactor);
        int imageHeight = (int)Math.Ceiling((background.transform.parent as RectTransform).rect.height * scaleFactor);
        int partSize = (int)Math.Ceiling(imageHeight / ratioY);
        int imageWidth = (int)(partSize * ratioX);

        if (imageWidth < screenWidth)
        {
            int diff = screenWidth - imageWidth;
            imageWidth += diff;
            imageHeight += (int)(diff / ratioX * ratioY);
        }

        background.rectTransform.sizeDelta = new Vector2(imageWidth, imageHeight);

        isReady = true;
    }

    /// <summary>
    /// Indica si esta lista los ajustes de la imagen
    /// </summary>
    /// <returns></returns>
    public bool IsReady()
    {
        return isReady;
    }

    /// <summary>
    /// Cambio de color para la imagen de fondo
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color)
    {
        background.color = color;
    }
}