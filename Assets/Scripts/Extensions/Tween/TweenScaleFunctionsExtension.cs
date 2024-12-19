using DigitalRuby.Tween;
using System;
using System.Collections;
using UnityEngine;

public class TweenScaleFunctionsExtension
{
    // Metodos extraidos de https://github.com/sudosays/duelistic/blob/master/Assets/Scripts/Support/Easing.cs

    /// <summary>
    /// Funcion elastica con rebote al final
    /// </summary>
    public static readonly Func<float, float> EaseElasticOut = EaseElasticOutFunc;
    private static float EaseElasticOutFunc(float progress) { return ElasticOut(progress); }

    /// <summary>
    /// Funcion elastica con rebote al principio
    /// </summary>
    public static readonly Func<float, float> EaseElasticIn = EaseElasticInFunc;
    private static float EaseElasticInFunc(float progress) { return ElasticIn(progress); }

    /// <summary>
    /// Easing equation function for an elastic (exponentially decaying sine wave) easing out: decelerating from zero velocity.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float ElasticOut(float t)
    {
        t = Mathf.Clamp01(t);
        if (t == 1) return 1;

        return (Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.075f) * (2 * Mathf.PI) / 0.3f) + 1);
    }

    /// <summary>
    /// Easing equation function for an elastic (exponentially decaying sine wave) easing in: accelerating from zero velocity.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float ElasticIn(float t)
    {
        t = Mathf.Clamp01(t);
        if (t == 1) return 1;

        return -(Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t - 0.075f) * (2 * Mathf.PI) / 0.3f));
    }

    /// <summary>
    /// Easing equation function for an elastic (exponentially decaying sine wave) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float ElasticInOut(float t)
    {
        t = Mathf.Clamp01(t);
        if (t == 1) return 1;

        t *= 2;
        if (t < 1) return -0.5f * (Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t - 0.1125f) * (2 * Mathf.PI) / 0.45f));
        return Mathf.Pow(2, -10 * (t -= 1)) * Mathf.Sin((t - 0.1125f) * (2 * Mathf.PI) / 0.45f) * 0.5f + 1;
    }

    /// <summary>
    /// Easing equation function for an elastic (exponentially decaying sine wave) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float ElasticOutIn(float t)
    {
        if (t < 0.5f)
        {
            return ElasticOut(t * 2) * 0.5f;
        }
        return 0.5f + (ElasticIn((t - 0.5f) * 2) * 0.5f);
    }


    /// <summary>
    /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out: decelerating from zero velocity.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float BounceOut(float t)
    {
        t = Mathf.Clamp01(t);
        if (t < 0.3636363636f) return (7.5625f * t * t);
        if (t < 0.7272727272f) return (7.5625f * (t -= 0.5454545454f) * t + 0.75f);
        if (t < (2.5 / 2.75)) return (7.5625f * (t -= 0.8181818181f) * t + 0.9375f);
        return (7.5625f * (t -= 0.96363636363f) * t + 0.984375f);
    }

    /// <summary>
    /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in: accelerating from zero velocity.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float BounceIn(float t)
    {
        t = 1 - Mathf.Clamp01(t);
        if (t < 0.3636363636f) return 1 - (7.5625f * t * t);
        if (t < 0.7272727272f) return 1 - (7.5625f * (t -= 0.5454545454f) * t + 0.75f);
        if (t < (2.5 / 2.75)) return 1 - (7.5625f * (t -= 0.8181818181f) * t + 0.9375f);
        return 1 - (7.5625f * (t -= 0.96363636363f) * t + 0.984375f);
    }

    /// <summary>
    /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float BounceInOut(float t)
    {
        t = Mathf.Clamp01(t) * 2;
        if (t < 1) return BounceIn(t) * 0.5f;
        return BounceOut(t - 1) * 0.5f + 1 * 0.5f;
    }

    /// <summary>
    /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float BounceOutIn(float t)
    {
        if (t < 0.5f)
        {
            return BounceOut(t * 2) * 0.5f;
        }
        return 0.5f + (BounceIn((t - 0.5f) * 2) * 0.5f);
    }

    /// <summary>
    /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out: decelerating from zero velocity.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float BackOut(float t)
    {
        t = Mathf.Clamp01(t) - 1;
        return t * t * ((1.70158f + 1f) * t + 1.70158f) + 1f;
    }

    /// <summary>
    /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in: accelerating from zero velocity.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float BackIn(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * ((1.70158f + 1) * t - 1.70158f);
    }

    /// <summary>
    /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in/out: acceleration until halfway, then deceleration.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float BackInOut(float t)
    {
        float s = 1.70158f;

        t = Mathf.Clamp01(t) * 2;
        if (t < 1) return 0.5f * (t * t * (((s *= (1.525f)) + 1) * t - s));
        return 0.5f * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2);
    }

    /// <summary>
    /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out/in: deceleration until halfway, then acceleration.
    /// </summary>
    /// <param name="t">Normalised time</param>
    /// <returns>The eased value</returns>
    public static float BackOutIn(float t)
    {
        if (t < 0.5f)
        {
            return BackOut(t * 2) * 0.5f;
        }
        return 0.5f + (BackIn((t - 0.5f) * 2) * 0.5f);
    }
}