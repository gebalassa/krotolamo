using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.Tween;

public class AttackStack : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] Sprite guu;
    [SerializeField] Sprite paa;
    [SerializeField] Sprite choki;
    [SerializeField] Sprite illuminati;
    [SerializeField] Sprite magicWand;
    [SerializeField] Color inactiveColor;

    // Posicion original en el eje Y de los elementos
    float showY = 0;
    float showYDelta = 40;

    // Color cuando no esta en pantalla
    Color fadeColor = new Color(1, 1, 1, 0);

    // Cambiar el material de todos los hijos
    private void Start()
    {
        foreach (Transform c in transform)
        {
            Image child = c.gameObject.GetComponent<Image>();
            child.material = new Material(child.material);
            showY = c.position.y;
        }
    }

    // Cambiar el ataque en el indice solicitado
    public void ChangeAttack(int index, Attacks attack)
    {
        if (index >= transform.childCount) return;

        Image child = transform.GetChild(index).gameObject.GetComponent<Image>();

        if (child.color == Color.white) return;

        // Realizar el cambio
        switch (attack)
        {
            case Attacks.Rock:
                child.sprite = guu;
                break;
            case Attacks.Paper:
                child.sprite = paa;
                break;
            case Attacks.Scissors:
                child.sprite = choki;
                break;
            case Attacks.MagicWand:
                child.sprite = magicWand;
                break;
        }

        child.color = fadeColor;
        child.material.SetFloat("_GrayscaleAmount", 1);

        // Realizar aparicion del ataque
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if(child != null) child.color = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> updatePosition = (t) =>
        {
            if (child != null) child.transform.position = t.CurrentValue;
        };

        Vector2 startVector = child.transform.position;
        startVector.y = showY - showYDelta;
        Vector2 endVector = new Vector2(startVector.x, showY);

        // Hacer fade in y mostrar elemento
        child.gameObject.Tween( string.Format("FadeIn{0}{1}",GetInstanceID(),index), fadeColor, Color.white, .5f, TweenScaleFunctions.QuadraticEaseOut, updateColor);
        child.gameObject.Tween(string.Format("FadeInMove{0}{1}", GetInstanceID(), index), startVector, endVector, .5f, TweenScaleFunctions.QuadraticEaseOut, updatePosition);

    }

    // Activar el ataque en el indice indicado
    public void ActiveAttack(int index)
    {
        if (index >= transform.childCount) return;

        Image child = transform.GetChild(index).gameObject.GetComponent<Image>();
        child.color = Color.white;
        child.material.SetFloat("_GrayscaleAmount", 1);

        // Realizar activacion del ataque
        System.Action<ITween<float>> updateColor = (t) =>
        {
            if (child != null) child.material.SetFloat("_GrayscaleAmount", t.CurrentValue);
        };

        // completion defaults to null if not passed in
        child.gameObject.Tween(string.Format("Activate{0}{1}", GetInstanceID(), index), 1, 0, .5f, TweenScaleFunctions.QuadraticEaseOut, updateColor);

    }

    // Resetear el stack
    public void Reset()
    {
        int index = 0;
        foreach(Transform c in transform)
        {
            Image child = c.gameObject.GetComponent<Image>();

            if(child.color == Color.white)
            {
                // Realizar desaparicion del ataque
                System.Action<ITween<Color>> updateColor = (t) =>
                {
                    if(child) child.color = t.CurrentValue;
                };

                // Hacer fade out
                child.gameObject.Tween(string.Format("FadeOut{0}{1}", GetInstanceID(), index), Color.white, fadeColor, .2f, TweenScaleFunctions.QuadraticEaseOut, updateColor);

                index++;
            }

           
        }

    }

}
