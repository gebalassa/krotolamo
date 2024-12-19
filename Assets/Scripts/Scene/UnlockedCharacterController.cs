using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JankenUp;
using UnityEngine.SocialPlatforms;
using TMPro;

public class UnlockedCharacterController : SceneController
{
    [Header("Setup")]
    [SerializeField] [Range(1f, 5f)] float showTime = 1f;
    [SerializeField] float winTime = 1f;
    [SerializeField] GameObject characterDisplayer;
    [SerializeField] TextMeshProUGUI characterName;
    [SerializeField] TextMeshProUGUI unlockedCharacterLabel;

    // Listado de nuevo personajes desbloqueados durante la sesion y los que ya estan desbloqueados
    static List<string> newUnlockedCharacters = new List<string>();
    static List<string> unlockedCharacters;

    // Uso frecuente
    CharacterInGameController characterInGameController;

    // Lsa puertas no se abriran automaticamente
    new void Start()
    {
        StartCoroutine(UnlockCharactersCoroutine());

        // Cambiar el titulo
        LocalizationHelper.Translate(unlockedCharacterLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.newChallenger);

        // Actualizar la font usada para el Label
        UpdateCurrentFont();
    }

    // Rutina encargada de mostrar todos los personajes desbloqueados
    IEnumerator UnlockCharactersCoroutine() {

        TransitionDoors td = TransitionDoors._this;

        // Por cada personaje desbloqueado, realizar la secuencia de muestra
        foreach (string identifier in newUnlockedCharacters) {

            // Configurar personaje
            GameObject character = CharacterPool.Instance.Get(identifier);
            
            GameObject newCharacter = Instantiate(
                character,
                characterDisplayer.transform.position,
                Quaternion.identity
            );

            // Destruir el jugador actual y asignar nueva referencia
            newCharacter.transform.parent = characterDisplayer.transform.parent;
            Destroy(characterDisplayer);
            characterDisplayer = newCharacter;

            CharacterConfiguration config = character.GetComponent<CharacterConfiguration>();
            characterName.text = config.GetName();
            characterInGameController = characterDisplayer.GetComponent<CharacterInGameController>();
            characterInGameController.ChangeState(InGameStates.Stand);
            characterInGameController.ChangeSpritesLayerTo(JankenUp.SpritesSortingLayers.Foreground);

            // Abrir puertas
            yield return StartCoroutine(td.Open());

            // Mantener a personaje en pantalla
            yield return new WaitForSeconds(showTime);

            // Cambiar al estado de win y reproducir sonido
            characterInGameController.ChangeState(InGameStates.Win);

            yield return new WaitForSeconds(winTime);

            // Cerrar puertas
            yield return StartCoroutine(td.Close());
        }

        // Vaciar listado de nuevos personajes
        newUnlockedCharacters.Clear();

        // Continuar a la scene que se requiera
        SceneLoaderManager slm = FindObjectOfType<SceneLoaderManager>();
        if (slm) slm.Back();

    }

    // Agregar un nuevo personaje desbloqueado al listado
    public static void Unlock(string identifier) {

        if(!unlockedCharacters.Contains(identifier)) unlockedCharacters.Add(identifier);

    }

    // Agrega un nuevo personaje al listado de nuevo desbloqueados
    public static void NewUnlock(string identifier)
    {

        if (!IsUnlocked(identifier))
        {
            newUnlockedCharacters.Add(identifier);
            Unlock(identifier);
            GameController.SaveUnlockedCharacter(unlockedCharacters);
        }

    }

    // Agrega un nuevo personaje al listado de nuevo desbloqueados
    public static void NewUnlockNotShow(string identifier)
    {

        if (!IsUnlocked(identifier))
        {
            Unlock(identifier);
            GameController.SaveUnlockedCharacter(unlockedCharacters);
        }

    }

    // Consulta por si un personaje ya fue desbloqueado
    public static bool IsUnlocked(string identifier) {
        
        if(unlockedCharacters == null) unlockedCharacters = GameController.Load().unlockedCharacters;

        return unlockedCharacters.Contains(identifier) || newUnlockedCharacters.Contains(identifier);

    }

    // Consultar por el listado actual de personajes desbloqueados
    public static void SaveUnlockedCharacter()
    {
        if (unlockedCharacters == null) unlockedCharacters = GameController.Load().unlockedCharacters;
        GameController.SaveUnlockedCharacter(unlockedCharacters);
    }

    // Consulta de si existen nuevos personajes desbloqueados
    public static bool AreThereNewCharacters() {
        return newUnlockedCharacters.Count > 0;
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        TMP_FontAsset currentCPFont = FontManager._mainManager.GetMainFont();
        unlockedCharacterLabel.font = currentCPFont;
        unlockedCharacterLabel.fontStyle = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
    }

    // Agregar un nuevo personaje desbloqueado al listado
    public static void UnlockDeluxe()
    {
        // Desbloquear a los personajes originales
        List<string> characters = new List<string>() {};
        characters.AddRange(Characters.freeCharacters);

        // Agregar los personajes de los packs que tenga el jugador
        List<JankenUp.Deluxe.Pack> packList = GameController.GetPlayerPacks();

        foreach(JankenUp.Deluxe.Pack pack in packList)
        {
            characters.AddRange(pack.characters);
        }

        foreach(string identifier in characters)
        {
            if (!IsUnlocked(identifier))
            {
                unlockedCharacters.Add(identifier);
            }
        }
        GameController.SaveUnlockedCharacter(unlockedCharacters);
    }

    // Eliminacion de personajes que antes estaban desbloqueados
    public static void RemoveCharacter(string identifier)
    {
        if (unlockedCharacters == null) unlockedCharacters = GameController.Load().unlockedCharacters;
        unlockedCharacters.Remove(identifier);
    }

}