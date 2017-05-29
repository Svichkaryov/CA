
class SeaCipher
{

    public static int SIZE = 96;//n : plaintext size, key size. k*6B
    public static int B = 16; //processor (or word) size.
    public static ushort NB = (ushort)(SIZE/(2*B));//nb = n/2b : number of words per Feistel branch.
    public static ushort NBROUND = 51; // odd number
    public static int MASK = 0xFFFF;

    public static void XOR(ushort[] x, ushort[] y)
    {
        for (ushort i = 0; i < NB; i++)
            x[i] ^= y[i];
    }

    public static void Sub(ushort[] x)
    {
        for (ushort i = 0; i < (SIZE / (6 * B)); i++)
        {
            x[3 * i] ^= (ushort)(x[3 * i + 1] & x[3 * i + 2]);
            x[3 * i + 1] ^= (ushort)(x[3 * i] & x[3 * i + 2]);
            x[3 * i + 2] ^= (ushort)(x[3 * i + 1] | x[3 * i]);
        }
    }

    public static void WordRot(ushort[] x)
    {
        ushort temp = x[NB - 1];
        for (ushort i = (ushort)((NB - 1)); i > 0; i--)
        {
            x[i] = x[i - 1];
        }
        x[0] = temp;
    }

    public static void InvWordRot(ushort[] x)
    {
        ushort temp = x[0];
        for (ushort i = 0; i < NB - 1; i++)
        {
            x[i] = x[i + 1];
        }
        x[NB - 1] = temp;
    }

    public static void BitRot(ushort[] x)
    {
        for (ushort i = 0; i < NB / 3; i++)
        {
            x[3 * i] = (ushort)((x[3 * i] >> 1) ^ (x[3 * i] << (B - 1)));
            x[3 * i + 2] = (ushort)((x[3 * i + 2] << 1) ^ (x[3 * i + 2] >> (B - 1)));
        }
    }

    public static void Add(ushort[] x, ushort[] y)
    {
        for (ushort i = 0; i < NB; i++)
            x[i] = (ushort)((x[i] + y[i]) & MASK);
    }
    
    public static void fk(ushort[] kr, ushort[] kl, ushort[] krDest, ushort[] klDest, ushort[] c)
    {
        for (ushort i = 0; i < NB; i++) krDest[i] = kr[i];
        for (ushort i = 0; i < NB; i++) klDest[i] = kr[i];

        Add(krDest, c);
        Sub(krDest);
        BitRot(krDest);
        WordRot(krDest);
        XOR(krDest, kl);
    }

    public static void fe(ushort[] r, ushort[] l, ushort[] k)
    {
        ushort[] temp = new ushort[NB];
        for (ushort i = 0; i < NB; i++)
            temp[i] = r[i];

        Add(r, k);
        Sub(r);
        BitRot(r);
        WordRot(l);
        XOR(r, l);

        for (ushort i = 0; i < NB; i++)
            l[i] = temp[i];

    }

    void fd(ushort[] r, ushort[] l, ushort[] k)
    {
        ushort[] temp = new ushort[NB];
        for (ushort i = 0; i < NB; i++)
            temp[i] = r[i];

        Add(r, k);
        Sub(r);
        BitRot(r);
        XOR(r, l);
        InvWordRot(r);

        for (ushort i = 0; i < NB; i++)
            l[i] = temp[i];
    }

    void KeySchedul(ushort[] mkey, ushort[,] rkey)
    {
        ushort temp,i;
        ushort[] c = new ushort[NB];
        for (i = 1; i < NB; i++) c[i] = 0;
        for (i = 0; i < 2 * NB; i++) rkey[0,i] = mkey[i];
        for (i = 1; i <= (NBROUND >> 2); i++)
        {
            c[0] = i;
            
            //[KLi , KRi ] = FK (KLi−1 , KRi−1 , C(i));
    //        fk(rkey[i - 1],rkey[i - 1] + 3, rkey[i], rkey[i] + 3, c);
        }

        for (ushort j = 0; j < NB; j++)
        {
            temp = rkey[NBROUND >> 2,j];
            rkey[NBROUND >> 2,j] = rkey[NBROUND >> 2,j + 3];
            rkey[NBROUND >> 2,j + 3] = temp;
        }

        for (; i < NBROUND; i++)
        {
            c[0] = (ushort)(NBROUND - i);
            //[KLi , KRi ] = FK (KLi−1 , KRi−1 , C(r − i));
  //          fk(rkey[i - 1], rkey[i - 1] + 3, rkey[i], rkey[i] + 3, c);
        }
    }

    public static void Decrypt(ushort[] state, ushort[,] rkey)
    {
        ushort i, temp;

        for (i = NBROUND; i > ((NBROUND + 1) >> 2); i--)
        {
   //         fd(state, state + 3, rkey[i - 1] + 3);
        }
        for (; i >= 1; i--)
        {
  //          fd(state, state + 3, rkey[i - 1]);
        }
        for (i = 0; i < NB; i++)
        {
            temp = state[i];
            state[i] = state[i + 3];
            state[i + 3] = temp;
        }
    }

    public static void Encrypt(ushort[] state, ushort[,] rkey)
    {
        ushort i, temp;
        for (i = 1; i <= ((NBROUND + 1) >> 2); i++)
        {
    //        fe(state, state + 3, rkey[i - 1]);
        }
        for (; i <= NBROUND; i++)
        {
  //          fe(state, state + 3, rkey[i - 1] + 3);
        }

        for (i = 0; i < NB; i++)
        {
            temp = state[i];
            state[i] = state[i + 3];
            state[i + 3] = temp;
        }
    }

}

