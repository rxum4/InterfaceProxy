using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation
{
    public interface IInterfaceProxy: IInvokationProxy,IPropertyProxy,IEventProxy  { }

    public interface IInvokationProxy
    {
        Exception InvokeFunction(MethodInfo methodInfo, object[] args, out object result);
        Exception InvokeAction(MethodInfo methodInfo, object[] args);
    }

    public interface IPropertyProxy
    {
        Exception GetPropertyValue(PropertyInfo propertyInfo, object[] args, out object result);
        Exception SetPropertyValue(PropertyInfo propertyInfo, object[] args);
    }

    public interface IEventProxy
    {
        Exception AddEventListener(string eventName, Type declaringType, Delegate listener);
        Exception RemoveEventListener(string eventName, Type declaringType, Delegate listener);
    }
}
