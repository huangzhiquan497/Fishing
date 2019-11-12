using UnityEngine;
using System.Collections;

public class UpFire : MonoBehaviour
{
    public int FireLevel;
	// Use this for initialization
	void Start ()
    {
	}
    void OnMouseDown()
    {
        FireLevel = MobileInterface.GetPlayerFireScore();
        if (FireLevel < 10)
            FireLevel += 1;
        else if (FireLevel < 100)
            FireLevel += 10;
        else if (FireLevel <= 1000)
            FireLevel += 100;
        if (FireLevel > 1000)
            FireLevel = 1;
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
