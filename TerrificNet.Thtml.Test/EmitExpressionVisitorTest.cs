﻿using System;
using System.Linq.Expressions;
using Moq;
using Moq.Language;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Rendering;
using Xunit;
using TerrificNet.Thtml.Test.Extensions;
using MemberExpression = TerrificNet.Thtml.Parsing.Handlebars.MemberExpression;

namespace TerrificNet.Thtml.Test
{
	public class EmitExpressionVisitorTest
	{
		private readonly IBindingScope _bindingScope;

		public EmitExpressionVisitorTest()
		{
			_bindingScope = new Mock<IBindingScope>().Object;
		}

		[Fact]
		public void EmitExpressionVisior_Element_NewScope()
		{
			TestSequence(new Document(new Element("div")), 
				a => a.Setup(s => s.Enter()), 
				a => a.Setup(s => s.Enter()), 
				a => a.Setup(s => s.Leave()).Returns(_bindingScope), 
				a => a.Setup(s => s.Leave()).Returns(_bindingScope));
		}

		[Fact]
		public void EmitExpressionVisitor_Attribute_NewScope()
		{
			TestSequence(new Document(new Element("div", new [] { new AttributeNode("attr", new AttributeContentStatement(new MemberExpression("value")))})),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.UseBinding(It.IsAny<IBinding>())),
				a => a.Setup(s => s.Leave()).Returns(_bindingScope),
				a => a.Setup(s => s.Leave()).Returns(_bindingScope),
				a => a.Setup(s => s.Leave()).Returns(_bindingScope));
		}

		[Fact]
		public void EmitExpressionVisitor_ChildElement_NewScope()
		{
			TestSequence(new Document(new Element("div", new Statement(new MemberExpression("value")), new Element("div"))),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.UseBinding(It.IsAny<IBinding>())),
				a => a.Setup(s => s.Enter()),
				a => a.Setup(s => s.Leave()).Returns(_bindingScope),
				a => a.Setup(s => s.Leave()).Returns(_bindingScope),
				a => a.Setup(s => s.Leave()).Returns(_bindingScope),
				a => a.Setup(s => s.Leave()).Returns(_bindingScope));
		}

		private static void TestSequence(Document input, params Action<ISetupConditionResult<IScopedExpressionBuilder>>[] sequence)
		{
			var formatter = new Mock<IOutputExpressionBuilder>();

			var scopedExpressionBuilderMock = new Mock<IScopedExpressionBuilder>(MockBehavior.Strict);

			scopedExpressionBuilderMock.InSequence(sequence);

			var dataScopeContract = new DataScopeContract(BindingPathTemplate.Global);
			var underTest = new EmitExpressionVisitor(dataScopeContract, CompilerExtensions.Default.WithOutput(formatter.Object),
				Expression.Parameter(typeof(IRenderingContext)), scopedExpressionBuilderMock.Object);

			underTest.Visit(input);

			scopedExpressionBuilderMock.Verify();
		}
	}
}