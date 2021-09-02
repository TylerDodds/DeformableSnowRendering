// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

using UnityEngine;

namespace SnowRendering
{

    /// <summary>
    /// Produces a sequence of values in [0,1] that will uniformly cover that range.
    /// </summary>
    public class LowDiscrepancySequence
    {
        //We use a Halton sequence.

        private int _sequenceIndex = 0;
        public int SequenceBase1 = 3;
        public int SequenceBase2 = 5;

        public LowDiscrepancySequence()
        {

        }

        public LowDiscrepancySequence(int startIndex, int sequenceBase1, int sequenceBase2)
        {
            _sequenceIndex = startIndex;
            SequenceBase1 = sequenceBase1;
            SequenceBase2 = sequenceBase2;
        }

        public float GetNextValue()
        {
            float result = GetHaltonValue(_sequenceIndex, SequenceBase1);
            _sequenceIndex++;
            return result;
        }

        public Vector2 GetNextValues2()
        {
            float result1 = GetHaltonValue(_sequenceIndex, SequenceBase1);
            float result2 = GetHaltonValue(_sequenceIndex, SequenceBase2);
            _sequenceIndex++;
            return new Vector2(result1, result2);
        }

        /// <summary>
        /// Gets a value at a given index of the sequence, with the given base value.
        /// </summary>
        /// <param name="index">Index along the sequence.</param>
        /// <param name="sequenceBase">Base value for Halton sequence.</param>
        /// <returns></returns>
        private static float GetHaltonValue(int index, int sequenceBase)
        {
            float result = 0f;
            float fraction = 1f / (float)sequenceBase;
            while (index > 0)
            {
                result += (float)(index % sequenceBase) * fraction;

                index /= sequenceBase;
                fraction /= (float)sequenceBase;
            }
            return result;
        }
    }
}