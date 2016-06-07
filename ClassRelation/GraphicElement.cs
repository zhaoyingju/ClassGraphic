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
    public class GraphicElement
    {

        private int _yMaxCount;
        public TypeInfo Info { get; set; }

        public int Depth {
            get
            {
                if (Info == null) return 0;
                 return Math.Abs(Info.Depth - this._yMaxCount) + 1;
            }
        }

        public int Index { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public GraphicElement(int yMaxCount)
        {
            _yMaxCount = yMaxCount;
        }

        public bool IsOwned(TypeInfo type)
        {
            if (Info.FullName == type.FullName)
                return true;
            return false;
        }
    }
}
