using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefereeSfxSettingsController : MonoBehaviour, SettingsOptionInterface
{

    [SerializeField] Slider slider;
    [SerializeField] AudioClip clipToTest;

    [Header("Labels")]
    [SerializeField] TextMeshProUGUI sfxLabel;

    // Utiles
    float timeBeforPlayTestSFX = .1f;
    bool readyForChanges = false;

    private void Start()
    {
        slider.value = PlayerPrefs.GetFloat(MasterAudioPlayer.REFEREE_SFX_MASTER_VOLUME, MasterAudioPlayer.refereesfxDefaultVolume);
        readyForChanges = true;
        Localize();
        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;
    }

    /// <summary>
    /// Disminuir el volumen de los sonidos
    /// </summary>
    /// <param name="times"></param>
    public void Decrease(float times = .1f) {
        // Nota: Se ignorara el times original
        times = 0.1f;
        slider.value = Mathf.Clamp(slider.value - times, slider.minValue, slider.maxValue);
    }

    /// <summary>
    /// Aumentar el volumen de los sonidos
    /// </summary>
    /// <param name="times"></param>
    public void Increase(float times = .1f) {
        times = 0.1f;
        slider.value = Mathf.Clamp(slider.value + times, slider.minValue, slider.maxValue);
    }

    /// <summary>
    /// Cambio en el slider de SFX
    /// </summary>
    public void ChangeSFXVolume()
    {
        if (!readyForChanges) return;
        float newValue = Mathf.Clamp(slider.value * MasterAudioPlayer.maxSfxVolume, 0f, MasterAudioPlayer.maxSfxVolume);
        PlayerPrefs.SetFloat(MasterAudioPlayer.REFEREE_SFX_MASTER_VOLUME, newValue);

        // Encontrar el MasterRefereeSFXPlayer
        MasterRefereeSFXPlayer masterSFXp = FindObjectOfType<MasterRefereeSFXPlayer>();
        if (masterSFXp) masterSFXp.ChangeSFXVolume();

        // Reproducir el sonido del prueba para saber el nivel del audio
        StopAllCoroutines();
        if (MasterRefereeSFXPlayer._player != null) MasterRefereeSFXPlayer._player.Playtest();
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        LocalizationHelper.Translate(sfxLabel, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.referee);
        UpdateCurrentFont();
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        sfxLabel.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        sfxLabel.fontStyle = style;
    }

    /// <summary>
    /// Remover suscripciones
    /// </summary>
    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= Localize;
    }

}