using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCUbeAttack
{
    class CHight
    {
        static public byte NBROUND = 32;
        /*void ConstantGeneration(u8 *delta)
        {
            u8 i,j;
            u8 s[133]= {0,1,0,1,1,0,1};
            delta[0] = 0b1011010 ;
            for(i = 1;i<128;i++)
            {
                s[i+6] = s[i+2] ^ s[i-1] ;
                delta[i]=s[i+6];
                for(j=1;j<7;j++)
                {	
                    delta[i]<<=1;
                    delta[i] ^= s[6-j+i];
                }
            }
            return;
        }
        */

        static public byte[] delta ={
0x5A,0x6D,0x36,0x1B,0xD,0x6,0x3,0x41,
0x60,0x30,0x18,0x4C,0x66,0x33,0x59,0x2C,
0x56,0x2B,0x15,0x4A,0x65,0x72,0x39,0x1C,
0x4E,0x67,0x73,0x79,0x3C,0x5E,0x6F,0x37,
0x5B,0x2D,0x16,0xB,0x5,0x42,0x21,0x50,
0x28,0x54,0x2A,0x55,0x6A,0x75,0x7A,0x7D,
0x3E,0x5F,0x2F,0x17,0x4B,0x25,0x52,0x29,
0x14,0xA,0x45,0x62,0x31,0x58,0x6C,0x76,
0x3B,0x1D,0xE,0x47,0x63,0x71,0x78,0x7C,
0x7E,0x7F,0x3F,0x1F,0xF,0x7,0x43,0x61,
0x70,0x38,0x5C,0x6E,0x77,0x7B,0x3D,0x1E,
0x4F,0x27,0x53,0x69,0x34,0x1A,0x4D,0x26,
0x13,0x49,0x24,0x12,0x9,0x4,0x2,0x1,
0x40,0x20,0x10,0x8,0x44,0x22,0x11,0x48,
0x64,0x32,0x19,0xC,0x46,0x23,0x51,0x68,
0x74,0x3A,0x5D,0x2E,0x57,0x6B,0x35,0x5A
        };

        public static byte F0(byte nb)
        {
            byte tmp1, tmp2, tmp3;

            tmp1 = (byte)(((nb << 1) & 0xFF) ^ ((nb >> 7) & 0xFF));
            tmp2 = (byte)(((nb << 2) & 0xFF) ^ ((nb >> 6) & 0xFF));
            tmp3 = (byte)(((nb << 7) & 0xFF) ^ ((nb >> 1) & 0xFF));
            return (byte)(tmp1 ^ tmp2 ^ tmp3);
        }

        public static byte F1(byte nb)
        {
            byte tmp1, tmp2, tmp3;
            tmp1 = (byte)(((nb << 3) & 0xFF) ^ ((nb >> 5) & 0xFF));
            tmp2 = (byte)(((nb << 4) & 0xFF) ^ ((nb >> 4) & 0xFF));
            tmp3 = (byte)(((nb << 6) & 0xFF) ^ ((nb >> 2) & 0xFF));
            return (byte)(tmp1 ^ tmp2 ^ tmp3);
        }


        /*****Decryption*******************************************************/

        public static void invFinalTransfomation(byte[] state, byte[] wk)
        {

            byte temp = state[7];
            state[7] = (byte)(state[6] ^ wk[7]);
            state[6] = (byte)(state[5]);
            state[5] = (byte)(state[4] - wk[6]);
            state[4] = (byte)(state[3]);
            state[3] = (byte)(state[2] ^ wk[5]);
            state[2] = (byte)(state[1]);
            state[1] = (byte)(state[0] - wk[4]);
            state[0] = temp;

            return;
        }

        public void invRoundFunction(byte[] state, byte[] sk)
        {
            byte temp = state[0];

            state[0] = (byte)(state[1]);
            state[1] = (byte)(state[2] - (F1(state[0]) ^ sk[0]));
            state[2] = (byte)(state[3]);
            state[3] = (byte)(state[4] ^ (F0(state[2]) + sk[1]));
            state[4] = (byte)(state[5]);
            state[5] = (byte)(state[6] - (F1(state[4]) ^ sk[2]));
            state[6] = (byte)(state[7]);
            state[7] = (byte)(temp ^ (F0(state[6]) + sk[3]));
            return;
        }


    public void invInitialTransfomation(byte[] state, byte[] wk)
    {
        state[0] = (byte)(state[0] - wk[0]);
        state[2] = (byte)(state[2] ^ wk[1]);
        state[4] = (byte)(state[4] - wk[2]);
        state[6] = (byte)(state[6] ^ wk[3]);
        return;
    }


    public void Decrypt(byte[] state, byte[] wk, byte[] sk)
    {
        byte i;

        invFinalTransfomation(state, wk);
        for (i = (byte)(NBROUND - 1); i >= 0; i--)
        {
        //    invRoundFunction(state, sk + (i << 2));
        }
        invInitialTransfomation(state, wk);

        return;
    }



    /******Encryption******************************************************/
    public static void FinalTransfomation(byte[] state, byte[] wk)
    {
        byte temp = state[0];
        state[0] = (byte)((state[1] + wk[4]) & 0xFF);
        state[1] = state[2];
        state[2] = (byte)(state[3] ^ wk[5]);
        state[3] = state[4];
        state[4] = (byte)((state[5] + wk[6]) & 0xFF);
        state[5] = state[6];
        state[6] = (byte)(state[7] ^ wk[7]);
        state[7] = temp;
        return;
    }


    public static void RoundFunction(byte[] state, byte[] sk)
    {
        byte temp6 = F0(state[6]);
        byte temp = state[7];

        state[7] = state[6];
        state[6] = (byte)((state[5] + (F1(state[4]) ^ sk[2])) & 0xFF);
        state[5] = state[4];
        state[4] = (byte)(state[3] ^ ((F0(state[2]) + sk[1]) & 0xFF));
        state[3] = state[2];
        state[2] = (byte)((state[1] + (F1(state[0]) ^ sk[0])) & 0xFF);
        state[1] = state[0];
        state[0] = (byte)(temp ^ ((temp6 + sk[3]) & 0xFF));

        return;
    }


    public static void InitialTransfomation(byte[] state, byte[] wk)
    {
        state[0] = (byte)((state[0] + wk[0]) & 0xFF);
        state[2] = (byte)(state[2] ^ wk[1]);
        state[4] = (byte)((state[4] + wk[2]) & 0xFF);
        state[6] = (byte)(state[6] ^ wk[3]);
        return;
    }


    public static void Encrypt(byte[] state, byte[] wk, byte[] sk)
    {
        byte i;

        InitialTransfomation(state, wk);
        for (i = 0; i < NBROUND; i++)
        {
          //  RoundFunction(state, sk + (i << 2));
        }
        FinalTransfomation(state, wk);
        return;
    }

    /**********************************************************************/

    /********Key schedule**************************************************/

    public static void WhiteningKeyGeneration(byte[] mk, byte[] wk)
    {
        byte i;
        for (i = 0; i < 4; i++)
        {
            wk[i] = mk[i + 12];
        }
        for (i = 4; i < 8; i++)
        {
            wk[i] = mk[i - 4];
        }
        return;
    }

    public static void SubkeyGeneration(byte[] mk, byte[] sk)
    {
        byte i, j, index;

        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                index = (byte)((j - i + 8) & 0x07);
                sk[16 * i + j] = (byte)((mk[index] + delta[16 * i + j]) & 0xFF);
                sk[16 * i + j + 8] = (byte)((mk[index + 8] + delta[16 * i + j + 8]) & 0xFF);
            }
        }
        return;
    }

    public static void KeySchedule(byte[] mk, byte[] wk, byte[] sk)
    {
        WhiteningKeyGeneration(mk, wk);
        SubkeyGeneration(mk, sk);
        return;
    }
}


}
