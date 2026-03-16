using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab16_V23
{
    // "Довге число" в десяткових цифрах (динамічна структура)
    // digits зберігаються у зворотному порядку: 123 -> [3,2,1]
    public sealed class LongNumber : IComparable<LongNumber>
    {
        private List<byte> digits; // 0..9
        private bool isNegative;

        // 1) Конструктор за замовчуванням
        public LongNumber()
        {
            digits = new List<byte> { 0 };
            isNegative = false;
        }

        // 2) Конструктор з long
        public LongNumber(long value)
        {
            digits = new List<byte>();
            if (value < 0)
            {
                isNegative = true;
                value = -value;
            }
            else isNegative = false;

            if (value == 0) digits.Add(0);
            while (value > 0)
            {
                digits.Add((byte)(value % 10));
                value /= 10;
            }
            Normalize();
        }

        // 3) Конструктор зі строки
        public LongNumber(string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            s = s.Trim();
            if (s.Length == 0) throw new ArgumentException("Порожній рядок", nameof(s));

            isNegative = false;
            int i = 0;
            if (s[0] == '-')
            {
                isNegative = true;
                i = 1;
            }
            else if (s[0] == '+')
            {
                i = 1;
            }

            if (i >= s.Length) throw new ArgumentException("Немає цифр у рядку", nameof(s));

            digits = new List<byte>(s.Length - i);
            for (int j = s.Length - 1; j >= i; j--)
            {
                if (s[j] < '0' || s[j] > '9')
                    throw new ArgumentException("Некоректний символ у числі", nameof(s));
                digits.Add((byte)(s[j] - '0'));
            }

            Normalize();
        }

        // 4) Конструктор копіювання
        public LongNumber(LongNumber other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            isNegative = other.isNegative;
            digits = new List<byte>(other.digits);
            Normalize();
        }

        // 5) "Деструктор" у C# (finalizer) — як вимагає умова
        ~LongNumber()
        {
            // В C# зазвичай НЕ треба, але для методички — показуємо наявність.
        }

        // Метод зміни "складових частин" — наприклад, змінити цифру за позицією (0 = найменша)
        public void SetDigit(int position, byte digit)
        {
            if (digit > 9) throw new ArgumentOutOfRangeException(nameof(digit));
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));

            while (digits.Count <= position) digits.Add(0);
            digits[position] = digit;
            Normalize();
        }

        // Метод зміни знаку
        public void SetNegative(bool negative)
        {
            isNegative = negative && !IsZero();
        }

        public override string ToString()
        {
            Normalize();
            var chars = new char[digits.Count + (isNegative ? 1 : 0)];
            int idx = 0;
            if (isNegative) chars[idx++] = '-';
            for (int i = digits.Count - 1; i >= 0; i--)
                chars[idx++] = (char)('0' + digits[i]);
            return new string(chars);
        }

        private bool IsZero() => digits.Count == 1 && digits[0] == 0;

        private void Normalize()
        {
            // прибрати провідні нулі
            for (int i = digits.Count - 1; i > 0 && digits[i] == 0; i--)
                digits.RemoveAt(i);

            if (IsZero()) isNegative = false;
        }

        // ------------------- Порівняння -------------------
        public int CompareTo(LongNumber? other)
        {
            if (other == null) return 1;

            if (isNegative != other.isNegative)
                return isNegative ? -1 : 1;

            int cmpAbs = CompareAbs(this, other);
            return isNegative ? -cmpAbs : cmpAbs;
        }

        private static int CompareAbs(LongNumber a, LongNumber b)
        {
            a.Normalize(); b.Normalize();
            if (a.digits.Count != b.digits.Count)
                return a.digits.Count.CompareTo(b.digits.Count);

            for (int i = a.digits.Count - 1; i >= 0; i--)
            {
                if (a.digits[i] != b.digits[i])
                    return a.digits[i].CompareTo(b.digits[i]);
            }
            return 0;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not LongNumber other) return false;
            return this == other;
        }

        public override int GetHashCode()
        {
            Normalize();
            unchecked
            {
                int h = isNegative ? 17 : 19;
                // не дуже “крипто”, але для лабораторної норм
                for (int i = 0; i < digits.Count; i++)
                    h = h * 31 + digits[i];
                return h;
            }
        }

        public static bool operator ==(LongNumber? a, LongNumber? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;

            a.Normalize(); b.Normalize();
            if (a.isNegative != b.isNegative) return false;
            if (a.digits.Count != b.digits.Count) return false;
            for (int i = 0; i < a.digits.Count; i++)
                if (a.digits[i] != b.digits[i]) return false;
            return true;
        }

        public static bool operator !=(LongNumber? a, LongNumber? b) => !(a == b);
        public static bool operator <(LongNumber a, LongNumber b) => a.CompareTo(b) < 0;
        public static bool operator >(LongNumber a, LongNumber b) => a.CompareTo(b) > 0;
        public static bool operator <=(LongNumber a, LongNumber b) => a.CompareTo(b) <= 0;
        public static bool operator >=(LongNumber a, LongNumber b) => a.CompareTo(b) >= 0;

        // ------------------- Арифметика -------------------
        public static LongNumber operator +(LongNumber a, LongNumber b)
        {
            if (a.isNegative == b.isNegative)
            {
                var sum = AddAbs(a, b);
                sum.isNegative = a.isNegative;
                sum.Normalize();
                return sum;
            }
            // різні знаки => віднімання модулів
            int cmp = CompareAbs(a, b);
            if (cmp == 0) return new LongNumber(0);
            if (cmp > 0)
            {
                var res = SubAbs(a, b);
                res.isNegative = a.isNegative;
                res.Normalize();
                return res;
            }
            else
            {
                var res = SubAbs(b, a);
                res.isNegative = b.isNegative;
                res.Normalize();
                return res;
            }
        }

        public static LongNumber operator -(LongNumber a, LongNumber b)
        {
            var nb = new LongNumber(b);
            nb.isNegative = !nb.isNegative;
            nb.Normalize();
            return a + nb;
        }

        public static LongNumber operator *(LongNumber a, LongNumber b)
        {
            a.Normalize(); b.Normalize();
            var res = new LongNumber();
            res.digits = Enumerable.Repeat((byte)0, a.digits.Count + b.digits.Count).ToList();
            res.isNegative = a.isNegative ^ b.isNegative;

            for (int i = 0; i < a.digits.Count; i++)
            {
                int carry = 0;
                for (int j = 0; j < b.digits.Count; j++)
                {
                    int idx = i + j;
                    int cur = res.digits[idx] + a.digits[i] * b.digits[j] + carry;
                    res.digits[idx] = (byte)(cur % 10);
                    carry = cur / 10;
                }
                int k = i + b.digits.Count;
                while (carry > 0)
                {
                    int cur = res.digits[k] + carry;
                    res.digits[k] = (byte)(cur % 10);
                    carry = cur / 10;
                    k++;
                    if (k >= res.digits.Count) res.digits.Add(0);
                }
            }

            res.Normalize();
            return res;
        }

        // Частка і залишок від ділення: a / b та a % b
        public static (LongNumber Quotient, LongNumber Remainder) DivRem(LongNumber a, LongNumber b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            b.Normalize();
            if (b.IsZero()) throw new DivideByZeroException("Ділення на нуль");

            a.Normalize();

            bool negQ = a.isNegative ^ b.isNegative;
            bool negR = a.isNegative;

            var aa = Abs(a);
            var bb = Abs(b);

            int cmp = CompareAbs(aa, bb);
            if (cmp < 0)
            {
                // частка 0, залишок = a
                var q0 = new LongNumber(0);
                var r0 = new LongNumber(a);
                r0.isNegative = negR && !r0.IsZero();
                r0.Normalize();
                return (q0, r0);
            }

            // Довге ділення у базі 10
            var quotientDigits = new List<byte>();
            var remainder = new LongNumber(0);

            for (int i = aa.digits.Count - 1; i >= 0; i--)
            {
                // remainder = remainder*10 + nextDigit
                remainder = remainder * new LongNumber(10) + new LongNumber(aa.digits[i].ToString());

                byte qDigit = 0;
                // підбираємо qDigit 0..9
                for (byte d = 9; d >= 1; d--)
                {
                    var test = bb * new LongNumber(d.ToString());
                    if (CompareAbs(test, remainder) <= 0)
                    {
                        qDigit = d;
                        remainder = remainder - test;
                        break;
                    }
                }
                quotientDigits.Add(qDigit);
            }

            // quotientDigits зараз у прямому порядку (старші -> молодші)
            var q = new LongNumber(0);
            q.digits = quotientDigits.AsEnumerable().Reverse().ToList();
            q.isNegative = negQ && !(q.digits.Count == 1 && q.digits[0] == 0);
            q.Normalize();

            remainder.isNegative = negR && !remainder.IsZero();
            remainder.Normalize();

            return (q, remainder);
        }

        public static LongNumber operator /(LongNumber a, LongNumber b) => DivRem(a, b).Quotient;
        public static LongNumber operator %(LongNumber a, LongNumber b) => DivRem(a, b).Remainder;

        private static LongNumber Abs(LongNumber x)
        {
            var r = new LongNumber(x);
            r.isNegative = false;
            r.Normalize();
            return r;
        }

        private static LongNumber AddAbs(LongNumber a, LongNumber b)
        {
            var res = new LongNumber();
            res.digits = new List<byte>();
            int carry = 0;
            int n = Math.Max(a.digits.Count, b.digits.Count);

            for (int i = 0; i < n; i++)
            {
                int da = i < a.digits.Count ? a.digits[i] : 0;
                int db = i < b.digits.Count ? b.digits[i] : 0;
                int s = da + db + carry;
                res.digits.Add((byte)(s % 10));
                carry = s / 10;
            }
            while (carry > 0)
            {
                res.digits.Add((byte)(carry % 10));
                carry /= 10;
            }
            res.isNegative = false;
            res.Normalize();
            return res;
        }

        // |a| - |b|, припускаємо |a| >= |b|
        private static LongNumber SubAbs(LongNumber a, LongNumber b)
        {
            var res = new LongNumber();
            res.digits = new List<byte>();
            int borrow = 0;

            for (int i = 0; i < a.digits.Count; i++)
            {
                int da = a.digits[i] - borrow;
                int db = i < b.digits.Count ? b.digits[i] : 0;
                if (da < db)
                {
                    da += 10;
                    borrow = 1;
                }
                else borrow = 0;

                res.digits.Add((byte)(da - db));
            }
            res.isNegative = false;
            res.Normalize();
            return res;
        }
    }

    internal class Program
    {
        static void Main()
        {
            // 1) Масив довгих чисел
            LongNumber[] arr =
            {
                new LongNumber("9999999999"),
                new LongNumber("100"),
                new LongNumber("42"),
                new LongNumber("12"),
                new LongNumber("7"),
                new LongNumber("7"),
                new LongNumber("0"),
                new LongNumber("-5"),
            };

            Console.WriteLine("Масив long (за спаданням):");
            Array.Sort(arr, (x, y) => y.CompareTo(x)); // спадання
            Console.WriteLine(string.Join(", ", arr.Select(x => x.ToString())));
            Console.WriteLine();

            // 2) Демонстрація операцій (можеш поставити свої A,B)
            var A = new LongNumber("123456789012345678901234567890");
            var B = new LongNumber("9876543210");

            Console.WriteLine($"A = {A}");
            Console.WriteLine($"B = {B}");
            Console.WriteLine($"A + B = {A + B}");
            Console.WriteLine($"A - B = {A - B}");
            Console.WriteLine($"A * B = {A * B}");

            var (q, r) = LongNumber.DivRem(A, B);
            Console.WriteLine($"A / B = {q}");
            Console.WriteLine($"A % B = {r}");

            Console.WriteLine();
            Console.WriteLine("Порівняння:");
            Console.WriteLine($"A == B ? {A == B}");
            Console.WriteLine($"A != B ? {A != B}");
            Console.WriteLine($"A >  B ? {A > B}");
            Console.WriteLine($"A >= B ? {A >= B}");
            Console.WriteLine($"A <  B ? {A < B}");
            Console.WriteLine($"A <= B ? {A <= B}");

            Console.WriteLine();
            Console.WriteLine("Натисніть будь-яку клавішу...");
            Console.ReadKey();
        }
    }
}
