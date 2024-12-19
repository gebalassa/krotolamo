using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* La dificultad esta dada por la cantidad de tiempo que aguanta un usuario*/
public class SurvivalModeDifficulty : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Niveles múltiples serán de jugada especial")]
    [SerializeField] int specialLevel = 5;
    [Tooltip("Tiempo inicial de cambio a partir de nivel base")]
    [SerializeField] float baseTime = 2f;
    [Tooltip("Aumento de velocidad")]
    [SerializeField] float deltaTime = 0.18f;
    [Tooltip("Velocidad mínima")]
    [SerializeField] float minSpeed = 0.4f;
    [Tooltip("Nivel desde el cual puede o no estar oculta la opción del rival")]
    [SerializeField] int illuminatiTutorial = 9;
    [SerializeField] int illuminatiFrom = 4;

    [Header("Shuffle Attacks")]
    [SerializeField] int shuffleAttacksFrom = 4;

    [Header("Coins")]
    [SerializeField] int coinsNormalLevel = 3;
    [SerializeField] int coinsSpecialLevel = 3;
    [SerializeField] int coinsPerDraw = 6;

    [Header("Flow")]
    [SerializeField] int recalculateEach = 5;

    [Header("Countdown")]
    [SerializeField] int countDownSeconds = 3;

    // Nivel de dificultad
    int level = -1;
    int minLevel = 1;
    int maxLevel = 9;

    // Tutoriales de jugadas especiales
    const int tutorialTripleTurn = 5;
    const int tutorialDestinyTurn = 10;

    // Obtener el ultimo valor de dificultad desde la data del usuario
    private void Start()
    {
        level = GameController.Load().survivalLevel;
    }

    // Obtencion del nivel de dificultad actual
    public int GetDifficultyLevel()
    {
        if (level == -1) CalcDifficulty();
        return level;
    }

    // Aumento de la dificultad
    public void CalcDifficulty()
    {
        if (level == -1) level = GameController.Load().survivalLevel;

        // Obtener los resultados desde la sesion
        SingleModeSession sms = FindObjectOfType<SingleModeSession>();
        List<int> results = sms.GetResultsSequence();

        if (results.Count == 0 || results.Count % recalculateEach != 0) return;

        level = Mathf.Clamp(level + 1, minLevel, maxLevel);
        int levelToSave = Mathf.Clamp(level-1, minLevel, maxLevel);

        // Guardar el nivel para la proxima sesion
        GameController.SaveDifficultySurvival(levelToSave);
    }

    /* Guardar la velocidad actual de juego menos uno */
    public void SaveFinalDifficulty() {
        int levelToSave = Mathf.Clamp(level - 1, minLevel, maxLevel);
        GameController.SaveDifficultySurvival(levelToSave);
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
        return countDownSeconds;
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
                    if (!TutorialCompleted(level)) return JankenUp.SinglePlayer.LevelType.Type.Triple;
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

    // Obtener la cantidad de monedas que son descontadas por empatar
    public int GetCoinsPerDraw()
    {
        return coinsPerDraw;
    }

    // Obtener si es necesario desordenar las opciones o no
    public bool ShuffleAttacks()
    {
        return level >= shuffleAttacksFrom;
    }

    // Obtener si un nivel tutorial ha sido completado
    private bool TutorialCompleted(int level)
    {
        return GameController.LoadTutorials().tutorials.Contains(level);
    }
}
