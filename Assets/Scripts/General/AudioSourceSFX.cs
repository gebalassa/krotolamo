using System.Collections;
using UnityEngine;

public class AudioSourceSFX : MonoBehaviour
{
    // Componente de audio
    AudioSource audioSource;

    // Cambio del volumen del audio source para los efectos de sonido
    void Start()
    {
        // Asignar al audiosource el mixer
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        //audioSource.outputAudioMixerGroup = MasterSFXPlayer._player.mixer.FindMatchingGroups("SFX")[0];
        UpdateVolume();
    }

    /// <summary>
    /// Volver al volumen original
    /// </summary>
    public void UpdateVolume() {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (audioSource) audioSource.volume = 1;
    }

    // Cambio en el pitch del audiosourceA
    public void ChangePitch(float pitch)
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (audioSource) audioSource.pitch = pitch;
    }

}