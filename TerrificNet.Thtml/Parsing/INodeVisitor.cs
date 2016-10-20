using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
	public interface INodeVisitor<out T>
	{
		T Visit(Element element);
		T Visit(TextNode textNode);
		T Visit(Statement statement);
		T Visit(AttributeNode attributeNode);
		T Visit(AttributeContentStatement constantAttributeContent);
		T Visit(ConstantAttributeContent attributeContent);
		T Visit(Document document);
		T Visit(CompositeAttributeContent compositeAttributeContent);
		T Visit(CallHelperExpression callHelperExpression);
		T Visit(UnconvertedExpression unconvertedExpression);
		T Visit(AttributeStatement attributeStatement);
		T Visit(IterationExpression iterationExpression);
		T Visit(ConditionalExpression conditionalExpression);
		T Visit(MemberExpression memberExpression);
		T Visit(ParentExpression parentExpression);
		T Visit(SelfExpression selfExpression);
	}

	public interface INodeVisitor
	{
		void Visit(Element element);
		void Visit(TextNode textNode);
		void Visit(Statement statement);
		void Visit(AttributeNode attributeNode);
		void Visit(AttributeContentStatement constantAttributeContent);
		void Visit(ConstantAttributeContent attributeContent);
		void Visit(Document document);
		void Visit(CompositeAttributeContent compositeAttributeContent);
		void Visit(CallHelperExpression callHelperExpression);
		void Visit(UnconvertedExpression unconvertedExpression);
		void Visit(AttributeStatement attributeStatement);
		void Visit(IterationExpression iterationExpression);
		void Visit(ConditionalExpression conditionalExpression);
		void Visit(MemberExpression memberExpression);
		void Visit(ParentExpression parentExpression);
		void Visit(SelfExpression selfExpression);
	}
}