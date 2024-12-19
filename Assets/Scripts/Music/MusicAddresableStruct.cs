using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MusicAddresableStruct
{
    public string identifier;
    public float delay;
    public bool fixedOrder;
    public List<string> addresablesKey;
}