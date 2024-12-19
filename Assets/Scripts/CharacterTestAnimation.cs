using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTestAnimation : MonoBehaviour
{
    int inOrder = 0;
    public void OnMouseDown()
    {
        InGameStates current = GetComponent<CharacterInGameController>().GetState() + 1;
        if ( (int) current >= Enum.GetNames(typeof(InGameStates)).Length)
        {
            current = InGameStates.Stand;
        }
        GetComponent<CharacterInGameController>().ChangeState((InGameStates)inOrder++);
    }

}
