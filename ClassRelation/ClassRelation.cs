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
    public class ClassRelation
    {
        private Dictionary<string, ClassInfo> _classContainer;

        private Dictionary<string, InterfaceInfo> _interfaceContainer;

        /// <summary>
        ///
        /// </summary>
        public Dictionary<string, Dictionary<string, TypeInfo>> NamespaceContainer
        {            
            get
            {               
                Dictionary<string, Dictionary<string, TypeInfo>> namespaces = new Dictionary<string, Dictionary<string, TypeInfo>>();
                var groups= TypeInfoContainer.GroupBy(kv =>
                   {
                       int pos = kv.Key.LastIndexOf('.');
                       pos = pos == -1 ? 0 : pos;
                       string typeShortNmae = kv.Key.Substring(0,pos);
                       return typeShortNmae;
                   });
                foreach (var group in groups)
                {
                    var dic = new Dictionary<string, TypeInfo>();
                    foreach (var item in group)
                    {                       
                        dic.Add(item.Key, item.Value);
                    }
                    namespaces[group.Key] = dic;
                }
                return namespaces;
            }
        }

        public Dictionary<string, TypeInfo> TypeInfoContainer
        {
            get
            {
                Dictionary<string, TypeInfo> container = new Dictionary<string, TypeInfo>();
                foreach (var key in _classContainer.Keys)
                {
                    container[key] = _classContainer[key];
                }

                foreach (var key in _interfaceContainer.Keys)
                {
                    container[key] = _interfaceContainer[key];
                }

                return container;
            }
        }

        public ClassRelation()
        {
            _classContainer = new Dictionary<string, ClassInfo>();
            _interfaceContainer = new Dictionary<string, InterfaceInfo>();
        }

        public void LoadFromAssemblyFile(string fileName)
        {
            AssemblyDefinition asmDe = AssemblyDefinition.ReadAssembly(fileName);
            foreach (var modul in asmDe.Modules)
            {
                foreach (var type in modul.Types)
                {
                    if (type.IsClass)
                        LoadClassFromType(type);
                    else if (type.IsInterface)
                        LoadInterfaceFromType(type);
                }
            }
        }

        protected ClassInfo LoadClassFromType(TypeDefinition type)
        {
            if (type == null)
                return null;
            var classInfo = default(ClassInfo);

            if (_classContainer.ContainsKey(type.FullName))
                classInfo = _classContainer[type.FullName];
            else
            {
                classInfo = new ClassInfo() { Name = type.Name, FullName = type.FullName };
                classInfo.BaseClassInfo = LoadClassFromType(type.BaseType as TypeDefinition);
                _classContainer.Add(type.FullName, classInfo);
                foreach (var interfaceType in type.Interfaces)
                {
                    if (IsDirectBaseInterface(interfaceType, type.Interfaces))
                    {
                        var baseintInfo = LoadInterfaceFromType(interfaceType as TypeDefinition);
                        if (baseintInfo != null)
                            classInfo.BaseInterfaceInfos.Add(baseintInfo);
                    }
                }
            }           
            return classInfo;
        }

        /// <summary>
        /// this  is direct base interface?
        /// </summary>
        /// <param name="inet"></param>
        /// <param name="baseInterfaces"></param>
        /// <returns></returns>
        private bool IsDirectBaseInterface(TypeReference inet, Collection<TypeReference> baseInterfaces)
        {
            foreach (var itme in baseInterfaces)
            {
                if (itme != inet)
                {
                    var itmeas = itme as TypeDefinition;
                    if (itmeas != null)
                        if (itmeas.Interfaces.Contains(inet))
                            return false;
                        else
                            IsDirectBaseInterface(inet, itmeas.Interfaces);
                }
            }
            return true;
        }

        protected InterfaceInfo LoadInterfaceFromType(TypeDefinition type)
        {
            if (type == null)
                return null;
            var inferfaceInfo = default(InterfaceInfo);
            if ((_interfaceContainer.ContainsKey(type.FullName)))
                inferfaceInfo = _interfaceContainer[type.FullName];
            else
            {
                inferfaceInfo = new InterfaceInfo() { Name = type.Name, FullName = type.FullName };
                foreach (var interfaceType in type.Interfaces)
                {
                    if (IsDirectBaseInterface(interfaceType, type.Interfaces))
                    {
                        var baseintInfo = LoadInterfaceFromType(interfaceType as TypeDefinition);
                        if (baseintInfo != null)
                            inferfaceInfo.BaseInterfaceInfos.Add(baseintInfo);
                    }
                }
                _interfaceContainer.Add(type.FullName, inferfaceInfo);
            }           
            return inferfaceInfo;
        }

        public ClassInfo FindClass(string fullName)
        {
            if (_classContainer.ContainsKey(fullName))
            {
                var classInfo = _classContainer[fullName];
                LoadClassSubType(classInfo);
                return classInfo;
            }
            return null;
        }

        public InterfaceInfo FindInterface(string fullName)
        {
            if (_interfaceContainer.ContainsKey(fullName))
            {
                var interfaceInfo = _interfaceContainer[fullName];
                LoadInterfaceSubType(interfaceInfo);
                return interfaceInfo;
            }
            return null;
        }

        public Bitmap Draw(ClassInfo classInfo)
        {
            LoadClassSubType(classInfo);
            var size = GetClassGraphicSize(classInfo);

            int xMaxCount = size.Item1;
            int yMaxCount = size.Item2;
            ClassGraphic graphic = new ClassGraphic(xMaxCount, yMaxCount);
            return graphic.Draw(classInfo);
        }

        public Bitmap Draw(ClassInfo classInfo,Graphics g)
        {
            var size = GetClassGraphicSize(classInfo);

            int xMaxCount = size.Item1;
            int yMaxCount = size.Item2;
            ClassGraphic graphic = new ClassGraphic(xMaxCount, yMaxCount,g);
            return graphic.Draw(classInfo);
        }


        protected void LoadClassSubType(ClassInfo baseClass)
        {
            foreach (ClassInfo itemType in _classContainer.Values)
            {             
                if (itemType.BaseClassInfo == baseClass)
                    if (!baseClass.SubType.Contains(itemType))
                    {
                        baseClass.SubType.Add(itemType);
                        itemType.BaseClassInfo = baseClass;
                        LoadClassSubType(itemType);
                    }
            }
        }

        protected void LoadInterfaceSubType(InterfaceInfo baseInterface)
        {
            foreach (var key in _interfaceContainer.Keys)
            {
                var itemType = this._interfaceContainer[key];
                if (itemType.BaseInterfaceInfos.Contains(baseInterface))
                    if (!baseInterface.SubType.Contains(itemType))
                    {
                        baseInterface.SubType.Add(itemType);
                        LoadInterfaceSubType(itemType);
                    }
            }
        }

        protected void CalcBaseTypeDepth(TypeInfo info, List<TypeInfo> _tempTypeContainer)
        {
            if (info == null)
                return;

            if (!_tempTypeContainer.Contains(info))
                _tempTypeContainer.Add(info);

            if (info is ClassInfo)
            {
                var classInfo = info as ClassInfo;
                if (classInfo.BaseClassInfo != null)
                {
                    //此处的判断是为了保证一个类被多个类继承，已经走了一条分支为其深度付值了
                   // if (classInfo.BaseClassInfo.Depth < info.Depth + 1)
                        classInfo.BaseClassInfo.Depth = info.Depth + 1;                   
                    CalcBaseTypeDepth(classInfo.BaseClassInfo, _tempTypeContainer);
                }
            }
            foreach (var inte in info.BaseInterfaceInfos)
            {
                if (inte != null)
                {
                   // if (inte.Depth < info.Depth + 1)
                        inte.Depth = info.Depth + 1;                   
                    CalcBaseTypeDepth(inte, _tempTypeContainer);
                }
            }
        }

        private int CaclTypeDepth(TypeInfo typeInfo ,List<TypeInfo> _tempTypeContainer)
        {
            if (typeInfo == null)
                return -1;
            if (!_tempTypeContainer.Contains(typeInfo))
                _tempTypeContainer.Add(typeInfo);
            int level = 1;
            int maxDepth = 1;
            foreach (var sub in typeInfo.SubType)
            {
                level = CaclTypeDepth(sub as ClassInfo, _tempTypeContainer) + 1;
                if (level > maxDepth)
                    maxDepth = level;

            }
            return maxDepth;
        }

        protected void CalcSubClassDepth(ClassInfo info, List<TypeInfo> _tempTypeContainer)
        {
            if (info == null)
                return;
            if (!_tempTypeContainer.Contains(info))
                _tempTypeContainer.Add(info);
            foreach (var sub in info.SubType)
            {
                sub.Depth = info.Depth - 1;
                CalcSubClassDepth(sub as ClassInfo, _tempTypeContainer);              
            }
        }

       
        protected Tuple<int, int> GetClassGraphicSize(ClassInfo classInfo)
        {
            List<TypeInfo> _tempTypeContainer = new List<TypeInfo>();
            classInfo.Depth = CaclTypeDepth(classInfo, _tempTypeContainer);
            CalcSubClassDepth(classInfo, _tempTypeContainer);
            CalcBaseTypeDepth(classInfo, _tempTypeContainer);

            int maxDepth = 0;
            int maxCount = 0;
            if (_tempTypeContainer.Count > 0)
            {
                maxDepth = _tempTypeContainer.Max((info) => { return info.Depth; });
                maxCount = _tempTypeContainer.Max((info) => {
                    int baseCount = info.BaseCount;
                    int subCount = classInfo.SubType.Count;
                    if (info.Depth <= classInfo.Depth)
                    {
                        int count = 0;
                        info.SubType.ForEach(type => {
                            count += type.SubType.Count;
                        });
                        subCount = subCount > count ? subCount : count;
                    }
                    return baseCount > subCount ? baseCount : subCount;
                });
            }        

            return new Tuple<int, int>(maxCount, maxDepth);
        }
    }
}
