using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraFrontend
{
	class AuraServers : ServerBase
	{
		private readonly string _startServersPath;
		private readonly string _auraDir;

		public AuraServers(string auraDir, string startServersPath)
		{
			_auraDir = auraDir;
			_startServersPath = startServersPath;
		}

		public override bool Start()
		{
			using (var t = new ChangingOutput("Starting Aura servers . . ."))
			{
				using (var p = new Process
				{
					StartInfo =
					{
						FileName = _startServersPath,
						CreateNoWindow = true,
						UseShellExecute = true,
						WorkingDirectory = _auraDir
					}
				})
				{
					p.Start();
					p.WaitForExit();

					t.PrintResult(p.ExitCode == 0);
					return p.ExitCode == 0;
				}
			}
		}

		public override void Stop()
		{

		}
	}
}
