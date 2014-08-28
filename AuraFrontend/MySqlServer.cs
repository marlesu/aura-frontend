using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

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
			using (var t = new ChangingOutput("Starting MySql server . . ."))
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

				var success = p.Start();

				t.PrintResult(success);

				if (!success)
					p = null;

				return p;
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
