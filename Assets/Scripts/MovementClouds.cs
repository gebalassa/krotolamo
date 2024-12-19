using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementClouds : MonoBehaviour
{

    [Header("Setup")]
    [SerializeField] [Range(-5f, 5f)] float speed = -0.1f;
    float lastSpeed = -0.1f;

    Material backgroundMaterial;
    Vector2 offset;

    // Start is called before the first frame update
    void Start()
    {
        backgroundMaterial = GetComponent<Renderer>().material;
        offset = new Vector2(speed, 0);
        lastSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if(lastSpeed != speed)
        {
            offset = new Vector2(speed, 0);
            lastSpeed = speed;
        }

        backgroundMaterial.mainTextureOffset += offset * Time.deltaTime;

    }
}
