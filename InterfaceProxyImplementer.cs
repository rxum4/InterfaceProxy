using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation
{
    public class InterfaceProxyImplementer : IInterfaceImplementer
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

        private IEnumerable<Type> GenerateInterfaceList(Type interfaceType)
        {
            IEnumerable<Type> list = new[] { interfaceType };
            foreach (var e in interfaceType.GetInterfaces())
                list = list.Union(GenerateInterfaceList(e));
            return list;
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
        
        private void ImplementInterface(TypeBuilder builder, Type interfaceType)
        {
            var stub = new StubImplementer(builder);
            var ifaces = builder.BaseType.GetInterfaces();

            var eventImpl = ifaces.Contains(typeof(IEventProxy))? (IEventImplementer)new EventImplementer(builder):stub;
            foreach (var e in interfaceType.GetEvents()) eventImpl.ImplementEvent(e);

            var propImpl = ifaces.Contains(typeof(IPropertyProxy)) ? (IPropertyImplementer)new PropertyImplementer(builder) : stub;
            foreach (var e in interfaceType.GetProperties()) propImpl.ImplementProperty(e);

            //implement "non special" methods (no getter, setter, events, constructors or operators)
            var methods = ifaces.Contains(typeof(IInvokationProxy)) ? (IMethodImplementer)new MethodImplementer(builder) : stub;
            foreach (var e in interfaceType.GetMethods().Where(m=>!m.IsSpecialName)) methods.ImplementMethod(e);
        }
    }
}
