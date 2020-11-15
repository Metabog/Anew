using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    // Start is called before the first frame update
    float time_started = 0.0f;
    public int explosion_type = 0;
    public AudioClip[] explosion_sfx;
    bool played_Sfx = false;
    void Start()
    {
        time_started = Time.time;
    }
    float scale = 0.001f;
    // Update is called once per frame
    void Update()
    {
        /*
         float anim_Time = Time.time - time_started;
        if(anim_Time < 0.5f)
        {
            scale = anim_Time / 0.5f;
        }
        else
        {
            scale = 1.0f  / (anim_Time - 0.5f + 1.0f);
        }
        transform.localScale = new Vector3(scale, scale, scale)*0.1f;
        */
        if (!played_Sfx)
        {
            played_Sfx = true;
            if (explosion_type == 0 && tag != "DecorativeExplosion")
                Instantiate(Resources.Load("ExplosionSound"), transform.position, Quaternion.identity);
        }

        if (Time.time - time_started > (explosion_type == 0 ? 0.5f : 0.5f))
        {
            if(transform.parent != null)
            if(transform.parent.name.Contains("EmptyBoye"))
                Destroy(transform.parent.gameObject);//usually we're in an EmptyBoye, so destroy it so it doesn't leak
            Destroy(this.gameObject);
        }
    }
}
