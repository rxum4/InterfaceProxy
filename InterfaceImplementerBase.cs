using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation
{
    public abstract class InterfaceImplementerBase : IInterfaceImplementer
    {
        protected InterfaceImplementerBase(object BaseTypeObject) : this((BaseTypeObject ?? throw new ArgumentNullException(nameof(BaseTypeObject))).GetType()) { }

        protected InterfaceImplementerBase(Type BaseType)
        {
            this.BaseType = BaseType ?? throw new ArgumentNullException(nameof(BaseType));
        }

        public Type BaseType { get; }

        public virtual TInterface CreateInterfaceInstance<TInterface>(params object[] ctorArgs)
        {
            var iface = typeof(TInterface);
            var type = GenerateImplementingType(iface);
            var result = Activator.CreateInstance(type, ctorArgs);
            return (TInterface)result;
        }

        public bool CheckTypeValidity(Type baseType,Type interfaceType, out Exception reason)
        {
            var exceptions = GetTypeValidationErros(baseType, interfaceType);
            switch (exceptions?.Count())
            {
                case null:
                case 0:
                    reason = null;
                    return true;
                case 1:
                    reason = exceptions.Single();
                    return false;
                default:
                    reason = new AggregateException(exceptions);
                    return false;
            }
        }
        protected virtual IEnumerable<Exception> GetTypeValidationErros(Type baseType, Type interfaceType)
        {
            var exs = new List<Exception>();
            if (BaseType.IsNotPublic)
                exs.Add(new TypeAccessException());
            
            if (BaseType.IsAbstract) throw new Exception();
            if (interfaceType.IsNotPublic) throw new Exception();
            if (!interfaceType.IsInterface) throw new Exception();
            return exs;
        }

        protected virtual Type GenerateImplementingType(Type interfaceType)
        {
            TypeBuilder builder = GenerateTypeBuilder(BaseType ,interfaceType);
            TunnelConstructors(builder, BaseType);

            foreach (var e in GenerateInterfaceList(interfaceType))
                ImplementInterface(builder, e);
            var type = builder.CreateType();
            return type;
        }

        protected virtual TypeBuilder GenerateTypeBuilder(Type baseType,Type interfaceType)
        {
            var typeBuilderFactory = new TypeBuilderFactory();
            TypeBuilder builder = typeBuilderFactory.CreatePublicTypeBuilder($"{interfaceType.Name}{BaseType.Name}_{Guid.NewGuid().ToString()}");
            builder.SetParent(BaseType);
            builder.AddInterfaceImplementation(interfaceType);
            return builder;
        }

        protected void TunnelConstructors(TypeBuilder builder,Type baseType)
        {
            IEnumerable<ConstructorInfo> ctors = baseType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public); ctors = ctors.Where(m => !m.IsPrivate);
            if (!ctors.Any()) throw new Exception();
            var ctorImplementer = new ConstructorImplementer(builder);
            foreach (var ctor in ctors) ctorImplementer.ImplementConstructor(ctor);
        }

        public virtual IEnumerable<Type> GenerateInterfaceList(Type interfaceType)
        {
            IEnumerable<Type> list = new[] { interfaceType };
            foreach (var e in interfaceType.GetInterfaces())
                list = list.Union(GenerateInterfaceList(e));
            return list;
        }

        internal abstract void ImplementInterface(TypeBuilder builder, Type interfaceType);

    }
}
