using System.Data.Linq;
using System.Text;

namespace Sitecore.SharedSource.Analytics.Context.Model
{
	public class TestVariationGrouping
	{
		private string _variationId;
		private double _engagementValue;
		private int _variationCount;
		private int _inActiveVisits;
		private int _activeVisits;
		private int _totalVisits;

		public TestVariationGrouping(string binary, double engagementValue, int variationCount)
		{
			_variationId = binary;
			_engagementValue = engagementValue;
			_variationCount = variationCount;
		}

		public string VariationId
		{
			get { return _variationId; }
		}

		public int VariationCount
		{
			get { return _variationCount; }
		}

		public double EngagementValueSum
		{
			get { return _engagementValue; }
		}

		public int TotalVisits
		{
			get { return _totalVisits; }
			set { _totalVisits = value; }
		}

		public int ActiveVisits
		{
			get { return _activeVisits; }
			set { _activeVisits = value; }
		}

		public int InActiveVisits
		{
			get { return _inActiveVisits; }
			set { _inActiveVisits = value; }
		}

		private string ConvertBinaryToItemId(Binary binary)
		{
			byte[] binaryString = binary.ToArray();

			// if the original encoding was ASCII
			string x = Encoding.ASCII.GetString(binaryString);

			// if the original encoding was UTF-8
			string y = Encoding.UTF8.GetString(binaryString);

			// if the original encoding was UTF-16
			string z = Encoding.Unicode.GetString(binaryString);

			string strBinary = binary.ToString();
			StringBuilder result = new StringBuilder(strBinary.Length / 8 + 1);

			// TODO: check all 1's or 0's... Will throw otherwise

			int mod4Len = binary.Length % 8;
			if (mod4Len != 0)
			{
				// pad to length multiple of 8
				strBinary = strBinary.PadLeft(((strBinary.Length / 8) + 1) * 8, '0');
			}

			for (int i = 0; i < binary.Length; i += 8)
			{
				string eightBits = strBinary.Substring(i, 8);
				result.AppendFormat("{0:X2}", System.Convert.ToByte(eightBits, 2));
			}

			return result.ToString();
		}
	}
}
