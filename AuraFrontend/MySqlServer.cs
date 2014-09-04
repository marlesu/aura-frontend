using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace AuraFrontend
{
	class MySqlServer : ServerBase
	{
		private readonly string _mySqlDPath;
		private readonly string _mySqlDir;
		private readonly string _mySqlPath;
		private readonly string _mySqlArgs;

		public MySqlServer(string mySqlDPath, string mySqlDir, string mySqlArgs, string mySqlPath)
		{
			_mySqlDPath = mySqlDPath;
			_mySqlDir = mySqlDir;
			_mySqlArgs = mySqlArgs;
			_mySqlPath = mySqlPath;
		}

		Process StartMySql()
		{
			var p = new Process
			{
				StartInfo =
				{
					FileName = _mySqlDPath,
					CreateNoWindow = true,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					WorkingDirectory = _mySqlDir
				}
			};

			using (var t = new ChangingOutput("Starting MySql server . . ."))
			{
				var success = p.Start();

				t.PrintResult(success);

				if (!success)
					p = null;
			}

			using (var _ = new ChangingOutput("Waiting for MySql to accept connections . . ."))
			{
				var i = 0;
				var timer = new Timer(1000);

				timer.Elapsed += (e, o) =>
				{
					_.PrintNumber(i++);
				};

				timer.Start();

				while (!CheckMysqlPort())
					Thread.Sleep(1000);

				timer.Stop();

				_.PrintResult(true);
			}

			return p;
		}

		private bool CheckMysqlPort()
		{
			try
			{
				using (var tcp = new TcpClient())
				{
					tcp.Connect("localhost", 3306);

					return true;
				}
			}
			catch (SocketException)
			{
				return false;
			}
		}

		public override bool Start()
		{
			ServerProcess = StartMySql();

			return ServerProcess != null;
		}

		public override void Stop()
		{
			if (ServerProcess != null)
			{
				ServerProcess.Kill();
				ServerProcess = null;
			}
		}

		public bool RunMainSql(string mainSqlPath)
		{
			using (var t = new ChangingOutput("Applying main.sql"))
			{
				using (var p = new Process())
				{
					p.StartInfo.FileName = _mySqlPath;
					p.StartInfo.Arguments = _mySqlArgs;
					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.RedirectStandardError = p.StartInfo.RedirectStandardInput = p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.WorkingDirectory = _mySqlDir;
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

					t.PrintNumber(0);

					var i = 0;

					var ti = new Timer(1000);
					ti.Elapsed += (e, o) => { t.PrintNumber(++i); };

					ti.Start();

					p.Start();

					p.StandardInput.Write(File.ReadAllText(mainSqlPath));
					p.StandardInput.WriteLine();
					p.StandardInput.WriteLine("exit");
					p.StandardInput.Flush();

					p.WaitForExit();

					ti.Stop();

					var success = true;

					if (!p.StandardError.EndOfStream)
					{
						t.FinishLine();
						Console.WriteLine("MySql reports errors: {0}", p.StandardError.ReadToEnd());
						success = false;
					}

					t.PrintResult(success);

					return success;
				}
			}
		}
	}
}
