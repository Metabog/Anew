using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Cruiser
{
    public Vector2 pos;
    public Vector2 aim_pos;
}

struct Order
{
    public Vector3 target_pos;
    public bool valid;
}

public class HumanShip : MonoBehaviour
{
    // Start is called before the first frame update
    TheGrid grid;
    RenderTexture quantized_grid;
    public Vector3 aim_pos;
    public bool valid_Aim = false;
    public bool valid_Aim_Enemy = false;
    public Vector3 enemy_aimpos;
    int ammo = 5;
    public float speed = 1.0f;
    public float taking_damage = 0.0f;
    public GameObject source_shipyard;
    public GameObject[] explosion_spawn_points;
    public GameObject myname;
    Order current_order;

    public string[] shipnames;

    void Start()
    {
        grid = GameObject.Find("Grid").GetComponent<TheGrid>();
        quantized_grid = grid.quantized_unit_grid;

        myname.GetComponent<TMPro.TextMeshPro>().text = shipnames[Random.Range(0, shipnames.Length)];

        //random move order
        if (Random.value < 0.4f)
        {
            current_order = new Order();
            current_order.valid = true;
            current_order.target_pos = new Vector3(Random.Range(-25.0f, 25.0f), 0.0f, Random.Range(-25.0f, 25.0f));
        }
        else //move to random planet to defend
        {
            GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
            int idx = Random.Range(0, planets.Length);
            current_order = new Order();
            current_order.valid = true;
            current_order.target_pos = new Vector3(planets[idx].transform.position.x, 0.0f, planets[idx].transform.position.z);
        }

    }

    public AudioClip[] missile_launch_sfx;
    public AudioClip[] laser_sfx;

    float last_shot_Time = 0.0f;
    float last_reload_Time = 0.0f;

    float last_bullet_Time = 0.0f;

    void AimShoot()
    {
        //return;
        //return;
        if (valid_Aim)
        {
            //Debug.DrawLine(transform.position, aim_pos, Color.red);
            if(Time.time - last_shot_Time > 1.0f && ammo != 0)
            {
                GameObject projectile = Instantiate(Resources.Load("Projectile"), transform.position, Quaternion.identity) as GameObject;
                projectile.transform.LookAt(aim_pos);
                projectile.GetComponent<Projectile>().explode_target = aim_pos;
                last_shot_Time = Time.time;
                //GetComponent<AudioSource>().Play();
               // GetComponent<AudioSource>().volume = Random.Range(0.1f, 0.3f);
                //GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
                ammo--;
            }
        }

        if (valid_Aim_Enemy)
        {
            Debug.DrawLine(transform.position, enemy_aimpos, Color.red);
            if (Time.time - last_bullet_Time > 0.2f)
            {
                GameObject projectile = Instantiate(Resources.Load("Bullet"), transform.position, Quaternion.identity) as GameObject;
                projectile.transform.LookAt(enemy_aimpos);
                projectile.GetComponent<Bullet>().explode_target = enemy_aimpos;
                last_bullet_Time = Time.time;

            }
        }

        if (Time.time - last_reload_Time > 1.0f)
        {
            last_reload_Time = Time.time;
            ammo++;
        }
        ammo = Mathf.Clamp(ammo, 0, 5);

        //ComputeBuffer buffer = new ComputeBuffer(data.Length, 28);

        /*buffer.SetData(data);
        /*int kernel = shader.FindKernel("CSMain");
        /*shader.SetBuffer(kernel, "dataBuffer", buffer);
        /*shader.SetTexture(kernel, "particle_tex", particle_tex);
        /*shader.SetTexture(kernel, "quantized_unit_grid", quantized_unit_grid);
        /*
        /*shader.SetTexture(kernel, "destroyer_sprite", destroyer_sprite);
        /*shader.Dispatch(kernel, 512, 1, 1);
        /*buffer.GetData(data);
        /*buffer.Dispose();
         * */
    }

    // Update is called once per frame
    float last_decorative_explo_time = 0.0f;
    void Update()
    {
        float perlin = Mathf.PerlinNoise(transform.position.x, transform.position.y);

        if(current_order.valid)
        {
            if ((transform.position - current_order.target_pos).magnitude < 5.0f)
                current_order.valid = false;//order fulfilled!
            transform.LookAt(current_order.target_pos);
        }
        else
        {
            transform.Rotate(0.0f, Mathf.Cos(perlin * Mathf.PI * 2.0f) * 0.1f * speed, 0.0f);
            //perlin random wander
        }

        if(transform.position.x > 75.0f || transform.position.z > 75.0f || transform.position.x < -75.0f || transform.position.z < -75.0f)
        {
            Debug.Log("aiming at centre because out of bounds" + transform.position);
            //out of bounds! set a course for the center
            transform.LookAt(new Vector3(0.0f,0.0f,0.0f));
        }

        transform.position += transform.forward * Time.deltaTime * speed;
        AimShoot();

        if(taking_damage != 0.0f && tag != "Freighter")
        {
            if (Time.time - last_decorative_explo_time > 0.25f)
            {

                GetComponent<Population>().population -= (int)taking_damage;
                taking_damage = 0.0f;

                if (GetComponent<Population>().population <= 0)
                {
                    Instantiate(Resources.Load("Explosion"), transform.position, Quaternion.identity);
                    Destroy(this.gameObject);
                    if(source_shipyard!=null)
                        source_shipyard.GetComponent<ShipyardRotate>().ships_spawned--;
                }

                if (explosion_spawn_points.Length > 0)
                {
                    int rand = Random.Range(0, 5);
                    GameObject explo_spawn = explosion_spawn_points[rand];
                    GameObject empty = Instantiate(Resources.Load("EmptyBoye"), explo_spawn.transform.position, Quaternion.identity) as GameObject;
                    empty.transform.localScale *= 0.3f;

                    GameObject ex = Instantiate(Resources.Load("DecorativeExplosion Variant"), explo_spawn.transform.position, Quaternion.identity) as GameObject;
                    ex.transform.parent = empty.transform;
                    last_decorative_explo_time = Time.time;
                }

                //when we subtract population, add it to the score, and spawn rate of spawners
                GameObject[] spawn_points = GameObject.FindGameObjectsWithTag("SpawnPoint");
                foreach (GameObject sp in spawn_points)
                {
                    sp.GetComponent<SpawnPoint>().torrentialness -= taking_damage*0.001f;
                }
            }

        }

    }

}
