using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipyardRotate : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject difficulty_dropdown;
    void Start()
    {
        
    }

    public int ships_spawned;
    float time_last_spawned_ship = -10.0f;

    // Update is called once per frame
    void Update()
    {
        float spawn_time = 35.0f;
        int max_ships = 3;

        if(difficulty_dropdown.GetComponent<TMPro.TMP_Dropdown>().value == 0)
        {
            spawn_time = 60.0f;
            max_ships = 1;
        }

        if (difficulty_dropdown.GetComponent<TMPro.TMP_Dropdown>().value == 1)
        {
            spawn_time = 45.0f;
            max_ships = 3;
        }

        if (difficulty_dropdown.GetComponent<TMPro.TMP_Dropdown>().value == 2)
        {
            spawn_time = 35.0f;
            max_ships = 4;
        }

        if (Time.time - time_last_spawned_ship > spawn_time && ships_spawned < max_ships)
        {
            time_last_spawned_ship = Time.time;
            Debug.Log("Spawned");
            GameObject ship = Instantiate(Resources.Load("Cruiser"), transform.position, Quaternion.identity) as GameObject;
            ship.GetComponent<HumanShip>().source_shipyard = this.gameObject;
            ships_spawned++;
        }
    }
}
