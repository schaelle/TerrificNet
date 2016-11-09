﻿using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IRenderingScope
	{
		IEnumerable<IBinding> GetBindings();

		IRenderingScope Parent { get; }

		IBinding Id { get; }

		IReadOnlyList<IRenderingScope> Children { get; }

		bool IsEmpty();

		void Process(ScopeParameters scopeParameters);
	}
}