using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Odachi.Abstractions;
using Odachi.AspNetCore.JsonRpc.Model;
using Odachi.Extensions.Reflection;
using Odachi.JsonRpc.Common.Converters;

#pragma warning disable CS0618

namespace Odachi.AspNetCore.JsonRpc.Internal
{
	public class ReflectedJsonRpcMethod : JsonRpcMethod
	{
		public ReflectedJsonRpcMethod(string moduleName, string methodName, Type type, MethodInfo method)
			: base(moduleName, methodName)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			Type = type;
			Method = method;
		}

		public Type Type { get; }
		public MethodInfo Method { get; }

		private IReadOnlyList<JsonRpcParameter> _parameters;
		public override IReadOnlyList<JsonRpcParameter> Parameters => _parameters == null ? throw new InvalidOperationException("Method wasn't analyzed") : _parameters;

		private Type _returnType;
		public override Type ReturnType =>
			// `_returnType` may be null (= `void` return type), so check on `_parameters` instead
			_parameters == null ? throw new InvalidOperationException("Method wasn't analyzed") : _returnType;

		public override void Analyze(JsonRpcServer server, Type[] internalTypes)
		{
			_parameters = Method.GetParameters()
				.Select(p => new JsonRpcParameter(
					p.Name,
					p.ParameterType,
					internalTypes.Contains(p.ParameterType),
					p.IsOptional,
					p.HasDefaultValue ? p.DefaultValue : (p.ParameterType.GetTypeInfo().IsValueType ? Activator.CreateInstance(p.ParameterType) : null)
				))
				.ToArray();

			if (Method.ReturnType.IsAwaitable())
			{
				_returnType = Method.ReturnType.GetAwaitedType();
			}
			else
			{
				_returnType = Method.ReturnType;
			}
		}

		public override async Task HandleAsync(JsonRpcContext context)
		{
			var request = context.Request;

			object service = null;
			if (!Method.IsStatic)
			{
				service = context.AppServices.GetRequiredService(Type);
			}

			var internalParams = 0;
			var parameters = new object[Parameters.Count];
			if (parameters.Length > 0)
			{
				// todo: this should be extracted somewhere else..
				var httpContext = context.AppServices.GetRequiredService<IHttpContextAccessor>().HttpContext;

				IBlob HandleBlob(string path, string name)
				{
					var form = httpContext.Request?.Form;
					if (form == null)
						return null;

					var file = form.Files[name];
					if (file == null)
						return null;

					return new FormFileBlob(file);
				}

				IStreamReference HandleReference(string path, string name)
				{
					var form = httpContext.Request?.Form;
					if (form == null)
						return null;

					var file = form.Files[name];
					if (file == null)
						return null;

					return new FormFileStreamReference(file);
				}

				using (new BlobReadHandler(HandleBlob))
				using (new StreamReferenceReadHandler(HandleReference))
				{
					for (var i = 0; i < Parameters.Count; i++)
					{
						var parameter = Parameters[i];
						var parameterType = parameter.Type;

						object value = null;

						if (parameter.IsInternal)
						{
							value = context.RpcServices.GetService(parameterType);

							internalParams++;
						}
						else
						{
							if (request.IsIndexed)
								value = request.GetParameter(i - internalParams, parameterType, Type.Missing);
							else
								value = request.GetParameter(parameter.Name, parameter.Type, Type.Missing);
						}

						// if parameter couldn't be resolved, use default value (the called method is responsible for validating parameters)
						if (value == Type.Missing && !parameter.IsOptional)
						{
							value = parameter.DefaultValue;
						}

						parameters[i] = value;
					}
				}
			}

			// invoke the method
			var result = await Method.InvokeAsync(service, parameters);

			// return the result
			context.SetResponse(result);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode() ^ Method.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var other = obj as ReflectedJsonRpcMethod;
			if (other == null)
				return false;

			return Name == other.Name && Method == other.Method;
		}
	}
}
