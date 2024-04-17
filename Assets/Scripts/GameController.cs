using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public MyThirdPersonController player;
    public Ring currentRing;
    public Transform startPosition;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q) ){
            player.EnableController(false);
            player.transform.position = startPosition.position;
            player.EnableController(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
