using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuznec
{
    class Helpers
    {
        public static ulong[] GetUlongArrayFromBitArray256(BitArray bitArray)
        {
            var v12 = GetUlongArrayFromBitArray128(getFirstN(bitArray, 128));
            var v34 = GetUlongArrayFromBitArray128(getLastN(bitArray, 128));
            return new ulong[4] { v12.First(), v12.Last(), v34.First(), v34.Last()};
        }

        public static ulong[] GetUlongArrayFromBitArray128(BitArray bitArray)
        {
            var firstPart = getFirstN(bitArray, 64);
            var lastPart = getLastN(bitArray, 64);


            var array = new int[2];
            firstPart.CopyTo(array, 0);
            var long1 = (uint)array[0] + ((ulong)(uint)array[1] << 32);

            lastPart.CopyTo(array, 0);
            var long2 = (uint)array[0] + ((ulong)(uint)array[1] << 32);

            ulong[] result = new ulong[2] { long1, long2 };
            return result;
        }

        public static BitArray bitArrayConcat(BitArray current, BitArray before)
        {
            var bools = new bool[current.Count + before.Count];
            before.CopyTo(bools, 0);
            current.CopyTo(bools, before.Count);
            return new BitArray(bools);
        }

        public static BitArray getFirstN(BitArray array, int n)
        {
            var bools = new bool[array.Count];
            array.CopyTo(bools, 0);
            var list = bools.ToList();
            var result = new bool[n];
            list.CopyTo(0, result, 0, n);
            return new BitArray(result);
        }

        public static BitArray getLastN(BitArray array, int n)
        {
            var bools = new bool[array.Count];
            array.CopyTo(bools, 0);
            var list = bools.ToList();
            var result = new bool[n];
            list.CopyTo(array.Count - n, result, 0, n);
            return new BitArray(result);
        }

        public static BitArray moveNLeftAndAdd(BitArray array, int n, BitArray add)
        {
            int m = array.Count;
            var bools = new bool[array.Count];
            getLastN(array, m - n).CopyTo(bools, 0);
            getFirstN(add, n).CopyTo(bools, m - n);
            return new BitArray(bools);
        }

        public static byte[] FromHexStringToByte(string input)
        {
            byte[] data = new byte[input.Length / 2];
            string HexByte = "";
            for (int i = 0; i < data.Length; i++)
            {
                HexByte = input.Substring(i * 2, 2);
                data[i] = Convert.ToByte(HexByte, 16);
            }
            return data;
        }

        public static string padding(string input, int size)
        {
            if (input.Length < size)
            {
                do
                {
                    input = "0" + input;
                } while (input.Length < size);
            }
            return input;
        }

        public static BigInteger ModSqrt(BigInteger a, BigInteger q)
        {
            BigInteger b = new BigInteger();
            do
            {
                b.genRandomBits(255, new Random());
            } while (Legendre(b, q) == 1);
            BigInteger s = 0;
            BigInteger t = q - 1;
            while ((t & 1) != 1)
            {
                s++;
                t = t >> 1;
            }
            BigInteger InvA = a.modInverse(q);
            BigInteger c = b.modPow(t, q);
            BigInteger r = a.modPow(((t + 1) / 2), q);
            BigInteger d = new BigInteger();
            for (int i = 1; i < s; i++)
            {
                BigInteger temp = 2;
                temp = temp.modPow((s - i - 1), q);
                d = (r.modPow(2, q) * InvA).modPow(temp, q);
                if (d == (q - 1))
                    r = (r * c) % q;
                c = c.modPow(2, q);
            }
            return r;
        }

        public static BigInteger Legendre(BigInteger a, BigInteger q)
        {
            return a.modPow((q - 1) / 2, q);
        }

    }
}
