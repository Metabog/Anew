using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 explode_target;
    // Start is called before the first frame update
    public AudioClip[] projectile_sfx;

    void Start()
    {
        timestarted = Time.time;

    }
    float timestarted = 0.0f;
    bool played_Sfx = false;
    // Update is called once per frame
    void Update()
    {

        if (!played_Sfx)
        {
            played_Sfx = true;
            GetComponent<AudioSource>().PlayOneShot(projectile_sfx[(int)Random.Range(0, projectile_sfx.Length)]);
        }

        transform.position += transform.forward * 4.0f * Time.deltaTime;
        bool killit = false;
        if (Time.time - timestarted > 6.0f)
        {
            killit = true;
        }

        if ((explode_target - transform.position).magnitude < 0.1f || killit)
        {
            GameObject exp = Instantiate(Resources.Load("Explosion"), transform.position, Quaternion.identity) as GameObject;
            exp.GetComponent<Explosion>().explosion_type = 0;
            Destroy(this.gameObject);
        }
    }
}
