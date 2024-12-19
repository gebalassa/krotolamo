using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JankenUp;
using UnityEditor;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterPool : MonoBehaviour, ElementBank
{
    [Header("Characters")]
    [SerializeField] List<GameObject> characters;
    [SerializeField] List<CharacterConfiguration> secretsCharacters;
    [SerializeField] List<string> tutorialCharacters;

    [Header("Custom")]
    [SerializeField] List<string> customCharacterPath = new List<string>();

    public static CharacterPool _instance;
    public static CharacterPool Instance { get { return _instance; } }

    // Personaje seleccionado actualmente por el jugador
    private List<string> currentCharacterIdentifer = new List<string>() { JankenUp.Characters.BOYLEE };

    // Listado de indices de juegadores en duelo
    private List<int> playersIndexAllowedToDuel = new List<int>();

    /// <summary>
    /// Singleton
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
        }
    }

    /// <summary>
    /// Configuraciones iniciales
    /// </summary>
    void Start()
    {
        StartCoroutine(AddCustomCharacters());
        AddSecretCharacter();
        StartCoroutine(CheckInitialCharacter());
    }

    /// <summary>
    /// Solicitud para cambiar al personaje seleccionado en un inicio
    /// </summary>
    private IEnumerator CheckInitialCharacter(int playerIndex = 0)
    {
        // TODO: Falta esperar a la confirmacion de que el inventario del jugador fue desbloqueado. Por ahora se dejara el ultimo PJ selecciado
        yield return null;
        string identifier = GameController.Load().characterIdentifier;
        if (currentCharacterIdentifer.Count - 1 < playerIndex) currentCharacterIdentifer.Add(identifier);
        else currentCharacterIdentifer[playerIndex] = identifier;
    }

    /// <summary>
    /// Obtencion de la cantidad de jugadores de la partida
    /// </summary>
    /// <returns></returns>
    public int GetPlayersCount()
    {
        return currentCharacterIdentifer.Count;
    }

    /// <summary>
    /// Obtener un caracter según el indice
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GameObject Get(int index)
    {
        return characters[index];
    }

    /// <summary>
    /// Obtener según el identificador de PJ
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public GameObject Get(string identifier)
    {
        if (SecretCharactersContains(identifier))
        {
            return SecretCharacterGameObject(identifier);
        }

        foreach (GameObject character in characters)
        {
            CharacterConfiguration chInfo = character.GetComponent<CharacterConfiguration>();
            if (identifier == chInfo.GetIdentifier())
            {
                return character;
            }
        }

        return null;
    }

    /// <summary>
    /// Obtener el indice segun identificador
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public int GetIndexByIdentifier(string identifier)
    {
        int index = -1;

        for (int i = 0; i < characters.Count; i++)
        {
            CharacterConfiguration chInfo = characters[i].GetComponent<CharacterConfiguration>();
            if (identifier == chInfo.GetIdentifier())
            {
                return i;
            }
        }

        return index;
    }

    /// <summary>
    /// Obtener un pj al azar
    /// </summary>
    /// <returns></returns>
    public GameObject Surprise()
    {
        return characters[Random.Range(0, characters.Count)];
    }

    /// <summary>
    /// Obtener la totalidad de personajes
    /// </summary>
    /// <returns></returns>
    public int Length()
    {
        return characters.Count;
    }

    /// <summary>
    /// Obtener el avatar del personaje según su identificador
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public Sprite GetAvatar(string identifier)
    {
        if (SecretCharactersContains(identifier))
        {
            return SecretCharacterAvatar(identifier);
        }

        foreach (GameObject character in characters)
        {
            CharacterConfiguration chInfo = character.GetComponent<CharacterConfiguration>();
            if (identifier == chInfo.GetIdentifier())
            {
                return chInfo.GetAvatar();
            }
        }

        return null;
    }

    /// <summary>
    ///  Obtener el listado de todos los personajes
    /// </summary>
    /// <param name="notAvailableFirst">Indicar si deben ir los que no se tienen primero</param>
    /// <returns>Todos los personajes del juego</returns>
    public List<GameObject> GetAll(bool notAvailableFirst = false)
    {
        if (notAvailableFirst)
        {
            // Copiar los personajes
            List<GameObject> availableCharacters = new List<GameObject>();
            List<GameObject> notAvailableCharacters = new List<GameObject>();
            foreach (GameObject character in characters)
            {
                CharacterConfiguration chInfo = character.GetComponent<CharacterConfiguration>();
                bool canBeBuyed = !chInfo.IsUnlocked();
                if (canBeBuyed) notAvailableCharacters.Add(character);
                else availableCharacters.Add(character);
            }
            notAvailableCharacters.AddRange(availableCharacters);
            return notAvailableCharacters;
        }
        else return characters;
    }

    /// <summary>
    /// Obtener el listado de todos los personajes
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetAll() {
        return GetAll(false);
    }

    /// <summary>
    /// Obtener solo el listado de personajes disponibles
    /// </summary>
    /// <returns>Listado de personajes disponibles para su uso</returns>
    public List<GameObject> GetAvailables()
    {
        List<GameObject> availableCharacters = new List<GameObject>();
        foreach (GameObject character in characters)
        {
            CharacterConfiguration chInfo = character.GetComponent<CharacterConfiguration>();
            if (chInfo.IsUnlocked()) availableCharacters.Add(character);
        }
        return availableCharacters;
    }

    /// <summary>
    /// Agregar los personajes custom
    /// </summary>
    public IEnumerator AddCustomCharacters() {
        foreach (string key in customCharacterPath)
        {
            AsyncOperationHandle<GameObject> opHandle = Addressables.LoadAssetAsync<GameObject>(key);
            GameObject result;
            yield return opHandle;
            if (opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                result = opHandle.Result;
                characters.Add(result);
                MasterAudioPlayer._player.AddToAdressables(result.GetComponent<CharacterConfiguration>().musicAddresableStruct);
            }

            // Realizar la liberacion del recurso
            Addressables.Release(opHandle);
        }

    }

    /// <summary>
    /// Agregar los personajes secretos desbloqueados
    /// </summary>
    public void AddSecretCharacter()
    {
        List<GameObject> unlockedSecretCharacters = new List<GameObject>();
        List<string> currentUnlockedCharacters = new List<string>();

        foreach (GameObject character in characters)
        {
            CharacterConfiguration chInfo = character.GetComponent<CharacterConfiguration>();
            currentUnlockedCharacters.Add(chInfo.GetIdentifier());
        }

        foreach (CharacterConfiguration config in secretsCharacters)
        {
            if (UnlockedCharacterController.IsUnlocked(config.GetIdentifier()) && !currentUnlockedCharacters.Contains(config.GetIdentifier()))
                unlockedSecretCharacters.Add(config.gameObject);
        }

        foreach (GameObject character in unlockedSecretCharacters)
        {
            characters.Add(character);
        }

        // Al llamar esta funcion, se debe ordenar los personajes segun su orden
        characters.Sort((x, y) => x.GetComponent<CharacterConfiguration>().GetOrder().CompareTo(y.GetComponent<CharacterConfiguration>().GetOrder()));
    }

    /// <summary>
    /// Obtener el siguiente PJ disponible dentro del pool
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="allowLocked"></param>
    /// <returns></returns>
    public CharacterConfiguration GetNextCharacterInfoByIdentifier(string identifier, bool allowLocked = false)
    {
        bool selected = false;
        int index = GetIndexByIdentifier(identifier);

        do
        {
            // Aumentar y comprobar el indice
            index++;
            if (index >= characters.Count) index = 0;

            GameObject character = Get(index);
            CharacterConfiguration characterInfo = character.GetComponent<CharacterConfiguration>();
            if (allowLocked || UnlockedCharacterController.IsUnlocked(characterInfo.GetIdentifier())){
                selected = true;
                return characterInfo;
            }

        } while (!selected);

        return null;
    }

    /// <summary>
    /// Obtencion de personaje en base al indice
    /// </summary>
    /// <param name="index"></param>
    /// <param name="allowLocked"></param>
    /// <returns></returns>
    public CharacterConfiguration GetCharacterInfoByIndex(int index = 0, bool allowLocked = false)
    {
        bool selected = false;
        do
        {
            // Aumentar y comprobar el indice
            if (index < 0) index = characters.Count - 1;
            if (index >= characters.Count) index = 0;

            GameObject character = Get(index);
            CharacterConfiguration characterInfo = character.GetComponent<CharacterConfiguration>();
            if (allowLocked || UnlockedCharacterController.IsUnlocked(characterInfo.GetIdentifier()))
            {
                selected = true;
                return characterInfo;
            }

        } while (!selected);

        return null;
    }

    /// <summary>
    /// Obtener el PJ anterior disponible dentro del pool
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="allowLocked"></param>
    /// <returns></returns>
    public CharacterConfiguration GetPreviousCharacterInfoByIdentifier(string identifier, bool allowLocked = false)
    {
        bool selected = false;
        int index = GetIndexByIdentifier(identifier);

        do
        {
            // Aumentar y comprobar el indice
            index--;
            if (index < 0) index = characters.Count - 1;

            GameObject character = Get(index);
            CharacterConfiguration characterInfo = character.GetComponent<CharacterConfiguration>();
            if (allowLocked || UnlockedCharacterController.IsUnlocked(characterInfo.GetIdentifier()))
            {
                selected = true;
                return characterInfo;
            }

        } while (!selected);

        return null;
    }

    /// <summary>
    /// Personajes secretos
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public bool SecretCharactersContains(string identifier)
    {
        bool contains = false;

        foreach(CharacterConfiguration config in secretsCharacters)
        {
            if (config.GetIdentifier() == identifier)
            {
                contains = true;
                break;
            }
        }

        return contains;
    }

    /// <summary>
    /// Obtencion del GameObject ligado al personaje secreto
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public GameObject SecretCharacterGameObject(string identifier)
    {
        GameObject gameObject = null;

        foreach (CharacterConfiguration config in secretsCharacters)
        {
            if (config.GetIdentifier() == identifier)
            {
                gameObject = config.gameObject;
                break;
            }
        }

        return gameObject;
    }

    /// <summary>
    /// Obtencion del avatar ligado al persobaje secreto
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public Sprite SecretCharacterAvatar(string identifier)
    {
        Sprite sprite = null;

        foreach (CharacterConfiguration config in secretsCharacters)
        {
            if (config.GetIdentifier() == identifier)
            {
                sprite = config.GetAvatar();
                break;
            }
        }

        return sprite;
    }

    /// <summary>
    /// Comprobacion de que personaje este desbloqueado
    /// </summary>
    /// <param name="itemSource"></param>
    /// <returns></returns>
    public bool CheckItem(GameObject itemSource)
    {
        CharacterConfiguration characterConfiguration = itemSource.GetComponent<CharacterConfiguration>();
        return characterConfiguration ? characterConfiguration.IsUnlocked() : false;
    }

    /// <summary>
    /// Cambia el personaje seleccionado por el jugador
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="playerIndex"></param>
    public void SetCurrentCharacterIdentifier(string identifier, int playerIndex = 0)
    {
        if (currentCharacterIdentifer.Count - 1 < playerIndex)
        {
            int initAt = currentCharacterIdentifer.Count > 0 ? currentCharacterIdentifer.Count : 0;
            for (int i = initAt; i < playerIndex; i++)
            {
                currentCharacterIdentifer.Add(null);
            }
            currentCharacterIdentifer.Add(identifier);
        }
        else currentCharacterIdentifer[playerIndex] = identifier;
    }

    /// <summary>
    /// Obtencion del identificador del personaje seleccionado por el jugador
    /// </summary>
    /// <param name="playerIndex"></param>
    public string GetCurrentCharacterIdentifier(int playerIndex = 0)
    {
        if (currentCharacterIdentifer.Count - 1 < playerIndex) return null;
        return currentCharacterIdentifer[playerIndex];
    }

    /// <summary>
    /// Obtencion del objeto asociado al personaje seleccionado por el jugador
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public GameObject GetCurrentCharacter(int playerIndex = 0)
    {
        if (currentCharacterIdentifer.Count - 1 < playerIndex) return Surprise();
        return Get(currentCharacterIdentifer[playerIndex]);
    }



    /// <summary>
    /// Reseteo de la lista de jugadores
    /// </summary>
    public void ResetPlayerList(List<int> maintainThisIndexes = null)
    {
        int index = 0;
        if (maintainThisIndexes == null || maintainThisIndexes.Count == 0) currentCharacterIdentifer.Clear();
        else
        {
            List<string> temp = new List<string>(currentCharacterIdentifer);
            int max = maintainThisIndexes.Max();
            currentCharacterIdentifer.Clear();
            for (int i = 0; i <= max; i++)
            {
                currentCharacterIdentifer.Add(maintainThisIndexes.Contains(i) ? temp[i] : null);
            }
            if(currentCharacterIdentifer.FindAll(cc => cc != null).Count == 0) currentCharacterIdentifer.Clear();
        }

        if (currentCharacterIdentifer.Count == 0) currentCharacterIdentifer.Add(JankenUp.Characters.BOYLEE);

        index = 0;
    }

    /// <summary>
    /// Seteo de indices de jugadores habilitados para ser seleccionados como jugadores de duelos
    /// </summary>
    /// <param name="playersIndexAllowedToDuel"></param>
    public void SetPlayersAllowedToDuel(List<int> playersIndexAllowedToDuel) {
        this.playersIndexAllowedToDuel = playersIndexAllowedToDuel;
    }

    /// <summary>
    /// Obtencion del listado de indices de jugadores habilitados para duelos
    /// </summary>
    /// <returns></returns>
    public List<int> GetPlayersAllowedToDuel()
    {
        return playersIndexAllowedToDuel;
    }
}
