using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTest : MonoBehaviour
{
    private string frameInput;

    // Use this for initialization
    void Start()
    { }

    // Update is called once per frame
    void OnGUI()
    {
        if (Event.current.isKey)
        {
            print("Detected key code: " + Event.current.keyCode);
        }
        else if (Event.current.isMouse)
        {
            print("Detected mouse: " + Event.current.button);
        }
    }
}
