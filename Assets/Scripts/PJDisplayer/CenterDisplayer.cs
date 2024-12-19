using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterDisplayer : MonoBehaviour
{
    [SerializeField] PJDisplayer pJDisplayer;

    public void OnMouseDown()
    {
        pJDisplayer.Previous();
    }

    // Fix para recibir esta funcion desde animacion
    public void HideHandAttack() { }

}
