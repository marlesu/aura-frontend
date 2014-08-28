using System;
using System.Diagnostics;

namespace AuraFrontend
{
	internal abstract class ServerBase : IDisposable
	{
		protected Process ServerProcess;
		public abstract bool Start();
		public abstract void Stop();

		public void Dispose()
		{
			Stop();
			GC.SuppressFinalize(this);
		}

		~ServerBase()
		{
			Dispose();
		}
	}
}