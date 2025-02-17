using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public enum Types //enumeration of character types
    {
        BackDoor,
        SideDoor,
        Pickup,
        NPC,
        Chest,
        Sign
    }

    public new string name;
    [System.NonSerialized] public string type;
    [System.NonSerialized] public Types typeOfInteractable;

    protected void Start()
    {
        //set name
        SetType();
    }

    public void SetType()
    {
        switch (typeOfInteractable)
        {
            case Types.BackDoor:
                type = "enter";
                break;
            case Types.SideDoor:
                type = "open";
                break;
            case Types.Pickup:
                type = "pick up";
                break;
            case Types.NPC:
                type = "talk to";
                break;
            case Types.Chest:
                type = "open";
                break;
            case Types.Sign:
                type = "read";
                break;
            default:
                Debug.LogError("Item: " + name + " (named " + gameObject.name + " in hierarchy) " +
                    "has an undefined type. Please set it in the InteractableObject script.");
                break;
        }
    }
}
