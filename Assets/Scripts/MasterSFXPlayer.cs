using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MasterSFXPlayer : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] public AudioMixer mixer;

    [Header("Sonidos Comunes")]
    [SerializeField] AudioClip uiClick;
    [SerializeField] AudioClip gong;
    [SerializeField] AudioClip lifeUp;
    [SerializeField] AudioClip winSuperPower;
    [SerializeField] AudioClip selectedAttack;
    [SerializeField] AudioClip coins;
    [SerializeField] AudioClip error;
    [SerializeField] AudioClip strongHit;
    [SerializeField] AudioClip toing;

    // Reproductor
    AudioSource audioSource;

    // Referencia al sfxplayer del juego
    public static MasterSFXPlayer _player;
    float timeBeforPlayTestSFX = .1f;

    // Singleton
    private void Awake()
    {
        int length = FindObjectsOfType<MasterSFXPlayer>().Length;
        if (length == 1)
        {
            DontDestroyOnLoad(gameObject);
            _player = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeSFXVolume();
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Reproduccion de la prueba de sonido
    /// </summary>
    public void Playtest()
    {
        StopAllCoroutines();
        StartCoroutine(PlayTestSFX());
    }

    /// <summary>
    /// Realizar prueba de sonido al hacer un cambio en el slide de sonidos
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayTestSFX()
    {
        // Si el tiempo esta pausado, reproducir inmediatamente el sonido
        yield return new WaitForSeconds(timeBeforPlayTestSFX);
        if (Time.timeScale != 0) audioSource.Play();
    }

    // Hacer sonar un audioclip
    public void PlayOneShot(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }

    /// <summary>
    /// Sonar un clip luego de X segundos
    /// </summary>
    /// <param name="audioClip"></param>
    /// <param name="afterSeconds"></param>
    /// <returns></returns>
    public void PlayOneShot(AudioClip audioClip, float afterSeconds)
    {
        StartCoroutine(PlayOneShotAfter(audioClip, afterSeconds));
    }

    private IEnumerator PlayOneShotAfter(AudioClip audioClip, float afterSeconds) {
        yield return new WaitForSeconds(afterSeconds);
        audioSource.PlayOneShot(audioClip);
    }

    // Sonido de click en UI
    public void UISFX()
    {
        PlayOneShot(uiClick);
    }

    // Sonido de gong
    public void GongSFX()
    {
        PlayOneShot(gong);
    }

    // Sonido de ganar vida
    public void LifeUp()
    {
        PlayOneShot(lifeUp);
    }

    // Sonido de ganar superpoder
    public void WinSuperPower()
    {
        PlayOneShot(winSuperPower);
    }

    // Sonido de seleccion ataque
    public void SelectAttack()
    {
        PlayOneShot(selectedAttack);
    }

    // Sonido de monedas
    public void Coins()
    {
        PlayOneShot(coins);
    }

    // Sonido de error
    public void Error()
    {
        PlayOneShot(error);
    }

    // Sonido de golpe fuerte
    public void StrongHit()
    {
        PlayOneShot(strongHit);
    }

    // Sonido de tooooinnnng
    public void Toing()
    {
        PlayOneShot(toing);
    }

    // Cambiar el volumen del audiosource para efectos
    public void ChangeSFXVolume()
    {
        mixer.SetFloat("SFXVolume", PercentToDB(PlayerPrefs.GetFloat(MasterAudioPlayer.SFX_MASTER_VOLUME, MasterAudioPlayer.sfxDefaultVolume)));
    }

    /// <summary>
    /// Obtencion del valor en decibeles del sonido en base a su porcentaje
    /// </summary>
    /// <param name="percent"></param>
    /// <returns></returns>
    public float PercentToDB(float percent)
    {
        return 20 * Mathf.Log10(percent);
    }

}
