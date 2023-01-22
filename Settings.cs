using System;
using System.Collections.Generic;
using System.Text;

namespace BTCPhillips {
	public class Settings {
		public static readonly int priceDecreasedHue = 100; // 100 is red
		public static readonly int priceIncreasedHue = 21000; // 21000 is green
		public static readonly int priceSidewaysHue = 3000; // 3000 is orange

		public static readonly List<string> lightIDs = new List<string> { "12", "13" };
	}
}
