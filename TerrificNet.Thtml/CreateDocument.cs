using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml
{
    public class CreateDocument : CreateNode
    {
        public CreateDocument()
        {
            ChildNodes = new List<CreateNode>();
        }

        public CreateDocument(IEnumerable<CreateNode> childNodes)
        {
            ChildNodes = childNodes.ToList();
        }

        public CreateDocument(params CreateNode[] childNodes) 
            : this((IEnumerable<CreateNode>)childNodes)
        {
        }

        public IReadOnlyList<CreateNode> ChildNodes { get; }

    }
}