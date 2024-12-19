using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Conseguir la preferencia del usuario. 0 izquierda 1 seria derecha
        /*int position = PlayerPrefs.GetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, PlayersPreferencesController.CONTROLPOSITIONKEYDEFAULT);
        RectTransform rect = GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x * (position == 0 ? 1 : -1), rect.anchoredPosition.y);
        rect.anchorMin = new Vector2(position == 0? 1 : 0, 0);
        rect.anchorMax = new Vector2(position == 0 ? 1 : 0, 0);
        rect.pivot = new Vector2(position == 0 ? 1 : 0, 0);*/
    }

}
