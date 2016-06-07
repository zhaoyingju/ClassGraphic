using ClassRelation;
using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ClassTool
{
    public class GraphicElementManager : Dictionary<int, List<GraphicElement>>
    {
        public GraphicElement GetElement(TypeInfo type)
        {

            foreach (var key in this.Keys)
            {
                foreach (var el in this[key])
                {
                    if (el.IsOwned(type))
                        return el;
                }
            }
            return null;
        }

        public void Add(int depth, GraphicElement el)
        {
            if (!this.ContainsKey(depth))
            {
                this[depth] = new List<GraphicElement>() { el };
            }
            else
                this[depth].Add(el);
        }      

        public int GetCountInSameDepth(int depth)
        {
            if (!this.ContainsKey(depth))
                return 0;
            else
                return this[depth].Count;
        }
    }
}
