using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FixXAMLReferences;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            AssemblyName n = new AssemblyName("CoApp.Toolkit");

            var a = AssemblyCacheEnum.GetAssemblyStrongNames("CoApp.Toolkit").ToArray();
        }
    }
}
