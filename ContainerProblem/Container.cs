using System;
using System.Collections.Generic;

namespace ContainerProblem
{
    class Container
    {
        private List<float> weights;
        private float size;

        public float Sum { get; private set; }
        public float Available => size - Sum;

        public Container(float size)
        {
            this.size = size;
            weights = new List<float>();
            Sum = 0;
        }

        public bool ReplaceWeight(ref float old_weight, float new_weight)
        {
            int index = FindAndCorrectValue(weights, ref old_weight, new_weight);
            if (index < 0)
                return false;

            weights[index] = new_weight;
            Sum += new_weight - old_weight;
            return true;
        }

        public int FindAndCorrectValue(List<float> array, ref float value, float limit)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] > value - Program.accuracy && array[i] < limit)
                {
                    value = array[i];
                    return i;
                }
            }
            return -1;
        }

        public float GetLast()
        {
            float last = weights[weights.Count - 1];
            weights.RemoveAt(weights.Count - 1);
            Sum -= last;
            return last;
        }

        public bool Add(float weight)
        {
            if (weight + Sum - size > Program.accuracy)
                return false;

            weights.Add(weight);
            Sum += weight;
            return true;
        }

        public void DisplayResult()
        {
            foreach (float weight in weights)
                Console.Write(weight + " ");
            Console.Write("\t\t| " + Sum);
            Console.Write("\n");
        }
    }
}
