#define PRINT

using System;


namespace NCubeAttack
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            CCubeAttack A = new CCubeAttack(CubeAttackSettings.CipherName.test);
            //CCubeAttack.Preprocessing();
            //CCubeAttack.Online();
            CCubeAttack.UserMode();

           // CCubeAttack.OnlinePhaseTest();

            //Console.WriteLine("Press Enter for exit");
            Console.ReadLine();
        }

    }
}