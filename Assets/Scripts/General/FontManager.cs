using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;

public class FontManager : MonoBehaviour
{

    [Header("Main Fonts")]
    [SerializeField] TMP_FontAsset MainFont;
    [SerializeField] TMP_FontAsset MainJapaseneFont;
    [SerializeField] TMP_FontAsset MainKoreanFont;
    [SerializeField] TMP_FontAsset MainChineseFont;
    [SerializeField] TMP_FontAsset MainRussianFont;
    [SerializeField] TMP_FontAsset MainArabicFont;

    [Header("Main Fonts Material")]
    [SerializeField] Material MaterialMainFontBlack;
    [SerializeField] Material MaterialMainFontWhite;
    [SerializeField] Material MaterialMainJapaseneFontBlack;
    [SerializeField] Material MaterialMainJapaseneFontWhite;
    [SerializeField] Material MaterialMainKoreanFontBlack;
    [SerializeField] Material MaterialMainKoreanFontWhite;
    [SerializeField] Material MaterialMainChineseFontBlack;
    [SerializeField] Material MaterialMainChineseFontWhite;
    [SerializeField] Material MaterialMainRussianFontBlack;
    [SerializeField] Material MaterialMainRussianFontWhite;
    [SerializeField] Material MaterialMainArabicFontBlack;
    [SerializeField] Material MaterialMainArabicFontWhite;

    [Header("Score Fonts")]
    [SerializeField] TMP_FontAsset ScoreFont;
    [SerializeField] TMP_FontAsset ScoreJapaseneFont;
    [SerializeField] TMP_FontAsset ScoreKoreanFont;
    [SerializeField] TMP_FontAsset ScoreChineseFont;
    [SerializeField] TMP_FontAsset ScoreRussianFont;
    [SerializeField] TMP_FontAsset ScoreArabicFont;

    [Header("Referee Fonts")]
    [SerializeField] TMP_FontAsset RefereeeFont;
    [SerializeField] TMP_FontAsset RefereeJapaseneFont;
    [SerializeField] TMP_FontAsset RefereeKoreanFont;
    [SerializeField] TMP_FontAsset RefereeChineseFont;
    [SerializeField] TMP_FontAsset RefereeRussianFont;
    [SerializeField] TMP_FontAsset RefereeArabicFont;

    [Header("Announcement Fonts")]
    [SerializeField] TMP_FontAsset AnnouncementFont;
    [SerializeField] TMP_FontAsset AnnouncementJapaseneFont;
    [SerializeField] TMP_FontAsset AnnouncementKoreanFont;
    [SerializeField] TMP_FontAsset AnnouncementChineseFont;
    [SerializeField] TMP_FontAsset AnnouncementRussianFont;
    [SerializeField] TMP_FontAsset AnnouncementArabicFont;

    [Header("Plain Fonts")]
    [SerializeField] Font MainPlainFont;
    [SerializeField] Font JapasenePlainFont;
    [SerializeField] Font KoreanPlainFont;
    [SerializeField] Font ChinesePlainFont;
    [SerializeField] Font RussianPlainFont;
    [SerializeField] Font ArabicPlainFont;

    // Unico FontManager
    public static FontManager _mainManager;

    // Idiomas soportados
    public const string japanese = "ja";
    public const string korean = "ko";
    public const string chinese = "zh";
    public const string russian = "ru";
    public const string arabic = "ar";

    // Tipos de materiales (Para outline)
    public enum MainFontMaterial
    {
        Black,
        White
    }

    // Singleton
    private void Awake()
    {
        int length = FindObjectsOfType<FontManager>().Length;
        if (length == 1)
        {
            DontDestroyOnLoad(gameObject);
            _mainManager = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Obtener la fuente principal segun idioma
    public TMP_FontAsset GetMainFont()
    {
        switch (LocalizationSettings.SelectedLocale.Identifier.Code)
        {
            case japanese:
                return MainJapaseneFont;
            case korean:
                return MainKoreanFont;
            case chinese:
                return MainChineseFont;
            case russian:
                return MainRussianFont;
            case arabic:
                return MainArabicFont;
            default:
                return MainFont;
        }
    }

    // Obtener la fuente del referee segun idioma
    public TMP_FontAsset GetRefereeFont()
    {
        switch (LocalizationSettings.SelectedLocale.Identifier.Code)
        {
            case japanese:
                return RefereeJapaseneFont;
            case korean:
                return RefereeKoreanFont;
            case chinese:
                return RefereeChineseFont;
            case russian:
                return RefereeRussianFont;
            case arabic:
                return RefereeArabicFont;
            default:
                return RefereeeFont;
        }
    }


    // Obtener la fuente de los anuncios del referee segun idioma
    public TMP_FontAsset GetAnnouncementFont()
    {
        switch (LocalizationSettings.SelectedLocale.Identifier.Code)
        {
            case japanese:
                return AnnouncementJapaseneFont;
            case korean:
                return AnnouncementKoreanFont;
            case chinese:
                return AnnouncementChineseFont;
            case russian:
                return AnnouncementRussianFont;
            case arabic:
                return AnnouncementArabicFont;
            default:
                return AnnouncementFont;
        }
    }

    // Obtener la fuente del score segun idioma
    public TMP_FontAsset GetScoreFont()
    {
        switch (LocalizationSettings.SelectedLocale.Identifier.Code)
        {
            case japanese:
                return ScoreJapaseneFont;
            case korean:
                return ScoreKoreanFont;
            case chinese:
                return ScoreChineseFont;
            case russian:
                return ScoreRussianFont;
            case arabic:
                return ScoreArabicFont;
            default:
                return ScoreFont;
        }
    }

    // Obtener la fuente CP para usar en texto plano segun idioma de jugador
    public Font GetPlainFont()
    {
        switch (LocalizationSettings.SelectedLocale.Identifier.Code)
        {
            case japanese:
                return JapasenePlainFont;
            case korean:
                return KoreanPlainFont;
            case chinese:
                return ChinesePlainFont;
            case russian:
                return RussianPlainFont;
            case arabic:
                return ArabicPlainFont;
            default:
                return MainPlainFont;
        }

    }

    // Obtener si un idioma necesita que el estilo sea Bold
    public bool IsBold()
    {
        switch (LocalizationSettings.SelectedLocale.Identifier.Code)
        {
            case russian:
                return true;
            default:
                return false;
        }
    }

    // Revisar si es arabe
    public bool IsArabic()
    {
        return LocalizationSettings.SelectedLocale.Identifier.Code == arabic;
    }

    /// <summary>
    /// Obtener el material relacionado con la fuente principal
    /// </summary>
    /// <returns></returns>
    public Material GetMainMaterial(MainFontMaterial mainFontMaterial)
    {
        switch (LocalizationSettings.SelectedLocale.Identifier.Code)
        {
            case japanese:
                return mainFontMaterial == MainFontMaterial.Black? MaterialMainJapaseneFontBlack : MaterialMainJapaseneFontWhite;
            case korean:
                return mainFontMaterial == MainFontMaterial.Black ? MaterialMainKoreanFontBlack : MaterialMainKoreanFontWhite;
            case chinese:
                return mainFontMaterial == MainFontMaterial.Black ? MaterialMainChineseFontBlack : MaterialMainChineseFontWhite;
            case russian:
                return mainFontMaterial == MainFontMaterial.Black ? MaterialMainRussianFontBlack : MaterialMainRussianFontWhite;
            case arabic:
                return mainFontMaterial == MainFontMaterial.Black ? MaterialMainArabicFontBlack : MaterialMainArabicFontWhite;
            default:
                return mainFontMaterial == MainFontMaterial.Black ? MaterialMainFontBlack : MaterialMainFontWhite;
        }
    }

}