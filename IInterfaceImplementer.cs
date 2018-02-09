namespace AbstractInterfaceImplementation
{
    public interface IInterfaceImplementer
    {
        TInterface CreateInterfaceInstance<TInterface>(params object[] ctorArgs);
    }
}