using System.Collections.Generic;
using UnityEngine;

namespace JankenUp
{
    public class Localization
    {

        public class tables
        {
            public class Tutorial
            {

                public static string tableName = "Tutorial";

            }
            public class InGame
            {

                public static string tableName = "InGame";
                public class Keys
                {
                    public static string harder = "Harder";
                    public static string easier = "Easier";
                    public static string resume = "Resume";
                    public static string back = "Back";
                    public static string floor = "Floor";
                    public static string victories = "Victories";
                    public static string defeats = "Defeats";
                    public static string ties = "Ties";
                    public static string coins = "Coins";
                    public static string score = "Score";
                    public static string maxCombo = "Max Combo";
                    public static string newChallenger = "A new challenger";
                    public static string classicMode = "Classic";
                    public static string survivalMode = "Frenzy";
                    public static string time = "Time";
                    public static string bravo = "Bravo";
                    public static string round = "Round";
                    public static string freeDeluxeUntil = "FreeDeluxeUntil";
                    public static string selectGameMode = "select_game_mode";
                    public static string player_x = "player_x";
                    public static string rank_x = "rank_x";
                    public static string press_to_join = "press_to_join";
                    public static string ready = "ready";
                }

            }

            public class Online
            {

                public static string tableName = "Online";
                public class Keys
                {
                    public static string yourName = "Your name";
                    public static string publicSession = "Public";
                    public static string privateSession = "Private";
                    public static string hostGame = "Host Game";
                    public static string code = "Code";
                    public static string codeLabel = "CodeLabel";
                    public static string joinGame = "Join Game";
                    public static string kos = "Knockouts";
                    public static string players = "Players";
                    public static string playersOnline = "Players online";
                    public static string champion = "The champion is";
                    public static string ranking = "Ranking";
                    public static string exit = "Exit";
                    public static string yes = "Yes";
                    public static string no = "No";
                    public static string turns = "Turns";
                    public static string type = "Type";
                    public static string lives = "Lives";
                    public static string tournament = "Tournament";
                    public static string deathmatch = "Deathmatch";
                    public static string publicModeRestructured = "public_mode_restructured";
                    public static string visibility = "visibility";
                    public static string timePerDuel = "TimeDuel"; 
                    public static string serverRestart = "serverRestart";
                    public static string feint = "feint";
                    public static string timePerRound = "TimeRound";
                    public static string points = "Points";
                    public static string exitGame = "ExitGame";
                }

            }

            public class Achievements
            {

                public static string tableName = "Achievements";
                public class Keys
                {
                    public static string doubleEnter = "double_enter";
                    public static string unlockMethod = "unlock_method";
                }

            }

            public class Deluxe
            {
                public static string tableName = "Deluxe";
                public class Keys
                {
                    public static string unlockWithDeluxe = "unlock_with_deluxe";
                    public static string deluxeTitle = "deluxe_title";
                    public static string deluxeBenfitPrefix = "deluxe_benefit";
                    public static string deluxeBuy = "buy_deluxe";
                    public static string deluxeCancel = "buy_cancel";
                }

            }

            public class Credits
            {
                public static string tableName = "Credits";
                public class Keys
                {
                    public static string gameDesign = "Game Design";
                    public static string artDesign = "Art and Design";
                    public static string programmer = "Programmer";
                    public static string music = "Music";
                    public static string sounds = "Sounds";
                    public static string voices = "Voices";
                    public static string specialThanks = "Special thanks";
                }

            }

            public class Characters
            {

                public static string tableName = "Characters";

                public class Keys
                {
                    public static string moreInfo = "more_info";
                }
            }

            public class Country
            {

                public static string tableName = "Country";

                public class Keys
                {
                    public static string chile = "chile";
                    public static string china = "china";
                    public static string usa = "usa";
                    public static string japan = "japan";
                    public static string brazil = "brazil";
                    public static string southkorea = "southkorea";
                    public static string czechrepublic = "czechrepublic";
                    public static string spain = "spain";
                    public static string indonesia = "indonesia";
                    public static string elyr = "elyr";
                    public static string russia = "russia";
                    public static string unknown = "unknown";
                }

            }

            public class JanKenShop
            {

                public static string tableName = "JanKenShop";

                public class Keys
                {
                    public static string already_bought = "already_bought";
                    public static string take_my_money = "take_my_money";
                    public static string details = "details";
                }

            }

            public class Options
            {

                public static string tableName = "Options";

                public class Keys
                {
                    public static string selectYourCharacter = "select_your_character";
                    public static string character = "character";
                    public static string tapToPlay = "tap_to_play";
                    public static string tapToPlayPlease = "tap_to_play_please";
                    public static string startToPlay = "start_to_play";
                    public static string startToPlayPlease = "start_to_play_please";
                    public static string loadingResources = "loading_resources";
                    public static string settings = "settings";
                    public static string music = "Music";
                    public static string sounds = "Sounds";
                    public static string controls = "Controls";
                    public static string left = "Left";
                    public static string right = "Right";
                    public static string vibration = "Vibration";
                    public static string yes = "Yes";
                    public static string no = "No";
                    public static string language = "Language";
                    public static string next = "next";
                    public static string play = "play";
                    public static string difficulty = "difficulty";
                    public static string activateTutorial = "activateTutorial";
                    public static string disableTutorial = "disableTutorial";
                    public static string characterDetails = "characterDetails";
                    public static string select = "select";
                    public static string referee = "referee";
                }

            }

            public class Joystick
            {

                public static string tableName = "Joystick";

                public class Keys
                {
                    public static string illuminati = "illuminati";
                    public static string time_master = "time_master";
                    public static string magic_wand = "magic_wand";
                    public static string super_attack = "super_attack";
                    public static string guu = "guu";
                    public static string paa = "paa";
                    public static string choki = "choki";
                    public static string button_layout = "button_layout";
                    public static string hold_to_feint = "hold_to_feint";
                    public static string button_layout_player = "button_layout_player";
                    public static string button_layout_spectator = "button_layout_spectator";
                    public static string support_player_left = "support_player_left";
                    public static string support_player_right = "support_player_right";
                    public static string throw_object_player_left = "throw_object_player_left";
                    public static string throw_object_player_right = "throw_object_player_right";
                }

            }

        }

    }

    public class Online
    {

        public class Events
        {

            public class General
            {
                public static string SETUP = "SETUP";
                public static string SETUPCHARACTER = "SETUPCHARACTER";
                public static string JKERROR = "JKERROR";
            }

            public class Public
            {
                public static string SETUP = "SETUP";
                public static string MATCHING = "MATCHING";
                public static string OPEN = "OPEN";
                public static string READY = "READY";
                public static string WIN = "WIN";
                public static string LOSE = "LOSE";
                public static string DRAW = "DRAW";
                public static string RESULTS = "RESULTS";
                public static string ENDSESSION = "ENDSESSION";
                public static string UPDATEPLAYERS = "UPDATEPLAYERS";
                public static string UPDATERANKING = "UPDATERANKING";
            }

            public class Private
            {
                public static string JOINED = "JOINED";
                public static string START = "START";
                public static string SETUP = "SETUP";
                public static string UPDATE = "UPDATE";
                public static string MATCH = "MATCH";
                public static string ATTACK = "ATTACK";
                public static string RESULTS = "RESULTS";
                public static string FINISH = "FINISH";
                public static string UPDATEDEATHMATCH = "UPDATEDEATHMATCH";
                public static string HORN = "HORN";
                public static string READYTOMATCH = "READYTOMATCH";
                public static string MESSAGE = "MESSAGE";
                public static string MATCHREADYTOSTART = "MATCHREADYTOSTART";
                public static string FEINT = "FEINT";
                public static string NEXTROUND = "NEXTROUND";
            }

        }

        public class Messages
        {

            public class General
            {
                public static string CONNECTING = "Connecting";
                public static string SELECT_ATTACK = "Select your attack";
                public static string WAITING_MOVE = "Waiting opponent move";
                public static string WIN = "You Win!";
                public static string LOSE = "You Lose";
                public static string DRAW = "Tie. Repeat";
            }

            public class Public
            {
                public static string MATCHING = "Matching";
            }

            public class Private
            {

                public static string WAITING_FOR_PLAYERS = "Wating for players";
                public static string TOURNAMENT_READY = "The tournament will start soon";

                public static string[] WAITING = new string[] {
                    "¿Quién ganará?",
                    "¡Es hora del DU-DU-DUELO!",
                    "¡Vamos! ¡Peleen!"
                };

                public static string[] RESULT = new string[]
                {
                    "¡Bueena!",
                    "¡Critical HIT!",
                    "Niceeee"
                };

                public static class TOURNAMENT_PHASE
                {
                    public static class QUARTERFINALS
                    {
                        public const string identifier = "QUARTERFINALS";
                        public static string text = "Quarterfinals";
                    }
                    public static class SEMIFINALS
                    {
                        public const string identifier = "SEMIFINALS";
                        public static string text = "Semifinals";
                    }
                    public static class FINAL
                    {
                        public const string identifier = "FINAL";
                        public static string text = "Final";
                    }
                }

            }

        }

        public class ConnectionType
        {

            public const string PUBLIC = "PUBLIC";
            public const string PRIVATE = "PRIVATE";

        }

        public class GameType
        {

            public const string CASUAL = "CASUAL";
            public const string COMPETITIVE = "COMPETITIVE";

        }

        public class Error
        {
            public class General
            {
                public class Unknow
                {
                    public const string code = "UNKNOW";
                    public const string message = "Connection could not be established";
                }

                public class ServerRestart
                {
                    public const string code = "SERVER_RESTART";
                    public const string message = "The server was rebooted";
                }
            }

            public class Public
            {

            }

            public class Private
            {
                public class RoomIsFull
                {
                    public const string code = "ROOM_FULL";
                    public const string message = "Room is full";
                }

                public class RoomNoExists
                {
                    public const string code = "ROOM_NO_EXISTS";
                    public const string message = "Invalid code";
                }
            }

            // Obtencion del mensaje segun un codigo de error
            public static string GetErrorMessage(string code)
            {
                switch (code)
                {

                    case Private.RoomIsFull.code:
                        return Private.RoomIsFull.message;

                    case Private.RoomNoExists.code:
                        return Private.RoomNoExists.message;

                    case General.Unknow.code:
                    default:
                        return General.Unknow.message;

                }
            }

        }

        // Configuraciones utiles para los distintos modos
        public class Other
        {
            public class Public
            {
                public static int streakGoal = 3;
                public static int maxLives = 3;
                public static int superJanKenUpCode = 4;

            }

            public class Private {

                public static int streakGoal = 3;
                public static int maxLives = 3;
                public static int superJanKenUpCode = 4;
                public static string shareURL = "https://jankenup.com/deep/room/";
                public static int[] superpowersServerCode = new int[2] { 3, 4 };
                public static int[] timePerDuelValids = new int[5] { 10, 15, 20, 25, 30 };
                public static int timePerDuelShowEvery = 5;
                public static int timePerDuelShowAlwaysLessThan = 6;
                public static int feintDefaultValue = -2;
                public static int[] timePerRoundValids = new int[6] { 30, 60, 90, 120, 150, 180 };
                public static int[] pointToEndRoundValids = new int[8] { 3, 4, 5, 6, 7, 8, 9, 10 };

                public enum Type
                {
                    TOURNAMENT,
                    ALL_VS_ALL,
                    FRENZY
                }

                public class Reward
                {
                    public const string LIVES = "lives";
                    public const string SUPERPOWER = "superpower";
                }

            }

        }

        // Mensajes de texto
        public class TextMessages
        {
            public static int maxLength = 128;
            public const string typePlain = "PLAIN";
            public const string typeEmote = "EMOTE";
        }

        // Emojis disponibles
        public class Emotes
        {
            public enum emotesID
            {
                RyuseiCaremalo,
                OriaxPacman,
                JadeCry,
                SakuraUwu,
                JoaquinAngry,
                AariniOk,
                BarryXD,
                MisaeYay,
                MatsuoWhat,
                JuanitaKiss,
                DuoCool,
                ArakataNerd
            }
        }

    }

    // Modo single player
    public class SinglePlayer
    {

        // Tipos de niveles disponibles para el single player
        public class LevelType
        {

            // Enumeracion de los tipos disponibles
            public enum Type
            {
                Normal,
                Triple,
                Destiny
            }

            // Definicion del tipo de nivel normal
            public class Normal
            {
                public const int turns = 1;
                public const bool showCPUAttack = true;
            }

            // Definicion del tipo de nivel triple
            public class Triple
            {
                public const int turns = 3;
                public const bool showCPUAttack = true;
                public const string announcement = "Triple";
            }

            // Definicion del tipo de nivel destino
            public class Destiny
            {
                public const int turns = 1;
                public const bool showCPUAttack = false;
                public const string announcement = "Trust";
                public const int basePercentaje = 95;
                public const int matchesToReview = 5;
                public const int time = 1;
            }

        }

    }

    // Personajes
    public class Characters
    {
        // Llave para personajes exclusivos de lujo
        public const string DELUXEEXCLUSIVE = "deluxe_exclusive";

        public const string BOYLEE = "boylee";
        public const string BOYMASK = "boymask";
        public const string GIRLSAN = "girlsan";
        public const string GIRLTWO = "girltwo";
        public const string YOUNGMAN = "youngman";
        public const string YOUNGWOMAN = "youngwoman";
        public const string ADULTMAN = "adultman";
        public const string ADULTWOMAN = "adultwoman";
        public const string OLDMAN = "oldman";
        public const string OLDWOMAN = "oldwoman";
        public const string REFEREE = "referee";
        public const string DUO = "duo";
        public const string CHORIZA = "choriza";

        // Legends Pack
        public const string GASTONMIAUFFIN = "gastonmiauffin";

        // Personajes gratis del juego
        public static List<string> freeCharacters = new List<string>() {
            BOYLEE, BOYMASK, GIRLSAN, GIRLTWO, YOUNGMAN, YOUNGWOMAN, ADULTMAN, ADULTWOMAN, OLDMAN, OLDWOMAN, DUO,
            GASTONMIAUFFIN
        };

        // Personajes exclusivos de la version Deluxe!
        public static List<string> DeluxeCharacters = new List<string>() {};

        // Personajes secretos del juego
        public static List<string> SecretCharacters = new List<string>() { REFEREE, CHORIZA };

        public class achievements
        {

            public static string BOYMASK = "CgkIzIuQypIeEAIQBQ";
            public static string GIRLTWO = "CgkIzIuQypIeEAIQBg";
            public static string YOUNGMAN = "CgkIzIuQypIeEAIQBw";
            public static string YOUNGWOMAN = "CgkIzIuQypIeEAIQCA";
            public static string ADULTMAN = "CgkIzIuQypIeEAIQCQ";
            public static string ADULTWOMAN = "CgkIzIuQypIeEAIQCg";
            public static string OLDMAN = "CgkIzIuQypIeEAIQCw";
            public static string OLDWOMAN = "CgkIzIuQypIeEAIQDA";
            public static string REFEREE = "CgkIzIuQypIeEAIQDQ";
            public static string DUO = "CgkIzIuQypIeEAIQKQ";
            public static string HARAMIYO = "CgkIzIuQypIeEAIQKg";
            public static string ELBRAYAN = "CgkIzIuQypIeEAIQKw";
            public static string SALFATE = "CgkIzIuQypIeEAIQLA";
            public static string GASTONMIAUFFIN = "x1";

            public static string GetAchievementId(string character)
            {

                switch (character)
                {
                    case Characters.BOYMASK:
                        return BOYMASK;
                    case Characters.GIRLTWO:
                        return GIRLTWO;
                    case Characters.YOUNGMAN:
                        return YOUNGMAN;
                    case Characters.YOUNGWOMAN:
                        return YOUNGWOMAN;
                    case Characters.ADULTMAN:
                        return ADULTMAN;
                    case Characters.ADULTWOMAN:
                        return ADULTWOMAN;
                    case Characters.OLDMAN:
                        return OLDMAN;
                    case Characters.OLDWOMAN:
                        return OLDWOMAN;
                    case Characters.REFEREE:
                        return REFEREE;
                    case Characters.GASTONMIAUFFIN:
                        return GASTONMIAUFFIN;
                }

                return "";
            }
        }

    }

    // Google Leaderboards
    public class Leaderboards
    {
        public static string FLOORMASTER = "Floor Master";//"CgkIzIuQypIeEAIQAQ";
        public static string INTRATERRESTIAL = "Intraterrestrial";//"CgkIzIuQypIeEAIQAw";
        public static string MAXSCORE = "Score Breaker";//"CgkIzIuQypIeEAIQAg";
        public static string MAXCOMBO = "Combo master";//"CgkIzIuQypIeEAIQBA";
        public static string WORLDCLASS = "CgkIzIuQypIeEAIQHA";
        public static string GRIMREAPER = "CgkIzIuQypIeEAIQHQ";
        public static string TIMEMASTER = "Time Master";//"CgkIzIuQypIeEAIQJQ";
        public static string KNOCKOUTSOFFLINE = "Knockouts";//"CgkIzIuQypIeEAIQJg";

    }

    // Google Achievements
    public class Achievements
    {

        // Logro por la compra de deluxe
        public static string deluxe = "CgkIzIuQypIeEAIQIw";

        // Obtener los logros segun un nivel dado
        public static string GetAchivementByLevel(int level)
        {

            switch (level)
            {
                case -33:
                    return "CgkIzIuQypIeEAIQGg";
                case 10:
                    return "CgkIzIuQypIeEAIQDg";
                case 20:
                    return "CgkIzIuQypIeEAIQDw";
                case 30:
                    return "CgkIzIuQypIeEAIQEA";
                case 33:
                    return "CgkIzIuQypIeEAIQEQ";
                case 40:
                    return "CgkIzIuQypIeEAIQEg";
                case 50:
                    return "CgkIzIuQypIeEAIQEw";
                case 75:
                    return "CgkIzIuQypIeEAIQFA";
                case 100:
                    return "CgkIzIuQypIeEAIQFQ";
            }

            return null;

        }

        // Obtener los logros segun cantidad de combos
        public static string GetAchivementByCombos(int level)
        {

            switch (level)
            {
                case 10:
                    return "CgkIzIuQypIeEAIQFg";
                case 20:
                    return "CgkIzIuQypIeEAIQFw";
                case 30:
                    return "CgkIzIuQypIeEAIQGA";
                case 40:
                    return "CgkIzIuQypIeEAIQGQ";
                case 50:
                    return "CgkIzIuQypIeEAIQGw";
            }

            return null;

        }

        // Obtener los logros segun cantidad de kos
        public static string GetAchivementByKOs(int kos)
        {

            switch (kos)
            {
                case 1:
                    return "CgkIzIuQypIeEAIQHg";
                case 2:
                    return "CgkIzIuQypIeEAIQHw";
                case 3:
                    return "CgkIzIuQypIeEAIQIA";
                case 4:
                    return "CgkIzIuQypIeEAIQIQ";
                case 5:
                    return "CgkIzIuQypIeEAIQIg";
            }

            return null;

        }

        // Obtener los logros segun tiempo resistido en modo survival
        public static string[] GetAchivementSurvivalMode(int seconds)
        {
            switch (seconds)
            {
                case 10:
                    return new string[1] { "CgkIzIuQypIeEAIQJw" };
                case 30:
                    return new string[2] { "CgkIzIuQypIeEAIQBQ", "CgkIzIuQypIeEAIQKA" };
                case 60:
                    return new string[1] { "CgkIzIuQypIeEAIQKQ" };
                case 120:
                    return new string[1] { "CgkIzIuQypIeEAIQKg" };
                case 150:
                    return new string[1] { "x1" };
                case 180:
                    return new string[1] { "x2" };
                case 210:
                    return new string[1] { "x3" };
                case 240:
                    return new string[1] { "x4" };
                case 270:
                    return new string[1] { "x5" };
                case 300:
                    return new string[1] { "CgkIzIuQypIeEAIQKw" };
                case 330:
                    return new string[1] { "x6" };
                case 360:
                    return new string[1] { "x7" };
                case 390:
                    return new string[1] { "x8" };
                case 420:
                    return new string[1] { "x9" };
                case 450:
                    return new string[1] { "x10" };
                case 600:
                    return new string[1] { "CgkIzIuQypIeEAIQLA" };
                case 666:
                    return new string[1] { "x11" };
            }

            return null;

        }

    }

    // Especificos de deluxe
    public class Deluxe
    {
        // Estados en los que puede estar un item no consumible
        public enum States
        {
            available,
            bought
        }

        // Monedas ganadas al comprar
        public static int coins = 600;

        // Listado de packs disponibles para comprar
        public static List<Pack> packsList = new List<Pack>() { new Legends(), new GolpeaDuroHara(), new Pichanga(), new ElBrayan() };

        // Base para los distintos packs
        public class Pack
        {
            public string productID = "";
            public string name = "";
            public int humitas = 1000;
            public List<string> characters = new List<string>() { };
            public List<string> crowd = new List<string>();
        }

        // Pack Legendas, de los que nos apoyaron en un comienzo
        public class Legends : Pack
        {
            public Legends()
            {
                productID = "com.humita.jankenup.pack.legends";
                name = "legends";
                characters.Add(Characters.GASTONMIAUFFIN);
            }
        }

        // Pack Golpea Duro Hara, de Marmota Studio
        public class GolpeaDuroHara : Pack
        {
            public GolpeaDuroHara()
            {
                productID = "com.humita.jankenup.pack.golpeadurohara";
                name = "golpeadurohara";
            }
        }

        // Pack Pichanga
        public class Pichanga : Pack
        {
            public Pichanga()
            {
                productID = "com.humita.jankenup.pack.pichanga";
                name = "pichanga";
            }
        }

        // Pack El Brayan
        public class ElBrayan : Pack
        {
            public ElBrayan()
            {
                productID = "com.humita.jankenup.pack.elbrayanmimundoalex";
                name = "elbrayan";
            }
        }

        public static string legacyProduct = "com.humita.jankenup.deluxe";
    }

    // Colores de janken
    public class JankenColors
    {
        public static UnityEngine.Color white = new UnityEngine.Color(212/255f, 244/255f, 221/255f, 1);
        public static UnityEngine.Color clearWhite = new UnityEngine.Color(212/255f, 244/255f, 221/255f, 0);
    }

    // JanKenSHOP!
    public class Shop
    {
        public static int limitPerItem = 3;
    }

    // Otros limites
    public class Limits
    {
        public static int superPowerUpLimit = 3;
        public static int minLives = 1;
        public static int maxLives = 5;
        public static int minStrikes = 1;
        public static int maxStrikes = 5;
    }
    
    // Reviews
    public class Reviews
    {
        public static int minSessionsBeforePrompt = 6;
    }

    // NPC
    public class NPC
    {
        public enum Identifiers
        {
            None,
            Shive
        }

        public enum Size
        {
            Small,
            Medium,
            Big
        }
    }

    // Sorting Layers
    public class SpritesSortingLayers
    {
        public static string Default = "Default";
        public static string ArenaBackground = "ArenaBackground";
        public static string ArenaCrowd = "ArenaCrowd";
        public static string ArenaCrowdSmall = "ArenaCrowdSmall";
        public static string ArenaCrowdMedium = "ArenaCrowdMedium";
        public static string ArenaCrowdBig = "ArenaCrowdBig";
        public static string ArenaPlatform = "ArenaPlatform";
        public static string Arena = "Arena";
        public static string Foreground = "Foreground";
    }

    // Identificadores de arena
    public enum BackgroundElementsIdentifier
    {
        Classic,
        Shoji,
        Arcade,
        Subway,
        Fonda2022,
        HumbleHouse
    }

    // Configuraciones relacionadas a las arenas
    public class Arenas
    {
        public static Dictionary<string, BackgroundElementsIdentifier> identifiers = new Dictionary<string, BackgroundElementsIdentifier>() {
            {"CLASSIC", BackgroundElementsIdentifier.Classic},
            {"SHOJI", BackgroundElementsIdentifier.Shoji},
            {"ARCADE", BackgroundElementsIdentifier.Arcade},
            {"SUBWAY", BackgroundElementsIdentifier.Subway},
            {"FONDA2022", BackgroundElementsIdentifier.Fonda2022},
            {"HUMBLEHOUSE", BackgroundElementsIdentifier.HumbleHouse}
        };

        public static int backgroundLenght = 6;
    }

    // Eventos relacionados con las analiticas
    public class AnalyticsEvents
    {
        // Identificadores de eventos
        public enum Events
        {
            OnlineSelectedCharacter,
            OfflineSelectedCharacter
        }

        // Eventos
        public static Dictionary<AnalyticsEvents.Events, string> EventsNames = new Dictionary<AnalyticsEvents.Events, string>()
        {
            { AnalyticsEvents.Events.OnlineSelectedCharacter, "OnlineSelectedCharacter"},
            { AnalyticsEvents.Events.OfflineSelectedCharacter, "OfflineSelectedCharacter"}
        };

        /// <summary>
        /// Obtener el valor en string de un evento
        /// </summary>
        /// <param name="eventKey"></param>
        /// <returns></returns>
        public static string GetEventString(AnalyticsEvents.Events eventKey)
        {
            string value = "";
            AnalyticsEvents.EventsNames.TryGetValue(eventKey, out value);
            return value;
        }
    }
    
}