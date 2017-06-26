using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NCubeAttack
{
    class CPiccolo
    {
        static ushort[] sBox = new ushort[] { 0xe, 0x4, 0xb, 0x2, 0x3, 0x8, 0x0, 0x9, 0x1, 0xa, 0x7, 0xf, 0x6, 0xc, 0x5, 0xd };
        static ushort[] mul2 = new ushort[] { 0x0, 0x2, 0x4, 0x6, 0x8, 0xa, 0xc, 0xe, 0x3, 0x1, 0x7, 0x5, 0xb, 0x9, 0xf, 0xd };
        static ushort[] mul3 = new ushort[] { 0x0, 0x3, 0x6, 0x5, 0xc, 0xf, 0xa, 0x9, 0xb, 0x8, 0xd, 0xe, 0x7, 0x4, 0x1, 0x2 };

        static int NBROUND = 1;

        public static void ps(ushort[] state)
        {
            Console.WriteLine(state[3].ToString("X"));
            Console.WriteLine(state[2].ToString("X"));
            Console.WriteLine(state[1].ToString("X"));
            Console.WriteLine(state[0].ToString("X"));
            return;
        }
        public static void Gr(ushort[] state, ushort[] wk, ushort[] rk)
        {

            int round;
            // Premier Wk 
            state[3] ^= wk[0];
            state[1] ^= wk[1];
            for (round = 0; round <= (NBROUND - 2); round++)
            {
                state[2] ^= (ushort)(FonctionF(state[3]) ^ rk[2 * round]);
                state[0] ^= (ushort)(FonctionF(state[1]) ^ rk[2 * round + 1]);

                RoundPermutation(state);
            }
            state[2] ^= (ushort)(FonctionF(state[3]) ^ rk[2 * NBROUND - 2]);
            state[0] ^= (ushort)(FonctionF(state[1]) ^ rk[2 * NBROUND - 1]);

            // Deuxième Wk
            state[3] ^= wk[2];
            state[1] ^= wk[3];

            return;
        }

        // Fonction de déchiffrement
        public static void Gr_1(ushort[] state, ushort[] wk, ushort[] rk)
        {
            ushort[] wk_1 = new ushort[4];
            ushort[] rk_1 = new ushort[2 * NBROUND];

            int i;

            wk_1[0] = wk[2];
            wk_1[1] = wk[3];
            wk_1[2] = wk[0];
            wk_1[3] = wk[1];

            for (i = 0; i < NBROUND; i++)
            {
                rk_1[2 * i] = rk[2 * NBROUND - 2 * i - 2 + (i % 2)];
                rk_1[2 * i + 1] = rk[2 * NBROUND - 2 * i - 1 - (i % 2)];
            }

            Gr(state, wk_1, rk_1);

            return;
        }

        public static void RoundPermutation(ushort[] chaine)
        {
            ushort temp_0 = chaine[0];
            ushort temp_2 = chaine[2];

            chaine[0] = (ushort)((chaine[3] & 0xFF00) | (chaine[1] & 0x00FF));
            chaine[2] = (ushort)((chaine[1] & 0xFF00) | (chaine[3] & 0x00FF));

            chaine[1] = (ushort)((temp_0 & 0xFF00) | (temp_2 & 0x00FF));
            chaine[3] = (ushort)((temp_2 & 0xFF00) | (temp_0 & 0x00FF));

            return;
        }

        public static void wKS_80(ushort[] k, ushort[] wkDest)
        {
            wkDest[0] = (ushort)((k[1] & 0x00FF) | (k[0] & 0xFF00));
            wkDest[1] = (ushort)((k[0] & 0x00FF) | (k[1] & 0xFF00));
            wkDest[2] = (ushort)((k[3] & 0x00FF) | (k[4] & 0xFF00));
            wkDest[3] = (ushort)((k[4] & 0x00FF) | (k[3] & 0xFF00));
            return;
        }

        public static void rKS_80(ushort[] k, ushort[] rkDest)
        {
            int i;
            ushort tmp;
            ushort[] con = new ushort[2];
            byte[] rkEvenBox = new byte[5] { 2, 0, 2, 4, 0 };
            byte[] rkOddBox = new byte[5] { 3, 1, 3, 4, 1 };
            byte mod;

            for (i = 0; i < NBROUND; i++)
            {
                tmp = (ushort)(((i + 1) << 10) | (i + 1));
                con[0] = (ushort)(tmp ^ 0x2D3C);
                con[1] = (ushort)((tmp << 1) ^ 0x0F1E);

                rkDest[i << 1] = con[1];
                rkDest[(i << 1) + 1] = con[0];

                mod = (byte)(i % 5);
                rkDest[2 * i] ^= k[rkEvenBox[mod]];
                rkDest[2 * i + 1] ^= k[rkOddBox[mod]];
            }
            return;
        }

        public static ushort FonctionF(ushort subState)
        {
            ushort subState0 = sBox[subState & 0xF];
            ushort subState1 = sBox[(subState >>= 4) & 0xF];
            ushort subState2 = sBox[(subState >>= 4) & 0xF];
            ushort subState3 = sBox[(subState >>= 4) & 0xF];

            ushort temp_0 = (ushort)(mul3[subState3] ^ subState2 ^ subState1 ^ mul2[subState0]);
            ushort temp_1 = (ushort)(subState3 ^ subState2 ^ mul2[subState1] ^ mul3[subState0]);
            ushort temp_2 = (ushort)(subState3 ^ mul2[subState2] ^ mul3[subState1] ^ subState0);
            ushort temp_3 = (ushort)(mul2[subState3] ^ mul3[subState2] ^ subState1 ^ subState0);

            subState = sBox[temp_3];
            subState <<= 4;
            subState ^= sBox[temp_2];
            subState <<= 4;
            subState ^= sBox[temp_1];
            subState <<= 4;
            subState ^= sBox[temp_0];

            return subState;
        }
  
    }
}
