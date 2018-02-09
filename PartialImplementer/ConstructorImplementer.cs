using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation
{
    public class ConstructorImplementer : MemberImplementer, IConstructorImplementer
    {
        private static readonly MethodInfo GetMethodMethod = typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), new[] { typeof(RuntimeMethodHandle) });
        public TypeBuilder TypeBuilder { get; }
        public ConstructorImplementer(TypeBuilder builder)
        {
            TypeBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public void ImplementConstructor(ConstructorInfo ctor)
        {
            var Args = ctor.GetParameters();
            if (!Args.Any())
            {
                TypeBuilder.DefineDefaultConstructor(ctor.Attributes);
                return;
            }
            var ctorBuilder = TypeBuilder.DefineConstructor(ctor.Attributes,
                                                            ctor.CallingConvention,
                                                            Args.Select(p => p.ParameterType).ToArray());
            PerformImplementation(ctorBuilder, ctor);
        }

        private void PerformImplementation(ConstructorBuilder ctorBuilder, ConstructorInfo ctor)
        {
            var il = ctorBuilder.GetILGenerator();
            PushInstance(il, ctor);
            PushArguments(il, ctor.GetParameters());
            il.Emit(OpCodes.Call, ctor);
            //this nop,nop is default for vc# compiler... just copied it
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);
        }
        protected void PushArguments(ILGenerator il, ParameterInfo[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                il.Emit(OpCodes.Ldarg, 1 + i);
            }
        }
        protected override void PushMemberInfo(ILGenerator il, MemberInfo memberInfo)
        {
            throw new NotImplementedException();
        }
    }
}
