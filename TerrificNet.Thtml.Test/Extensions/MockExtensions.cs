using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;

namespace TerrificNet.Thtml.Test.Extensions
{
	internal static class MockExtensions
	{
		public static Mock<T> InSequence<T>(this Mock<T> mock, params Expression<Action<T>>[] expressions) where T : class
		{
			return InSequence(mock, (IEnumerable<Expression<Action<T>>>) expressions);
		}

		public static Mock<T> InSequence<T>(this Mock<T> mock, IEnumerable<Expression<Action<T>>> expressions) where T : class
		{
			var sequence = new MockSequence();
			foreach (var expression in expressions)
			{
				mock.InSequence(sequence).Setup(expression);
			}

			return mock;
		}
	}
}