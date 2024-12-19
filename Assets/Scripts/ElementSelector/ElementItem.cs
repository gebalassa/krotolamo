using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalRuby.Tween;

public class ElementItem: MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject header;
    [SerializeField] Text title;
    [SerializeField] Image illustration;
    [SerializeField] GameObject border;

    [Header("Fade")]
    [SerializeField] [Range(0, 1)] float selectedAlpha = 1f;
    [SerializeField] [Range(0, 1)] float unselectedAlpha = 0.8f;
    [SerializeField] [Range(0, 2)] float fadeTime = .5f;

    Button button;

    // Objeto asociado
    GameObject elementSource;

    // Estados y propiedades
    bool selected = false;
    string identifier = "";

    // Delegados para acciones sobre click
    public delegate void OnClickDelegate(ElementItem elementItem);
    public OnClickDelegate onClickDelegate;

    // Materiales para ilustraciones y elementos
    private Material normalMaterial;
    private Material blackAndWhiteMaterial;
    private CanvasGroup canvasGroup;

    // Tweens
    private Tween<float> tweenFade;

    /// <summary>
    /// Copia del material asociado a la imagen
    /// </summary>
    private void Awake()
    {
        normalMaterial = illustration.material;
        blackAndWhiteMaterial = new Material(illustration.material);
        blackAndWhiteMaterial.SetFloat("_GrayscaleAmount", 1);
    }

    /// <summary>
    /// Inicializar los componentes necesarios
    /// </summary>
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Llamar a los eventos suscritos
    /// </summary>
    void OnClick()
    {
        if (onClickDelegate != null) onClickDelegate(this);
    }

    /// <summary>
    /// Obtencion del elemento desde donde se obtiene la data
    /// </summary>
    /// <returns></returns>
    public GameObject GetElementSource()
    {
        return elementSource;
    }

    /// <summary>
    /// Guardado del elemento fuente y configuracion del item
    /// </summary>
    /// <param name="element"></param>
    public void SetElementSource(GameObject element)
    {
        elementSource = element;
        SetData();
    }

    /// <summary>
    /// Set de toda la data del elemento base
    /// </summary>
    private void SetData()
    {
        IElementItem itemData = elementSource? elementSource.GetComponent<IElementItem>() : null;
        if (itemData != null)
        SetIdentifier(itemData.GetIdentifier());
        SetTitle(itemData.GetName());
        SetIllustration(itemData.GetAvatar());
        UpdateBlackAndWhite(!itemData.IsUnlocked());
    }

    /// <summary>
    /// Indica si se debe mostrar o no el header
    /// </summary>
    /// <param name="show"></param>
    public void ShowHeader(bool show = true)
    {
        header.SetActive(show);
    }

    /// <summary>
    /// Cambio del identificador del elemento
    /// </summary>
    /// <param name="newIdentifier"></param>
    private void SetIdentifier(string newIdentifier) {
        identifier = newIdentifier;
    }

    /// <summary>
    /// Obtencion del identificador
    /// </summary>
    /// <returns></returns>
    public string GetIdentifier()
    {
        return identifier;
    }

    /// <summary>
    /// Cambio en el titulo del elemento
    /// </summary>
    /// <param name="newTitle"></param>
    private void SetTitle(string newTitle) {
        title.text = newTitle;
    }

    /// <summary>
    /// Cambio en la ilustracion del elemento
    /// </summary>
    /// <param name="newIllustration"></param>
    private void SetIllustration(Sprite avatar) {
        illustration.sprite = avatar;
    }

    /// <summary>
    /// Indicar que elemento esta o no seleccionado
    /// </summary>
    /// <param name="value"></param>
    public void Select(bool value = true) {
        selected = value;
        Fade(selected? selectedAlpha : unselectedAlpha);
        //border.SetActive(selected);
    }

    /// <summary>
    /// Realizar un fade sobre el elemento
    /// </summary>
    /// <param name="to"></param>
    private void Fade(float to)
    {
        // Cambiar alpha
        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        float from = canvasGroup.alpha;
        if(tweenFade != null)
        {
            tweenFade.Stop(TweenStopBehavior.DoNotModify);
            from = tweenFade.CurrentValue;
        }

        // Hacer fade in y mostrar elemento
        tweenFade = gameObject.Tween(string.Format("Fade{0}", GetInstanceID()), from, to, fadeTime, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);
    }

    /// <summary>
    /// Indicar si imagen debe estar en blanco y negro
    /// </summary>
    protected virtual void UpdateBlackAndWhite(bool blackAndWhite)
    {
        illustration.material = blackAndWhite ? blackAndWhiteMaterial : normalMaterial;
    }

    /// <summary>
    /// Actualizacion del elemento para reflejar el estado actual del elemento base
    /// </summary>
    public void Refresh()
    {
        SetData();
    }
}