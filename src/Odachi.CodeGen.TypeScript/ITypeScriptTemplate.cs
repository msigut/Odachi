﻿namespace Odachi.CodeModelGen.TypeScript
{
	public interface ITypeScriptTemplate
	{
		void Write(TypeScriptWriter writer, TypeScriptModule module);
	}
}
