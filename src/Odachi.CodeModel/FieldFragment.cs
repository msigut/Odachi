﻿using Odachi.CodeModel.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Odachi.CodeModel
{
	/// <summary>
	/// Represents a field.
	/// </summary>
	public class FieldFragment : Fragment
	{
		public TypeReference Type { get; set; }
	}
}
