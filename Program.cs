using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CubeAttack
{
    internal class Program
    {
        static private string outputKeyBits;
        static private int[] key = { 1, 0, 1};

        private enum CubeAttackMode { preprocessing, online};

        static public int NumLinearTest = 10; // number of testing function for linearity(BLR TEST)
        static public int NumSecretParam = 3; // number of secret param(lenght of key in the cipher implemention)
        static public int NumPublicVar = 3;

        static public Matrix.Matrix superpolyMatrix = null;
        static public List<List<int>> listCubeIndexes = null;
        static public Vector.Vector outputBits = null;           

        /// <summary>
        /// Master polynom represent such: f(v,x) = v_0*v_1*x_0 + v_0*v_1*x_1 + v_2*x_0*x_2 + v_1*x_2 + v_0*x_0 + v_0*v_1+
        ///                                         +x_0*x_2 + v_1 + x_2+1 .
        /// </summary>
        /// <param name="v">public input.</param>
        /// <param name="x">secret input.</param>
        /// <returns>Returns output bit, either 0 or 1.</returns>
        static private int func(int[] v, int[] x)
        {
            return v[0] & v[1] & x[0] ^ v[0] & v[1] & x[1] ^ v[2] & x[0] & x[2] ^ v[1] & x[2] ^
                   v[0] & x[0] ^ v[0] & v[1] ^ x[0] & x[2] ^ v[1] ^ x[2] ^ 1;
        }

        /// <summary>
        /// The function returns the black box output bit.
        /// </summary>
        /// <param name="v">Public variables.</param>
        /// <param name="x">Secret variables.</param>
        /// <returns>Returns the black box output bit, either 0 or 1.</returns>
        static public int black_box(int[] v, int[] x)
        {
            return func(v, x);
        }

        /// <summary>
        /// Test for linearity of superpoly (BLR linearity test).
        /// </summary>
        /// <param name="v">public variable.</param>
        /// <returns>A boolean value indicating if the superpoly is probably linear or not.</returns>
        static public bool linearity_test(int[] v, List<int> maxterm)
        {
            Random rnd = new Random();
            int[] x = new int[NumSecretParam];
            int[] y = new int[NumSecretParam];
            int[] xy = new int[NumSecretParam];
            int[] secVarElement = new int[NumSecretParam];
            int res = 0;

            for (int i = 0; i < NumLinearTest; i++)
            {
                for (int j = 0; j < NumSecretParam; j++)
                {
                    x[j] = rnd.Next(0, 2);
                    y[j] = rnd.Next(0, 2);
                    xy[j] = x[j] ^ y[j];
                }

                //Fix the public inputs not in the set of cube I to zero and for other
                //we put in all state(i.e in 2^(cube.size))
                for (ulong k = 0; k < Math.Pow(2, maxterm.Count); k++)
                {
                    for (int b = 0; b < maxterm.Count; b++)
                        v[maxterm[b]] = (k & ((ulong)1 << b)) > 0 ? 1 : 0;
                    res ^= black_box(v, x) ^ black_box(v, y) ^ black_box(v, xy) ^ black_box(v, (int[])secVarElement.Clone());
                }

                if (res == 0) return true;
                if (res == 1) return false;
            }
            return true;
        }

        /// <summary>
        /// The function derives the algebraic structure of the superpoly from the maxterm.
        /// The structure is derived by computing the free term and the coefficients in the superpoly.
        /// </summary>
        /// <param name="pubVarElement">Public variables.</param>
        /// <param name="maxterm">Maxterm.</param>
        /// <returns>Returns the superpoly.</returns>
        static public List<int> ComputeSuperpoly(int[] pubVarElement, List<int> maxterm)
        {
            int constant = 0;
            int coeff = 0;
            List<int> superpoly = new List<int>();
            int[] secVarElement = new int[NumSecretParam];

            // Compute the free term such:
            // sume over selected cube, and other variables into 0;
            for (ulong i = 0; i < Math.Pow(2, maxterm.Count); i++)
            {
                for (int j = 0; j < maxterm.Count; j++)
                    pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                constant ^= black_box((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
            }
            superpoly.Add(constant);

            // Compute coefficients such:
            // sume over selected cube inputing other variables into 0 +
            // sume over selected cube with other variables into 0, except x_j place.
            for (int k = 0; k < NumSecretParam; k++)
            {
                for (ulong i = 0; i < Math.Pow(2, maxterm.Count); i++)
                {
                    secVarElement[k] = 1;
                    for (int j = 0; j < maxterm.Count; j++)
                        pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                    coeff ^= black_box((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
                }
                superpoly.Add(constant ^ coeff);

                coeff = 0;
                secVarElement[k] = 0;
            }
            return superpoly;
        }

        /// <summary>
        /// Represent superpoly in the string style : consts + x_j , j=1,...,n.
        /// </summary>
        /// <param name="superpoly">Superpoly represent by list of index</param>
        /// <returns>Superpoly in string format</returns>
        static public string SuperpolyAsString(List<int> superpoly)
        {
            List<string> sp = new List<string>();

            for (int i = 0; i < superpoly.Count; i++)
                if (superpoly[i] == 1) sp.Add(i == 0 ? "1" : "x" + (i - 1));

            return (sp.Count == 0) ? "0" : string.Join("+", sp);
        }

        /// <summary>
        /// Information.
        /// </summary>
        /// <param name="cubeIndexes"></param>
        /// <param name="superpoly"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string GetLogMessage(List<int> cubeIndexes, List<int> superpoly, int? value = null)
        {
           // cubeIndexes.Sort();

            return "Superpoly: " + SuperpolyAsString(superpoly) + ((value != null) ? " = " + value : "") +
                   " \tCube indexes: {" + string.Join(",", cubeIndexes) + "}" + "\n";
        }

        /// <summary>
        /// The function outputs the key bits.
        /// </summary>
        /// <param name="res">Result vector</param>
        static public string OutputKey(Vector.Vector res)
        {
            StringBuilder output = new StringBuilder(string.Empty);
            for (int i = 0; i < res.Length; i++)
                output.AppendLine("x" + i + " = " + res[i]);
            outputKeyBits = output.ToString();
            return outputKeyBits;
        }

        /// <summary>
        /// Test if superpoly is already in matrix.
        /// </summary>
        /// <param name="superpoly">The superpoly.</param>
        /// <param name="matrix">An n x n matrix whose rows contain their corresponding superpolys.</param>
        /// <returns>A boolean value indicating if the superpoly is in the matrix or not.</returns>
        static public bool InMatrix(List<int> superpoly, Matrix.Matrix matrix)
        {
            bool isEqual = true;
            for (int i = 0; i < matrix.Rows; i++)
            {
                isEqual = true;
                for (int j = 0; j < superpoly.Count; j++)
                    if (matrix[i, j] != superpoly[j])
                        isEqual = false;
                if (isEqual)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Test if an n x m matrix contains n linearly independent vectors.
        /// </summary>
        /// <param name="A">n x m matrix.</param>
        /// <returns>A boolean value indicating if the matrix is regular or not.</returns>
        static public bool IsLinearIndependent(Matrix.Matrix A)
        {
            double maxval;
            int maxind;
            double temp;
            int Rang = 0;
            double[,] a = new double[A.Cols, A.Rows];

            for (int i = 0; i < A.Cols; i++)
                for (int j = 0; j < A.Rows; j++)
                    a[i, j] = A[j, i];

            for (int j = 0; j < A.Rows; j++)
            {
                // Find maximum
                maxval = a[j, j];
                maxind = j;
                for (int k = j; k < A.Cols; k++)
                {
                    if (a[k, j] > maxval)
                    {
                        maxval = a[k, j];
                        maxind = k;
                    }
                    if (-a[k, j] > maxval)
                    {
                        maxval = -a[k, j];
                        maxind = k;
                    }
                }

                if (maxval != 0)
                {
                    Rang++;
                    // Swap_Rows(j, maxind)
                    for (int k = j; k < A.Rows; k++)
                    {
                        temp = a[j, k];
                        a[j, k] = a[maxind, k];
                        a[maxind, k] = temp;
                    }

                    // Gauss elimination
                    for (int i = j + 1; i < A.Cols; i++)
                        for (int k = j + 1; k < A.Rows; k++)
                            a[i, k] = a[i, k] - (a[i, j] / a[j, j] * a[j, k]);
                }
            }

            return A.Rows == Rang;
        }

        /// <summary>
        /// Preprocessing phase of the cube attack.
        /// </summary>
        static public void PreprocessingPhase()
        {
            superpolyMatrix = new Matrix.Matrix(0, NumSecretParam + 1);
            listCubeIndexes = new List<List<int>>();
            var nulSeq = new List<int>();
            for (int i = 0; i < NumPublicVar + 1; i++)
            {
                nulSeq.Add(0);
            }

            int maxCubeSize = NumPublicVar;
            double numOfSubsets = Math.Pow(2, 3);
            int lci_size = 1;

            using (StreamWriter sw = new StreamWriter(Param.Path.PathToTheFolderResult + "preprocessingPhaseResult" + ".txt", false, Encoding.Default))
            {
                while (lci_size < maxCubeSize + 1)   // find lci_size'th cube
                {
                    // iterate through all the cubes
                    for (int i = 1; i < numOfSubsets; i++)
                    {
                        // cube formation
                        listCubeIndexes.Add(new List<int>());

                        for (int j = maxCubeSize; j > -1; j--)  // to adding into list in right order(for beauty).(But not necessarily).
                        {
                            if (((i >> (j - 1)) & 1) == 1)      // getting the j-th bit of i (right to left).
                            {
                                listCubeIndexes[lci_size - 1].Add(maxCubeSize - j);    // believe that indexing cubes from left to right.
                            }
                        }

                        var superpoly = new List<int>();
                        if (linearity_test(new int[NumPublicVar], listCubeIndexes[lci_size - 1]))
                        {
                            superpoly = ComputeSuperpoly(new int[NumPublicVar], listCubeIndexes[lci_size - 1]);

                            if ((!(superpoly.SequenceEqual(nulSeq))) && (!InMatrix(superpoly, superpolyMatrix)))
                            {
                                superpolyMatrix = superpolyMatrix.AddRow(superpoly);
                                if (!IsLinearIndependent(superpolyMatrix))
                                {
                                    superpolyMatrix = superpolyMatrix.DeleteLastRow();
                                    listCubeIndexes.RemoveAt(lci_size - 1);
                                    continue;
                                }

                                Console.WriteLine(GetLogMessage(listCubeIndexes[lci_size - 1], superpoly));
                                sw.WriteLine(GetLogMessage(listCubeIndexes[lci_size - 1], superpoly));

                                lci_size++;
                                continue;
                            }
                            listCubeIndexes.RemoveAt(lci_size - 1);
                            continue;
                        }
                        listCubeIndexes.RemoveAt(lci_size - 1);
                        continue;
                    }

                }
                sw.Close();
            }

            Operation.Serialize_W.serialize_w(superpolyMatrix, "superpolyMatrix");
            Operation.Serialize_W.serialize_w(listCubeIndexes, "cubeIndexes");
        }

        /// <summary>
        /// Online phase of the cube attack.
        /// </summary>
        static public void OnlinePhase()
        {
            outputBits = new Vector.Vector(NumSecretParam);
            var superpolyMatrixWithoutConst = new Matrix.Matrix(NumSecretParam, NumSecretParam);
            int[] pubVarElement = new int[NumPublicVar];

            if (listCubeIndexes==null & superpolyMatrix == null)
            {
                listCubeIndexes = Operation.Serialize_W.deserialize_w_ll("cubeIndexes");
                superpolyMatrix = Operation.Serialize_W.deserialize_w_mm("superpolyMatrix");
            }

            superpolyMatrixWithoutConst = superpolyMatrix.DeleteFirstColumn();

            for (int i = 0; i < listCubeIndexes.Count(); i++)
            {
                for (ulong j = 0; j < Math.Pow(2, listCubeIndexes[i].Count); j++)
                {
                    for (int k = 0; k < listCubeIndexes[i].Count; k++)
                        pubVarElement[listCubeIndexes[i][k]] = (j & ((ulong)1 << k)) > 0 ? 1 : 0;
                    outputBits[i] ^= black_box(pubVarElement, key);
                }
                outputBits[i] ^= superpolyMatrix[i,0];
            }

            var _key = new Vector.Vector(NumSecretParam);
            _key = superpolyMatrixWithoutConst.Inverse() * outputBits;

            Console.WriteLine(OutputKey(_key));
        }

        /// <summary>
        /// Does the actual CubeAttack processing
        /// </summary>
        static private void ProcessCubeAttack(CubeAttackMode mode)
        {
            switch (mode)
            {
                case CubeAttackMode.preprocessing:
                    PreprocessingPhase();
                    break;

                case CubeAttackMode.online:
                    OnlinePhase();
                    break;
            }
        }

        static public void Preprocessing()
        {
            ProcessCubeAttack(CubeAttackMode.preprocessing);
        }

        static public void Online()
        {
            ProcessCubeAttack(CubeAttackMode.online);
        }

        private static void Main(string[] args)
        {
            // Preprocessing();
            // Online();
            BigInteger key = Operation.BigInteger_W.ToHex("FFFFFFFFFFFFFFFFFFFF");
            BigInteger cipher = Operation.BigInteger_W.ToHex("3333DCD3213210D2");

            Present A = new Present(key);
            Console.WriteLine(A.Encrypt(0).ToString("X"));
         
            Console.ReadLine(); 
        }

    }
}