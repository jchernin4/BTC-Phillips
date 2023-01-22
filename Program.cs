using Q42.HueApi;
using Q42.HueApi.Converters;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Groups;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Drawing;

namespace BTCPhillips {
	public class Program {
		public static void Main(string[] args) {
			Console.WriteLine("Starting...");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			run();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			while (true) {
				Thread.Sleep(100000);
			}
		}

		public static async Task run() {

			IBridgeLocator locator = new HttpBridgeLocator();
			var bridges = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5));

			foreach (var bridge in bridges) {
				Console.WriteLine("Found bridge: " + bridge.BridgeId + " " + bridge.IpAddress);
			}


			ILocalHueClient client = new LocalHueClient("192.168.1.163");
			// Uncomment to generate key
			/*try {
				Console.WriteLine("Press the button on the hue now. Press enter to continue.");
				Console.ReadLine();
				Console.WriteLine("Continuing...");

				Console.WriteLine("Generating key");
				var appKey = await client.RegisterAsync("BTCPhillips", "BTCPhillipsDevice");
				Console.WriteLine(appKey);
			} catch (Exception e) {
				Console.WriteLine(e);
			}*/
			client.Initialize("CJnsuLHh-M1Fb8ngtEP80qBQnz5gkf3eTUG7vW-V");
			Console.WriteLine("Initialized");
			foreach (var l in await client.GetLightsAsync()) {
				Console.WriteLine("Light name: " + l.Name + " ID: " + l.Id);
			}

			Thread.Sleep(2000);
			Console.Clear();

			double prevPrice = 0.0;
			ChangeType change = ChangeType.Sideways;
			while (true) {
				bool updateLights = false;
				Console.Write("\r");
				double curPrice = getCurrentPrice();
				curPrice = Math.Round(curPrice, 2);

				LightCommand command = new LightCommand();
				command.On = true;
				command.Saturation = 254;
				command.Brightness = 254;
				command.TurnOn();

				Console.Write("Price: $" + curPrice);
				double percentDifference = Math.Round((Math.Abs(curPrice - prevPrice) / curPrice) * 100, 4);
				if (curPrice > prevPrice) {
					command.Hue = Settings.priceIncreasedHue;
					change = ChangeType.Increase;
					updateLights = true;

				} else if (curPrice < prevPrice) {
					command.Hue = Settings.priceDecreasedHue;
					change = ChangeType.Decrease;
					updateLights = true;

				} else {
					command.Hue = Settings.priceSidewaysHue;
					change = ChangeType.Sideways;
					updateLights = true;
				}

				Console.Write(" (" + change + " by " + percentDifference + "% ($" + Math.Round(curPrice - prevPrice, 2) + "))          ");

				prevPrice = curPrice;
				// Only send command if the color is actually changing (don't send green color if color is already green)
				if (updateLights) {
					await client.SendCommandAsync(command, Settings.lightIDs);
				}
				Thread.Sleep(2000);
			}
		}

		public enum ChangeType {
			Increase,
			Decrease,
			Sideways
		}

		public static double getCurrentPrice() {
			string url = "https://www.binance.com/api/v3/avgPrice?symbol=BTCUSDT";

			WebRequest wr = WebRequest.Create(url);
			wr.Method = "GET";

			WebResponse response = wr.GetResponse();
			System.IO.Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream);

			string respStr = reader.ReadToEnd();
			reader.Close();
			dataStream.Close();
			response.Close();

			int startIndex = respStr.IndexOf("\"price\":\"") + 9;
			string price = respStr.Substring(startIndex, respStr.Length - startIndex - 2);

			return double.Parse(price);
		}
	}
}
