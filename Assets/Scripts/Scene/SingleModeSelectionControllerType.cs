using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SingleModeSelectionControllerType : SceneController
{
    [Header("Label")]
    [SerializeField] Text classicText;
    [SerializeField] Text survivalText;
    [SerializeField] Text shopClassicText;
    [SerializeField] Text shopSurvivalText;

    [Header("UIPanels")]
    [SerializeField] GameObject panelWithOutShop;
    [SerializeField] GameObject panelWithShop;
    [SerializeField] JanKenShop janKenShop;

    // Activar el panel que corresponda si ya se ha pasado el nivel tutorial de los superpoderes
    new void Start()
    {
        base.Start();
        bool tutorialComplete = GameController.LoadTutorials().tutorials.Contains(Tutorial.GetSuperPowerLevel());
        panelWithOutShop.SetActive(!tutorialComplete);
        panelWithShop.SetActive(tutorialComplete);

        UpdateCurrentFont();
    }

    // Actualiza todos los elementos ligados a un translate
    protected override void Localize()
    {
        LocalizationHelper.Translate(classicText, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.classicMode);
        LocalizationHelper.Translate(survivalText, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.survivalMode);
        LocalizationHelper.Translate(shopClassicText, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.classicMode);
        LocalizationHelper.Translate(shopSurvivalText, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.survivalMode);
    }

    // Reproducir sonido de boton
    public void UIButtonSFX()
    {
        MasterSFXPlayer._player.UISFX();
    }

    // Funcion llamada despues de comprar el paquete deluxe
    public void AfterPurchaseDeluxe()
    {
        if (janKenShop) janKenShop.SyncUI();
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        Font plainFont = FontManager._mainManager.GetPlainFont();
        classicText.font = plainFont;
        survivalText.font = plainFont;
        shopClassicText.font = plainFont;
        shopSurvivalText.font = plainFont;
    }

}
