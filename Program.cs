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
            CCubeAttack A = new CCubeAttack(CubeAttackSettings.CipherName.present);
            //CCubeAttack.Preprocessing();
           // CCubeAttack.Online();
            CCubeAttack.UserMode();

            // int[] t1 = new int[64];
            // int[] t2 = new int[64];
            // int[] t3 = new int[100];

            //   t1 = Operation.MathHelper.RandomGenerator(64);
            //   t2 = Operation.MathHelper.RandomGenerator(10);
            //   t3 = Operation.MathHelper.RandomGenerator(10);

         //     CCubeAttack.PreprocessingPhase2();



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

            // var cube = new List<int> { 63, 61, 23, 20 };
            //  Console.WriteLine(CCubeAttackSC.SuperpolyAsString(CCubeAttackSC.ComputeSuperpolyForBlockCipher(new int[CCubeAttackSC.NumPublicVar], new List<int> { 25})));
            //cube = CCubeAttackSC.SecretVariableIndexesForBlockCipher(new int[64], new List<int> { 63, 61, 10, 8 });
            //  var listt = new List<List<int>> { };
            //listt = CCubeAttackSC.ComputeSuperpoly2ForBlockCipher(new int[64], CCubeAttackSC.SecretVariableIndexesForBlockCipher(new int[64], cube), cube);
            //Console.WriteLine(CCubeAttackSC.Superpoly2AsString(CCubeAttackSC.ComputeSuperpoly2ForBlockCipher(new int[64], CCubeAttackSC.SecretVariableIndexesForBlockCipher(new int[64],cube),cube)));



            //ushort[] plaintext = new ushort[2] { 0x694c, 0x6574 };
            //Console.WriteLine(Operation.MathHelper.GetIBit(Operation.MathHelper.BitCount(plaintext[0]),1));
            //ushort[] key = new ushort[4] { 0x0100, 0x0908, 0x1110, 0x1918 };
            //ushort[] ciphertext = new ushort[2];
            //SpeckCipher.speck_block(plaintext, key, ciphertext);
            //Console.WriteLine("{0} {1}", plaintext[0].ToString("X"), plaintext[1].ToString("X"));
            //Console.WriteLine("{0} {1} {2} {3}", key[0].ToString("X"), key[1].ToString("X"), key[2].ToString("X"), key[3].ToString("X"));
            //Console.WriteLine("{0} {1}", ciphertext[0].ToString("X"), ciphertext[1].ToString("X"));
            //Console.WriteLine("Expected:   0x42f2 0xa868\n");
            //Console.WriteLine(0x694c);


            //CubeAttack.ComputeSuperpoly(new int[3], new List<int>() { 0, 1 });
            //   Console.WriteLine(Operation.MathHelper.GetBigIntFromIndexArrayFromMSB(new int[] { 1,0,1,1,1}));
            Console.WriteLine("Press Enter for exit");
            Console.ReadLine(); 
        }

    }
}