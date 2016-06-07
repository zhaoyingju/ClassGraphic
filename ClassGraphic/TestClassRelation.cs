using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDll
{
    public interface I
    { }
    public interface IC:I
    { }

    public interface IA : IC
    { }

    public interface IB : IC
    { }


    public class ClassA:IA
    {

    }

    public class ClassAB : IA,IB
    {

    }

    public class ClassC : ClassAB, I
    { 
    
    }

    public class ClassC1 : ClassC
    {

    }
    public class ClassC2 : ClassC
    {

    }

    public class ClassD : ClassAB
    {

    }

    public class ClassD1 : ClassD
    {

    }
    public class ClassD2 : ClassD
    {

    }
}
