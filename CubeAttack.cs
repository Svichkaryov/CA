using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using OM = Operation.MathHelper;

namespace CubeAttackTest
{
    class CubeAttack
    {
        static private string outputKeyBits;
        static private int[] key = { 1, 0, 1 };

        private enum CubeAttackMode { preprocessing, online, setPublicBits };

        static public int NumLinearTest = 100;
        static public int NumQuadraticTest = 100;
        static public int NumSecretParam = 80;   // number of secret param(lenght of key in the cipher implemention)
        static public int NumPublicVar = 80;

        static public Matrix.Matrix superpolyMatrix = null;
        static public List<List<int>> listCubeIndexes1 = null;
        static public List<List<int>> listCubeIndexes2 = null;
        static public Vector.Vector outputBits = null;

        /// <summary>
        /// Master polynom represent such: f(v,x) = ....
        /// </summary>
        /// <param name="v">public input.</param>
        /// <param name="x">secret input.</param>
        /// <returns>Returns output bit, either 0 or 1.</returns>
        static private int func(int[] v, int[] x)
        {
            return (v[0] & v[1] & x[0]) ^ (v[0] & v[1] & x[2]) ^ (v[1] & x[2]) ^ (v[1] & x[1]) ^ v[0] & x[0] ^
                   v[0] & v[1] ^ (v[2] & x[1] & x[0]) ^ v[1] ^ x[1] ^ 1;
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
            Random rnd = new Random(DateTime.Now.Millisecond);
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

        static public bool quadratic_test(int[] v, List<int> maxterm)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int[] x = new int[NumSecretParam];
            int[] y = new int[NumSecretParam];
            int[] z = new int[NumSecretParam];
            int[] xy = new int[NumSecretParam];
            int[] xz = new int[NumSecretParam];
            int[] zy = new int[NumSecretParam];
            int[] secVarElement = new int[NumSecretParam];
            int res = 0;

            for (int i = 0; i < NumQuadraticTest; i++)
            {
                for (int j = 0; j < NumSecretParam; j++)
                {
                    x[j] = rnd.Next(0, 2);
                    y[j] = rnd.Next(0, 2);
                    z[j] = rnd.Next(0, 2);
                    xy[j] = x[j] ^ y[j];
                    xz[j] = x[j] ^ z[j];
                    zy[j] = z[j] ^ y[j];
                }

                //Fix the public inputs not in the set of cube I to zero and for other
                //we put in all state(i.e in 2^(cube.size))
                for (ulong k = 0; k < Math.Pow(2, maxterm.Count); k++)
                {
                    for (int b = 0; b < maxterm.Count; b++)
                        v[maxterm[b]] = (k & ((ulong)1 << b)) > 0 ? 1 : 0;
                    res ^= black_box(v, x) ^ black_box(v, y) ^ black_box(v, z) ^ black_box(v, xy)
                         ^ black_box(v, xz) ^ black_box(v, zy) ^ black_box(v, (int[])secVarElement.Clone());
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
        /// Compute indexes of secret variables in superpoly(deg=2)
        /// </summary>
        /// <param name="pubVarElement"></param>
        /// <param name="maxterm">list of indexes(I)</param>
        /// <returns>indexes of secret variables</returns>
        public static List<int> SecretVariableIndexes(int[] pubVarElement, List<int> maxterm)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int[] y = new int[NumSecretParam];
            int[] y0 = new int[NumSecretParam];
            int[] y1 = new int[NumSecretParam];
            int res = 0;
            int NumOfRandomSample = 300;
            List<int> LSecretVariableIndexes = new List<int>();

            for (int i = 0; i < NumSecretParam; i++)
            {
                for (int k = 0; k < NumOfRandomSample; k++)
                {
                    for (int j = 0; j < NumSecretParam; j++)
                    {
                        y[j] = rnd.Next(0, 2);
                        y0[j] = y[j];
                        y1[j] = y[j];
                    }
                    y0[i] = 0;
                    y1[i] = 1;

                    for (ulong l = 0; l < Math.Pow(2, maxterm.Count); l++)
                    {
                        for (int b = 0; b < maxterm.Count; b++)
                            pubVarElement[maxterm[b]] = (l & ((ulong)1 << b)) > 0 ? 1 : 0;
                        res ^= black_box(pubVarElement, y0) ^ black_box(pubVarElement, y1);
                    }

                    if (res == 1)
                    {
                        LSecretVariableIndexes.Add(i);
                        res = 0;
                        break;
                    }
                }
            }


            for (int i = 0; i < NumPublicVar; i++)
                pubVarElement[i] = 0;

            return LSecretVariableIndexes;
        }

        /// <summary>
        /// Compute term of superpoly(deg=2)
        /// </summary>
        /// <param name="v">public variables</param>
        /// <param name="SVI">Secret variable indexes</param>
        /// <returns>List of term</returns>
        public static List<List<int>> ComputeSuperpoly2(int[] v, List<int> SVI, List<int> I)
        {
            var ListOfTerm = new List<List<int>>();
            ListOfTerm.Add(new List<int>());
            ListOfTerm.Add(new List<int>());
            int[] secVarElement = new int[NumSecretParam];
            int res = 0;

            // for K -- 1-demension (amount = binom_coeff(1,SVI.Count()))
            for (int r1 = 0; r1 < SVI.Count(); r1++)
            {
                int[] yr0 = new int[NumSecretParam];
                int[] yr1 = new int[NumSecretParam];
                for (int j = 0; j < NumSecretParam; j++)
                {
                    yr0[j] = 0;
                    yr1[j] = 0;
                }
                yr0[r1] = 0;
                yr1[r1] = 1;


                for (ulong l = 0; l < Math.Pow(2, I.Count); l++)
                {
                    for (int b = 0; b < I.Count; b++)
                        v[I[b]] = (l & ((ulong)1 << b)) > 0 ? 1 : 0;
                    res ^= black_box(v, yr0) ^ black_box(v, yr1);
                }

                if (res == 1)
                {
                    ListOfTerm[0].Add(r1);
                    ListOfTerm[1].Add(0);
                    res = 0;
                }
            }

            // for K -- 2-demension (amount = binom_coeff(2,SVI.Count()))
            for (int r2_1 = 0; r2_1 < SVI.Count() - 1; r2_1++)
            {
                for (int r2_2 = r2_1 + 1; r2_2 < SVI.Count; r2_2++)
                {
                    int[] yr00 = new int[NumSecretParam];
                    int[] yr01 = new int[NumSecretParam];
                    int[] yr10 = new int[NumSecretParam];
                    int[] yr11 = new int[NumSecretParam];
                    yr01[r2_2] = 1;
                    yr10[r2_1] = 1;
                    yr11[r2_1] = 1;
                    yr11[r2_2] = 1;

                    for (ulong l = 0; l < Math.Pow(2, I.Count); l++)
                    {
                        for (int b = 0; b < I.Count; b++)
                            v[I[b]] = (l & ((ulong)1 << b)) > 0 ? 1 : 0;
                        res ^= black_box(v, yr00) ^ black_box(v, yr01)
                             ^ black_box(v, yr10) ^ black_box(v, yr11);
                    }

                    if (res == 1)
                    {
                        ListOfTerm[0].Add(r2_1);
                        ListOfTerm[1].Add(r2_2);
                        res = 0;
                    }
                }
            }

            //Compute constant
            for (ulong l = 0; l < Math.Pow(2, I.Count); l++)
            {
                for (int b = 0; b < I.Count; b++)
                    v[I[b]] = (l & ((ulong)1 << b)) > 0 ? 1 : 0;
                res ^= black_box(v, (int[])secVarElement.Clone());
            }

            if (res == 1)
            {
                ListOfTerm[0].Add(res);
                ListOfTerm[1].Add(0);
                res = 0;
            }

            return ListOfTerm;
        }

        /// <summary>
        /// Represent superpoly(deg=1) in the string style : consts + x_j , j=1,...,n.
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
        /// Represent superpoly(deg=2) in the string
        /// </summary>
        /// <param name="superpoly2"></param>
        /// <returns></returns>
        static public string Superpoly2AsString(List<List<int>> superpoly2)
        {
            List<string> sp = new List<string>();

            for (int i = 0; i < superpoly2[0].Count(); i++)
            {
                if (superpoly2[0][i] == 1 & superpoly2[1][i] == 0) sp.Add(i == superpoly2[0].Count() ? "1" : "x" + superpoly2[0][i]);
                if (superpoly2[1][i] == 1) sp.Add(i == superpoly2[0].Count() ? "1" : "x" + superpoly2[0][i] + "*x" + superpoly2[1][i]);
            }
            return (sp.Count == 0) ? "0" : string.Join("+", sp);
        }

        private static string GetLogMessage1(List<int> cubeIndexes, List<int> superpoly, int? value = null)
        {
            // cubeIndexes.Sort();

            return "Superpoly: " + SuperpolyAsString(superpoly) + ((value != null) ? " = " + value : "") +
                   " \tCube indexes: {" + string.Join(",", cubeIndexes) + "}" + "\n";
        }

        private static string GetLogMessage2(List<int> cubeIndexes, List<List<int>> superpoly, int? value = null)
        {
            // cubeIndexes.Sort();

            return "Superpoly: " + Superpoly2AsString(superpoly) + ((value != null) ? " = " + value : "") +
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
            listCubeIndexes1 = new List<List<int>>();
            listCubeIndexes2 = new List<List<int>>();
            var nulSeq = new List<int>();
            for (int i = 0; i < NumPublicVar + 1; i++)
            {
                nulSeq.Add(0);
            }

            int maxCubeSize = NumPublicVar;
            double numOfSubsets = Math.Pow(2, NumPublicVar);
            int lci1_size = 1;
            int lci2_size = 0;

            using (StreamWriter sw = new StreamWriter(Param.Path.PathToTheFolderResult + "preprocessingPhaseResult1" + ".txt", false, Encoding.Default))
            {
                using (StreamWriter sw2 = new StreamWriter(Param.Path.PathToTheFolderResult + "preprocessingPhaseResult2" + ".txt", false, Encoding.Default))
                {
                    while (lci1_size < maxCubeSize + 1)   // find lci_size'th cube
                    {
                        // iterate through all the cubes
                        for (int i = 1; i < numOfSubsets; i++)
                        {
                            // cube formation
                            listCubeIndexes1.Add(new List<int>());

                            for (int j = maxCubeSize; j > -1; j--)  // to adding into list in right order(for beauty).(But not necessarily).
                            {
                                if (((i >> (j - 1)) & 1) == 1)      // getting the j-th bit of i (right to left).
                                {
                                    listCubeIndexes1[lci1_size - 1].Add(maxCubeSize - j);    // believe that indexing cubes from left to right.
                                }
                            }

                            Console.WriteLine("Computing for such cube: {" + string.Join(",", listCubeIndexes1[lci1_size - 1]) + "}");

                            var superpoly = new List<int>();
                            var superpoly2 = new List<List<int>>();
                            if (linearity_test(new int[NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                            {
                                superpoly = ComputeSuperpoly(new int[NumPublicVar], listCubeIndexes1[lci1_size - 1]);
                                if ((!(superpoly.SequenceEqual(nulSeq))) & (!InMatrix(superpoly, superpolyMatrix)))
                                {
                                    superpolyMatrix = superpolyMatrix.AddRow(superpoly);
                                    if (!IsLinearIndependent(superpolyMatrix))
                                    {
                                        superpolyMatrix = superpolyMatrix.DeleteLastRow();
                                        listCubeIndexes1.RemoveAt(lci1_size - 1);
                                        Console.WriteLine("bad cube");
                                        Console.WriteLine();
                                        continue;
                                    }

                                    Console.WriteLine(GetLogMessage1(listCubeIndexes1[lci1_size - 1], superpoly));
                                    sw.WriteLine(GetLogMessage1(listCubeIndexes1[lci1_size - 1], superpoly));

                                    lci1_size++;
                                    continue;
                                }

                                superpoly2 = new List<List<int>>();
                                if (quadratic_test(new int[NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                                {
                                    superpoly2 = ComputeSuperpoly2(new int[NumPublicVar], SecretVariableIndexes(new int[NumPublicVar], listCubeIndexes1[lci1_size - 1]),
                                                                   listCubeIndexes1[lci1_size - 1]);
                                    if (superpoly2[1].Sum() != 0)
                                    {
                                        Console.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));
                                        sw2.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));

                                        listCubeIndexes2.Add(new List<int>());
                                        foreach (var el in listCubeIndexes1[lci1_size - 1])
                                            listCubeIndexes2[lci2_size].Add(el);
                                        lci2_size++;
                                    }
                                }

                                listCubeIndexes1.RemoveAt(lci1_size - 1);
                                continue;
                            }

                            superpoly2 = new List<List<int>>();
                            if (quadratic_test(new int[NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                            {
                                superpoly2 = ComputeSuperpoly2(new int[NumPublicVar], SecretVariableIndexes(new int[NumPublicVar], listCubeIndexes1[lci1_size - 1]),
                                                               listCubeIndexes1[lci1_size - 1]);
                                if (superpoly2[1].Sum() != 0)
                                {
                                    Console.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));
                                    sw2.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));

                                    listCubeIndexes2.Add(new List<int>());
                                    foreach (var el in listCubeIndexes1[lci1_size - 1])
                                        listCubeIndexes2[lci2_size].Add(el);
                                    lci2_size++;
                                }
                            }

                            listCubeIndexes1.RemoveAt(lci1_size - 1);
                            continue;
                        }
                    }
                    sw2.Close();
                }
                sw.Close();
            }

            Operation.Serialize_W.serialize_w(superpolyMatrix, "superpolyMatrix");
            Operation.Serialize_W.serialize_w(listCubeIndexes1, "cubeIndexes1");
        }


        /// <summary>
        /// Online phase of the cube attack.
        /// </summary>
        static public void OnlinePhase()
        {
            outputBits = new Vector.Vector(NumSecretParam);
            var superpolyMatrixWithoutConst = new Matrix.Matrix(NumSecretParam, NumSecretParam);
            int[] pubVarElement = new int[NumPublicVar];

            if (listCubeIndexes1 == null & superpolyMatrix == null)
            {
                listCubeIndexes1 = Operation.Serialize_W.deserialize_w_ll("cubeIndexes1");
                superpolyMatrix = Operation.Serialize_W.deserialize_w_mm("superpolyMatrix");
            }

            superpolyMatrixWithoutConst = superpolyMatrix.DeleteFirstColumn();

            for (int i = 0; i < listCubeIndexes1.Count(); i++)
            {
                for (ulong j = 0; j < Math.Pow(2, listCubeIndexes1[i].Count); j++)
                {
                    for (int k = 0; k < listCubeIndexes1[i].Count; k++)
                        pubVarElement[listCubeIndexes1[i][k]] = (j & ((ulong)1 << k)) > 0 ? 1 : 0;
                    outputBits[i] ^= black_box(pubVarElement, key);
                }
                outputBits[i] ^= superpolyMatrix[i, 0];
            }

            var _key = new Vector.Vector(NumSecretParam);
            _key = superpolyMatrixWithoutConst.Inverse() * outputBits;

            Console.WriteLine(OutputKey(_key));
        }


        /// <summary>
        /// User-Mode to set public bit values manually.
        /// </summary>
        static public void SetPublicBitsPhase()
        {
            var cube = new List<int>();
            Console.WriteLine("Input index of cube separated by commas: ");
            var cube_str = Console.ReadLine();

            char separator = ',';
            string[] items = cube_str.Split(separator);
            foreach (var cubeIndex in items)
                cube.Add(int.Parse(cubeIndex));

            var nulSeq = new List<int>();
            for (int i = 0; i < NumPublicVar + 1; i++)
            {
                nulSeq.Add(0);
            }

            var superpoly = new List<int>();
            var superpoly2 = new List<List<int>>();
            if (linearity_test(new int[NumPublicVar], cube))
            {
                superpoly = ComputeSuperpoly(new int[NumPublicVar], cube);

                if ((!(superpoly.SequenceEqual(nulSeq))))
                {
                    Console.WriteLine(GetLogMessage1(cube, superpoly));
                }
            }

            superpoly2 = new List<List<int>>();
            if (quadratic_test(new int[NumPublicVar], cube))
            {
                superpoly2 = ComputeSuperpoly2(new int[NumPublicVar], SecretVariableIndexes(new int[NumPublicVar], cube), cube);

                if (superpoly2[1].Sum() != 0)
                {
                    Console.WriteLine(GetLogMessage2(cube, superpoly2));
                }
            }
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
                case CubeAttackMode.setPublicBits:
                    SetPublicBitsPhase();
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

        public void SetPublicBits()
        {
            ProcessCubeAttack(CubeAttackMode.setPublicBits);
        }

        static public List<int> ComputeSuperpolyForBlockCipher(int[] pubVarElement, List<int> maxterm)
        {
            int constant = 0;
            int coeff = 0;
            List<int> superpoly = new List<int>();
            int[] secVarElement = new int[NumSecretParam];

            BigInteger key = Operation.BigInteger_W.FromHexToDec("0000000000000000");
            Present A = new Present(key);

            // Compute the free term such:
            // sume over selected cube, and other variables into 0;
            for (ulong i = 0; i < Math.Pow(2, maxterm.Count); i++)
            {
                for (int j = 0; j < maxterm.Count; j++)
                    pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                constant ^= OM.GetIBit(OM.BitCount(A.Encrypt(OM.GetBigIntFromIndexArray(pubVarElement))), 1);
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
                    key = OM.GetBigIntFromIndexArray(secVarElement);
                    A = new Present(key);
                    for (int j = 0; j < maxterm.Count; j++)
                        pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                    coeff ^= OM.GetIBit(OM.BitCount(A.Encrypt(OM.GetBigIntFromIndexArray(pubVarElement))), 1);
                }
                superpoly.Add(constant ^ coeff);


                coeff = 0;
                secVarElement[k] = 0;
            }
            return superpoly;
        }
    }
}