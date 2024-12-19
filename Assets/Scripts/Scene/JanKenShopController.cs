using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

public class JanKenShopController : SceneController
{
    [Header("Containers")]
    [SerializeField] GameObject contentContainer;
    [SerializeField] GameObject elementsContainer;

    [Header("Packs")]
    [SerializeField] List<string> packs;

    [Header("Others")]
    [SerializeField] Button restoreButton;

    new void Start()
    {
        base.Start();

        // Iniciar todos los elementos disponibles
        StartCoroutine(FillTheShop());

        // Actualizar la fuente de todos los labels
        UpdateCurrentFont();
    }

    /// <summary>
    /// Agregar el contenido de la tienda
    /// </summary>
    IEnumerator FillTheShop()
    {
        // Revisar si es necesario mostrar o no el boton de restore
        restoreButton.gameObject.SetActive(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor);

        // Agregar los packs
        foreach (string key in packs)
        {
            yield return LoadAsset(key, elementsContainer.transform);
        }

        // Ajustar contenido
        StartCoroutine(AdjustContentWidth());
    }

    /// <summary>
    /// Carga de un asset especifico
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private IEnumerator LoadAsset(string key, Transform parent)
    {
        AsyncOperationHandle<GameObject> opHandle = Addressables.LoadAssetAsync<GameObject>(key);
        yield return opHandle;
        if (opHandle.Status == AsyncOperationStatus.Succeeded)
        {
            var op = Addressables.InstantiateAsync(key, parent);
            if (op.IsDone)
            {
                Addressables.Release(opHandle);
            }
        }
    }

    /// <summary>
    /// Realiza el ajuste del ancho para el contenedor del contenido scroleable
    /// </summary>
    /// <returns></returns>
    IEnumerator AdjustContentWidth()
    {
        yield return new WaitForEndOfFrame();

        // Obtener los datos para determinar el ancho
        HorizontalLayoutGroup gridLayout = elementsContainer.GetComponent<HorizontalLayoutGroup>();
        float pageWidth = ((RectTransform)elementsContainer.transform).rect.width + gridLayout.padding.left;

        // Actualizar el elemento
        RectTransform rect = ((RectTransform)contentContainer.transform);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pageWidth);
    }

    // Actualiza todos los elementos ligados a un translate
    protected override void Localize(){}

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        //gameDesign.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;

        //gameDesign.fontStyle = style;
    }

    /// <summary>
    /// Llamada cuando es presionado el click de restore
    /// </summary>
    public void RestoreInit()
    {
        restoreButton.interactable = false;
    }

}
