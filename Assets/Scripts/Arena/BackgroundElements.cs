using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundElements : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] JankenUp.BackgroundElementsIdentifier identifier = JankenUp.BackgroundElementsIdentifier.Classic;

    [Header("Elements")]
    [SerializeField] Canvas background;
    [SerializeField] protected MeshRenderer wall;
    [SerializeField] GameObject wallColor;
    [SerializeField] protected MeshRenderer floor;
    [SerializeField] MeshRenderer cloudsFront;
    [SerializeField] MeshRenderer cloudsBack;
    [SerializeField] int backgroundSortingOrder;

    [Header("Arena Objects")]
    [SerializeField] ArenaObject[] arenaObjects;

    [Header("Colors")]
    [SerializeField] Color[] backgroundColor = new Color[6];
    [SerializeField] Color[] cloudsColors = new Color[6];
    [SerializeField] Color crowdColor = new Color(1,1,1);

    // Components
    Image backgroundImage;
    ArenaController arena;

    // Use this for initialization
    protected virtual void Start()
    {
        // Los fondos deben estar al fondo, como su nombre lo indica
        string backgroundSortingLayerName = JankenUp.SpritesSortingLayers.ArenaBackground;

        background.worldCamera = Camera.main;
        background.sortingLayerName = backgroundSortingLayerName;
        background.sortingOrder = backgroundSortingOrder - 3;
        cloudsFront.sortingLayerName = backgroundSortingLayerName;
        cloudsFront.sortingOrder = backgroundSortingOrder - 2;
        cloudsBack.sortingLayerName = backgroundSortingLayerName;
        cloudsBack.sortingOrder = backgroundSortingOrder - 2;
        wall.sortingLayerName = backgroundSortingLayerName;
        wall.sortingOrder = backgroundSortingOrder;
        floor.sortingLayerName = backgroundSortingLayerName;
        floor.sortingOrder = backgroundSortingOrder;

        // Obtener los elementos de fondo recurrente y la arena
        backgroundImage = background.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Cambiar el estado de active
    /// </summary>
    /// <param name="active"></param>
    public virtual void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// Cambio de color del fondo
    /// </summary>
    /// <param name="backgroundColorIndex"></param>
    public virtual void Config(int backgroundColorIndex)
    {
        // Comprobar el valor predeterminado para el fondo
        if (backgroundColorIndex >= JankenUp.Arenas.backgroundLenght) backgroundColorIndex = -1;

        // Configurar el color de fondo
        int index = backgroundColorIndex != -1? backgroundColorIndex : Random.Range(0, backgroundColor.Length);

        if (backgroundImage == null)
        {
            backgroundImage = background.GetComponent<Image>();
        }

        backgroundImage.color = backgroundColor[index];
        cloudsFront.material.color = cloudsColors[index];
        cloudsBack.material.color = cloudsColors[index];
    }

    /// <summary>
    /// Por defecto, se realiza un random del color de fondo
    /// </summary>
    public void Config()
    {
        Config(-1);
    }

    /// <summary>
    /// Obtencion del identificador del background
    /// </summary>
    /// <returns></returns>
    public JankenUp.BackgroundElementsIdentifier GetIdentifier()
    {
        return identifier;
    }

    /// <summary>
    /// Obtencion del color a utilizar en el publico
    /// </summary>
    /// <returns></returns>
    public Color GetCrowdColor()
    {
        return crowdColor;
    }

    /// <summary>
    /// Realizar un salto de los objetos asociados al elemento. Llamado desde el controlador de arena
    /// </summary>
    /// <param name="minForce"></param>
    /// <param name="maxForce"></param>
    /// <param name="multiplier"></param>
    public virtual void Jump(float minForce, float maxForce, int multiplier){
        // Aplicar con distinta fuerza a los distintos arcades
        foreach (ArenaObject arenaObject in arenaObjects)
        {
            if(arenaObject) arenaObject.Jump(minForce, maxForce, multiplier);
        }
    }
}