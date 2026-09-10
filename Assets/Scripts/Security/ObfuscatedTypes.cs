using System;
using UnityEngine;

namespace Platformer.Security
{
    /// <summary>
    /// Tipo de dato entero seguro en memoria.
    /// Almacena el valor enmascarado mediante una clave XOR aleatoria unica por instancia,
    /// evitando que herramientas de escaneo de memoria (Cheat Engine) localicen el valor real.
    /// </summary>
    [Serializable]
    public struct ObfuscatedInt : IEquatable<ObfuscatedInt>
    {
        [SerializeField] private int _maskedValue;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;

        public ObfuscatedInt(int value)
        {
            _key = GenerateRandomKey();
            _maskedValue = value ^ _key;
            _checksum = ComputeChecksum(_maskedValue, _key);
        }

        public int Value
        {
            get
            {
                if (ComputeChecksum(_maskedValue, _key) != _checksum)
                {
                    Debug.LogWarning("[Security Alert] Se detectó alteración de memoria en ObfuscatedInt. Restaurando valor seguro.");
                    _maskedValue = 0 ^ _key;
                    _checksum = ComputeChecksum(_maskedValue, _key);
                    return 0;
                }
                return _maskedValue ^ _key;
            }
            set
            {
                _key = GenerateRandomKey();
                _maskedValue = value ^ _key;
                _checksum = ComputeChecksum(_maskedValue, _key);
            }
        }

        private static int GenerateRandomKey()
        {
            int seed = Environment.TickCount ^ System.Threading.Thread.CurrentThread.ManagedThreadId;
            return (seed != 0) ? seed : 0x5A5A5A5A;
        }

        private static int ComputeChecksum(int masked, int key)
        {
            return (masked * 31) ^ (key * 17);
        }

        public static implicit operator int(ObfuscatedInt ob) => ob.Value;
        public static implicit operator ObfuscatedInt(int val) => new ObfuscatedInt(val);

        public static ObfuscatedInt operator +(ObfuscatedInt a, int b) => new ObfuscatedInt(a.Value + b);
        public static ObfuscatedInt operator -(ObfuscatedInt a, int b) => new ObfuscatedInt(a.Value - b);
        public static ObfuscatedInt operator *(ObfuscatedInt a, int b) => new ObfuscatedInt(a.Value * b);
        public static ObfuscatedInt operator /(ObfuscatedInt a, int b) => new ObfuscatedInt(a.Value / b);

        public static ObfuscatedInt operator ++(ObfuscatedInt a) => new ObfuscatedInt(a.Value + 1);
        public static ObfuscatedInt operator --(ObfuscatedInt a) => new ObfuscatedInt(a.Value - 1);

        public static bool operator ==(ObfuscatedInt a, ObfuscatedInt b) => a.Value == b.Value;
        public static bool operator !=(ObfuscatedInt a, ObfuscatedInt b) => a.Value != b.Value;
        public static bool operator ==(ObfuscatedInt a, int b) => a.Value == b;
        public static bool operator !=(ObfuscatedInt a, int b) => a.Value != b;

        public bool Equals(ObfuscatedInt other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ObfuscatedInt ob && Equals(ob);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }

    /// <summary>
    /// Tipo de dato flotante seguro en memoria.
    /// Convierte los bits del float a entero y los enmascara con una clave XOR aleatoria.
    /// </summary>
    [Serializable]
    public struct ObfuscatedFloat : IEquatable<ObfuscatedFloat>
    {
        [SerializeField] private int _maskedBits;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;

        public ObfuscatedFloat(float value)
        {
            _key = Environment.TickCount ^ 0x3C3C3C3C;
            int bits = BitConverter.SingleToInt32Bits(value);
            _maskedBits = bits ^ _key;
            _checksum = (_maskedBits * 31) ^ (_key * 17);
        }

        public float Value
        {
            get
            {
                if (((_maskedBits * 31) ^ (_key * 17)) != _checksum)
                {
                    Debug.LogWarning("[Security Alert] Se detectó alteración de memoria en ObfuscatedFloat.");
                    return 0f;
                }
                int bits = _maskedBits ^ _key;
                return BitConverter.Int32BitsToSingle(bits);
            }
            set
            {
                _key = Environment.TickCount ^ 0x3C3C3C3C;
                int bits = BitConverter.SingleToInt32Bits(value);
                _maskedBits = bits ^ _key;
                _checksum = (_maskedBits * 31) ^ (_key * 17);
            }
        }

        public static implicit operator float(ObfuscatedFloat ob) => ob.Value;
        public static implicit operator ObfuscatedFloat(float val) => new ObfuscatedFloat(val);

        public static ObfuscatedFloat operator +(ObfuscatedFloat a, float b) => new ObfuscatedFloat(a.Value + b);
        public static ObfuscatedFloat operator -(ObfuscatedFloat a, float b) => new ObfuscatedFloat(a.Value - b);

        public bool Equals(ObfuscatedFloat other) => Mathf.Approximately(Value, other.Value);
        public override bool Equals(object obj) => obj is ObfuscatedFloat ob && Equals(ob);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }
}
