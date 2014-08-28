using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AuraFrontend
{
	class PortTester
	{
		private readonly IDictionary<int, Tuple<string, bool>> _ports;

		public PortTester(IDictionary<int, Tuple<string, bool>> ports)
		{
			_ports = ports;
		}

		public bool Test()
		{
			using (var t = new ChangingOutput("Checking Ports . . ."))
			{
				t.FinishLine();

				var pass = true;

				foreach (var kvp in _ports)
				{
					var result = TestPort(kvp.Key, kvp.Value.Item1);
					if (result != null)
					{
						Console.WriteLine(result);
						Console.WriteLine("{0} services will not be avalible.", kvp.Value.Item1);
						Console.WriteLine();
						if (kvp.Value.Item2)
							pass = false;
					}
				}

				t.PrintResult(pass);
				return pass;
			}
		}

		private static string TestPort(int number, string name)
		{
			using (var t = new ChangingOutput("Testing port {0} ({1}) . . .", number, name))
			{
				using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
				{
					socket.ExclusiveAddressUse = true;
					try
					{
						socket.Bind(new IPEndPoint(IPAddress.Any, number));
					}
					catch (SocketException ex)
					{
						t.PrintResult(false);
						return ex.Message;
					}
				}

				t.PrintResult(true);
				return null;
			}
		}
	}
}
