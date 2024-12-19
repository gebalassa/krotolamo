using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeAttackFillBar : MonoBehaviour
{
    GameObject fillObject;
    Image fillImage;

    float minSpeed = 0.2f;
    float maxSpeed = 10;

    // Ajustar posicion segun preferencia de jugador
    private void Start()
    {
        // Conseguir la preferencia del usuario. 0 izquierda 1 seria derecha
        int position = PlayerPrefs.GetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, PlayersPreferencesController.CONTROLPOSITIONKEYDEFAULT);
        if(position == 1)
        {
            RectTransform rect = GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x * (position == 0 ? 1 : -1), rect.anchoredPosition.y);
            rect.eulerAngles = new Vector3(0, 0, (position == 0 ? 45 : 135));
            rect.localScale = new Vector2(1, (position == 0 ? 1 : -1));
        }
        
    }

    // Asignar la camara correspondiente
    public void AssignCamera()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    // Asignar el tween para llenar la barra
    public IEnumerator Fill(float time)
    {
        // Nota: Sorry lo feo de poner los valores en bruto. Esto se hizo por la version Switch y para palear un error que aveces pasa, el minimo es 0.4 en los singles player pero a veces quedaba en 0! Asi que dejaremos en 0.2 por si le pasa a alguien, no sea tan mortal
        time = Mathf.Clamp(time, minSpeed, maxSpeed);

        // Si no esta activo, activar
        gameObject.SetActive(true);

        // Encontrar el fill para rellenar
        if (!fillObject)
        {
            fillObject = transform.Find("Fill").gameObject;
            fillImage = fillObject.GetComponent<Image>();
        }

        System.Action<ITween<float>> updateFill = (t) =>
        {
            if(fillImage != null) fillImage.fillAmount = t.CurrentValue;
        };

        // Aumentar la puntuación
        fillObject.Tween(string.Format("Score{0}", GetInstanceID()), 0, 1, time, TweenScaleFunctions.Linear, updateFill);

        yield return new WaitForSeconds(time);
    }

    // Detener el corutina de relleno de ataque
    public void StopFilling()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
}
