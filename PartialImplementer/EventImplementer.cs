using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation
{
    public class EventImplementer : MemberImplementer, IEventImplementer
    {
        private static readonly MethodInfo GetEventMethod = typeof(Type).GetMethod(nameof(Type.GetEvent), new[] { typeof(string) });
        public TypeBuilder TypeBuilder { get; }
        public EventImplementer(TypeBuilder builder)
        {
            TypeBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public void ImplementEvent(EventInfo eventInfo)
        {
            ImplementAddEventMethod(eventInfo, typeof(IEventProxy).GetMethod(nameof(IEventProxy.AddEventListener)));
            ImplementRemoveEventMethod(eventInfo, typeof(IEventProxy).GetMethod(nameof(IEventProxy.RemoveEventListener)));
        }
        private void ImplementAddEventMethod(EventInfo eventInfo, MethodInfo proxyMethod)
        {
            var methodInfo = eventInfo.AddMethod;
            var parameters = methodInfo.GetParameters();
            var methodBuilder = TypeBuilder.DefineMethod($"<{methodInfo.DeclaringType.Name}>{methodInfo.Name}",
                                                    MethodAttributes.Public | MethodAttributes.Virtual,
                                                    methodInfo.CallingConvention,
                                                    methodInfo.ReturnType,
                                                    parameters.Select(p => p.ParameterType).ToArray());
            var il = methodBuilder.GetILGenerator();
            PushInstance(il, methodInfo);
            PushMemberInfo(il, eventInfo);
            il.Emit(OpCodes.Ldarg_1);
            CallProxy(il, proxyMethod);
            il.Emit(OpCodes.Ret); //return
            TypeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
        }
        private void ImplementRemoveEventMethod(EventInfo eventInfo, MethodInfo proxyMethod)
        {
            var methodInfo = eventInfo.RemoveMethod;
            var parameters = methodInfo.GetParameters();
            var methodBuilder = TypeBuilder.DefineMethod($"<{methodInfo.DeclaringType.Name}>{methodInfo.Name}",
                                                    MethodAttributes.Public | MethodAttributes.Virtual,
                                                    methodInfo.CallingConvention,
                                                    methodInfo.ReturnType,
                                                    parameters.Select(p => p.ParameterType).ToArray());
            var il = methodBuilder.GetILGenerator();
            PushInstance(il, methodInfo);
            PushMemberInfo(il, eventInfo);
            il.Emit(OpCodes.Ldarg_1);
            CallProxy(il, proxyMethod);
            il.Emit(OpCodes.Ret); //return
            TypeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
        }
        

        protected override void PushMemberInfo(ILGenerator il, MemberInfo memberInfo)
        {
            var eInfo = (EventInfo)memberInfo;
            var type = eInfo.DeclaringType;
            il.Emit(OpCodes.Ldstr, memberInfo.Name);
            il.Emit(OpCodes.Ldtoken, type);
        }
    }
}
