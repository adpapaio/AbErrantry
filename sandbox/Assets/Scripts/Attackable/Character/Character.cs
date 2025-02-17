using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character2D
{
    public class Character : MonoBehaviour
    {
        public enum Types //enumeration of character types
        {
            Knight,
            MaleAttacker,
            FemaleAttacker,
            Goblin,
            Skeleton,
            Slime,
            Rat,
            Crate,
            Dummy,
            ExplodingCrate,
            DestroyableTile,
            Bear,
            Ogre,
            Golem,
            Wraith,
            Robot,
            Horseman,
        }

        public Types type; //the type of character

        public CharacterFields fields;

        public void Start()
        {
            fields = GameData.data.characterData.characterDictionary[type.ToString()];
        }
    }
}
