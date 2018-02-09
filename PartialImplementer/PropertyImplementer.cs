using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation
{
   public class PropertyImplementer : MemberImplementer, IPropertyImplementer
    {
        private static readonly MethodInfo GetPropertyMethod = typeof(Type).GetMethod(nameof(Type.GetProperty), new[] { typeof(string) });
        public TypeBuilder TypeBuilder { get; }
        public PropertyImplementer(TypeBuilder builder)
        {
            TypeBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public void ImplementProperty(PropertyInfo propertyInfo)
        {
            ImplementGetter(propertyInfo);
            ImplementSetter(propertyInfo);
        }

        private void ImplementGetter(PropertyInfo propertyInfo)
        {
            var methodInfo = propertyInfo.GetMethod;
            if (methodInfo == null || !methodInfo.IsPublic) return;
            MethodBuilder methodBuilder = GenerateMethodBuilder(methodInfo);
            var il = methodBuilder.GetILGenerator();
            PushInstance(il, methodInfo);
            PushMemberInfo(il, propertyInfo);
            PushArguments(il, methodInfo.GetParameters());
            ReadProxyReturnValue(il, typeof(IPropertyProxy).GetMethod(nameof(IPropertyProxy.GetPropertyValue)));
            UnboxOrCast(il, methodInfo.ReturnType); //cast to wanted type
            il.Emit(OpCodes.Ret); //return
            TypeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
        }

        private void ImplementSetter(PropertyInfo propertyInfo)
        {
            var methodInfo = propertyInfo.SetMethod;
            if (methodInfo == null || !methodInfo.IsPublic) return;
            MethodBuilder methodBuilder = GenerateMethodBuilder(methodInfo);
            var il = methodBuilder.GetILGenerator();
            PushInstance(il, methodInfo);
            PushMemberInfo(il, propertyInfo);
            PushArguments(il, methodInfo.GetParameters());
            CallProxy(il, typeof(IPropertyProxy).GetMethod(nameof(IPropertyProxy.SetPropertyValue)));
            il.Emit(OpCodes.Ret); //return
            TypeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
        }

        protected virtual void PushArguments(ILGenerator il, ParameterInfo[] args)
        {
            //create and push params array
            il.Emit(OpCodes.Ldc_I4, args.Length);
            il.Emit(OpCodes.Newarr, typeof(object));

            //Fill params array
            for (int i = 0; i < args.Length; ++i)
            {
                //add array entry
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i);

                //copy incoming arguments to the array
                il.Emit(OpCodes.Ldarg, 1 + i);

                //its object[] so we have to box value types
                if (args[i].ParameterType.IsValueType)
                    il.Emit(OpCodes.Box, args[i].ParameterType);

                //read last 3 values, fill (3) in (1) at postion (2)
                il.Emit(OpCodes.Stelem_Ref);
            }
        }

        private MethodBuilder GenerateMethodBuilder(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var methodBuilder = TypeBuilder.DefineMethod($"<{methodInfo.DeclaringType.Name}>{methodInfo.Name}",
                                                    MethodAttributes.Public | MethodAttributes.Virtual,
                                                    methodInfo.CallingConvention,
                                                    methodInfo.ReturnType,
                                                    parameters.Select(p => p.ParameterType).ToArray());
            return methodBuilder;
        }


        protected override void PushMemberInfo(ILGenerator il, MemberInfo memberInfo)
        {
            var pInfo = (PropertyInfo)memberInfo;
            il.Emit(OpCodes.Ldtoken, pInfo.DeclaringType);
            il.Emit(OpCodes.Ldstr, memberInfo.Name);
            il.Emit(OpCodes.Call, GetPropertyMethod);
        }
    
    }
}
