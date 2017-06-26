#define PRINT

using System;


namespace NCubeAttack
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            CCubeAttack A = new CCubeAttack(CubeAttackSettings.CipherName.speck32_64);
            CCubeAttack.Preprocessing();
            //CCubeAttack.Online();
            //CCubeAttack.UserMode();

            Console.WriteLine("Press Enter for exit");
            Console.ReadLine();
        }

    }
}