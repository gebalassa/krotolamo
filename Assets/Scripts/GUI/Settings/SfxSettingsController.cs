using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SfxSettingsController : MonoBehaviour, SettingsOptionInterface
{

    [SerializeField] Slider slider;

    [Header("Labels")]
    [SerializeField] TextMeshProUGUI sfxLabel;

    // Utiles
    AudioSource audioSource;
    float timeBeforPlayTestSFX = .1f;
    bool readyForChanges = false;

    private void Start()
    {
        slider.value = PlayerPrefs.GetFloat(MasterAudioPlayer.SFX_MASTER_VOLUME, MasterAudioPlayer.sfxDefaultVolume);
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
        PlayerPrefs.SetFloat(MasterAudioPlayer.SFX_MASTER_VOLUME, newValue);


        // Encontrar el MasterSFXPlayer e indicar el cambio de volumen para los sonidos generales
        MasterSFXPlayer masterSFXp = FindObjectOfType<MasterSFXPlayer>();
        if (masterSFXp) masterSFXp.ChangeSFXVolume();

        // Reproducir el sonido del prueba para saber el nivel del audio
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        StopAllCoroutines();
        if (MasterSFXPlayer._player != null) MasterSFXPlayer._player.Playtest();
    }

    /// <summary>
    /// Realizar prueba de sonido al hacer un cambio en el slide de sonidos
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayTestSFX()
    {
        // Si el tiempo esta pausado, reproducir inmediatamente el sonido
        if (Time.timeScale == 0) audioSource.Play();
        yield return new WaitForSeconds(timeBeforPlayTestSFX);
        if (Time.timeScale != 0) audioSource.Play();
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        LocalizationHelper.Translate(sfxLabel, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.sounds);
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