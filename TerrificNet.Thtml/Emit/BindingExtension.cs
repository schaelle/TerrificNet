using System;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Schema;

namespace TerrificNet.Thtml.Emit
{
	public static class BindingExtension
	{
		public static void Train(this IBinding binding,
			Func<BindingResultDescriptionBuilder<string>, BindingResultDescription<string>> before,
			Func<BindingResultDescriptionBuilder<string>, BindingResultDescription<string>> after, ChangeOperation operation)
		{
			var tBinding = binding as DataScopeContract.Binding;
			if (tBinding == null)
				return;

			var descriptionBuilder = new BindingResultDescriptionBuilder<string>();
			var beforeResult = before(descriptionBuilder);
			var afterResult = after(descriptionBuilder);

			tBinding._collection.Add(tBinding.Path, beforeResult, afterResult, operation);
		}

		public static void TrainMove(this IBinding binding, ChangeOperation operation)
		{
			var tBinding = binding as DataScopeContract.Binding;
			tBinding?._collection.MoveNode(tBinding.Path, operation);
		}

		public static void TrainAdd(this IBinding binding, ChangeOperation operation)
		{
			var tBinding = binding as DataScopeContract.Binding;
			tBinding?._collection.AddNode(tBinding.Path, operation);
		}

		public static void TrainRemove(this IBinding binding, ChangeOperation operation)
		{
			var tBinding = binding as DataScopeContract.Binding;
			tBinding?._collection.RemoveNode(tBinding.Path, operation);
		}

		public static IBindingWithExpression EnsureBinding(this IBinding binding)
		{
			if (binding == null)
				throw new ArgumentNullException(nameof(binding));

			var exBinding = binding as IBindingWithExpression;
			if (exBinding == null)
				throw new NotSupportedException($"The binding with path '{binding.Path}' doesn't support bindings to server-side models.");

			return exBinding;
		}

		public static bool TryGetExpression(this IBinding binding, out Expression expression)
		{
			if (binding == null)
				throw new ArgumentNullException(nameof(binding));

			expression = null;

			var exBinding = binding as IBindingWithExpression;
			if (exBinding == null)
				return false;

			expression = exBinding.Expression;
			return true;
		}
	}
}