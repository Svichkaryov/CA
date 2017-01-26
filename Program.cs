using System;
using System.Collections.Generic;
using System.Text;
using Matrix;
using Vector;

namespace CubeAttack
{
    class Program
    {
        static private string outputKeyBits;
        static private int[] key = { 0, 1, 1 };

        private enum CubeAttackMode { preprocessing, online, setPublicBits };

        static public int NumLinearTest = 50; // number of testing function for linearity(BLR TEST)
        static public int NumSecretParam = 3; // number of secret param(lenght of key in the cipher implemention)
     
        static public int PublicVar = 3;

        static public int[] pubVarGlob = null;
        public int indexOutputBit = 1;

        static public Matrix.Matrix superpolyMatrix = null;
        static public List<List<int>> listCubeIndexes = null;
     
        /// <summary>
        /// Master polynom represent such: f(v,x) = v_0*v_1*x_0 + v_0*v_1*x_1 + v_2*x_0*x_2 + v_1*x_2 + v_0*x_0 + v_0*v_1+
        ///                                         +x_0*x_2 + v_1 + x_2+1 . 
        /// </summary>
        /// <param name="v">public input.</param>
        /// <param name="x">secret input.</param>
        /// <returns>Returns output bit, either 0 or 1.</returns>
        static private int func(int []v, int[] x)
        {
            return v[0]&v[1]&x[0] ^ v[0]&v[1]&x[1] ^ v[2]&x[0]&x[2] ^ v[1]&x[2] ^ 
                   v[0]&x[0]      ^ v[0]&v[1]      ^ x[0]&x[2]      ^ v[1]      ^ x[2]  ^ 1;
        }

        /// <summary>
        /// The function returns the black box output bit.
        /// </summary>
        /// <param name="v">Public variables.</param>
        /// <param name="x">Secret variables.</param>
        /// <returns>Returns the black box output bit, either 0 or 1.</returns>
        static public int black_box(int []v, int[]x)
        { 
            return func(v,x);
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
            int f_0 = black_box(v, (int[])secVarElement.Clone());

            for (int i=0; i < NumLinearTest; i++)
            {
                for (int j = 0; j < NumSecretParam; j++)
                {
                    x[j] = rnd.Next(0,2);
                    y[j] = rnd.Next(0, 2);
                    xy[j] = x[j] ^ y[j];
                }

                //Fix the public inputs not in the set of cube I to zero and for other 
                // we put in all state(i.e in 2^(cube.size))
                for (ulong k = 0; k < Math.Pow(2,maxterm.Count); k++)
                {
                    for (int b = 0; b < maxterm.Count; b++)
                        v[maxterm[b]] = (k & ((ulong)1 << b)) > 0 ? 1 : 0;
                    res += black_box(v, x) ^ black_box(v, y) ^ black_box(v, xy) ^ f_0;
                }

                if (res == 0) continue;
                else return false;
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
            for(ulong i = 0; i < Math.Pow(2, maxterm.Count); i++)
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
        static string GetLogMessage(List<int> cubeIndexes, List<int> superpoly, int? value = null)
        {
            cubeIndexes.Sort();

            return "Superpoly: " + SuperpolyAsString(superpoly) + ((value != null) ? " = " + value : "") +
                " \tCube indexes: {" + string.Join(",", cubeIndexes) + "}" + "\n";
        }

        /// <summary>
        /// The function outputs the key bits.
        /// </summary>
        /// <param name="res">Result vector</param>
        static public void OutputKey(Vector.Vector res)
        {
            StringBuilder output = new StringBuilder(string.Empty);
            for (int i = 0; i < res.Length; i++)
                output.AppendLine("x" + i + " = " + res[i]);
            outputKeyBits = output.ToString();
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
        public void PreprocessingPhase()
        {

        }

        /// <summary>
        /// Online phase of the cube attack.
        /// </summary>
        public void OnlinePhase()
        {

        }

        /// <summary>
        /// Does the actual CubeAttack processing
        /// </summary>
        private void ProcessCubeAttack(CubeAttackMode mode)
        {
            switch (mode)
            {
                case CubeAttackMode.preprocessing:
                  //  PreprocessingPhase();
                    break;
                case CubeAttackMode.online:
                   // OnlinePhase();
                    break;
                case CubeAttackMode.setPublicBits:
                  //  SetPublicBitsPhase();
                    break;
            }
        }


        static void Main(string[] args)
        {
            int[] maxterm = {2};
            int[] comp = new int[3];
            for (ulong i = 0; i < Math.Pow(2, maxterm.Length); i++)
            {
                for (int j = 0; j < maxterm.Length; j++)
                {
                    comp[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                  
                }
                for (int k = 0; k < 3; k++)
                {

                    Console.WriteLine(comp[k]);
                }
                Console.WriteLine('\n');
            }
                Console.ReadLine();
        }
    }
}
