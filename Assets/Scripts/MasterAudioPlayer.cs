using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MasterAudioPlayer : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] string addresableMixerKey = "Assets/AudioMixer.mixer";
    [SerializeField] AudioMixer mixer;

    [Header("Addresables")]
    public List<MusicAddresableStruct> adressables;

    /// <summary>
    /// Diccionario de canciones y listado de audios
    /// Por cada elemento puede existir 2 o mas elementos
    /// Por concesion, se indica que:
    /// Index 0: Intro de cancion
    /// Index n + 1: Loop de cancion
    /// </summary>
    private Dictionary<string, List<AudioClip>> audioClipsBook = new Dictionary<string, List<AudioClip>>();

    // Reproductor y musicas
    AudioSource audioSourceA;
    AudioSource audioSourceB;
    AudioSource audioSourceC;
    AudioClip menuMusic;

    [Header("Defaults")]
    [SerializeField] [Range(0f, 1f)] public static float musicDefaultVolume = .3f;
    [SerializeField] [Range(0f, 1f)] public static float sfxDefaultVolume = 1f;
    [SerializeField] [Range(0f, 1f)] public static float refereesfxDefaultVolume = 1f;
    [SerializeField] [Range(0f, 1f)] public static float maxMusicVolume = 1f;
    [SerializeField] [Range(0f, 1f)] public static float maxSfxVolume = 1f;

    // Keys para volumentes
    public static string MUSIC_MASTER_VOLUME = "MUSIC_MASTER_VOLUME";
    public static string SFX_MASTER_VOLUME = "SFX_MASTER_VOLUME";
    public static string REFEREE_SFX_MASTER_VOLUME = "REFEREE_SFX_MASTER_VOLUME";

    // Referencia al musicplayer del juego
    public static MasterAudioPlayer _player;

    // Tiempo que demora en variar el volumen de la musica
    private float timeToFade = 2f;

    // Indicador de carga actual
    private List<string> loadingAddressable = new List<string>();

    /// <summary>
    /// Singleton
    /// </summary>
    private void Awake()
    {
        int length = FindObjectsOfType<MasterAudioPlayer>().Length;
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
    IEnumerator Start()
    {
        yield return StartCoroutine(LoadMixer());
        ChangeMusicVolume();
        CheckAudioSources();
        LoadMenuMusic();
    }

    /// <summary>
    /// Comenzar a reproducir la fuente A
    /// </summary>
    public void Play()
    {
        audioSourceA.Play();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Redoble de tambores
    /// NOTA: Solo esta por legacy
    /// </summary>
    public void DrumRoll() { }

    /// <summary>
    /// Detiene la condicion de que no se pueda reproducir otra musica
    /// NOTA: Solo esta por legacy
    /// </summary>
    public void StopDrumRoll() { }

    /// <summary>
    /// Reproducir la música del menú
    /// </summary>
    public void MenuMusic()
    {
        StartCoroutine(InitMenuMusic());
    }

    /// <summary>
    /// Carga de la musica de menu
    /// </summary>
    public void LoadMenuMusic()
    {
        StartCoroutine(LoadAssets(adressables.Find(p => p.identifier == "menu")));
    }

    /// <summary>
    /// Configura y reproduce la musica del menu
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    private IEnumerator InitMenuMusic()
    {
        string identifier = "menu";
        while (audioSourceA.outputAudioMixerGroup == null) yield return null;
        while (!audioClipsBook.ContainsKey(identifier)) yield return null;

        if (audioSourceA.clip != menuMusic || menuMusic == null)
        {
            // Siempre checkar fuentes :O
            CheckAudioSources();

            // Reproducir la musica
            if (audioClipsBook.ContainsKey(identifier) && audioSourceA.clip != audioClipsBook[identifier][0])
            {
                if (menuMusic != audioClipsBook[identifier][0]) menuMusic = audioClipsBook[identifier][0];
                audioSourceA.Stop();
                audioSourceB.Stop();
                audioSourceC.Stop();
                audioSourceA.clip = menuMusic;
                audioSourceA.loop = true;
                audioSourceA.Play();
            }
        }

    }

    /// <summary>
    /// Reproducir la musica de juego seleccionada para la scena
    /// </summary>
    /// <param name="audioClipA"></param>
    /// <param name="audioClipB"></param>
    private void PlayInGameMusic(AudioClip audioClipA, AudioClip audioClipB)
    {
        PlayInGameMusic(audioClipA, audioClipB, 0);
    }

    /// <summary>
    /// Reproducir la musica de juego seleccionada para la scena
    /// </summary>
    /// <param name="audioClipA"></param>
    /// <param name="audioClipB"></param>
    /// <param name="delay"></param>
    private void PlayInGameMusic(AudioClip audioClipA, AudioClip audioClipB, float delay)
    {
        audioSourceA.clip = audioClipA;
        audioSourceA.loop = false;
        audioSourceB.clip = audioClipB;
        audioSourceB.loop = true;
        audioSourceA.Play();
        audioSourceB.PlayDelayed(delay != 0 ? delay : audioClipA.length);
    }

    /// <summary>
    /// Reproduccion de la musica en orden segun lista, alternando los audioSources
    /// </summary>
    /// <param name="audioClips"></param>
    /// <param name="delay"></param>
    private void PlayInGameMusicWithPreloop(List<AudioClip> audioClips, float delay)
    {
        audioSourceA.clip = audioClips[0];
        audioSourceA.loop = false;
        audioSourceB.clip = audioClips[1];
        audioSourceB.loop = false;
        audioSourceC.clip = audioClips[2];
        audioSourceC.loop = true;
        audioSourceA.Play();
        audioSourceB.PlayDelayed(audioSourceA.clip.length);
        audioSourceC.PlayDelayed(audioSourceA.clip.length + audioSourceB.clip.length);
    }

    /// <summary>
    /// Configura y reproduce la musica asociada al identificador
    /// </summary>
    /// <param name="musicAddresableStruct"></param>
    /// <param name="play"></param>
    /// <returns></returns>
    private IEnumerator InitMusic(MusicAddresableStruct musicAddresableStruct, bool play = true)
    {
        // Esperar si es que ya se esta cargando el mismo elemento
        while (loadingAddressable.Contains(musicAddresableStruct.identifier)) yield return null;

        // Siempre checkar fuentes :O
        CheckAudioSources();

        string identifier = musicAddresableStruct.identifier;
        float delay = musicAddresableStruct.delay;
        bool fixedOrder = musicAddresableStruct.fixedOrder;

        // Revisar existencia de los clips y cargar de no estar diponibles
        if (!audioClipsBook.ContainsKey(identifier)) yield return LoadAssets(adressables.Find(p => p.identifier == identifier));

        // Reproducir la musica
        if (play && audioClipsBook.ContainsKey(identifier) && audioClipsBook.Count >= 2 && audioSourceA.clip != audioClipsBook[identifier][0]){
            if (fixedOrder){
                PlayInGameMusicWithPreloop(audioClipsBook[identifier], delay);
            }
            else{
                PlayInGameMusic(audioClipsBook[identifier][0], audioClipsBook[identifier][Random.Range(1, audioClipsBook[identifier].Count)], delay);
            }
            
        }
    }

    /// <summary>
    /// Cambio en el volumen de la musica
    /// </summary>
    public void ChangeMusicVolume()
    {
        CheckAudioSources();
        mixer.SetFloat("MusicVolume", PercentToDB(PlayerPrefs.GetFloat(MUSIC_MASTER_VOLUME, musicDefaultVolume)));
    }

    /// <summary>
    /// Cambio en el pitch de ambas audiosources
    /// </summary>
    /// <param name="pitch"></param>
    public void ChangePitchAudio(float pitch)
    {
        mixer.SetFloat("MusicPitch", pitch);
        mixer.SetFloat("SFXPitch", pitch);
        mixer.SetFloat("RefereeSFXPitch", pitch);
    }

    /// <summary>
    /// Cambio en el pitch del audiosourceA
    /// </summary>
    /// <param name="pitch"></param>
    public void ChangePitchAudioA(float pitch)
    {
        if (!audioSourceA) audioSourceA = GetComponents<AudioSource>()[0];
        audioSourceA.pitch = pitch;
    }

    /// <summary>
    /// Cambio en el pitch del audiosourceB
    /// </summary>
    /// <param name="pitch"></param>
    public void ChangePitchAudioB(float pitch)
    {
        if (!audioSourceB) audioSourceB = GetComponents<AudioSource>()[1];
        audioSourceB.pitch = pitch;
    }

    /// <summary>
    /// Se obtendra el pitch actual tomando en cuenta la audioSourceA
    /// </summary>
    /// <returns></returns>
    public float GetPitchAudio()
    {
        return audioSourceA.pitch;
    }

    /// <summary>
    /// Disminucion del volumen de las audiosources de manera paulatina
    /// </summary>
    /// <param name="up"></param>
    public void FadeAudioSource(bool up)
    {
        FadeAudioSource(up, 1);
    }

    /// <summary>
    /// Disminucion del volumen para los supers
    /// </summary>
    /// <param name="up"></param>
    public void FadeAudioSourceSuper(bool up)
    {
        FadeAudioSource(up, 0.85f);
    }

    /// <summary>
    /// Disminucion del volumen de las audiosources de manera paulatina
    /// </summary>
    /// <param name="up"></param>
    /// <param name="max"></param>
    public void FadeAudioSource(bool up, float max)
    {
        // Obtener el volumen actual del audiosource
        float currentVolume = PlayerPrefs.GetFloat(MUSIC_MASTER_VOLUME, musicDefaultVolume);
        float from = up ? max : 0;
        float to = up ? 0 : max;

        System.Action<ITween<float>> reduceVolume = (t) =>
        {
            mixer.SetFloat("MusicVolume", PercentToDB(currentVolume - (currentVolume * t.CurrentValue)));
        };

        // Que se vaya alejando
        gameObject.Tween(string.Format("ReduceVolume{0}", audioSourceA.GetInstanceID()), from, to,
            timeToFade, TweenScaleFunctions.Linear, reduceVolume);

    }

    /// <summary>
    /// Comprobacion de que audioSources esten cargadas
    /// </summary>
    private void CheckAudioSources()
    {
        if (!audioSourceA) audioSourceA = GetComponents<AudioSource>()[0];
        if (!audioSourceB) audioSourceB = GetComponents<AudioSource>()[1];
        if (!audioSourceC) audioSourceC = GetComponents<AudioSource>()[2];
    }

    /// <summary>
    /// Llamado para tocar una cancion dentro de la zona de juego
    /// </summary>
    /// <param name="music"></param>
    /// <param name="play"></param>
    public void PlayOrLoadThis(Music music, bool play = true)
    {
        MusicAddresableStruct? candidateStruct = adressables.Find(a => a.identifier.ToLower() == music.ToString().ToLower());
        MusicAddresableStruct musicAddresableStruct = !candidateStruct.Equals(default(MusicAddresableStruct)) ? candidateStruct.Value : adressables.Find(a => a.identifier.ToLower() == "default");

        switch (musicAddresableStruct.identifier)
        {
            case "menu":
                MenuMusic();
                break;
            default:
                StartCoroutine(InitMusic(musicAddresableStruct, play));
                break;
        }
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

    /// <summary>
    /// Carga de assets necesarios para la cancion de juego
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadAssets(MusicAddresableStruct musicAddresableStruct)
    {
        if (!loadingAddressable.Contains(musicAddresableStruct.identifier))
        {
            loadingAddressable.Add(musicAddresableStruct.identifier);

            // Realizar la carga de los demas elementos
            foreach (string key in musicAddresableStruct.addresablesKey)
            {
                yield return LoadAsset(musicAddresableStruct.identifier, key);
            }

            loadingAddressable.Remove(musicAddresableStruct.identifier);
        }
    }

    /// <summary>
    /// Carga de un asset especifico
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private IEnumerator LoadAsset(string identifier, string key)
    {
        AsyncOperationHandle<AudioClip> opHandle = Addressables.LoadAssetAsync<AudioClip>(key);
        AudioClip result;
        yield return opHandle;
        int index = 0;
        if (opHandle.Status == AsyncOperationStatus.Succeeded)
        {
            result = opHandle.Result;
            AssignAudioClip(identifier, result, index);
        }

        // Realizar la liberacion del recurso
        Addressables.Release(opHandle);
    }

    /// <summary>
    /// Asigna un audioClip al personaje que le corresponda
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="audioClip"></param>
    /// <param name="index"></param>
    private void AssignAudioClip(string identifier, AudioClip audioClip, int index)
    {
        if (!audioClipsBook.ContainsKey(identifier)) audioClipsBook.Add(identifier, new List<AudioClip>());
        audioClipsBook[identifier].Add(audioClip);
    }

    /// <summary>
    /// Carga del mixer para las audioSource
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private IEnumerator LoadMixer()
    {
        AsyncOperationHandle<AudioMixer> opHandle = Addressables.LoadAssetAsync<AudioMixer>(addresableMixerKey);
        yield return opHandle;
        if (opHandle.Status == AsyncOperationStatus.Succeeded)
        {
            mixer = opHandle.Result;
            CheckAudioSources();
            audioSourceA.outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
            audioSourceB.outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
        }
    }

    /// <summary>
    /// Agrega una estructura de musica de personaje
    /// </summary>
    /// <param name="musicAddresableStruct"></param>
    public void AddToAdressables(MusicAddresableStruct musicAddresableStruct)
    {
        adressables.Add(musicAddresableStruct);
    }
}
