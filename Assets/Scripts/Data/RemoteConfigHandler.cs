using System.Collections;
using System.Collections.Generic;
using Unity.Services.RemoteConfig;
using UnityEngine;
using JankenUp;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class RemoteConfigHandler : MonoBehaviour
{
#if DEBUG
    private string environmentID = "fdf4816a-6869-480d-9569-20693ad4acab";
#else
    private string environmentID = "cdc8e4c8-0563-4524-a686-deba0caddce0";
#endif

    // Referencia al sfxplayer del juego
    public static RemoteConfigHandler _main;

    public struct userAttributes{}

    public struct appAttributes{}

    // Llaves de las configuracions
    public const string FREE_DELUXE_CHARACTERS = "freeDeluxeCharacters";
    public const string FREE_DELUXE_UNTIL = "freeDeluxeUntil";
    public const string JANKENSHOP = "JanKenShop";

    // Valores por defecto asociados a las llaves
    public Dictionary<string, object> values = new Dictionary<string, object>()
    {
        { FREE_DELUXE_CHARACTERS, "" },
        { FREE_DELUXE_UNTIL, "" },
        { JANKENSHOP, "{}" }
    };

    // Especificaciones de que tipo de parse se debe realizar por cada llave diferente a string
    public Dictionary<string, string> parseMethod = new Dictionary<string, string>()
    {
        {JANKENSHOP, "JSON" }
    };

    // Singleton
    private async Task Awake()
    {
        int length = FindObjectsOfType<RemoteConfigHandler>().Length;
        if (length == 1)
        {
            DontDestroyOnLoad(gameObject);
            _main = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (Utilities.CheckForInternetConnection())
        {
            await InitializeRemoteConfigAsync();
        }

        // Configurar el ambiente
        RemoteConfigService.Instance.SetEnvironmentID(environmentID);

        // Add a listener to apply settings when successfully retrieved: 
        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;

        // Fetch configuration setting from the remote service: 
        RemoteConfigService.Instance.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());

    }

    /// <summary>
    /// Inicializar el core
    /// </summary>
    /// <returns></returns>
    async Task InitializeRemoteConfigAsync()
    {
        // options can be passed in the initializer, e.g if you want to set analytics-user-id or an environment-name use the lines from below:
        var options = new InitializationOptions()
            .SetOption("com.unity.services.core.environment-id", environmentID);
        await UnityServices.InitializeAsync(options);

        // remote config requires authentication for managing environment information
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }


    void ApplyRemoteSettings(ConfigResponse configResponse)
    {

        // Conditionally update settings, depending on the response's origin:
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                break;
            case ConfigOrigin.Cached:
                break;
            case ConfigOrigin.Remote:
                UpdateValues();
                break;
        }

    }

    // Actualizacion de los valores de las llaves
    private void UpdateValues()
    {

        // Ajustar los valores segun respuesta
        foreach (string key in RemoteConfigService.Instance.appConfig.GetKeys())
        {
            if (parseMethod.ContainsKey(key)){

                switch (parseMethod[key]) {
                    case "JSON":
                        values[key] = RemoteConfigService.Instance.appConfig.GetJson(key);
                        break;
                }

            }
            else
            {
                if (values.ContainsKey(key)) values[key] = RemoteConfigService.Instance.appConfig.GetString(key);
                else values.Add(key, RemoteConfigService.Instance.appConfig.GetString(key));
            }
        }

        // Ejecutar la actualizacion de las configuraciones conocidas
        StartCoroutine(ExecuteUpdate());

        // Convertir la data de JanKenShop
        ConvertJanKenShopJson();

    }

    // Ejecucion de actualizacion como el despliegue de los nuevos personajes deluxes gratis
    private IEnumerator ExecuteUpdate()
    {
        while (!IAPController.IsInitialized()) yield return null;
        while (!CharacterPool.Instance) yield return null;

        // Revision de los personajes deluxe liberados
        string[] deluxeCharacters = (values[FREE_DELUXE_CHARACTERS] as string).Split(',');
        List<string> freeCharacters = new List<string>();

        foreach (string identifier in deluxeCharacters)
        {
            // Comprobar que sea un identificador valido
            if (Characters.DeluxeCharacters.Contains(identifier) && CharacterPool.Instance.Get(identifier) && !GameController.IsCharacterInPlayerAccount(identifier))
            {
                UnlockedDeluxeController.NewUnlock(identifier);
                freeCharacters.Add(identifier);
            }
        }

        // Quitar todos los personajes exclusivos deluxe que ya no sean gratuitos
        foreach (string identifier in Characters.DeluxeCharacters)
        {
            if (!freeCharacters.Contains(identifier) && !GameController.IsCharacterInPlayerAccount(identifier) && !JankenUp.Characters.SecretCharacters.Contains(identifier))
            {
                UnlockedCharacterController.RemoveCharacter(identifier);
            }
        }

        // Realizar el guardado de la data
        UnlockedCharacterController.SaveUnlockedCharacter();
    }

    // Obtener hasta cuando los personajes deluxes gratis estan gratis
    public string GetDeluxeUntil()
    {
        return RemoteConfigService.Instance.appConfig.GetString(FREE_DELUXE_UNTIL);
    }

    /// <summary>
    /// Convierte el string obtenido del remoto de JanKenShop
    /// </summary>
    private void ConvertJanKenShopJson()
    {
        try
        {
            string json = values[JANKENSHOP] as string;
            JanKenShopJSON janKenShopJSON = JanKenShopJSON.CreateFromJSON(json);
            JanKenShopJSON.SetInstance(janKenShopJSON);
        }
        catch(Exception e) {
            Debug.Log(e);
        }
    }

}