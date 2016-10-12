﻿using System.Linq.Expressions;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class VDomBuilderExpression
	{
		private readonly Expression _instance;

		public VDomBuilderExpression(Expression instance)
		{
			_instance = instance;
		}

		public Expression ElementOpenStart(Expression tagName)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpenStart(null));
			return Expression.Call(_instance, method, tagName);
		}

		public Expression ElementOpenEnd()
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpenEnd());
			return Expression.Call(_instance, method);
		}

		public Expression ElementOpen(Expression tagName)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpen(null));
			return Expression.Call(_instance, method, tagName);
		}

		public Expression ElementClose()
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementClose());
			return Expression.Call(_instance, method);
		}

		public Expression PropertyStart(Expression propertyName)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.PropertyStart(null));
			return Expression.Call(_instance, method, propertyName);
		}

		public Expression PropertyEnd()
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.PropertyEnd());
			return Expression.Call(_instance, method);
		}

		public Expression Value(Expression value)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.Value(null));
			return Expression.Call(_instance, method, value);
		}
	}
}