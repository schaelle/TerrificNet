﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class AttributeDataBinder : IDataScopeContract
	{
		private readonly Dictionary<string, IDataScopeContract> _contracts;

		public AttributeDataBinder(Dictionary<string, IDataScopeContract> contracts)
		{
			_contracts = contracts;
		}

		public BindingPathTemplate Path
		{
			get { throw new NotSupportedException(); }
		}

		public Expression Expression
		{
			get { throw new NotSupportedException(); }
		}

		public IDataScopeContract Property(string propertyName, SyntaxNode node)
		{
			return _contracts[propertyName];
		}

		public IBinding<string> RequiresString()
		{
			throw new NotSupportedException();
		}

		public IBinding<bool> RequiresBoolean()
		{
			throw new NotSupportedException();
		}

		public IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			throw new NotSupportedException();
		}

		public IDataScopeContract Parent
		{
			get
			{
				throw new NotSupportedException();
			}
		}
	}
}