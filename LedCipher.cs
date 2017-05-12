using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class LedCipher : ICipher
{
    public static int LED = 64; // to use, change this to the key size wanted, assumed to be multiple of 4.
    //public static int RN = 32;

    public static int[,] MixColMatrix = {
            {4,  1,  2,  2},
            {8,  6,  5,  6},
            {11, 14, 10, 9},
            {2,  2,  15, 11},
        };

    public static int[] sbox = { 12, 5, 6, 11, 9, 0, 10, 13, 3, 14, 15, 8, 4, 7, 1, 2 };

    public static int[,] invMixColMatrix = {
            {12, 12, 13, 4},
            {3,  8,  4,  5},
            {7,  6,  2,  14},
            {13, 9,  9,  13}
        };

    public static int[] invSbox = { 5, 14, 15, 8, 12, 1, 2, 13, 11, 4, 6, 3, 0, 7, 9, 10 };

    public static int FieldMult(int a, int b)
    {
        const int ReductionPoly = 0x3;
        int x = a, ret = 0;
        for (int i = 0; i < 4; i++)
        {
            if (((b >> i) & 1) == 1) ret ^= x;
            if ((x & 0x8) == 1)
            {
                x <<= 1;
                x ^= ReductionPoly;
            }
            else x <<= 1;
        }
        return ret & 0xF;
    }

    public static void AddKey(int[,] state, int[] keyBytes, int step)
    {
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                state[i,j] ^= keyBytes[(4 * i + j + step * 16) % (LED / 4)];
    }

    public static void AddConstants(int[,] state, int r)
    {
        int[] RC = {
                0x01, 0x03, 0x07, 0x0F, 0x1F, 0x3E, 0x3D, 0x3B, 0x37, 0x2F,
                0x1E, 0x3C, 0x39, 0x33, 0x27, 0x0E, 0x1D, 0x3A, 0x35, 0x2B,
                0x16, 0x2C, 0x18, 0x30, 0x21, 0x02, 0x05, 0x0B, 0x17, 0x2E,
                0x1C, 0x38, 0x31, 0x23, 0x06, 0x0D, 0x1B, 0x36, 0x2D, 0x1A,
                0x34, 0x29, 0x12, 0x24, 0x08, 0x11, 0x22, 0x04
            };

        state[1, 0] ^= 1;
        state[2, 0] ^= 2;
        state[3, 0] ^= 3;

        int tmp = (RC[r] >> 3) & 7;
        state[0, 1] ^= tmp;
        state[2, 1] ^= tmp;
        tmp = RC[r] & 7;
        state[1, 1] ^= tmp;
        state[3, 1] ^= tmp;
    }

    public static void SubCell(int[,] state)
    {
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                state[i, j] = sbox[state[i, j]];
    }

    public static void ShiftRow(int[,] state)
    {
   
        int[] tmp = new int[4];
        for (int i = 1; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
                tmp[j] = state[i, j];
            for (int j = 0; j < 4; j++)
                state[i, j] = tmp[(j + i) % 4];
        }
    }

    public static void MixColumn(int[,] state)
    {
        int[] tmp = new int[4];
        for (int j = 0; j < 4; j++)
        {
            for (int i = 0; i < 4; i++)
            {
                int sum = 0;
                for (int k = 0; k < 4; k++)
                    sum ^= FieldMult(MixColMatrix[i, k], state[k, j]);
                tmp[i] = sum;
            }
            for (int i = 0; i < 4; i++)
                state[i, j] = tmp[i];
        }
    }

    public static void LEDRound(int[,] state, int[] keyBytes)
    {
        AddKey(state, keyBytes, 0);
        for (int i = 0; i < 32 / 4; i++)                    
        {
            for (int j = 0; j < 4; j++)
            {
                AddConstants(state, i * 4 + j);
                SubCell(state);
                ShiftRow(state);
                MixColumn(state);
            }
            AddKey(state, keyBytes, i + 1);
        }
    }

    public static void LED_enc(int[] input, int[] userkey, int ksbits)
    {

        int[,] state = new int[4, 4];
        int[] keyNibbles = new int[32];

        for (int i = 0; i < 16; i++)
        {
            if (i % 2 == 1) state[i / 4, i % 4] = input[i >> 1] & 0xF;
            else state[i / 4, i % 4] = (input[i >> 1] >> 4) & 0xF;
        }

        for (int iter = 0; iter < 32; iter++)
            keyNibbles[iter] = 0;
        for (int i = 0; i < ksbits / 4; i++)
        {
            if (i % 2  == 1) keyNibbles[i] = userkey[i >> 1] & 0xF;
            else keyNibbles[i] = (userkey[i >> 1] >> 4) & 0xF;
        }
        LED = ksbits;
        int RN = 48;
        if (LED <= 64)
            RN = 32;
        
        AddKey(state, keyNibbles, 0);
        for (int i = 0; i < RN / 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                AddConstants(state, i * 4 + j);
                SubCell(state);
                ShiftRow(state);
                MixColumn(state);
            }

           AddKey(state, keyNibbles, i + 1);
        }
        for (int i = 0; i < 8; i++)
            input[i] = ((state[(2 * i) / 4,(2 * i) % 4] & 0xF) << 4) | (state[(2 * i + 1) / 4,(2 * i + 1) % 4] & 0xF);
    }


    /************************************************************************/
    public static void invSubCell(int[,] state)
    {
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                state[i, j] = invSbox[state[i, j]];
    }

    public static void invShiftRow(int[,] state)
    {
        int[] tmp = new int[4];
        for (int i = 1; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
                tmp[j] = state[i, j];
            for (int j = 0; j < 4; j++)
                state[i, j] = tmp[(j + (4 - i)) % 4];
        }
    }


    public static void invMixColumn(int[,] state)
    {
        int[] tmp = new int[4];
        for (int j = 0; j < 4; j++)
        {
            for (int i = 0; i < 4; i++)
            {
                int sum = 0;
                for (int k = 0; k < 4; k++)
                    sum ^= FieldMult(invMixColMatrix[i, k], state[k, j]);
                tmp[i] = sum;
            }
            for (int i = 0; i < 4; i++)
                state[i, j] = tmp[i];
        }
    }

    public static void invLEDRound(int[,] state, int[] keyBytes)
    {
        for (int i = (32 / 4) - 1; i >= 0; i--)                            
        {
            AddKey(state, keyBytes, i + 1);
            for (int j = 3; j >= 0; j--)
            {
                invMixColumn(state);
                invShiftRow(state);
                invSubCell(state);
                AddConstants(state, i * 4 + j);
            }

        }
        AddKey(state, keyBytes, 0);
    }

}