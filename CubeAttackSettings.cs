using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NCubeAttack
{
    class CubeAttackSettings
    {
        public enum CubeAttackMode { preprocessing, online, setPublicBits };
        public enum CipherName { test, present, speck32_64, led }

        public int NumLinearTest    = 100;
        public int NumQuadraticTest = 100;
        public int NumSecretParam   = 0;   // number of secret param(lenght of key in the cipher implemention)
        public int NumPublicVar     = 0;
        private int[] Testkey = { 1, 0, 1 };

        public int[] GetKey()
        {
            return Testkey;
        }

        public CubeAttackSettings()
        {
            if (CCubeAttack.BlackBoxID == 0)
            {
                NumSecretParam = 3;
                NumPublicVar = 3;
            }

            if (CCubeAttack.BlackBoxID == 1)
            {
                NumSecretParam = 80;   
                NumPublicVar = 64;
            }

            if (CCubeAttack.BlackBoxID == 2)
            {
                NumSecretParam = 64;
                NumPublicVar = 32;
            }

            if (CCubeAttack.BlackBoxID == 3)
            {
                NumSecretParam = 80;
                NumPublicVar = 64;
            }

        }
    } 
}
