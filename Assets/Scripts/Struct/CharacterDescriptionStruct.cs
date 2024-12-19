using System;
using UnityEngine;

[Serializable]
public struct CharacterDescriptionStruct
{
    public string lang;
    public string description;

    public CharacterDescriptionStruct(string lang, string description)
    {
        this.lang = lang;
        this.description = description;
    }
}