using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Operation
{
    /// Serialize wrapper 
    class Serialize_W
    {
        /// <summary>
        /// Function-wrapper which serialize object.
        /// </summary>
        /// <param name="superpolyMatrix">object that is serialized.</param>
        /// <param name="nameSerializeObj">Save obj into this* file.</param>
        static public void serialize_w(object superpolyMatrix, string nameSerializeObj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(Param.Path.PathToTheFolderResult + nameSerializeObj + ".dat", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, superpolyMatrix);
                Console.WriteLine("The object is serialized.\n");
            }
        }

        /// <summary>
        /// Function-wrapper which deserialize object.
        /// </summary>
        /// <param name="nameDeserializeObj">object is read from this* file.</param>
        /// <returns>desired object from a file.</returns>
        static public Matrix.Matrix deserialize_w_mm(string nameDeserializeObj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Matrix.Matrix superpolyMatrix = null;

            using (FileStream fs = new FileStream(Param.Path.PathToTheFolderResult + nameDeserializeObj + ".dat", FileMode.OpenOrCreate))
            {
                superpolyMatrix = (Matrix.Matrix)formatter.Deserialize(fs);
                Console.WriteLine("The Matrix {0} deserialized.\n", nameDeserializeObj);
            }
            return superpolyMatrix;
        }

        static public List<List<int>> deserialize_w_ll(string nameDeserializeObj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            List<List<int>> listCubeIndexes = null;

            using (FileStream fs = new FileStream(Param.Path.PathToTheFolderResult + nameDeserializeObj + ".dat", FileMode.OpenOrCreate))
            {
                listCubeIndexes = (List<List<int>>)formatter.Deserialize(fs);
                Console.WriteLine("The List<List<int>> {0} deserialized.", nameDeserializeObj);
            }
            return listCubeIndexes;
        }

    }

    /// My stream reader 
    class IO
    {
        /// <summary>
        /// Function to create List<List<int>> (AmmountOfCube x maxLenghtCube).
        /// </summary>
        /// <param name="fileName">reading from this* file.</param>
        /// <param name="maxCubeSize">Maximum lenght of cube.</param>
        /// <returns>List of Cube indexes </returns>
        static public List<List<int>> readerFormFile(string fileName, int maxCubeSize)
        {
            List<List<int>> TlistCubeIndexes = new List<List<int>>();

            StreamReader s = File.OpenText(Param.Path.PathToTheFolderResult + fileName + ".txt");
            string read;
            int lci_size = 0;
            while ((read = s.ReadLine()) != null)
            {
                for (int i = 0; i < read.Length - 3; i += 4)
                {
                    TlistCubeIndexes.Add(new List<int>());

                    for (int j = 0; j < 4; j++)
                    {
                        TlistCubeIndexes[lci_size].Add(int.Parse(read[i + j].ToString()));
                    }
                    lci_size++;
                }
            }
            s.Close();

            return TlistCubeIndexes;
        }
    }

    public class BigInteger_W
    {
        /// <summary>
        /// Convert decimal value into hex.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BigInteger FromHexToDec(string value)
        {
            BigInteger output = BigInteger.Parse("00" + value, NumberStyles.AllowHexSpecifier);
            return output;
        }
    }

    public class MathHelper
    {
        public static int BitCount(BigInteger key)
        {
            int bitC = 0;

            for (int i = 0; i < BigInteger.Log(key, 2) + 1; i++)
            {
                if (((key >> i) & 1) == 1) { bitC++; };
            }
            return bitC;
        }

        public static int BitCount(BigInteger key,int nByte)
        {
            int bitC = 0;

            for (int i = 8*nByte; i < 8*nByte+8; i++)
            {
                if (((key >> i) & 1) == 1) { bitC++; };
            }
            return bitC;
        }

        public static int BitCount(int key)
        {
            int bitC = 0;

            for (int i = 0; i < BigInteger.Log(key, 2) + 1; i++)
            {
                if (((key >> i) & 1) == 1) { bitC++; };
            }
            return bitC;
        }

        public static int BitCount(ushort key)
        {
            int bitC = 0;

            for (int i = 0; i < BigInteger.Log(key, 2) + 1; i++)
            {
                if (((key >> i) & 1) == 1) { bitC++; };
            }
            return bitC;
        }

        public static int BitCount(int key, int nByte)
        {
            int bitC = 0;

            for (int i = 8 * nByte; i < 8 * nByte + 8; i++)
            {
                if (((key >> i) & 1) == 1) { bitC++; };
            }
            return bitC;
        }

        public static int BitCount(ushort key, int nByte)
        {
            int bitC = 0;

            for (int i = 8 * nByte; i < 8 * nByte + 8; i++)
            {
                if (((key >> i) & 1) == 1) { bitC++; };
            }
            return bitC;
        }

        public static int BitCount(List<int> superpoly)
        {
            int bitC = 0;

            for (int i = 0; i < superpoly.Count; i++)
            {
                if (superpoly[i] != 0)
                    bitC += 1;
            }

            return bitC;
        }

        public static int CountNotNullEl(List<int> superpoly)
        {
            int bitC = 0;

            for (int i = 0; i < superpoly.Count; i++)
            {
                if (superpoly[i] != 0)
                    bitC += 1;
            }

            return bitC;
        }

        public static int GetIBit(int value, int index)
        {
            return (value >> index) & 1;
        }

        public static BigInteger GetBigIntFromIndexArrayFromLSB(int[] bitArray)
        {
            BigInteger value = 0;

            for (int i = 0; i<bitArray.Length; i++)
            {
                if (bitArray[i] == 1)
                    value += BigInteger.Pow(2, bitArray.Length-i-1 );
            }

            return value;
        }

        public static BigInteger GetBigIntFromIndexArrayFromMSB(int[] bitArray)
        {
            BigInteger value = 0;

            for (int i = 0; i < bitArray.Length; i++)
            {
                if (bitArray[i] == 1)
                    value += BigInteger.Pow(2, i);
            }

            return value;
        }

        /// <summary>
        /// Lehmer High randon generator.
        /// </summary>
        public static int[] RandomGenerator(int size)
        {
            int[] seq = new int[size];
            Encoding enc = Encoding.GetEncoding(1251);
            Random random = new Random(DateTime.Now.Millisecond);
            var state_L20 = new List<int>();
            for (int j = 0; j < 20; j++)
            {
                random = new Random(DateTime.Now.Millisecond);
                state_L20.Add(random.Next(0, 2));
            }
            for (int i = 0; i < size; i++)
            {

                state_L20.Add(state_L20[17] ^ state_L20[15] ^ state_L20[11] ^ state_L20[0]);
                seq[i] = state_L20[0];
                state_L20.RemoveAt(0);
            }

            return seq;
        }

        //public static int[] RandomGenerator(int size)
        //{
        //    int[] seq = new int[size];
        //    Encoding enc = Encoding.GetEncoding(1251);
        //    Random random = new Random();
        //    var state_L89 = new List<int>();
        //    for (int j = 0; j < 89; j++)
        //    {
        //        random = new Random(DateTime.Now.Millisecond);
        //        state_L89.Add(random.Next(0, 2));
        //    }

        //    for (int i = 0; i < size; i++)
        //    {
        //        state_L89.Add(state_L89[51] ^ state_L89[0]);
        //        seq[i] = state_L89[0];
        //        state_L89.RemoveAt(0);
        //    }

        //    return seq;
        //}

        //private const string p = "D5BBB96D30086EC484EBA3D7F9CAEB07";
        //private const string q = "425D2B9BFDB25B9CF6C416CC6E37B59C1F";
        //private static Random randomForBM = new Random();
        //private static Random random = new Random();

        //public static string RandomString(int length)
        //{
        //    const string chars = "ABCDEF0123456789";
        //    return new string(Enumerable.Repeat(chars, length).Select(s => s[randomForBM.Next(s.Length)]).ToArray());
        //}
        //public static int[] RandomGenerator(int size)
        //{
        //    int[] seq = new int[size];
        //    string randomHex = RandomString(2);
        //    BigInteger r0 = BigInteger_W.FromHexToDec(randomHex);
        //    BigInteger P = BigInteger_W.FromHexToDec(p);
        //    BigInteger Q = BigInteger_W.FromHexToDec(q);
        //    BigInteger xi = new BigInteger();
        //    BigInteger n = BigInteger.Multiply(P, Q);

        //    for (int i = 0; i < size; i++)
        //    {
        //        r0 = BigInteger.ModPow(r0, r0, n);
        //        xi = r0 % 2;
        //        seq[i] = (int)xi;
        //    }
        //    return seq;
        //}

        public static List<int> RandomGeneratorList(int size)
        {
            Random randx0 = new Random(DateTime.Now.Millisecond);
            ulong m = (ulong)Math.Pow(2, 32);
            ulong a = (ulong)Math.Pow(2, 16) + 1;
            ulong c = 119;
            List<int> seq = new List<int>(size);
            ulong xi = (ulong)randx0.Next(1, (int)Math.Pow(2, 30));

            for (int i = 0; i < size; i++)
            {
                xi = (a * xi + c) % m;
                int xcasted = (int)xi;
                for (int j = 25; j < 33; j++)
                {
                    seq.Add(((xcasted >> j) & 1));
                }
                xi = (ulong)xcasted;
            }

            return seq;
        }

        public static ushort[] ConvertFromBoolVectorToByteArray(int[] boolVector, int wordVectorSize)
        {
            int sizeByteV = boolVector.Length / wordVectorSize;
            ushort[] byteV = new ushort[sizeByteV];
            for (int b = 0; b < sizeByteV; b++)
            {
                for (int x = b * wordVectorSize; x < b * wordVectorSize + wordVectorSize; x++)
                    if (boolVector[x] == 1)
                    {
                        double deg = x - b * wordVectorSize;
                        byteV[b] += (ushort)Math.Pow(2, deg);
                    }
            }
            return byteV;
        }


        public static int[] ConvertFromBoolVectorToIntArray(int[] boolVector, int wordVectorSize)
        {
            int sizeByteV = boolVector.Length / wordVectorSize;
            int[] byteV = new int[sizeByteV];
            for (int b = 0; b < sizeByteV; b++)
            {
                for (int x = b * wordVectorSize; x < b * wordVectorSize + wordVectorSize; x++)
                    if (boolVector[x] == 1)
                    {
                        double deg = x - b * wordVectorSize;
                        byteV[b] += (ushort)Math.Pow(2, deg);
                    }
            }
            return byteV;
        }

        public static byte[] ConvertFromBoolVectorToByteNibbleArray(int[] boolVector, int wordVectorSize)
        {
            int sizeByteV = boolVector.Length / wordVectorSize;
            byte[] byteV = new byte[sizeByteV];
            for (int b = 0; b < sizeByteV; b++)
            {
                for (int x = b * wordVectorSize; x < b * wordVectorSize + wordVectorSize; x++)
                    if (boolVector[x] == 1)
                    {
                        double deg = x - b * wordVectorSize;
                        byteV[b] += (byte)Math.Pow(2, deg);
                    }
            }
            return byteV;
        }



        public static long combination(long n, long k)
        {
            double sum = 0;
            for (long i = 0; i < k; i++)
            {
                sum += Math.Log10(n - i);
                sum -= Math.Log10(i + 1);
            }
            return (long)Math.Pow(10, sum);
        }
    }

}