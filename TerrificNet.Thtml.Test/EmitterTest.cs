﻿using System.Collections.Generic;
using Moq;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.Test.Asserts;
using TerrificNet.Thtml.VDom;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class EmitterTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestEmit(string description, Document input, IDataBinder dataBinder, object data, VTree expected, IHelperBinder helperBinder)
        {
            var compiler = new Emitter();
            var method = compiler.Emit(input, dataBinder, helperBinder);

            var result = method.Execute(new ObjectDataContext(data), null);

            VTreeAsserts.AssertTree(expected, result);
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[]
                {
                    "empty document",
                    new Document(),
                    new NullDataBinder(),
                    null,
                    new VNode(),
                    new NullHelperBinder()
                };

                yield return new object[]
                {
                    "one element",
                    new Document(
                        new Element("h1", 
                            new TextNode("hallo"))),
                    new NullDataBinder(),
                    null,
                    new VNode(
                        new VElement("h1", 
                            new VText("hallo"))),
                    new NullHelperBinder()
                };

                yield return new object[]
                {
                    "element with attributes",
                    new Document(
                        new Element("h1", new ElementPart [] { new AttributeNode("attr1", new ConstantAttributeContent("hallo")) },
                            new Element("h2", new ElementPart [] { new AttributeNode("attr2", new ConstantAttributeContent("hallo2")) })
                        )),
                    new NullDataBinder(),
                    null,
                    new VNode(
                        new VElement("h1", new[] { new VProperty("attr1", new StringVPropertyValue("hallo")) },
                            new VElement("h2", new[] { new VProperty("attr2", new StringVPropertyValue("hallo2")) }))
                        ),
                    new NullHelperBinder()
                };

                var obj = new { Name = "hallo" };   
                yield return new object[]
                {
                    "one element with dynamic expression",
                    new Document(
                        new Element("h1",
                            new Statement(new MemberExpression("name")))),
                    TypeDataBinder.BinderFromObject(obj),
                    obj,
                    new VNode(
                        new VElement("h1",
                            new VText("hallo"))),
                    new NullHelperBinder()
                };

                var obj2 = new
                {
                    Items = new[] { new Dummy { Name = "test1" }, new Dummy { Name = "test2" } }
                };
                yield return new object[]
                {
                    "one element with iteration expression",
                    new Document(
                        new Element("h1",
                            new Statement(
                                new IterationExpression(new MemberExpression("items")), 
                            new Element("div", 
                                new Statement(new MemberExpression("name")))))),
                    TypeDataBinder.BinderFromObject(obj2),
                    obj2,
                    new VNode(
                        new VElement("h1",
                            new VElement("div",
                                new VText("test1")),
                            new VElement("div",
                                new VText("test2")))),
                    new NullHelperBinder()
                };
                var obj3 = new { Name = "value" };
                yield return new object[]
                {
                    "one element with attribute expression",
                    new Document(
                        new Element("h1", new ElementPart[] { new AttributeNode("title", new AttributeContentStatement(new MemberExpression("name"))) })),
                    TypeDataBinder.BinderFromObject(obj3),
                    obj3,
                    new VNode(
                        new VElement("h1", new[] { new VProperty("title", new StringVPropertyValue("value")) }, null)),
                    new NullHelperBinder()
                };

                var obj4 = new
                {
                    List = new []
                    {
                      new { Do = false, Value = "hallo1" },
                      new { Do = true, Value = "hallo2" },
                    }
                };

                yield return new object[]
                {
                    "one element with iteration and including conditional expression",
                    new Document(
                        new Statement(new IterationExpression(new MemberExpression("list")),
                            new Statement(new ConditionalExpression(new MemberExpression("do")),
                                new Element("h1", new Statement(new MemberExpression("value"))))
                        )),
                    TypeDataBinder.BinderFromObject(obj4),
                    obj4,
                    new VNode(
                        new VElement("h1", new VText("hallo2"))
                        ),
                    new NullHelperBinder()
                };

                var result = new Mock<HelperBinderResult>(MockBehavior.Loose);
                result.Setup(d => d.CreateEmitter(It.IsAny<IListEmitter<VTree>>(), It.IsAny<IHelperBinder>(), It.IsAny<IDataBinder>())).Returns(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText("helper output"))));

                var helper = new Mock<IHelperBinder>();
                helper.Setup(h => h.FindByName("helper", It.IsAny<IDictionary<string, string>>())).Returns(result.Object);

                yield return new object[]
                {
                    "one element with helper",
                    new Document(
                        new Element("h1", new Statement(new CallHelperExpression("helper")))),
                    TypeDataBinder.BinderFromObject(obj3),
                    obj3,
                    new VNode(
                        new VElement("h1", new VText("helper output"))),
                    helper.Object
                };
            }
        }
    }

    public class Dummy
    {
        public string Name { get; set; }
    }

    public class NullDataBinder : IDataBinder
    {
        public DataBinderResult Property(string propertyName)
        {
            return null;
        }

        public DataBinderResult Item()
        {
            return null;
        }

        public DataBinderResult Context()
        {
            return null;
        }
    }
}