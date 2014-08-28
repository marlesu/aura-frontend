using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace AuraFrontend
{
	class AuraCompiler
	{
		private readonly string _slnPath;
		private readonly bool _release;

		public AuraCompiler(string slnPath, bool release)
		{
			_slnPath = slnPath;
			_release = release;
		}

		public bool Build()
		{
			return Compile("Build");
		}

		public bool Rebuild()
		{
			return Compile("Rebuild");
		}

		bool Compile(params string[] targets)
		{
			using (var t = new ChangingOutput("Compiling target(s) {0} . . .", string.Join(", ", targets)))
			{
				var logger = new ConsoleLogger(LoggerVerbosity.Quiet);

				logger.SkipProjectStartedText = true;

				var props = new Dictionary<string, string>
				{
					{"Configuration", _release ? "Release" : "Debug"},
				};

				var request = new BuildRequestData(_slnPath, props, null, targets, null);
				var p = new BuildParameters()
				{
					Loggers = new[] {logger},
					GlobalProperties = props
				};

				var result = BuildManager.DefaultBuildManager.Build(p, request);

				t.PrintResult(result.OverallResult == BuildResultCode.Success);

				return result.OverallResult == BuildResultCode.Success;
			}
		}
	}
}
