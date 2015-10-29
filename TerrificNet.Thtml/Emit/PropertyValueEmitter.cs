using System;
using System.Linq;
using System.Text;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class PropertyValueEmitter : EmitNodeVisitorBase<VPropertyValue>
    {
        public PropertyValueEmitter(IDataScopeContract dataScopeContract, IHelperBinder helperBinder) : base(dataScopeContract, helperBinder)
        {
        }

        public override IListEmitter<VPropertyValue> Visit(AttributeContentStatement constantAttributeContent)
        {
            return HandleStatement(constantAttributeContent.Expression, constantAttributeContent.Children);
        }

        public override IListEmitter<VPropertyValue> Visit(MemberExpression memberExpression)
        {
            var scope = ScopeEmitter.Bind(DataScopeContract, memberExpression);

            var evaluator = scope.RequiresString();
            return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new StringVPropertyValue(evaluator.Evaluate(d))));
        }

        public override IListEmitter<VPropertyValue> Visit(CompositeAttributeContent compositeAttributeContent)
        {
            return EmitterNode.Many(compositeAttributeContent.ContentParts.Select(p => p.Accept(this)).ToList());
        }

        public override IListEmitter<VPropertyValue> Visit(ConstantAttributeContent attributeContent)
        {
            return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new StringVPropertyValue(attributeContent.Text)));
        }

        protected override INodeVisitor<IListEmitter<VPropertyValue>> CreateVisitor(IDataScopeContract childScopeContract)
        {
            return new PropertyValueEmitter(childScopeContract, HelperBinder);
        }
    }
}