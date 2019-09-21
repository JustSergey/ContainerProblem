using System;
using System.IO;
using System.Globalization;

namespace ContainerProblem
{
    class Program
    {
        public const float size_of_containers = 1.0f;
        public const float accuracy = 0.000001f;
        public const string file_path = "info.txt";
        public const int max_num_of_containers = 1000;
        public const int max_num_of_things = 100000;

        static void Main(string[] args)
        {
            float[][] data = GetInputData(out int num_of_containers);

            if (data is null)
            {
                Console.ReadKey(true);
                return;
            }
            
            for (int i = 0; i < data.Length; i++)
            {
                float[] weights = data[i];
                Container[] first_containers = new Container[weights.Length];
                Container[] second_containers = new Container[weights.Length];
                for (int j = 0; j < weights.Length; j++)
                {
                    first_containers[j] = new Container(size_of_containers);
                    second_containers[j] = new Container(size_of_containers);
                }

                QuickSort(weights, 0, weights.Length - 1);
                FirstDistribution(weights, first_containers);
                ReDistribution(first_containers);
                SecondDistribution(weights, second_containers, 0);
                ReDistribution(second_containers);
                Container[] result_containers = GetResult(first_containers, second_containers);

                DisplayResult(result_containers, num_of_containers);
            }
            Console.ReadKey(true);
        }

        static void FirstDistribution(float[] weights, Container[] containers)
        {
            float average = GetAverage(weights);

            int k = 0;
            while (k < weights.Length && weights[k] > average - accuracy)
            {
                containers[k].Add(weights[k]);
                k++;
            }

            SecondDistribution(weights, containers, k);
        }

        static void SecondDistribution(float[] weights, Container[] containers, int begin_position)
        {
            int k = begin_position, l = 0;
            while (k < weights.Length)
            {
                if (containers[l].Add(weights[k]))
                {
                    k++;
                    l = 0;
                }
                else
                    l++;
            }
        }

        static void ReDistribution(Container[] containers)
        {
            int k = containers.Length - 1;
            while (k >= 0)
            {
                if (containers[k].Sum < accuracy || containers[k].Available < accuracy)
                {
                    k--;
                    continue;
                }

                float last = containers[k].GetLast();
                for (int i = 0; i <= k; i++)
                {
                    if (containers[i].Available < accuracy)
                        continue;

                    float replace = last - containers[i].Available;
                    if (replace < accuracy)
                    {
                        containers[i].Add(last);
                        if (i >= k)
                            k = -1;
                        break;
                    }
                    else
                    {
                        if (containers[i].ReplaceWeight(ref replace, last))
                        {
                            containers[k].Add(replace);
                            break;
                        }
                    }
                }
            }
        }

        static Container[] GetResult(Container[] first_containers, Container[] second_containers)
        {
            int k = 0, l = 0;
            for (int i = 0; i < first_containers.Length; i++)
            {
                if (first_containers[i].Sum > accuracy)
                    k++;
                if (second_containers[i].Sum > accuracy)
                    l++;
            }
            Array.Resize(ref first_containers, k);
            Array.Resize(ref second_containers, l);
            if (k > l)
                return second_containers;
            else
                return first_containers;
        }

        static float GetAverage(float[] array)
        {
            float average = 0;
            for (int i = 0; i < array.Length; i++)
                average += array[i];
            return average / array.Length;
        }

        static void QuickSort(float[] array, int low, int high)
        {
            int center = low + (high - low) / 2;
            int i = low, j = high;
            float mean = array[center];
            bool isEnd = false;
            bool isSorted = true;
            while (!isEnd)
            {
                while (array[i] > mean) i++;
                while (array[j] < mean) j--;
                if (i < j)
                {
                    isSorted = false;
                    float temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                    i++;
                    j--;
                }
                else
                    isEnd = true;
            }
            if (!isSorted)
            {
                if (i - 1 > low) QuickSort(array, low, i - 1);
                if (high > i) QuickSort(array, i, high);
            }
        }

        static void DisplayResult(Container[] containers, int num_of_containers)
        {
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("Результат:");
            float sum = 0;
            for (int i = 0; i < containers.Length; i++)
            {
                containers[i].DisplayResult();
                sum += containers[i].Sum;
            }
            Console.WriteLine("Всего контейнеров: " + containers.Length + " | Сумма масс: " + sum);
            if (num_of_containers < containers.Length)
                Console.WriteLine("Невозможно поместить предметы в " + num_of_containers + " контейнера(ов)");
        }

        static float[][] GetInputData(out int num_of_containers)
        {
            bool isCorrectData = false;
            int method = 0;
            while (!isCorrectData)
            {
                Console.WriteLine("Введите способ ввода данных: (1 - случайная генерация, 2 - ручной ввод, 3 - из файла " + file_path + ")");
                string s = Console.ReadLine();
                if (!int.TryParse(s, out method) || method < 1 || method > 3)
                    Console.WriteLine("\tВведено некорректное значение");
                else
                    isCorrectData = true;
            }
            num_of_containers = GetNumOfContainers();

            if (method == 1)
                return RandomInput();
            else if (method == 2)
                return ConsoleInput();
            else
                return FileInput();
        }

        static float[][] RandomInput()
        {
            Random rnd = new Random();
            int num_of_things = GetNumOfThings();
            float[][] weights = new float[1][];
            weights[0] = new float[num_of_things];
            for (int i = 0; i < num_of_things; i++)
                weights[0][i] = (float)rnd.NextDouble();
            return weights;
        }

        static float[][] ConsoleInput()
        {
            int num_of_things = GetNumOfThings();
            float[][] weights = new float[1][];
            weights[0] = new float[num_of_things];
            int i = 0;
            while (i < num_of_things)
            {
                Console.WriteLine("Введите массу " + (i + 1).ToString() + "-го предмета:");
                string s = Console.ReadLine().Replace(',', '.');
                if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out weights[0][i]))
                    Console.WriteLine("\tВведена некорректная масса предмета");
                else if (weights[0][i] <= 0 || weights[0][i] > 1)
                    Console.WriteLine("\tМасса предмета должна быть больше 0 и не больше 1");
                else
                    i++;
            }
            return weights;
        }

        static float[][] FileInput()
        {
            if (!File.Exists(file_path))
            {
                Console.WriteLine("Файл " + file_path + " не найден");
                return null;
            }
            string[] data = File.ReadAllLines(file_path);
            if (data.Length < 1)
            {
                Console.WriteLine("Файл " + file_path + " пустой");
                return null;
            }
            float[][] result = new float[data.Length][];
            for (int j = 0; j < data.Length; j++)
            {
                string[] weights = data[j].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                result[j] = new float[weights.Length];
                for (int i = 0; i < weights.Length; i++)
                {
                    weights[i] = weights[i].Replace(',', '.');
                    if (!float.TryParse(weights[i], NumberStyles.Float, CultureInfo.InvariantCulture, out result[j][i]))
                        return null;
                }
            }
            return result;
        }

        static int GetNumOfContainers()
        {
            bool isCorrectData = false;
            int num_of_containers = 0;
            while (!isCorrectData)
            {
                Console.WriteLine("Введите количество контейнеров:");
                string s = Console.ReadLine();
                if (!int.TryParse(s, out num_of_containers))
                    Console.WriteLine("\tВведено некорректное количество контейнеров");
                else if (num_of_containers > max_num_of_containers)
                    Console.WriteLine("\tКоличество контейнеров не может быть больше " + max_num_of_containers);
                else if (num_of_containers < 1)
                    Console.WriteLine("\tКоличество контейнеров не может быть меньше 1");
                else
                    isCorrectData = true;
            }
            return num_of_containers;
        }

        static int GetNumOfThings()
        {
            bool isCorrectData = false;
            int num_of_things = 0;
            while (!isCorrectData)
            {
                Console.WriteLine("Введите количество предметов:");
                string s = Console.ReadLine();
                if (!int.TryParse(s, out num_of_things))
                    Console.WriteLine("\tВведено некорректное количество предметов");
                else if (num_of_things > max_num_of_things)
                    Console.WriteLine("\tКоличество предметов не может быть больше " + max_num_of_things);
                else if (num_of_things < 1)
                    Console.WriteLine("\tКоличество предметов не может быть меньше 1");
                else
                    isCorrectData = true;
            }
            return num_of_things;
        }
    }
}
