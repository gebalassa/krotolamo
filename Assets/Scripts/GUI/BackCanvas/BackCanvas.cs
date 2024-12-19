using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackCanvas : MonoBehaviour
{
    [SerializeField] Button backButton;

    // Recurrentes
    SceneController sceneController;

    // Use this for initialization
    void Start()
    {
        backButton.onClick.AddListener(BackFunction);
    }

    /// <summary>
    /// Llamada a funcion GoBack en el controlador
    /// </summary>
    void BackFunction()
    {
        GetSceneController();
        sceneController.GoBack();
    }

    /// <summary>
    /// Simulacion de click en el boton de back
    /// </summary>
    public void Back()
    {
        GameController.SimulateClick(backButton);
    }

    /// <summary>
    /// Obtencion del controlador de escena activo
    /// </summary>
    void GetSceneController()
    {
        if (sceneController != null) return;
        sceneController = FindObjectOfType<SceneController>();
    }
}