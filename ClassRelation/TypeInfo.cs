using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassRelation
{
    public class TypeInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public Type InfoType { get; set; }
        public int Depth { get; set; }
        public virtual int BaseCount
        {
            get
            {
                if (BaseInterfaceInfos == null)
                    return 0;
                return BaseInterfaceInfos.Count;

            }
        }
        public virtual int BaseCountInSameDepth
        {
            get
            {
                int count = 0;
                foreach (var item in BaseInterfaceInfos)
                {
                    if (item != null && item.Depth == this.Depth + 1)
                        count++;
                }
                return count;
            }
        }

        public List<InterfaceInfo> BaseInterfaceInfos { get; set; }

        public List<TypeInfo> SubType { get; set; }

    }

    public class ClassInfo : TypeInfo
    {
        public ClassInfo BaseClassInfo { get; set; }

        public ClassInfo()
        {
            BaseInterfaceInfos = new List<InterfaceInfo>();
            SubType = new List<TypeInfo>();
            Depth = 1;
        }

        public Tuple<int, int> GetPosition()
        {
            throw new NotImplementedException();
        }

        public override int BaseCount
        {
            get
            {
                if (BaseClassInfo != null)
                    return base.BaseCount + 1;
                return base.BaseCount;
            }
        }

        public override int BaseCountInSameDepth
        {
            get
            {
                int count = base.BaseCountInSameDepth;
                if (BaseClassInfo != null && BaseClassInfo.Depth == this.Depth + 1)
                    count= count + 1;             
                return count;
            }
        }

    }

    public class InterfaceInfo : TypeInfo
    {
        public InterfaceInfo()
        {
            BaseInterfaceInfos = new List<InterfaceInfo>();
            SubType = new List<TypeInfo>();
            Depth = 1;
        }
    }
}
