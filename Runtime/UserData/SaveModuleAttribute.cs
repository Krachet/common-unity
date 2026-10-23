using System;

namespace Com.Krackhet.Runtime.UserData
{
    /// <summary>
    /// Gives a <see cref="SaveModule"/> its storage key and reset scope. Modules are registered explicitly with
    /// <see cref="SaveService.Register{T}"/>. The attribute is not inherited: a derived module that replaces a
    /// built-in one is registered with <see cref="SaveService.Register{TBase,TDerived}"/> and keeps the base key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SaveModuleAttribute : Attribute
    {
        public string Key { get; }

        public ResetScope Scope { get; }

        public SaveModuleAttribute(string key, ResetScope scope = ResetScope.Never)
        {
            Key = key;
            Scope = scope;
        }

        internal static SaveModuleAttribute Read(Type type)
        {
            return (SaveModuleAttribute)GetCustomAttribute(type, typeof(SaveModuleAttribute), false);
        }
    }
}
