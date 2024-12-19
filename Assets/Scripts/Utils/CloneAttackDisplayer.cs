using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalRuby.Tween;

public class CloneAttackDisplayer : MonoBehaviour {

    // Movimiento de ataque
    float distance = .1f;
    float timeToFade = .5f;
    float timeToMove = 2f;
    float timeToMoveWait = 1f;
    float attackTorque = 10f;

    // Utiles
    Coroutine animateCoroutine;
    Tween<Color> fadeTween;
    Tween<Vector2> moveTween;

    /// <summary>
    /// Iniciar animacion del ataque
    /// </summary>
    public void Animate(bool attackHasGravity = false)
    {
        animateCoroutine = StartCoroutine(AnimateCoroutine(attackHasGravity));
    }

    /// <summary>
    /// Animacion del elemento
    /// </summary>
    public IEnumerator AnimateCoroutine(bool attackHasGravity = false)
    {
        SpriteRenderer copyRenderer = GetComponent<SpriteRenderer>();

        // Obtener la orientacion del personaje y cambiar los valores si es negativo
        float orientation = gameObject.transform.localScale.x;

        System.Action<ITween<Color>> fadeIn = (t) =>
        {
            if (copyRenderer != null) copyRenderer.color = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> moveIn = (t) =>
        {
            if (gameObject != null) gameObject.transform.position = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> moveInComplete = (t) =>
        {
            Destroy(gameObject);
        };

        System.Action<ITween<Color>> FadeOutComplete = (t) =>
        {
            Destroy(gameObject);
        };

        Color initialColor = new Color(1, 1, 1, 0);
        Color endColor = Color.white;

        Vector2 startVector = new Vector2(transform.position.x - distance * orientation, transform.position.y - distance);
        Vector2 endVector = new Vector2(transform.position.x + distance * orientation, transform.position.y + distance);

        // Que ataque aparezca
        fadeTween = gameObject.Tween(string.Format("FadeIn{0}", GetInstanceID()), initialColor, endColor,
            timeToFade, TweenScaleFunctions.QuadraticEaseInOut, fadeIn);

        moveTween = gameObject.Tween(string.Format("Move{0}", GetInstanceID()), startVector, endVector,
            timeToMove, TweenScaleFunctions.QuadraticEaseInOut, moveIn, moveInComplete);

        yield return new WaitForSeconds(timeToMoveWait);

        // En caso de que exista gravedad para el ataque, agregar el componente
        if (attackHasGravity)
        {
            attackHasGravity = false;
            moveTween.Stop(TweenStopBehavior.DoNotModify);
            Rigidbody2D rigidBody = gameObject.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
            rigidBody.AddTorque(attackTorque);

            // Que ataque desaparezca
            fadeTween = gameObject.Tween(string.Format("FadeOut{0}", GetInstanceID()), endColor, initialColor,
                timeToMove, TweenScaleFunctions.QuadraticEaseInOut, fadeIn, FadeOutComplete);
        }
        else
        {
            // Que ataque desaparezca
            fadeTween = gameObject.Tween(string.Format("FadeOut{0}", GetInstanceID()), endColor, initialColor,
                timeToFade, TweenScaleFunctions.QuadraticEaseInOut, fadeIn);
        }
    }

    /// <summary>
    /// Destruir corutinas y detener tweens
    /// </summary>
    private void OnDestroy()
    {
        if (fadeTween != null) fadeTween.Stop(TweenStopBehavior.DoNotModify);
        if (moveTween != null) moveTween.Stop(TweenStopBehavior.DoNotModify);
        if (animateCoroutine != null) StopCoroutine(animateCoroutine);
    }

}