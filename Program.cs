using System;
using System.Collections.Generic;


namespace CubeAttackTest
{
    internal class Program
    {
       
        private static void Main(string[] args)
        {
          //  CubeAttack.Preprocessing();
          //  CubeAttack.Online();
          //  CubeAttack.SetPublicBitsPhase();

            CubeAttack.ComputeSuperpolyForBlockCipher(new int[CubeAttack.NumPublicVar], new List<int> { 58, 59 });
            
            Console.WriteLine("Press Enter for exit");
            Console.ReadLine(); 
        }

    }
}