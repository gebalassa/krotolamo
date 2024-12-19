using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DeepLinkManager : MonoBehaviour
{
    public static DeepLinkManager Instance { get; private set; }
    public string deeplinkURL;
    public bool deeplinkValid = false;
    private string[] customProtocols = new string[2] { "https://jankenup.com/deep/", "jankenup://deep/" };
    private string nextScene = "";

    // Palabras reservadas para los parametros
    List<string> reservedWords = new List<string>() { "room", "currentplayername", "charidentifier" };

    // Cada par de parametros corresponde a un parametro valido. Ahora, si existen 2 palabras reservadas como llave-valor, el deeplink es invalido a menos que se demuestre lo contrario (Filosofo?)
    Dictionary<string, string> keyValues = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.deepLinkActivated += onDeepLinkActivated;
            if (String.IsNullOrEmpty(Application.absoluteURL)){
                deeplinkURL = "[none]";
            }
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void onDeepLinkActivated(string url)
    {

        // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
        deeplinkURL = url;
        deeplinkValid = false;

        // Extraer los parametros
        deeplinkURL = UnityWebRequest.UnEscapeURL(deeplinkURL);
        foreach (string protocol in customProtocols)
        {
            deeplinkURL = deeplinkURL.Replace(protocol, "");
        }
        string[] parameters = deeplinkURL.Split('/');

        // Limpiar los valores previos
        ClearKeyValues();

        // Si no existe al menos 2 parametros, el link no se considerara valido
        if (parameters.Length < 2) return;

         // Obtener la accion principal
         string action = parameters[0];

        for(int i = 0; i < parameters.Length; i += 2)
        {
            // Primero revisar si se cumple con la condicion para el value
            if (i + 1 >= parameters.Length) break;

            string key = parameters[i];
            string value = parameters[i + 1];

            keyValues.Add(key, value);
        }

        // Si existe el initController, dejar que se encargue de derivar. De lo contrario, derivar por aca
        bool dealWithIt = true;
        if (!FindObjectOfType<InitSceneController>()) dealWithIt = true;

        // Revisar el primer par para ver que accion se va a realizar
        switch (action.ToLower())
        {
            default:
                nextScene = "";
                ClearKeyValues();
                break;
        }

        if (dealWithIt && nextScene != "")
        {
            StartCoroutine(MoveToNextScene());
        }

    }

    // Mover a la siguiente escena cuando este lista las puertas
    private IEnumerator MoveToNextScene()
    {
        while (!TransitionDoors._this.IsTotallyOpen()) yield return null;

        SceneLoaderManager sceneLoaderManager = FindObjectOfType<SceneLoaderManager>();
        sceneLoaderManager.GoTo(GetNextScene());
    }

    // Obtencion de las llaves/valores actuales
    public Dictionary<string,string> GetKeyValues()
    {
        // Como estamos hablando de strings, podemos hacer una clonacion como la siguiente, pero lo recomendable es tener un metodo de copia
        Dictionary<string, string> copy = new Dictionary<string, string>(keyValues);
        ClearKeyValues();
        return copy;
    }

    // Obtencion del valor de una llave en especifico
    public string GetValue(string key)
    {
        return keyValues.ContainsKey(key) ? keyValues[key] : "";
    }


    // Borrado de las llaves/valores actuales
    public void ClearKeyValues()
    {
        deeplinkValid = false;
        keyValues.Clear();
    }

    // Obtener la siguiente scene configurada
    public string GetNextScene()
    {
        string nextCopy = nextScene;
        nextScene = "";
        return nextCopy;
    }
}