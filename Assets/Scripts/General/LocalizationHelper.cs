using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Threading.Tasks;
using UnityEngine.Localization.SmartFormat;
using ArabicSupport;
using System.Collections.Generic;
using System;

public class LocalizationHelper
{

    public static LocalizationHelper _this;

    // Metodo para encontrar un texto en base a su key
    public static void Translate(TextMeshProUGUI textObject, string table, string key, string prefix = null, string postfix = null, Action<bool> action = null)
    {
        if (_this == null) _this = new LocalizationHelper();
        _this.TranslateThis(textObject, table, key, prefix, postfix, action);
    }

    // Metodo para encontrar un texto en base a su key
    public static void Translate(TextMeshPro textObject, string table, string key, string prefix = null, string postfix = null, Action<bool> action = null)
    {
        if (_this == null) _this = new LocalizationHelper();
        _this.TranslateThis(textObject, table, key, prefix, postfix, action);
    }

    // Metodo para encontrar un texto en base a su key
    public static void Translate(Text textObject, string table, string key, string prefix = null, string postfix = null, Action<bool> action = null)
    {
        if (_this == null) _this = new LocalizationHelper();
        _this.TranslateThis(textObject, table, key, prefix, postfix, action);
    }

    // Metodo para encontrar un texto en base a su key
    public void TranslateThis(TextMeshProUGUI textObject, string table, string key, string prefix = null, string postfix = null, Action<bool> action = null)
    {
        var translatedText = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key);
        translatedText.Completed += (delegate {

            string result = prefix != null || postfix != null ? string.Format("{0}{1}{2}", prefix != null? prefix : "", translatedText.Result, postfix != null ? postfix : "") : translatedText.Result;
            textObject.text = FontManager._mainManager.IsArabic() ? ArabicFixer.Fix(result) : result;
            if (action != null) action(true);
        });
    }

    public void TranslateThis(TextMeshPro textObject, string table, string key, string prefix = null, string postfix = null, Action<bool> action = null)
    {
        var translatedText = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key);
        translatedText.Completed += (delegate {
            string result = prefix != null || postfix != null ? string.Format("{0}{1}{2}", prefix != null ? prefix : "", translatedText.Result, postfix != null ? postfix : "") : translatedText.Result;
            textObject.text = FontManager._mainManager.IsArabic() ? ArabicFixer.Fix(result) : result;
            if (action != null) action(true);
        });
    }

    public void TranslateThis(Text textObject, string table, string key, string prefix = null, string postfix = null, Action<bool> action = null)
    {
        var translatedText = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key);
        translatedText.Completed += (delegate {
            string result = prefix != null || postfix != null ? string.Format("{0}{1}{2}", prefix != null ? prefix : "", translatedText.Result, postfix != null ? postfix : "") : translatedText.Result;
            textObject.text = FontManager._mainManager.IsArabic() ? ArabicFixer.Fix(result) : result;
            if (action != null) action(true);
        });
    }

    // Metodo para poblar un texto con Smart
    public static void FormatTranslate(TextMeshProUGUI textObject, string table, string key, object[] args)
    {
        if (_this == null) _this = new LocalizationHelper();
        _this.FormatTranslateThis(textObject, table, key, args);
    }

    public static void FormatTranslate(TextMeshPro textObject, string table, string key, object[] args)
    {
        if (_this == null) _this = new LocalizationHelper();
        _this.FormatTranslateThis(textObject, table, key, args);
    }

    public static void FormatTranslate(Text textObject, string table, string key, object[] args)
    {
        if (_this == null) _this = new LocalizationHelper();
        _this.FormatTranslateThis(textObject, table, key, args);
    }

    public void FormatTranslateThis(TextMeshProUGUI  textObject, string table, string key, object[] args)
    {
        var translatedText = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key, null, FallbackBehavior.UseProjectSettings, args);
        translatedText.Completed += (delegate {
            if (FontManager._mainManager.IsArabic())
            {
                textObject.text = ArabicFixer.Fix(translatedText.Result);
            }
            else
            {
                textObject.text = translatedText.Result;
            }
        });
    }

    public void FormatTranslateThis(TextMeshPro textObject, string table, string key, object[] args)
    {
        var translatedText = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key, null, FallbackBehavior.UseProjectSettings, args);
        translatedText.Completed += (delegate {
            if (FontManager._mainManager.IsArabic())
            {
                textObject.text = ArabicFixer.Fix(translatedText.Result);
                Debug.Log(translatedText.Result);
            }
            else
            {
                textObject.text = translatedText.Result;
            }
        });
    }

    public void FormatTranslateThis(Text textObject, string table, string key, object[] args)
    {
        var translatedText = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key, null, FallbackBehavior.UseProjectSettings, args);
        translatedText.Completed += (delegate {
            if (FontManager._mainManager.IsArabic())
            {
                textObject.text = ArabicFixer.Fix(translatedText.Result);
            }
            else
            {
                textObject.text = translatedText.Result;
            }
        });
    }

    /// <summary>
    /// Traduccion multiple de elementos, agregando cada uno al Text que se indica
    /// </summary>
    /// <param name="textObject"></param>
    /// <param name="tables"></param>
    /// <param name="keys"></param>
    public IEnumerator TranslateGetText(Text textObject, List<string> tables, List<string> keys)
    {
        bool completeRound = false;
        int index = 0;

        while(index < tables.Count)
        {
            completeRound = false;

            string table = tables[index];
            string key = index < keys.Count ? keys[index] : "";

            if(key != "")
            {
                var translatedText = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key, null, FallbackBehavior.UseProjectSettings);

                translatedText.Completed += (delegate {
                    textObject.text = string.Format("{0} {1}", textObject.text, translatedText.Result).Trim();
                    completeRound = true;
                });

                while (!completeRound) yield return null;
            }

            index++;
        }

        if (FontManager._mainManager.IsArabic()) textObject.text = ArabicFixer.Fix(textObject.text);
    }

}