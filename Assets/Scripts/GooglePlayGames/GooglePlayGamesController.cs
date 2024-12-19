using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class GooglePlayGamesController : MonoBehaviour
{
    static bool activated = false;

    // Singleton
    public static GooglePlayGamesController _this;

    // Lista de achievements reportados cuando el usuario no estaba aun autentificado
    public static List<Tuple<string, float>> achivementsBeforeLogin = new List<Tuple<string, float>>();

    private void Awake()
    {
        int length = FindObjectsOfType<GooglePlayGamesController>().Length;
        if (length == 1)
        {
            DontDestroyOnLoad(gameObject);
            _this = this;

            if (!GooglePlayGamesController.activated)
            {

                /*PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                // enables saving game progress.
                //.EnableSavedGames()
                .Build();

                PlayGamesPlatform.InitializeInstance(config);
                // Activate the Google Play Games platform
                PlayGamesPlatform.Activate();*/

                GooglePlayGamesController.activated = true;
            }

        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Solicitud desde el menu al acceder alguno de los modos por primera vez
    public void FirstTimeSetup(Action postMethod)
    {
        // La primera vez, pedir confirmacion para los datos altiro
        /*PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, (result) =>
        {

            if (result != SignInStatus.Canceled && result != SignInStatus.UiSignInRequired)
            {
                Social.localUser.Authenticate((bool success) =>
                {
                    if (success)
                    {
                        ((GooglePlayGames.PlayGamesPlatform)Social.Active).SetGravityForPopups(Gravity.BOTTOM);

                        // Revisar si existen logros que deben ser reportados
                        CheckUnreportedAchievement();

                        // Llamar a metodo
                        postMethod();

                    }
                });
            }

        });*/
    }

    // Llamar a la configuracion sin setup de fondo
    public void FirstTimeSetup()
    {
        FirstTimeSetup( () => { });
    }

    // Intentar iniciar sesion en Google Play Games Services
    public void Setup(Action postMethod)
    {
        /*PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (result) =>
        {
            if (result == SignInStatus.Success)
            {
                ((GooglePlayGames.PlayGamesPlatform)Social.Active).SetGravityForPopups(Gravity.BOTTOM);

                // Revisar si existen logros que deben ser reportados
                CheckUnreportedAchievement();

                // Llamar a metodo
                postMethod();

            }

        });*/
    }

    // Mostrar los diferentes tableros de liderazgos
    public void ShowLeaderboards() {

        if (Social.localUser.authenticated)
        {
            Social.ShowLeaderboardUI();
        }
        else
        {
            Setup( () => Social.ShowLeaderboardUI() );
        }
    }

    // Mostrar los logros del juego
    public void ShowAchievements()
    {
        if (Social.localUser.authenticated)
        {
            Social.ShowAchievementsUI();
        }
        else
        {
            Setup(() => Social.ShowAchievementsUI());
        }
    }

    // Solicitud de completitud de logro
    public bool ReportAchievement(string achievement, float progress) {

        // Solo reportar si el jugador esta logeado
        if (Social.localUser.authenticated)
        {
            Social.ReportProgress(achievement, progress, (bool achievementSuccess) => { });
            return true;
        }
        else
        {
            achivementsBeforeLogin.Add(new Tuple<string, float>( achievement, progress));
        }

        return false;

    }

    // Solicitud de completitud de logro forzando la autenticacion del usuario a GooglePlayGames
    public void ReportAchievement(string achievement, float progress, bool forceSignIn)
    {

        bool reported = ReportAchievement(achievement, progress);
        if(!reported && forceSignIn)
        {
            Setup(() => ReportAchievement(achievement, progress));
        }

    }

    // Revisar los achievements no reportados por que el jugador no estaba logeado
    private void CheckUnreportedAchievement()
    {
        if (achivementsBeforeLogin.Count == 0) return;
        
        foreach(Tuple<string,float> achievement in achivementsBeforeLogin)
        {
            this.ReportAchievement(achievement.Item1, achievement.Item2);
        }

        achivementsBeforeLogin.Clear();

    }


}