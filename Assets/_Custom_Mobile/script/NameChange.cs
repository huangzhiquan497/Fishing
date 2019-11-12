﻿using UnityEngine;
using System.Collections;

public class NameChange : MonoBehaviour
{
    public static TouchScreenKeyboard keybord;
    public static string message;
    public static bool bPress=false;
	// Use this for initialization
	void Start ()
    {
	
	}
    public static string getName()
    {
        if(bPress)
            return message;
        return "";
    }
    void OnMouseDown()
    {
        bPress = true;
        keybord = TouchScreenKeyboard.Open(message, TouchScreenKeyboardType.Default);
    }
	// Update is called once per frame
	void Update ()
    {
        if (keybord != null)
        {
            //Debug.Log(keybord.text);
            message = keybord.text;
        }
	}
}
