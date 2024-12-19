using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotesPool : MonoBehaviour
{
    [SerializeField] List<Emote> emotes;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Obtencion de un emote en base a su ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Emote GetEmote(int id)
    {
        Emote emote = emotes.Find(e => e.GetIDAsInt() == id);
        return emote;
    }
}