using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSound : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioClip[] explosion_sfx;
    float time_started;
    void Start()
    {
        GetComponent<AudioSource>().PlayOneShot(explosion_sfx[(int)Random.Range(0, explosion_sfx.Length)]);
        time_started = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - time_started > 2.0f)
            Destroy(this);
    }
}
