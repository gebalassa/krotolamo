using System;
using System.Collections.Generic;
using UnityEngine;

public class JankenSession : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] GameObject character;

    [Header("Stats")]
    [SerializeField] protected int coins = 0;
    [SerializeField] protected int initialCoins = 0;
    [SerializeField] protected int victories = 0;
    [SerializeField] protected int defeats = 0;
    [SerializeField] protected int ties = 0;

    [Header("SuperPowers")]
    [SerializeField] protected int timeMaster = 0;
    [SerializeField] protected int magicWand = 0;
    [SerializeField] protected int janKenUp = 0;

    // Guardado de las jugadas y los resultados
    protected List<Attacks> attackSequence = new List<Attacks>();
    protected List<int> resultsSequence = new List<int>();
    protected List<int> parcialResultsSequence = new List<int>();
    protected bool perfectScore = true;

    // Singleton
    protected void Awake()
    {
        int length = FindObjectsOfType<JankenSession>().Length;
        if (length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Dejar los valores por defecto
        Reset();
    }

    // Cambio y obtención del personaje del jugador
    public void SetPlayer(GameObject character)
    {
        this.character = character;
        CharacterConfiguration characterConfiguration = character.GetComponent<CharacterConfiguration>();
        // Precargar la musica del personaje
        if(characterConfiguration) MasterAudioPlayer._player.PlayOrLoadThis(characterConfiguration.GetCharacterMusic(), false);
    }

    /// <summary>
    /// Obtencion del personaje del jugador X
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public GameObject GetPlayer(int playerIndex = 0)
    {
        return CharacterPool.Instance.GetCurrentCharacter(playerIndex);
    }

    // Obtención de las monedas
    public int GetCoins() { return coins; }

    // Gastar X cantidad de monedas si es que hay disponibles
    public bool SpendCoins(int price)
    {
        if (coins < price) return false;

        coins -= price;
        return true;
    }

    // Obtención de las monedas ganadas en la sesión
    public int GetSessionCoins()
    {
        return coins - initialCoins;
    }

    // Incremento de conteo de jugadas ganadas
    public void Win()
    {
        victories++;
        AddCoins();
    }

    // Incremento de conteo de jugadas perdidas
    public void Lose()
    {
        defeats++;
        SubtractCoins();
    }

    // Incremento de conteo de jugadas empatadas
    public void Draw() { ties++; }

    // Obtención de las victorias
    public int GetVictories() { return victories; }

    // Obtención de las derrotas
    public int GetDefeats() { return defeats; }

    // Obtención de los empates
    public int GetTies() { return ties; }

    // Resetear los valores de la sesión
    public void Reset()
    {
        victories = 0;
        defeats = 0;
        ties = 0;
        coins = GameController.Load().coins;
        initialCoins = GameController.Load().coins;
        attackSequence.Clear();
        resultsSequence.Clear();
        parcialResultsSequence.Clear();
    }

    // Agregar monedas
    public void AddCoins()
    {
        // TODO: Implementar
    }

    // Sustraer monedas
    public void SubtractCoins()
    {
        // TODO: Implementar
    }

    // Guardado de la ultima jugada
    public void AddAttackSequence(Attacks attack) {
        attackSequence.Add(attack);
    }

    /// <summary>
    /// Obtencion de la lista de ataques jugados durante la partida
    /// </summary>
    public List<Attacks> GetAttackSequence()
    {
        return attackSequence;
    }

    // Guardado del resultado global de una jugada
    public void AddResultsSequence(int result)
    {
        resultsSequence.Add(result);
    }

    // Obencion del resultado global de una jugada
    public List<int> GetResultsSequence()
    {
        return resultsSequence;
    }

    // Guardado del resultado parcial de una jugada
    public void AddParcialResultsSequence(int result) {
        parcialResultsSequence.Add(result);

        // Si el resultado es distinto de 1, quitar el perfect score
        if (result != 1) perfectScore = false;
    }

    // Obencion del resultado parcial de la partida
    public List<int> GetPartialResultsSequence()
    {
        return parcialResultsSequence;
    }

    // Obtener la cantidad de poderes TimeMaster disponible
    public int GetTimeMaster()
    {
        return timeMaster;
    }

    // Revisar la posibilidad de gastar un super poder TimeMaster
    public bool SpendTimeMaster()
    {
        if( timeMaster > 0)
        {
            timeMaster--;
            return true;
        }
        
        return false;
    }

    // Obtener la cantidad de poderes MagicWand disponible
    public int GetMagicWand()
    {
        return magicWand;
    }

    // Revisar la posibilidad de gastar un super poder MagicWand
    public bool SpendMagicWand()
    {
        if (magicWand > 0)
        {
            magicWand--;
            return true;
        }

        return false;
    }

    // Obtener la cantidad de poderes MegaPunch disponible
    public int GetJanKenUp()
    {
        return janKenUp;
    }

    // Revisar la posibilidad de gastar un super poder MegaPunch (Cambiar a algo mas bacan, en chileno, Charchazo)
    public bool SpendJanKenUp()
    {
        if (janKenUp > 0)
        {
            janKenUp--;
            return true;
        }

        return false;
    }

    // Sincronizar la cantidad de monedas inicial con la cantidad de monedas actuales
    public void SyncInitialCoins()
    {
        initialCoins = coins;
    }

}