// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace System.Xml.Serialization
{
    internal sealed class ContextAwareTables<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> where T : class?
    {
        private readonly Hashtable _defaultTable;
        private readonly ConditionalWeakTable<Type, T> _collectibleTable;

        public ContextAwareTables()
        {
            _defaultTable = new Hashtable();
            _collectibleTable = new ConditionalWeakTable<Type, T>();
        }

        internal T GetOrCreateValue(Type t, Func<Type, T> f)
        {
            // The fast and most common default case
            T? ret = (T?)_defaultTable[t];
            if (ret != null)
                return ret;

            // Common case for collectible contexts
            if (_collectibleTable.TryGetValue(t, out ret))
                return ret;

            // Not found. Do the slower work of creating the value in the correct collection.
            AssemblyLoadContext? alc = LoadContextAwareTypeResolver.GetLoadContext(t);

            // Null and non-collectible load contexts use the default table
            if (alc == null || !alc.IsCollectible)
            {
                lock (_defaultTable)
                {
                    if ((ret = (T?)_defaultTable[t]) == null)
                    {
                        ret = f(t);
                        _defaultTable[t] = ret;
                    }
                }
            }

            // Collectible load contexts should use the ConditionalWeakTable so they can be unloaded
            else
            {
                lock (_collectibleTable)
                {
                    if (!_collectibleTable.TryGetValue(t, out ret))
                    {
                        ret = f(t);
                        _collectibleTable.AddOrUpdate(t, ret);
                    }
                }
            }

            return ret;
        }
    }

    internal static class LoadContextAwareTypeResolver
    {
        internal static Assembly? GetLoadContextAssembly(Type? type)
        {
            if (type is null)
            {
                return null;
            }

            Assembly assembly = type.Assembly;
            if (IsNonDefaultLoadContext(assembly))
            {
                return assembly;
            }

            if (type.HasElementType)
            {
                Assembly? elementAssembly = GetLoadContextAssembly(type.GetElementType());
                if (IsNonDefaultLoadContext(elementAssembly))
                {
                    return elementAssembly;
                }
            }

            if (type.IsConstructedGenericType)
            {
                foreach (Type genericArgument in type.GetGenericArguments())
                {
                    Assembly? argumentAssembly = GetLoadContextAssembly(genericArgument);
                    if (IsNonDefaultLoadContext(argumentAssembly))
                    {
                        return argumentAssembly;
                    }
                }
            }

            return assembly;
        }

        internal static AssemblyLoadContext? GetLoadContext(Type? type) =>
            GetLoadContext(GetLoadContextAssembly(type));

        internal static Type? GetRepresentativeType(Type?[]? types)
        {
            Type? fallback = null;
            if (types is null)
            {
                return null;
            }

            foreach (Type? type in types)
            {
                if (type is null)
                {
                    continue;
                }

                fallback ??= type;
                if (IsNonDefaultLoadContext(GetLoadContextAssembly(type)))
                {
                    return type;
                }
            }

            return fallback;
        }

        internal static Assembly? GetLoadContextAssembly(Type?[]? types) =>
            GetLoadContextAssembly(GetRepresentativeType(types));

        private static AssemblyLoadContext? GetLoadContext(Assembly? assembly) =>
            assembly is null ? null : AssemblyLoadContext.GetLoadContext(assembly);

        // Constructed wrapper types like List<T> are defined in CoreLib even when a generic
        // argument loaded in a custom ALC determines the effective load context.
        private static bool IsNonDefaultLoadContext(Assembly? assembly)
        {
            AssemblyLoadContext? alc = GetLoadContext(assembly);
            return alc != null && alc != AssemblyLoadContext.Default;
        }
    }
}
