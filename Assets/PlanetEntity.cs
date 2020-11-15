using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetEntity : MonoBehaviour
{
    // Start is called before the first frame update
    public float taking_damage;
    public GameObject[] explosion_spawn_points;
    public GameObject centerobj;
    void Start()
    {
        taking_damage = 0.0f;
    }
    float last_decorative_explo_time = 0.0f;
    // Update is called once per frame
    void Update()
    {
        if (taking_damage != 0.0f)
        {
            if (Time.time - last_decorative_explo_time > 0.25f)
            {
                GetComponent<Population>().population -= (int)taking_damage;
                taking_damage = 0.0f;

                if (explosion_spawn_points.Length>0)
                {
                    int rand = Random.Range(0, 5);
                    GameObject explo_spawn = explosion_spawn_points[rand];
                    GameObject empty = Instantiate(Resources.Load("EmptyBoye"), explo_spawn.transform.position, Quaternion.identity) as GameObject;
                    empty.transform.localScale *= 0.5f;

                    GameObject ex = Instantiate(Resources.Load("DecorativeExplosion Variant"), explo_spawn.transform.position, Quaternion.identity) as GameObject;
                    ex.transform.parent = empty.transform;
                    last_decorative_explo_time = Time.time;
                }

                //when we subtract population, add it to the score, and spawn rate of spawners
                GameObject[] spawn_points = GameObject.FindGameObjectsWithTag("SpawnPoint");
                foreach (GameObject sp in spawn_points)
                {
                    sp.GetComponent<SpawnPoint>().torrentialness -= taking_damage * 0.001f;
                }

                if (GetComponent<Population>().population <= 0)
                {

                    GameObject empty = Instantiate(Resources.Load("EmptyBoye"), transform.position, Quaternion.identity) as GameObject;
                    empty.transform.localScale *= 3.3f;

                    GameObject exp = Instantiate(Resources.Load("Explosion"), transform.position, Quaternion.identity) as GameObject;
                    exp.transform.parent = empty.transform;
                    Destroy(this.gameObject);
                }
            }

        }
    }
}
