// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace System.Text.Json.Serialization.Tests
{
    public interface ITestClass
    {
        void Initialize();
        void Verify();
    }

    public enum SampleEnumByte : byte
    {
        MinZero = byte.MinValue,
        One = 1,
        Two = 2,
        Max = byte.MaxValue
    }

    public enum SampleEnumSByte : sbyte
    {
        MinNegative = sbyte.MinValue,
        Zero = 0,
        One = 1,
        Two = 2,
        Max = sbyte.MaxValue
    }

    public enum SampleEnum
    {
        MinZero = 0,
        One = 1,
        Two = 2
    }

    public enum SampleEnumInt16 : short
    {
        MinNegative = short.MinValue,
        Zero = 0,
        One = 1,
        Two = 2,
        Max = short.MaxValue
    }

    public enum SampleEnumUInt16 : ushort
    {
        MinZero = ushort.MinValue,
        One = 1,
        Two = 2,
        Max = ushort.MaxValue
    }

    public enum SampleEnumInt32 : Int32
    {
        MinNegative = Int32.MinValue,
        Zero = 0,
        One = 1,
        Two = 2,
        Max = Int32.MaxValue
    }

    public enum SampleEnumUInt32 : UInt32
    {
        MinZero = UInt32.MinValue,
        One = 1,
        Two = 2,
        Max = UInt32.MaxValue
    }

    public enum SampleEnumInt64 : long
    {
        MinNegative = long.MinValue,
        Zero = 0,
        One = 1,
        Two = 2,
        Max = long.MaxValue
    }

    public enum SampleEnumUInt64 : ulong
    {
        MinZero = ulong.MinValue,
        One = 1,
        Two = 2,
        Max = ulong.MaxValue
    }

    public struct SimpleStruct
    {
        public int One { get; set; }
        public double Two { get; set; }
    }

    public struct SimpleStructWithSimpleClass: ITestClass
    {
        public short MyInt32 { get; set; }
        public SimpleTestClass MySimpleClass { get; set; }
        public int[] MyInt32Array { get; set; }

        public static readonly string s_json =
            @"{" +
                @"""MySimpleClass"" : {""MyString"" : ""Hello"", ""MyDouble"" : 3.14}," +
                @"""MyInt32"" : 32," +
                @"""MyInt32Array"" : [32]" +
            @"}";

        public void Initialize()
        {
            MySimpleClass = new SimpleTestClass { MyString = "Hello", MyDouble = 3.14 };
            MyInt32 = 32;
            MyInt32Array = new int[] { 32 };
        }

        public void Verify()
        {
            Assert.Equal(32, MyInt32);
            Assert.Equal(32, MyInt32Array[0]);
            Assert.Equal("Hello", MySimpleClass.MyString);
            Assert.Equal(3.14, MySimpleClass.MyDouble);
        }
    }

    public class TestClassWithNull
    {
        public string? MyString { get; set; }
        public static readonly string s_json =
                @"{" +
                @"""MyString"" : null" +
                @"}";

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(s_json);

        public void Verify()
        {
            Assert.Null(MyString);
        }
    }

    public class TestClassWithInitializedProperties
    {
        public string? MyString { get; set; } = "Hello";
        public int? MyInt { get; set; } = 1;
        public DateTime? MyDateTime { get; set; } = new DateTime(1995, 4, 16);
        public int[]? MyIntArray { get; set; } = new int[] { 1 };
        public List<int>? MyIntList { get; set; } = new List<int> { 1 };
        public List<int?>? MyNullableIntList { get; set; } = new List<int?> { 1 };
        public List<object>? MyObjectList { get; set; } = new List<object> { 1 };
        public List<List<object>>? MyListList { get; set; } = new List<List<object>> { new List<object> { 1 } };
        public List<Dictionary<string, string>>? MyDictionaryList { get; set; } = new List<Dictionary<string, string>> {
            new Dictionary<string, string> { ["key"] = "value" }
        };
        public Dictionary<string, string>? MyStringDictionary { get; set; } = new Dictionary<string, string> { ["key"] = "value" };
        public Dictionary<string, DateTime?>? MyNullableDateTimeDictionary { get; set; } = new Dictionary<string, DateTime?> { ["key"] = new DateTime(1995, 04, 16) };
        public Dictionary<string, object>? MyObjectDictionary { get; set; } = new Dictionary<string, object> { ["key"] = "value" };
        public Dictionary<string, Dictionary<string, string>>? MyStringDictionaryDictionary { get; set; } = new Dictionary<string, Dictionary<string, string>>
        {
            ["key"] = new Dictionary<string, string>
            {
                ["key"] = "value"
            }
        };
        public Dictionary<string, List<object>>? MyListDictionary { get; set; } = new Dictionary<string, List<object>> {
            ["key"] = new List<object> { "value" }
        };
        public Dictionary<string, Dictionary<string, object>>? MyObjectDictionaryDictionary { get; set; } = new Dictionary<string, Dictionary<string, object>>
        {
            ["key"] = new Dictionary<string, object>
            {
                ["key"] = "value"
            }
        };

        public static readonly string s_null_json =
                @"{" +
                    @"""MyString"" : null," +
                    @"""MyInt"" : null," +
                    @"""MyDateTime"" : null," +
                    @"""MyIntArray"" : null," +
                    @"""MyIntList"" : null," +
                    @"""MyNullableIntList"" : null," +
                    @"""MyObjectList"" : [null]," +
                    @"""MyListList"" : [[null]]," +
                    @"""MyDictionaryList"" : [{""key"" : null}]," +
                    @"""MyStringDictionary"" : {""key"" : null}," +
                    @"""MyNullableDateTimeDictionary"" : {""key"" : null}," +
                    @"""MyObjectDictionary"" : {""key"" : null}," +
                    @"""MyStringDictionaryDictionary"" : {""key"" : {""key"" : null}}," +
                    @"""MyListDictionary"" : {""key"" : [null]}," +
                    @"""MyObjectDictionaryDictionary"" : {""key"" : {""key"" : null}}" +
                @"}";

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(s_null_json);
    }

    public class TestClassWithNestedObjectInner : ITestClass
    {
        public SimpleTestClass MyData { get; set; }

        public static readonly string s_json =
            @"{" +
                @"""MyData"":" + SimpleTestClass.s_json +
            @"}";

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(s_json);

        public void Initialize()
        {
            MyData = new SimpleTestClass();
            MyData.Initialize();
        }

        public void Verify()
        {
            Assert.NotNull(MyData);
            MyData.Verify();
        }
    }

    public class TestClassWithNestedObjectOuter : ITestClass
    {
        public TestClassWithNestedObjectInner MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":" + TestClassWithNestedObjectInner.s_json +
            @"}");

        public void Initialize()
        {
            MyData = new TestClassWithNestedObjectInner();
            MyData.Initialize();
        }

        public void Verify()
        {
            Assert.NotNull(MyData);
            MyData.Verify();
        }
    }

    public class TestClassWithObjectList : ITestClass
    {
        public List<SimpleTestClass> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    SimpleTestClass.s_json + "," +
                    "null," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<SimpleTestClass>();

            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyData.Add(obj);
            }

            MyData.Add(null);

            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyData.Add(obj);
            }
        }

        public void Verify()
        {
            Assert.Equal(3, MyData.Count);
            MyData[0].Verify();
            Assert.Null(MyData[1]);
            MyData[2].Verify();
        }
    }

    public class TestClassWithObjectArray : ITestClass
    {
        public SimpleTestClass[] MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            SimpleTestClass obj1 = new SimpleTestClass();
            obj1.Initialize();

            SimpleTestClass obj2 = new SimpleTestClass();
            obj2.Initialize();

            MyData = new SimpleTestClass[2] { obj1, obj2 };
        }

        public void Verify()
        {
            MyData[0].Verify();
            MyData[1].Verify();
            Assert.Equal(2, MyData.Length);
        }
    }

    public class TestClassWithObjectIEnumerableT : ITestClass
    {
        public IEnumerable<SimpleTestClass> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            SimpleTestClass obj1 = new SimpleTestClass();
            obj1.Initialize();

            SimpleTestClass obj2 = new SimpleTestClass();
            obj2.Initialize();

            MyData = new SimpleTestClass[] { obj1, obj2 };
        }

        public void Verify()
        {
            int count = 0;

            foreach (SimpleTestClass data in MyData)
            {
                data.Verify();
                count++;
            }

            Assert.Equal(2, count);
        }
    }

    public class TestClassWithObjectIListT : ITestClass
    {
        public IList<SimpleTestClass> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<SimpleTestClass>();

            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyData.Add(obj);
            }

            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyData.Add(obj);
            }
        }

        public void Verify()
        {
            Assert.Equal(2, MyData.Count);
            MyData[0].Verify();
            MyData[1].Verify();
        }
    }

    public class TestClassWithObjectICollectionT : ITestClass
    {
        public ICollection<SimpleTestClass> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<SimpleTestClass>();

            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyData.Add(obj);
            }

            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyData.Add(obj);
            }
        }

        public void Verify()
        {
            Assert.Equal(2, MyData.Count);

            foreach (SimpleTestClass data in MyData)
            {
                data.Verify();
            }
        }
    }

    public class TestClassWithObjectIEnumerable : ITestClass
    {
        public IEnumerable MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            SimpleTestClass obj1 = new SimpleTestClass();
            obj1.Initialize();

            SimpleTestClass obj2 = new SimpleTestClass();
            obj2.Initialize();

            MyData = new SimpleTestClass[] { obj1, obj2 };
        }

        public void Verify()
        {
            int count = 0;
            foreach (object data in MyData)
            {
                if (data is JsonElement element)
                {
#if BUILDING_SOURCE_GENERATOR_TESTS
                    SimpleTestClass obj = JsonSerializer.Deserialize(element, System.Text.Json.SourceGeneration.Tests.CollectionTests_Default.CollectionTestsContext_Default.Default.SimpleTestClass);
#else
                    SimpleTestClass obj = JsonSerializer.Deserialize<SimpleTestClass>(element);
#endif
                    obj.Verify();
                }
                else
                {
                    ((SimpleTestClass)data).Verify();
                }
                count++;
            }
            Assert.Equal(2, count);
        }
    }

    public class TestClassWithObjectIList : ITestClass
    {
        public IList MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<SimpleTestClass>();

            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyData.Add(obj);
            }

            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyData.Add(obj);
            }
        }

        public void Verify()
        {
            int count = 0;
            foreach (object data in MyData)
            {
                if (data is JsonElement element)
                {
#if BUILDING_SOURCE_GENERATOR_TESTS
                    SimpleTestClass obj = JsonSerializer.Deserialize(element, System.Text.Json.SourceGeneration.Tests.CollectionTests_Default.CollectionTestsContext_Default.Default.SimpleTestClass);
#else
                    SimpleTestClass obj = JsonSerializer.Deserialize<SimpleTestClass>(element);
#endif
                    obj.Verify();
                }
                else
                {
                    ((SimpleTestClass)data).Verify();
                }
                count++;
            }
            Assert.Equal(2, count);
        }
    }

    public class TestClassWithObjectICollection : ITestClass
    {
        public ICollection MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            List<SimpleTestClass> dataTemp = new List<SimpleTestClass>();

            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                dataTemp.Add(obj);
            }

            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                dataTemp.Add(obj);
            }

            MyData = dataTemp;
        }

        public void Verify()
        {
            int count = 0;
            foreach (object data in MyData)
            {
                if (data is JsonElement element)
                {
#if BUILDING_SOURCE_GENERATOR_TESTS
                    SimpleTestClass obj = JsonSerializer.Deserialize(element, System.Text.Json.SourceGeneration.Tests.CollectionTests_Default.CollectionTestsContext_Default.Default.SimpleTestClass);
#else
                    SimpleTestClass obj = JsonSerializer.Deserialize<SimpleTestClass>(element);
#endif
                    obj.Verify();
                }
                else
                {
                    ((SimpleTestClass)data).Verify();
                }
                count++;
            }
            Assert.Equal(2, count);
        }
    }

    public class TestClassWithObjectIReadOnlyCollectionT : ITestClass
    {
        public IReadOnlyCollection<SimpleTestClass> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            SimpleTestClass obj1 = new SimpleTestClass();
            obj1.Initialize();

            SimpleTestClass obj2 = new SimpleTestClass();
            obj2.Initialize();

            MyData = new SimpleTestClass[] { obj1, obj2 };
        }

        public void Verify()
        {
            Assert.Equal(2, MyData.Count);

            foreach (SimpleTestClass data in MyData)
            {
                data.Verify();
            }
        }
    }

    public class TestClassWithObjectIReadOnlyListT : ITestClass
    {
        public IReadOnlyList<SimpleTestClass> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            SimpleTestClass obj1 = new SimpleTestClass();
            obj1.Initialize();

            SimpleTestClass obj2 = new SimpleTestClass();
            obj2.Initialize();

            MyData = new SimpleTestClass[] { obj1, obj2 };
        }

        public void Verify()
        {
            Assert.Equal(2, MyData.Count);
            MyData[0].Verify();
            MyData[1].Verify();
        }
    }

    public class TestClassWithObjectISetT : ITestClass
    {
        public ISet<SimpleTestClass> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            SimpleTestClass obj1 = new SimpleTestClass();
            obj1.Initialize();

            SimpleTestClass obj2 = new SimpleTestClass();
            obj2.Initialize();

            MyData = new HashSet<SimpleTestClass> { obj1, obj2 };
        }

        public void Verify()
        {
            Assert.Equal(2, MyData.Count);

            foreach (SimpleTestClass obj in MyData)
            {
                obj.Verify();
            }
        }
    }

    public class TestClassWithInitializedArray
    {
        public int[] Values { get; set; }

        public TestClassWithInitializedArray()
        {
            Values = Array.Empty<int>();
        }
    }

    public class SimpleClassWithDictionary
    {
        public int MyInt { get; set; }
        public Dictionary<string, string>? MyDictionary { get; set; }
    }
    public class OuterClassHavingPropertiesDefinedAfterClassWithDictionary
    {
        public double MyDouble { get; set; }
        public SimpleClassWithDictionary MyInnerTestClass { get; set; }
        public int MyInt { get; set; }
        public string MyString { get; set; }
        public List<string> MyList { get; set; }
        public int[] MyIntArray { get; set; }
    }

    public class TestClassWithStringArray : ITestClass
    {
        public string[] MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    @"""Hello""," +
                    @"""World""" +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new string[] { "Hello", "World" };
        }

        public void Verify()
        {
            Assert.Equal("Hello", MyData[0]);
            Assert.Equal("World", MyData[1]);
            Assert.Equal(2, MyData.Length);
        }
    }

    public class TestClassWithGenericList : ITestClass
    {
        public List<string> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    @"""Hello""," +
                    @"""World""" +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<string>
            {
                "Hello",
                "World"
            };
            Assert.Equal(2, MyData.Count);
        }

        public void Verify()
        {
            Assert.Equal("Hello", MyData[0]);
            Assert.Equal("World", MyData[1]);
            Assert.Equal(2, MyData.Count);
        }
    }

    public class TestClassWithGenericIEnumerable : ITestClass
    {
        public IEnumerable MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    @"""Hello""," +
                    @"""World""" +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<string>
            {
                "Hello",
                "World"
            };

            int count = 0;
            foreach (string data in MyData)
            {
                count++;
            }
            Assert.Equal(2, count);
        }

        public void Verify()
        {
            string[] expected = { "Hello", "World" };
            int count = 0;
            foreach (object data in MyData)
            {
                if (data is JsonElement element)
                {
                    Assert.Equal(expected[count], element.GetString());
                }
                else
                {
                    Assert.Equal(expected[count], (string)data);
                }
                count++;
            }
            Assert.Equal(2, count);
        }
    }

    public class TestClassWithGenericIList : ITestClass
    {
        public IList MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    @"""Hello""," +
                    @"""World""" +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<string>
            {
                "Hello",
                "World"
            };
            Assert.Equal(2, MyData.Count);
        }

        public void Verify()
        {
            string[] expected = { "Hello", "World" };
            int count = 0;
            foreach (object data in MyData)
            {
                if (data is JsonElement element)
                {
                    Assert.Equal(expected[count], element.GetString());
                }
                else
                {
                    Assert.Equal(expected[count], (string)data);
                }
                count++;
            }
            Assert.Equal(2, count);
        }
    }

    public class TestClassWithGenericICollection : ITestClass
    {
        public ICollection MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    @"""Hello""," +
                    @"""World""" +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<string>
            {
                "Hello",
                "World"
            };
            Assert.Equal(2, MyData.Count);
        }

        public void Verify()
        {
            string[] expected = { "Hello", "World" };
            int count = 0;
            foreach (object data in MyData)
            {
                if (data is JsonElement element)
                {
                    Assert.Equal(expected[count], element.GetString());
                }
                else
                {
                    Assert.Equal(expected[count], (string)data);
                }
                count++;
            }
            Assert.Equal(2, count);
        }
    }

    public class TestClassWithGenericIEnumerableT : ITestClass
    {
        public IEnumerable<string> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    @"""Hello""," +
                    @"""World""" +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<string>
            {
                "Hello",
                "World"
            };

            int count = 0;
            foreach (string data in MyData)
            {
                count++;
            }
            Assert.Equal(2, count);
        }

        public void Verify()
        {
            string[] expected = { "Hello", "World" };
            int count = 0;

            foreach (string data in MyData)
            {
                Assert.Equal(expected[count], data);
                count++;
            }

            Assert.Equal(2, count);
        }
    }

    public class TestClassWithGenericIListT : ITestClass
    {
        public IList<string> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    @"""Hello""," +
                    @"""World""" +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<string>
            {
                "Hello",
                "World"
            };
            Assert.Equal(2, MyData.Count);
        }

        public void Verify()
        {
            Assert.Equal("Hello", MyData[0]);
            Assert.Equal("World", MyData[1]);
            Assert.Equal(2, MyData.Count);
        }
    }

    public class TestClassWithGenericICollectionT : ITestClass
    {
        public ICollection<string> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    @"""Hello""," +
                    @"""World""" +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<string>
            {
                "Hello",
                "World"
            };
            Assert.Equal(2, MyData.Count);
        }

        public void Verify()
        {
            string[] expected = { "Hello", "World" };
            int i = 0;

            foreach (string data in MyData)
            {
                Assert.Equal(expected[i++], data);
            }

            Assert.Equal(2, MyData.Count);
        }
    }

    public class TestClassWithGenericIReadOnlyCollectionT : ITestClass
    {
        public IReadOnlyCollection<string> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    @"""Hello""," +
                    @"""World""" +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<string>
            {
                "Hello",
                "World"
            };
            Assert.Equal(2, MyData.Count);
        }

        public void Verify()
        {
            string[] expected = { "Hello", "World" };
            int i = 0;

            foreach (string data in MyData)
            {
                Assert.Equal(expected[i++], data);
            }

            Assert.Equal(2, MyData.Count);
        }
    }

    public class TestClassWithGenericIReadOnlyListT : ITestClass
    {
        public IReadOnlyList<string> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    @"""Hello""," +
                    @"""World""" +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new List<string>
            {
                "Hello",
                "World"
            };
            Assert.Equal(2, MyData.Count);
        }

        public void Verify()
        {
            Assert.Equal("Hello", MyData[0]);
            Assert.Equal("World", MyData[1]);
            Assert.Equal(2, MyData.Count);
        }
    }

    public class TestClassWithGenericISetT : ITestClass
    {
        public ISet<string> MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyData"":[" +
                    @"""Hello""," +
                    @"""World""" +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyData = new HashSet<string>
            {
                "Hello",
                "World"
            };
            Assert.Equal(2, MyData.Count);
        }

        public void Verify()
        {
            Assert.Equal(2, MyData.Count);

            bool helloSeen = false;
            bool worldSeen = false;

            foreach (string data in MyData)
            {
                if (data == "Hello")
                {
                    helloSeen = true;
                }
                else if (data == "World")
                {
                    worldSeen = true;
                }
            }

            Assert.True(helloSeen && worldSeen);
        }
    }

    public class TestClassWithStringToPrimitiveDictionary : ITestClass
    {
        public Dictionary<string, int> MyInt32Dict { get; set; }
        public Dictionary<string, bool> MyBooleanDict { get; set; }
        public Dictionary<string, float> MySingleDict { get; set; }
        public Dictionary<string, double> MyDoubleDict { get; set; }
        public Dictionary<string, DateTime> MyDateTimeDict { get; set; }
        public IDictionary<string, int> MyInt32IDict { get; set; }
        public IDictionary<string, bool> MyBooleanIDict { get; set; }
        public IDictionary<string, float> MySingleIDict { get; set; }
        public IDictionary<string, double> MyDoubleIDict { get; set; }
        public IDictionary<string, DateTime> MyDateTimeIDict { get; set; }
        public IReadOnlyDictionary<string, int> MyInt32IReadOnlyDict { get; set; }
        public IReadOnlyDictionary<string, bool> MyBooleanIReadOnlyDict { get; set; }
        public IReadOnlyDictionary<string, float> MySingleIReadOnlyDict { get; set; }
        public IReadOnlyDictionary<string, double> MyDoubleIReadOnlyDict { get; set; }
        public IReadOnlyDictionary<string, DateTime> MyDateTimeIReadOnlyDict { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyInt32Dict"":{" +
                    @"""key1"": 1," +
                    @"""key2"": 2" +
                @"}," +
                @"""MyBooleanDict"":{" +
                    @"""key1"": true," +
                    @"""key2"": false" +
                @"}," +
                @"""MySingleDict"":{" +
                    @"""key1"": 1.1," +
                    @"""key2"": 2.2" +
                @"}," +
                @"""MyDoubleDict"":{" +
                    @"""key1"": 3.3," +
                    @"""key2"": 4.4" +
                @"}," +
                @"""MyDateTimeDict"":{" +
                    @"""key1"": ""2019-01-30T12:01:02.0000000""," +
                    @"""key2"": ""2019-01-30T12:01:02.0000000Z""" +
                @"}," +
                @"""MyInt32IDict"":{" +
                    @"""key1"": 1," +
                    @"""key2"": 2" +
                @"}," +
                @"""MyBooleanIDict"":{" +
                    @"""key1"": true," +
                    @"""key2"": false" +
                @"}," +
                @"""MySingleIDict"":{" +
                    @"""key1"": 1.1," +
                    @"""key2"": 2.2" +
                @"}," +
                @"""MyDoubleIDict"":{" +
                    @"""key1"": 3.3," +
                    @"""key2"": 4.4" +
                @"}," +
                @"""MyDateTimeIDict"":{" +
                    @"""key1"": ""2019-01-30T12:01:02.0000000""," +
                    @"""key2"": ""2019-01-30T12:01:02.0000000Z""" +
                @"}," +
                @"""MyInt32IReadOnlyDict"":{" +
                    @"""key1"": 1," +
                    @"""key2"": 2" +
                @"}," +
                @"""MyBooleanIReadOnlyDict"":{" +
                    @"""key1"": true," +
                    @"""key2"": false" +
                @"}," +
                @"""MySingleIReadOnlyDict"":{" +
                    @"""key1"": 1.1," +
                    @"""key2"": 2.2" +
                @"}," +
                @"""MyDoubleIReadOnlyDict"":{" +
                    @"""key1"": 3.3," +
                    @"""key2"": 4.4" +
                @"}," +
                @"""MyDateTimeIReadOnlyDict"":{" +
                    @"""key1"": ""2019-01-30T12:01:02.0000000""," +
                    @"""key2"": ""2019-01-30T12:01:02.0000000Z""" +
                @"}" +
            @"}");

        public void Initialize()
        {
            MyInt32Dict = new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } };
            MyBooleanDict = new Dictionary<string, bool> { { "key1", true }, { "key2", false } };
            MySingleDict = new Dictionary<string, float> { { "key1", 1.1f }, { "key2", 2.2f } };
            MyDoubleDict = new Dictionary<string, double> { { "key1", 3.3d }, { "key2", 4.4d } };
            MyDateTimeDict = new Dictionary<string, DateTime> { { "key1", new DateTime(2019, 1, 30, 12, 1, 2) }, { "key2", new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc) } };

            MyInt32IDict = new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } };
            MyBooleanIDict = new Dictionary<string, bool> { { "key1", true }, { "key2", false } };
            MySingleIDict = new Dictionary<string, float> { { "key1", 1.1f }, { "key2", 2.2f } };
            MyDoubleIDict = new Dictionary<string, double> { { "key1", 3.3d }, { "key2", 4.4d } };
            MyDateTimeIDict = new Dictionary<string, DateTime> { { "key1", new DateTime(2019, 1, 30, 12, 1, 2) }, { "key2", new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc) } };

            MyInt32IReadOnlyDict = new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } };
            MyBooleanIReadOnlyDict = new Dictionary<string, bool> { { "key1", true }, { "key2", false } };
            MySingleIReadOnlyDict = new Dictionary<string, float> { { "key1", 1.1f }, { "key2", 2.2f } };
            MyDoubleIReadOnlyDict = new Dictionary<string, double> { { "key1", 3.3d }, { "key2", 4.4d } };
            MyDateTimeIReadOnlyDict = new Dictionary<string, DateTime> { { "key1", new DateTime(2019, 1, 30, 12, 1, 2) }, { "key2", new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc) } };
        }

        public void Verify()
        {
            Assert.Equal(1, MyInt32Dict["key1"]);
            Assert.Equal(2, MyInt32Dict["key2"]);
            Assert.Equal(2, MyInt32Dict.Count);

            Assert.True(MyBooleanDict["key1"]);
            Assert.False(MyBooleanDict["key2"]);
            Assert.Equal(2, MyBooleanDict.Count);

            Assert.Equal(1.1f, MySingleDict["key1"]);
            Assert.Equal(2.2f, MySingleDict["key2"]);
            Assert.Equal(2, MySingleDict.Count);

            Assert.Equal(3.3d, MyDoubleDict["key1"]);
            Assert.Equal(4.4d, MyDoubleDict["key2"]);
            Assert.Equal(2, MyDoubleDict.Count);

            Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2), MyDateTimeDict["key1"]);
            Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc), MyDateTimeDict["key2"]);
            Assert.Equal(2, MyDateTimeDict.Count);

            Assert.Equal(1, MyInt32IDict["key1"]);
            Assert.Equal(2, MyInt32IDict["key2"]);
            Assert.Equal(2, MyInt32IDict.Count);

            Assert.True(MyBooleanIDict["key1"]);
            Assert.False(MyBooleanIDict["key2"]);
            Assert.Equal(2, MyBooleanIDict.Count);

            Assert.Equal(1.1f, MySingleIDict["key1"]);
            Assert.Equal(2.2f, MySingleIDict["key2"]);
            Assert.Equal(2, MySingleIDict.Count);

            Assert.Equal(3.3d, MyDoubleIDict["key1"]);
            Assert.Equal(4.4d, MyDoubleIDict["key2"]);
            Assert.Equal(2, MyDoubleIDict.Count);

            Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2), MyDateTimeIDict["key1"]);
            Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc), MyDateTimeIDict["key2"]);
            Assert.Equal(2, MyDateTimeIDict.Count);

            Assert.Equal(1, MyInt32IReadOnlyDict["key1"]);
            Assert.Equal(2, MyInt32IReadOnlyDict["key2"]);
            Assert.Equal(2, MyInt32IReadOnlyDict.Count);

            Assert.True(MyBooleanIReadOnlyDict["key1"]);
            Assert.False(MyBooleanIReadOnlyDict["key2"]);
            Assert.Equal(2, MyBooleanIReadOnlyDict.Count);

            Assert.Equal(1.1f, MySingleIReadOnlyDict["key1"]);
            Assert.Equal(2.2f, MySingleIReadOnlyDict["key2"]);
            Assert.Equal(2, MySingleIReadOnlyDict.Count);

            Assert.Equal(3.3d, MyDoubleIReadOnlyDict["key1"]);
            Assert.Equal(4.4d, MyDoubleIReadOnlyDict["key2"]);
            Assert.Equal(2, MyDoubleIReadOnlyDict.Count);

            Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2), MyDateTimeIReadOnlyDict["key1"]);
            Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc), MyDateTimeIReadOnlyDict["key2"]);
            Assert.Equal(2, MyDateTimeIReadOnlyDict.Count);
        }
    }

    public class TestClassWithObjectIEnumerableConstructibleTypes : ITestClass
    {
        public Stack<SimpleTestClass> MyStack { get; set; }
        public Queue<SimpleTestClass> MyQueue { get; set; }
        public HashSet<SimpleTestClass> MyHashSet { get; set; }
        public LinkedList<SimpleTestClass> MyLinkedList { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyStack"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]," +
                @"""MyQueue"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]," +
                @"""MyHashSet"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]," +
                @"""MyLinkedList"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            MyStack = new Stack<SimpleTestClass>();
            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyStack.Push(obj);
            }
            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyStack.Push(obj);
            }

            MyQueue = new Queue<SimpleTestClass>();
            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyQueue.Enqueue(obj);
            }
            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyQueue.Enqueue(obj);
            }

            MyHashSet = new HashSet<SimpleTestClass>();
            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyHashSet.Add(obj);
            }
            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyHashSet.Add(obj);
            }

            MyLinkedList = new LinkedList<SimpleTestClass>();
            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyLinkedList.AddLast(obj);
            }
            {
                SimpleTestClass obj = new SimpleTestClass();
                obj.Initialize();
                MyLinkedList.AddLast(obj);
            }
        }

        public void Verify()
        {
            Assert.Equal(2, MyStack.Count);
            foreach (SimpleTestClass data in MyStack)
            {
                data.Verify();
            }

            Assert.Equal(2, MyQueue.Count);
            foreach (SimpleTestClass data in MyQueue)
            {
                data.Verify();
            }

            Assert.Equal(2, MyHashSet.Count);
            foreach (SimpleTestClass data in MyHashSet)
            {
                data.Verify();
            }

            Assert.Equal(2, MyLinkedList.Count);
            foreach (SimpleTestClass data in MyLinkedList)
            {
                data.Verify();
            }
        }
    }

    public class TestClassWithObjectImmutableTypes : ITestClass
    {
        public ImmutableArray<SimpleTestClass> MyImmutableArray { get; set; }
        public IImmutableList<SimpleTestClass> MyIImmutableList { get; set; }
        public IImmutableStack<SimpleTestClass> MyIImmutableStack { get; set; }
        public IImmutableQueue<SimpleTestClass> MyIImmutableQueue { get; set; }
        public IImmutableSet<SimpleTestClass> MyIImmutableSet { get; set; }
        public ImmutableHashSet<SimpleTestClass> MyImmutableHashSet { get; set; }
        public ImmutableList<SimpleTestClass> MyImmutableList { get; set; }
        public ImmutableStack<SimpleTestClass> MyImmutableStack { get; set; }
        public ImmutableQueue<SimpleTestClass> MyImmutableQueue { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                @"""MyImmutableArray"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]," +
                @"""MyIImmutableList"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]," +
                @"""MyIImmutableStack"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]," +
                @"""MyIImmutableQueue"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]," +
                @"""MyIImmutableSet"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]," +
                @"""MyImmutableHashSet"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]," +
                @"""MyImmutableList"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]," +
                @"""MyImmutableStack"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]," +
                @"""MyImmutableQueue"":[" +
                    SimpleTestClass.s_json + "," +
                    SimpleTestClass.s_json +
                @"]" +
            @"}");

        public void Initialize()
        {
            {
                SimpleTestClass obj1 = new SimpleTestClass();
                obj1.Initialize();

                SimpleTestClass obj2 = new SimpleTestClass();
                obj2.Initialize();

                MyImmutableArray = ImmutableArray.CreateRange(new List<SimpleTestClass> { obj1, obj2 });
            }
            {
                SimpleTestClass obj1 = new SimpleTestClass();
                obj1.Initialize();

                SimpleTestClass obj2 = new SimpleTestClass();
                obj2.Initialize();

                MyIImmutableList = ImmutableList.CreateRange(new List<SimpleTestClass> { obj1, obj2 });
            }
            {
                SimpleTestClass obj1 = new SimpleTestClass();
                obj1.Initialize();

                SimpleTestClass obj2 = new SimpleTestClass();
                obj2.Initialize();

                MyIImmutableStack = ImmutableStack.CreateRange(new List<SimpleTestClass> { obj1, obj2 });
            }
            {
                SimpleTestClass obj1 = new SimpleTestClass();
                obj1.Initialize();

                SimpleTestClass obj2 = new SimpleTestClass();
                obj2.Initialize();

                MyIImmutableQueue = ImmutableQueue.CreateRange(new List<SimpleTestClass> { obj1, obj2 });
            }
            {
                SimpleTestClass obj1 = new SimpleTestClass();
                obj1.Initialize();

                SimpleTestClass obj2 = new SimpleTestClass();
                obj2.Initialize();

                MyIImmutableSet = ImmutableHashSet.CreateRange(new List<SimpleTestClass> { obj1, obj2 });
            }
            {
                SimpleTestClass obj1 = new SimpleTestClass();
                obj1.Initialize();

                SimpleTestClass obj2 = new SimpleTestClass();
                obj2.Initialize();

                MyImmutableHashSet = ImmutableHashSet.CreateRange(new List<SimpleTestClass> { obj1, obj2 });
            }
            {
                SimpleTestClass obj1 = new SimpleTestClass();
                obj1.Initialize();

                SimpleTestClass obj2 = new SimpleTestClass();
                obj2.Initialize();

                MyImmutableList = ImmutableList.CreateRange(new List<SimpleTestClass> { obj1, obj2 });
            }
            {
                SimpleTestClass obj1 = new SimpleTestClass();
                obj1.Initialize();

                SimpleTestClass obj2 = new SimpleTestClass();
                obj2.Initialize();

                MyImmutableStack = ImmutableStack.CreateRange(new List<SimpleTestClass> { obj1, obj2 });
            }
            {
                SimpleTestClass obj1 = new SimpleTestClass();
                obj1.Initialize();

                SimpleTestClass obj2 = new SimpleTestClass();
                obj2.Initialize();

                MyImmutableQueue = ImmutableQueue.CreateRange(new List<SimpleTestClass> { obj1, obj2 });
            }
        }

        public void Verify()
        {
            Assert.Equal(2, MyImmutableArray.Length);
            foreach (SimpleTestClass data in MyImmutableArray)
            {
                data.Verify();
            }

            Assert.Equal(2, MyIImmutableList.Count);
            foreach (SimpleTestClass data in MyIImmutableList)
            {
                data.Verify();
            }

            int i = 0;
            foreach (SimpleTestClass data in MyIImmutableStack)
            {
                data.Verify();
                i++;
            }
            Assert.Equal(2, i);

            i = 0;
            foreach (SimpleTestClass data in MyIImmutableQueue)
            {
                data.Verify();
                i++;
            }
            Assert.Equal(2, i);

            Assert.Equal(2, MyIImmutableSet.Count);
            foreach (SimpleTestClass data in MyIImmutableSet)
            {
                data.Verify();
            }

            Assert.Equal(2, MyImmutableHashSet.Count);
            foreach (SimpleTestClass data in MyImmutableHashSet)
            {
                data.Verify();
            }

            Assert.Equal(2, MyImmutableList.Count);
            foreach (SimpleTestClass data in MyImmutableList)
            {
                data.Verify();
            }

            i = 0;
            foreach (SimpleTestClass data in MyImmutableStack)
            {
                data.Verify();
                i++;
            }
            Assert.Equal(2, i);

            i = 0;
            foreach (SimpleTestClass data in MyImmutableQueue)
            {
                data.Verify();
                i++;
            }
            Assert.Equal(2, i);
        }
    }

    public class SimpleDerivedTestClass : SimpleTestClass
    {
    }

    public class OverridePropertyNameRuntime_TestClass
    {
        public Int16 MyInt16 { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
            @"""blah"" : 1" +
            @"}"
        );
    }

    public class LargeDataTestClass : ITestClass
    {
        public List<LargeDataChildTestClass> Children { get; set; } = new List<LargeDataChildTestClass>();
        public const int ChildrenCount = 10;

        public string MyString { get; set; }
        public const int MyStringLength = 1000;

        public void Initialize()
        {
            MyString = new string('1', MyStringLength);

            for (int i = 0; i < ChildrenCount; i++)
            {
                var child = new LargeDataChildTestClass
                {
                    MyString = new string('2', LargeDataChildTestClass.MyStringLength),
                    MyStringArray = new string[LargeDataChildTestClass.MyStringArrayArrayCount]
                };
                for (int j = 0; j < child.MyStringArray.Length; j++)
                {
                    child.MyStringArray[j] = new string('3', LargeDataChildTestClass.MyStringArrayElementStringLength);
                }

                Children.Add(child);
            }
        }

        public void Verify()
        {
            Assert.Equal(MyStringLength, MyString.Length);
            Assert.Equal('1', MyString[0]);
            Assert.Equal('1', MyString[MyStringLength - 1]);

            Assert.Equal(ChildrenCount, Children.Count);
            for (int i = 0; i < ChildrenCount; i++)
            {
                LargeDataChildTestClass child = Children[i];
                Assert.Equal(LargeDataChildTestClass.MyStringLength, child.MyString.Length);
                Assert.Equal('2', child.MyString[0]);
                Assert.Equal('2', child.MyString[LargeDataChildTestClass.MyStringLength - 1]);

                Assert.Equal(LargeDataChildTestClass.MyStringArrayArrayCount, child.MyStringArray.Length);
                for (int j = 0; j < LargeDataChildTestClass.MyStringArrayArrayCount; j++)
                {
                    Assert.Equal('3', child.MyStringArray[j][0]);
                    Assert.Equal('3', child.MyStringArray[j][LargeDataChildTestClass.MyStringArrayElementStringLength - 1]);
                }
            }
        }
    }

    public class LargeDataChildTestClass
    {
        public const int MyStringLength = 2000;
        public string MyString { get; set; }

        public string[] MyStringArray { get; set; }
        public const int MyStringArrayArrayCount = 1000;
        public const int MyStringArrayElementStringLength = 50;
    }

    public class EmptyClass { }

    public class BasicPerson : ITestClass
    {
        public int age { get; set; }
        public string first { get; set; }
        public string last { get; set; }
        public List<string> phoneNumbers { get; set; }
        public BasicJsonAddress address { get; set; }

        public void Initialize()
        {
            age = 30;
            first = "John";
            last = "Smith";
            phoneNumbers = new List<string> { "425-000-0000", "425-000-0001" };
            address = new BasicJsonAddress
            {
                street = "1 Microsoft Way",
                city = "Redmond",
                zip = 98052
            };
        }

        public void Verify()
        {
            Assert.Equal(30, age);
            Assert.Equal("John", first);
            Assert.Equal("Smith", last);
            Assert.Equal("425-000-0000", phoneNumbers[0]);
            Assert.Equal("425-000-0001", phoneNumbers[1]);
            Assert.Equal("1 Microsoft Way", address.street);
            Assert.Equal("Redmond", address.city);
            Assert.Equal(98052, address.zip);
        }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            "{" +
                @"""age"" : 30," +
                @"""first"" : ""John""," +
                @"""last"" : ""Smith""," +
                @"""phoneNumbers"" : [" +
                    @"""425-000-0000""," +
                    @"""425-000-0001""" +
                @"]," +
                @"""address"" : {" +
                    @"""street"" : ""1 Microsoft Way""," +
                    @"""city"" : ""Redmond""," +
                    @"""zip"" : 98052" +
                "}" +
            "}");
    }

    public class BasicJsonAddress
    {
        public string street { get; set; }
        public string city { get; set; }
        public int zip { get; set; }
    }

    public class BasicCompany : ITestClass
    {
        public List<BasicJsonAddress>? sites { get; set; }
        public BasicJsonAddress? mainSite { get; set; }
        public string? name { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            "{\n" +
                @"""name"" : ""Microsoft""," + "\n" +
                @"""sites"" :[" + "\n" +
                    "{\n" +
                        @"""street"" : ""1 Lone Tree Rd S""," + "\n" +
                        @"""city"" : ""Fargo""," + "\n" +
                        @"""zip"" : 58104" + "\n" +
                    "},\n" +
                    "{\n" +
                        @"""street"" : ""8055 Microsoft Way""," + "\n" +
                        @"""city"" : ""Charlotte""," + "\n" +
                        @"""zip"" : 28273" + "\n" +
                    "}\n" +
                "],\n" +
                @"""mainSite"":" + "\n" +
                    "{\n" +
                        @"""street"" : ""1 Microsoft Way""," + "\n" +
                        @"""city"" : ""Redmond""," + "\n" +
                        @"""zip"" : 98052" + "\n" +
                    "}\n" +
            "}");

        public void Initialize()
        {
            name = "Microsoft";
            sites = new List<BasicJsonAddress>
            {
                new BasicJsonAddress
                {
                    street = "1 Lone Tree Rd S",
                    city = "Fargo",
                    zip = 58104
                },
                new BasicJsonAddress
                {
                    street = "8055 Microsoft Way",
                    city = "Charlotte",
                    zip = 28273
                }
            };

            mainSite =
                new BasicJsonAddress
                {
                    street = "1 Microsoft Way",
                    city = "Redmond",
                    zip = 98052
                };
        }

        public void Verify()
        {
            Assert.Equal("Microsoft", name);
            Assert.Equal("1 Lone Tree Rd S", sites[0].street);
            Assert.Equal("Fargo", sites[0].city);
            Assert.Equal(58104, sites[0].zip);
            Assert.Equal("8055 Microsoft Way", sites[1].street);
            Assert.Equal("Charlotte", sites[1].city);
            Assert.Equal(28273, sites[1].zip);
            Assert.Equal("1 Microsoft Way", mainSite.street);
            Assert.Equal("Redmond", mainSite.city);
            Assert.Equal(98052, mainSite.zip);
        }
    }

    public class ClassWithUnicodeProperty
    {
        public int A\u0467 { get; set; }

        // A 400 character property name with a unicode character making it 401 bytes.
        public int A\u046734567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890 { get; set; }
    }

    public class ClassWithExtensionProperty
    {
        public SimpleTestClass? MyNestedClass { get; set; }
        public int MyInt { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JsonElement> MyOverflow { get; set; }
    }

    public class ClassWithExtensionField
    {
        public SimpleTestClass? MyNestedClass { get; set; }
        public int MyInt { get; set; }

        [JsonInclude]
        [JsonExtensionData]
        public IDictionary<string, JsonElement> MyOverflow;
    }

    public class TestClassWithNestedObjectCommentsInner : ITestClass
    {
        public SimpleTestClass MyData { get; set; }

        public static readonly string s_json =
            @"{" +
                @"""MyData"":" + SimpleTestClass.s_json + " // Trailing comment\n" +
                "/* Multi\nLine Comment with } */\n" +
            @"}";

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(s_json);

        public void Initialize()
        {
            MyData = new SimpleTestClass();
            MyData.Initialize();
        }

        public void Verify()
        {
            Assert.NotNull(MyData);
            MyData.Verify();
        }
    }

    public class TestClassWithNestedObjectCommentsOuter : ITestClass
    {
        public TestClassWithNestedObjectCommentsInner MyData { get; set; }

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(
            @"{" +
                " // This } will be ignored\n" +
                @"""MyData"":" + TestClassWithNestedObjectCommentsInner.s_json +
                " /* As will this [ */\n" +
            @"}");

        public void Initialize()
        {
            MyData = new TestClassWithNestedObjectCommentsInner();
            MyData.Initialize();
        }

        public void Verify()
        {
            Assert.NotNull(MyData);
            MyData.Verify();
        }
    }

    public class ClassWithType<T>
    {
        public T Prop { get; set; }
    }

    public class ConverterForInt32 : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return 25;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Converter was called");
        }

        // In source-gen, internal converters are not used as fallbacks when custom converters don't provide an implementation.
#if BUILDING_SOURCE_GENERATOR_TESTS
        public override int ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => int.Parse(reader.GetString());

        public override void WriteAsPropertyName(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            => writer.WritePropertyName(value.ToString());
#endif
    }

    public static class ReflectionExtensions
    {
#if NET
        [return: System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
        public static Type WithConstructors(
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
            this Type type) => type;
#else
        public static Type WithConstructors(this Type type) => type;
#endif
    }

    public static class CollectionTestTypes
    {
        public static IEnumerable<Type> EnumerableTypes<TElement>()
        {
            yield return typeof(TElement[]).WithConstructors(); // ArrayConverter
            yield return typeof(ConcurrentQueue<TElement>).WithConstructors(); // ConcurrentQueueOfTConverter
            yield return typeof(GenericICollectionWrapper<TElement>).WithConstructors(); // ICollectionOfTConverter
            yield return typeof(WrapperForIEnumerable).WithConstructors(); // IEnumerableConverter
            yield return typeof(WrapperForIReadOnlyCollectionOfT<TElement>).WithConstructors(); // IEnumerableOfTConverter
            yield return typeof(Queue).WithConstructors(); // IEnumerableWithAddMethodConverter
            yield return typeof(WrapperForIList).WithConstructors(); // IListConverter
            yield return typeof(Collection<TElement>).WithConstructors(); // IListOfTConverter
            yield return typeof(ImmutableList<TElement>).WithConstructors(); // ImmutableEnumerableOfTConverter
            yield return typeof(HashSet<TElement>).WithConstructors(); // ISetOfTConverter
            yield return typeof(List<TElement>).WithConstructors(); // ListOfTConverter
            yield return typeof(Queue<TElement>).WithConstructors(); // QueueOfTConverter
        }

        public static IEnumerable<Type> DeserializableGenericEnumerableTypes<TElement>()
        {
            yield return typeof(TElement[]).WithConstructors(); // ArrayConverter
            yield return typeof(ConcurrentQueue<TElement>).WithConstructors(); // ConcurrentQueueOfTConverter
            yield return typeof(GenericICollectionWrapper<TElement>).WithConstructors(); // ICollectionOfTConverter
            yield return typeof(IEnumerable<TElement>).WithConstructors(); // IEnumerableConverter
            yield return typeof(Collection<TElement>).WithConstructors(); // IListOfTConverter
            yield return typeof(ImmutableList<TElement>).WithConstructors(); // ImmutableEnumerableOfTConverter
            yield return typeof(HashSet<TElement>).WithConstructors(); // ISetOfTConverter
            yield return typeof(List<TElement>).WithConstructors(); // ListOfTConverter
            yield return typeof(Queue<TElement>).WithConstructors(); // QueueOfTConverter
        }

        public static IEnumerable<Type> DeserializableNonGenericEnumerableTypes()
        {
            yield return typeof(Queue).WithConstructors(); // IEnumerableWithAddMethodConverter
            yield return typeof(WrapperForIList).WithConstructors(); // IListConverter
        }

        public static IEnumerable<Type> DictionaryTypes<TElement>()
        {
            yield return typeof(Dictionary<string, TElement>).WithConstructors(); // DictionaryOfStringTValueConverter
            yield return typeof(Hashtable).WithConstructors(); // IDictionaryConverter
            yield return typeof(ConcurrentDictionary<string, TElement>).WithConstructors(); // IDictionaryOfStringTValueConverter
            yield return typeof(GenericIDictionaryWrapper<string, TElement>).WithConstructors(); // IDictionaryOfStringTValueConverter
            yield return typeof(ImmutableDictionary<string, TElement>).WithConstructors(); // ImmutableDictionaryOfStringTValueConverter
            yield return typeof(GenericIReadOnlyDictionaryWrapper<string, TElement>).WithConstructors(); // IReadOnlyDictionaryOfStringTValueConverter
        }

        public static IEnumerable<Type> DeserializableDictionaryTypes<TKey, TValue>()
        {
            yield return typeof(Dictionary<TKey, TValue>).WithConstructors(); // DictionaryOfStringTValueConverter
            yield return typeof(Hashtable).WithConstructors(); // IDictionaryConverter
            yield return typeof(IDictionary).WithConstructors(); // IDictionaryConverter
            yield return typeof(ConcurrentDictionary<TKey, TValue>).WithConstructors(); // IDictionaryOfStringTValueConverter
            yield return typeof(IDictionary<TKey, TValue>).WithConstructors(); // IDictionaryOfStringTValueConverter
            yield return typeof(GenericIDictionaryWrapper<TKey, TValue>).WithConstructors(); // IDictionaryOfStringTValueConverter
            yield return typeof(ImmutableDictionary<TKey, TValue>).WithConstructors(); // ImmutableDictionaryOfStringTValueConverter
            yield return typeof(IReadOnlyDictionary<TKey, TValue>).WithConstructors(); // IReadOnlyDictionaryOfStringTValueConverter
        }

        public static IEnumerable<Type> DeserializableNonGenericDictionaryTypes()
        {
            yield return typeof(Hashtable).WithConstructors(); // IDictionaryConverter
            yield return typeof(SortedList).WithConstructors(); // IDictionaryConverter
        }
    }

    public class PocoDictionary
    {
        public Dictionary<string, string> key { get; set; }
    }

    public class Poco
    {
        public int Id { get; set; }
    }

    public class UppercaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name.ToUpperInvariant();
        }
    }

    public class NullNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return null;
        }
    }

    public class PersonReference
    {
        internal Guid Id { get; set; }
        public string Name { get; set; }
        public PersonReference Spouse { get; set; }
    }

    public class JsonElementClass : ITestClass
    {
        public JsonElement Number { get; set; }
        public JsonElement True { get; set; }
        public JsonElement False { get; set; }
        public JsonElement String { get; set; }
        public JsonElement Array { get; set; }
        public JsonElement Object { get; set; }
        public JsonElement Null { get; set; }

        public static readonly string s_json =
            @"{" +
                @"""Number"" : 1," +
                @"""True"" : true," +
                @"""False"" : false," +
                @"""String"" : ""Hello""," +
                @"""Array"" : [2, false, true, ""Goodbye""]," +
                @"""Object"" : {}," +
                @"""Null"" : null" +
            @"}";

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(s_json);

        public void Initialize()
        {
            Number = JsonElement.Parse(@"1");
            True = JsonElement.Parse(@"true");
            False = JsonElement.Parse(@"false");
            String = JsonElement.Parse(@"""Hello""");
            Array = JsonElement.Parse(@"[2, false, true, ""Goodbye""]");
            Object = JsonElement.Parse(@"{}");
            Null = JsonElement.Parse(@"null");
        }

        public void Verify()
        {
            Assert.Equal(JsonValueKind.Number, Number.ValueKind);
            Assert.Equal("1", Number.ToString());
            Assert.Equal(JsonValueKind.True, True.ValueKind);
            Assert.Equal("True", True.ToString());
            Assert.Equal(JsonValueKind.False, False.ValueKind);
            Assert.Equal("False", False.ToString());
            Assert.Equal(JsonValueKind.String, String.ValueKind);
            Assert.Equal("Hello", String.ToString());
            Assert.Equal(JsonValueKind.Array, Array.ValueKind);
            JsonElement[] elements = Array.EnumerateArray().ToArray();
            Assert.Equal(JsonValueKind.Number, elements[0].ValueKind);
            Assert.Equal("2", elements[0].ToString());
            Assert.Equal(JsonValueKind.False, elements[1].ValueKind);
            Assert.Equal("False", elements[1].ToString());
            Assert.Equal(JsonValueKind.True, elements[2].ValueKind);
            Assert.Equal("True", elements[2].ToString());
            Assert.Equal(JsonValueKind.String, elements[3].ValueKind);
            Assert.Equal("Goodbye", elements[3].ToString());
            Assert.Equal(JsonValueKind.Object, Object.ValueKind);
            Assert.Equal("{}", Object.ToString());
            Assert.Equal(JsonValueKind.Null, Null.ValueKind);
            Assert.Equal("", Null.ToString()); // JsonElement returns empty string for null.
        }
    }

    public class JsonElementArrayClass : ITestClass
    {
        public JsonElement[] Array { get; set; }

        public static readonly string s_json =
            @"{" +
                @"""Array"" : [" +
                    @"1, " +
                    @"true, " +
                    @"false, " +
                    @"""Hello""," +
                    @"[2, false, true, ""Goodbye""]," +
                    @"{}" +
                @"]" +
            @"}";

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(s_json);

        public void Initialize()
        {
            Array = new JsonElement[]
            {
                JsonElement.Parse(@"1"),
                JsonElement.Parse(@"true"),
                JsonElement.Parse(@"false"),
                JsonElement.Parse(@"""Hello""")
            };
        }

        public void Verify()
        {
            Assert.Equal(JsonValueKind.Number, Array[0].ValueKind);
            Assert.Equal("1", Array[0].ToString());
            Assert.Equal(JsonValueKind.True, Array[1].ValueKind);
            Assert.Equal("True", Array[1].ToString());
            Assert.Equal(JsonValueKind.False, Array[2].ValueKind);
            Assert.Equal("False", Array[2].ToString());
            Assert.Equal(JsonValueKind.String, Array[3].ValueKind);
            Assert.Equal("Hello", Array[3].ToString());
        }
    }

    public class ClassWithComplexObjects : ITestClass
    {
        public object Array { get; set; }
        public object Object { get; set; }

        public static readonly string s_array =
            @"[" +
                @"1," +
                @"""Hello""," +
                @"true," +
                @"false," +
                @"{}," +
                @"[2, ""Goodbye"", false, true, {}, [3]]" +
            @"]";

        public static readonly string s_object =
            @"{" +
                @"""NestedArray"" : " +
                s_array +
            @"}";

        public static readonly string s_json =
            @"{" +
                @"""Array"" : " +
                s_array + "," +
                @"""Object"" : " +
                s_object +
            @"}";

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(s_json);

        public void Initialize()
        {
            Array = JsonElement.Parse(s_array);
            Object = JsonElement.Parse(s_object);
        }

        public void Verify()
        {
            Assert.IsType<JsonElement>(Array);
            ValidateArray((JsonElement)Array);
            Assert.IsType<JsonElement>(Object);
            JsonElement jsonObject = (JsonElement)Object;
            Assert.Equal(JsonValueKind.Object, jsonObject.ValueKind);
            JsonElement.ObjectEnumerator enumerator = jsonObject.EnumerateObject();
            JsonProperty property = enumerator.First();
            Assert.Equal("NestedArray", property.Name);
            Assert.True(property.NameEquals("NestedArray"));
            ValidateArray(property.Value);

            void ValidateArray(JsonElement element)
            {
                Assert.Equal(JsonValueKind.Array, element.ValueKind);
                JsonElement[] elements = element.EnumerateArray().ToArray();

                Assert.Equal(JsonValueKind.Number, elements[0].ValueKind);
                Assert.Equal("1", elements[0].ToString());
                Assert.Equal(JsonValueKind.String, elements[1].ValueKind);
                Assert.Equal("Hello", elements[1].ToString());
                Assert.Equal(JsonValueKind.True, elements[2].ValueKind);
                Assert.True(elements[2].GetBoolean());
                Assert.Equal(JsonValueKind.False, elements[3].ValueKind);
                Assert.False(elements[3].GetBoolean());
            }
        }
    }

    public class JsonDocumentClass : ITestClass, IDisposable
    {
        public JsonDocument Document { get; set; }

        public static readonly string s_json =
            @"{" +
                @"""Number"" : 1," +
                @"""True"" : true," +
                @"""False"" : false," +
                @"""String"" : ""Hello""," +
                @"""Array"" : [2, false, true, ""Goodbye""]," +
                @"""Object"" : {}," +
                @"""Null"" : null" +
            @"}";

        public readonly byte[] s_data = Encoding.UTF8.GetBytes(s_json);

        public void Initialize()
        {
            Document = JsonDocument.Parse(s_json);
        }

        public void Verify()
        {
            JsonElement number = Document.RootElement.GetProperty("Number");
            JsonElement trueBool = Document.RootElement.GetProperty("True");
            JsonElement falseBool = Document.RootElement.GetProperty("False");
            JsonElement stringType = Document.RootElement.GetProperty("String");
            JsonElement arrayType = Document.RootElement.GetProperty("Array");
            JsonElement objectType = Document.RootElement.GetProperty("Object");
            JsonElement nullType = Document.RootElement.GetProperty("Null");

            Assert.Equal(JsonValueKind.Number, number.ValueKind);
            Assert.Equal("1", number.ToString());
            Assert.Equal(JsonValueKind.True, trueBool.ValueKind);
            Assert.Equal("True", true.ToString());
            Assert.Equal(JsonValueKind.False, falseBool.ValueKind);
            Assert.Equal("False", false.ToString());
            Assert.Equal(JsonValueKind.String, stringType.ValueKind);
            Assert.Equal("Hello", stringType.ToString());
            Assert.Equal(JsonValueKind.Array, arrayType.ValueKind);
            JsonElement[] elements = arrayType.EnumerateArray().ToArray();
            Assert.Equal(JsonValueKind.Number, elements[0].ValueKind);
            Assert.Equal("2", elements[0].ToString());
            Assert.Equal(JsonValueKind.False, elements[1].ValueKind);
            Assert.Equal("False", elements[1].ToString());
            Assert.Equal(JsonValueKind.True, elements[2].ValueKind);
            Assert.Equal("True", elements[2].ToString());
            Assert.Equal(JsonValueKind.String, elements[3].ValueKind);
            Assert.Equal("Goodbye", elements[3].ToString());
            Assert.Equal(JsonValueKind.Object, objectType.ValueKind);
            Assert.Equal("{}", objectType.ToString());
            Assert.Equal(JsonValueKind.Null, nullType.ValueKind);
            Assert.Equal("", nullType.ToString()); // JsonElement returns empty string for null.
        }

        public void Dispose()
        {
            Document.Dispose();
        }
    }

    public class JsonDocumentArrayClass : ITestClass, IDisposable
    {
        public JsonDocument Document { get; set; }

        public static readonly string s_json =
            @"{" +
                @"""Array"" : [" +
                    @"1, " +
                    @"true, " +
                    @"false, " +
                    @"""Hello""," +
                    @"[2, false, true, ""Goodbye""]," +
                    @"{}" +
                @"]" +
            @"}";

        public static readonly byte[] s_data = Encoding.UTF8.GetBytes(s_json);

        public void Initialize()
        {
            Document = JsonDocument.Parse(s_json);
        }

        public void Verify()
        {
            JsonElement[] array = Document.RootElement.GetProperty("Array").EnumerateArray().ToArray();

            Assert.Equal(JsonValueKind.Number, array[0].ValueKind);
            Assert.Equal("1", array[0].ToString());
            Assert.Equal(JsonValueKind.True, array[1].ValueKind);
            Assert.Equal("True", array[1].ToString());
            Assert.Equal(JsonValueKind.False, array[2].ValueKind);
            Assert.Equal("False", array[2].ToString());
            Assert.Equal(JsonValueKind.String, array[3].ValueKind);
            Assert.Equal("Hello", array[3].ToString());
        }

        public void Dispose()
        {
            Document.Dispose();
        }
    }

    public class ClassWithRecursiveCollectionTypes
    {
        public ClassWithRecursiveCollectionTypes? Nested { get; set; }
        public List<ClassWithRecursiveCollectionTypes>? List { get; set; }
        public IReadOnlyDictionary<string, ClassWithRecursiveCollectionTypes>? Dictionary { get; set; }
    }

    internal class MemoryOfTClass<T>
    {
        public Memory<T> Memory { get; set; }
    }

    internal class ReadOnlyMemoryOfTClass<T>
    {
        public ReadOnlyMemory<T> ReadOnlyMemory { get; set; }
    }
}
