using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class IDEACipher
{
    public static ushort NBROUND = 1;

    public static ushort mul(ushort a, ushort b)
    {
        uint r;
        uint A = a, B = b;

        if (A == 0 && B == 0) return 0x0001;
        if (A == 0)
        {
            A = 0x10000;
        }
        else if (B == 0)
        {
            B = 0x10000;
        }
        r = A * B;
        r %= 0x10001;
        if (r == 0x10000) r = 0;
        return (ushort)(r & 0xFFFF);
    }

    public static ushort invMod(ushort b)
    {
        int x = 0, lastx = 1, y = 1, lasty = 0, quotient, temp;
        uint A = 0x10001, B = b;
        while (B != 0)
        {
            quotient = (int)(A / B);
            temp = (int)B;
            B = A % B;
            A = (uint)temp;

            temp = x;
            x = lastx - quotient * x;
            lastx = temp;

            temp = y;
            y = lasty - quotient * y;
            lasty = temp;
        }
        if (lasty < 0)
        {
            lasty += (int)0x10001U;
        }
        return (ushort)lasty;

    }
   
    public static void KeyScheduleEncrypt(ushort[] key, ushort[] subkey)
    {
        byte i;
        ushort[] k = new ushort[8];
        ushort temp0, temp1;

        for (i = 0; i < 8; i++) k[i] = key[i];

        for (i = 0; i < 6; i++)
        {
            subkey[(i << 3)] = k[0];
            subkey[(i << 3) + 1] = k[1];
            subkey[(i << 3) + 2] = k[2];
            subkey[(i << 3) + 3] = k[3];
            subkey[(i << 3) + 4] = k[4];
            subkey[(i << 3) + 5] = k[5];
            subkey[(i << 3) + 6] = k[6];
            subkey[(i << 3) + 7] = k[7];

            temp0 = k[0];
            temp1 = k[1];

            k[0] = (ushort)(((k[1] << 9) & 0xFFFF) ^ (k[2] >> 7));
            k[1] = (ushort)(((k[2] << 9) & 0xFFFF) ^ (k[3] >> 7));
            k[2] = (ushort)(((k[3] << 9) & 0xFFFF) ^ (k[4] >> 7));
            k[3] = (ushort)(((k[4] << 9) & 0xFFFF) ^ (k[5] >> 7));
            k[4] = (ushort)(((k[5] << 9) & 0xFFFF) ^ (k[6] >> 7));
            k[5] = (ushort)(((k[6] << 9) & 0xFFFF) ^ (k[7] >> 7));
            k[6] = (ushort)(((k[7] << 9) & 0xFFFF) ^ (temp0 >> 7));
            k[7] = (ushort)(((temp0 << 9) & 0xFFFF) ^ (temp1 >> 7));
        }

        subkey[48] = k[0];
        subkey[49] = k[1];
        subkey[50] = k[2];
        subkey[51] = k[3];
        return;
    }


    public static void KeyScheduleDecrypt(ushort[] key, ushort[] subkey)
    {

        ushort[] tempkey = new ushort[52];
        KeyScheduleEncrypt(key, tempkey);

        subkey[0] = invMod(tempkey[48]);
        subkey[1] = (ushort)((~tempkey[49]) + 1);
        subkey[2] = (ushort)((~tempkey[50]) + 1);
        subkey[3] = invMod(tempkey[51]);
        subkey[4] = tempkey[46];
        subkey[5] = tempkey[47];

        for (ushort i = 1; i < NBROUND; i++)
        {
            subkey[(6 * i) + 0] = invMod(tempkey[(8 - i) * 6 + 0]);
            subkey[(6 * i) + 1] = (ushort)((~tempkey[(8 - i) * 6 + 2]) + 1);
            subkey[(6 * i) + 2] = (ushort)((~tempkey[(8 - i) * 6 + 1]) + 1);
            subkey[(6 * i) + 3] = invMod(tempkey[(8 - i) * 6 + 3]);
            subkey[(6 * i) + 4] = tempkey[(7 - i) * 6 + 4];
            subkey[(6 * i) + 5] = tempkey[(7 - i) * 6 + 5];
        }

        subkey[48] = invMod(tempkey[0]);
        subkey[49] = (ushort)((~tempkey[1]) + 1);
        subkey[50] = (ushort)((~tempkey[2]) + 1);
        subkey[51] = invMod(tempkey[3]);

        return;
    }
  

    public static void Encrypt(ushort[] state, ushort[] subkey)
    {
        ushort i, t0, t1, t2;
        for (i = 0; i < NBROUND; i++)
        {
            state[0] = mul(state[0], subkey[(6 * i) + 0]);
            state[1] = (ushort)((state[1] + subkey[(6 * i) + 1]) & 0xFFFF);
            state[2] = (ushort)((state[2] + subkey[(6 * i) + 2]) & 0xFFFF);
            state[3] = mul(state[3], subkey[(6 * i) + 3]);


            t0 = mul(subkey[(6 * i) + 4], (ushort)(state[0] ^ state[2]));
            t1 = mul(subkey[(6 * i) + 5], (ushort)((t0 + (state[1] ^ state[3])) & 0xFFFF));
            t2 = (ushort)((t0 + t1) & 0xFFFF);

            state[0] = (ushort)(state[0] ^ t1);
            state[3] = (ushort)(state[3] ^ t2);
            t0 = (ushort)(t2 ^ state[1]);
            state[1] = (ushort)(state[2] ^ t1);
            state[2] = t0;
        }

        t1 = state[1];
        state[0] = mul(state[0], subkey[48]);
        state[1] = (ushort)((state[2] + subkey[49]) & 0xFFFF);
        state[2] = (ushort)((t1 + subkey[50]) & 0xFFFF);
        state[3] = mul(state[3], subkey[51]);
    }


}

