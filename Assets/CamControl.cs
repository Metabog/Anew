using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
public class CamControl : MonoBehaviour
{
    // Start is called before the first frame update
    Vector3 cam_velo;
    void Start()
    {
        cam_velo = new Vector3(0.0f, 0.0f, 0.0f);
    }

    const float cam_accel = 0.04f;
    const float max_cam_spd = 4.0f;
    public float camsize_targ = 13.76f;
    float camsize = 13.75f;

    void PP()
    {
        ColorGrading colorGradingLayer = null;

        // somewhere during initializing
        PostProcessVolume volume = gameObject.GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out colorGradingLayer);
        colorGradingLayer.colorFilter.value = Color.Lerp(Color.black, Color.white, Mathf.Clamp(Time.time*0.2f, 0.0f, 1.0f));
    }

    // Update is called once per frame
    void Update()
    {
        //forward vector, but flattened onto the ground plane
        Vector3 forward_perp = new Vector3(transform.forward.x, 0.0f, transform.forward.z).normalized;

        camsize_targ += -Input.mouseScrollDelta.y;
        camsize_targ = Mathf.Clamp(camsize_targ, 5.0f, 42.0f);
        
        camsize = Mathf.Lerp(camsize, camsize_targ, 0.25f);

        GetComponent<Camera>().orthographicSize = camsize;

        float zoomboost = 1.0f + (camsize_targ - 5.0f) / 37.0f;

        //update audio listener so it vaguely relates to the zoom, so we can zoom in on the battles and have sound attenuate/etc
        GameObject.Find("AudioListener").transform.position = transform.position + transform.forward * (1.0f - (zoomboost-1.0f)) * 39.0f;


        if (Input.GetKey(KeyCode.D))
            cam_velo += transform.right * cam_accel * zoomboost;
        if (Input.GetKey(KeyCode.A))
            cam_velo -= transform.right * cam_accel * zoomboost; // new Vector3(-cam_accel, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.W))
            cam_velo += forward_perp * cam_accel * zoomboost;// new Vector3(0.0f, 0.0f , cam_accel);
        if (Input.GetKey(KeyCode.S))
            cam_velo -= forward_perp * cam_accel * zoomboost;//new Vector3(0.0f, 0.0f, -cam_accel);

        cam_velo.x = Mathf.Clamp(cam_velo.x, -max_cam_spd, max_cam_spd);
        cam_velo.y = Mathf.Clamp(cam_velo.y, -max_cam_spd, max_cam_spd);
        cam_velo.z = Mathf.Clamp(cam_velo.z, -max_cam_spd, max_cam_spd);

        transform.position += cam_velo;

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -1000.0f, 1000.0f), transform.position.y, Mathf.Clamp(transform.position.z, -1000.0f, 1000.0f));

        float x = transform.position.x;
        float z = transform.position.z;
        x = Mathf.Clamp(x, -100.0f, 100.0f);
        z = Mathf.Clamp(z, -100.0f, 100.0f);
        transform.position = new Vector3(x, transform.position.y, z);

        cam_velo *= 0.9f;

        PP();
    }
}
