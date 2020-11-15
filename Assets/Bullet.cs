using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public Vector3 explode_target;
    // Start is called before the first frame update

    void Start()
    {
        timestarted = Time.time;
    }
    public AudioClip[] laser_sfx;

    bool played_Sfx = false;
    float timestarted = 0.0f;
    // Update is called once per frame
    void Update()
    {
        if (!played_Sfx)
        {
            played_Sfx = true;
            GetComponent<AudioSource>().PlayOneShot(laser_sfx[(int)Random.Range(0, laser_sfx.Length)]);
        }

        bool killit = false;

        if(Time.time - timestarted>2.0f)
        {
            killit = true;
        }

        transform.position += transform.forward * 8.0f * Time.deltaTime;

        if ((explode_target - transform.position).magnitude < 0.2f || killit)
        {
            GameObject empty = Instantiate(Resources.Load("EmptyBoye"), transform.position, Quaternion.identity) as GameObject;
            empty.transform.localScale *= 0.3f;

            GameObject exp = Instantiate(Resources.Load("Explosion"), transform.position, Quaternion.identity) as GameObject;
            exp.transform.parent = empty.transform;
            exp.GetComponent<Explosion>().explosion_type = 1;
            Destroy(this.gameObject);
        }
    }
}
