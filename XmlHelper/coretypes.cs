// YOU SHOULD NOT MODIFY THIS FILE

using System.Text;

namespace Helper
{
	public class HexBinary
    {
        private static string EncodingTable = "0123456789ABCDEF";
        private static sbyte[] aDecodingTable = new sbyte[256]
		{
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			0,	 1,	 2,	 3,	 4,	 5,	 6,	 7,	 8,	 9,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	10,	11,	12,	13,	14,	15,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	10,	11,	12,	13,	14,	15,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1
		};

        static public string decode(byte[] v)
        {
            string result = "";
            for (int i = 0; i < v.Length; ++i)
            {
                result += EncodingTable[v[i] >> 4];
                result += EncodingTable[v[i] & 15];
            }
            return result;
        }

        static public byte[] encode(string s)
        {
            if (s == null) return null;
            StringBuilder sb = new StringBuilder(s);
            sb.Replace(" ", "");
            sb.Replace("\t", "");
            sb.Replace("\n", "");
            sb.Replace("\r", "");
            string newvalue = sb.ToString().Trim();
            if (newvalue.Length == 0) return new byte[0];
            char[] cSrc = newvalue.ToCharArray();
            byte[] value = new byte[cSrc.Length / 2];
            int nSrcIndex = 0;
            int nTarIndex = 0;
            while (nSrcIndex < cSrc.Length)
            {
                sbyte c = aDecodingTable[cSrc[nSrcIndex++]];
                if (c != -1)
                {
                    value[nTarIndex >> 1] |= (byte)((nTarIndex & 1) == 1 ? c : (c << 4));
                    nTarIndex++;
                }
            }
            return value;
        }
    }

    public class CoreTypes
    {
		/// <summary>
		/// Helper class for converting numbers valid according to XML Schema Part 2 : Datatypes to 
		/// numbers valid for C# parse functions.
		/// </summary>
		public class NumberParts
		{
			public readonly char Sign;
			public readonly string Mantissa;
			public readonly string Exponent;
			public readonly bool Valid;

			public NumberParts(string input)
			{
				input = input.Trim();
				if (input.StartsWith("+") || input.StartsWith("-"))
				{
					Sign = input[0];
					input = input.Substring(1);
				}
				else
					Sign = '+';

				int indexOfE = input.IndexOfAny(new char[]{'e', 'E'});
				if (indexOfE < 0)
				{
					Mantissa = input;
					Exponent = "";
				}
				else
				{
					Mantissa = input.Substring(0, indexOfE);
					Exponent = input.Substring(indexOfE + 1);
				}

				if (Mantissa.StartsWith("."))
					Mantissa = "0" + Mantissa;
				if (Mantissa.EndsWith("."))
					Mantissa = Mantissa + "0";
				if (Mantissa.Length == 0)
					Mantissa = "0";
				if (Exponent.EndsWith("+") || Exponent.EndsWith("-"))
					Exponent = Exponent + "0";
			}

			public override string ToString()
			{
				if (Exponent.Length > 0)
					return Sign + Mantissa + "E" + Exponent;
				return Sign + Mantissa;
			}

			public double ToDouble()
			{
				return double.Parse(ToString(), System.Globalization.CultureInfo.InvariantCulture);
			}

			public decimal ToDecimal()
			{
				return decimal.Parse(Sign + Mantissa, System.Globalization.CultureInfo.InvariantCulture);
			}

			public long ToLong()
			{
				int dot = Mantissa.IndexOf('.');
				if (dot >= 0)
					return long.Parse(Sign + Mantissa.Substring(0, dot));
				else
					return long.Parse(Sign + Mantissa);
			}
		
			public ulong ToULong()
			{
				int dot = Mantissa.IndexOf('.');
				if (dot >= 0)
					return ulong.Parse(Sign + Mantissa.Substring(0, dot));
				else
					return ulong.Parse(Sign + Mantissa);
			}
		
			public bool IsNumber()
			{
				try {
					double.Parse(ToString());
					return true;
				}
				catch (System.Exception)
				{
					return false;
				}
			}
		}

		static bool IsSchemaSign(char c)
		{
			return c == '+' || c == '-';
		}

		static bool IsSchemaDigit(char c)
		{
			return c >= '0' && c <= '9';
		}

		static bool IsSchemaRadix(char c)
		{
			return c == '.';
		}

		static bool IsSchemaExponent(char c)
		{
			return c == 'e' || c == 'E';
		}

		static int PrepareNumber(System.Text.StringBuilder buffer, string input)
		{
			int[,] actions = new int[9,5]{
				{ 0, 0, 1, 2, 2 },	
				{ 2, 0, 1, 2, 2 },	
				{ 2, 0, 0, 0, 2 },	
				{ 2, 0, 2, 2, 2 },	
				{ 2, 0, 2, 0, 2 },	
				{ 2, 0, 2, 1, 2 }, 
				{ 0, 0, 2, 2, 2 },
				{ 2, 0, 2, 2, 2 },
				{ 2, 0, 2, 2, 2 },
			};

			int[,] follow = new int[9,5]{ 
				{ 1, 2, 3, 9, 9 }, 
				{ 9, 2, 3, 9, 9 },
				{ 9, 2, 5, 6, 9 },
				{ 9, 4, 9, 9, 9 },
				{ 9, 4, 9, 6, 9 },
				{ 9, 4, 9, 6, 9 },
				{ 7, 8, 9, 9, 9 },
				{ 9, 8, 9, 9, 9 },
				{ 9, 8, 9, 9, 9 },
			};

			int state = 0;

			foreach(char c in input)
			{
				int cls;

				if (IsSchemaDigit(c))
					cls = 1;
				else if (IsSchemaSign(c))
					cls = 0;
				else if (IsSchemaRadix(c))
					cls = 2;
				else if (IsSchemaExponent(c))
					cls = 3;
				else
					cls = 4;

				switch (actions[state,cls])
				{
				case 0:
					buffer.Append(c);
					break;
				case 1:
					buffer.Append('0');
					buffer.Append(c);
					break;
				case 2:
					return 0;
				}
				state = follow[state, cls];
			}

			switch (state)
			{
			case 0:
			case 1:
			case 3:
			case 6:
			case 7:
				return 0;		

			case 2:
				return 1;	// integer
			case 4:
				return 2;
			case 5:
				buffer.Append('0');
				return 2;
			case 8:
				return 3;	// float
			}

			return 0;
		}

		public static System.Collections.IEnumerable castToEnumerable(object o)
		{
            return (System.Collections.IEnumerable)o;
		}

        public static int CastToInt(int i)
        {
            return i;
        }

        public static int CastToInt(uint i)
		{
            if (i > (uint)int.MaxValue)
                throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for int.", i));
            return (int)i;
		}

        public static int CastToInt(double d)
        {
            if (d < int.MinValue || d > int.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for int.", d));
			return (int) d;
        }

        public static int CastToInt(string s)
        {
			if (s == null)
				return 0;
			s = s.Trim();
			if (s == "NaN" || s == "INF" || s == "-INF")
				throw new System.OverflowException("'" + s + "' is out of range for int.");
			
			System.Text.StringBuilder bld = new System.Text.StringBuilder();
			checked 
			{ 
				switch (PrepareNumber(bld, s))
				{
					case 1:
						return int.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

					case 2:
						return (int)decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
					case 3:
						return (int)double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

					default:
						throw new System.ArgumentException("'" + s + "' cannot be parsed as int.");
				}
			}
        }

        public static int CastToInt(long i)
        {
            if (i < int.MinValue || i > int.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for int.", i));
			return (int) i;
        }

        public static int CastToInt(ulong i)
        {
            if (i > (ulong)int.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for int.", i));
			return (int)i;
        }

		public static int CastToInt(decimal d)
		{
			return (int)d;
		}

        public static uint CastToUInt(int i)
        {
            if (i < 0)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for uint.", i));
			return (uint) i;

        }

        public static uint CastToUInt(uint i)
        {
            return i;
        }

        public static uint CastToUInt(double d)
        {
            if (d < 0 || d > uint.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for uint.", d));
			return (uint) d;
        }

        public static uint CastToUInt(string s)
        {
			if (s == null)
				return 0;
			s = s.Trim();
			if (s == "NaN" || s == "INF" || s == "-INF")
				throw new System.OverflowException("'" + s + "' is out of range for int.");
			
			System.Text.StringBuilder bld = new System.Text.StringBuilder();
			checked 
			{ 
				switch (PrepareNumber(bld, s))
				{
					case 1:
						return uint.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

					case 2:
						return (uint)decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
					case 3:
						return (uint)double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

					default:
						throw new System.ArgumentException("'" + s + "' cannot be parsed as int.");
				}
			}
        }

        public static uint CastToUInt(long i)
        {
            if (i < 0 || i > uint.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for uint.", i));
			return (uint) i;
        }

        public static uint CastToUInt(ulong i)
        {
            if (i > uint.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for uint.", i));
			return (uint) i;
        }

		public static uint CastToUInt(decimal d)
		{
			return (uint)d;
		}


        public static long CastToInt64(int i)
        {
            return (long) i;
        }

        public static long CastToInt64(uint i)
        {
            return (long)i;
        }

        public static long CastToInt64(double d)
        {
            if (d < long.MinValue || d > long.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for long.", d));
			return (long) d;
        }

        public static long CastToInt64(string s)
        {
			if (s == null)
				return 0;
			s = s.Trim();
			if (s == "NaN" || s == "INF" || s == "-INF")
				throw new System.OverflowException("'" + s + "' is out of range for int.");
			
			System.Text.StringBuilder bld = new System.Text.StringBuilder();
			checked 
			{ 
				switch (PrepareNumber(bld, s))
				{
					case 1:
						return long.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

					case 2:
						return (long)decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
					case 3:
						return (long)double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

					default:
						throw new System.ArgumentException("'" + s + "' cannot be parsed as int.");
				}
			}
        }

        public static long CastToInt64(long i)
        {
            return i;
        }

        public static long CastToInt64(ulong i)
        {
            if (i > long.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for long.", i));
			return (long) i;
        }

		public static long CastToInt64(decimal d)
		{
			return (long)d;
		}


        public static ulong CastToUInt64(int i)
        {
            if (i < 0)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for ulong.", i));
			return (ulong) i;
        }

        public static ulong CastToUInt64(uint i)
        {
            return (ulong)i;
        }

        public static ulong CastToUInt64(double d)
        {
            if (d < 0 || d > ulong.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for ulong.", d));
			return (ulong) d;
        }

        public static ulong CastToUInt64(string s)
        {
			if (s == null)
				return 0;
			s = s.Trim();
			if (s == "NaN" || s == "INF" || s == "-INF")
				throw new System.OverflowException("'" + s + "' is out of range for int.");
			
			System.Text.StringBuilder bld = new System.Text.StringBuilder();
			checked 
			{ 
				switch (PrepareNumber(bld, s))
				{
					case 1:
						return ulong.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

					case 2:
						return (ulong)decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
					case 3:
						return (ulong)double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

					default:
						throw new System.ArgumentException("'" + s + "' cannot be parsed as int.");
				}
			}
        }

        public static ulong CastToUInt64(long i)
        {
            if (i < 0)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for ulong.", i));
			return (ulong) i;
        }

        public static ulong CastToUInt64(ulong i)
        {
            return i;
        }

		public static ulong CastToUInt64(decimal d)
		{
			return (ulong)d;
		}

        public static double CastToDouble(bool b)
        {
            return b?1:0;
        }

        public static double CastToDouble(int i)
        {
            return (double) i;
        }

        public static double CastToDouble(uint i)
        {
            return (double) i;
        }

        public static double CastToDouble(long i)
        {
            return (double) i;
        }

        public static double CastToDouble(ulong i)
        {
            return (double) i;
        }

        public static double CastToDouble(double d)
        {
            return d;
        }

        public static double CastToDouble(string s)
        {
			if (s == null)
				return 0;
			s = s.Trim();
			if (s == "NaN")
				return double.NaN;
			else if (s == "INF")
				return double.PositiveInfinity;
			else if (s == "-INF")
				return double.NegativeInfinity;
			
			System.Text.StringBuilder bld = new System.Text.StringBuilder();
			checked 
			{ 
				switch (PrepareNumber(bld, s))
				{
					case 1:
					case 2:
					case 3:
						return double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

					default:
						throw new System.ArgumentException("'" + s + "' cannot be parsed as int.");
				}
			}
        }

		public static double CastToDouble(decimal d)
		{
			return (double)d;
		}
        
		public static decimal CastToDecimal(bool b)
		{
			return b ? 1 : 0;
		}

		public static decimal CastToDecimal(int i)
		{
			return i;
		}

		public static decimal CastToDecimal(uint i)
		{
			return i;
		}

		public static decimal CastToDecimal(long i)
		{
			return i;
		}

		public static decimal CastToDecimal(ulong i)
		{
			return i;
		}

		public static decimal CastToDecimal(double i)
		{
			return (decimal)i;
		}

		public static decimal CastToDecimal(decimal i)
		{
			return i;
		}

		public static decimal CastToDecimal(string s)
		{
			if (s == null)
				return 0;
			s = s.Trim();
			if (s == "NaN" || s == "INF" || s == "-INF")
				throw new System.OverflowException("'" + s + "' is out of range for int.");
			
			System.Text.StringBuilder bld = new System.Text.StringBuilder();
			checked 
			{ 
				switch (PrepareNumber(bld, s))
				{
					case 1:
					case 2:
						return decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
					case 3:
						return (decimal)double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

					default:
						throw new System.ArgumentException("'" + s + "' cannot be parsed as int.");
				}
			}
		}

        public static string CastToString(int i)
        {
            return i.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string CastToString(uint i)
        {
            return i.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string CastToString(long i)
        {
            return i.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string CastToString(ulong i)
        {
            return i.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string CastToString(double d)
        {
            return d.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string CastToString(string s)
        {
            return s;
        }

        public static string CastToString(bool b)
        {
            if (b) 
                return "true";
            return "false";
        }

        public static string CastToString(Helper.Types.DateTime dt)
        {
            return dt.ToString();
        }

		public static string CastToString(Helper.Types.DateTime dt, Helper.Types.DateTimeFormat format)
		{
			return dt.ToString(format);
		}
		
		public static string CastToString(object o_which_is_a_dt, Helper.Types.DateTimeFormat format)
		{
			return ((Helper.Types.DateTime)o_which_is_a_dt).ToString(format);
		}
		

        public static string CastToString(Helper.Types.Duration dur)
        {
            return dur.ToString();
        }

		public static string CastToString(byte[] val)
		{
			return System.Convert.ToBase64String(val);
		}

		public static string CastToString(decimal d)
		{
			string s = d.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
			int iComma = s.LastIndexOf('.');
			if( iComma >= 0 )
			{
				if (s.Length > iComma)
					s = s.TrimEnd(new char[] { '0' });
				if (s.Length == iComma + 1)
					s = s.Substring(0, iComma);
			}
			if( s.Length == 0 )
				s = "0";
			return s;
		}
		
		public static Helper.Types.QName CastToQName(object o)
        {
            if (o is string)
                return CastToQName((string) o);

            if (o is Helper.Types.QName)
                return CastToQName((Helper.Types.QName) o);
            return null;
        }
		
		public static string CastToString(Helper.Types.QName qn)
		{
			return qn.ToString();
		}


		public static Helper.Types.QName CastToQName(string s)
        {
            return new Helper.Types.QName(s);
        }

        public static Helper.Types.QName CastToQName(Helper.Types.QName qn)
        {
            return qn;
        }

        /*
        public static string CastToString(Helper.Types.DayTimeDuration dt)
        {
            return dt.ToString();
        }

        public static string CastToString(Helper.Types.YearMonthDuration dur)
        {
            return dur.ToString();
        }
        */

        public static Helper.Types.DateTime CastToDateTime(string s)
        {
			if (s == null)
                throw new System.NullReferenceException();
			if (s == "")
                throw new Helper.Types.StringParseException("Cast to DateTime failed.");
            return Helper.Types.DateTime.Parse(s);
        }

        public static Helper.Types.Duration CastToDuration(string s)
        {
			if (s == null)
                throw new System.NullReferenceException();
			if (s == "")
                throw new Helper.Types.StringParseException("Cast to Duration failed.");
            return Helper.Types.Duration.Parse(s);
        }

        /*
        public static Helper.Types.YearMonthDuration CastToYearMonthDuration(string s)
        {
            return Helper.Types.YearMonthDuration.Parse(s);
        }

        public static Helper.Types.DayTimeDuration CastToDayTimeDuration(string s)
        {
            return Helper.Types.DayTimeDuration.Parse(s);
        }
        */

        public static Helper.Types.DateTime CastToDateTime(Helper.Types.DateTime s)
        {
            return s;
        }

		public static Helper.Types.DateTime CastToDateTime(Helper.Types.DateTime s, Helper.Types.DateTimeFormat format)
		{
			return s;
		}

		public static Helper.Types.Duration CastToDuration(Helper.Types.Duration s)
        {
            return s;
        }

        /*
        public static Helper.Types.YearMonthDuration CastToYearMonthDuration(Helper.Types.YearMonthDuration s)
        {
            return s;
        }

        public static Helper.Types.DayTimeDuration CastToDayTimeDuration(Helper.Types.DayTimeDuration s)
        {
            return s;
        }
        */

        public static bool CastToBool(bool b)
        {
            return b;
        }

        public static bool CastToBool(int i)
        {
            return i != 0;
        }

        public static bool CastToBool(uint i)
        {
            return i != 0;
        }

        public static bool CastToBool(long i)
        {
            return i != 0;
        }

        public static bool CastToBool(ulong i)
        {
            return i != 0;
        }

        public static bool CastToBool(double d)
        {
            return d != 0;
        }

        public static bool CastToBool(string v)
        {
			string s = v.Trim();
			if (s == "true" || s == "1") return true;
			if (s == "false" || s == "0") return false;
			throw new System.ArgumentException("'" + v + "' cannot be parsed as boolean.");
        }

		public static bool CastToBool(decimal d)
		{
			return d != 0;
		}
        
        public static bool Exists(object o)
        {
            return o != null;
        }
   		
        public static int CastToInt(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();

			if (v is string)
				return CastToInt((string)v);

			return System.Convert.ToInt32(v, System.Globalization.CultureInfo.InvariantCulture);
		}             

		public static uint CastToUInt(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();

			if (v is string)
				return CastToUInt((string)v);
			return System.Convert.ToUInt32(v, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static string CastToString(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is Helper.Types.DateTime)
				return CastToString(CastToDateTime(v));
			if (v is System.Boolean)
				return CastToString(CastToBool(v));	// to ensure the returned string is not capitalized
			if (v is System.Decimal)
				return CastToString(CastToDecimal(v));	// to ensure the returned string has our desired format

			return System.Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static double CastToDouble(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is string)
				return CastToDouble((string)v);
			return System.Convert.ToDouble(v, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static decimal CastToDecimal(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is string)
				return CastToDecimal((string)v);
			return System.Convert.ToDecimal(v, System.Globalization.CultureInfo.InvariantCulture);
		}


		public static long CastToInt64(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is string)
				return CastToInt64((string)v);
			return System.Convert.ToInt64(v, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static ulong CastToUInt64(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is string)
				return CastToUInt64((string)v);
			return System.Convert.ToUInt64(v, System.Globalization.CultureInfo.InvariantCulture);
		}
		
		// see #26364 - somebody actually maps dates to bools
		public static bool CastToBool(Helper.Types.DateTime v, Helper.Types.DateTimeFormat format)
        {
            return (v != null);
        }
		
		public static bool CastToBool(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is string)
				return CastToBool((string)v);
			return System.Convert.ToBoolean(v, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static Helper.Types.DateTime CastToDateTime(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is string)
				return CastToDateTime((string)v);            
			return (Helper.Types.DateTime)v;
		}

		public static Helper.Types.DateTime CastToDateTime(object v, Helper.Types.DateTimeFormat format)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is string)
				return CastToDateTime((string)v, format);            
			return (Helper.Types.DateTime)v;
		}

		public static Helper.Types.Duration CastToDuration(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is string)
				return CastToDuration((string)v);
			return (Helper.Types.Duration)v;
		}

		public static byte[] CastToBinary(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
            if (v is string)
                CastToBinary((string)v);
			return (byte[]) v;
		}
        
        public static byte[] CastToBinary(string s)
        {
            System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding();
            return encoding.GetBytes(s);
        }
     
        public static byte[] CastToBinary(byte[] b) { return b; }
        
        public static string FormatNumber(uint value, uint minDigits)
        {
            return value.ToString("D" + minDigits.ToString(), System.Globalization.NumberFormatInfo.InvariantInfo);
        }
        
        public static string FormatNumber(int value, uint minDigits)
        {
            return value.ToString("D" + minDigits.ToString(), System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string FormatTimezone(int value)
        {
            string result = "";
            
            if (value == 0)
                result += 'Z';
            else
            {
                if (value < 0)
                {
                    result += '-';
                    value = -value;
                }
                else
                {
                    result += '+';
                }
                result += FormatNumber((uint) value / 60, 2);
                result += ':';
                result += FormatNumber((uint) value % 60, 2);
            }
            return result;
        }

        public static string FormatFraction(uint value, uint precision)
        {
            string result = "";
	        if (value != 0)
	        {
		        result += '.';
		        result += FormatNumber(value, precision);
                int i = result.Length;
		        while (result[i - 1] == '0')
			        i -= 1;
                result = result.Remove(i, result.Length-i);
	        }
            return result;
        }

		public static string DecimalToString(decimal v)
		{
			return CastToString(v);
		}

		public static decimal ParseDecimal(string v)
		{
			return CastToDecimal(v);
		}

		public static string DoubleToString(double v)
		{
			return CastToString(v);
		}

		public static double ParseDouble(string v)
		{
			return CastToDouble(v);
		}

		public static Helper.Types.DateTime CastToDateTime(string v, Helper.Types.DateTimeFormat format)
		{
			return Helper.Types.DateTime.Parse(v, format);
		}

		public static string DateTimeToString(Helper.Types.DateTime v)
		{
			return CastToString(v, Helper.Types.DateTimeFormat.W3_dateTime);
		}

		public static Helper.Types.DateTime ParseDateTime(string v)
		{
			return CastToDateTime(v, Helper.Types.DateTimeFormat.W3_dateTime);
		}

		public static string DateToString(Helper.Types.DateTime v)
		{
			return CastToString(v, Helper.Types.DateTimeFormat.W3_date);
		}

		public static Helper.Types.DateTime ParseDate(string v)
		{
			return CastToDateTime(v, Helper.Types.DateTimeFormat.W3_date);
		}

		public static string TimeToString(Helper.Types.DateTime v)
		{
			return CastToString(v, Helper.Types.DateTimeFormat.W3_time);
		}

		public static Helper.Types.DateTime ParseTime(string v)
		{
			return CastToDateTime(v, Helper.Types.DateTimeFormat.W3_time);
		}

		public static string GYearToString(Helper.Types.DateTime v)
		{
			return CastToString(v, Helper.Types.DateTimeFormat.W3_gYear);
		}

		public static Helper.Types.DateTime ParseGYear(string v)
		{
			return CastToDateTime(v, Helper.Types.DateTimeFormat.W3_gYear);
		}

		public static decimal GYearToInteger(Helper.Types.DateTime v)
		{
			return v.Value.Year;
		}

		public static Helper.Types.DateTime IntegerToGYear(decimal v)
		{
			return new Helper.Types.DateTime((int)v,1,1);
		}

		public static string GMonthToString(Helper.Types.DateTime v)
		{
			return CastToString(v, Helper.Types.DateTimeFormat.W3_gMonth);
		}

		public static Helper.Types.DateTime ParseGMonth(string v)
		{
			return CastToDateTime(v, Helper.Types.DateTimeFormat.W3_gMonth);
		}

		public static string GDayToString(Helper.Types.DateTime v)
		{
			return CastToString(v, Helper.Types.DateTimeFormat.W3_gDay);
		}

		public static Helper.Types.DateTime ParseGDay(string v)
		{
			return CastToDateTime(v, Helper.Types.DateTimeFormat.W3_gDay);
		}

		public static string GYearMonthToString(Helper.Types.DateTime v)
		{
			return CastToString(v, Helper.Types.DateTimeFormat.W3_gYearMonth);
		}

		public static Helper.Types.DateTime ParseGYearMonth(string v)
		{
			return CastToDateTime(v, Helper.Types.DateTimeFormat.W3_gYearMonth);
		}

		public static string GMonthDayToString(Helper.Types.DateTime v)
		{
			return CastToString(v, Helper.Types.DateTimeFormat.W3_gMonthDay);
		}

		public static Helper.Types.DateTime ParseGMonthDay(string v)
		{
			return CastToDateTime(v, Helper.Types.DateTimeFormat.W3_gMonthDay);
		}

		public static string DurationToString(Helper.Types.Duration v)
		{
			return CastToString(v);
		}

		public static Helper.Types.Duration ParseDuration(string v)
		{
			return CastToDuration(v);
		}

		public static string BinaryToBase64String(byte[] v)
		{
            return System.Convert.ToBase64String(v, 0, v.Length);
		}

		public static byte[] ParseBase64Binary(string v)
		{
            return System.Convert.FromBase64String(v);
		}

		public static string BinaryToHexString(byte[] v)
		{
            return HexBinary.decode(v); 
		}

		public static byte[] ParseHexBinary(string v)
		{
            return HexBinary.encode(v);
		}

		public static string BooleanToString(bool v)
		{
			return v ? "true" : "false";
		}

		public static int BooleanToInt(bool v)
		{
			return v ? 1 : 0;
		}

		public static bool ParseBoolean(string v)
		{
			return CastToBool(v);
		}

		public static string QNameToString(Helper.Types.QName v)
		{
			return v.ToString();
		}
		
		public static Helper.Types.QName StringToQName(string s)
		{
			return CastToQName(s);
		}

		public static bool StringToBoolean(string v)
		{
			return CastToBool(v);
		}

		public static decimal LongToDecimal(long v)
		{
			return (decimal)v;
		}

		public static decimal ULongToDecimal(ulong v)
		{
			return (decimal)v;
		}

		public static long IntToLong(int v)
		{
			return (long)v;
		}

		public static decimal IntToDecimal(int v)
		{
			return (decimal)v;
		}

		public static ulong DecimalToULong(decimal v)
		{
			return (ulong)v;
		}

		public static int LongToInt(long v)
		{
			return (int)v;
		}

		public static long DecimalToLong(decimal v)
		{
			return (long)v;
		}

		public static int DecimalToInt(decimal v)
		{
			return (int)v;
		}
		
		public static decimal DecimalToInteger(decimal v)
		{
			return System.Decimal.Truncate(v);
		}

		public static long ParseLong(string v)
		{
			return CastToInt64(v);
		}

		public static ulong ParseULong(string v)
		{
			return CastToUInt64(v);
		}

		public static int ParseInt(string v)
		{
			return CastToInt(v);
		}

		public static decimal BooleanToDecimal(bool v)
		{
			return v ? 1.0m : 0.0m;
		}

		public static double BooleanToDouble(bool v)
		{
			return v ? 1.0 : 0.0;
		}

		public static bool DecimalToBoolean(decimal v)
		{
			return v != 0.0m;
		}

		public static bool DoubleToBoolean(double v)
		{
			return v != 0.0;
		}

		public static decimal DoubleToDecimal(double v)
		{
			return (decimal)v;
		}

		public static double DecimalToDouble(decimal v)
		{
			return (double)v;
		}


	}
}
