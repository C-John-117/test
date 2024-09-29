using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardTemplate
{
    public class ChipStack
    {
        int quantity;
        const int MAX_STACK = 10;

        public ChipStack()
        {
            quantity = 0;
        }

        public float GetStackCount()
        {
            return (float)quantity / (float)MAX_STACK;
        }
    }
}

