using UnityEngine;
using System.Collections;

public class DownFire : MonoBehaviour 
{
    public int FireLevel;
	// Use this for initialization
	void Start ()
    {
       // MobileInterface.SetPlayerFireScore(10);
	}
    void OnMouseDown()
    {
        FireLevel = MobileInterface.GetPlayerFireScore();
        if (FireLevel > 100)
            FireLevel -= 100;
        else if (FireLevel > 10)
            FireLevel -= 10;
        else if (FireLevel >=0)
            FireLevel -= 1;
        if (FireLevel == 0)
            FireLevel = 1000;
        MobileInterface.SetPlayerFireScore(FireLevel);
    }
    void OnMouseUp()
    {
        
    }
	// Update is called once per frame
	void Update ()
    {
	    
	}
}
