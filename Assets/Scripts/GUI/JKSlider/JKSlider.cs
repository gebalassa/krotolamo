using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JKSlider : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] List<JKSliderStruct> levelList;

    // Utiles
    Slider slider;
    Image sliderHandle;

    // Use this for initialization
    void Start()
    {
        slider = GetComponent<Slider>();
        sliderHandle = slider.handleRect.GetComponent<Image>();
        slider.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(slider.value);

        // Ordenar la lista en base a su limite
        levelList.OrderBy(x => x.threshold).ToList();

    }

    /// <summary>
    /// Cambio en el valor del slider
    /// </summary>
    /// <param name="value"></param>
    private void OnValueChanged(float value)
    {
        foreach(JKSliderStruct level in levelList)
        {
            if(level.threshold <= value)
            {
                sliderHandle.sprite = level.sprite;
            }
        }
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnValueChanged);
    }
}