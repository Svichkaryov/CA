class SpeckCipher : ICipher 
{
    public static int ROUNDS = 4;

    public static int RotateRight16(ushort Value, byte Count)
    {
        return ((Value >> Count) | (Value << (16 - Count))) & 0xffff;
    }

    public static int RotateLeft16(ushort Value, byte Count)
    {
        return ((Value << Count) | (Value >> (16 - Count))) & 0xffff;
    }

    public static void speck_round(ref ushort x, ref ushort y, ushort k)
    {
        x = (ushort)((RotateRight16(x, 7)) | (RotateLeft16(x,(16 - 7)))); // x = ROTR(x, 7)
        x += y;
        x ^= k;
        y = (ushort)((RotateLeft16(y,2)) | (RotateRight16(y,(16 - 2)))); // y = ROTL(y, 2)
        y ^= x;
    }

    public static void speck_block(ushort[] plaintext, ushort[] key, ushort[] ciphertext)
    {
        ciphertext[0] = plaintext[0];
        ciphertext[1] = plaintext[1];
        ushort b = key[0];
        ushort[] a = new ushort[3];
        a[0] = key[1]; // ring buffer
        a[1] = key[2];
        a[2] = key[3];
        for (ushort i = 0; i < ROUNDS; i++)
        {
            speck_round(ref ciphertext[1], ref ciphertext[0], b);
            speck_round(ref a[i % 3],ref b, i);
        }
    }
}