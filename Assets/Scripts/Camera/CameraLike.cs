using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DigitalRuby.Tween;

public class CameraLike : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField][Range(0f,5f)] float yPositionMagnitude = .2f;
    [SerializeField][Range(0f,5f)] float xPositionMagnitude = .2f;
    [SerializeField][Range(0f,5f)] float yRotationMagnitude = .5f;
    [SerializeField][Range(0f,5f)] float xRotationMagnitude = .25f;
    [SerializeField][Range(0f,5f)] float movementDuration = 2f;

    // Camera
    Vector3 cameraInitialPosition;
    Quaternion cameraInitialRotation;

    Coroutine mainCoroutine;
    Vector3Tween moveTween;
    Vector3Tween rotateTween;

    InGameSequence inGameSequence;


    private void OnEnable()
    {
        // Obtener la posición inicial de la camara
        cameraInitialPosition = Shake._this.GetCameraInitialPosition();
        cameraInitialRotation = Shake._this.GetCameraRotationPosition();
        mainCoroutine = StartCoroutine(MoveIt());
        inGameSequence = FindObjectOfType<InGameSequence>();
    }

    /// <summary>
    /// Retornar la camara a su posicion y rotacion original
    /// </summary>
    void OnDisable()
    {
        StopCoroutine(mainCoroutine);
        if(moveTween != null) moveTween.Stop(TweenStopBehavior.Complete);
        if(rotateTween != null) rotateTween.Stop(TweenStopBehavior.Complete);
        if(Camera.main) Camera.main.transform.localPosition = cameraInitialPosition;
        if (Camera.main) Camera.main.transform.localRotation = cameraInitialRotation;
    }

    /// <summary>
    /// Realizar un movimiento continuo sobre la camara
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveIt(){

        // Utiles
        bool even = true;
        Vector3 endPosition = Camera.main.transform.localPosition;
        Vector3 endRotation = new Vector2(Camera.main.transform.localRotation.x, Camera.main.transform.localRotation.y);

        while (true)
        {
            bool completeStep = false;


            #region Movimiento en Eje X e Y
            System.Action<ITween<Vector3>> moveCameraY = (t) =>
            {
                if(!Shake._this.IsShaking() && Camera.main) Camera.main.transform.localPosition = cameraInitialPosition + t.CurrentValue;
            };

            System.Action<ITween<Vector3>> moveCameraYComplete = (t) =>
            {
                completeStep = true;
            };

            // Calcular una magnitud en base a lo configurado
            float currentYPositionMagnitude = Random.Range(0f, yPositionMagnitude);
            float currentXPositionMagnitude = Random.Range(0f, xPositionMagnitude);

            // Calcular el rango de movimiento
            Vector3 startPosition = endPosition;
            endPosition = new Vector3(cameraInitialPosition.x + (even ? -currentXPositionMagnitude : currentXPositionMagnitude), cameraInitialPosition.y + (even? -currentYPositionMagnitude : currentYPositionMagnitude));

            moveTween = gameObject.Tween(string.Format("Move{0}", GetInstanceID()), startPosition, endPosition,
             movementDuration, TweenScaleFunctions.QuadraticEaseInOut, moveCameraY, moveCameraYComplete);
            #endregion

            #region Rotacion en Eje X e Y
            System.Action<ITween<Vector3>> rotateCameraY = (t) =>
            {
                if(Camera.main) Camera.main.transform.eulerAngles = t.CurrentValue;
            };

            // Calcular una magnitud en base a lo configurado
            float currentYRotationMagnitude = Random.Range(0f, yRotationMagnitude);
            float currentXRotationMagnitude = Random.Range(0f, xRotationMagnitude);

            // Calcular el rango de movimiento
            Vector3 startRotation = endRotation;
            endRotation = new Vector2(cameraInitialRotation.x + (even ? currentXRotationMagnitude : -currentXRotationMagnitude),
                cameraInitialRotation.y + (even ? -currentYRotationMagnitude : currentYRotationMagnitude));

            rotateTween = gameObject.Tween(string.Format("Rotate{0}", GetInstanceID()), startRotation, endRotation,
             movementDuration, TweenScaleFunctions.QuadraticEaseInOut, rotateCameraY);
            #endregion

            while (!completeStep) yield return null;
            even = !even;
        }

    }

    /// <summary>
    /// Indica si existe una rotacion activa sobre la camara
    /// </summary>
    /// <returns></returns>
    public bool HasRotation()
    {
        return xRotationMagnitude != 0 || yRotationMagnitude != 0;
    }
}
