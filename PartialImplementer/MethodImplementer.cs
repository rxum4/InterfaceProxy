using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation
{
    public class MethodImplementer : MemberImplementer, IMethodImplementer
    {
        private static readonly MethodInfo GetMethodMethod = typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), new[] { typeof(RuntimeMethodHandle) });
        public TypeBuilder TypeBuilder { get; }
        public MethodImplementer(TypeBuilder builder)
        {
            TypeBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public void ImplementMethod(MethodInfo methodInfo)
        {
            MethodBuilder methodBuilder = GenerateMethodBuilder(methodInfo);
            PerformImplementation(methodBuilder.GetILGenerator(), methodInfo);
            TypeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
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

        private void PerformImplementation(ILGenerator il, MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            PushInstance(il, methodInfo);
            PushMemberInfo(il, methodInfo);
            PushArguments(il, parameters);
            CallMatchingProxy(il, methodInfo);
            il.Emit(OpCodes.Ret); //return
        }

        private void CallMatchingProxy(ILGenerator il, MethodInfo methodInfo)
        {
            if (methodInfo.ReturnType != typeof(void))
            {
                ReadProxyReturnValue(il, typeof(IInvokationProxy).GetMethod(nameof(IInvokationProxy.InvokeFunction)));
                UnboxOrCast(il, methodInfo.ReturnType); //cast to wanted type
            }
            else
            {
                CallProxy(il, typeof(IInvokationProxy).GetMethod(nameof(IInvokationProxy.InvokeAction)));
            }
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

        protected override void PushMemberInfo(ILGenerator il, MemberInfo memberInfo)
        {
            var methodInfo = (MethodInfo)memberInfo;
            il.Emit(OpCodes.Ldtoken, methodInfo);
            il.Emit(OpCodes.Call, GetMethodMethod);
            il.Emit(OpCodes.Castclass, typeof(MethodInfo));
        }
    }
}
