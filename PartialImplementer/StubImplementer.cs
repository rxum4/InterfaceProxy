using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation
{
    public class StubImplementer : IEventImplementer, IMethodImplementer,IPropertyImplementer
    {
        public StubImplementer(TypeBuilder TypeBuilder)
        {
            this.TypeBuilder = TypeBuilder;
        }

        public TypeBuilder TypeBuilder { get; }
        public void ImplementNOPFor(MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType & ~MemberTypes.NestedType & ~MemberTypes.Custom)
            {
                //case MemberTypes.Constructor:
                //    NopCtor((ConstructorInfo)memberInfo);
                //    break;
                case MemberTypes.Event:
                    ImplementEvent((EventInfo)memberInfo);
                    break;
                case MemberTypes.Method:
                    ImplementMethod((MethodInfo)memberInfo);
                    break;
                case MemberTypes.Property:
                    ImplementProperty((PropertyInfo)memberInfo);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void ImplementProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetMethod != null)
                ImplementMethod(propertyInfo.GetMethod);
            if (propertyInfo.SetMethod != null)
                ImplementMethod(propertyInfo.SetMethod);
        }

        public void ImplementMethod(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var methodBuilder = TypeBuilder.DefineMethod($"<{methodInfo.DeclaringType.Name}>{methodInfo.Name}",
                                                    MethodAttributes.Public | MethodAttributes.Virtual,
                                                    methodInfo.CallingConvention,
                                                    methodInfo.ReturnType,
                                                    parameters.Select(p => p.ParameterType).ToArray());
            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Nop);
            if (methodInfo.ReturnType != typeof(void))
            {
                var field = il.DeclareLocal(methodInfo.ReturnType);
                il.Emit(OpCodes.Ldloca, field);
                il.Emit(OpCodes.Initobj, methodInfo.ReturnType);
                il.Emit(OpCodes.Ldloc, field);
            }
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);
            TypeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
        }

        public void ImplementEvent(EventInfo eventInfo)
        {
            if (eventInfo.AddMethod != null)
                ImplementMethod(eventInfo.AddMethod);
            if (eventInfo.RemoveMethod != null)
                ImplementMethod(eventInfo.RemoveMethod);
            if (eventInfo.RaiseMethod != null)
                ImplementMethod(eventInfo.RaiseMethod);
        }
    }
}
