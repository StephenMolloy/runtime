// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if MULTIMODULE_BUILD && !DEBUG
// Some tests won't work if we're using optimizing codegen, but scanner doesn't run.
// This currently happens in optimized multi-obj builds.
#define OPTIMIZED_MODE_WITHOUT_SCANNER
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Reflection;

[assembly: TestAssembly]
[module: TestModule]

internal static class ReflectionTest
{
    private static int Main()
    {
        // Things I would like to test, but we don't fully support yet:
        // * Interface method is reflectable if we statically called it through a constrained call
        // * Delegate Invoke method is reflectable if we statically called it

        //
        // Tests for dependency graph in the compiler
        //
        TestSimpleDelegateTargets.Run();
        TestVirtualDelegateTargets.Run();
        TestRunClassConstructor.Run();
        TestFieldMetadata.Run();
        TestLinqInvocation.Run();
        TestGenericMethodsHaveSameReflectability.Run();
#if !OPTIMIZED_MODE_WITHOUT_SCANNER
        TestContainment.Run();
        TestInterfaceMethod.Run();
        TestByRefLikeTypeMethod.Run();
#endif
        TestILScanner.Run();
        TestTypeGetType.Run();
        TestUnreferencedEnum.Run();
        TestTypesInMethodSignatures.Run();

        TestAttributeInheritance.Run();
        Test113750Regression.Run();
        TestStringConstructor.Run();
        TestAssemblyAndModuleAttributes.Run();
        TestAttributeExpressions.Run();
        TestParameterAttributes.Run();
        TestPropertyAndEventAttributes.Run();
        TestNecessaryEETypeReflection.Run();
        TestRuntimeLab929Regression.Run();
        CodelessMethodMetadataTest.Run();
#if !REFLECTION_FROM_USAGE
        TestNotReflectedIsNotReflectable.Run();
        TestGenericInstantiationsAreEquallyReflectable.Run();
        TestStackTraces.Run();
#endif
        TestAttributeInheritance2.Run();
        TestInvokeMethodMetadata.Run();
        TestVTableOfNullableUnderlyingTypes.Run();
        TestInterfaceLists.Run();
        TestMethodConsistency.Run();
        TestGenericMethodOnGenericType.Run();
        TestIsValueTypeWithoutTypeHandle.Run();
        TestMdArrayLoad.Run();
        TestMdArrayLoad2.Run();
        TestByRefTypeLoad.Run();
        TestGenericLdtoken.Run();
        TestAbstractGenericLdtoken.Run();
        TestTypeHandlesVisibleFromIDynamicInterfaceCastable.Run();
        TestCompilerGeneratedCode.Run();
        Test105034Regression.Run();
        TestMethodsNeededFromNativeLayout.Run();
        TestFieldAndParamMetadata.Run();

        //
        // Mostly functionality tests
        //
        TestCreateDelegate.Run();
        TestGetUninitializedObject.Run();
        TestInstanceFields.Run();
        TestReflectionInvoke.Run();
        TestConstructors.Run();
        TestInvokeMemberParamsCornerCase.Run();
        TestDefaultInterfaceInvoke.Run();
        TestCovariantReturnInvoke.Run();
        TypeConstructionTest.Run();
        TestThreadStaticFields.Run();
        TestByRefReturnInvoke.Run();
        TestAssemblyLoad.Run();
        TestBaseOnlyUsedFromCode.Run();
        TestEntryPoint.Run();
        TestGenericAttributesOnEnum.Run();
        TestLdtokenWithSignaturesDifferingInModifiers.Run();
        TestActivatingThingsInSignature.Run();
        TestDelegateInvokeFromEvent.Run();

        return 100;
    }

    class TestReflectionInvoke
    {
        internal class InvokeTests
        {
            private string _world = "world";

            public InvokeTests() { }

            public InvokeTests(string message) { _world = message; }

#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
#endif
            public static string GetHello(string name)
            {
                return "Hello " + name;
            }

#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
#endif
            public static void GetHelloByRef(string name, out string result)
            {
                result = "Hello " + name;
            }

#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
#endif
            public static string GetHelloGeneric<T>(T obj)
            {
                return "Hello " + obj;
            }


#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
#endif
            public string GetHelloInstance()
            {
                return "Hello " + _world;
            }

#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
#endif
            public static unsafe string GetHelloPointer(char* ptr)
            {
                return "Hello " + unchecked((int)ptr);
            }

#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
#endif
            public static unsafe string GetHelloPointerToo(char** ptr)
            {
                return "Hello " + unchecked((int)ptr);
            }

#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
#endif
            public static unsafe bool* GetPointer(void* ptr, object dummyJustToMakeThisUseSharedThunk)
            {
                return (bool*)ptr;
            }

        }

        internal class InvokeTestsGeneric<T>
        {
            private string _hi = "Hello ";

#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
#endif
            public string GetHelloGeneric<U>(U obj)
            {
                return _hi + obj + " " + typeof(U);
            }

#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
#endif
            public string GetHello(object obj)
            {
                return _hi + obj + " " + typeof(T);
            }
        }

        public static unsafe void Run()
        {
            Console.WriteLine(nameof(TestReflectionInvoke));

            // Ensure things we reflect on are in the static callgraph
            if (string.Empty.Length > 0)
            {
                InvokeTests.GetHelloGeneric<int>(0);
                new InvokeTestsGeneric<int>().GetHelloGeneric<double>(0);
            }

            {
                object? arg = "world";
                MethodInfo helloMethod = typeof(InvokeTests).GetTypeInfo().GetDeclaredMethod("GetHello");

                string result = (string)helloMethod.Invoke(null, new object[] { arg });
                if (result != "Hello world")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloMethod).Invoke(null, arg);
                if (result != "Hello world")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloMethod).Invoke(null, new Span<object?>(ref arg));
                if (result != "Hello world")
                    throw new Exception();
            }

            {
                object? arg = 12345;
                MethodInfo helloGenericMethod = typeof(InvokeTests).GetTypeInfo().GetDeclaredMethod("GetHelloGeneric").MakeGenericMethod(typeof(int));
                string result = (string)helloGenericMethod.Invoke(null, new object[] { arg });
                if (result != "Hello 12345")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloGenericMethod).Invoke(null, arg);
                if (result != "Hello 12345")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloGenericMethod).Invoke(null, new Span<object?>(ref arg));
                if (result != "Hello 12345")
                    throw new Exception();
            }

            {
                object? arg = "buddy";
                MethodInfo helloGenericMethod = typeof(InvokeTests).GetTypeInfo().GetDeclaredMethod("GetHelloGeneric").MakeGenericMethod(typeof(string));
                string result = (string)helloGenericMethod.Invoke(null, new object[] { arg });
                if (result != "Hello buddy")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloGenericMethod).Invoke(null, arg);
                if (result != "Hello buddy")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloGenericMethod).Invoke(null, new Span<object?>(ref arg));
                if (result != "Hello buddy")
                    throw new Exception();
            }

            {
                object? arg = typeof(string);
                MethodInfo helloGenericMethod = typeof(InvokeTests).GetTypeInfo().GetDeclaredMethod("GetHelloGeneric").MakeGenericMethod(typeof(Type));
                string result = (string)helloGenericMethod.Invoke(null, new object[] { arg });
                if (result != "Hello System.String")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloGenericMethod).Invoke(null, arg);
                if (result != "Hello System.String")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloGenericMethod).Invoke(null, new Span<object?>(ref arg));
                if (result != "Hello System.String")
                    throw new Exception();
            }

            {
                object? arg = "world";
                MethodInfo helloByRefMethod = typeof(InvokeTests).GetTypeInfo().GetDeclaredMethod("GetHelloByRef");
                object[] args = new object[] { arg, null };

                helloByRefMethod.Invoke(null, args);
                if ((string)args[1] != "Hello world")
                    throw new Exception();

                args = new object[] { arg, null };
                MethodInvoker.Create(helloByRefMethod).Invoke(null, new Span<object?>(args));
                if ((string)args[1] != "Hello world")
                    throw new Exception();
            }

            {
                MethodInfo helloPointerMethod = typeof(InvokeTests).GetTypeInfo().GetDeclaredMethod("GetHelloPointer");

                string resultNull = (string)helloPointerMethod.Invoke(null, new object[] { null });
                if (resultNull != "Hello 0")
                    throw new Exception();

                resultNull = (string)MethodInvoker.Create(helloPointerMethod).Invoke(null, arg1: null);
                if (resultNull != "Hello 0")
                    throw new Exception();

                object? arg = null;
                resultNull = (string)MethodInvoker.Create(helloPointerMethod).Invoke(null, new Span<object?>(ref arg));
                if (resultNull != "Hello 0")
                    throw new Exception();

                arg = Pointer.Box((void*)42, typeof(char*));
                string resultVal = (string)helloPointerMethod.Invoke(null, new object[] { arg });
                if (resultVal != "Hello 42")
                    throw new Exception();

                resultNull = (string)MethodInvoker.Create(helloPointerMethod).Invoke(null, arg);
                if (resultVal != "Hello 42")
                    throw new Exception();

                resultNull = (string)MethodInvoker.Create(helloPointerMethod).Invoke(null, new Span<object?>(ref arg));
                if (resultVal != "Hello 42")
                    throw new Exception();
            }

            {
                MethodInfo helloPointerTooMethod = typeof(InvokeTests).GetTypeInfo().GetDeclaredMethod("GetHelloPointerToo");
                object? arg = Pointer.Box((void*)85, typeof(char**));

                string result = (string)helloPointerTooMethod.Invoke(null, new object[] { arg });
                if (result != "Hello 85")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloPointerTooMethod).Invoke(null, arg);
                if (result != "Hello 85")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloPointerTooMethod).Invoke(null, new Span<object?>(ref arg));
                if (result != "Hello 85")
                    throw new Exception();
            }

            {
                MethodInfo getPointerMethod = typeof(InvokeTests).GetTypeInfo().GetDeclaredMethod("GetPointer");
                object? arg = Pointer.Box((void*)2018, typeof(void*));
                object[] args = new object[] { arg, null };

                object result = getPointerMethod.Invoke(null, args);
                if (Pointer.Unbox(result) != (void*)2018)
                    throw new Exception();

                result = MethodInvoker.Create(getPointerMethod).Invoke(null, arg, null);
                if (Pointer.Unbox(result) != (void*)2018)
                    throw new Exception();

                result = MethodInvoker.Create(getPointerMethod).Invoke(null, new Span<object?>(args));
                if (Pointer.Unbox(result) != (void*)2018)
                    throw new Exception();
            }

            {
                MethodInfo helloMethod = typeof(InvokeTestsGeneric<string>).GetTypeInfo().GetDeclaredMethod("GetHello");
                object? arg = "world";

                string result = (string)helloMethod.Invoke(new InvokeTestsGeneric<string>(), new object[] { arg });
                if (result != "Hello world System.String")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloMethod).Invoke(new InvokeTestsGeneric<string>(), arg);
                if (result != "Hello world System.String")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloMethod).Invoke(new InvokeTestsGeneric<string>(), new Span<object?>(ref arg));
                if (result != "Hello world System.String")
                    throw new Exception();
            }

            {
                MethodInfo helloGenericMethod = typeof(InvokeTestsGeneric<string>).GetTypeInfo().GetDeclaredMethod("GetHelloGeneric").MakeGenericMethod(typeof(object));
                object? arg = "world";

                string result = (string)helloGenericMethod.Invoke(new InvokeTestsGeneric<string>(), new object[] { arg });
                if (result != "Hello world System.Object")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloGenericMethod).Invoke(new InvokeTestsGeneric<string>(), arg);
                if (result != "Hello world System.Object")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloGenericMethod).Invoke(new InvokeTestsGeneric<string>(), new Span<object?>(ref arg));
                if (result != "Hello world System.Object")
                    throw new Exception();
            }

            {
                MethodInfo helloMethod = typeof(InvokeTestsGeneric<int>).GetTypeInfo().GetDeclaredMethod("GetHello");
                object? arg = "world";

                string result = (string)helloMethod.Invoke(new InvokeTestsGeneric<int>(), new object[] { arg });
                if (result != "Hello world System.Int32")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloMethod).Invoke(new InvokeTestsGeneric<int>(), arg);
                if (result != "Hello world System.Int32")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloMethod).Invoke(new InvokeTestsGeneric<int>(), new Span<object?>(ref arg));
                if (result != "Hello world System.Int32")
                    throw new Exception();
            }

            {
                MethodInfo helloGenericMethod = typeof(InvokeTestsGeneric<int>).GetTypeInfo().GetDeclaredMethod("GetHelloGeneric").MakeGenericMethod(typeof(double));
                object? arg = 1.0;

                string result = (string)helloGenericMethod.Invoke(new InvokeTestsGeneric<int>(), new object[] { arg });
                if (result != "Hello 1 System.Double")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloGenericMethod).Invoke(new InvokeTestsGeneric<int>(), arg);
                if (result != "Hello 1 System.Double")
                    throw new Exception();

                result = (string)MethodInvoker.Create(helloGenericMethod).Invoke(new InvokeTestsGeneric<int>(), new Span<object?>(ref arg));
                if (result != "Hello 1 System.Double")
                    throw new Exception();
            }
        }
    }

    class TestInvokeMemberParamsCornerCase
    {
        public struct MyStruct { }

        public static int Count(params MyStruct[] myStructs)
        {
            return myStructs.Length;
        }

        public static void Run()
        {
            Console.WriteLine(nameof(TestInvokeMemberParamsCornerCase));

            // Needs MethodTable for MyStruct[] and the compiler should have created it.
            typeof(TestInvokeMemberParamsCornerCase).InvokeMember(nameof(Count),
                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                null, null, new object[] { default(MyStruct) });
        }
    }

    class TestDefaultInterfaceInvoke
    {
        interface IFoo<T>
        {
            string Format(string s) => "IFoo<" + typeof(T) + ">::Format(" + s + ")";
            sealed string InstanceMethod(string s) => "IFoo<" + typeof(T) + ">::InstanceMethod(" + s + ")";
        }

        interface IFoo
        {
            string Format(string s) => "IFoo::Format(" + s + ")";
            sealed string InstanceMethod(string s) => "IFoo::InstanceMethod(" + s + ")";
        }

        interface IBar : IFoo
        {
            string IFoo.Format(string s) => "IBar::Format(" + s + ")";
        }

        class Foo : IFoo<string>, IFoo<object>, IFoo<int>, IFoo<Enum>, IBar
        {
            string IFoo<Enum>.Format(string s) => "Foo.IFoo<Enum>::Format(" + s + ")";
        }

        public static void Run()
        {
            Console.WriteLine(nameof(TestDefaultInterfaceInvoke));

            {
                var result = (string)typeof(IFoo<string>).GetMethod(nameof(IFoo<int>.Format)).Invoke(new Foo(), new object[] { "abc" });
                if (result != "IFoo<System.String>::Format(abc)")
                    throw new Exception();
            }

            {
                var result = (string)typeof(IFoo<object>).GetMethod(nameof(IFoo<int>.Format)).Invoke(new Foo(), new object[] { "abc" });
                if (result != "IFoo<System.Object>::Format(abc)")
                    throw new Exception();
            }

            {
                var result = (string)typeof(IFoo<int>).GetMethod(nameof(IFoo<int>.Format)).Invoke(new Foo(), new object[] { "abc" });
                if (result != "IFoo<System.Int32>::Format(abc)")
                    throw new Exception();
            }

            {
                var result = (string)typeof(IFoo<Enum>).GetMethod(nameof(IFoo<int>.Format)).Invoke(new Foo(), new object[] { "abc" });
                if (result != "Foo.IFoo<Enum>::Format(abc)")
                    throw new Exception();
            }

            {
                var result = (string)typeof(IFoo).GetMethod(nameof(IFoo.Format)).Invoke(new Foo(), new object[] { "abc" });
                if (result != "IBar::Format(abc)")
                    throw new Exception();
            }

            {
                var result = (string)typeof(IFoo).GetMethod(nameof(IFoo.InstanceMethod)).Invoke(new Foo(), new object[] { "abc" });
                if (result != "IFoo::InstanceMethod(abc)")
                    throw new Exception();
            }

            {
                var result = (string)typeof(IFoo<Enum>).GetMethod(nameof(IFoo<Enum>.InstanceMethod)).Invoke(new Foo(), new object[] { "abc" });
                if (result != "IFoo<System.Enum>::InstanceMethod(abc)")
                    throw new Exception();
            }
        }
    }

    class TestCovariantReturnInvoke
    {
        interface IFoo
        {
        }
        class Foo : IFoo
        {
            public readonly string State;
            public Foo(string state) => State = state;
        }
        class Base
        {
            public virtual IFoo GetFoo() => throw new NotImplementedException();
        }
        class Derived : Base
        {
            public override Foo GetFoo() => new Foo("Derived");
        }
        class SuperDerived : Derived
        {
            public override Foo GetFoo() => new Foo("SuperDerived");
        }

        public static void Run()
        {
            Console.WriteLine(nameof(TestCovariantReturnInvoke));

            MethodInfo mi = typeof(Base).GetMethod(nameof(Base.GetFoo));

            if (((Foo)mi.Invoke(new Derived(), Array.Empty<object>())).State != "Derived")
                throw new Exception();

            if (((Foo)mi.Invoke(new SuperDerived(), Array.Empty<object>())).State != "SuperDerived")
                throw new Exception();
        }
    }

    class TestInstanceFields
    {
        public class FieldInvokeSample
        {
            public String InstanceField;
        }

        public class GenericFieldInvokeSample<T>
        {
            public int IntField;
            public T TField;
        }

        private static void TestGenerics<T>(T value)
        {
            TypeInfo ti = typeof(GenericFieldInvokeSample<T>).GetTypeInfo();

            var obj = new GenericFieldInvokeSample<T>();

            FieldInfo intField = ti.GetDeclaredField("IntField");
            obj.IntField = 1234;
            if ((int)(intField.GetValue(obj)) != 1234)
                throw new Exception();

            FieldInfo tField = ti.GetDeclaredField("TField");
            obj.TField = value;
            if (!tField.GetValue(obj).Equals(value))
                throw new Exception();
        }

        public static void Run()
        {
            Console.WriteLine(nameof(TestInstanceFields));

            TypeInfo ti = typeof(FieldInvokeSample).GetTypeInfo();

            FieldInfo instanceField = ti.GetDeclaredField("InstanceField");
            FieldInvokeSample obj = new FieldInvokeSample();

            String value = (String)(instanceField.GetValue(obj));
            if (value != null)
                throw new Exception();

            obj.InstanceField = "Hi!";
            value = (String)(instanceField.GetValue(obj));
            if (value != "Hi!")
                throw new Exception();

            instanceField.SetValue(obj, "Bye!");
            if (obj.InstanceField != "Bye!")
                throw new Exception();

            value = (String)(instanceField.GetValue(obj));
            if (value != "Bye!")
                throw new Exception();

            TestGenerics(new object());
            TestGenerics("Hi");
        }
    }

    unsafe class TestThreadStaticFields
    {
        class Generic<T>
        {
            [ThreadStatic]
            public static int ThreadStaticValueType;

            [ThreadStatic]
            public static object ThreadStaticReferenceType;

            [ThreadStatic]
            public static int* ThreadStaticPointerType;
        }

        class NonGeneric
        {
            [ThreadStatic]
            public static int ThreadStaticValueType;

            [ThreadStatic]
            public static object ThreadStaticReferenceType;

            [ThreadStatic]
            public static int* ThreadStaticPointerType;
        }

        static void TestGeneric<T>()
        {
            var refType = new object();

            Generic<T>.ThreadStaticValueType = 123;
            Generic<T>.ThreadStaticReferenceType = refType;
            Generic<T>.ThreadStaticPointerType = (int*)456;

            {
                var fd = typeof(Generic<T>).GetField(nameof(Generic<T>.ThreadStaticValueType));
                var val = (int)fd.GetValue(null);
                if (val != 123)
                    throw new Exception();
                fd.SetValue(null, 234);
                if (Generic<T>.ThreadStaticValueType != 234)
                    throw new Exception();
            }

            {
                var fd = typeof(Generic<T>).GetField(nameof(Generic<T>.ThreadStaticReferenceType));
                var val = fd.GetValue(null);
                if (val != refType)
                    throw new Exception();
                val = new object();
                fd.SetValue(null, val);
                if (Generic<T>.ThreadStaticReferenceType != val)
                    throw new Exception();
            }

            {
                var fd = typeof(Generic<T>).GetField(nameof(Generic<T>.ThreadStaticPointerType));
                var val = Pointer.Unbox(fd.GetValue(null));
                if (val != (int*)456)
                    throw new Exception();
                fd.SetValue(null, Pointer.Box((void*)678, typeof(int*)));
                if (Generic<T>.ThreadStaticPointerType != (void*)678)
                    throw new Exception();
            }
        }

        public static void Run()
        {
            Console.WriteLine(nameof(TestThreadStaticFields));

            var refType = new object();

            NonGeneric.ThreadStaticValueType = 123;
            NonGeneric.ThreadStaticReferenceType = refType;
            NonGeneric.ThreadStaticPointerType = (int*)456;

            {
                var fd = typeof(NonGeneric).GetField(nameof(NonGeneric.ThreadStaticValueType));
                var val = (int)fd.GetValue(null);
                if (val != 123)
                    throw new Exception();
                fd.SetValue(null, 234);
                if (NonGeneric.ThreadStaticValueType != 234)
                    throw new Exception();
            }

            {
                var fd = typeof(NonGeneric).GetField(nameof(NonGeneric.ThreadStaticReferenceType));
                var val = fd.GetValue(null);
                if (val != refType)
                    throw new Exception();
                val = new object();
                fd.SetValue(null, val);
                if (NonGeneric.ThreadStaticReferenceType != val)
                    throw new Exception();
            }

            {
                var fd = typeof(NonGeneric).GetField(nameof(NonGeneric.ThreadStaticPointerType));
                var val = Pointer.Unbox(fd.GetValue(null));
                if (val != (int*)456)
                    throw new Exception();
                fd.SetValue(null, Pointer.Box((void*)678, typeof(int*)));
                if (NonGeneric.ThreadStaticPointerType != (void*)678)
                    throw new Exception();
            }

            TestGeneric<string>();

            TestGeneric<int>();
        }
    }

    class Test105034Regression
    {
        interface IFactory
        {
            object Make();
        }

        interface IOption<T> where T : new() { }

        class OptionFactory<T> : IFactory where T : class, new()
        {
            public object Make() => new T();
        }

        class Gen<T> { }

        struct Atom { }

        static Type Register<T>() => typeof(T).GetGenericArguments()[0];
        static IFactory Activate(Type t) => (IFactory)Activator.CreateInstance(typeof(OptionFactory<>).MakeGenericType(t));

        public static void Run()
        {
            Console.WriteLine(nameof(Test105034Regression));

            Wrap<Atom>();

            static void Wrap<T>()
            {

                Type t = Register();
                static Type Register() => Register<IOption<Gen<T>>>();

                var f = Activate(t);
                f.Make();
            }
        }
    }

    class TestMethodsNeededFromNativeLayout
    {
        class MyAttribute : Attribute;

        class GenericClass<T> where T : class
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            [My]
            public static void GenericMethod<U>([My] string namedParameter = "Hello") { }

            public GenericClass() => GenericMethod<T>(null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static Type GetObjectType() => typeof(object);

        public static void Run()
        {
            // This tests that limited reflection metadata (that was only needed for native layout)
            // works within the reflection stack.
            Activator.CreateInstance(typeof(GenericClass<>).MakeGenericType(GetObjectType()));

            // This should succeed because of the Activator
            Type testType = GetTestType(nameof(TestMethodsNeededFromNativeLayout), "GenericClass`1");

            // This should succeed because native layout forces the metadata.
            // If this ever starts breaking, replace this pattern with something else that forces limited method metadata.
            MethodInfo mi = testType.GetMethod(nameof(GenericClass<object>.GenericMethod));

            // We got a MethodInfo that is limited, check the reflection APIs work fine with it
            if (mi.Name != nameof(GenericClass<object>.GenericMethod))
                throw new Exception("Name");

            // Unless we're doing REFLECTION_FROM_USAGE, we don't expect to see attributes
#if !REFLECTION_FROM_USAGE
            if (mi.GetCustomAttributes(inherit: true).Length != 0)
                throw new Exception("Attributes");
#endif

            // Unless we're doing REFLECTION_FROM_USAGE, we don't expect to be able to reflection-invoke
            var mi2 = (MethodInfo)typeof(GenericClass<string>).GetMemberWithSameMetadataDefinitionAs(mi);
#if !REFLECTION_FROM_USAGE
            try
#endif
            {

                mi2.MakeGenericMethod(typeof(string)).Invoke(null, [ null ]);
#if !REFLECTION_FROM_USAGE
                throw new Exception("Invoke");
#endif
            }
#if !REFLECTION_FROM_USAGE
            catch (NotSupportedException)
            {
            }
#endif

            // Parameter count should match no matter what
            var parameters = mi.GetParameters();
            if (parameters.Length != 1)
                throw new Exception("ParamCount");

            // But parameter names, default values, attributes should only work in REFLECTION_FROM_USAGE
#if !REFLECTION_FROM_USAGE
            if (parameters[0].Name != null)
                throw new Exception("ParamName");

            if (parameters[0].HasDefaultValue)
                throw new Exception("DefaultValue");

            if (parameters[0].GetCustomAttributes(inherit: true).Length != 0)
                throw new Exception("Attributes");
#endif
        }
    }

    class TestFieldAndParamMetadata
    {
        public class FieldType;

        public FieldType TheField;

        public class ParameterType;

        public static void TheMethod(ParameterType p) { }

        public static void Run()
        {
            Type fieldType = typeof(TestFieldAndParamMetadata).GetField(nameof(TheField)).FieldType;

            if (fieldType.Name != nameof(FieldType))
                throw new Exception();

            Type parameterType = typeof(TestFieldAndParamMetadata).GetMethod(nameof(TheMethod)).GetParameters()[0].ParameterType;
            if (parameterType.Name != nameof(ParameterType))
                throw new Exception();
        }
    }

    class TestCreateDelegate
    {
        internal class Greeter
        {
            private string _who;

            public Greeter(string who) { _who = who; }

#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
#endif
            public string Greet()
            {
                return "Hello " + _who;
            }
        }

        delegate string GetHelloInstanceDelegate(Greeter o);

        public static void Run()
        {
            Console.WriteLine(nameof(TestCreateDelegate));

            TypeInfo ti = typeof(Greeter).GetTypeInfo();
            MethodInfo mi = ti.GetDeclaredMethod(nameof(Greeter.Greet));
            {
                var d = (GetHelloInstanceDelegate)mi.CreateDelegate(typeof(GetHelloInstanceDelegate));
                if (d(new Greeter("mom")) != "Hello mom")
                    throw new Exception();
            }

            {
                var d = (Func<Greeter, string>)mi.CreateDelegate(typeof(Func<Greeter, string>));
                if (d(new Greeter("pop")) != "Hello pop")
                    throw new Exception();
            }
        }
    }

    class TestGetUninitializedObject
    {
        struct NeverAllocated
        {
            public override int GetHashCode() => 800;
        }

        struct AlsoNeverAllocated
        {
            public override int GetHashCode() => 500;
        }

        class NeverAllocatedButUsedInGenericMethod<T>
        {
        }

        class Atom;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Type GetNeverAllocatedButUsedInGenericMethod() => typeof(NeverAllocatedButUsedInGenericMethod<>);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static object GenericMethod<T>() => null;

        public static void Run()
        {
            Console.WriteLine(nameof(TestGetUninitializedObject));

            // Check that the vtable of a type passed to GetUninitializedObject
            // as a Nullable is intact.
            var obj1 = RuntimeHelpers.GetUninitializedObject(typeof(NeverAllocated?));
            if (obj1.GetHashCode() != 800)
                throw new Exception();

            // Check that the vtable of a type passed to GetUninitializedObject is intact.
            var obj2 = RuntimeHelpers.GetUninitializedObject(typeof(AlsoNeverAllocated));
            if (obj2.GetHashCode() != 500)
                throw new Exception();

            // Do what's needed so that we force an unconstructed MT for NeverAllocatedButUsedInGenericMethod<Atom> into the program
            // 1. Statically call the method
            // 2. Make the method visible target of reflection
            // This will force the compiler to place the method generic dictionary into a hashtable addressable using the instantiation.
            GenericMethod<NeverAllocatedButUsedInGenericMethod<Atom>>();
            typeof(TestGetUninitializedObject).GetMethod(nameof(GenericMethod));

            Type t1 = GetNeverAllocatedButUsedInGenericMethod().MakeGenericType(typeof(Atom));
            _ = t1.TypeHandle; // Type handle is only suitable for casting but we can get it

            bool thrown = true;
            try
            {
                // Needs to throw, the MT is only a necessary MT, not constructed MT
                RuntimeHelpers.GetUninitializedObject(t1);
                thrown = false;
            }
            catch (NotSupportedException e)
            {
                if (!e.Message.Contains("ReflectionTest+TestGetUninitializedObject+NeverAllocatedButUsedInGenericMethod`1[ReflectionTest+TestGetUninitializedObject+Atom]"))
                    throw new Exception();
            }
            if (!thrown)
                throw new Exception();
        }
    }

    class TestParameterAttributes
    {
#if OPTIMIZED_MODE_WITHOUT_SCANNER
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
#endif
        public static bool Method([Parameter] ParameterType parameter)
        {
            return parameter == null;
        }

        public class ParameterType { }

        class ParameterAttribute : Attribute
        {
            public ParameterAttribute([CallerMemberName] string memberName = null)
            {
                MemberName = memberName;
            }

            public string MemberName { get; }
        }

        public static void Run()
        {
            Console.WriteLine(nameof(TestParameterAttributes));

            MethodInfo method = typeof(TestParameterAttributes).GetMethod(nameof(Method));

            var attribute = method.GetParameters()[0].GetCustomAttribute<ParameterAttribute>();
            if (attribute.MemberName != nameof(Method))
                throw new Exception();
        }
    }

    class TestPropertyAndEventAttributes
    {
        [Property("MyProperty")]
        public static int Property
        {
#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
            get;
#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
            set;
        }

        class PropertyAttribute : Attribute
        {
            public PropertyAttribute(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        [Event("MyEvent")]
        public static event Action Event
        {
#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
            add { }
#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
            remove { }
        }

        class EventAttribute : Attribute
        {
            public EventAttribute(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        public static void Run()
        {
            Console.WriteLine(nameof(TestPropertyAndEventAttributes));

            {
                PropertyInfo property = typeof(TestPropertyAndEventAttributes).GetProperty(nameof(Property));
                var attribute = property.GetCustomAttribute<PropertyAttribute>();
                if (attribute.Value != "MyProperty")
                    throw new Exception();
            }

            {
                EventInfo @event = typeof(TestPropertyAndEventAttributes).GetEvent(nameof(Event));
                var attribute = @event.GetCustomAttribute<EventAttribute>();
                if (attribute.Value != "MyEvent")
                    throw new Exception();
            }
        }
    }

    class TestAttributeExpressions
    {
        struct FirstNeverUsedType { }

        struct SecondNeverUsedType { }

        struct ThirdNeverUsedType { }

        class Gen<T> { }

        class TypeAttribute : Attribute
        {
            public Type SomeType { get; set; }

            public TypeAttribute() { }
            public TypeAttribute(Type someType)
            {
                SomeType = someType;
            }
        }

        enum MyEnum { }

        class EnumArrayAttribute : Attribute
        {
            public MyEnum[] EnumArray;
        }

        [Type(typeof(FirstNeverUsedType*[,]))]
        class Holder1 { }

        [Type(SomeType = typeof(Gen<SecondNeverUsedType>))]
        class Holder2 { }

        [EnumArray(EnumArray = new MyEnum[] { 0 })]
        class Holder3 { }

        [Type(SomeType = typeof(ThirdNeverUsedType))]
        class Holder4 { }

        public static void Run()
        {
            Console.WriteLine(nameof(TestAttributeExpressions));

            const string attr1expected = "ReflectionTest+TestAttributeExpressions+FirstNeverUsedType*[,]";
            TypeAttribute attr1 = typeof(Holder1).GetCustomAttribute<TypeAttribute>();
            if (attr1.SomeType.ToString() != attr1expected)
                throw new Exception();

            // We don't expect to have an EEType because a mention of a type in the custom attribute
            // blob is not a sufficient condition to create EETypes.
            string exceptionString1 = "";
            try
            {
                _ = attr1.SomeType.TypeHandle;
            }
            catch (Exception ex)
            {
                exceptionString1 = ex.Message;
            }
            if (!exceptionString1.Contains(attr1expected))
                throw new Exception(exceptionString1);

            const string attr2expected = "ReflectionTest+TestAttributeExpressions+Gen`1[ReflectionTest+TestAttributeExpressions+SecondNeverUsedType]";
            TypeAttribute attr2 = typeof(Holder2).GetCustomAttribute<TypeAttribute>();
            if (attr2.SomeType.ToString() != attr2expected)
                throw new Exception();

            // We don't expect to have an EEType because a mention of a type in the custom attribute
            // blob is not a sufficient condition to create EETypes.
            string exceptionString2 = "";
            try
            {
                _ = attr2.SomeType.TypeHandle;
            }
            catch (Exception ex)
            {
                exceptionString2 = ex.Message;
            }
            if (!exceptionString2.Contains(attr2expected))
                throw new Exception(exceptionString2);

            // Make sure we created EEType for the enum array.

            EnumArrayAttribute attr3 = typeof(Holder3).GetCustomAttribute<EnumArrayAttribute>();
            if (attr3.EnumArray[0] != 0)
                throw new Exception();

            TypeAttribute attr4 = typeof(Holder4).GetCustomAttribute<TypeAttribute>();
            if (attr4.SomeType.ToString() != "ReflectionTest+TestAttributeExpressions+ThirdNeverUsedType")
                throw new Exception();
        }
    }

    class TestAssemblyAndModuleAttributes
    {
        public static void Run()
        {
            Console.WriteLine(nameof(TestAssemblyAndModuleAttributes));

            // Also tests GetExecutingAssembly
            var assAttr = Assembly.GetExecutingAssembly().GetCustomAttribute<TestAssemblyAttribute>();
            if (assAttr == null)
                throw new Exception();

            // Also tests GetEntryAssembly
            var modAttr = Assembly.GetEntryAssembly().ManifestModule.GetCustomAttribute<TestModuleAttribute>();
            if (modAttr == null)
                throw new Exception();
        }
    }

    class TestConstructors
    {
        public static void Run()
        {
            Console.WriteLine(nameof(TestConstructors));

            ConstructorInfo ctor = typeof(ClassToConstruct).GetConstructor(new Type[] { typeof(int) });
            ClassToConstruct obj = (ClassToConstruct)ctor.Invoke(new object[] { 1 });
            if (obj._i != 1)
                throw new Exception();

            obj = (ClassToConstruct)ConstructorInvoker.Create(ctor).Invoke(1);
            if (obj._i != 1)
                throw new Exception();
        }

        public class ClassToConstruct
        {
            public int _i;

            public ClassToConstruct(int i)
            {
                _i = i;
            }
        }
    }

    class Test113750Regression
    {
        class Atom;

        public static void Run()
        {
            var arr = Array.CreateInstance(GetAtom(), 0);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static Type GetAtom() => typeof(Atom);

            if (!(arr is Atom[]))
                throw new Exception();
        }
    }

    class TestStringConstructor
    {
        public static void Run()
        {
            Console.WriteLine(nameof(TestStringConstructor));

            ConstructorInfo ctor = typeof(string).GetConstructor(new Type[] { typeof(char[]), typeof(int), typeof(int) });
            object str = ctor.Invoke(new object[] { new char[] { 'a' }, 0, 1 });
            if ((string)str != "a")
                throw new Exception();

            str = ConstructorInvoker.Create(ctor).Invoke(new char[] { 'a' }, 0, 1 );
            if ((string)str != "a")
                throw new Exception();
        }
    }

    class TestTypesInMethodSignatures
    {
        interface IUnreferenced { }

        interface IReferenced { }

        class UnreferencedBaseType : IUnreferenced, IReferenced { }
        class UnreferencedMidType : UnreferencedBaseType { }
        class ReferencedDerivedType : UnreferencedMidType { }

        static void DoSomething(ReferencedDerivedType d) { }

        public static void Run()
        {
            var mi = typeof(TestTypesInMethodSignatures).GetMethod(nameof(DoSomething), BindingFlags.Static | BindingFlags.NonPublic);
            Type t = mi.GetParameters()[0].ParameterType;
            int count = 0;
            while (t != typeof(object))
            {
                t = t.BaseType;
                count++;
            }

            Assert.Equal(count, 3);

            // We expect to see only IReferenced but not IUnreferenced
            Assert.Equal(1, mi.GetParameters()[0].ParameterType.GetInterfaces().Length);
            Assert.Equal(typeof(IReferenced), mi.GetParameters()[0].ParameterType.GetInterfaces()[0]);
        }
    }

    class TestAttributeInheritance
    {
        class BaseAttribute : Attribute
        {
            public string Field;
            public int Property { get; set; }
        }

        class DerivedAttribute : BaseAttribute { }

        [Derived(Field = "Hello", Property = 100)]
        class TestType { }

        public static void Run()
        {
            Console.WriteLine(nameof(TestAttributeInheritance));

            DerivedAttribute attr = typeof(TestType).GetCustomAttribute<DerivedAttribute>();
            if (attr.Field != "Hello" || attr.Property != 100)
                throw new Exception();
        }
    }

    class TestByRefLikeTypeMethod
    {
        ref struct ByRefLike
        {
            public readonly int Value;

            public ByRefLike(int value)
            {
                Value = value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        ref struct ByRefLike<T>
        {
            public readonly T Value;

            public ByRefLike(T value)
            {
                Value = value;
            }

            public override string ToString()
            {
                return Value.ToString() + " " + typeof(T).ToString();
            }
        }

        delegate string ToStringDelegate(ref ByRefLike thisObj);
        delegate string ToStringDelegate<T>(ref ByRefLike<T> thisObj);

#if !REFLECTION_FROM_USAGE
        [DynamicDependency("ToString", typeof(ByRefLike))]
        [DynamicDependency("ToString", typeof(ByRefLike<>))]
#endif
        public static void Run()
        {
            Console.WriteLine(nameof(TestByRefLikeTypeMethod));

#if REFLECTION_FROM_USAGE
            // Ensure things we reflect on are in the static callgraph
            if (string.Empty.Length > 0)
            {
                default(ByRefLike).ToString();
                ToStringDelegate s = null;
                s = s.Invoke;
                default(ByRefLike<object>).ToString();
                ToStringDelegate<object> s2 = null;
                s2 = s2.Invoke;
            }
#endif

            {
                Type byRefLikeType = GetTestType(nameof(TestByRefLikeTypeMethod), nameof(ByRefLike));
                MethodInfo toStringMethod = byRefLikeType.GetMethod("ToString");
                var toString = (ToStringDelegate)toStringMethod.CreateDelegate(typeof(ToStringDelegate));

                ByRefLike foo = new ByRefLike(123);
                if (toString(ref foo) != "123")
                    throw new Exception();
            }

            {
                Type byRefLikeGenericType = typeof(ByRefLike<string>);
                MethodInfo toStringGenericMethod = byRefLikeGenericType.GetMethod("ToString");
                var toStringGeneric = (ToStringDelegate<string>)toStringGenericMethod.CreateDelegate(typeof(ToStringDelegate<string>));

                ByRefLike<string> fooGeneric = new ByRefLike<string>("Hello");
                if (toStringGeneric(ref fooGeneric) != "Hello System.String")
                    throw new Exception();
            }

            {
                Type byRefLikeGenericType = typeof(ByRefLike<object>);
                MethodInfo toStringGenericMethod = byRefLikeGenericType.GetMethod("ToString");
                var toStringGeneric = (ToStringDelegate<object>)toStringGenericMethod.CreateDelegate(typeof(ToStringDelegate<object>));

                ByRefLike<object> fooGeneric = new ByRefLike<object>("Hello");
                if (toStringGeneric(ref fooGeneric) != "Hello System.Object")
                    throw new Exception();
            }
        }
    }

    class TestNecessaryEETypeReflection
    {
        struct NeverUsed { }

        public static unsafe void Run()
        {
            Console.WriteLine(nameof(TestNecessaryEETypeReflection));

            // Pointer types don't have a constructed EEType form but the compiler should
            // still track them as reflectable if a typeof happens.
            Type necessaryType = typeof(NeverUsed*);

            Type neverUsedType = necessaryType.GetElementType();

            if (neverUsedType.Name != nameof(NeverUsed))
                throw new Exception();
        }
    }

    class TestInterfaceMethod
    {
        interface IFoo
        {
            string Frob(int x);
        }

        class Foo : IFoo
        {
            public string Frob(int x)
            {
                return x.ToString();
            }
        }

        class Gen<T> { }

        interface IFoo<out T>
        {
            string Frob();
        }

        class Foo<T> : IFoo<Gen<T>>
        {
            public string Frob()
            {
                return typeof(T).ToString();
            }
        }

#if !REFLECTION_FROM_USAGE
        [DynamicDependency("Frob", typeof(IFoo))]
        [DynamicDependency("Frob", typeof(IFoo<>))]
#endif
        public static void Run()
        {
            Console.WriteLine(nameof(TestInterfaceMethod));

#if REFLECTION_FROM_USAGE
            // Ensure things we reflect on are in the static callgraph
            if (string.Empty.Length > 0)
            {
                ((IFoo)new Foo()).Frob(1);
                ((IFoo<object>)new Foo<string>()).Frob();
            }
#endif

            object result = InvokeTestMethod(typeof(IFoo), "Frob", new Foo(), 42);
            if ((string)result != "42")
                throw new Exception();

            result = InvokeTestMethod(typeof(IFoo<object>), "Frob", new Foo<string>());
            if ((string)result != "System.String")
                throw new Exception();
        }
    }

    class TestContainment
    {
        class NeverUsedContainerType
        {
            public class UsedNestedType
            {
                public static int CallMe()
                {
                    return 42;
                }
            }
        }

#if !REFLECTION_FROM_USAGE
        [DynamicDependency("CallMe", typeof(NeverUsedContainerType.UsedNestedType))]
#endif
        public static void Run()
        {
            Console.WriteLine(nameof(TestContainment));

#if REFLECTION_FROM_USAGE
            // Ensure things we reflect on are in the static callgraph
            if (string.Empty.Length > 0)
            {
                NeverUsedContainerType.UsedNestedType.CallMe();
            }
#endif

            Type neverUsedContainerType = GetTestType(nameof(TestContainment), nameof(NeverUsedContainerType));
            Type usedNestedType = neverUsedContainerType.GetNestedType(nameof(NeverUsedContainerType.UsedNestedType));

            // Since we called CallMe, it has reflection metadata and it is invokable
            object o = InvokeTestMethod(usedNestedType, nameof(NeverUsedContainerType.UsedNestedType.CallMe));
            if ((int)o != 42)
                throw new Exception();

            // We can get a type handle for the nested type (the invoke mapping table needs it)
            if (!HasTypeHandle(usedNestedType))
                throw new Exception($"{nameof(NeverUsedContainerType.UsedNestedType)} should have an EEType");

            // But the containing type doesn't need an EEType
            if (HasTypeHandle(neverUsedContainerType))
                throw new Exception($"{nameof(NeverUsedContainerType)} should not have an EEType");
        }
    }

    class TestByRefReturnInvoke
    {
        enum Mine { One = 2018 }

        [StructLayout(LayoutKind.Sequential)]
        struct BigStruct { public ulong X, Y, Z, W, A, B, C, D; }

        public ref struct ByRefLike { }

        private sealed class TestClass<T>
        {
            private T _value;

            public TestClass(T value) { _value = value; }
            public ref T RefReturningProp
            {
#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
                get => ref _value;
            }
#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
            public static unsafe ref ByRefLike ByRefLikeRefReturningMethod(ByRefLike* a) => ref *a;
        }

        private sealed class TestClass2<T>
        {
            private T _value;

            public TestClass2(T value) { _value = value; }

#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
            public ref T RefReturningMethod(T someArgument) => ref _value;
        }

        private sealed unsafe class TestClassIntPointer
        {
            private int* _value;

            public TestClassIntPointer(int* value) { _value = value; }
            public ref int* RefReturningProp
            {
#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
                get => ref _value;
            }
            public unsafe ref int* NullRefReturningProp
            {
#if OPTIMIZED_MODE_WITHOUT_SCANNER
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
                get => ref *(int**)null;
            }
        }

        public static void TestRefReturnPropertyGetValue()
        {
            TestRefReturnInvoke('a', (p, t) => p.GetValue(t));
            TestRefReturnInvoke(Mine.One, (p, t) => p.GetValue(t));
            TestRefReturnInvoke("Hello", (p, t) => p.GetValue(t));
            TestRefReturnInvoke(new BigStruct { X = 123, D = 456 }, (p, t) => p.GetValue(t));
            TestRefReturnInvoke(new object(), (p, t) => p.GetValue(t));
            TestRefReturnInvoke((object)null, (p, t) => p.GetValue(t));
        }

        public static void TestRefReturnMethodInvoke()
        {
            TestRefReturnInvoke(Mine.One, (p, t) => p.GetGetMethod().Invoke(t, Array.Empty<object>()));
            TestRefReturnInvoke("Hello", (p, t) => p.GetGetMethod().Invoke(t, Array.Empty<object>()));
            TestRefReturnInvoke(new BigStruct { X = 123, D = 456 }, (p, t) => p.GetGetMethod().Invoke(t, Array.Empty<object>()));
            TestRefReturnInvoke(new object(), (p, t) => p.GetGetMethod().Invoke(t, Array.Empty<object>()));
            TestRefReturnInvoke((object)null, (p, t) => p.GetGetMethod().Invoke(t, Array.Empty<object>()));

            // Regression test
            MethodInfo mi = typeof(TestClass2<string>).GetMethod(nameof(TestClass2<string>.RefReturningMethod));
            mi.Invoke(new TestClass2<string>("Hello"), new object[] { "Hello" });
        }

        public static void TestRefReturnNullable()
        {
            TestRefReturnInvokeNullable<int>(42);
            TestRefReturnInvokeNullable<Mine>(Mine.One);
            TestRefReturnInvokeNullable<BigStruct>(new BigStruct { X = 987, D = 543 });
        }

        public static void TestRefReturnNullableNoValue()
        {
            TestRefReturnInvokeNullable<int>(default(int?));
            TestRefReturnInvokeNullable<Mine>(default(Mine?));
            TestRefReturnInvokeNullable<BigStruct>(default(BigStruct?));
        }

        public static unsafe void TestRefReturnOfPointer()
        {
            int* expected = (int*)0x1122334455667788;
            TestClassIntPointer tc = new TestClassIntPointer(expected);

            PropertyInfo p = typeof(TestClassIntPointer).GetProperty(nameof(TestClassIntPointer.RefReturningProp));
            object rv = p.GetValue(tc);
            Assert.True(rv is Pointer);
            int* actual = (int*)(Pointer.Unbox(rv));
            Assert.Equal((IntPtr)expected, (IntPtr)actual);
        }

        public static unsafe void TestNullRefReturnOfPointer()
        {
            TestClassIntPointer tc = new TestClassIntPointer(null);

            PropertyInfo p = typeof(TestClassIntPointer).GetProperty(nameof(TestClassIntPointer.NullRefReturningProp));
            Assert.NotNull(p);
            Assert.Throws<TargetInvocationException>(() => p.GetValue(tc));
        }

        public static unsafe void TestByRefLikeRefReturn()
        {
            if (string.Empty.Length > 0)
            {
                TestClass<int>.ByRefLikeRefReturningMethod(null);
            }

            ByRefLike brl = new ByRefLike();
            ByRefLike* pBrl = &brl;
            MethodInfo mi = typeof(TestClass<int>).GetMethod(nameof(TestClass<int>.ByRefLikeRefReturningMethod));
            try
            {
                // Don't use Assert.Throws because that will make a lambda and invalidate the pointer
                object o = mi.Invoke(null, new object[] { Pointer.Box(pBrl, typeof(ByRefLike*)) });
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }
        }

        private static void TestRefReturnInvoke<T>(T value, Func<PropertyInfo, TestClass<T>, object> invoker)
        {
            TestClass<T> tc = new TestClass<T>(value);

            if (String.Empty.Length > 0)
            {
                tc.RefReturningProp.ToString();
            }

            PropertyInfo p = typeof(TestClass<T>).GetProperty(nameof(TestClass<T>.RefReturningProp));
            object rv = invoker(p, tc);
            if (rv != null)
            {
                Assert.Equal(typeof(T), rv.GetType());
            }

            if (typeof(T).IsValueType)
            {
                Assert.Equal(value, rv);
            }
            else
            {
                Assert.Same(value, rv);
            }
        }

        private static void TestRefReturnInvokeNullable<T>(T? nullable) where T : struct
        {
            TestClass<T?> tc = new TestClass<T?>(nullable);

            if (string.Empty.Length > 0)
            {
                tc.RefReturningProp.ToString();
            }

            PropertyInfo p = typeof(TestClass<T?>).GetProperty(nameof(TestClass<T?>.RefReturningProp));
            object rv = p.GetValue(tc);
            if (rv != null)
            {
                Assert.Equal(typeof(T), rv.GetType());
            }
            if (nullable.HasValue)
            {
                Assert.Equal(nullable.Value, rv);
            }
            else
            {
                Assert.Null(rv);
            }
        }

        public static void Run()
        {
            Console.WriteLine(nameof(TestByRefReturnInvoke));
            TestRefReturnPropertyGetValue();
            TestRefReturnMethodInvoke();
            TestRefReturnNullable();
            TestRefReturnNullableNoValue();
            TestRefReturnOfPointer();
            TestNullRefReturnOfPointer();
            TestByRefLikeRefReturn();
        }
    }

    class TestTypeGetType
    {
        public static void Run()
        {
            try
            {
                Type.GetType("System.Span`1[[System.Byte, System.Runtime]][], System.Runtime");
                Type.GetType("System.Collections.Generic.Dictionary`2[System.String]");
            }
            catch { }

            if (Type.GetType("MyClassOnlyReferencedFromTypeGetType") == null)
                throw new Exception();
        }
    }

    class TestILScanner
    {
        class MyGenericUnusedClass<T>
        {
            public static void TheMethod() { }
        }

        class LinqTestCase<T>
        {
            public static void Create() { }
        }

        class OtherLinqTestCase<T>
        {
            public static int Update { get; }
        }

        enum Mine { One }

        class PartialCanonTestType<T, U>
        {
            public int TestMethod() => 42;
        }

        static int TestPartialCanon<T>()
        {
            return (int)typeof(PartialCanonTestType<object, T>).GetMethod("TestMethod").Invoke(new PartialCanonTestType<object, T>(), Array.Empty<object>());
        }

        static MethodInfo GetTotallyUnreferencedMethod(Type t) => t.GetMethod(String.Format("Totally{0}", "UnreferencedMethod"));

        public static void Run()
        {
            Console.WriteLine(nameof(TestILScanner));

            Console.WriteLine("Search current assembly");
            {
                {
                    Type t = Type.GetType(nameof(MyUnusedClass), throwOnError: false);
                    if (t == null)
                        throw new Exception(nameof(MyUnusedClass));

                    Console.WriteLine("GetMethod on a non-generic type");
                    MethodInfo mi = t.GetMethod(nameof(MyUnusedClass.UnusedMethod1));
                    if (mi == null)
                        throw new Exception(nameof(MyUnusedClass.UnusedMethod1));

                    mi.Invoke(null, Array.Empty<object>());
                }

                {
                    Type t = Type.GetType(nameof(MyUnusedClass), throwOnError: false);
                    if (t == null)
                        throw new Exception(nameof(MyUnusedClass));

                    Console.WriteLine("Totally unreferenced method on a non-generic type (we should not find it)");
                    MethodInfo mi = GetTotallyUnreferencedMethod(t);
                    if (mi != null)
                        throw new Exception("UnreferencedMethod");
                }

                {
                    Type t = Type.GetType(nameof(MyUnusedClass), throwOnError: false);
                    if (t == null)
                        throw new Exception(nameof(MyUnusedClass));

                    Console.WriteLine("GetMethod on a non-generic type for a generic method");
                    MethodInfo mi = t.GetMethod(nameof(MyUnusedClass.GenericMethod));
                    if (mi == null)
                        throw new Exception(nameof(MyUnusedClass.GenericMethod));

                    mi.MakeGenericMethod(typeof(object)).Invoke(null, Array.Empty<object>());
                }
            }

            Console.WriteLine("Generics");
            {
                MethodInfo mi = typeof(MyGenericUnusedClass<object>).GetMethod(nameof(MyGenericUnusedClass<object>.TheMethod));
                if (mi == null)
                    throw new Exception(nameof(MyGenericUnusedClass<object>.TheMethod));

                mi.Invoke(null, Array.Empty<object>());
            }

            Console.WriteLine("Partial canonical types");
            {
                if (TestPartialCanon<string>() != 42)
                    throw new Exception("PartialCanon");
            }

            Console.WriteLine("Search in system assembly");
            {
                Type t = Type.GetType("System.Runtime.CompilerServices.SuppressIldasmAttribute", throwOnError: false);
                if (t == null)
                    throw new Exception("SuppressIldasmAttribute");
            }

#if !MULTIMODULE_BUILD
            Console.WriteLine("Search through a forwarder");
            {
                Type t = Type.GetType("System.Collections.Generic.List`1, System.Collections", throwOnError: false);
                if (t == null)
                    throw new Exception("List");
            }

            Console.WriteLine("Search in mscorlib");
            {
                Type t = Type.GetType("System.Runtime.CompilerServices.CompilerGlobalScopeAttribute, mscorlib", throwOnError: false);
                if (t == null)
                    throw new Exception("CompilerGlobalScopeAttribute");
            }
#endif

            Console.WriteLine("Enum.GetValues");
            {
                if (Enum.GetValues(typeof(Mine)).GetType().GetElementType() != typeof(Mine))
                    throw new Exception("GetValues");
            }

            Console.WriteLine("Enum.GetValuesAsUnderlyingType");
            {
                if (Enum.GetValuesAsUnderlyingType(typeof(Mine)).GetType() != typeof(int[]))
                    throw new Exception("Enum.GetValuesAsUnderlyingType");
            }

            Console.WriteLine("Pattern in LINQ expressions");
            {
                Type objType = typeof(object);

                MethodInfo mi = typeof(LinqTestCase<>).MakeGenericType(objType).GetMethod(nameof(LinqTestCase<object>.Create));

                if (mi == null)
                    throw new Exception("GetValues");

                mi.Invoke(null, Array.Empty<object>());
            }

            Console.WriteLine("Other pattern in LINQ expressions");
            {
                Type objType = typeof(object);

                PropertyInfo pi = typeof(OtherLinqTestCase<>).MakeGenericType(objType).GetProperty(nameof(OtherLinqTestCase<object>.Update));

                if (pi == null)
                    throw new Exception("GetProperty");

                pi.GetValue(null, Array.Empty<object>());
            }
        }
    }

    class TestUnreferencedEnum
    {
        public enum UnreferencedEnum { One }

#if OPTIMIZED_MODE_WITHOUT_SCANNER
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif
        public static void ReferenceEnum(UnreferencedEnum r)
        {
        }

        public static void Run()
        {
            Console.WriteLine(nameof(TestUnreferencedEnum));

            if (String.Empty.Length > 0)
            {
                ReferenceEnum(default);
            }

            MethodInfo mi = typeof(TestUnreferencedEnum).GetMethod(nameof(ReferenceEnum));
            Type enumType = mi.GetParameters()[0].ParameterType;
            if (Enum.GetUnderlyingType(enumType) != typeof(int))
                throw new Exception();
        }
    }

    class TestAssemblyLoad
    {
        public static void Run()
        {
            Assert.Equal("System.Private.CoreLib", Assembly.Load("System.Private.CoreLib, PublicKeyToken=cccccccccccccccc").GetName().Name);
            Assert.Equal("System.Console", Assembly.Load("System.Console, PublicKeyToken=cccccccccccccccc").GetName().Name);
#if !MULTIMODULE_BUILD
            Assert.Equal("mscorlib", Assembly.Load("mscorlib, PublicKeyToken=cccccccccccccccc").GetName().Name);
#endif
        }
    }

    class TestBaseOnlyUsedFromCode
    {
        class SomeReferenceType { }

        class SomeGenericClass<T>
        {
            public static string Cookie;
        }

        class OtherGenericClass<T>
        {
            public override string ToString() => SomeGenericClass<T>.Cookie;
        }

        public static void Run()
        {
            SomeGenericClass<SomeReferenceType>.Cookie = "Hello";

            var inst = Activator.CreateInstance(typeof(OtherGenericClass<>).MakeGenericType(GetSomeReferenceType()));

            if (inst.ToString() != "Hello")
                throw new Exception();

            static Type GetSomeReferenceType() => typeof(SomeReferenceType);
        }
    }

    class TestRuntimeLab929Regression
    {
        static Type s_atom = typeof(Atom);

        class Atom { }

        abstract class Declaring
        {
            public abstract Type DoTheGenericThing<T>();
        }

        class Defining : Declaring
        {
            public override Type DoTheGenericThing<T>() => typeof(T[,,,]);
        }

        class Gen<T>
        {
            private static Declaring s_declaring = new Defining();

            public static Type GetTheThing() => s_declaring.DoTheGenericThing<T>();
        }

        public static void Run()
        {
            // We don't want the analysis to see what Gen is instantiated with
            // so that we force it to make up an instantiation argument.
            var t = (Type)typeof(Gen<>).MakeGenericType(s_atom).GetMethod("GetTheThing").Invoke(null, Array.Empty<object>());
            Assert.Equal(typeof(Atom), t.GetElementType());
            Assert.Equal(4, t.GetArrayRank());
        }
    }

#if !REFLECTION_FROM_USAGE
    class TestNotReflectedIsNotReflectable
    {
        static Type s_type = typeof(TestNotReflectedIsNotReflectable);

#if OPTIMIZED_MODE_WITHOUT_SCANNER
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif
        public static void IsCalledAndReflected()
        {
        }

#if OPTIMIZED_MODE_WITHOUT_SCANNER
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif
        public static void IsCalledOnly()
        {
        }

        public static void Run()
        {
            if (String.Empty.Length > 0)
            {
                // Call both, but reflect only on one. Only one should be reflectable.
                IsCalledAndReflected();
                IsCalledOnly();
                typeof(TestNotReflectedIsNotReflectable).GetMethod(nameof(IsCalledAndReflected));
            }

            if (s_type.GetMethod(nameof(IsCalledAndReflected)) == null)
                throw new Exception();

            if (s_type.GetMethod(nameof(IsCalledOnly)) != null)
                throw new Exception();
        }
    }

    class TestGenericInstantiationsAreEquallyReflectable
    {
        static Type s_type = typeof(GenericType<>);

        class GenericType<T>
        {
            public static Type Gimme() => typeof(T);
        }

        public static void Run()
        {
            if (String.Empty.Length > 0)
            {
                // Reflect over GenericType<object>, but also call GenericType<double>.
                // Both should be equally reflectable.
                GenericType<double>.Gimme();
                typeof(GenericType<>).MakeGenericType(typeof(object)).GetMethod("Gimme");
            }

            var t = (Type)s_type.MakeGenericType(GetDouble()).GetMethod("Gimme").Invoke(null, Array.Empty<object>());
            if (t != typeof(double))
                throw new Exception();
            static Type GetDouble() => typeof(double);
        }
    }

    class TestStackTraces
    {
        class RefType { }
        struct ValType { }

        class C1<T>
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static string M1(T c1m1param) => Environment.StackTrace;
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static string M2(T c1m2param) => Environment.StackTrace;
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static string M3<U>(T c1m3param1, U c1m3param2) => Environment.StackTrace;
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static string M4<U>(T c1m4param1, U c1m4param2) => Environment.StackTrace;
        }

        public static void Run()
        {
            // These methods are visible to reflection
            typeof(C1<RefType>).GetMethod(nameof(C1<RefType>.M1));
            typeof(C1<RefType>).GetMethod(nameof(C1<RefType>.M3));
            typeof(C1<ValType>).GetMethod(nameof(C1<ValType>.M1));
            typeof(C1<ValType>).GetMethod(nameof(C1<ValType>.M3));

            Check("C1", "M1", "c1m1param", true, C1<RefType>.M1(default));
            Check("C1", "M1", "c1m1param", true, C1<ValType>.M1(default));
            Check("C1", "M2", "c1m2param", false, C1<RefType>.M2(default));
            Check("C1", "M2", "c1m2param", false, C1<ValType>.M2(default));
            Check("C1", "M3", "c1m3param", true, C1<RefType>.M3<RefType>(default, default));
            Check("C1", "M3", "c1m3param", true, C1<RefType>.M3<ValType>(default, default));
            Check("C1", "M3", "c1m3param", true, C1<ValType>.M3<RefType>(default, default));
            Check("C1", "M3", "c1m3param", true, C1<ValType>.M3<ValType>(default, default));
            Check("C1", "M4", "c1m4param", false, C1<RefType>.M4<RefType>(default, default));
            Check("C1", "M4", "c1m4param", false, C1<RefType>.M4<ValType>(default, default));
            Check("C1", "M4", "c1m4param", false, C1<ValType>.M4<RefType>(default, default));
            Check("C1", "M4", "c1m4param", false, C1<ValType>.M4<ValType>(default, default));

            static void Check(string type, string method, string param, bool hasParam, string s)
            {
                if (!s.Contains(type))
                    throw new Exception($"'{s}' doesn't contain '{type}'");
                if (!s.Contains(method))
                    throw new Exception($"'{s}' doesn't contain '{method}'");
                if (hasParam && !s.Contains(param))
                    throw new Exception($"'{s}' doesn't contain '{param}'");
                if (!hasParam && s.Contains(param))
                    throw new Exception($"'{s}' contains '{param}'");
            }
        }
    }
#endif

    class TypeConstructionTest
    {
        struct Atom { }
        struct ArrayElementUsedInGenericDictionary { }

        class Gen<T> { }

        static Type s_atom = typeof(Atom);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static Type GetArrayElementUsedInGenericDictionary() => typeof(ArrayElementUsedInGenericDictionary);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static object GenericMethod<T>() => null;

        public static void Run()
        {
            string message1 = "";
            try
            {
                _ = typeof(Gen<>).MakeGenericType(s_atom).TypeHandle;
            }
            catch (Exception ex)
            {
                message1 = ex.Message;
            }
            if (!message1.Contains("ReflectionTest+TypeConstructionTest+Gen`1[ReflectionTest+TypeConstructionTest+Atom]"))
                throw new Exception();

            string message2 = "";
            try
            {
                _ = s_atom.MakeArrayType().TypeHandle;
            }
            catch (Exception ex)
            {
                message2 = ex.Message;
            }
            if (!message2.Contains("ReflectionTest+TypeConstructionTest+Atom[]"))
                throw new Exception();

            string message3 = "";
            try
            {
                Array.CreateInstance(s_atom, 10);
            }
            catch (Exception ex)
            {
                message3 = ex.Message;
            }
            if (!message3.Contains("ReflectionTest+TypeConstructionTest+Atom[]"))
                throw new Exception();

            // Do what's needed so that we force an unconstructed MT for ArrayElementUsedInGenericDictionary[] into the program
            // 1. Statically call the method
            // 2. Make the method visible target of reflection
            // This will force the compiler to place the method generic dictionary into a hashtable addressable using the instantiation.
            GenericMethod<ArrayElementUsedInGenericDictionary[]>();
            typeof(TestGetUninitializedObject).GetMethod(nameof(GenericMethod));
            string message4 = "";
            try
            {
                Array.CreateInstance(GetArrayElementUsedInGenericDictionary(), 10);
            }
            catch (Exception ex)
            {
                message4 = ex.Message;
            }
            if (!message4.Contains("ReflectionTest+TypeConstructionTest+ArrayElementUsedInGenericDictionary[]"))
                throw new Exception();
        }
    }

    class CodelessMethodMetadataTest
    {
        static class TypeWithCodelessMethods
        {
            // "where T: struct" prevents the compiler from coming up with a good T
            public static void CodelessMethod<T>() where T : struct { }
        }

        static class CodelessType<T> where T : struct
        {
            public static void CodelessMethod() { }
        }

        public static void Run()
        {
            if (typeof(CodelessType<>).GetMethods(BindingFlags.Public | BindingFlags.Static).Length != 1)
                throw new Exception();

            if (typeof(TypeWithCodelessMethods).GetMethods(BindingFlags.Public | BindingFlags.Static).Length != 1)
                throw new Exception();
        }
    }

    class TestSimpleDelegateTargets
    {
        class TestClass
        {
            public static void StaticMethod() { }
            public void InstanceMethod() { }
            public static void SimplyCalledMethod() { }
        }

        class TestClass<T>
        {
            public static void StaticMethod() { }
        }

        static void CheckGeneric<T>()
        {
            Action staticMethod = TestClass<T>.StaticMethod;
            if (staticMethod.GetMethodInfo().Name != nameof(TestClass<T>.StaticMethod))
                throw new Exception();
        }

        public static void Run()
        {
            Console.WriteLine("Testing delegate targets are reflectable...");

            Action staticMethod = TestClass.StaticMethod;
            if (staticMethod.GetMethodInfo().Name != nameof(TestClass.StaticMethod))
                throw new Exception();

            Action instanceMethod = new TestClass().InstanceMethod;
            if (instanceMethod.GetMethodInfo().Name != nameof(TestClass.InstanceMethod))
                throw new Exception();

            TestClass.SimplyCalledMethod();

            Assert.Equal(
#if REFLECTION_FROM_USAGE
                3,
#else
                2,
#endif
                typeof(TestClass).CountMethods());

            CheckGeneric<object>();
        }
    }

    class TestVirtualDelegateTargets
    {
        abstract class Base
        {
            public virtual void VirtualMethod() { }
            public abstract void AbstractMethod();
        }

        class Derived : Base, IBar
        {
            public override void AbstractMethod() { }
            public override void VirtualMethod() { }
            void IFoo.InterfaceMethod() { }
        }

        interface IFoo
        {
            void InterfaceMethod();
            void DefaultInterfaceMethod() { }
        }

        interface IBar : IFoo
        {
            void IFoo.DefaultInterfaceMethod() { }
        }

        static Base s_baseInstance = new Derived();
        static IFoo s_ifooInstance = new Derived();

        public static void Run()
        {
            Console.WriteLine("Testing virtual delegate targets are reflectable...");

            Action abstractMethod = s_baseInstance.AbstractMethod;
            if (abstractMethod.GetMethodInfo().Name != nameof(Derived.AbstractMethod))
                throw new Exception();

            Action virtualMethod = s_baseInstance.VirtualMethod;
            if (virtualMethod.GetMethodInfo().Name != nameof(Derived.VirtualMethod))
                throw new Exception();

            Action interfaceMethod = s_ifooInstance.InterfaceMethod;
            if (!interfaceMethod.GetMethodInfo().Name.EndsWith("IFoo.InterfaceMethod"))
                throw new Exception();

            Action defaultMethod = s_ifooInstance.DefaultInterfaceMethod;
            if (!defaultMethod.GetMethodInfo().Name.EndsWith("IFoo.DefaultInterfaceMethod"))
                throw new Exception();
        }
    }

    class TestFieldMetadata
    {
        class ClassWithTwoFields
        {
            public static int UsedField;
            public static UnusedClass UnusedField;
        }

        class UnusedClass { }

        interface IMessWithYou { }
        class GenericTypeWithUnsatisfiableConstrains<T> where T : struct, IMessWithYou
        {
            public static int SomeField;
        }

        class GenericClass<T>
        {
            public static string SomeField;
        }

        struct Atom1 { }
        struct Atom2 { }
        struct Atom3 { }

        public static void Run()
        {
            ClassWithTwoFields.UsedField = 123;

#if REFLECTION_FROM_USAGE
            // Merely accessing a field should trigger full reflectability of it when reflection from
            // usage is active.
            Type classWithTwoFields = GetTestType(nameof(TestFieldMetadata), nameof(ClassWithTwoFields));
            if ((int)classWithTwoFields.GetField(nameof(ClassWithTwoFields.UsedField)).GetValue(null) != 123)
            {
                throw new Exception();
            }

            // But the unused field should not exist
            if (classWithTwoFields.GetField(nameof(ClassWithTwoFields.UnusedField)) != null)
            {
                throw new Exception();
            }
#else
            // If reflection from usage is not active, we shouldn't even see the owning type, despite the field use
            if (SecretGetType(nameof(TestFieldMetadata), nameof(ClassWithTwoFields)) != null)
            {
                throw new Exception();
            }
#endif
            // The type of the unused field shouldn't be generated under any circumstances
            if (SecretGetType(nameof(TestFieldMetadata), nameof(UnusedClass)) != null)
            {
                throw new Exception();
            }

            // The compiler cannot fulfil this instantiation without universal shared code.
            // We should get a working metadata-only type that can be inspected.
            if (typeof(GenericTypeWithUnsatisfiableConstrains<>).GetField("SomeField").Name != "SomeField")
            {
                throw new Exception();
            }

            // Access a field on a generic type in an obvious way
            if (typeof(GenericClass<Atom1>).GetField(nameof(GenericClass<Atom1>.SomeField)).Name != "SomeField")
            {
                throw new Exception();
            }

            // We should be able to reflection-see the field on other reflection visible generic instances
            // of the same generic type definition.
            GetGenericClassOfAtom2().GetField("SomeField").SetValue(null, "Cookie");
            static Type GetGenericClassOfAtom2() => typeof(GenericClass<Atom2>);

            GenericClass<Atom3>.SomeField = "1234";
#if REFLECTION_FROM_USAGE
            // If we're doing reflection from usage, we used the field and we expect it to be reflectable
            typeof(GenericClass<>).MakeGenericType(GetAtom3()).GetField("SomeField").SetValue(null, "Cookie");
#else
            // If we're not doing reflection from usage, the generic instance shouldn't even exist.
            // We only touched the static base of the type, not the whole type.
            bool exists = false;
            try
            {
                _ = typeof(GenericClass<>).MakeGenericType(GetAtom3()).TypeHandle;
                exists = true;
            }
            catch
            {
                exists = false;
            }
            if (exists)
                throw new Exception();
#endif
            static Type GetAtom3() => typeof(Atom3);
        }
    }

    class TestLinqInvocation
    {
        delegate void RunMeDelegate();

        static void RunMe() { }

        public static void Run()
        {
            Expression<Action<RunMeDelegate>> ex = (RunMeDelegate m) => m();
            ex.Compile()(RunMe);
        }
    }

    class TestGenericMethodsHaveSameReflectability
    {
        public interface IHardToGuess { }

        struct SomeStruct<T> : IHardToGuess { }

        public static void TakeAGuess<T>() where T : struct, IHardToGuess { }

        class Atom1 { }
        class Atom2 { }

        static Type s_someStructOverAtom1 = typeof(SomeStruct<Atom1>);

        public static void Run()
        {
            // Statically call with SomeStruct over Atom2
            TakeAGuess<SomeStruct<Atom2>>();

            // MakeGenericMethod the method with SomeStruct over Atom1
            // Note the compiler cannot figure out a suitable instantiation here because
            // of the "struct, interface" constraint on T. But the expected side effect is that the static
            // call above now became reflection-visible and will let the type loader make this
            // work at runtime. All generic instantiations share the same refection visibility.
            var mi = typeof(TestGenericMethodsHaveSameReflectability).GetMethod(nameof(TakeAGuess)).MakeGenericMethod(s_someStructOverAtom1);

            mi.Invoke(null, Array.Empty<object>());
        }
    }

    class TestRunClassConstructor
    {
        static class TypeWithNoStaticFieldsButACCtor
        {
            static TypeWithNoStaticFieldsButACCtor()
            {
                s_cctorRan = true;
            }
        }

        class ClassWithCctor
        {
            static ClassWithCctor() => s_field = 42;
        }

        class ClassWithCctor<T>
        {
            static ClassWithCctor() => s_field = 11;
        }

        class DynamicClassWithCctor<T>
        {
            static DynamicClassWithCctor() => s_field = 1000;
        }

        class Atom { }

        private static bool s_cctorRan;
        private static int s_field;

        public static void Run()
        {
            RuntimeHelpers.RunClassConstructor(typeof(TypeWithNoStaticFieldsButACCtor).TypeHandle);
            if (!s_cctorRan)
                throw new Exception();

            RunTheCctor(typeof(ClassWithCctor));
            if (s_field != 42)
                throw new Exception();

            RunTheCctor(typeof(ClassWithCctor<Atom>));
            if (s_field != 11)
                throw new Exception();

            RunTheCctor(typeof(DynamicClassWithCctor<>).MakeGenericType(GetAtom()));
            if (s_field != 1000)
                throw new Exception();

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void RunTheCctor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type t)
                => RuntimeHelpers.RunClassConstructor(t.TypeHandle);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static Type GetAtom() => typeof(Atom);
        }
    }

    class TestAttributeInheritance2
    {
        [AttributeUsage(AttributeTargets.All, Inherited = true)]
        class AttAttribute : Attribute { }

        class Base
        {
            [Att]
            public virtual void VirtualMethodWithAttribute() { }
        }

        class Derived : Base
        {
            public override void VirtualMethodWithAttribute() { }
        }

        public static void Run()
        {
            object[] attrs = typeof(Derived).GetMethod(nameof(Derived.VirtualMethodWithAttribute)).GetCustomAttributes(inherit: true);
            if (attrs.Length != 1 || attrs[0].GetType().Name != nameof(AttAttribute))
            {
                throw new Exception();
            }
        }
    }

    class TestInvokeMethodMetadata
    {
        delegate int WithDefaultParameter1(int value = 1234);
        delegate DateTime WithDefaultParameter2(DateTime value);

        public static int Method(int value) => value;

        public static DateTime Method(DateTime value) => value;

        public static void Run()
        {
            // Check that we have metadata for the Invoke method to convert Type.Missing to the actual value.
            WithDefaultParameter1 del1 = Method;
            int val = (int)del1.DynamicInvoke(new object[] { Type.Missing });
            if (val != 1234)
                throw new Exception();

            // Check that we have metadata for the Invoke method to find a matching method
            Delegate del2 = Delegate.CreateDelegate(typeof(WithDefaultParameter2), typeof(TestInvokeMethodMetadata), nameof(Method));
            if (del2.Method.ReturnType != typeof(DateTime))
                throw new Exception();
        }
    }

    class TestVTableOfNullableUnderlyingTypes
    {
        struct NeverAllocated
        {
            public override string ToString() => "Never allocated";
        }

        static Type s_hidden = typeof(Nullable<NeverAllocated>);

        public static void Run()
        {
            // Underlying type of a Nullable needs to be considered constructed.
            // Trimming warning suppressions in the libraries depend on this invariant.
            var instance = RuntimeHelpers.GetUninitializedObject(s_hidden);
            if (instance.ToString() != "Never allocated")
                throw new Exception();
        }
    }

    class TestInterfaceLists
    {
        interface IGeneric<T> { }

        class Class<T> : IGeneric<T> { }

        static Type s_hidden = typeof(Class<string>);

        public static void Run()
        {
            // Can't drop an interface from the interface list if the interface is referenced.
            // Trimming warning suppressions in the libraries depend on this invariant.
            foreach (var intface in s_hidden.GetInterfaces())
            {
                if (intface.HasSameMetadataDefinitionAs(typeof(IGeneric<object>)))
                    return;
            }

            throw new Exception();
        }
    }

    class TestMethodConsistency
    {
        class MyGenericType<T>
        {
            public static string MyMethod() => typeof(T).Name;
        }

        struct Atom { }

        public static void Run()
        {
            object returned = Grab<Atom>().Invoke(null, null);
            if ((string)returned != nameof(Atom))
                throw new Exception();

            static MethodInfo Grab<T>() => typeof(MyGenericType<T>).GetMethod(nameof(MyGenericType<T>.MyMethod));
        }
    }

    class TestGenericMethodOnGenericType
    {
        class Gen<T, U> { }

        struct Atom1 { }
        struct Atom2 { }

        class GenericType<T>
        {
            public static Gen<T, U> GenericMethod1<U>() => new Gen<T, U>();
            public static Gen<T, U> GenericMethod2<U>() => new Gen<T, U>();
        }

        static MethodInfo s_genericMethod2 = typeof(GenericType<>).GetMethod("GenericMethod2");

        static object Make<T>(Type t)
        {
            if (t.IsValueType) throw null;
            // AOT safe because we just checked t is not a valuetype
            return typeof(GenericType<T>).GetMethod("GenericMethod1").MakeGenericMethod(t).Invoke(null, null);
        }

        public static void Run()
        {
            Make<Atom1>(typeof(object));

            // This is supposed to be all AOT safe because we're instantiating over a reference type
            // Ideally there would also be no AOT warning.
            ((MethodInfo)(typeof(GenericType<Atom2>).GetMemberWithSameMetadataDefinitionAs(s_genericMethod2)))
                .MakeGenericMethod(typeof(object))
                .Invoke(null, null);
        }
    }

    class TestIsValueTypeWithoutTypeHandle
    {
        [Nothing(
        ReferenceTypes = new[]
        {
            typeof(NonGenericType),
            typeof(GenericType<int>),
            typeof(ReferencedBaseType<int>),
            typeof(GenericWithReferenceBaseType<int>)
        },
        ValueTypes = new[]
        {
            typeof(GenericStruct<int>),
            typeof(NonGenericStruct),
            typeof(Container<int>.GenericEnum)
        })]
        public static void Run()
        {
            var ps = MethodBase.GetCurrentMethod().GetCustomAttribute<NothingAttribute>();
            foreach (var t in ps.ReferenceTypes)
            {
                AssertNoTypeHandle(t);
                Console.WriteLine(t.IsValueType);
            }
            foreach (var t in ps.ValueTypes)
            {
                AssertNoTypeHandle(t);
                Console.WriteLine(t.IsValueType);
            }

            if (!typeof(G<>).GetGenericArguments()[0].IsValueType)
                throw new Exception();

            static void AssertNoTypeHandle(Type t)
            {
                RuntimeTypeHandle h = default;
                try
                {
                    h = t.TypeHandle;
                }
                catch (Exception) { }

                if (!h.Equals(default(RuntimeTypeHandle)))
                    throw new Exception();
            }
        }

        public class GenericBaseType<T> { }
        public class GenericType<T> : GenericBaseType<T> { }
        public class NonGenericType : GenericBaseType<int> { }
        public class ReferencedBaseType<T> { }
        public class GenericWithReferenceBaseType<T> : ReferencedBaseType<T> { }
        public struct GenericStruct<T> { }
        public struct NonGenericStruct { }
        public class Container<T>
        {
            public enum GenericEnum { }
        }

        class NothingAttribute : Attribute
        {
            public Type[] ReferenceTypes;
            public Type[] ValueTypes;
        }

        class G<T> where T : struct { }
    }

    class TestMdArrayLoad
    {
        class Atom { }

        public static Type MakeMdArray<T>() => typeof(T[,,]);

        public static void Run()
        {
            var mi = typeof(TestMdArrayLoad).GetMethod(nameof(MakeMdArray)).MakeGenericMethod(GetAtom());
            if ((Type)mi.Invoke(null, Array.Empty<object>()) != typeof(Atom[,,]))
                throw new Exception();
            static Type GetAtom() => typeof(Atom);
        }
    }

    class TestMdArrayLoad2
    {
        class Atom { }

        public static object MakeMdArray<T>() => new T[1, 1, 1];

        public static void Run()
        {
            var mi = typeof(TestMdArrayLoad2).GetMethod(nameof(MakeMdArray)).MakeGenericMethod(GetAtom());
            if (mi.Invoke(null, Array.Empty<object>()) is not Atom[,,])
                throw new Exception();
            static Type GetAtom() => typeof(Atom);
        }
    }

    class TestByRefTypeLoad
    {
        class Atom { }

        public static Type MakeFnPtrType<T>() => typeof(delegate*<ref T>);

        public static void Run()
        {
            var mi = typeof(TestByRefTypeLoad).GetMethod(nameof(MakeFnPtrType)).MakeGenericMethod(GetAtom());
            if ((Type)mi.Invoke(null, Array.Empty<object>()) != typeof(delegate*<ref Atom>))
                throw new Exception();
            static Type GetAtom() => typeof(Atom);
        }
    }

    class TestGenericLdtoken
    {
        class Base
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static Base GetInstance() => new Base();
            public virtual Type GrabType<T>() => typeof(T);
        }

        class Derived : Base
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static new Base GetInstance() => new Derived();
            public override Type GrabType<T>() => typeof(T);
        }

        class Generic<T> { }

        class Atom { }

        public static void Run()
        {
            Expression<Func<Type>> grabType = () => Base.GetInstance().GrabType<Generic<Atom>>();
            Console.WriteLine(grabType.Compile()().FullName);
        }
    }

    class TestAbstractGenericLdtoken
    {
        abstract class Base
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static Base GetInstance() => null;
            public abstract Type GrabType<T>();
        }

        class Generic<T> { }

        class Atom { }

        public static void Run()
        {
            Expression<Func<Type>> grabType = () => Base.GetInstance().GrabType<Generic<Atom>>();
            try
            {
                grabType.Compile()();
            }
            catch (NullReferenceException)
            {
            }
        }
    }

    class TestTypeHandlesVisibleFromIDynamicInterfaceCastable
    {
        class MyAttribute : Attribute { }

        [My]
        interface IUnusedInterface { }

        class Foo : IDynamicInterfaceCastable
        {
            public RuntimeTypeHandle GetInterfaceImplementation(RuntimeTypeHandle interfaceType) => throw new NotImplementedException();

            public bool IsInterfaceImplemented(RuntimeTypeHandle interfaceType, bool throwIfNotImplemented)
            {
                Type t = Type.GetTypeFromHandle(interfaceType);
                if (t.GetCustomAttribute<MyAttribute>() == null)
                    throw new Exception();

                return true;
            }
        }

        public static void Run()
        {
            object myObject = new Foo();
            if (myObject is not IUnusedInterface)
                throw new Exception();
        }
    }

    class TestEntryPoint
    {
        public static void Run()
        {
            Console.WriteLine(nameof(TestEntryPoint));
            if (Assembly.GetEntryAssembly().EntryPoint == null)
                throw new Exception();
        }
    }

    class TestCompilerGeneratedCode
    {
        class Canary1 { }
        class Canary2 { }
        class Canary3 { }
        class Canary4 { }

        private static void ReflectionInLambda()
        {
            var func = () => {
                Type helpersType = Type.GetType(nameof(ReflectionTest) + "+" + nameof(TestCompilerGeneratedCode) + "+" + nameof(Canary1));
                Assert.NotNull(helpersType);
            };

            func();
        }

        private static void ReflectionInLocalFunction()
        {
            func();

            void func()
            {
                Type helpersType = Type.GetType(nameof(ReflectionTest) + "+" + nameof(TestCompilerGeneratedCode) + "+" + nameof(Canary2));
                Assert.NotNull(helpersType);
            };
        }

        private static async void ReflectionInAsync()
        {
            await System.Threading.Tasks.Task.Delay(100);
            Type helpersType = Type.GetType(nameof(ReflectionTest) + "+" + nameof(TestCompilerGeneratedCode) + "+" + nameof(Canary3));
            Assert.NotNull(helpersType);
        }

        private static async void ReflectionInLambdaAsync()
        {
            await System.Threading.Tasks.Task.Delay(100);

            var func = () => {
                Type helpersType = Type.GetType(nameof(ReflectionTest) + "+" + nameof(TestCompilerGeneratedCode) + "+" + nameof(Canary4));
                Assert.NotNull(helpersType);
            };

            func();
        }

        public static void Run()
        {
            ReflectionInLambda();
            ReflectionInLocalFunction();
            ReflectionInAsync();
            ReflectionInLambdaAsync();
        }
    }

    // Regression test for https://github.com/dotnet/runtime/issues/111578
    class TestGenericAttributesOnEnum
    {
        class MyAttribute<T> : Attribute { }

        [My<int>]
        enum MyEnum { A = 1, B = 2 }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static object GetVal() => MyEnum.A | MyEnum.B;

        public static void Run()
        {
            if (GetVal().ToString() != "3")
                throw new Exception();
        }
    }

    unsafe class TestLdtokenWithSignaturesDifferingInModifiers
    {
        delegate string StdcallDelegate(delegate* unmanaged[Stdcall]<void>[] p);
        delegate string CdeclDelegate(delegate* unmanaged[Cdecl]<void>[] p);

        static string Method(delegate* unmanaged[Stdcall]<void>[] p) => "Stdcall";
        static string Method(delegate* unmanaged[Cdecl]<void>[] p) => "Cdecl";

        public static void Run()
        {
            Expression<StdcallDelegate> stdcall = x => Method(x);
            if (stdcall.Compile()(null) != "Stdcall")
                throw new Exception();

            Expression<CdeclDelegate> cdecl = x => Method(x);
            if (cdecl.Compile()(null) != "Cdecl")
                throw new Exception();
        }
    }

    class TestActivatingThingsInSignature
    {
        public static unsafe void Run()
        {
            var mi = typeof(TestActivatingThingsInSignature).GetMethod(nameof(MethodWithThingsInSignature));
            var p = mi.GetParameters();

            var d = typeof(TestActivatingThingsInSignature).GetMethod(nameof(Run)).CreateDelegate(p[0].ParameterType);
            Console.WriteLine(d.ToString());

            Span<byte> storage = stackalloc byte[sizeof(MyStruct)];
            var s = RuntimeHelpers.Box(ref MemoryMarshal.GetReference(storage), p[1].ParameterType.TypeHandle);
            Console.WriteLine(s.ToString());

            var a = Array.CreateInstanceFromArrayType(p[2].ParameterType, 0);
            Console.WriteLine(a.ToString());
        }

        public void MethodWithThingsInSignature(MyDelegate d, MyStruct s, MyArrayElementStruct[] a) { }

        public delegate void MyDelegate();

        public struct MyStruct;

        public struct MyArrayElementStruct;
    }

    class TestDelegateInvokeFromEvent
    {
        class MyClass
        {
            public event EventHandler<int> MyEvent { add { } remove { } }
        }

        static EventInfo s_eventInfo = typeof(MyClass).GetEvent("MyEvent");

        public static unsafe void Run()
        {
            var invokeMethod = s_eventInfo.EventHandlerType.GetMethod("Invoke");
            if (invokeMethod.Name != "Invoke")
                throw new Exception();
        }
    }

    #region Helpers

    private static Type SecretGetType(string testName, string typeName)
    {
        string fullTypeName = $"{nameof(ReflectionTest)}+{testName}+{typeName}";
        return Type.GetType(fullTypeName);
    }

    private static Type GetTestType(string testName, string typeName)
    {
        Type result = SecretGetType(testName, typeName);
        if (result == null)
            throw new Exception($"'{testName}.{typeName}' could not be located");
        return result;
    }

    private static object InvokeTestMethod(Type type, string methodName, object thisObj = null, params object[] param)
    {
        MethodInfo method = type.GetMethod(methodName);
        if (method == null)
            throw new Exception($"Method '{methodName}' not found on type {type}");

        return method.Invoke(thisObj, param);
    }

    private static bool HasTypeHandle(Type type)
    {
        try
        {
            RuntimeTypeHandle typeHandle = type.TypeHandle;
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070:UnrecognizedReflectionPattern",
        Justification = "That's the point")]
    public static int CountMethods(this Type t)
        => t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Length;

    class Assert
    {
        public static void Equal<T>(T expected, T actual)
        {
            if (object.ReferenceEquals(expected, actual))
                return;

            if ((object)expected == null || (object)actual == null)
                throw new Exception();

            if (!expected.Equals(actual))
                throw new Exception();
        }

        public static void Same<T>(T expected, T actual)
        {
            if (!object.ReferenceEquals(expected, actual))
                throw new Exception();
        }

        public static void Null(object x)
        {
            if (x != null)
                throw new Exception();
        }

        public static void NotNull(object x)
        {
            if (x == null)
                throw new Exception();
        }

        public static void True(bool x)
        {
            if (!x)
                throw new Exception();
        }

        public static void Fail()
        {
            throw new Exception();
        }

        public static void Throws<T>(Action a)
        {
            try
            {
                a();
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(T))
                    throw new Exception();
                return;
            }

            throw new Exception();
        }
    }

    #endregion
}

class TestAssemblyAttribute : Attribute { }
class TestModuleAttribute : Attribute { }
class MyUnusedClass
{
    public static void UnusedMethod1() { }
    public static void TotallyUnreferencedMethod() { }
    public static void GenericMethod<T>() { }
}
class MyClassOnlyReferencedFromTypeGetType { }
