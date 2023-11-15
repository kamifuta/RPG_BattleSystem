using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUtil
{
    public static class Calculator
    {
        public static float CalcNormalizedValue(int min, int max, int value)
        {
            float returnValue = (float)(value - min) / (max - min);
            return returnValue;
        }
    }
}

