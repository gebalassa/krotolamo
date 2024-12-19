using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MasterRefereeSFXPlayer : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] public AudioMixer mixer;

    // Reproductor
    AudioSource audioSource;

    // Referencia al refereeSfxplayer del juego
    public static MasterRefereeSFXPlayer _player;
    float timeBeforPlayTestSFX = .1f;

    // Singleton
    private void Awake()
    {
        int length = FindObjectsOfType<MasterRefereeSFXPlayer>().Length;
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

    // Cambiar el volumen del audiosource para efectos
    public void ChangeSFXVolume()
    {
        mixer.SetFloat("RefereeSFXVolume", PercentToDB(PlayerPrefs.GetFloat(MasterAudioPlayer.REFEREE_SFX_MASTER_VOLUME, MasterAudioPlayer.sfxDefaultVolume)));
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
