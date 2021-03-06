using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Odachi.CodeModel.Mapping;

namespace Odachi.CodeModel
{
	/// <summary>
	/// Represents a service.
	/// </summary>
	public class ServiceFragment : TypeFragment
	{
		public override string Kind => "service";

		public IList<MethodFragment> Methods { get; } = new List<MethodFragment>();
	}
}
