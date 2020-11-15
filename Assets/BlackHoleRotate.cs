using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleRotate : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    float speed = 0.1f;
    float norm_scale = 0.0f;
    void Start()
    {
        norm_scale = transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {

        float scale = Time.time / 10.0f;
        scale = Mathf.Clamp(scale, 0.0f, 1.0f);

        transform.localScale = new Vector3(scale * norm_scale, scale * norm_scale, scale * norm_scale);
        transform.Rotate(0.0f, 0.0f, -speed);
    }
}
