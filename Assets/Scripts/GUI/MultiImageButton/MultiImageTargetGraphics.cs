using UnityEngine;
using UnityEngine.UI;

public class MultiImageTargetGraphics : MonoBehaviour
{
    [SerializeField] private Transform graphicsContainer;
    private Graphic[] targetGraphics;
    
    public Graphic[] GetTargetGraphics() {
        targetGraphics = graphicsContainer.GetComponentsInChildren<Graphic>();
        return targetGraphics;
    }
}