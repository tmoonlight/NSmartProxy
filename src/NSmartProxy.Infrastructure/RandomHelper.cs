using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Infrastructure
{
    /// <summary>
    /// MoonEngine 随机库。
    /// </summary>
    public static class RandomHelper
    {
        static readonly object Padlock = new object();

        /// <summary>
        /// Initializes the <see cref="RandomHelper"/> class.
        /// </summary>
        static RandomHelper()
        {
            RandomHelper.Random = new Random();
        }

        /// <summary>
        /// Gets or sets the random number generator.
        /// </summary>
        /// <value>The random.</value>
        static private Random Random { get; set; }

        /// <summary>
        /// Returns a non-negetive random whole number.
        /// </summary>
        static public int NextInt()
        {
            lock (RandomHelper.Padlock)
            {
                return RandomHelper.Random.Next();
            }
        }

        /// <summary>
        /// Returns a non-negetive random whole number less than the specified maximum.
        /// </summary>
        /// <param name="max">The exclusive upper bound the random number to be generated.</param>
        static public int NextInt(int max)
        {
            lock (RandomHelper.Padlock)
            {
                return RandomHelper.Random.Next(max);
            }
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="min">The inclusive lower bound of the random number returned.</param>
        /// <param name="max">The exclusive upper bound of the random number returned.</param>
        static public int NextInt(int min, int max)
        {
            lock (RandomHelper.Padlock)
            {
                return RandomHelper.Random.Next(min, max);
            }
        }

        /// <summary>
        /// Returns a random float between 0.0 and 1.0.
        /// </summary>
        static public float NextFloat()
        {
            lock (RandomHelper.Padlock)
            {
                return (float)RandomHelper.Random.NextDouble();
            }
        }

        static public string NextString(int length, bool hasSpecialChara = true)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null;
            string str = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (hasSpecialChara) str += "!#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }

        /// <summary>
        /// Returns a random float betwen 0.0 and the specified upper bound.
        /// </summary>
        /// <param name="max">The inclusive upper bound of the random number returned.</param>
        static public float NextFloat(float max)
        {
            return max * RandomHelper.NextFloat();
        }

        /// <summary>
        /// Returns a random float within the specified range.
        /// </summary>
        /// <param name="min">The inclusive lower bound of the random number returned.</param>
        /// <param name="max">The inclusive upper bound of the random number returned.</param>
        static public float NextFloat(float min, float max)
        {
            return ((max - min) * RandomHelper.NextFloat()) + min;
        }


        /// <summary>
        /// Returns a random byte.
        /// </summary>
        static public byte NextByte()
        {
            return (byte)RandomHelper.NextInt(255);
        }

        /// <summary>
        /// Returns a random boolean value.
        /// </summary>
        static public bool NextBool()
        {
            return RandomHelper.NextInt(2) == 1;
        }


        /// <summary>
        /// Returns a random variation of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="variation">The variation amount.</param>
        /// <example>A value of 10 with a variation of 5 will result in a random number between 5.0 and 15.</example>
        static public float Variation(float value, float variation)
        {
            float min = (value - variation),
                  max = (value + variation);

            return RandomHelper.NextFloat(min, max);
        }

        /// <summary>
        /// Chooses a random item from the specified parameters and returns it.
        /// </summary>
        static public int ChooseOne(params int[] values)
        {
            int index = RandomHelper.NextInt(values.Length);

            return values[index];
        }


        /// <summary>
        /// Chooses a random item from the specified parameters and returns it.
        /// </summary>
        static public float ChooseOne(params float[] values)
        {
            int index = RandomHelper.NextInt(values.Length);

            return values[index];
        }

        /// <summary>
        /// Chooses a random item from the specified parameters and returns it.
        /// </summary>
        static public T ChooseOne<T>(params T[] values)
        {
            int index = RandomHelper.NextInt(values.Length);

            return values[index];
        }

        /// <summary>
        /// 洗牌并取出前面第n个.
        /// </summary>
        static public T[] ShuffleAndReturnN<T>(int n, params T[] values)
        {
            for (int i = 0; i < n; i++)
            {
                int preSwapIndex = RandomHelper.NextInt(values.Length);
                T temp = values[preSwapIndex];
                values[preSwapIndex] = values[i];
                values[i] = temp;
            }
            if (n > 0)
            {
                T[] returnedArray = new T[n];
                for (int j = 0; j < returnedArray.Length; j++)
                {
                    returnedArray[j] = values[j];
                }
                return returnedArray;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 洗牌算法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        static public void Shuffle<T>(params T[] values)
        {
            ShuffleAndReturnN<T>(0, values);
        }

    }
}
