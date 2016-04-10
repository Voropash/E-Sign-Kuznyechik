using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuznec
{
    class Signer
    {
        private BigInteger p = new BigInteger();
        private BigInteger a = new BigInteger();
        private BigInteger b = new BigInteger();
        private BigInteger n = new BigInteger();
        private byte[] xG;
        private byte[] h0;
        private ECPoint G = new ECPoint();
        private BitArray R0 = new BitArray(128);

        public Signer(BigInteger p, BigInteger a, BigInteger b, BigInteger n, byte[] xG, byte[] h0)
        {
            this.a = a;
            this.b = b;
            this.n = n;
            this.p = p;
            this.xG = xG;
            this.h0 = h0;
        }

        // Шифруем
        private BitArray kuznech(BitArray message, BitArray prev_hash, int n = 128, int m = 256)
        {
            BitArray result = new BitArray(0);
            kuznec k = new kuznec();
            ulong[] prev_hash_long = Helpers.GetUlongArrayFromBitArray256(prev_hash);

            var R = R0;
            var P1 = Helpers.getFirstN(message, n);
            var P2 = Helpers.getLastN(message, n);

            P1 = P1.Xor(R);
            ulong[] P1_long = Helpers.GetUlongArrayFromBitArray128(P1);
            var cifr = k.start(P1_long, prev_hash_long);
            result = Helpers.bitArrayConcat(new BitArray(BitConverter.GetBytes(cifr[0])), new BitArray(BitConverter.GetBytes(cifr[1])));
            R = result;

            P2 = P2.Xor(R);
            ulong[] P2_long = Helpers.GetUlongArrayFromBitArray128(P2);
            cifr = k.start(P2_long, prev_hash_long);
            result = Helpers.bitArrayConcat(result, 
                Helpers.bitArrayConcat(new BitArray(BitConverter.GetBytes(cifr[0])), new BitArray(BitConverter.GetBytes(cifr[1]))));
            
            return result;
        }

        private byte[] crypt(byte[] m, byte[] h0)
        {
            int n = 256;
            BitArray message = new BitArray(m);
            BitArray h = new BitArray(h0);
            BitArray result = new BitArray(0);

            int j = 0;
            while (true)
            {
                BitArray mBlock = new BitArray(n);
                int i = 0;
                while ((i + j * n < message.Count) && (i < n))
                {
                    mBlock[i] = message[i + j * n];
                    i++;
                }

                h = kuznech(mBlock, h, 128, n).Xor(mBlock);

                if (i + j * n >= message.Count) { break; }
                j++;
            }

            var res_b = new byte[h.Count];
            h.CopyTo(res_b, 0);
            return res_b;
        }

        //подписываем сообщение
        public string SingIt(byte[] h, BigInteger d)
        {
            BigInteger alpha = new BigInteger(crypt(h, h0));
            BigInteger e = alpha % n;
            if (e == 0)
                e = 1;
            BigInteger k = new BigInteger();
            ECPoint C=new ECPoint();
            BigInteger r=new BigInteger();
            BigInteger s = new BigInteger();
            do
            {
                do
                {
                    k.genRandomBits(n.bitCount(), new Random());
                } while ((k < 0) || (k > n));
                C = ECPoint.multiply(k, G);
                r = C.x % n;
                s = ((r * d) + (k * e)) % n;
            } while ((r == 0)||(s==0));
            string Rvector = Helpers.padding(r.ToHexString(),n.bitCount()/4);
            string Svector = Helpers.padding(s.ToHexString(), n.bitCount() / 4);
            return Rvector + Svector;
        }

        //проверяем подпись 
        public bool SingVerify(byte[] H, string sing, ECPoint Q)
        {
            string Rvector = sing.Substring(0, n.bitCount() / 4);
            string Svector = sing.Substring(n.bitCount() / 4, n.bitCount() / 4);
            BigInteger r = new BigInteger(Rvector, 16);
            BigInteger s = new BigInteger(Svector, 16);
            if ((r < 1) || (r > (n - 1)) || (s < 1) || (s > (n - 1)))
                return false;
            BigInteger alpha = new BigInteger(crypt(H, h0));
            BigInteger e = alpha % n;
            if (e == 0)
                e = 1;
            BigInteger v = e.modInverse(n);
            BigInteger z1 = (s * v) % n;
            BigInteger z2 = n + ((-(r * v)) % n);
            this.G = dec();
            ECPoint A = ECPoint.multiply(z1, G);
            ECPoint B = ECPoint.multiply(z2, Q);
            ECPoint C = A + B;
            BigInteger R = C.x % n;
            if (R == r)
                return true;
            else
                return false;
        }

        public BigInteger GetPrivateKey(int BitSize)
        {
            BigInteger d = new BigInteger();
            do
            {
                d.genRandomBits(BitSize, new Random());
            } while ((d < 0) || (d > n));
            return d;
        }

        public ECPoint GetPublicKey(BigInteger d)
        {
            ECPoint G = dec();
            ECPoint Q = ECPoint.multiply(d, G);
            return Q;
        }

        public ECPoint dec()
        {
            byte y = xG[0];
            byte[] x = new byte[xG.Length - 1];
            Array.Copy(xG, 1, x, 0, xG.Length - 1);
            BigInteger Xcord = new BigInteger(x);
            BigInteger temp = (Xcord * Xcord * Xcord + a * Xcord + b) % p;
            BigInteger beta = Helpers.ModSqrt(temp, p);
            BigInteger Ycord = new BigInteger();
            if ((beta % 2) == (y % 2))
                Ycord = beta;
            else
                Ycord = p - beta;
            ECPoint G = new ECPoint();
            G.a = a;
            G.b = b;
            G.FieldChar = p;
            G.x = Xcord;
            G.y = Ycord;
            this.G = G;
            return G;
        }


    }
}
