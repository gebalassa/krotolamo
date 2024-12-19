using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DigitalRuby.Tween;

public class FeintController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] FeintTrigger triggerButton;
    [SerializeField] TextMeshProUGUI feintText;
    [SerializeField] FeintExtraItem feintExtraText;
    [SerializeField] FeintExtraItem feintExtraImage;
    [SerializeField] FeintButton rockButton;
    [SerializeField] FeintButton paperButton;
    [SerializeField] FeintButton scissorButton;

    // Indica si el controlador esta activo o no
    private bool isActive = false;

    // Indica si el controlador esta abierto o no
    private bool opened = false;

    // Indica si es posible selecionar una finta o no
    private bool selected = false;
    private Attacks attack;

    // Controlador de partida
    private GamePlayScene gamePlayScene;

    // Indica si se esta reportando la finta
    private bool isReporting = false;

    private void Start()
    {
        Localization();
        // Encontrar el controlador de la partida actual
        gamePlayScene = FindObjectOfType<GamePlayScene>();
        Show();
    }

    /// <summary>
    /// Cambiar el texto y la fuente del mensaje
    /// </summary>
    private void Localization()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        feintText.font = mainFont;
        LocalizationHelper.Translate(feintText, JankenUp.Localization.tables.Online.tableName, JankenUp.Localization.tables.Online.Keys.feint);
    }

    /// <summary>
    /// Indica si el controlador esta activo o no
    /// </summary>
    /// <returns></returns>
    public bool IsActive()
    {
        return isActive;
    }

    /// <summary>
    /// Muestra el controlador de fintas
    /// </summary>
    public void Show()
    {
        if (isActive) return;
        // Animar la aparición del elemento finta
        triggerButton.AnimationShow();
        ResetController();
        isActive = true;
    }

    /// <summary>
    /// Oculta el controlador y sus elementos
    /// </summary>
    public void Hide()
    {
        if (!isActive) return;
        // Ocultar todos los elementos
        triggerButton.AnimationHide();
        Toggle(false);
        isActive = false;
    }

    /// <summary>
    /// Mostrar/Ocultar opciones
    /// </summary>
    /// <param name="value">Valor fijo para cambiar el toggle</param>
    public void Toggle(bool value)
    {
        if (opened == value || isReporting) return;
        opened = value;

        // Si se deben mostrar las opciones
        if (value)
        {
            rockButton.AnimationShow();
            paperButton.AnimationShow();
            scissorButton.AnimationShow();
        }
        else
        {
            rockButton.AnimationHide();
            paperButton.AnimationHide();
            scissorButton.AnimationHide();
        }

        // Animar todo, por ahora sera mostrar los elementos para probar
        if (opened)
        {
            feintExtraText.AnimationHide();
            feintExtraImage.AnimationShow();
        }
        else
        {
            feintExtraText.AnimationShow();
            feintExtraImage.AnimationHide();
        }
    }

    public void Toggle() {
        Toggle(!opened);
    }

    /// <summary>
    /// Seleccion del ataque a enviar como finta
    /// </summary>
    /// <param name="attack">Ataque selecionado. Ver equivalencia de los ataques en namespace JanKenUP</param>
    /// <param name="final">Indica si es la decision final de la seleccion de finta</param>
    public void SelectAttack(Attacks attack, bool final)
    {
        this.attack = attack;
        selected = true;
        if (final)
        {
            ReportFeint();
        }
        else
        {
            // Significa que llego por arrastre al boton
            switch (attack)
            {
                case Attacks.Rock:
                    paperButton.AnimationBlur();
                    scissorButton.AnimationBlur();
                    break;
                case Attacks.Paper:
                    rockButton.AnimationBlur();
                    scissorButton.AnimationBlur();
                    break;
                case Attacks.Scissors:
                    rockButton.AnimationBlur();
                    paperButton.AnimationBlur();
                    break;
            }
        }
    }

    /// <summary>
    /// Deseleccionar la finta
    /// </summary>
    public void DeSelectAttack()
    {
        this.selected = false;
    }

    /// <summary>
    /// Se se esta realizando un drag desde el trigger, comprobar si se selecciono un ataque
    /// </summary>
    public void OnDragFinish() {
        if (selected) ReportFeint();
    }

    /// <summary>
    /// Reportar al controlador de juego que se ha seleccionado una finta
    /// </summary>
    private void ReportFeint()
    {
        // Resetear controlador
        DeSelectAttack();

        // Indicar a los ataques que no deben permitir mas seleccion
        rockButton.SetCanBeSelected(false);
        paperButton.SetCanBeSelected(false);
        scissorButton.SetCanBeSelected(false);

        // Notificar de la seleccion
        if (gamePlayScene) gamePlayScene.SetFeint(this.attack);

        // Ocultar controlador
        Hide();

        // Evitar que se pueda volver a abrir
        isReporting = true;

    }

    /// <summary>
    /// Cambiar la escala de los elementos que lo necesiten al invertir la escala.
    /// </summary>
    /// <param name="xScale"></param>
    public void FlipElements(float xScale)
    {
        // Por ahora, solo debe cambiar las letras
        feintText.gameObject.transform.localScale = new Vector3(xScale, feintText.gameObject.transform.localScale.y, feintText.gameObject.transform.localScale.z);
    }

    /// <summary>
    /// Volver al controlador a su estado inicial
    /// </summary>
    public void ResetController()
    {
        isReporting = false;
        DeSelectAttack();
    }

}
