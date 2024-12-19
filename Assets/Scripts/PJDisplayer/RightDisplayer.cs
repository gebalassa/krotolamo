using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightDisplayer : MonoBehaviour
{
    [SerializeField] PJDisplayer pJDisplayer;

    public void OnMouseDown()
    {
        pJDisplayer.Next();
    }

    // Fix para recibir esta funcion desde animacion
    public void HideHandAttack() { }
}
