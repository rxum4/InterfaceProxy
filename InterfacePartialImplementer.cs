using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation
{
    public class InterfacePartialImplementer : IInterfaceImplementer
    {
        public InterfaceProxyImplementer(object BaseTypeObject) : this((BaseTypeObject ?? throw new ArgumentNullException(nameof(BaseTypeObject))).GetType()) { }

        public InterfaceProxyImplementer(Type BaseType)
        {
            this.BaseType = BaseType ?? throw new ArgumentNullException(nameof(BaseType));
            if (BaseType.IsNotPublic) throw new Exception();
            if (BaseType.IsAbstract) throw new Exception();
        }

        public Type BaseType { get; }

        public TInterface CreateInterfaceInstance<TInterface>(params object[] ctorArgs)
        {
            var iface = typeof(TInterface);
            if (iface.IsNotPublic) throw new Exception();
            if (!iface.IsInterface) throw new Exception();
            var type = GenerateImplementingType(iface);
            var result = Activator.CreateInstance(type, ctorArgs);
            return (TInterface)result;
        }

        private Type GenerateImplementingType(Type interfaceType)
        {
            var typeBuilderFactory = new TypeBuilderFactory();
            TypeBuilder builder = typeBuilderFactory.CreatePublicTypeBuilder($"{interfaceType.Name}{BaseType.Name}_{Guid.NewGuid().ToString()}");
            ApplyBaseTypeInfo(builder);

            builder.AddInterfaceImplementation(interfaceType);
            foreach (var e in GenerateInterfaceList(interfaceType))
                ImplementInterface(builder, e);
            var type = builder.CreateType();
            typeBuilderFactory.AssemblyBuilder.Save($"{typeBuilderFactory.AssemblyName.Name}.dll");
            return type;
        }
        private void ApplyBaseTypeInfo(TypeBuilder builder)
        {
            builder.SetParent(BaseType);
            IEnumerable<ConstructorInfo> ctors = BaseType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            ctors = ctors.Where(m => !m.IsPrivate);
            if (!ctors.Any()) throw new Exception();
            var ctorImplementer = new ConstructorImplementer(builder);
            foreach (var ctor in ctors) ctorImplementer.ImplementConstructor(ctor);
        }
    }
}
