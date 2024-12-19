using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* La dificultad estara dada por la habilidad del usuario, asi que no sera fija por niveles.
 Cada 5 niveles se revisara las victorias del usuario. En caso de que sean al menos 3, esta aumentara. De lo contrario, el nivel disminuira.
La regla corre al inverso si el jugador se encuentra en niveles negativos
 */
public class SingleModeDifficulty : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Niveles múltiples serán de jugada especial")]
    [SerializeField] int specialLevel = 5;
    [Tooltip("Tiempo inicial de cambio a partir de nivel base")]
    [SerializeField] float baseTime = 3f;
    [Tooltip("Aumento de velocidad")]
    [SerializeField] float deltaTime = 0.14f;
    [Tooltip("Velocidad mínima")]
    [SerializeField] float minSpeed = 0.4f;
    [Tooltip("Nivel desde el cual puede o no estar oculta la opción del rival")]
    [SerializeField] int illuminatiTutorial = 9;
    [SerializeField] int illuminatiFrom = 4;

    [Header("Shuffle Attacks")]
    [SerializeField] int shuffleAttacksFrom = 4;
    [SerializeField] int disableAttacksFrom = 16;

    [Header("Coins")]
    [SerializeField] int coinsNormalLevel = 1;
    [SerializeField] int coinsSpecialLevel = 3;

    [Header("Flow")]
    [SerializeField] int recalculateEach = 5;

    [Header("Countdown")]
    [SerializeField] int countDownSecondsMax = 9;
    [SerializeField] int countDownSecondsMin = 3;

    [Header("Rounds")]
    [SerializeField] int roundMin = 10;
    [SerializeField] int roundDelta = 5;
    [SerializeField] int roundMax = 20;
    [SerializeField] int firstTutorialRound = 5;
    [SerializeField] int secondTutorialRound = 5;

    // Nivel de dificultad
    int level = -1;
    int minLevel = 0;
    int maxLevel = 20;
    int lastChangeDificulty = 0;

    // Tutoriales de jugadas especiales
    const int tutorialTripleTurn = 5;
    const int tutorialDestinyTurn = 10;

    // Obtener el ultimo valor de dificultad desde la data del usuario
    private void Start()
    {
        
        LoadCurrentDifficultyLevel();
    }

    // Obtener la dificultad actual
    private void LoadCurrentDifficultyLevel()
    {
        SingleModeSession sms = FindObjectOfType<SingleModeSession>();
        level = sms && sms.GetDifficultyLevel() != -1 && sms.GetCurrentRound() > 1 ? sms.GetDifficultyLevel() : GameController.Load().difficultyLevel;
    }

    // Obtencion del nivel de dificultad actual
    public int GetDifficultyLevel()
    {
        if (level == -1) CalcDifficulty();
        return level;
    }

    // Calculo del nivel actual de dificultad en base a los ultimos resultados
    public void CalcDifficulty()
    {
        if (level == -1) LoadCurrentDifficultyLevel();

        // Obtener los resultados desde la sesion
        SingleModeSession sms = FindObjectOfType<SingleModeSession>();
        List<int> results = sms.GetResultsSequence();
        int currentFloor = sms.GetLevel();

        if (results.Count == 0 || results.Count % recalculateEach != 0) return;

        // Revisar si se debe aumentar o no. Tener en cuenta el piso en que esta para saber el factor determinante (Victorias o derrotas)
        int victories = 0;
        int defeats = 0;

        // Navegar solo los ultimos resultados
        int[] lastResults = new int[5];
        results.CopyTo(results.Count - 5, lastResults, 0, 5);
        foreach (int r in lastResults)
        {
            if (r == 1) victories++;
            else defeats++;
        }

        // Determinar si la dificultad debe incrementarse, mantenerse o disminuir
        int harder = 0;
        if (victories == lastResults.Length || victories + 1 == lastResults.Length)
        {
            harder = currentFloor > 0 ? 1 : -1;
        }
        else if (defeats + 1 == lastResults.Length || defeats == lastResults.Length
            || lastChangeDificulty == 0)
        {
            harder = currentFloor > 0 ? -1 : 1;
        }

        // Guardar el ultimo cambio de dificultad
        lastChangeDificulty = harder;

        if (harder == 0) return;

        level = Mathf.Clamp(level + harder, minLevel, maxLevel);
        int levelToSave = Mathf.Clamp(level-2, minLevel, maxLevel);

        // Guardar el nivel para la proxima sesion
        GameController.SaveDifficultyLevel(levelToSave);
    }

    /*
     * Obtener la velocidad de cambio de la jugada de la CPU
     * Si el valor devuelto es -1, significa que no hay velocidad de cambio
     */
    public float GetSpeed()
    {
        // Nivel 0 no presenta dificultades
        if (0 == level) return -1;

        // Los siguientes niveles estarán determinados por la dificultad calculada
        float speed = Mathf.Clamp(baseTime - deltaTime * level, minSpeed, baseTime - deltaTime * level) ;

        return speed;

    }

    /*
     * Obtener la cantidad de segundos que tendra el jugador para elegir una jugada
     */
    public int GetCountdown()
    {
        int seconds = (int) Mathf.Clamp(countDownSecondsMax - 3 * level, countDownSecondsMin, countDownSecondsMax);
        return seconds;
    }

    // Obtener el tipo de nivel que se jugara, si es normal o especial
    public JankenUp.SinglePlayer.LevelType.Type GetType(int level)
    {
        // Si es un nivel especial o uno por turnos
        bool special = level % specialLevel == 0 && level != 0;
        if (special)
        {
            // Determinar si es un tutorial
            switch (level)
            {
                case tutorialTripleTurn:
                    if(!TutorialCompleted(level)) return JankenUp.SinglePlayer.LevelType.Type.Triple;
                    break;
                case tutorialDestinyTurn:
                    if (!TutorialCompleted(level)) return JankenUp.SinglePlayer.LevelType.Type.Destiny;
                    break;
            }

            List<JankenUp.SinglePlayer.LevelType.Type> specialLevelsList = new List<JankenUp.SinglePlayer.LevelType.Type>() {
                JankenUp.SinglePlayer.LevelType.Type.Triple,
                JankenUp.SinglePlayer.LevelType.Type.Destiny
            };

            return specialLevelsList[Random.Range(0, specialLevelsList.Count)];

        }

        return JankenUp.SinglePlayer.LevelType.Type.Normal;
    }

    // Obtención si nivel es tipo illuminati o no
    public bool Illuminati(int level)
    {
        // Debe funcionar tanto para pisos positivos como pisos negativos
        int rawLevel = Mathf.Abs(level);

        // Si es el nivel tutorial, devolver verdadero
        if (rawLevel == illuminatiTutorial) return true;

        // Puede o no ser illuminati
        return illuminatiFrom <= this.level && rawLevel > illuminatiTutorial? (Random.Range(0,2) == 1) : false;
    }

    // Obtener el nivel que es de tipo illuminati
    public bool IlluminatiTutorialLevel(int level)
    {
        return illuminatiTutorial == level;
    }

    // Obtener la cantidad de monedas que entrega un nivel
    public int GetCoinsPerLevel(int level)
    {
        return level % specialLevel == 0 && level != 0 ? coinsSpecialLevel : coinsNormalLevel;
    }

    // Obtener si es necesario desordenar las opciones o no
    public bool ShuffleAttacks()
    {
        return level >= shuffleAttacksFrom;
    }

    // Obtener si es necesario desactivar una de las opciones
    public bool DisableAttacks()
    {
        return level >= disableAttacksFrom;
    }

    // Calculo en base a la dificultad de cuanto deberia durar una ronda de juego
    public int GetRoundLevelCountBasedOnCurrentDifficulty()
    {
        // Si es la primera vez que juega el usuario (No haber pasado tutorial), devolver la cantidad especial
        if (!TutorialCompleted(5)) return firstTutorialRound;
        if (!TutorialCompleted(10)) return secondTutorialRound;

        // Calcular dificultad actual
        if (level == -1) CalcDifficulty();

        int howMuchLevel = roundMin + (level / roundDelta * roundDelta);
        return Mathf.Clamp(howMuchLevel, roundMin, roundMax);    
    }

    // Obtener si un nivel tutorial ha sido completado
    private bool TutorialCompleted(int level)
    {
        return GameController.LoadTutorials().tutorials.Contains(level);
    }

}
