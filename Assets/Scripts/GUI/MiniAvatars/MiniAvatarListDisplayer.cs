using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MiniAvatarListDisplayer : MonoBehaviour {

    [Header("Setup")]
    [SerializeField] Transform playersContainer;
    [SerializeField] GameObject miniAvatarDisplayerPrefab;

    // Utiles
    List<MiniAvatarDisplayer> miniAvatarList = new List<MiniAvatarDisplayer>();

    /// <summary>
    /// Llamada para crear los elementos
    /// </summary>
    public void Init() {
        for(int i = 0; i < CharacterPool.Instance.GetPlayersCount(); i++)
        {
            MiniAvatarDisplayer miniAvatarDisplayer = Instantiate(miniAvatarDisplayerPrefab, playersContainer).GetComponent<MiniAvatarDisplayer>();
            miniAvatarList.Add(miniAvatarDisplayer);
            miniAvatarDisplayer.Setup(i);
        }
    }

    /// <summary>
    /// Refresco de los avatares
    /// </summary>
    /// <param name="playerIndex">Actualizacion de avbatar en especifico</param>
    public void Refresh(int playerIndex = -1) {
        if (playerIndex == -1)
        {
            foreach(MiniAvatarDisplayer miniAvatarDisplayer in miniAvatarList)
            {
                miniAvatarDisplayer.Refresh();
            }
        }
        else if (playerIndex >= 0 && playerIndex < miniAvatarList.Count) miniAvatarList[playerIndex].Refresh();
    }

}