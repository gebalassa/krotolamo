using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionDoors : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] bool isOpen = false;
    [SerializeField] bool forceOpenState = false;
    [Tooltip("Tiempo que se demora en cerrar/abrir las puertas")]
    [SerializeField] [Range(0.1f, 1f)] float timeInSeconds = 0.25f;
    [Tooltip("Tiempo que permanece con las puertas cerradas antes de abrirse nuevamente")]
    [SerializeField] [Range(0.1f, 1f)] float timeClosed = 0.25f;
    
    [Header("Doors")]
    [SerializeField] GameObject leftDoor;
    [SerializeField] GameObject rightDoor;
    float minDistance = 20f;
    float offset = 20f;

    [Header("SFX")]
    [SerializeField] AudioClip bellSFX;
    [SerializeField] AudioClip doorsSFX;
    AudioSource audioSource;

    bool lastIsOpen = false;
    
    // Posiciones de las puertas
    Vector2 leftDoorOpen, leftDoorClosed;
    Vector2 rightDoorOpen, rightDoorClosed;
    float distance = 0f;

    // Resolución
    float lastScreenWidth;

    // Flag para indicar si la puerta ya esta completamente abierta
    bool isTotallyOpen = false;

    // Ver si se esta haciendo la transicion
    bool isInTransition = false;

    // Singleton
    public static TransitionDoors _this;

    private void Awake()
    {
        int length = FindObjectsOfType<TransitionDoors>().Length;
        if (length == 1)
        {
            DontDestroyOnLoad(gameObject);
            _this = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        lastScreenWidth = Screen.width;

        // Calcular el valor inicial y final de las puertas
        leftDoorClosed = new Vector2(Screen.width / 2, Screen.height / 2);
        leftDoorOpen = new Vector2(-offset, Screen.height / 2);
        rightDoorClosed = new Vector2(Screen.width / 2, Screen.height / 2);
        rightDoorOpen = new Vector2(Screen.width + offset, Screen.height / 2);
        
        // Ubicar puertas en posición de cierre
        leftDoor.transform.position = leftDoorClosed;
        rightDoor.transform.position = rightDoorClosed;

        // Calcular la distancia entre las 2 posiciones
        distance = Vector2.Distance(leftDoorClosed, leftDoorOpen);

        if (forceOpenState) OpenStateComplete();

    }

    // Update is called once per frame
    void Update()
    {

        // Revisar si ha cambiado la aparetura
        if (lastIsOpen != isOpen){

            if (isOpen) {
                StopAllCoroutines();
                StartCoroutine(Open());
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(Close());
            }

        }

    }

    // Revisar si resolución cambio y adaptar posiciónes de las puertas
    private void FixedUpdate()
    {

        if(lastScreenWidth != Screen.width)
        {
            lastScreenWidth = Screen.width;

            leftDoorClosed = new Vector2(Screen.width / 2, Screen.height / 2);
            leftDoorOpen = new Vector2(-offset, Screen.height / 2);
            rightDoorClosed = new Vector2(Screen.width / 2, Screen.height / 2);
            rightDoorOpen = new Vector2(Screen.width + offset, Screen.height / 2);

            // Calcular la distancia entre las 2 posiciones
            distance = Vector2.Distance(leftDoorClosed, leftDoorOpen);
        }

    }

    // Indica que las puertas deben ser abiertas
    public IEnumerator Open() {

        isOpen = true;
        lastIsOpen = isOpen;
        isInTransition = true;

        // Mover todo
        Shake._this.StopIt();
        Shake._this.ShakeIt();

        // Permanecer cerrado
        yield return new WaitForSeconds(timeClosed);

        // Sonido de campana
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(bellSFX);

        // Si la distancia es practicamente igual, terminar el movimiento
        // Solo se considera una puerta ya que la velocidad es la misma para ambas
        while (Vector2.Distance(leftDoor.transform.position, leftDoorOpen ) > minDistance)
        {

            // Calculo de la velocidad
            float step = (distance / timeInSeconds) * Time.deltaTime;

            // Puerta izquierda
            leftDoor.transform.position = Vector2.MoveTowards(
                leftDoor.transform.position,
                leftDoorOpen,
                step
                );

            // Puerta derecha
            rightDoor.transform.position = Vector2.MoveTowards(
                rightDoor.transform.position,
                rightDoorOpen,
                step
                );

            yield return null;
        }

        OpenStateComplete();

    }

    /// <summary>
    /// Pasos finales para indicar que puerta esta abierta por completo
    /// </summary>
    private void OpenStateComplete()
    {
        isTotallyOpen = true;
        isInTransition = false;

        leftDoor.transform.position = leftDoorOpen;
        rightDoor.transform.position = rightDoorOpen;
        if (Shake._this) Shake._this.StopIt();

        leftDoor.SetActive(false);
        rightDoor.SetActive(false);

        // Indicar que transicion ya esta lista
        SceneLoaderManager.TransitionReady();
    }

    // Indica que las puertas deben ser cerradas
    public IEnumerator Close()
    {

        isOpen = false;
        lastIsOpen = isOpen;
        isTotallyOpen = false;
        isInTransition = true;

        leftDoor.SetActive(true);
        rightDoor.SetActive(true);

        // Mover todo
        Shake._this.StopIt();
        Shake._this.ShakeIt();

        // Sonido de cierre de puertas
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(doorsSFX);

        // Si la distancia es practicamente igual, terminar el movimiento
        // Solo se considera una puerta ya que la velocidad es la misma para ambas
        while (Vector2.Distance(leftDoor.transform.position, leftDoorClosed) > minDistance)
        {

            // Calculo de la velocidad
            float step = (distance / timeInSeconds) * Time.deltaTime;

            // Puerta izquierda
            leftDoor.transform.position = Vector2.MoveTowards(
                leftDoor.transform.position,
                leftDoorClosed,
                step
                );

            // Puerta derecha
            rightDoor.transform.position = Vector2.MoveTowards(
                rightDoor.transform.position,
                rightDoorClosed,
                step
                );

            yield return null;
        }

        isInTransition = false;

        leftDoor.transform.position = leftDoorClosed;
        rightDoor.transform.position = rightDoorClosed;

    }

    // Consulta para saber si la puerta esta totalmente abierta
    public bool IsTotallyOpen()
    {
        return isTotallyOpen;
    }

    // Consulta para saber si se esta haciendo una transicion
    public bool IsInTransition()
    {
        return isInTransition;
    }

}
