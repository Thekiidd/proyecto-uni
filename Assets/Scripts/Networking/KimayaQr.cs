using System;
using System.Collections.Generic;
using System.Text;

namespace Kimaya.Networking
{
    /// <summary>
    /// QR Model 2, modo byte, versiones 1 a 40.
    /// Algoritmo de Project Nayuki (licencia MIT): https://www.nayuki.io/page/qr-code-generator-library
    /// </summary>
    public sealed class KimayaQr
    {
        public int Size { get; }

        readonly bool[,] modules;

        KimayaQr(int size, bool[,] modules)
        {
            Size = size;
            this.modules = modules;
        }

        public bool Dark(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Size && y < Size && modules[y, x];
        }

        public static KimayaQr Encode(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text ?? "");
            int version = 1;
            int usedBits = 0;
            for (; version <= 40; version++)
            {
                int countBits = CountBits(version);
                usedBits = 4 + countBits + data.Length * 8;
                if (data.Length < (1 << countBits) && usedBits <= DataCodewords(version, 1) * 8)
                    break;
                if (version == 40) throw new ArgumentException("El texto no cabe en un QR");
            }

            int ecl = 1;
            for (int higher = 1; higher <= 3; higher++)
            {
                if (usedBits <= DataCodewords(version, higher) * 8) ecl = higher;
            }

            var bits = new List<int>(usedBits + 16);
            AppendBits(bits, 0x4, 4);
            AppendBits(bits, data.Length, CountBits(version));
            foreach (byte value in data) AppendBits(bits, value, 8);

            int capacity = DataCodewords(version, ecl) * 8;
            AppendBits(bits, 0, Math.Min(4, capacity - bits.Count));
            AppendBits(bits, 0, (8 - bits.Count % 8) % 8);
            var pads = new[] { 0xEC, 0x11 };
            int pad = 0;
            while (bits.Count < capacity)
            {
                AppendBits(bits, pads[pad], 8);
                pad ^= 1;
            }

            var codewords = new byte[bits.Count / 8];
            for (int i = 0; i < bits.Count; i++)
                codewords[i >> 3] |= (byte)(bits[i] << (7 - (i & 7)));

            return new KimayaQr(version * 4 + 17, Draw(version, ecl, codewords));
        }

        static bool[,] Draw(int version, int ecl, byte[] data)
        {
            int size = version * 4 + 17;
            var modules = new bool[size, size];
            var function = new bool[size, size];

            for (int i = 0; i < size; i++)
            {
                SetFunction(modules, function, 6, i, i % 2 == 0);
                SetFunction(modules, function, i, 6, i % 2 == 0);
            }
            DrawFinder(modules, function, size, 3, 3);
            DrawFinder(modules, function, size, size - 4, 3);
            DrawFinder(modules, function, size, 3, size - 4);
            int[] align = AlignmentPositions(version, size);
            for (int i = 0; i < align.Length; i++)
            {
                for (int j = 0; j < align.Length; j++)
                {
                    if ((i == 0 && j == 0) || (i == 0 && j == align.Length - 1) || (i == align.Length - 1 && j == 0))
                        continue;
                    DrawAlignment(modules, function, align[i], align[j]);
                }
            }
            DrawFormat(modules, function, size, ecl, 0);
            DrawVersion(modules, function, size, version);

            byte[] all = AddEcc(version, ecl, data);
            int bit = 0;
            for (int column = size - 1; column > 0; column -= 2)
            {
                int right = column <= 6 ? column - 1 : column;
                bool upward = ((right + 1) & 2) == 0;
                for (int vert = 0; vert < size; vert++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        int x = right - j;
                        int y = upward ? size - 1 - vert : vert;
                        if (!function[y, x] && bit < all.Length * 8)
                        {
                            modules[y, x] = ((all[bit >> 3] >> (7 - (bit & 7))) & 1) != 0;
                            bit++;
                        }
                    }
                }
            }

            int mask = 0;
            int best = int.MaxValue;
            for (int i = 0; i < 8; i++)
            {
                ApplyMask(modules, function, size, i);
                DrawFormat(modules, function, size, ecl, i);
                int penalty = Penalty(modules, size);
                if (penalty < best)
                {
                    mask = i;
                    best = penalty;
                }
                ApplyMask(modules, function, size, i);
            }
            ApplyMask(modules, function, size, mask);
            DrawFormat(modules, function, size, ecl, mask);
            return modules;
        }

        static void DrawFinder(bool[,] modules, bool[,] function, int size, int cx, int cy)
        {
            for (int dy = -4; dy <= 4; dy++)
            {
                for (int dx = -4; dx <= 4; dx++)
                {
                    int x = cx + dx;
                    int y = cy + dy;
                    if (x < 0 || y < 0 || x >= size || y >= size) continue;
                    int chebyshev = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    SetFunction(modules, function, x, y, chebyshev != 2 && chebyshev != 4);
                }
            }
        }

        static void DrawAlignment(bool[,] modules, bool[,] function, int cx, int cy)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    int chebyshev = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    SetFunction(modules, function, cx + dx, cy + dy, chebyshev != 1);
                }
            }
        }

        static void DrawFormat(bool[,] modules, bool[,] function, int size, int ecl, int mask)
        {
            int data = (FormatBits[ecl] << 3) | mask;
            int rem = data;
            for (int i = 0; i < 10; i++) rem = (rem << 1) ^ ((rem >> 9) * 0x537);
            int bits = ((data << 10) | rem) ^ 0x5412;
            for (int i = 0; i < 6; i++) SetFunction(modules, function, 8, i, Bit(bits, i));
            SetFunction(modules, function, 8, 7, Bit(bits, 6));
            SetFunction(modules, function, 8, 8, Bit(bits, 7));
            SetFunction(modules, function, 7, 8, Bit(bits, 8));
            for (int i = 9; i < 15; i++) SetFunction(modules, function, 14 - i, 8, Bit(bits, i));
            for (int i = 0; i < 8; i++) SetFunction(modules, function, size - 1 - i, 8, Bit(bits, i));
            for (int i = 8; i < 15; i++) SetFunction(modules, function, 8, size - 15 + i, Bit(bits, i));
            SetFunction(modules, function, 8, size - 8, true);
        }

        static void DrawVersion(bool[,] modules, bool[,] function, int size, int version)
        {
            if (version < 7) return;
            int rem = version;
            for (int i = 0; i < 12; i++) rem = (rem << 1) ^ ((rem >> 11) * 0x1F25);
            int bits = (version << 12) | rem;
            for (int i = 0; i < 18; i++)
            {
                bool bit = Bit(bits, i);
                int a = size - 11 + i % 3;
                int b = i / 3;
                SetFunction(modules, function, a, b, bit);
                SetFunction(modules, function, b, a, bit);
            }
        }

        static byte[] AddEcc(int version, int ecl, byte[] data)
        {
            int numBlocks = NumBlocks[ecl][version];
            int blockEccLen = EccPerBlock[ecl][version];
            int raw = RawModules(version) / 8;
            int numShort = numBlocks - raw % numBlocks;
            int shortLen = raw / numBlocks;
            var divisor = ReedSolomonDivisor(blockEccLen);
            var blocks = new byte[numBlocks][];
            int k = 0;
            for (int i = 0; i < numBlocks; i++)
            {
                int dataLen = shortLen - blockEccLen + (i < numShort ? 0 : 1);
                var dat = new byte[dataLen + (i < numShort ? 1 : 0) + blockEccLen];
                Buffer.BlockCopy(data, k, dat, 0, dataLen);
                k += dataLen;
                byte[] ecc = ReedSolomonRemainder(dat, dataLen, divisor);
                if (i < numShort) dat[dataLen] = 0;
                int eccAt = dataLen + (i < numShort ? 1 : 0);
                Buffer.BlockCopy(ecc, 0, dat, eccAt, blockEccLen);
                blocks[i] = dat;
            }

            var result = new byte[raw];
            int at = 0;
            for (int i = 0; i < blocks[0].Length; i++)
            {
                for (int j = 0; j < blocks.Length; j++)
                {
                    if (i == shortLen - blockEccLen && j < numShort) continue;
                    result[at++] = blocks[j][i];
                }
            }
            return result;
        }

        static void ApplyMask(bool[,] modules, bool[,] function, int size, int mask)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!function[y, x] && MaskBit(mask, x, y) == 0)
                        modules[y, x] = !modules[y, x];
                }
            }
        }

        static int MaskBit(int mask, int x, int y)
        {
            switch (mask)
            {
                case 0: return (x + y) % 2;
                case 1: return y % 2;
                case 2: return x % 3;
                case 3: return (x + y) % 3;
                case 4: return (x / 3 + y / 2) % 2;
                case 5: return x * y % 2 + x * y % 3;
                case 6: return (x * y % 2 + x * y % 3) % 2;
                default: return ((x + y) % 2 + x * y % 3) % 2;
            }
        }

        static int Penalty(bool[,] modules, int size)
        {
            int result = 0;
            for (int y = 0; y < size; y++)
                result += RunPenalty(modules, size, true, y);
            for (int x = 0; x < size; x++)
                result += RunPenalty(modules, size, false, x);
            for (int y = 0; y < size - 1; y++)
            {
                for (int x = 0; x < size - 1; x++)
                {
                    bool color = modules[y, x];
                    if (color == modules[y, x + 1] && color == modules[y + 1, x] && color == modules[y + 1, x + 1])
                        result += 3;
                }
            }
            int dark = 0;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    if (modules[y, x]) dark++;
            int total = size * size;
            int k = (Math.Abs(dark * 20 - total * 10) + total - 1) / total - 1;
            result += k * 10;
            return result;
        }

        static int RunPenalty(bool[,] modules, int size, bool row, int fixedIndex)
        {
            int result = 0;
            bool runColor = false;
            int run = 0;
            var history = new int[7];
            for (int i = 0; i < size; i++)
            {
                bool color = row ? modules[fixedIndex, i] : modules[i, fixedIndex];
                if (color == runColor)
                {
                    run++;
                    if (run == 5) result += 3;
                    else if (run > 5) result += 1;
                }
                else
                {
                    AddHistory(history, size, run);
                    if (!runColor) result += CountPatterns(history) * 40;
                    runColor = color;
                    run = 1;
                }
            }
            result += Terminate(history, size, runColor, run) * 40;
            return result;
        }

        static int Terminate(int[] history, int size, bool color, int run)
        {
            if (color)
            {
                AddHistory(history, size, run);
                run = 0;
            }
            run += size;
            AddHistory(history, size, run);
            return CountPatterns(history);
        }

        static void AddHistory(int[] history, int size, int run)
        {
            if (history[0] == 0) run += size;
            for (int i = 6; i > 0; i--) history[i] = history[i - 1];
            history[0] = run;
        }

        static int CountPatterns(int[] history)
        {
            int n = history[1];
            bool core = n > 0 && history[2] == n && history[3] == n * 3 && history[4] == n && history[5] == n;
            int count = 0;
            if (core && history[0] >= n * 4 && history[6] >= n) count++;
            if (core && history[6] >= n * 4 && history[0] >= n) count++;
            return count;
        }

        static int[] AlignmentPositions(int version, int size)
        {
            if (version == 1) return new int[0];
            int count = version / 7 + 2;
            int step = (version * 8 + count * 3 + 5) / (count * 4 - 4) * 2;
            var result = new int[count];
            for (int i = 0; i < count - 1; i++) result[i] = size - 7 - i * step;
            result[count - 1] = 6;
            Array.Reverse(result);
            return result;
        }

        static int RawModules(int version)
        {
            int result = (16 * version + 128) * version + 64;
            if (version >= 2)
            {
                int count = version / 7 + 2;
                result -= (25 * count - 10) * count - 55;
                if (version >= 7) result -= 36;
            }
            return result;
        }

        static int DataCodewords(int version, int ecl)
        {
            return RawModules(version) / 8 - EccPerBlock[ecl][version] * NumBlocks[ecl][version];
        }

        static int CountBits(int version)
        {
            int[] widths = { 8, 16, 16 };
            return widths[(version + 7) / 17];
        }

        static byte[] ReedSolomonDivisor(int degree)
        {
            var result = new byte[degree];
            result[degree - 1] = 1;
            int root = 1;
            for (int i = 0; i < degree; i++)
            {
                for (int j = 0; j < degree; j++)
                {
                    result[j] = (byte)Multiply(result[j], root);
                    if (j + 1 < degree) result[j] ^= result[j + 1];
                }
                root = Multiply(root, 0x02);
            }
            return result;
        }

        static byte[] ReedSolomonRemainder(byte[] data, int length, byte[] divisor)
        {
            var result = new byte[divisor.Length];
            for (int i = 0; i < length; i++)
            {
                int factor = data[i] ^ result[0];
                Buffer.BlockCopy(result, 1, result, 0, result.Length - 1);
                result[result.Length - 1] = 0;
                for (int j = 0; j < divisor.Length; j++)
                    result[j] ^= (byte)Multiply(divisor[j], factor);
            }
            return result;
        }

        static int Multiply(int x, int y)
        {
            int z = 0;
            for (int i = 7; i >= 0; i--)
            {
                z = (z << 1) ^ ((z >> 7) * 0x11D);
                z ^= ((y >> i) & 1) * x;
            }
            return z;
        }

        static void AppendBits(List<int> bits, int value, int count)
        {
            for (int i = count - 1; i >= 0; i--) bits.Add((value >> i) & 1);
        }

        static void SetFunction(bool[,] modules, bool[,] function, int x, int y, bool dark)
        {
            modules[y, x] = dark;
            function[y, x] = true;
        }

        static bool Bit(int value, int index)
        {
            return ((value >> index) & 1) != 0;
        }

        static readonly int[] FormatBits = { 1, 0, 3, 2 };

        static readonly int[][] EccPerBlock =
        {
            new[] { -1, 7, 10, 15, 20, 26, 18, 20, 24, 30, 18, 20, 24, 26, 30, 22, 24, 28, 30, 28, 28, 28, 28, 30, 30, 26, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 },
            new[] { -1, 10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26, 26, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28 },
            new[] { -1, 13, 22, 18, 26, 18, 24, 18, 22, 20, 24, 28, 26, 24, 20, 30, 24, 28, 28, 26, 30, 28, 30, 30, 30, 30, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 },
            new[] { -1, 17, 28, 22, 16, 22, 28, 26, 26, 24, 28, 24, 28, 22, 24, 24, 30, 28, 28, 26, 28, 30, 24, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 }
        };

        static readonly int[][] NumBlocks =
        {
            new[] { -1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 4, 4, 4, 4, 4, 6, 6, 6, 6, 7, 8, 8, 9, 9, 10, 12, 12, 12, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 22, 24, 25 },
            new[] { -1, 1, 1, 1, 2, 2, 4, 4, 4, 5, 5, 5, 8, 9, 9, 10, 10, 11, 13, 14, 16, 17, 17, 18, 20, 21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49 },
            new[] { -1, 1, 1, 2, 2, 4, 4, 6, 6, 8, 8, 8, 10, 12, 16, 12, 17, 16, 18, 21, 20, 23, 23, 25, 27, 29, 34, 34, 35, 38, 40, 43, 45, 48, 51, 53, 56, 59, 62, 65, 68 },
            new[] { -1, 1, 1, 2, 4, 4, 4, 5, 6, 8, 8, 11, 11, 16, 16, 18, 16, 19, 21, 25, 25, 25, 34, 30, 32, 35, 37, 40, 42, 45, 48, 51, 54, 57, 60, 63, 66, 70, 74, 77, 81 }
        };
    }
}
