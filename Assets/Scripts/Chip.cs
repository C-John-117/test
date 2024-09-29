using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardTemplate
{
    public class Chip : MonoBehaviour
    {
        public enum Color
        {
            White,
            Red,
            Green,
            Blue,
            Black
        }

        int value;
        Sprite sprite;
        Color color;

        public Color GetColor()
        {
            return color;
        }
    }
}
