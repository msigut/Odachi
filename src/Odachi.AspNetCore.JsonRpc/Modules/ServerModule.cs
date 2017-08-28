﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Odachi.AspNetCore.JsonRpc.Internal;
using Odachi.Annotations;

namespace Odachi.AspNetCore.JsonRpc.Modules
{
	public class ServerModule
	{
		[RpcMethod]
		public static string[] ListMethods(JsonRpcServer server)
		{
			var methods = server.Methods
				.Select(m => m.Name)
				.ToArray();

			return methods;
		}

		[RpcMethod]
		public static DateTime Ping()
		{
			return DateTime.Now;
		}
	}
}
