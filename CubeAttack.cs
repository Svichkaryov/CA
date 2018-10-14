//#define FULL
#define ITERATE
#define LEXICOGRAPHICALLY_ORDER
//#define RANDOM_WALK
#define QUADRATIC
//#define RECORD

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
        static private string outputKeyBits               = null;
        static public List<List<int>> listCubeIndexes1    = null;
        static public List<List<int>> listCubeIndexes2    = null;
        static public List<List<int>> listCubeIndexesTest = null;
        static public Matrix.Matrix superpolyMatrix       = null;
        static public Vector.Vector outputBits            = null;

        static public int BlackBoxID = 0;
        static public CubeAttackSettings settings;

        /// <summary>
        /// Master polynom represent such: f(v,x) = ....
        /// </summary>
        /// <param name="v">public input.</param>
        /// <param name="x">secret input.</param>
        /// <returns>Returns output bit, either 0 or 1.</returns>
        static private int TestFunc(int[] v, int[] x)
        {
            return (v[0] & v[1] & x[0]) ^ (v[0] & v[1] & x[2]) ^ (v[1] & x[2]) ^ (v[1] & x[1]) ^ v[0] & x[0] ^
                   v[0] & v[1] ^ (v[2] & v[1] & x[1] & x[0]) ^ (v[2] & v[1] & x[0] & x[2]) ^
                   (v[2] & v[1]) ^ v[1] ^ x[1] ^ 1;
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

                case CubeAttackSettings.CipherName.speck32_64:
                    BlackBoxID = 2;
                    settings = new CubeAttackSettings();
                    break;

                case CubeAttackSettings.CipherName.led:
                    BlackBoxID = 3;
                    settings = new CubeAttackSettings();
                    break;

                case CubeAttackSettings.CipherName.idea:
                    BlackBoxID = 4;
                    settings = new CubeAttackSettings();
                    break;

                case CubeAttackSettings.CipherName.midori:
                    BlackBoxID = 5;
                    settings = new CubeAttackSettings();
                    break;

                case CubeAttackSettings.CipherName.piccolo80:
                    BlackBoxID = 6;
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
                    Present A = new Present(key);
                    return OM.GetIBit(OM.BitCount(A.Encrypt(OM.GetBigIntFromIndexArrayFromMSB(v))), 1);

                case 2: // Speck
                    ushort[] keySpeck = new ushort[4] { 0, 0, 0, 0 };
                    ushort[] plaintext = new ushort[2];
                    ushort[] ciphertext = new ushort[2];
                    keySpeck = OM.ConvertFromBoolVectorToByteArray(x, 16);
                    plaintext = OM.ConvertFromBoolVectorToByteArray(v, 16);
                    SpeckCipher.speck_block(plaintext, keySpeck, ciphertext);
                    return OM.GetIBit(OM.BitCount(ciphertext[0]) + OM.BitCount(ciphertext[1]), 1);
                  //  return OM.GetIBit(OM.BitCount(ciphertext[0]) + OM.BitCount(ciphertext[1]), 1);
                         

                case 3: // Led
                    int[] p = new int[8];
                    int[] c = new int[8];
                    int[] k = new int[16];
                    p = OM.ConvertFromBoolVectorToIntArray(v, 8);
                    k = OM.ConvertFromBoolVectorToIntArray(x, 8);
                    LedCipher.LED_enc(c, k, settings.NumSecretParam);
                    return OM.GetIBit(OM.BitCount(c[0])+ OM.BitCount(c[1])+ OM.BitCount(c[2])+ OM.BitCount(c[3])+
                                      OM.BitCount(c[4]) + OM.BitCount(c[5]) + OM.BitCount(c[6])+ OM.BitCount(c[7]), 0);

                case 4: //IDEA
                    ushort[] state = { 0, 0, 0, 0 };
                    ushort[] IdeaK = { 0, 0, 0, 0, 0, 0, 0, 0 };
                    IdeaK = OM.ConvertFromBoolVectorToByteArray(x, 16);
                    state = OM.ConvertFromBoolVectorToByteArray(v, 16);
                    ushort[] subkey = new ushort[52];
                    IDEACipher.KeyScheduleEncrypt(IdeaK, subkey);
                    IDEACipher.Encrypt(state, subkey);
                    return OM.GetIBit(OM.BitCount(state[0]),0);

                case 5: //Midori
                    byte[] s = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    byte[] K = new byte[32] { 0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0
                                             ,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0 };
                    s = OM.ConvertFromBoolVectorToByteNibbleArray(v, 4);
                    K = OM.ConvertFromBoolVectorToByteNibbleArray(x, 4);
                    CMidori.Encrypt(1, s, K);
                    return OM.GetIBit(OM.BitCount(s[0]) + OM.BitCount(s[1]) 
                     //   +  OM.BitCount(s[2]) + OM.BitCount(s[3]) +
                     //   OM.BitCount(s[4]) + OM.BitCount(s[5])+
                      //  OM.BitCount(s[6]) + OM.BitCount(s[7])+
                      //  OM.BitCount(s[8]) + OM.BitCount(s[9])+
                      //  OM.BitCount(s[10]) + OM.BitCount(s[11])+
                      //  OM.BitCount(s[12]) + OM.BitCount(s[13]) +
                      //  OM.BitCount(s[14]) + OM.BitCount(s[15])
                      , 1);

                case 6: // Piccolo
                    ushort[] text = { 0, 0, 0, 0 };
                    ushort[] k80 = { 0, 0, 0, 0, 0 };
                    ushort[] wk = new ushort[4];
                    ushort[] rk = new ushort[2 * 1];
                    text = OM.ConvertFromBoolVectorToByteArray(v, 16);
                    k80 = OM.ConvertFromBoolVectorToByteArray(x, 16);

                    CPiccolo.wKS_80(k80, wk);
                    CPiccolo.rKS_80(k80, rk);
                    return OM.GetIBit(OM.BitCount(text[1], 1), 3);
                    
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
            int[] x = new int[settings.NumSecretParam];
            int[] y = new int[settings.NumSecretParam];
            int[] xy = new int[settings.NumSecretParam];
            int[] secVarElement = new int[settings.NumSecretParam];
            int res = 0;
            ulong cardinalDegree = (ulong)Math.Pow(2, maxterm.Count);
            Encoding enc = Encoding.GetEncoding(1251);
            Random random = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < settings.NumLinearTest; i++)
            {
                random = new Random(DateTime.Now.Millisecond);
                // x = OM.RandomGenerator(settings.NumSecretParam);
                // y = OM.RandomGenerator(settings.NumSecretParam);

                for (int w = 0; w < settings.NumSecretParam; w++)
                {
                    random = new Random(DateTime.Now.Millisecond + w);
                    x[w] = random.Next(0, 2);
                }

                for (int wq = 0; wq < settings.NumSecretParam; wq++)
                {
                    random = new Random(DateTime.Now.Millisecond + wq);
                    y[wq] = random.Next(0, 2);
                }

                for (int j = 0; j < settings.NumSecretParam; j++)
                    xy[j] = x[j] ^ y[j];

                //Fix the public inputs not in the set of cube I to zero and for other
                //we put in all state(i.e in 2^(cube.size))
                for (ulong k = 0; k < cardinalDegree; k++)
                {
                    for (int b = 0; b < maxterm.Count; b++)
                        v[maxterm[b]] = (k & ((ulong)1 << b)) > 0 ? 1 : 0;
                    res ^= BlackBox(v, x) ^ BlackBox(v, y) ^ BlackBox(v, xy) ^ BlackBox(v, secVarElement);
                }

                if (res == 1) return false;
            }

            return true;
        }

        static public bool LinearityTest2(int[] v, List<int> maxterm, List<int> I)
        {
            int[] z = new int[settings.NumSecretParam];
            int[] y = new int[settings.NumSecretParam];
            int[] secVarElement = new int[settings.NumSecretParam];
            int res = 0;
            ulong cardinalDegree = (ulong)Math.Pow(2, maxterm.Count);

            var S = new List<int>();
            if (I.Count() == 0) return false;
            for(int j=1;j<I.Count();j++)
            {
                if (I[j] == 1)
                    S.Add(j-1);
            }

            foreach(var j in S)
            {
                for (int i = 0; i < settings.NumLinearTest; i++)
                {
                    y = OM.RandomGenerator(settings.NumSecretParam);
                    z = y;
                    res = 0;

                    y[j] = 0;
                    z[j] = 1;

                     //Fix the public inputs not in the set of cube I to zero and for other
                    //we put in all state(i.e in 2^(cube.size))
                    for (ulong k = 0; k < cardinalDegree; k++)
                    {
                        for (int b = 0; b < maxterm.Count; b++)
                            v[maxterm[b]] = (k & ((ulong)1 << b)) > 0 ? 1 : 0;
                        res ^= BlackBox(v, z) ^ BlackBox(v, y);
                    }

                    if (res == 0) return false;
                }
                
            }
            return true;
        }

        static public bool QuadraticTest(int[] v, List<int> maxterm)
        {
            int[] x = new int[settings.NumSecretParam]; 
            int[] y = new int[settings.NumSecretParam];
            int[] z = new int[settings.NumSecretParam];
            int[] xy = new int[settings.NumSecretParam];
            int[] xz = new int[settings.NumSecretParam];
            int[] yz = new int[settings.NumSecretParam];
            int[] xyz = new int[settings.NumSecretParam];
            int[] secVarElement = new int[settings.NumSecretParam];
            int res = 0;
            ulong cardinalDegree = (ulong)Math.Pow(2, maxterm.Count);
            Encoding enc = Encoding.GetEncoding(1251);
            Random random = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < settings.NumQuadraticTest; ++i)
            {
                //x = OM.RandomGenerator(settings.NumSecretParam);
                //y = OM.RandomGenerator(settings.NumSecretParam);
                //z = OM.RandomGenerator(settings.NumSecretParam);
                res = 0;

                random = new Random(DateTime.Now.Millisecond);

                for (int w = 0; w < settings.NumSecretParam; ++w)
                {
                    random = new Random(DateTime.Now.Millisecond + w);
                    x[w] = random.Next(0, 2);
                }

                for (int wq = 0; wq < settings.NumSecretParam; ++wq)
                {
                    random = new Random(DateTime.Now.Millisecond + wq);
                    y[wq] = random.Next(0, 2);
                }

                for (int wqq = 0; wqq < settings.NumSecretParam; ++wqq)
                {
                    random = new Random(DateTime.Now.Millisecond + wqq);
                    z[wqq] = random.Next(0, 2);
                }

                for (int j = 0; j < settings.NumSecretParam; j++)
                {
                    xy[j]  = x[j] ^ y[j];
                    xz[j]  = x[j] ^ z[j];
                    yz[j]  = y[j] ^ z[j];
                    xyz[j] = x[j] ^ y[j] ^ z[j];
                }

                //Fix the public inputs not in the set of cube I to zero and for other
                //we put in all state(i.e in 2^(cube.size))
                for (ulong k = 0; k < cardinalDegree; ++k)
                {
                    for (int b = 0; b < maxterm.Count; ++b)
                        v[maxterm[b]] = (k & ((ulong)1 << b)) > 0 ? 1 : 0;
                    res ^= BlackBox(v, x) ^ BlackBox(v, y) ^ BlackBox(v, z) ^ BlackBox(v, xy)
                         ^ BlackBox(v, xz) ^ BlackBox(v, yz) ^ BlackBox(v, xyz)
                         ^ BlackBox(v, secVarElement);
                }

                if (res == 1) return false;
            }
            return true;
        }

        static public bool ConstantTest(int[] v, List<int> maxterm)
        {
            int[] x = new int[settings.NumSecretParam];
            int flag = 0;
            int output = 0;
            int[] secVarElement = new int[settings.NumSecretParam];

            for (int i = 0; i < settings.NumConstTest; i++)
            {
                x = OM.RandomGenerator(settings.NumSecretParam);

                output = 0;

                for (ulong j = 0; j < Math.Pow(2, maxterm.Count); j++)
                {
                    for (int k = 0; k < maxterm.Count; k++)
                        v[maxterm[k]] = (j & ((ulong)1 << k)) > 0 ? 1 : 0;
                    output ^= BlackBox(v, x);
                }
                if (i == 0)
                    flag = output;
                if (flag != output)
                    return false;
                output = 0;
            }

            if (settings.NumConstTest > 0)
                return true;
            else
                return false;
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
                constant ^= BlackBox(pubVarElement, (int[])secVarElement.Clone());
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

            ulong cardinalDegree = (ulong)Math.Pow(2, maxterm.Count);
            List<int> LSecretVariableIndexes = new List<int>();
            Encoding enc = Encoding.GetEncoding(1251);
            Random random = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < settings.NumSecretParam; ++i)
            {
                for (int k = 0; k < settings.NumOfRandomSample; ++k)
                {
                    for (int wq = 0; wq < settings.NumSecretParam; ++wq)
                    {
                        random = new Random(DateTime.Now.Millisecond + wq);
                        y[wq] = random.Next(0, 2);
                    }

                    for (ulong l = 0; l < cardinalDegree; ++l)
                    {
                        for (int b = 0; b < maxterm.Count; ++b)
                            pubVarElement[maxterm[b]] = (l & ((ulong)1 << b)) > 0 ? 1 : 0;
                        y[i] = 0;
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
            int[] nul = new int[settings.NumSecretParam];
            int res = 0;

            // for K -- 1-demension (amount = binom_coeff(1,SVI.Count()))
            int[] yr0 = new int[settings.NumSecretParam];
            int[] yr1 = new int[settings.NumSecretParam];

            for (int r1 = 0; r1 < SVI.Count(); r1++)
            {
               // yr0[SVI[r1]] = 0;
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

            for (int i = 0; i < superpoly2[0].Count(); ++i)
            {
                if (superpoly2[0][i] != 0 & superpoly2[1][i] == 0) sp.Add(i != superpoly2[0].Count()-1 ? "x" + superpoly2[0][i] : "1");
                if (superpoly2[1][i] != 0) sp.Add("x" + superpoly2[0][i] + "*x" + superpoly2[1][i]);
            }
            return (sp.Count == 0) ? "0" : string.Join("+", sp);
        }

        public static string GetLogMessage1(List<int> cubeIndexes, List<int> superpoly, int? value = null)
        {
            // cubeIndexes.Sort();

            return "Superpoly: " + SuperpolyAsString(superpoly) + ((value != null) ? " = " + value : "") +
                   " \tCube indexes: {" + string.Join(",", cubeIndexes) + "}" + "\n";
        }

        public static string GetLogMessage2(List<int> cubeIndexes, List<List<int>> superpoly, int? value = null)
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

                           // Console.WriteLine("Computing for such cube: {" + string.Join(",", listCubeIndexes1[lci1_size - 1]) + "}");

                            var superpoly = new List<int>();
                            var superpoly2 = new List<List<int>>();
                        if (LinearityTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                        {
                            superpoly = ComputeSuperpoly(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]);
                            if ((!(superpoly.SequenceEqual(nulSeq))) & (!InMatrix(superpoly, superpolyMatrix)))
                            {
                                superpolyMatrix = superpolyMatrix.AddRow(superpoly);
#if (RECORD)
                                    superpolyMatrix = superpolyMatrix.AddRow(superpoly);
                                    if (!IsLinearIndependent(superpolyMatrix))
                                    {
                                        superpolyMatrix = superpolyMatrix.DeleteLastRow();
                                        listCubeIndexes1.RemoveAt(lci1_size - 1);
                                 //       Console.WriteLine("bad cube");
                                 //       Console.WriteLine();
                                        continue;
                                    }
#endif
                                //               Console.WriteLine(GetLogMessage1(listCubeIndexes1[lci1_size - 1], superpoly));
                                //                 sw.WriteLine(GetLogMessage1(listCubeIndexes1[lci1_size - 1], superpoly));


                                lci1_size++;
                                continue;
                            }
#if (QUADRATIC)
                            superpoly2 = new List<List<int>>();
                            if (QuadraticTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                            {
                                superpoly2 = ComputeSuperpoly2(new int[settings.NumPublicVar], SecretVariableIndexes(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]),
                                                               listCubeIndexes1[lci1_size - 1]);
                                if (superpoly2[1].Sum() != 0 && OM.BitCount(superpoly2[0]) > 0 && OM.BitCount(superpoly2[0]) < 4)
                                {
                                    Console.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));
                                    sw2.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));

                                    listCubeIndexes2.Add(new List<int>());
                                    foreach (var el in listCubeIndexes1[lci1_size - 1])
                                        listCubeIndexes2[lci2_size].Add(el);
                                    lci2_size++;
                                }
                            }
#endif
                            listCubeIndexes1.RemoveAt(lci1_size - 1);
                            continue;
                        }
#if (QUADRATIC)
                        superpoly2 = new List<List<int>>();
                            if (QuadraticTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                            {
                                superpoly2 = ComputeSuperpoly2(new int[settings.NumPublicVar], SecretVariableIndexes(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]),
                                                               listCubeIndexes1[lci1_size - 1]);
                                if (superpoly2[1].Sum() != 0 && OM.BitCount(superpoly2[0]) > 0 && OM.BitCount(superpoly2[0]) < 4)
                                {
                                    Console.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));
                                    sw2.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));

                                    listCubeIndexes2.Add(new List<int>());
                                    foreach (var el in listCubeIndexes1[lci1_size - 1])
                                        listCubeIndexes2[lci2_size].Add(el);
                                    lci2_size++;
                                }
                            }
#endif
                            listCubeIndexes1.RemoveAt(lci1_size - 1);
                            continue;
                        }

#endif

#if (ITERATE)

                    long cp_i = 1;   // current permutation of bits 
                    long cp = 1;
                    long np = 0;   // next permutation of bits
                    long t = 0;

                    for (int i = 1; i < settings.NumPublicVar + 1; i++)
                    {
                        Console.WriteLine(i);
#if (RECORD)
                        if (lci1_size == settings.NumSecretParam + 1)
                            break;
#endif
                        for (int j = 0; j < OM.combination(settings.NumPublicVar, i); j++)
                        {
                            // Console.WriteLine(cp);
#if (RECORD)
                            if (lci1_size == settings.NumSecretParam + 1)
                                break;
#endif

#if (LEXICOGRAPHICALLY_ORDER)                            
                            // cube formation(in lexicographically order)
                            listCubeIndexes1.Add(new List<int>());
                            
                            for (int b = 0; b < Math.Log(cp) / Math.Log(2) + 1; b++)  // to adding into list in right order(for beauty).(But not necessarily).
                            {
                                if (((cp >> b) & 1) == 1)      // getting the j-th bit of i (right to left).
                                {
                                    listCubeIndexes1[lci1_size - 1].Add(maxCubeSize - b - 1);    // believe that indexing cubes from left to right.
                                }
                            }
#endif
                            // Console.WriteLine("Computing for such cube: {" + string.Join(",", listCubeIndexes1[lci1_size - 1]) + "}");

                            var superpoly = new List<int>();
                            var superpoly2 = new List<List<int>>();
                            if (LinearityTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1])
                               //&& !ConstantTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1])
                               )
                            {
                                superpoly = ComputeSuperpoly(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]);
                                if (OM.BitCount(superpoly) <= 4 && OM.BitCount(superpoly) > 0
                                // && !ConstantTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1])
                                 && (!InMatrix(superpoly, superpolyMatrix))
                                     // && (!QuadraticTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                                     // && LinearityTest2(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1], superpoly)
                                     )
                                {
                                    superpolyMatrix = superpolyMatrix.AddRow(superpoly);

#if (RECORD)
                                    //if need search = n equations
                                    superpolyMatrix = superpolyMatrix.AddRow(superpoly);
                                    if (!IsLinearIndependent(superpolyMatrix))
                                    {
                                        superpolyMatrix = superpolyMatrix.DeleteLastRow();
                                        listCubeIndexes1.RemoveAt(lci1_size - 1);

                                        t = (cp | (cp - 1)) + 1;
                                        np = (t | ((((t & -t) / (cp & -cp)) >> 1) - 1));
                                        cp = np;

                                        //  Console.WriteLine("bad cube");
                                        //  Console.WriteLine();
                                        continue;
                                    }
#endif
                                    Console.WriteLine(GetLogMessage1(listCubeIndexes1[lci1_size - 1], superpoly));
                                    sw.WriteLine(GetLogMessage1(listCubeIndexes1[lci1_size - 1], superpoly));

                                    t = (cp | (cp - 1)) + 1;
                                    np = (t | ((((t & -t) / (cp & -cp)) >> 1) - 1));
                                    cp = np;

                                    lci1_size++;
                                    continue;
                                }
#if (QUADRATIC)
                                superpoly2 = new List<List<int>>();
                                if (QuadraticTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                                {
                                    superpoly2 = ComputeSuperpoly2(new int[settings.NumPublicVar], SecretVariableIndexes(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]),
                                                                   listCubeIndexes1[lci1_size - 1]);
                                    if (superpoly2[1].Sum() != 0 && OM.BitCount(superpoly2[0]) > 0)// && OM.BitCount(superpoly2[0]) < 4)
                                    {
                                        Console.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));
                                        sw2.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));

                                        listCubeIndexes2.Add(new List<int>());
                                        foreach (var el in listCubeIndexes1[lci1_size - 1])
                                            listCubeIndexes2[lci2_size].Add(el);
                                        lci2_size++;
                                    }
                                }
#endif
                                listCubeIndexes1.RemoveAt(lci1_size - 1);

                                t = (cp | (cp - 1)) + 1;
                                np = (t | ((((t & -t) / (cp & -cp)) >> 1) - 1));
                                cp = np;

                                //Console.WriteLine("bad cube");
                                //Console.WriteLine();
                                continue;
                            }
#if (QUADRATIC)
                            superpoly2 = new List<List<int>>();
                            if (QuadraticTest(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]))
                            {
                                superpoly2 = ComputeSuperpoly2(new int[settings.NumPublicVar], SecretVariableIndexes(new int[settings.NumPublicVar], listCubeIndexes1[lci1_size - 1]),
                                                               listCubeIndexes1[lci1_size - 1]);
                                if (superpoly2[1].Sum() != 0 && OM.BitCount(superpoly2[0]) > 0 && OM.BitCount(superpoly2[0]) < 5)
                                {
                                    Console.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));
                                    sw2.WriteLine(GetLogMessage2(listCubeIndexes1[lci1_size - 1], superpoly2));

                                    listCubeIndexes2.Add(new List<int>());
                                    foreach (var el in listCubeIndexes1[lci1_size - 1])
                                        listCubeIndexes2[lci2_size].Add(el);
                                    lci2_size++;
                                }
                            }
#endif
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
                    sw2.Close();
                }
                sw.Close();
            }

            Operation.Serialize_W.serialize_w(superpolyMatrix, "superpolyMatrix");
            Operation.Serialize_W.serialize_w(listCubeIndexes1, "cubeIndexes1");

            TimeSpan ts = stopWatch.Elapsed;

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

            for (int i = 0; i < settings.NumSecretParam; i++)
            {
                for (int j = 0; j < settings.NumSecretParam; j++)
                {
                    superpolyMatrixWithoutConst[i, j] = superpolyMatrix[i, j + 1];
                }
            }

            for (int i = 0; i < listCubeIndexes1.Count(); i++)
            {
                for (ulong j = 0; j < Math.Pow(2, listCubeIndexes1[i].Count); j++)
                {
                    for (int c = 0; c < settings.NumPublicVar; c++)
                        pubVarElement[c] = 0;

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
            string action;
            do
            {
                var cube = new List<int>();
                Console.WriteLine("Input index of cube separated by commas: ");
                var cube_str = Console.ReadLine();

                char separator = ',';
                string[] items = cube_str.Split(separator);
                foreach (var cubeIndex in items)
                    cube.Add(int.Parse(cubeIndex));

                var nulSeq = new List<int>();
                for (int i = 0; i < settings.NumPublicVar + 1; i++)
                {
                    nulSeq.Add(0);
                }

                var superpoly = new List<int>();
                var superpoly2 = new List<List<int>>();
                
                if (QuadraticTest(new int[settings.NumPublicVar], cube))
                {
                    if (LinearityTest(new int[settings.NumPublicVar], cube))
                    {
                        superpoly = ComputeSuperpoly(new int[settings.NumPublicVar], cube);

                        if ((!(superpoly.SequenceEqual(nulSeq))))
                        {
                            Console.WriteLine(GetLogMessage1(cube, superpoly));
                        }
                    }

                    superpoly2 = ComputeSuperpoly2(new int[settings.NumPublicVar], SecretVariableIndexes(new int[settings.NumPublicVar], cube), cube);
                    if (superpoly2[1].Sum() != 0)
                    {
                        Console.WriteLine(GetLogMessage2(cube, superpoly2));
                    }
                }
                Console.Write("action(g/e): ");
                action = Console.ReadLine();
            }
            while (action != "e");
        }

        static public void OnlinePhaseTest()
        {
            //outputBits = new Vector.Vector(NumSecretParam);
            int[] outpu = new int[240];
            listCubeIndexesTest = new List<List<int>>();
            // var superpolyMatrixWithoutConst = new Matrix.Matrix(NumSecretParam, NumSecretParam);
            int[] pub = new int[settings.NumPublicVar];

            // speck 1 round, hw(state) - 2st bit, coutt -16
            //listCubeIndexesTest.Add(new List<int> { 31, 14 });
            //listCubeIndexesTest.Add(new List<int> { 22, 15 });
            //listCubeIndexesTest.Add(new List<int> { 31, 0 });
            //listCubeIndexesTest.Add(new List<int> { 31, 1 });
            //listCubeIndexesTest.Add(new List<int> { 31, 2 });
            //listCubeIndexesTest.Add(new List<int> { 31, 3 });
            //listCubeIndexesTest.Add(new List<int> { 22, 4 });
            //listCubeIndexesTest.Add(new List<int> { 31, 5 });
            //listCubeIndexesTest.Add(new List<int> { 26, 6 });
            //listCubeIndexesTest.Add(new List<int> { 29, 7 });
            //listCubeIndexesTest.Add(new List<int> { 30, 8 });
            //listCubeIndexesTest.Add(new List<int> { 29, 9 });
            //listCubeIndexesTest.Add(new List<int> { 31, 10 });
            //listCubeIndexesTest.Add(new List<int> { 30, 11 });
            //listCubeIndexesTest.Add(new List<int> { 31, 12 });
            //listCubeIndexesTest.Add(new List<int> { 31, 13 });

            listCubeIndexesTest.Add(new List<int> { 0});
            listCubeIndexesTest.Add(new List<int> { 1 });
            listCubeIndexesTest.Add(new List<int> { 2 });
            listCubeIndexesTest.Add(new List<int> { 3 });
            listCubeIndexesTest.Add(new List<int> { 4 });
            listCubeIndexesTest.Add(new List<int> { 5 });
            listCubeIndexesTest.Add(new List<int> { 6 });
            listCubeIndexesTest.Add(new List<int> { 7 });
            listCubeIndexesTest.Add(new List<int> { 8 });
            listCubeIndexesTest.Add(new List<int> { 9 });
            listCubeIndexesTest.Add(new List<int> { 10 });
            listCubeIndexesTest.Add(new List<int> { 11 });
            listCubeIndexesTest.Add(new List<int> { 12 });
            listCubeIndexesTest.Add(new List<int> { 13 });
            listCubeIndexesTest.Add(new List<int> { 14 });
            listCubeIndexesTest.Add(new List<int> { 15 });
            listCubeIndexesTest.Add(new List<int> { 16 });
            listCubeIndexesTest.Add(new List<int> { 17 });
            listCubeIndexesTest.Add(new List<int> { 18 });
            listCubeIndexesTest.Add(new List<int> { 19 });
            listCubeIndexesTest.Add(new List<int> { 20 });
            listCubeIndexesTest.Add(new List<int> { 21 });
            listCubeIndexesTest.Add(new List<int> { 22 });
            listCubeIndexesTest.Add(new List<int> { 23 });
            listCubeIndexesTest.Add(new List<int> { 24 });
            listCubeIndexesTest.Add(new List<int> { 25 });
            listCubeIndexesTest.Add(new List<int> { 26 });
            listCubeIndexesTest.Add(new List<int> { 27 });
            listCubeIndexesTest.Add(new List<int> { 28 });
            listCubeIndexesTest.Add(new List<int> { 29 });
            listCubeIndexesTest.Add(new List<int> { 30 });
            listCubeIndexesTest.Add(new List<int> { 31 });
            //listCubeIndexesTest.Add(new List<int> { 30, 21, 20 });
            //listCubeIndexesTest.Add(new List<int> { 30, 25, 19 });
            //listCubeIndexesTest.Add(new List<int> { 30, 25, 17 });
            //listCubeIndexesTest.Add(new List<int> { 24, 9, 7 });
            //listCubeIndexesTest.Add(new List<int> { 21, 7, 5 });

            // Present 1 round 3 bit, 18 vector
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 3, 1 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 2, 0 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 6, 4 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 11, 9 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 10, 8 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 14, 12 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 19, 17 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 18, 16 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 22, 20 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 27, 25 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 26, 24 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 30, 28 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 35, 33 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 34, 32 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 38, 36 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 43, 41 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 42, 40 });
            //listCubeIndexesTest.Add(new List<int> { 62, 61, 60, 59, 58, 56, 46, 44 });


            for (int i = 0; i < listCubeIndexesTest.Count(); i++)
            {
                for (ulong j = 0; j < Math.Pow(2, listCubeIndexesTest[i].Count()); j++)
                {
                    for (int g = 0; g < settings.NumPublicVar; g++)
                        pub[g] = 0;

                    for (int k = 0; k < listCubeIndexesTest[i].Count(); k++)
                        pub[listCubeIndexesTest[i][k]] = (j & ((ulong)1 << k)) > 0 ? 1 : 0;
                    outpu[i] ^= BlackBox(pub, settings.GetKey());
                }
                //outputBits[i] ^= superpolyMatrix[i, 0];
               // Console.WriteLine("{0} iter: {1}", i, outpu[i]);
            }

            var superpoly = new List<int>();
            var superpoly2 = new List<List<int>>();

            for (int s = 0; s < listCubeIndexesTest.Count(); s++)
            {
                Console.WriteLine("s: ");
                if (LinearityTest(new int[settings.NumPublicVar], listCubeIndexesTest[s]))
                {
                    superpoly = ComputeSuperpoly(new int[settings.NumPublicVar], listCubeIndexesTest[s]);
                    Console.WriteLine(GetLogMessage1(listCubeIndexesTest[s], superpoly, outpu[s]));
                }
                if (QuadraticTest(new int[settings.NumPublicVar], listCubeIndexesTest[s]))
                {
                    //var a = SecretVariableIndexes(new int[settings.NumPublicVar], listCubeIndexesTest[s]);
                    superpoly2 = ComputeSuperpoly2(new int[settings.NumPublicVar], SecretVariableIndexes(new int[settings.NumPublicVar], listCubeIndexesTest[s]), listCubeIndexesTest[s]);
                    Console.WriteLine(GetLogMessage2(listCubeIndexesTest[s], superpoly2, outpu[s]));
                }

            }
        }
    }
}