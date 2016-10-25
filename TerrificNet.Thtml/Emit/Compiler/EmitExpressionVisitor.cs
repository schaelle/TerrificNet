using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using ConditionalExpression = TerrificNet.Thtml.Parsing.Handlebars.ConditionalExpression;
using MemberExpression = TerrificNet.Thtml.Parsing.Handlebars.MemberExpression;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class EmitExpressionVisitor : NodeVisitorBase, INodeCompilerVisitor
	{
		private readonly IDataScopeContract _dataScopeContract;
		private readonly IHelperBinder _helperBinder;
		private readonly CompilerExtensions _extensions;
		private readonly Expression _renderingContextExpression;
		private readonly IOutputExpressionBuilder _expressionBuilder;
		private readonly IExpressionBuilder _exBuilder;

		public EmitExpressionVisitor(IDataScopeContract dataScopeContract, CompilerExtensions extensions, Expression renderingContextExpression, IExpressionBuilder expressionBuilder)
		{
			_dataScopeContract = dataScopeContract;
			_extensions = extensions;
			_renderingContextExpression = renderingContextExpression;
			_helperBinder = _extensions.HelperBinder;
			_expressionBuilder = _extensions.ExpressionBuilder;
			_exBuilder = expressionBuilder;
		}

		public override void Visit(Document document)
		{
			foreach (var child in document.ChildNodes)
				child.Accept(this);
		}

		public override void Visit(Element element)
		{
			var tagResult = _extensions.TagHelper.FindByName(element);
			if (tagResult != null)
			{
				tagResult.CreateExpression(new HelperParameters(_dataScopeContract, this, _extensions, _renderingContextExpression, _exBuilder));
				return;
			}

			var staticAttributeNodes = element.Attributes.Where(e => e.IsFixed).ToList();
			var staticAttributeList = CreateAttributeDictionary(staticAttributeNodes);
			var attributeList = element.Attributes.Except(staticAttributeNodes).ToList();

			if (attributeList.Count > 0)
			{
				_exBuilder.Add(_expressionBuilder.ElementOpenStart(element.TagName, staticAttributeList));
				foreach (var attr in element.Attributes)
					attr.Accept(this);

				_exBuilder.Add(_expressionBuilder.ElementOpenEnd());
			}
			else
				_exBuilder.Add(_expressionBuilder.ElementOpen(element.TagName, staticAttributeList));

			foreach (var child in element.ChildNodes)
				child.Accept(this);

			_exBuilder.Add(_expressionBuilder.ElementClose(element.TagName));
		}

		public override void Visit(AttributeNode attributeNode)
		{
			_exBuilder.Add(_expressionBuilder.PropertyStart(attributeNode.Name));
			attributeNode.Value.Accept(this);
			_exBuilder.Add(_expressionBuilder.PropertyEnd());
		}

		public override void Visit(ConstantAttributeContent attributeContent)
		{
			_exBuilder.Add(_expressionBuilder.Value(Expression.Constant(attributeContent.Text)));
		}

		public override void Visit(AttributeContentStatement constantAttributeContent)
		{
			HandleStatement(constantAttributeContent.Expression, constantAttributeContent.Children);
		}

		public override void Visit(Statement statement)
		{
			var expression = statement.Expression;
			HandleStatement(expression, statement.ChildNodes);
		}

		public override void Visit(UnconvertedExpression unconvertedExpression)
		{
			unconvertedExpression.Expression.Accept(this);
		}

		public override void Visit(CompositeAttributeContent compositeAttributeContent)
		{
			foreach (var part in compositeAttributeContent.ContentParts)
				part.Accept(this);
		}

		public override void Visit(MemberExpression memberExpression)
		{
			HandleCall(memberExpression);
		}

		public override void Visit(ParentExpression parentExpression)
		{
			HandleCall(parentExpression);
		}

		public override void Visit(SelfExpression selfExpression)
		{
			HandleCall(selfExpression);
		}

		private void HandleCall(MustacheExpression memberExpression)
		{
			var scope = ScopeEmitter.Bind(_dataScopeContract, memberExpression);
			var binding = scope.RequiresString();

			var expression = binding.Expression;
			_exBuilder.Add(_expressionBuilder.Value(expression));
		}

		public override void Visit(TextNode textNode)
		{
			_exBuilder.Add(_expressionBuilder.Value(Expression.Constant(textNode.Text)));
		}

		private void HandleStatement(MustacheExpression expression, IEnumerable<HtmlNode> childNodes)
		{
			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, iterationExpression.Expression);

				IDataScopeContract childScopeContract;
				var binding = scope.RequiresEnumerable(out childScopeContract);

				Action<Expression> childrenAction = l =>
				{
					var childVisitor = ChangeContract(childScopeContract);
					foreach (var child in childNodes)
						child.Accept(childVisitor);
				};

				var collection = binding.Expression;

				_exBuilder.Foreach(collection, childrenAction, (ParameterExpression) childScopeContract.Expression);

				return;
			}

			var conditionalExpression = expression as ConditionalExpression;
			if (conditionalExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, conditionalExpression.Expression);
				var binding = scope.RequiresBoolean();
				var testExpression = binding.Expression;

				Action children = () =>
				{
					foreach (var c in childNodes)
						c.Accept(this);
				};

				_exBuilder.IfThen(testExpression, children);
				return;
			}

			var callHelperExpression = expression as CallHelperExpression;
			if (callHelperExpression != null)
			{
				var result = _helperBinder.FindByName(callHelperExpression.Name, CreateDictionaryFromArguments(callHelperExpression.Attributes));
				if (result == null)
					throw new Exception($"Unknown helper with name {callHelperExpression.Name}.");

				result.CreateExpression(new HelperParameters(_dataScopeContract, this, _extensions, _renderingContextExpression, _exBuilder));
				return;
			}

			expression.Accept(this);

			foreach (var child in childNodes)
				child.Accept(this);
		}

		private static IDictionary<string, string> CreateDictionaryFromArguments(IEnumerable<HelperAttribute> attributes)
		{
			return attributes.ToDictionary(d => d.Name, d => d.Value);
		}

		public INodeCompilerVisitor ChangeContract(IDataScopeContract childScopeContract)
		{
			return new EmitExpressionVisitor(childScopeContract, _extensions, _renderingContextExpression, _exBuilder);
		}

		public INodeCompilerVisitor ChangeExtensions(CompilerExtensions extensions)
		{
			return new EmitExpressionVisitor(_dataScopeContract, extensions, _renderingContextExpression, _exBuilder);
		}

		public void Visit(IEnumerable<Node> nodes)
		{
			foreach (var node in nodes)
				node.Accept(this);
		}

		private static IReadOnlyDictionary<string, string> CreateAttributeDictionary(IEnumerable<ElementPart> staticAttributeNodes)
		{
			var dict = new Dictionary<string, string>();

			var visitor = new AttributeDictionaryVisitor(dict);
			foreach (var node in staticAttributeNodes)
			{
				node.Accept(visitor);
			}

			return dict;
		}
	}
}