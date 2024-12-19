using System.Collections;
using UnityEngine;

public class PartyHorn : MonoBehaviour
{
    [SerializeField] AudioClip[] partyHorns;
    [SerializeField] bool destroy = true;
    [SerializeField] bool soundOn = true;

    // Use this for initialization
    void Start()
    {
        if(soundOn) StartCoroutine(PlaySound());
    }

    /// <summary>
    /// Iniciar la reproduccion y posterior destruccion del elemento
    /// </summary>
    /// <returns></returns>
    IEnumerator PlaySound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        AudioClip clip = partyHorns[Random.Range(0, partyHorns.Length)];
        audioSource.PlayOneShot(clip);
        yield return new WaitForSeconds(clip.length);
        if (destroy) Destroy(gameObject);
    }

    /// <summary>
    /// Indicar si se debe reproducir el sonido o no
    /// </summary>
    /// <param name="soundOn"></param>
    public void SetSoundOn(bool soundOn)
    {
        this.soundOn = soundOn;
    }
}