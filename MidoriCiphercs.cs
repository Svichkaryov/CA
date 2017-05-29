using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CMidori : ICipher
{
    public static byte[] s_box = { 0xc, 0xa, 0xd, 0x3, 0xe, 0xb, 0xf, 0x7, 0x8, 0x9, 0x1, 0x5, 0x0, 0x2, 0x4, 0x6 };
    public static byte[,] const_key = { 
        {0,0,0,1,0,1,0,1,1,0,1,1,0,0,1,1}, 
        {0,1,1,1,1,0,0,0,1,1,0,0,0,0,0,0},
        {1,0,1,0,0,1,0,0,0,0,1,1,0,1,0,1}, 
        {0,1,1,0,0,0,1,0,0,0,0,1,0,0,1,1},
		{0,0,0,1,0,0,0,0,0,1,0,0,1,1,1,1}, 
        {1,1,0,1,0,0,0,1,0,1,1,1,0,0,0,0},
        {0,0,0,0,0,0,1,0,0,1,1,0,0,1,1,0}, 
        {0,0,0,0,1,0,1,1,1,1,0,0,1,1,0,0},
        {1,0,0,1,0,1,0,0,1,0,0,0,0,0,0,1}, 
        {0,1,0,0,0,0,0,0,1,0,1,1,1,0,0,0},
        {0,1,1,1,0,0,0,1,1,0,0,1,0,1,1,1}, 
        {0,0,1,0,0,0,1,0,1,0,0,0,1,1,1,0},
        {0,1,0,1,0,0,0,1,0,0,1,1,0,0,0,0}, 
        {1,1,1,1,1,0,0,0,1,1,0,0,1,0,1,0},
        {1,1,0,1,1,1,1,1,1,0,0,1,0,0,0,0} };

    public static void SubCell(byte[] state)
    {
        int i;
        for (i = 0; i <= 15; i++)
        {
            state[i] = s_box[state[i]];
        }
    }

    public static void ShuffleCell(byte[] state)
    {
        byte[] temp = new byte[16];
        temp[0]  = state[0];
        temp[1]  = state[10];
        temp[2]  = state[5];
        temp[3]  = state[15];
        temp[4]  = state[14];
        temp[5]  = state[4];
        temp[6]  = state[11];
        temp[7]  = state[1];
        temp[8]  = state[9];
        temp[9]  = state[3];
        temp[10] = state[12];
        temp[11] = state[6];
        temp[12] = state[7];
        temp[13] = state[13];
        temp[14] = state[2];
        temp[15] = state[8];

        for (int i = 0; i <= 15; i++)
        {
            state[i] = temp[i];
        }
    }

    public static void Inv_ShuffleCell(byte[] state)
    {
        byte[] temp = new byte[16];
        temp[0] = state[0];
        temp[1] = state[7];
        temp[2] = state[14];
        temp[3] = state[9];
        temp[4] = state[5];
        temp[5] = state[2];
        temp[6] = state[11];
        temp[7] = state[12];
        temp[8] = state[15];
        temp[9] = state[8];
        temp[10] = state[1];
        temp[11] = state[6];
        temp[12] = state[10];
        temp[13] = state[13];
        temp[14] = state[4];
        temp[15] = state[3];

        for (int i = 0; i <= 15; i++)
        {
            state[i] = temp[i];
        }
    }

    public static void MixColumn(byte[] state)
    {
        byte[] temp = new byte[16];

        for (int i = 0; i <= 3; i++)
        {
            temp[4 * i + 0] = (byte)(state[4 * i + 1] ^ state[4 * i + 2] ^ state[4 * i + 3]);
            temp[4 * i + 1] = (byte)(state[4 * i + 0] ^ state[4 * i + 2] ^ state[4 * i + 3]);
            temp[4 * i + 2] = (byte)(state[4 * i + 0] ^ state[4 * i + 1] ^ state[4 * i + 3]);
            temp[4 * i + 3] = (byte)(state[4 * i + 0] ^ state[4 * i + 1] ^ state[4 * i + 2]);
        }
        for (int i = 0; i <= 15; i++)
        {
            state[i] = temp[i];
        }
    }


    public static void rth_Round_Encrypt_KeyAdd(int r, byte[] state, byte[] K)
    {
        if (r % 2 == 0)
        {
            for (int i = 0; i <= 15; i++)
            {
                state[i] = (byte)(state[i] ^ K[i] ^ const_key[r,i]);
            }
        }
        else
        {
            for (int i = 0; i <= 15; i++)
            {
                state[i] = (byte)(state[i] ^ K[i + 16] ^ const_key[r,i]);
            }
        }
    }

    public void rth_Round_Decrypt_KeyAdd(int r, byte[] state, byte[] K)
    {
        byte[] Kr = new byte[16];
        if (r % 2 == 0)
        {
            for (int i = 0; i <= 15; i++)
            {
                Kr[i] = (byte)(K[i] ^ const_key[r, i]);
            }
            MixColumn(Kr);
            Inv_ShuffleCell(Kr);
            for (int i = 0; i <= 15; i++)
            {
                state[i] = (byte)(state[i] ^ Kr[i]);
            }
        }
        else
        {
            for (int i = 0; i <= 15; i++)
            {
                Kr[i] = (byte)(K[i + 16] ^ const_key[r, i]);
            }
            MixColumn(Kr);
            Inv_ShuffleCell(Kr);
            for (int i = 0; i <= 15; i++)
            {
                state[i] = (byte)(state[i] ^ Kr[i]);
            }
        }
    }


    public static void Encrypt(int r, byte[] state, byte[] K)
    {
        for (int i = 0; i <= 15; i++)
        {
            state[i] = (byte)(state[i] ^ K[i] ^ K[i + 16]);
        }
        for (int i = 0; i <= r; i++)
        {
            SubCell(state);
            ShuffleCell(state);
            MixColumn(state);
            rth_Round_Encrypt_KeyAdd(i, state, K);
        }
        SubCell(state);
        for (int i = 0; i <= 15; i++)
        {
            state[i] = (byte)(state[i] ^ K[i] ^ K[i + 16]);
        }
    }

    public void Decrypt(int r, byte[] state, byte[] K)
    {
        for (int i = 0; i <= 15; i++)
        {
            state[i] = (byte)(state[i] ^ K[i] ^ K[i + 16]);
        }
        for (int i = r; i >= 0; i--)
        {
            SubCell(state);
            MixColumn(state);
            Inv_ShuffleCell(state);
            rth_Round_Decrypt_KeyAdd(i, state, K);

        }
        SubCell(state);
        for (int i = 0; i <= 15; i++)
        {
            state[i] = (byte)(state[i] ^ K[i] ^ K[i + 16]);
        }
    }


}

