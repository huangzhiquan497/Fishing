using UnityEngine;
using System.Collections;

public class mobileMain : MonoBehaviour {
    public enum State
    {
        Start,
        Fishing,
        Pause
    }
    private State mState;
	// Use this for initialization
	void Start () {
        mState = State.Start;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
