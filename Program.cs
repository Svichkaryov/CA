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
           for(int i=0;i<4;i++)
                CCubeAttack.OnlinePhase2();

            CCubeAttack.UserMode();

           // var cube = new List<int>() {62,60,9,7};
           // var superpoly = new List<int>() { };
          //  superpoly = CCubeAttack.SecretVariableIndexes(new int[64], cube);

          //  var superpoly2 = new List<List<int>>();
          //  superpoly2 = CCubeAttack.ComputeSuperpoly2(new int[64], 
          //      CCubeAttack.SecretVariableIndexes(new int[64], cube),
            //                                   cube);
           /// Console.WriteLine(CCubeAttack.GetLogMessage2(cube, superpoly2));
            
            
            //if (CCubeAttack.LinearityTest2(new int[32], cube,superpoly) == true)
            //{
            //    Console.WriteLine("true");
            //}
            //else Console.WriteLine("huina");

            //int i, j = 0;
            //byte a, b, c, d;
            //byte[] inr = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //byte[] K = new byte[32] { 0x5,0x3,0xa,0xf,0xf,0x3,0x1,0x6,0x5,0x3,0xa,0xf,0xf,0x3,0x1,0x6
            //         ,0x2,0x1,0xc,0x7,0x2,0x5,0x7,0x8,0x0,0x2,0x5,0x3,0x8,0xb,0xa,0x2 };
            //for (a = 0; a <= 0xf; a++)
            //{
            //    inr[0] = a;
            //    for (b = 0; b <= 0xf; b++)
            //    {
            //        inr[1] = b;
            //        for (c = 0; c <= 0xf; c++)
            //        {
            //            inr[2] = c;
            //            for (d = 0; d <= 0xf; d++)
            //            {

            //                CMidori.Encrypt(14, inr, K);
            //                j++;

            //                Console.Write("After K_Guess Decryption: ");
            //                for (i = 0; i <= 15; i++)
            //                {

            //                    Console.WriteLine(inr[i].ToString("X"));
            //                }
            //                Console.WriteLine();
            //            }
            //        }
            //    }
            //}

            //  Console.WriteLine(j);



            //Console.WriteLine("Press Enter for exit");
            Console.ReadLine();
        }

    }
}