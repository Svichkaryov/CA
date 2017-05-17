//#define FULL
#define ITERATE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using OM = Operation.MathHelper;


namespace NCubeAttack
{
    /// <summary>
    /// Class for attack on stream ciphers
    /// </summary>
    class CCubeAttack
    {
        static private string outputKeyBits = null;
        static public List<List<int>> listCubeIndexes1 = null;
        static public List<List<int>> listCubeIndexes2 = null;
        static public List<List<int>> listCubeIndexesTest = null;
        static public Matrix.Matrix superpolyMatrix = null;
        static public Vector.Vector outputBits = null;

        static public int BlackBoxID = 0;
        static public CubeAttackSettings settings;

        static private int[] Testkey = { 1, 0, 1 };

        /// <summary>
        /// Master polynom represent such: f(v,x) = ....
        /// </summary>
        /// <param name="v">public input.</param>
        /// <param name="x">secret input.</param>
        /// <returns>Returns output bit, either 0 or 1.</returns>
        static private int TestFunc(int[] v, int[] x)
        {
            return (v[0] & v[1] & x[0]) ^ (v[0] & v[1] & x[2]) ^ (v[1] & x[2]) ^ (v[1] & x[1]) ^ v[0] & x[0] ^
                   v[0] & v[1] ^ (v[2] & x[1] & x[0]) ^ v[1] ^ x[1] ^ 1;
        }

        public CCubeAttack(CubeAttackSettings.CipherName cipher)
        {
            switch (cipher)
            {
                case CubeAttackSettings.CipherName.test:
                    BlackBoxID = 0;
                    settings = new CubeAttackSettings();
                    break;

                case CubeAttackSettings.CipherName.present:
                    BlackBoxID = 1;
                    settings = new CubeAttackSettings();
                    break;

                case CubeAttackSettings.CipherName.led:
                    BlackBoxID = 2;
                    settings = new CubeAttackSettings();
                    break;

                case CubeAttackSettings.CipherName.speck32_64:
                    BlackBoxID = 3;
                    settings = new CubeAttackSettings();
                    break;
            }
        }

        static private void ProcessCubeAttack(CubeAttackSettings.CubeAttackMode mode)
        {
            switch (mode)
            {
                case CubeAttackSettings.CubeAttackMode.preprocessing:
                    PreprocessingPhase();
                    break;

                case CubeAttackSettings.CubeAttackMode.online:
                    OnlinePhase();
                    break;
                case CubeAttackSettings.CubeAttackMode.setPublicBits:
                    SetPublicBitsPhase();
                    break;
            }
        }

        static public void Preprocessing()
        {
            ProcessCubeAttack(CubeAttackSettings.CubeAttackMode.preprocessing);
        }

        static public void Online()
        {
            ProcessCubeAttack(CubeAttackSettings.CubeAttackMode.online);
        }

        static public void UserMode()
        {
            ProcessCubeAttack(CubeAttackSettings.CubeAttackMode.setPublicBits);
        }

        /// <summary>
        /// Function output 1 bit.
        /// </summary>
        /// <param name="v">public variables</param>
        /// <param name="x">secret variables</param>
        /// <returns></returns>
        static public int BlackBox(int[] v, int[] x)
        {
            switch (BlackBoxID)
            {
                case 0: // Test
                    return TestFunc(v, x);

                case 1: // Present
                    BigInteger key = OM.GetBigIntFromIndexArrayFromMSB(x);
                    Present A      = new Present(key);
                    return OM.GetIBit(OM.BitCount(A.Encrypt(OM.GetBigIntFromIndexArrayFromMSB(v))), 2);

                case 2:
                    ushort[] keySpeck   = new ushort[4] { 0, 0, 0, 0 };
                    ushort[] plaintext  = new ushort[2];
                    ushort[] ciphertext = new ushort[2];
                    keySpeck            = OM.ConvertFromBoolVectorToByteArray(x, 16);
                    plaintext           = OM.ConvertFromBoolVectorToByteArray(v, 16);
                    SpeckCipher.speck_block(plaintext, keySpeck, ciphertext);
                    return OM.GetIBit(OM.BitCount(ciphertext[0]) + OM.BitCount(ciphertext[1]), 0);

                case 3:

                    return 1;

                default:
                    break;
            }
            return 0;
        }

        /// <summary>
        /// Test for linearity of superpoly (BLR linearity test).
        /// </summary>
        /// <param name="v">public variable.</param>
        /// <returns>A boolean value indicating if the superpoly is probably linear or not.</returns>
        static public bool LinearityTest(int[] v, List<int> maxterm)
        {
            int[] x = OM.RandomGenerator(settings.NumPublicVar);
            int[] y = OM.RandomGenerator(settings.NumPublicVar);
            int[] xy = new int[settings.NumPublicVar];
            int[] secVarElement = new int[settings.NumPublicVar];
            int res = 0;
            ulong cardinalDegree = (ulong)Math.Pow(2, maxterm.Count);

            for (int i = 0; i < settings.NumLinearTest; i++)
            {
                for (int j = 0; j < settings.NumPublicVar; j++)
                    xy[j] = x[j] ^ y[j];

                //Fix the public inputs not in the set of cube I to zero and for other
                //we put in all state(i.e in 2^(cube.size))
                for (ulong k = 0; k < cardinalDegree; k++)
                {
                    for (int b = 0; b < maxterm.Count; b++)
                        v[maxterm[b]] = (k & ((ulong)1 << b)) > 0 ? 1 : 0;
                    res ^= BlackBox(v, x) ^ BlackBox(v, y) ^ BlackBox(v, xy) ^ BlackBox(v, (int[])secVarElement.Clone());
                }

                if (res == 0) return true;
                if (res == 1) return false;
            }

            return true;
        }


        static public bool QuadraticTest(int[] v, List<int> maxterm)
        {
            int[] x = OM.RandomGenerator(settings.NumPublicVar);
            int[] y = OM.RandomGenerator(settings.NumPublicVar);
            int[] z = OM.RandomGenerator(settings.NumPublicVar);
            int[] xy = new int[settings.NumPublicVar];
            int[] xz = new int[settings.NumPublicVar];
            int[] zy = new int[settings.NumPublicVar];
            int[] secVarElement = new int[settings.NumPublicVar];
            int res = 0;
            ulong cardinalDegree = (ulong)Math.Pow(2, maxterm.Count);

            for (int i = 0; i < settings.NumQuadraticTest; i++)
            {
                for (int j = 0; j < settings.NumPublicVar; j++)
                {
                    xy[j] = x[j] ^ y[j];
                    xz[j] = x[j] ^ z[j];
                    zy[j] = z[j] ^ y[j];
                }

                //Fix the public inputs not in the set of cube I to zero and for other
                //we put in all state(i.e in 2^(cube.size))
                for (ulong k = 0; k < cardinalDegree; k++)
                {
                    for (int b = 0; b < maxterm.Count; b++)
                        v[maxterm[b]] = (k & ((ulong)1 << b)) > 0 ? 1 : 0;
                    res ^= BlackBox(v, x) ^ BlackBox(v, y) ^ BlackBox(v, z) ^ BlackBox(v, xy)
                         ^ BlackBox(v, xz) ^ BlackBox(v, zy) ^ BlackBox(v, (int[])secVarElement.Clone());
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
            int[] secVarElement = new int[settings.NumSecretParam];
            ulong cardinalDegree = (ulong)Math.Pow(2, maxterm.Count);

            // Compute the free term such:
            // sume over selected cube, and other variables into 0;
            for (ulong i = 0; i < cardinalDegree; i++)
            {
                for (int j = 0; j < maxterm.Count; j++)
                    pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                constant ^= BlackBox((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
            }
            superpoly.Add(constant);

            // Compute coefficients such:
            // sume over selected cube inputing other variables into 0 +
            // sume over selected cube with other variables into 0, except x_j place.
            for (int k = 0; k < settings.NumSecretParam; k++)
            {
                secVarElement[k] = 1;

                for (ulong i = 0; i < cardinalDegree; i++)
                {
                    for (int j = 0; j < maxterm.Count; j++)
                        pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                    coeff ^= BlackBox((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
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
            int[] y = new int[settings.NumSecretParam];
            int res = 0;
            int NumOfRandomSample = 50;
            ulong cardinalDegree = (ulong)Math.Pow(2, maxterm.Count);
            List<int> LSecretVariableIndexes = new List<int>();

            for (int i = 0; i < settings.NumSecretParam; i++)
            {
                for (int k = 0; k < NumOfRandomSample; k++)
                {
                    y = OM.RandomGenerator(settings.NumSecretParam);
                    y[i] = 0;

                    for (ulong l = 0; l < cardinalDegree; l++)
                    {
                        for (int b = 0; b < maxterm.Count; b++)
                            pubVarElement[maxterm[b]] = (l & ((ulong)1 << b)) > 0 ? 1 : 0;
                        res ^= BlackBox(pubVarElement, y);
                        y[i] = 1;
                        res ^= BlackBox(pubVarElement, y);
                    }

                    if (res == 1)
                    {
                        LSecretVariableIndexes.Add(i);
                        res = 0;
                        break;
                    }
                }
            }

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
            int[] secVarElement = new int[settings.NumSecretParam];
            int res = 0;

            // for K -- 1-demension (amount = binom_coeff(1,SVI.Count()))
            int[] yr0 = new int[settings.NumSecretParam];
            int[] yr1 = new int[settings.NumSecretParam];

            for (int r1 = 0; r1 < SVI.Count(); r1++)
            {
                //yr0[r1] = 0;
                yr1[SVI[r1]] = 1;

                for (ulong l = 0; l < Math.Pow(2, I.Count); l++)
                {
                    for (int b = 0; b < I.Count; b++)
                        v[I[b]] = (l & ((ulong)1 << b)) > 0 ? 1 : 0;
                    res ^= BlackBox(v, yr0) ^ BlackBox(v, yr1);
                }

                if (res == 1)
                {
                    ListOfTerm[0].Add(SVI[r1]);
                    ListOfTerm[1].Add(0);
                    res = 0;
                }

                yr1[SVI[r1]] = 0;
            }


            // for K -- 2-demension (amount = binom_coeff(2,SVI.Count()))
            int[] yr00 = new int[settings.NumSecretParam];
            int[] yr01 = new int[settings.NumSecretParam];
            int[] yr10 = new int[settings.NumSecretParam];
            int[] yr11 = new int[settings.NumSecretParam];


            for (int r2_1 = 0; r2_1 < SVI.Count() - 1; r2_1++)
            {
                for (int r2_2 = r2_1 + 1; r2_2 < SVI.Count; r2_2++)
                {
                    yr01[SVI[r2_2]] = 1;
                    yr10[SVI[r2_1]] = 1;
                    yr11[SVI[r2_1]] = 1;
                    yr11[SVI[r2_2]] = 1;

                    for (ulong l = 0; l < Math.Pow(2, I.Count); l++)
                    {
                        for (int b = 0; b < I.Count; b++)
                            v[I[b]] = (l & ((ulong)1 << b)) > 0 ? 1 : 0;
                        res ^= BlackBox(v, yr00) ^ BlackBox(v, yr01)
                             ^ BlackBox(v, yr10) ^ BlackBox(v, yr11);
                    }

                    if (res == 1)
                    {
                        ListOfTerm[0].Add(SVI[r2_1]);
                        ListOfTerm[1].Add(SVI[r2_2]);
                        res = 0;
                    }

                    yr01[SVI[r2_2]] = 0;
                    yr10[SVI[r2_1]] = 0;
                    yr11[SVI[r2_1]] = 0;
                    yr11[SVI[r2_2]] = 0;
                }
            }

            //Compute constant
            for (ulong l = 0; l < Math.Pow(2, I.Count); l++)
            {
                for (int b = 0; b < I.Count; b++)
                    v[I[b]] = (l & ((ulong)1 << b)) > 0 ? 1 : 0;
                res ^= BlackBox(v, (int[])secVarElement.Clone());
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
        static public string Superpoly2AsString(List<List<int>> superpoly2)
        {
            List<string> sp = new List<string>();

            for (int i = 0; i < superpoly2[0].Count(); i++)
            {
                if (superpoly2[0][i] != 0 & superpoly2[1][i] == 0) sp.Add(i == superpoly2[0].Count() ? "1" : "x" + superpoly2[0][i]);
                if (superpoly2[1][i] != 0) sp.Add(i == superpoly2[0].Count() ? "1" : "x" + superpoly2[0][i] + "*x" + superpoly2[1][i]);
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
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            superpolyMatrix = new Matrix.Matrix(0, settings.NumSecretParam + 1);
            listCubeIndexes1 = new List<List<int>>();
            listCubeIndexes2 = new List<List<int>>();
            var nulSeq = new List<int>();
            for (int i = 0; i < settings.NumPublicVar + 1; i++)
            {
                nulSeq.Add(0);
            }

            int maxCubeSize = settings.NumPublicVar;
            double numOfSubsets = Math.Pow(2, settings.NumPublicVar);
            int lci1_size = 1;
            int lci2_size = 0;

          
            using (StreamWriter sw = new StreamWriter(Param.Path.PathToTheFolderResult + "preprocessingPhaseResult1" + ".txt", false, Encoding.Default))
            {
                using (StreamWriter sw2 = new StreamWriter(Param.Path.PathToTheFolderResult + "preprocessingPhaseResult2" + ".txt", false, Encoding.Default))
                {
                    while (lci1_size < maxCubeSize + 1)   // find lci_size'th cube
                    {
#if (FULL)
                        
                        // iterate through all the cubes
                        for (int i = 1; i < numOfSubsets; i++)
                        {
                            // cube formation(brute force)
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
                            if (LinearityTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                            {
                                superpoly = ComputeSuperpoly(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]);
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

                                //superpoly2 = new List<List<int>>();
                                //if (QuadraticTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                                //{
                                //    superpoly2 = ComputeSuperpoly2(new int[settings.NumPublicVar], SecretVariableIndexes(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]),
                                //                                   listCubeIndexes1[lci1_size - 1]);
                                //    if (superpoly2[1].Sum() != 0)
                                //    {
                                //        Console.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));
                                //        sw2.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));

                                //        listCubeIndexes2.Add(new List<int>());
                                //        foreach (var el in listCubeIndexes1[lci1_size - 1])
                                //            listCubeIndexes2[lci2_size].Add(el);
                                //        lci2_size++;
                                //    }
                                //}

                                listCubeIndexes1.RemoveAt(lci1_size - 1);
                                continue;
                            }

                            //superpoly2 = new List<List<int>>();
                            //if (QuadraticTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                            //{
                            //    superpoly2 = ComputeSuperpoly2(new int[settings.NumPublicVar], SecretVariableIndexes(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]),
                            //                                   listCubeIndexes1[lci1_size - 1]);
                            //    if (superpoly2[1].Sum() != 0)
                            //    {
                            //        Console.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));
                            //        sw2.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));

                            //        listCubeIndexes2.Add(new List<int>());
                            //        foreach (var el in listCubeIndexes1[lci1_size - 1])
                            //            listCubeIndexes2[lci2_size].Add(el);
                            //        lci2_size++;
                            //    }
                            //}

                            listCubeIndexes1.RemoveAt(lci1_size - 1);
                            continue;
                        }

#endif

#if (ITERATE)

                        long cp_i = 1; // current permutation of bits 
                        long cp = 1;
                        long np = 0; // next permutation of bits
                        long t = 0;

                        for (int i = 1; i < settings.NumPublicVar + 1; i++)
                        {
                            for (int j = 0; j < OM.combination(settings.NumPublicVar, i); j++)
                            {
                                // Console.WriteLine(cp);

                                //---------------------------------------------

                                // cube formation(in lexicographically order)
                                listCubeIndexes1.Add(new List<int>());

                                for (int b = 0; b < Math.Log(cp) / Math.Log(2) + 1; b++)  // to adding into list in right order(for beauty).(But not necessarily).
                                {
                                    if (((cp >> b) & 1) == 1)      // getting the j-th bit of i (right to left).
                                    {
                                        listCubeIndexes1[lci1_size - 1].Add(maxCubeSize - b - 1);    // believe that indexing cubes from left to right.
                                    }
                                }
                                
                                //Console.WriteLine("Computing for such cube: {" + string.Join(",", listCubeIndexes1[lci1_size - 1]) + "}");

                                var superpoly = new List<int>();
                                var superpoly2 = new List<List<int>>();
                                if (LinearityTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                                {
                                    superpoly = ComputeSuperpoly(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]);
                                    if ((!(superpoly.SequenceEqual(nulSeq))) & (!InMatrix(superpoly, superpolyMatrix)))
                                    {
                                        superpolyMatrix = superpolyMatrix.AddRow(superpoly);
                                        if (!IsLinearIndependent(superpolyMatrix))
                                        {
                                            superpolyMatrix = superpolyMatrix.DeleteLastRow();
                                            listCubeIndexes1.RemoveAt(lci1_size - 1);

                                            t = (cp | (cp - 1)) + 1;
                                            np = (t | ((((t & -t) / (cp & -cp)) >> 1) - 1));
                                            cp = np;

//                                            Console.WriteLine("bad cube");
//                                            Console.WriteLine();
                                            continue;
                                        }

                                        Console.WriteLine(GetLogMessage1(listCubeIndexes1[lci1_size - 1], superpoly));
                                        sw.WriteLine(GetLogMessage1(listCubeIndexes1[lci1_size - 1], superpoly));

                                        t = (cp | (cp - 1)) + 1;
                                        np = (t | ((((t & -t) / (cp & -cp)) >> 1) - 1));
                                        cp = np;

                                        lci1_size++;
                                        continue;
                                    }

                                    //superpoly2 = new List<List<int>>();
                                    //if (QuadraticTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                                    //{
                                    //    superpoly2 = ComputeSuperpoly2(new int[settings.NumPublicVar], SecretVariableIndexes(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]),
                                    //                                   listCubeIndexes1[lci1_size - 1]);
                                    //    if (superpoly2[1].Sum() != 0)
                                    //    {
                                    //        Console.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));
                                    //        sw2.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));

                                    //        listCubeIndexes2.Add(new List<int>());
                                    //        foreach (var el in listCubeIndexes1[lci1_size - 1])
                                    //            listCubeIndexes2[lci2_size].Add(el);
                                    //        lci2_size++;
                                    //    }
                                    //}

                                    listCubeIndexes1.RemoveAt(lci1_size - 1);

                                    t = (cp | (cp - 1)) + 1;
                                    np = (t | ((((t & -t) / (cp & -cp)) >> 1) - 1));
                                    cp = np;

//                                    Console.WriteLine("bad cube");
  //                                  Console.WriteLine();
                                    continue;
                                }

                                //superpoly2 = new List<List<int>>();
                                //if (QuadraticTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                                //{
                                //    superpoly2 = ComputeSuperpoly2(new int[settings.NumPublicVar], SecretVariableIndexes(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]),
                                //                                   listCubeIndexes1[lci1_size - 1]);
                                //    if (superpoly2[1].Sum() != 0)
                                //    {
                                //        Console.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));
                                //        sw2.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));

                                //        listCubeIndexes2.Add(new List<int>());
                                //        foreach (var el in listCubeIndexes1[lci1_size - 1])
                                //            listCubeIndexes2[lci2_size].Add(el);
                                //        lci2_size++;
                                //    }
                                //}

                                listCubeIndexes1.RemoveAt(lci1_size - 1);

                                t = (cp | (cp - 1)) + 1;
                                np = (t | ((((t & -t) / (cp & -cp)) >> 1) - 1));
                                cp = np;

                                continue;
                            }

                            cp_i = cp_i * 2 + 1;
                            cp = cp_i;
                        }
#endif
                    }
                    sw2.Close();
                }
                sw.Close();
            } 
        

        

            Operation.Serialize_W.serialize_w(superpolyMatrix, "superpolyMatrix");
            Operation.Serialize_W.serialize_w(listCubeIndexes1, "cubeIndexes1");

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
        }

  
        /// <summary>
        /// Online phase of the cube attack.
        /// </summary>
        static public void OnlinePhase()
        {
            outputBits = new Vector.Vector(settings.NumSecretParam);
            var superpolyMatrixWithoutConst = new Matrix.Matrix(settings.NumSecretParam, settings.NumSecretParam);
            int[] pubVarElement = new int[settings.NumPublicVar];

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
                    outputBits[i] ^= BlackBox(pubVarElement, settings.GetKey());
                }
                outputBits[i] ^= superpolyMatrix[i, 0];
            }

            var _key = new Vector.Vector(settings.NumSecretParam);
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
            for (int i = 0; i < settings.NumPublicVar + 1 ; i++)
            {
                nulSeq.Add(0);
            }

            var superpoly = new List<int>();
            var superpoly2 = new List<List<int>>();
            if (LinearityTest(new int[settings.NumPublicVar], cube))
            {
                superpoly = ComputeSuperpoly(new int[settings.NumPublicVar], cube);

                if ((!(superpoly.SequenceEqual(nulSeq))))
                {
                    Console.WriteLine(GetLogMessage1(cube, superpoly));
                }
            }

            superpoly2 = new List<List<int>>();
            if (QuadraticTest(new int[settings.NumPublicVar], cube))
            {
                superpoly2 = ComputeSuperpoly2(new int[settings.NumPublicVar], SecretVariableIndexes(new int[settings.NumPublicVar], cube), cube);

                if (superpoly2[1].Sum() != 0)
                {
                    Console.WriteLine(GetLogMessage2(cube, superpoly2));
                }
            }
        }
      
     
        static BigInteger KEY = Operation.BigInteger_W.FromHexToDec("AF324");
        static Present Abonent = new Present(KEY);
        static public void OnlinePhase2()
        {
            //outputBits = new Vector.Vector(NumSecretParam);
            int[] outpu = new int[4];
            listCubeIndexesTest = new List<List<int>>();
           // var superpolyMatrixWithoutConst = new Matrix.Matrix(NumSecretParam, NumSecretParam);
            int[] pub = new int[settings.NumPublicVar];

            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 46, 44 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 56, 51, 50, 48 });
            listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 57, 51, 50, 48 });
            
            listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 51, 49 });
            listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 54, 52 });
            listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 50, 48 });



            //   superpolyMatrixWithoutConst = superpolyMatrix.DeleteFirstColumn();
                    
            for (int i = 0; i < listCubeIndexesTest.Count(); i++)
            {
                for (int g = 0; g < settings.NumPublicVar; g++)
                    pub[g] = 0;
                for (ulong j = 0; j < Math.Pow(2, listCubeIndexesTest[i].Count()); j++)
                {
                    for (int k = 0; k < listCubeIndexesTest[i].Count(); k++)
                        pub[listCubeIndexesTest[i][k]] = (j & ((ulong)1 << k)) > 0 ? 1 : 0;
                    outpu[i] ^=  OM.GetIBit(OM.BitCount(Abonent.Encrypt(OM.GetBigIntFromIndexArrayFromMSB(pub))), 2);
                }
             //   outputBits[i] ^= superpolyMatrix[i, 0];
                Console.WriteLine("{0} iter: {1}",i,outpu[i]);
               
            }

          //  var _key = new Vector.Vector(NumSecretParam);
          //  _key = superpolyMatrixWithoutConst.Inverse() * outputBits;

//            Console.WriteLine(OutputKey(_key));
        }


    }
}