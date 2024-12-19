using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogoDeluxe : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] Image jan;
    [SerializeField] Image ken;
    [SerializeField] Image up;
    [SerializeField] Image japaneseImage;
    [SerializeField] Image deluxeImage;
    [SerializeField] Color clearWhite = new Color(1,1,1,0);

    [Header("Times")]
    [SerializeField] float timeToFade = .5f;
    [SerializeField] float timeToMove = .5f;

    [Header("Positions")]
    [SerializeField] Vector3 finalPosition;

    [Header("Audios")]
    [SerializeField] AudioClip animationDeluxeAudioClip;

    // Inicio de animacion para mostrar el logo
    public IEnumerator Show()
    {
        bool complete = false;
        AudioSource audioSource = GetComponent<AudioSource>();

        // Cambio de color de elementos
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if(jan) jan.color = t.CurrentValue;
            if (ken) ken.color = t.CurrentValue;
            if (up) up.color = t.CurrentValue;
            if (japaneseImage) japaneseImage.color = t.CurrentValue;
            if (deluxeImage) deluxeImage.color = t.CurrentValue;
        };

        // Indicar el sonido de deluxe
        System.Action<ITween<Color>> updateColorComplete = (t) =>
        {
            audioSource.PlayOneShot(animationDeluxeAudioClip);
        };

        // Subida a posicion final
        System.Action<ITween<Vector3>> updatePosition = (t) =>
        {
           if(transform) transform.localPosition = t.CurrentValue;
        };

        System.Action<ITween<Vector3>> updatePositionComplete = (t) =>
        {
            complete = true;
        };

        gameObject.Tween("FadeIn", clearWhite, Color.white, timeToFade, TweenScaleFunctions.QuadraticEaseInOut, updateColor, updateColorComplete)
            .ContinueWith(new Vector3Tween().Setup(transform.localPosition, finalPosition, timeToMove, TweenScaleFunctions.QuadraticEaseInOut, updatePosition, updatePositionComplete));

        while (!complete) yield return null;

    }

    // Inicio de animacion para esconder el logo
    public void Hide()
    {
        // Cambio de color de elementos
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if (jan) jan.color = t.CurrentValue;
            if (ken) ken.color = t.CurrentValue;
            if (up) up.color = t.CurrentValue;
            if (japaneseImage) japaneseImage.color = t.CurrentValue;
            if (deluxeImage) deluxeImage.color = t.CurrentValue;
        };

        gameObject.Tween("FadeIn", Color.white, clearWhite, timeToFade, TweenScaleFunctions.QuadraticEaseInOut, updateColor);
    }
}
