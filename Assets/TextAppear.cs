using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TextAppear : MonoBehaviour
{
    // Start is called before the first frame update
    public string txt;
    void Start()
    {
        txt = GetComponent<TextMeshProUGUI>().text;
    }

    // Update is called once per frame
    int letters = 0;
    float time_last_letter = 0.0f;
    void Update()
    {
        if(Time.time - time_last_letter > 0.05f)
        {
            letters++;
            time_last_letter = Time.time;
        }
        letters = Mathf.Clamp(letters, 0, txt.Length);
        GetComponent<TextMeshProUGUI>().text = txt.Substring(0, letters);
    }
}
