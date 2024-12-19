using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JanKenShopOpenButton : MonoBehaviour
{
    Button buttonComponent;

    // Use this for initialization
    void Start()
    {
        buttonComponent = GetComponent<Button>();
        buttonComponent.onClick.AddListener(GoToShop);
    }

    /// <summary>
    /// Apertura de la tienda
    /// </summary>
    void GoToShop()
    {
        SceneController sceneController = FindObjectOfType<SceneController>();
        if (sceneController) sceneController.UIButtonSFX();
        SceneLoaderManager sceneLoaderManager = FindObjectOfType<SceneLoaderManager>();
        if (sceneLoaderManager) sceneLoaderManager.JanKenShop(sceneLoaderManager.GetCurrentScene());
    }

}