using System;
using System.Numerics;

public class Present : ICipher
{
    public static BigInteger[] SBox = new BigInteger[] {
        0xC, 0x5, 0x6, 0xB, 0x9, 0x0, 0xA, 0xD, 0x3, 0xE, 0xF, 0x8, 0x4, 0x7, 0x1, 0x2
    };

    public static BigInteger[] SBox_inv = new BigInteger[] {
        0x5, 0xE, 0xF, 0x8, 0xC, 0x1, 0x2, 0xD, 0xB, 0x4, 0x6, 0x3, 0x0, 0x7, 0x9, 0xA
    };

    public static int[] PBox = new int[] {
        0, 16, 32, 48, 1,  17, 33, 49, 2,  18, 34, 50, 3,  19, 35, 51,
        4, 20, 36, 52, 5,  21, 37, 53, 6,  22, 38, 54, 7,  23, 39, 55,
        8, 24, 40, 56, 9,  25, 41, 57, 10, 26, 42, 58, 11, 27, 43, 59,
       12, 28, 44, 60, 13, 29, 45, 61, 14, 30, 46, 62, 15, 31, 47, 63
    };

    public static int[] PBox_inv = new int[] {
        0, 4, 8,  12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60,
        1, 5, 9,  13, 17, 21, 25, 29, 33, 37, 41, 45, 49, 53, 57, 61,
        2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50, 54, 58, 62,
        3, 7, 11, 15, 19, 23, 27, 31, 35, 39, 43, 47, 51, 55, 59, 63
    };

    private static BigInteger MASK4 = (BigInteger.One <<4) - BigInteger.One;

    private int rounds;
    private BigInteger[] roundKeys;


    public Present(BigInteger key, int rounds) 
    {
        this.rounds = rounds;

        if (Operation.MathHelper.BitCount(key) > 128)
        {
            throw new Exception("Key too big. It has to be either 80 or 128 bit long.");
        }
		if (Operation.MathHelper.BitCount(key) > 80)
        {
            roundKeys = GenerateRoundkeys128(key, rounds);
        }
        else
        {
            roundKeys = GenerateRoundkeys80(key, rounds);
        }
    }


    public Present(BigInteger key) : this(key,1) { }
  
    private BigInteger[] GenerateRoundkeys80(BigInteger key, int rounds)
    {
        BigInteger[] roundKeys = new BigInteger[rounds];
        BigInteger tmpKey = key;
        BigInteger mask19 = (BigInteger.One << 19) - BigInteger.One;
        BigInteger mask76 = (BigInteger.One << 76) - BigInteger.One;

        for (int i = 1; i <= rounds; i++)
        {
            roundKeys[i - 1] = tmpKey >> 16;
            // 1. shift
            tmpKey = ((tmpKey & mask19) << 61) + (tmpKey >> 19);
            // 2. SBox
            BigInteger s = (tmpKey >> 76);
            tmpKey = (SBox[(int)s] << 76) + (tmpKey & mask76);
            // 3. Salt
            tmpKey ^= ((BigInteger)i) << 15;
        }
        return roundKeys;
    }

    private BigInteger[] GenerateRoundkeys128(BigInteger key, int rounds)
    {
        BigInteger[] roundKeys = new BigInteger[rounds];
        BigInteger tmpKey = key;
        BigInteger mask67 = (BigInteger.One << 67) - BigInteger.One;
        BigInteger mask120 = (BigInteger.One << 120) - BigInteger.One;

        for (int i = 1; i <= rounds; i++)
        {
            roundKeys[i - 1] = tmpKey >> 64;
            // 1. shift
            tmpKey = ((tmpKey & mask67) << 61) + (tmpKey >> 67);
            // 2. SBox
            tmpKey = (SBox[(long)tmpKey >> 124] << 124) + (SBox[((long)tmpKey >> 120) & 15] << 120) + (tmpKey & mask120);
            // 3. Salt
            tmpKey ^= ((BigInteger)i) << 62;
        }
        return roundKeys;
    }

    public BigInteger Encrypt(BigInteger message)
    {
        BigInteger state = message;

      //  for (int i = 0; i < rounds - 1; i++)
      //  {
            state = AddRoundKey(state, roundKeys[0]);
            state = SBoxLayer(state);
            state = PLayer(state);
      //  }
      //  return AddRoundKey(state, roundKeys[rounds - 1]);
        return state;
    }

    public BigInteger Decrypt(BigInteger cipher)
    {
        BigInteger state = cipher;
        for (int i = 0; i < rounds - 1; i++)
        {
            state = AddRoundKey(state, roundKeys[rounds - 1 - i]);
            state = PLayer_dec(state);
            state = SBoxLayer_dec(state);
        }

        return AddRoundKey(state, roundKeys[0]);
    }

    private BigInteger AddRoundKey(BigInteger state, BigInteger roundKey)
    {
        return state ^ roundKey;
    }

    private BigInteger SBoxLayer(BigInteger state)
    {
        BigInteger output = BigInteger.Zero;

        for (int i = 0; i < 16; i++)
        {
            output = output + (SBox[(long)((state >> (i * 4)) & (MASK4))] << (i * 4));
        }
        return output;
    }

    private BigInteger SBoxLayer_dec(BigInteger state)
    {
        BigInteger output = BigInteger.Zero;

        for (int i = 0; i < 16; i++)
        {
            output = output + (SBox_inv[(long)((state >> (i * 4)) & (MASK4))] << (i * 4));
        }
        return output;
    }

    //private BigInteger PLayer(BigInteger state)
    //{
    //    BigInteger output = BigInteger.Zero;

    //    for (int i = 0; i < 64; i++)
    //    {
    //        if (((state >> i) & 1) == 1)                          // get i'th bit (..<-..)            
    //        {
    //            output = output | (1 << PBox[i]);
    //        }
    //    }
    //    return output;
    //}

    private BigInteger PLayer(BigInteger state)
    {
        BigInteger pstate = 0;
        BigInteger output = state;

        foreach (var p in PBox)
        {
            pstate ^= (output & 0x1) << p;
            output >>= 1;
        }
        output = pstate;
        return output;
    }


    private BigInteger PLayer_dec(BigInteger state)
    {
        BigInteger pstate = 0;
        BigInteger output = state;

        foreach (var p in PBox_inv)
        {
            pstate ^= (output & 0x1) << p;
            output >>= 1;
        }
        output = pstate;
        return output;
    }

    private BigInteger[] GetRoundKeys()
    {
        return roundKeys;
    }

}
