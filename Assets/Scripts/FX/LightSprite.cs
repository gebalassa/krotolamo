using System.Collections;
using UnityEngine;

public class LightSprite : MonoBehaviour
{
    [SerializeField] float speed = -0.1f;

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + speed);
    }
}