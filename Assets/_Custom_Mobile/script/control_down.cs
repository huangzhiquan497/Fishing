using UnityEngine;
using System.Collections;

public class control_down : MonoBehaviour
{
    private bool ON = true;
    public Move_UP[] move;
    public Chain_zoom[] zoom;
    private float IntervalTime = 0.7f;
    private float ClickTime = 0;
    private bool record = true; 

	// Use this for initialization
	void Start ()
    {
	
	}

    void OnMouseDown()
    { 
        if (record == false)
        {
            if (ClickTime + IntervalTime > Time.time)
            {
                return;
            }
            record = true;
        }
        if (ON)
        {
            record = false ;
            ClickTime = Time.time;
            //Move_UP.Singlton.Play();
            for (int i = 0; i < 4; i++)
            {
                move[i].Play();
            }
            for (int i = 0; i < 4; i++)
            {
                zoom[i].Play();
            }
            ON = false;
        }
        else
        {
            record = false;
            ClickTime = Time.time;
            for (int i = 0; i < 4; i++)
            {
                move[i].rePlay();
            }
            for (int i = 0; i < 4; i++)
            {
                zoom[i].rePlay();
            }
            ON = true;
        }
       // Move_UP.
    }
    // Update is called once per frame
    void Update() 
    {
	
	}
}
