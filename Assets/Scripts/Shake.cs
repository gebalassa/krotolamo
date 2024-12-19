using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Shake : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField][Range(0f,1f)] float magnitude = 0.25f;
    [SerializeField] bool shakeGUI = true;
    [SerializeField] [Range(0f, 10f)] float magnitudeGUI = 10f;
    [SerializeField] [Range(0f, 1f)] float stopShakingAfter = .25f;
    bool isShaking = false;
    float customMagnitude = 0;

    // Camera
    Vector3 cameraInitialPosition;
    Quaternion cameraInitialRotation;

    // GUIs
    GameObject[] UICanvas;
    Vector3[] UIInitialPositions;

    // Coroutines
    Coroutine mainShakeCoroutine;
    Coroutine defaultStopShakeCoroutine;

    // Singleton
    public static Shake _this;
    private void Awake()
    {
        int length = FindObjectsOfType<Shake>().Length;
        if(length == 1)
        {
            DontDestroyOnLoad(gameObject);
            _this = this;

            // Obtener tanto la posicion como la rotacion original de la camara
            cameraInitialPosition = Camera.main.transform.localPosition;
            cameraInitialRotation = Camera.main.transform.localRotation;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inicio del shake de la camara y el GUI si esta habilitada la opción
    /// </summary>
    public void ShakeIt(){

        if (isShaking) return;
        if (mainShakeCoroutine != null) StopCoroutine(mainShakeCoroutine);
        mainShakeCoroutine = StartCoroutine(ShakeItCoroutine());
    }

    // Shake con magnitud custom
    public void ShakeIt(float customMagnitude)
    {
        this.customMagnitude = customMagnitude;
        ShakeIt();
    }

    /// <summary>
    /// Rutina de movimiento de objetos de canvas
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShakeItCoroutine()
    {
        isShaking = true;
        if (defaultStopShakeCoroutine != null) StopCoroutine(defaultStopShakeCoroutine);
        defaultStopShakeCoroutine = StartCoroutine(DefaultStopShaking());

        // Obtener los UI disponibles si la opción esta activada
        if (shakeGUI)
        {
            UICanvas = GameObject.FindGameObjectsWithTag("Canvas");
            UIInitialPositions = UICanvas.Select(canvas =>
                Camera.main.ScreenToViewportPoint(canvas.transform.position)).ToArray();
        }

        while (isShaking){
            // Mover la camara
            Camera.main.transform.localPosition =
                cameraInitialPosition + UnityEngine.Random.insideUnitSphere * (customMagnitude != 0 ? customMagnitude : magnitude);

            // ¿Se debe mover los distintos canvas?
            if (shakeGUI)
            {
                for (int index = 0; index < UICanvas.Length; index++)
                {

                    if (UICanvas[index])
                    {
                        UICanvas[index].transform.localPosition =
                        UIInitialPositions[index]
                        + UnityEngine.Random.insideUnitSphere * magnitudeGUI;
                    }


                }
            }
            yield return null;
        }

        isShaking = false;
    }

    /// <summary>
    /// Corutina para detener en caso de no hacerse de manera manual la ejecucion de la corutina de shake
    /// </summary>
    /// <returns></returns>
    private IEnumerator DefaultStopShaking()
    {
        yield return new WaitForSeconds(stopShakingAfter);
        StopIt();
    }

    /// <summary>
    /// Se deja de hacer shake a la camara y a los UI
    /// </summary>
    public void StopIt(){

        if (!isShaking) return;
        if (mainShakeCoroutine != null) StopCoroutine(mainShakeCoroutine);
        if (defaultStopShakeCoroutine != null) StopCoroutine(defaultStopShakeCoroutine);
        isShaking = false;

        Camera.main.transform.localPosition = cameraInitialPosition;

        // Volver GUI a su posición original
        if (shakeGUI)
        {
            for (int index = 0; index < UICanvas.Length; index++){
                if(UICanvas[index]) UICanvas[index].transform.localPosition = UIInitialPositions[index];
            }
        }

        customMagnitude = 0;
    }

    /// <summary>
    /// Shake sobre un gameobject en particular por X tiempo
    /// </summary>
    /// <param name="target"></param>
    /// <param name="time"></param>
    public void ShakeThis(GameObject target, float time)
    {
        StartCoroutine(ShakeThisCoroutine(target, time, false));
    }

    /// <summary>
    /// Shake con magnitud custom
    /// </summary>
    /// <param name="target"></param>
    /// <param name="time"></param>
    /// <param name="customMagnitude"></param>
    public void ShakeThis(GameObject target, float time, float customMagnitude)
    {
        this.customMagnitude = customMagnitude;
        StartCoroutine(ShakeThisCoroutine(target, time, false));
    }

    /// <summary>
    /// Shake sobre un UI en particular por X tiempo
    /// </summary>
    /// <param name="target"></param>
    /// <param name="time"></param>
    public void ShakeThisUI(GameObject target, float time)
    {
        StartCoroutine(ShakeThisCoroutine(target, time, true));
    }

    /// <summary>
    /// Corutina para realizar la vibracion sobre el objeto indicado
    /// </summary>
    /// <param name="target"></param>
    /// <param name="time"></param>
    /// <param name="UI"></param>
    /// <returns></returns>
    public IEnumerator ShakeThisCoroutine(GameObject target, float time, bool UI)
    {
        if (target)
        {

            Vector3 initialPosition = target.transform.localPosition;

            while (time > 0 && target)
            {
                if (Time.timeScale == 1)
                {
                    target.transform.localPosition =
                        initialPosition
                        + UnityEngine.Random.insideUnitSphere * (UI ? magnitudeGUI : (customMagnitude == 0? magnitude : customMagnitude));
                }
                time -= Time.deltaTime;

                yield return null;
            }

            if (target) target.transform.localPosition = initialPosition;
            customMagnitude = 0;

        }

    }

    /// <summary>
    /// Retorna si se esta ejecutando un shake
    /// </summary>
    /// <returns></returns>
    public bool IsShaking()
    {
        return isShaking;
    }

    /// <summary>
    /// Obtencion de la posicion inicial que tenia la camara
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCameraInitialPosition()
    {
        return cameraInitialPosition;
    }

    /// <summary>
    /// Obtencion de la rotacion inicial que tenia la camara
    /// </summary>
    /// <returns></returns>
    public Quaternion GetCameraRotationPosition()
    {
        return cameraInitialRotation;
    }
}
