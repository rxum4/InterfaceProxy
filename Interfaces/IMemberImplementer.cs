using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AbstractInterfaceImplementation.Interfaces
{
    public interface IMemberImplementer
    {
        bool CanImplement(MemberInfo memberInfo);
        void Implement(MemberInfo memberInfo);
    }
}
