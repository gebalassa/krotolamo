using DigitalRuby.Tween;
using System.Collections;
using UnityEngine;

public class ArcadeLightController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] Color fromColor = new Color(1, 1, 1, 1);
    [SerializeField] Color toColor = new Color(1, 1, 1, .58f);
    [SerializeField] [Range(1, 4)] float timePerWayMin = 1;
    [SerializeField] [Range(1, 4)] float timePerWayMax = 2.5f;

    // Tiempo que demora en una cambio de color
    float timePerWay = 1;

    // Componentes recurrentes
    SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Calcular el tiempo por camino que tendra la luz
        if(timePerWayMax < timePerWayMin)
        {
            float temp = timePerWayMax;
            timePerWayMax = timePerWayMin;
            timePerWayMin = temp;
        }

        timePerWay = Random.Range(timePerWayMin, timePerWayMax);
        StartCoroutine(LightBlink());
    }

    /// <summary>
    /// Cambiar el color del elemento
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color)
    {
        if(!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
        fromColor = new Color(color.r, color.g, color.b, fromColor.a);
        toColor = new Color(color.r, color.g, color.b, toColor.a);
    }

    /// <summary>
    /// Cambio de la opacidad de las luces para dar efecto de destellos
    /// </summary>
    /// <returns></returns>
    private IEnumerator LightBlink()
    {
        bool inProgress = true;

        // Cambiar el color del sprite
        System.Action<ITween<Color>> changeColor = (t) =>
        {
            if (spriteRenderer) spriteRenderer.color = t.CurrentValue;
        };

        // Completar el proceso
        System.Action<ITween<Color>> changeColorFinal = (t) =>
        {
            inProgress = false;
        };

        // Disminuir opacidad
        gameObject.Tween(string.Format("FadeOutColor{0}", GetInstanceID()), fromColor, toColor,
            timePerWay, TweenScaleFunctions.QuadraticEaseInOut, changeColor, changeColorFinal);

        yield return new WaitForSeconds(timePerWay);
        while (inProgress) yield return null;

        // Realizar la vuelta al color original
        inProgress = true;

        // Aumentar opacidad
        gameObject.Tween(string.Format("FadeInColor{0}", GetInstanceID()), toColor, fromColor,
            timePerWay, TweenScaleFunctions.QuadraticEaseInOut, changeColor, changeColorFinal);

        yield return new WaitForSeconds(timePerWay);
        while (inProgress) yield return null;

        if (gameObject) StartCoroutine(LightBlink());
    }
}