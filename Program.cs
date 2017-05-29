#define PRINT

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;



namespace NCubeAttack
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            CCubeAttack A = new CCubeAttack(CubeAttackSettings.CipherName.speck32_64);
            CCubeAttack.Preprocessing();
            //  CCubeAttack.Online();
           // CCubeAttack.OnlinePhase2();
    
            CCubeAttack.UserMode();
            
            //string answer = "";
            //while (answer != "e")
            //{
            //    Console.WriteLine("action: ");
            //    answer = Console.ReadLine(); 
            //    CCubeAttack.UserMode();
            //}



            //#if PRINT
            //            ushort i, j;
            //#endif

            //            int[] IdeaKey = {
            //                1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            //            };

            //            ushort[] state = { 0, 1, 2, 3 };
            //            //ushort[] key = { 1, 2, 3, 4, 5, 6, 7, 8 };
            //            ushort[] key = { 0, 0,0, 0, 0, 0, 0, 0 };
            //            key = Operation.MathHelper.ConvertFromBoolVectorToByteArray(IdeaKey, 16);
            //            //ushort state[4]={0xFFFF,0xFFFF,0xFFFF,0xFFFF};
            //            //ushort key[8]={0xFFFF,0xFFFF,0xFFFF,0xFFFF,0xFFFF,0xFFFF,0xFFFF,0xFFFF};
            //            //ushort state[4]={0,0,0,0};
            //            //ushort key[8]={0,0,0,0,0,0,0,0};
            //            ushort[] subkey = new ushort[52];


            //            IDEACipher.KeyScheduleEncrypt(key, subkey);
            //#if PRINT
            //            Console.WriteLine("\n\n----------Encryption Keys----------\n\n");
            //            for (j = 0; j < 8; j++)
            //            {
            //                for (i = 0; i < 6; i++)
            //                    Console.WriteLine(subkey[6 * j + i].ToString("X"));
            //                Console.WriteLine();
            //            }

            //            for (i = 0; i < 4; i++) Console.WriteLine(subkey[48 + i].ToString("X"));
            //            Console.WriteLine("\n\n");
            //            Console.WriteLine("\n\n----------Fin Encryption Keys----------\n\n");
            //#endif
            //            IDEACipher.Encrypt(state, subkey);
            //#if PRINT
            //            Console.WriteLine("\n\n----------Cipher text----------\n\n");
            //            for (i = 0; i < 4; i++) Console.WriteLine(state[i].ToString("X"));
            //            Console.Write("\n\n");
            //            Console.WriteLine("\n\n----------Fin Cipher text----------\n\n");
            //#endif


            //            IDEACipher.KeyScheduleDecrypt(key, subkey);
            //#if PRINT
            //            Console.WriteLine("\n\n----------Decryption Keys----------\n\n");
            //            for (j = 0; j < 8; j++) { for (i = 0; i < 6; i++) Console.WriteLine(subkey[6 * j + i].ToString("X")); Console.Write("\n"); }
            //            for (i = 0; i < 4; i++) Console.WriteLine(subkey[48 + i].ToString("X")); Console.Write("\n\n");
            //            Console.WriteLine("\n\n----------Fin Decryption Keys----------\n\n");
            //#endif
            //            IDEACipher.Encrypt(state, subkey);
            //#if PRINT
            //            Console.WriteLine("\n\n----------Plain text----------\n\n");
            //            for (i = 0; i < 4; i++) Console.WriteLine(state[i].ToString("X")); Console.Write("\n\n");
            //            Console.Write("\n\n----------Fin Plain text----------\n\n");
            //#endif


            //  long v = 4; // current permutation of bits 
            //   long w = 0; // next permutation of bits

            //   long t = (v | (v - 1)) + 1;
            //   w = (t | ((((t & -t) / (v & -v)) >> 1) - 1));
            //   Console.WriteLine(w);
            // int[] t1 = new int[64];
            // int[] t2 = new int[64];
            // int[] t3 = new int[100];

            //   t1 = Operation.MathHelper.RandomGenerator(64);
            //   t2 = Operation.MathHelper.RandomGenerator(10);
            //   t3 = Operation.MathHelper.RandomGenerator(10);

            //     CCubeAttack.PreprocessingPhase2();

            //int n = 5;

            //long cp_i = 1; // current permutation of bits 
            //long cp = 1;
            //long np = 0; // next permutation of bits

            //for (int i = 1; i < n + 1 ; i++)
            //{
            //    for (int j = 0; j < Operation.MathHelper.combination(n, i); j++)
            //    {
            //        Console.WriteLine(cp);
            //        long t = (cp | (cp - 1)) + 1;
            //        np = (t | ((((t & -t) / (cp & -cp)) >> 1) - 1));
            //        cp = np;
            //    }

            //    Console.WriteLine("------------------");
            //    cp_i = cp_i * 2 + 1;
            //    cp = cp_i;
            //}

            //Console.WriteLine(Math.Log(16)/Math.Log(2));


            //int[,] state = new int[4,4];
            //int[] keys = new int[32];
            //int i, j;


            //for (i = 0; i < 4; i++)
            //    for (j = 0; j < 4; j++)
            //        state[i, j] = 0;

            //   for (i = 0; i < 4; i++)
            //   {
            //       for (j = 0; j < 4; j++)
            //           Console.WriteLine(state[i, j]);
            //       Console.WriteLine();
            //   }

            //   Console.WriteLine("enc");
            //   Console.WriteLine();

            //   for (i=0; i< 32; i++) keys[i] = i;


            //LedCipher.LEDRound(state, keys);



            //   LedCipher.invLEDRound(state, keys);

            //for (i = 0; i < 4; i++)
            //{
            //    for (j = 0; j < 4; j++)
            //        Console.WriteLine(state[i, j]);
            //    Console.WriteLine();
            //}

            //Random rand = new Random();
            //int[] p = new int[8];
            //int[] c = new int[8];
            //int[] k = new int[16];
            //int n;
            //int kbits = 64;
            //int i;
            //for (i = 0; i < 8; i++) c[i] = p[i] = 0;
            //for (i = 0; i < 16; i++) k[i] = 0;
            //Console.Write("K = ");
            //for (i = 0; i < kbits / 8; i++)
            //    Console.Write(k[i].ToString("X"));
            //Console.WriteLine();

            //Console.Write("P = ");
            //for (i = 0; i < 8; i++)
            //    Console.Write(p[i].ToString("X"));
            //Console.WriteLine();

            //LedCipher.LED_enc(c, k, kbits);

            //Console.Write("C = ");
            //for (i = 0; i < 8; i++)
            //    Console.Write(c[i].ToString("X"));
            //Console.WriteLine();

            //Console.WriteLine(30200 & 0xff);

            //for (int s = 0; s < 16; s++)
            //    k[s] = s & 1;
            //int[] byt = new int[2];
            //byt = Operation.MathHelper.ConvertFromBoolVectorToByteArray(k, 2);
            //  CubeAttack.OnlinePhase2();

            //var superpolyMatrixM = new Matrix.Matrix(0, 64 + 1);
            //var listCubeIndexesM = new List<List<int>>();
            //var superpoly1 = new List<int>();
            //var superpoly2 = new List<int>();

            //listCubeIndexesM.Add(new List<int> { 27, 28, 30, 31 });
            //listCubeIndexesM.Add(new List<int> { 27, 28, 29 });

            //superpoly1 = CCubeAttack.ComputeSuperpolyForBlockCipher(new int[32], listCubeIndexesM[0]);
            //superpoly2 = CCubeAttack.ComputeSuperpolyForBlockCipher(new int[32], listCubeIndexesM[1]);

            //superpolyMatrixM = superpolyMatrixM.AddRow(superpoly1);
            //superpolyMatrixM = superpolyMatrixM.AddRow(superpoly2);
            //if (CCubeAttack.IsLinearIndependent(superpolyMatrixM))
            //{
            //    Console.WriteLine("true");
            //}

            //   var cube = new List<int> { 63, 62, 7, 5 };
            //     var cube = new List<int> { };
            //   // Console.WriteLine(CCubeAttack.SuperpolyAsString(CCubeAttack.ComputeSuperpolyForBlockCipher(new int[CCubeAttack.NumPublicVar], new List<int> { 25})));
            //   cube = CCubeAttack.SecretVariableIndexes(new int[64], new List<int> { 63, 61, 10, 8 });
            //      var listt = new List<List<int>> { };
            //listt = CCubeAttack.ComputeSuperpoly2(new int[64], CCubeAttack.SecretVariableIndexes(new int[64], new List<int> { 63, 62, 7, 5 }), new List<int> { 62, 61, 6, 4 });
            //Console.WriteLine(CCubeAttack.Superpoly2AsString(CCubeAttack.ComputeSuperpoly2(new int[64], CCubeAttack.SecretVariableIndexes(new int[64], new List<int> { 63, 62, 7, 5 }), new List<int> { 63, 62, 7, 5 })));



            //ushort[] plaintext = new ushort[2] { 0x694c, 0x6574 };
            //Console.WriteLine(Operation.MathHelper.GetIBit(Operation.MathHelper.BitCount(plaintext[0]), 1));
            //ushort[] key = new ushort[4] { 0x0100, 0x0908, 0x1110, 0x1918 };
            //ushort[] ciphertext = new ushort[2];
            //SpeckCipher.speck_block(plaintext, key, ciphertext);
            //Console.WriteLine("{0} {1}", plaintext[0].ToString("X"), plaintext[1].ToString("X"));
            //Console.WriteLine("{0} {1} {2} {3}", key[0].ToString("X"), key[1].ToString("X"), key[2].ToString("X"), key[3].ToString("X"));
            //Console.WriteLine("{0} {1}", ciphertext[0].ToString("X"), ciphertext[1].ToString("X"));
            //Console.WriteLine("Expected:   0x42f2 0xa868\n");
            //Console.WriteLine(0x694c);

            //ushort[] key1 = new ushort[4] { 0x0100, 0x0908, 0x1110, 0x1918 };
            //ushort[] plaintext1 = new ushort[2] { 0x694c, 0x6574 };
            //ushort[] ciphertext1 = new ushort[2] { 0x42f2, 0xa868 };
            //ushort[] actualCipherText1 = new ushort[2] { 0x0000, 0x0000 };

            //SpeckCipher.speck_block(plaintext1, key1, actualCipherText1);

            //CubeAttack.ComputeSuperpoly(new int[3], new List<int>() { 0, 1 });
            //   Console.WriteLine(Operation.MathHelper.GetBigIntFromIndexArrayFromMSB(new int[] { 1,0,1,1,1}));


            //int[] random1 = Operation.MathHelper.RandomGenerator(10);
            //Console.WriteLine(Operation.MathHelper.GetBigIntFromIndexArrayFromLSB(random1));


            //int[] random2 = Operation.MathHelper.RandomGenerator(10);
            //Console.WriteLine(Operation.MathHelper.GetBigIntFromIndexArrayFromLSB(random2));
            Console.WriteLine("Press Enter for exit");
            Console.ReadLine();
        }

    }
}