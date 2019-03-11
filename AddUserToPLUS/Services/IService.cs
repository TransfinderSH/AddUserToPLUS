using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddUserToPLUS.Services
{
	public interface IService
	{
		void Start();
		void Stop();
		void Restart();
		bool AutoStart { get; }
	}
}
