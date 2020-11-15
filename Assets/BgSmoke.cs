using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgSmoke : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Renderer>().material.SetFloat("_Offset", transform.position.y);
    }
}
