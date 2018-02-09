using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation
{
    public abstract class MemberImplementer
    {
        protected void PushInstance(ILGenerator ILGen, MethodBase methodInfo)
        {
            if (!methodInfo.IsStatic) //only do this if it is nessecary
                ILGen.Emit(OpCodes.Ldarg_0); //push [this]
        }

        protected abstract void PushMemberInfo(ILGenerator il, MemberInfo memberInfo);

        protected virtual void ReadProxyReturnValue(ILGenerator il, MethodInfo proxyMethod)
        {
            //declare and push result variable
            var resultObject = il.DeclareLocal(typeof(object)); 
            il.Emit(OpCodes.Ldloca_S, resultObject.LocalIndex);

            CallProxy(il, proxyMethod);

            //load value of out param
            il.Emit(OpCodes.Ldloc, resultObject);
        }

        protected virtual void CallProxy(ILGenerator il, MethodInfo proxyMethod)
        {
            Label returnLabel = il.DefineLabel();
            //declare exception variable
            var exObject = il.DeclareLocal(typeof(Exception));
            
            //perform proxy call
            il.Emit(OpCodes.Call, proxyMethod);
            il.Emit(OpCodes.Stloc, exObject); //assign result (temporary save)
            
            //check if IProxy result is null
            il.Emit(OpCodes.Ldloc, exObject);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            
            //if return == true goto returnLabel
            il.Emit(OpCodes.Brtrue_S, returnLabel);

            //(else) retrieve and throw exception
            il.Emit(OpCodes.Ldloc, exObject);
            il.Emit(OpCodes.Throw);
            il.MarkLabel(returnLabel);
        }

        protected void UnboxOrCast(ILGenerator il, Type type)
        {
            if (type.IsValueType || type.IsGenericParameter)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }
    }
}
