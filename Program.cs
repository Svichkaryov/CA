using System;
using System.Collections.Generic;
using System.Numerics;

namespace NAttack
{
    internal class Program
    {
       
        private static void Main(string[] args)
        {
              CCubeAttackSC.Preprocessing();
              CCubeAttackSC.Online();
            //  CubeAttack.SetPublicBitsPhase();
            // CubeAttack.PreprocessingPhase2();
            //  CubeAttack.OnlinePhase2();
            //   Console.WriteLine(CubeAttack.ComputeSuperpolyForBlockCipher(new int[CubeAttack.NumPublicVar], new List<int> { 62, 61, 60, 59, 58, 56, 50, 48 })[72]);
            //   Console.WriteLine(CubeAttack.SuperpolyAsString(CubeAttack.ComputeSuperpolyForBlockCipher(new int[CubeAttack.NumPublicVar], new List<int> { 62, 61, 60, 59, 58, 56, 50, 48 })));
            //CubeAttack.ComputeSuperpoly(new int[3], new List<int>() { 0, 1 });
         //   Console.WriteLine(Operation.MathHelper.GetBigIntFromIndexArrayFromMSB(new int[] { 1,0,1,1,1}));
            Console.WriteLine("Press Enter for exit");
            Console.ReadLine(); 
        }

    }
}