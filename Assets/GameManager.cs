using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject victory_Screen;
    public GameObject victory_time;
    public int difficulty = 1;
    void Start()
    {
        victory_Screen.active = false;
    }

    public void Restart()
    {
        SceneManager.LoadScene("SampleScene");
    }

    float time_at_End;
    bool stored_Time = false;
    // Update is called once per frame
    void Update()
    {
        GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
        GameObject[] ships = GameObject.FindGameObjectsWithTag("Cruiser");

        if (planets.Length + ships.Length == 0)
        {
            Debug.Log("GAME OVER!");
            victory_Screen.active = true;

            if (!stored_Time)
            {
                time_at_End = Time.time;
                stored_Time = true;
            }

            string minutes = Mathf.Floor(time_at_End / 60).ToString("00");
            string seconds = Mathf.Floor(time_at_End % 60).ToString("00");
            victory_time.GetComponent<TextMeshProUGUI>().text = "incursion time: " + minutes + "m:" + seconds + "s";
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
