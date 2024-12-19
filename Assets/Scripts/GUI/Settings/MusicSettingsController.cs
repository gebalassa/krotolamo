using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicSettingsController : MonoBehaviour, SettingsOptionInterface
{

    [SerializeField] Slider slider;

    [Header("Labels")]
    [SerializeField] TextMeshProUGUI musicLabel;

    // Utiles
    bool readyForChanges = false;

    private void Start()
    {
        slider.value = PlayerPrefs.GetFloat(MasterAudioPlayer.MUSIC_MASTER_VOLUME, MasterAudioPlayer.musicDefaultVolume);
        readyForChanges = true;
        Localize();
        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;
    }

    /// <summary>
    /// Disminuir el volumen de la musica
    /// </summary>
    /// <param name="times"></param>
    public void Decrease(float times = .1f) {
        // Nota: Se ignorara el times original
        times = 0.1f;
        slider.value = Mathf.Clamp(slider.value - times, slider.minValue, slider.maxValue);
    }

    /// <summary>
    /// Aumentar el volumen de la musica
    /// </summary>
    /// <param name="times"></param>
    public void Increase(float times = .1f) {
        times = 0.1f;
        slider.value = Mathf.Clamp(slider.value + times, slider.minValue, slider.maxValue);
    }

    /// <summary>
    /// Cambio en el slider de musica
    /// </summary>
    public void ChangeMusicVolume()
    {
        if (!readyForChanges) return;
        float newValue = Mathf.Clamp(slider.value * MasterAudioPlayer.maxMusicVolume, 0f, MasterAudioPlayer.maxMusicVolume);
        PlayerPrefs.SetFloat(MasterAudioPlayer.MUSIC_MASTER_VOLUME, newValue);

        // Encontrar el MasterAudioPlayer e indicar el cambio de volumen para la musica
        MasterAudioPlayer masterap = FindObjectOfType<MasterAudioPlayer>();
        if (masterap) masterap.ChangeMusicVolume();
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        LocalizationHelper.Translate(musicLabel, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.music);
        UpdateCurrentFont();
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        musicLabel.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        musicLabel.fontStyle = style;
    }

    /// <summary>
    /// Remover suscripciones
    /// </summary>
    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= Localize;
    }

}