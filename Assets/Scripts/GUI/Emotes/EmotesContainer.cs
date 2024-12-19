using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotesContainer : MonoBehaviour
{
    [SerializeField] List<Transform> spawnPortals;

    /// <summary>
    /// Obtener uno de los portales de aparicion
    /// </summary>
    /// <returns></returns>
    public Transform RandomSpawnPortal()
    {
        return spawnPortals[Random.Range(0, spawnPortals.Count)];
    }
}