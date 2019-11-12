using UnityEngine;
using System.Collections;

public class RandomOddsNum : MonoBehaviour {
    public int MinOdds = 2;
    public int MaxOdds = 101;
	// Use this for initialization
	void Start () {
        Fish f = GetComponent<Fish>();
        f.Odds = Random.Range(MinOdds, MaxOdds);
	}

}
