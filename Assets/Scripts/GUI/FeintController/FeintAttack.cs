using DigitalRuby.Tween;
using System;
using System.Collections;
using UnityEngine;

public class FeintAttack : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] AudioClip popSound;

    [Header("Attack Animation")]
    [SerializeField] [Range(0, 1f)] float attackTimeToggle = 0.5f;
    [SerializeField] [Range(0, 1f)] float attackScale = 0.8f;
    private void Start()
    {
        Show();
    }

    /// <summary>
    /// Muestra el ataque
    /// </summary>
    public void Show()
    {
        AnimationShow();
    }

    /// <summary>
    /// Cambio del sprite asociado a la finta
    /// </summary>
    /// <param name="sprite">Sprite a usar</param>
    public void SetSprite(Sprite sprite)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) spriteRenderer.sprite = sprite;
    }

    #region Animations

    /// <summary>
    /// Realizar corutina especificada
    /// </summary>
    /// <param name="coroutine">Rutina a realizar</param>
    private void DoAnimation(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    /// <summary>
    /// Animar la aparicion del trigger
    /// </summary>
    public void AnimationShow()
    {
        MasterSFXPlayer._player.PlayOneShot(popSound);
        DoAnimation(AnimationAttack(0, attackScale, TweenScaleFunctionsExtension.EaseElasticOut));
    }

    /// <summary>
    /// Animar la desaparicion del trigger
    /// </summary>
    public void AnimationHide()
    {
        DoAnimation(AnimationAttack(attackScale, 0, TweenScaleFunctionsExtension.EaseElasticIn));
    }

    /// <summary>
    /// Aparicion del trigger de finta
    /// </summary>
    /// <param name="from">Escala inicial</param>
    /// <param name="to">Escala final</param>
    public IEnumerator AnimationAttack(float from, float to, Func<float,float> scaleFunction)
    {

        System.Action<ITween<float>> zoomInOut = (t) =>
        {
            if (gameObject != null) gameObject.transform.localScale = new Vector3(t.CurrentValue, t.CurrentValue, t.CurrentValue);
        };

        System.Action<ITween<float>> zoomInOutEnd = (t) =>
        {
            if (gameObject != null && to == 0) Destroy(gameObject);
        };

        // Que ataque aparezca
        gameObject.Tween(string.Format("ZoomInOut{0}", gameObject.GetInstanceID()), from, to,
            attackTimeToggle, scaleFunction, zoomInOut, zoomInOutEnd);

        yield return null;

    }

    #endregion

}