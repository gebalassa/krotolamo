using System.Collections;
using UnityEngine;
using TMPro;

public class JanKenShopCharacterCard : JanKenShopContentCard
{
    [Header("Setup")]
    [SerializeField] CharacterConfiguration characterConfiguration;
    [SerializeField] GameObject characterOverlay;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        // Anadir la apertura del overlay del personaje
        button.onClick.AddListener(ShowCharacterMoreInfo);
    }

    // Mostrar la informacion adicional del personaje deluxe seleccionado actualmente
    public void ShowCharacterMoreInfo()
    {
        // Obtener la escena actual
        SceneController sceneController = FindObjectOfType<SceneController>();

        // Mostrar el detalle del personaje
        sceneController.SwitchUI(OpenDetail());

    }

    IEnumerator OpenDetail()
    {
        JanKenShopCharacterOverlay jksOverlay = Instantiate(characterOverlay).GetComponent<JanKenShopCharacterOverlay>();
        jksOverlay.Setup(characterConfiguration, false);
        yield return null;
    }
}