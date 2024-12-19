using UnityEngine;

public class LangFlag : MonoBehaviour{
    [SerializeField] LanguageAvailable languageCode;

    /// <summary>
    /// Obtencion del codigo en formato para localizacion
    /// </summary>
    /// <returns></returns>
    public string GetLangCode()
    {
        string[] code = languageCode.ToString().Split("_");
        for(int i = 0; i < code.Length; i++)
        {
            if (i == 0) code[i] = code[i].ToLower();
            else code[i] = code[i].ToUpper();
        }
        return string.Join("-", code);
    }
}