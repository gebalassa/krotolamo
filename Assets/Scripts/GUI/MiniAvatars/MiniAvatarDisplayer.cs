using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniAvatarDisplayer : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] Image illustration;
    [SerializeField] TextMeshProUGUI livesLabel;

    // Utiles
    int playerIndex = -1;
    CharacterConfiguration characterConfiguration;
    SingleModeSession singleModeSession;
    private Material normalMaterial;
    private Material blackAndWhiteMaterial;

    /// <summary>
    /// Realizar copia de material de imagen
    /// </summary>
    private void Awake()
    {
        normalMaterial = illustration.material;
        blackAndWhiteMaterial = new Material(illustration.material);
        blackAndWhiteMaterial.SetFloat("_GrayscaleAmount", 1);
    }

    /// <summary>
    /// Configuracion de jugador al que hara referencia el displayer
    /// </summary>
    /// <param name="playerIndex"></param>
    public void Setup(int playerIndex = 0) {
        GameObject player = CharacterPool.Instance.GetCurrentCharacter(playerIndex);
        if (!player)
        {
            gameObject.SetActive(false);
            return;
        }
        this.playerIndex = playerIndex;
        characterConfiguration = player.GetComponent<CharacterConfiguration>();
        singleModeSession = FindObjectOfType<SingleModeSession>();
        SetLives();
        SetAvatar();
    }

    /// <summary>
    /// Actualizacion del avatar del jugador
    /// </summary>
    private void SetAvatar(){
        if (playerIndex == -1) return;
        illustration.sprite = characterConfiguration.GetAvatar();
    }

    /// <summary>
    /// Actualizacion de las vidas del jugador
    /// </summary>
    private void SetLives() {
        if (playerIndex == -1) return;
        int playerLives = singleModeSession.GetLives(playerIndex);
        if (livesLabel) livesLabel.text = string.Format("x{0}", playerLives <= 0? "D" : playerLives);
        illustration.material = playerLives <= 0? blackAndWhiteMaterial : normalMaterial;
    }

    /// <summary>
    /// Refresco de las vidas del jugador asociado
    /// </summary>
    public void Refresh()
    {
        if (playerIndex == -1) return;
        SetLives();
        SetAvatar();
    }
}