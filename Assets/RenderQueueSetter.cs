using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderQueueSetter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<ParticleSystem>().GetComponent<Renderer>().material.renderQueue = 3000;
    }
}
