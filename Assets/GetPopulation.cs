using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetPopulation : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject parent;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Population pop = parent.GetComponent<Population>();
        GetComponent<TMPro.TextMeshPro>().text = "Lifesigns: " + pop.population;
    }
}
