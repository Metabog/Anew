using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Particle
{
    public Vector2 pos;
    public Vector2 velocity;
    public int active;
    public float time;
    public float life;
}

struct Ship
{
    public Vector2 pos;
    public Vector2 aim_pos;
    public float mindist;

    public float enemy_mindist;
    public Vector2 enemy_aimpos;
    public float dealt_damage;
}

struct Planet
{
    public Vector2 pos;
    public float size;
    public float dealt_damage;
}

struct ExplosionStruct
{
    public Vector2 pos;
    public int type;
}

public class TheGrid : MonoBehaviour
{
    public ComputeShader shader;
    public ComputeShader clear_tex_shader;
    public ComputeShader quant_grid_cleaner;
    public ComputeShader vector_field_editor;

    int frame;

    [SerializeField]
    RenderTexture particle_tex;
    [SerializeField]
    Texture2D destroyer_sprite;
    [SerializeField]
    public RenderTexture quantized_unit_grid;
    [SerializeField]
    public RenderTexture vector_field;

    const float spawn_particles_per_second = 100.0f;

    float time_last_spawned = 0.0f;

    Vector2 last_edit_vec_pos;
    // Start is called before the first frame update
    Particle[] data;
    bool ready = false;
    const int maxparticles = 200000;

    float wormhole_Start_time = 10.0f;
    float wormshole_warmup_time = 10.0f;

    void Start()
    {
        last_edit_vec_pos = new Vector2(0.0f, 0.0f);
        particle_tex = new RenderTexture(4096, 4096, 24);
        particle_tex.enableRandomWrite = true;
        particle_tex.Create();
        //particle_tex.filterMode = FilterMode.Point;

        //a low res grid, used for targeting by the turrets/etc
        quantized_unit_grid = new RenderTexture(64, 64, 24);
        quantized_unit_grid.enableRandomWrite = true;
        quantized_unit_grid.Create();
        //quantized_unit_grid.filterMode = FilterMode.Point;

        vector_field = new RenderTexture(128, 128, 24, RenderTextureFormat.ARGBHalf);
        vector_field.enableRandomWrite = true;
        vector_field.Create();

        data = new Particle[maxparticles];
        for (int i =0; i< maxparticles; i++)
        {
            data[i].pos = new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            data[i].velocity = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f))*0.00001f;
            data[i].active = 0;
            data[i].life = 1.0f;
            data[i].time = 0.0f;
        }
        ready = true;
    }

    const float spawn_poll_time = 0.1f;
    float last_polled = 0.0f;
    float gushiness = 0.0f;
    void SpawnParticle(Vector3 spawnpos)
    {

        if (Time.time < wormhole_Start_time)
            return;
        gushiness += 0.05f * Time.deltaTime;

        gushiness = Mathf.Clamp(gushiness, 0.0f, 1.0f);

        if (Time.time - last_polled > spawn_poll_time)
        {
            spawnpos += new Vector3(1.0f, 0.0f, 1.0f);
            spawnpos /= 2.0f;
            spawnpos.x = 1.0f - spawnpos.x;
            spawnpos.z = 1.0f - spawnpos.z;

            int num_particles_to_spawn = (int)((Time.time - last_polled) * spawn_particles_per_second * gushiness);
            for (int p = 0; p < num_particles_to_spawn; p++)
            {
                //this is super inefficient sorry gamejam lol
                for (int i = 0; i < maxparticles; i++)
                {
                    if (data[i].active == 0)
                    {
                        data[i].active = 1;
                        data[i].pos = new Vector2(spawnpos.x, spawnpos.z) + new Vector2(Random.Range(-0.03f, 0.03f), Random.Range(-0.03f, 0.03f));

                        data[i].velocity = new Vector2(Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f)) * 0.0001f;
                        data[i].life = 1.0f;
                        data[i].time = 0.0f;
                        break;
                    }
                }
            }

            last_polled = Time.time;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!ready)
            return;

        GameObject[] spawners = GameObject.FindGameObjectsWithTag("SpawnPoint");
        foreach(GameObject sp in spawners)
        {
            Vector3 spawnpos = sp.transform.position / 80.0f; //weird huh
            SpawnPoint spoint = sp.GetComponent<SpawnPoint>();

            if (Time.time - spoint.last_spawn_time > spoint.torrentialness)
            {
                spoint.last_spawn_time = Time.time;
                SpawnParticle(spawnpos);
            }

        }


        //edit the quant grid to set the vector field state
       // if (Input.GetMouseButton(0))
        {
            int kernelHandle = vector_field_editor.FindKernel("CSMain");
            vector_field_editor.SetTexture(kernelHandle, "vector_field", vector_field);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int state = 0;
            if (Input.GetMouseButton(0))
                state = 1;
            if (Input.GetMouseButton(1))
                state = 2;
            vector_field_editor.SetInt("editing_state", state);

            if (GetComponent<MeshCollider>().Raycast(ray, out hit, 10000.0f))
            {
                //raycast to point on the ground plane to edit at
                Vector3 wpos = hit.point;
                wpos /= 80.0f;
                wpos += new Vector3(1.0f, 0.0f, 1.0f);
                wpos /= 2.0f;
                wpos.x = 1.0f - wpos.x;
                wpos.z = 1.0f - wpos.z;

                Vector2 new_pos = new Vector2(wpos.x, wpos.z);

                Vector2 direction = (new_pos - last_edit_vec_pos);
                direction *= 64.0f;

                //clamp vector length
                float mag = direction.magnitude;
                if (mag > 1.0f)
                    mag = 1.0f;
                //todo - I wanted this to have some amount of user variation, but it sucks atm
                //so I Am making all vector unit vectors
                direction = direction.normalized;

                last_edit_vec_pos = new Vector2(wpos.x, wpos.z);

                vector_field_editor.SetVector("edit_pos", new_pos);
                vector_field_editor.SetVector("edit_vector", direction);

                vector_field_editor.SetInt("frame", frame);

            }

            vector_field_editor.Dispatch(kernelHandle, 128 / 8, 128 / 8, 1);
        }

        //first clear up the particle texture
        {
            int kernelHandle = clear_tex_shader.FindKernel("CSMain");
            clear_tex_shader.SetTexture(kernelHandle, "particle_tex", particle_tex);
            clear_tex_shader.Dispatch(kernelHandle, 4096 / 8, 4096 / 8, 1);
        }

        //clean the quant grid
        {
            int kernelHandle = quant_grid_cleaner.FindKernel("CSMain");
            quant_grid_cleaner.SetTexture(kernelHandle, "particle_tex", quantized_unit_grid);
            quant_grid_cleaner.Dispatch(kernelHandle, 64 / 8, 64 / 8, 1);
        }

        GameObject[] ships = GameObject.FindGameObjectsWithTag("Cruiser");
        Ship[] shipdata = new Ship[512];

        for(int i =0; i<ships.Length; i++)
        {
            shipdata[i].pos = new Vector2(ships[i].transform.position.x / 80.0f, ships[i].transform.position.z / 80.0f);
            //convert ship positions into the normalized shader space...
            shipdata[i].pos += new Vector2(1.0f, 1.0f);
            shipdata[i].pos /= 2.0f;
            shipdata[i].pos.x = 1.0f - shipdata[i].pos.x;
            shipdata[i].pos.y = 1.0f - shipdata[i].pos.y;
            shipdata[i].aim_pos = new Vector2(-1.0f,-1.0f);
            shipdata[i].mindist = 10000.0f;
            shipdata[i].enemy_mindist = 10000.0f;
            shipdata[i].enemy_aimpos = new Vector2(-1.0f, -1.0f);
            shipdata[i].dealt_damage = 0.0f;
        }

        GameObject[] explosions = GameObject.FindGameObjectsWithTag("Explosion");
        ExplosionStruct[] explosiondata = new ExplosionStruct[512]; //not zero, just make room

        for (int i = 0; i < explosions.Length; i++)
        {
            if (i >= explosiondata.Length) //don't overrun the rwbuffer
                break;

            explosiondata[i].pos = new Vector2(explosions[i].transform.position.x / 80.0f, explosions[i].transform.position.z / 80.0f);
            explosiondata[i].pos += new Vector2(1.0f, 1.0f);
            explosiondata[i].pos /= 2.0f;
            explosiondata[i].pos.x = 1.0f - explosiondata[i].pos.x;
            explosiondata[i].pos.y = 1.0f - explosiondata[i].pos.y;

            explosiondata[i].type = explosions[i].GetComponent<Explosion>().explosion_type;
        }

        GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
        Planet[] planetdata = new Planet[16];
        for(int i = 0; i<planets.Length; i++)
        {
            planetdata[i].pos = new Vector2(planets[i].transform.position.x / 80.0f, planets[i].transform.position.z / 80.0f);
            
            //use a specified center for the planet object if set
            PlanetEntity pe = planets[i].GetComponent<PlanetEntity>();
            if (pe.centerobj)
                planetdata[i].pos = new Vector2(planets[i].GetComponent<PlanetEntity>().centerobj.transform.position.x / 80.0f, planets[i].GetComponent<PlanetEntity>().centerobj.transform.position.z / 80.0f);

            planetdata[i].pos += new Vector2(1.0f, 1.0f);
            planetdata[i].pos /= 2.0f;
            planetdata[i].pos.x = 1.0f - planetdata[i].pos.x;
            planetdata[i].pos.y = 1.0f - planetdata[i].pos.y;
            planetdata[i].dealt_damage = 0.0f;  
        }

        ComputeBuffer buffer = new ComputeBuffer(data.Length, 28);
        ComputeBuffer ships_buffer = new ComputeBuffer(shipdata.Length, 36);
        ComputeBuffer explosion_buffer = new ComputeBuffer(explosiondata.Length, 12);
        ComputeBuffer planet_buffer = new ComputeBuffer(planetdata.Length, 16);

        buffer.SetData(data);
        ships_buffer.SetData(shipdata);
        explosion_buffer.SetData(explosiondata);
        planet_buffer.SetData(planetdata);

        int kernel = shader.FindKernel("CSMain");
        shader.SetBuffer(kernel, "dataBuffer", buffer);
        shader.SetBuffer(kernel, "shipBuffer", ships_buffer);
        shader.SetBuffer(kernel, "explosionBuffer", explosion_buffer);
        shader.SetBuffer(kernel, "planetBuffer", planet_buffer);

        shader.SetInt("numShips", ships.Length);
        shader.SetInt("numExplosions", explosions.Length);
        shader.SetInt("numPlanets", planets.Length);

        shader.SetFloat("deltatime", Time.deltaTime);

        shader.SetTexture(kernel, "particle_tex", particle_tex);
        shader.SetTexture(kernel, "quantized_unit_grid", quantized_unit_grid);
        shader.SetTexture(kernel, "vector_field", vector_field);

        shader.SetTexture(kernel, "destroyer_sprite", destroyer_sprite);
        shader.Dispatch(kernel, 512, 1, 1);
        buffer.GetData(data);
        ships_buffer.GetData(shipdata);
        planet_buffer.GetData(planetdata);

        for (int i = 0; i < planets.Length; i++)
        {
            if(planetdata[i].dealt_damage != 0.0f)
            {
                Debug.Log("planet hit! " + planetdata[i].dealt_damage);
                //THIS KEEPS TRIGGERING AN EXCEPTION!
                PlanetEntity pe = planets[i].GetComponent<PlanetEntity>();
                planets[i].GetComponent<PlanetEntity>().taking_damage += planetdata[i].dealt_damage;// -= 1;
            }
        }

            for (int i = 0; i < ships.Length; i++)
        {
            if (shipdata[i].dealt_damage != 0.0f)
                ships[i].GetComponent<HumanShip>().taking_damage += shipdata[i].dealt_damage;// -= 1;

            if (shipdata[i].mindist != 10000.0f)
                ships[i].GetComponent<HumanShip>().valid_Aim = true;
            else
                ships[i].GetComponent<HumanShip>().valid_Aim = false;

            if (shipdata[i].enemy_mindist != 10000.0f)
                ships[i].GetComponent<HumanShip>().valid_Aim_Enemy = true;
            else
                ships[i].GetComponent<HumanShip>().valid_Aim_Enemy = false;

            Vector3 aim_pos = new Vector3(shipdata[i].aim_pos.x, 0.0f, shipdata[i].aim_pos.y);
            aim_pos.x = 1.0f - aim_pos.x;
            aim_pos.z = 1.0f - aim_pos.z;
            aim_pos *= 2.0f;
            aim_pos -= new Vector3(1.0f, 0.0f, 1.0f);
            aim_pos *= 80.0f;

            ships[i].GetComponent<HumanShip>().aim_pos = aim_pos;

            Vector3 enemy_aim_pos = new Vector3(shipdata[i].enemy_aimpos.x, 0.0f, shipdata[i].enemy_aimpos.y);
            enemy_aim_pos.x = 1.0f - enemy_aim_pos.x;
            enemy_aim_pos.z = 1.0f - enemy_aim_pos.z;
            enemy_aim_pos *= 2.0f;
            enemy_aim_pos -= new Vector3(1.0f, 0.0f, 1.0f);
            enemy_aim_pos *= 80.0f;

            ships[i].GetComponent<HumanShip>().enemy_aimpos = enemy_aim_pos;
        }

        buffer.Dispose();
        ships_buffer.Dispose();
        explosion_buffer.Dispose();
        planet_buffer.Dispose();

        GetComponent<Renderer>().material.SetTexture("_ParticleTex", particle_tex);
        GetComponent<Renderer>().material.SetTexture("_QuantizedGrid", quantized_unit_grid);
        GetComponent<Renderer>().material.SetTexture("_VectorField", vector_field);
        frame++;

    }
}
