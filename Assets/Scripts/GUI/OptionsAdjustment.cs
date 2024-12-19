using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArabicSupport;

public class OptionsAdjustment : MonoBehaviour
{

    [Header("Sliders")]
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [Header("Posicion de los controles")]
    [SerializeField] Image leftControl;
    [SerializeField] Image rightControl;
    [SerializeField] Color controlActiveColor;
    [SerializeField] Color controlInactiveColor;

    [Header("Vibracion")]
    [SerializeField] Image vibrateButtonYes;
    [SerializeField] Image vibrateButtonNo;

    [Header("Labels")]
    [SerializeField] TextMeshProUGUI musicLabel;
    [SerializeField] TextMeshProUGUI sfxLabel;
    [SerializeField] TextMeshProUGUI controlsLabel;
    [SerializeField] Text controlLeftLabel;
    [SerializeField] Text controlRightLabel;
    [SerializeField] TextMeshProUGUI vibrateLabel;
    [SerializeField] Text vibrateYesLabel;
    [SerializeField] Text vibrateNoLabel;
    [SerializeField] TextMeshProUGUI languageLabel;

    // Audiolistener usado para probar el volumen SFX
    AudioSource audioSource;
    float timeBeforPlayTestSFX = .1f;
    bool readyForChanges = false;

    void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat(MasterAudioPlayer.MUSIC_MASTER_VOLUME, MasterAudioPlayer.musicDefaultVolume);
        sfxSlider.value = PlayerPrefs.GetFloat(MasterAudioPlayer.SFX_MASTER_VOLUME, MasterAudioPlayer.sfxDefaultVolume);
        audioSource = GetComponent<AudioSource>();

        int position = PlayerPrefs.GetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, PlayersPreferencesController.CONTROLPOSITIONKEYDEFAULT);
        if (position == 0) SelectLeftControl();
        else SelectRightControl();

        if (PlayersPreferencesController.CanVibrate())
        {
            SetVibrateYes();
        }
        else
        {
            SetVibrateNo();
        }

        readyForChanges = true;

        // Actualizar la fuente de todos los labels
        UpdateCurrentFont();
    }

    // Cambio en el slider de musica
    public void ChangeMusicVolume()
    {
        if (!readyForChanges) return;
        float newValue = Mathf.Clamp(musicSlider.value * MasterAudioPlayer.maxMusicVolume, 0f, MasterAudioPlayer.maxMusicVolume);
        PlayerPrefs.SetFloat(MasterAudioPlayer.MUSIC_MASTER_VOLUME, newValue);

        // Encontrar el MasterAudioPlayer e indicar el cambio de volumen para la musica
        MasterAudioPlayer masterap = FindObjectOfType<MasterAudioPlayer>();
        if (masterap) masterap.ChangeMusicVolume();
    }

    // Cambio en el slider de SFX
    public void ChangeSFXVolume()
    {
        if (!readyForChanges) return;
        float newValue = Mathf.Clamp(sfxSlider.value * MasterAudioPlayer.maxSfxVolume, 0f, MasterAudioPlayer.maxSfxVolume);
        PlayerPrefs.SetFloat(MasterAudioPlayer.SFX_MASTER_VOLUME, newValue);


        // Encontrar el MasterSFXPlayer e indicar el cambio de volumen para los sonidos generales
        MasterSFXPlayer masterSFXp = FindObjectOfType<MasterSFXPlayer>();
        if (masterSFXp) masterSFXp.ChangeSFXVolume();

        // Reproducir el sonido del prueba para saber el nivel del audio
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        StopAllCoroutines();
        StartCoroutine(PlayTestSFX());
    }

    IEnumerator PlayTestSFX()
    {
        // Si el tiempo esta pausado, reproducir inmediatamente el sonido
        if(Time.timeScale == 0) audioSource.Play();
        yield return new WaitForSeconds(timeBeforPlayTestSFX);
        if (Time.timeScale != 0) audioSource.Play();

    }

    // Seleccion del control izquierdo
    public void SelectLeftControl()
    {
        PlayerPrefs.SetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, 0);
        leftControl.color = controlActiveColor;
        rightControl.color = controlInactiveColor;
    }

    // Seleccion del control izquierdo
    public void SelectRightControl()
    {
        PlayerPrefs.SetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, 1);
        rightControl.color = controlActiveColor;
        leftControl.color = controlInactiveColor;
    }

    // Seleccion de si vibra o no el celular
    public void SetVibrateYes()
    {
        PlayerPrefs.SetInt(PlayersPreferencesController.VIBRATE, 1);
        vibrateButtonYes.color = controlActiveColor;
        vibrateButtonNo.color = controlInactiveColor;
    }

    // Seleccion del control izquierdo
    public void SetVibrateNo()
    {
        PlayerPrefs.SetInt(PlayersPreferencesController.VIBRATE, 0);
        vibrateButtonYes.color = controlInactiveColor;
        vibrateButtonNo.color = controlActiveColor;
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        musicLabel.font = mainFont;
        sfxLabel.font = mainFont;
        controlsLabel.font = mainFont;
        vibrateLabel.font = mainFont;
        languageLabel.font = mainFont;

        Font plainFont = FontManager._mainManager.GetPlainFont();
        controlLeftLabel.font = plainFont;
        controlRightLabel.font = plainFont;
        vibrateYesLabel.font = plainFont;
        vibrateNoLabel.font = plainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        musicLabel.fontStyle = style;
        sfxLabel.fontStyle = style;
        controlsLabel.fontStyle = style;
        vibrateLabel.fontStyle = style;
        languageLabel.fontStyle = style;
    }

    /* Los siguientes metodos cumplen la funcion de actualizar los distintos labels que aparecen la pantalla*/
    public void UpdateMusicString(string text)
    {
        if (FontManager._mainManager.IsArabic())
        {
            musicLabel.text = ArabicFixer.Fix(text);
        }
        else
        {
            musicLabel.text = text;
        }
    }

    public void UpdatSFXString(string text)
    {
        if (FontManager._mainManager.IsArabic())
        {
            sfxLabel.text = ArabicFixer.Fix(text);
        }
        else
        {
            sfxLabel.text = text;
        }
    }

    public void UpdateControlsString(string text)
    {
        if (FontManager._mainManager.IsArabic())
        {
            controlsLabel.text = ArabicFixer.Fix(text);
        }
        else
        {
            controlsLabel.text = text;
        }
    }

    public void UpdateLeftControlString(string text)
    {
        if (FontManager._mainManager.IsArabic())
        {
            controlLeftLabel.text = ArabicFixer.Fix(text);
        }
        else
        {
            controlLeftLabel.text = text;
        }
    }

    public void UpdateRightControlString(string text)
    {
        if (FontManager._mainManager.IsArabic())
        {
            controlRightLabel.text = ArabicFixer.Fix(text);
        }
        else
        {
            controlRightLabel.text = text;
        }
    }

    public void UpdateVibrationString(string text)
    {
        if (FontManager._mainManager.IsArabic())
        {
            vibrateLabel.text = ArabicFixer.Fix(text);
        }
        else
        {
            vibrateLabel.text = text;
        }
    }

    public void UpdateVibrateYesString(string text)
    {
        if (FontManager._mainManager.IsArabic())
        {
            vibrateYesLabel.text = ArabicFixer.Fix(text);
        }
        else
        {
            vibrateYesLabel.text = text;
        }
    }

    public void UpdateVibrateNoString(string text)
    {
        if (FontManager._mainManager.IsArabic())
        {
            vibrateNoLabel.text = ArabicFixer.Fix(text);
        }
        else
        {
            vibrateNoLabel.text = text;
        }
    }
    public void UpdateLanguageString(string text)
    {
        if (FontManager._mainManager.IsArabic())
        {
            languageLabel.text = ArabicFixer.Fix(text);
        }
        else
        {
            languageLabel.text = text;
        }
    }

    // Reproducir sonido de boton
    public void UIButtonSFX()
    {
        MasterSFXPlayer._player.UISFX();
    }
}