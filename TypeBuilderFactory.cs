using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation
{
    public class TypeBuilderFactory
    {
        public AssemblyName AssemblyName { get; private set; }
        public AssemblyBuilder AssemblyBuilder { get; private set; }
        public ModuleBuilder ModuleBuilder { get; private set; }
        public bool EmitSymbols { get; set; }
        public TypeBuilderFactory()
        {
            var entryAssembly = Assembly.GetEntryAssembly().GetName();
            AssemblyName = entryAssembly.Clone() as AssemblyName;
            AssemblyName.Name = $"{AssemblyName.Name}.RuntimeProxys";
        }
        public TypeBuilder CreatePublicTypeBuilder(string typeName)
        {
            AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(AssemblyName.Name,$"{AssemblyName.Name}.dll",true);

            TypeBuilder tb = ModuleBuilder.DefineType(typeName, TypeAttributes.Public);
            return tb;
        }
    }
}
