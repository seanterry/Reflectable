/*  Copyright 2017 Sean Terry

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Reflectable.Internal
{
    public class PropertyReflector_T_V_Tests
    {
        PropertyInfo property;
        IPropertyReflector<T> instance<T,V>() => new PropertyReflector<T,V>( property );
        IPropertyReflector<T> instance<T,V>( string property ) => new PropertyReflector<T,V>( typeof(T).GetProperty( property ) );

        public class Constructor : PropertyReflector_T_V_Tests
        {
            public string Value { get; set; }

            [Fact]
            public void requires_property()
            {
                property = null;
                Assert.Throws<ArgumentNullException>( nameof(property), ()=>instance<Constructor,string>() );
            }

            [Fact]
            public void requires_matching_declaring_type()
            {
                property = typeof(Constructor).GetProperty( nameof(Value) );
                Assert.Throws<ArgumentException>( nameof(property), ()=>instance<object,string>() );
            }

            [Fact]
            public void requires_matching_property_type()
            {
                property = typeof( Constructor ).GetProperty( nameof( Value ) );
                Assert.Throws<ArgumentException>( nameof(property), ()=>instance<Constructor,int>() );
            }
        }

        public class Property : PropertyReflector_T_V_Tests
        {
            public string Value { get; set; }

            [Fact]
            public void matches_metadata()
            {
                var expected = property = typeof(Property).GetProperty( nameof(Value) );
                var actual = instance<Property,string>().Property;

                Assert.Equal( expected, actual );
            }
        }

        public class Copy : PropertyReflector_T_V_Tests
        {
            class Model
            {
                public Guid ReadOnlyProp { get; } = Guid.NewGuid();
                public Guid ValueProp { get; set; }
                public string StringProp { get; set; }
                public object ReferenceProp { get; set; }
                public byte[] ArrayProp { get; set; }
            }

            Model source = new Model();
            Model target = new Model();
            void invoke<TValue>() => instance<Model,TValue>().Copy( source, target );
            
            [Fact]
            public void requires_source()
            {
                property = typeof( Model ).GetProperty( nameof( Model.StringProp ) );
                source = null;
                Assert.Throws<ArgumentNullException>( nameof(source), ()=>invoke<string>() );
            }

            [Fact]
            public void requires_target()
            {
                property = typeof( Model ).GetProperty( nameof( Model.StringProp ) );
                target = null;
                Assert.Throws<ArgumentNullException>( nameof(target), ()=>invoke<string>() );
            }

            [Fact]
            public void requires_writeable_property()
            {
                property = typeof(Model).GetProperty( nameof( Model.ReadOnlyProp ) );
                Assert.Throws<InvalidOperationException>( ()=>invoke<Guid>() );
            }

            [Fact]
            public void when_value_type_copies_value()
            {
                var expected = source.ValueProp = Guid.NewGuid();
                property = typeof(Model).GetProperty( nameof(Model.ValueProp) );
                invoke<Guid>();
                var actual = target.ValueProp;

                Assert.Equal( expected, actual );
            }

            [Theory]
            [InlineData( null )]
            [InlineData( "Hello World" )]
            public void when_string_type_copies_string( string expected )
            {
                source.StringProp = expected;
                property = typeof(Model).GetProperty( nameof(Model.StringProp) );
                invoke<string>();
                var actual = target.StringProp;

                Assert.Equal( expected, actual );
            }

            public static IEnumerable<object[]> reference_values() => new object[][]
            {
                new object[] { null },
                new object[] { new object() },
                new object[] { new int[5] },
            };

            [Theory]
            [MemberData(nameof(reference_values))]
            public void when_reference_type_copies_reference( object expected )
            {
                source.ReferenceProp = expected;
                property = typeof(Model).GetProperty( nameof(Model.ReferenceProp) );
                invoke<object>();
                var actual = target.ReferenceProp;

                Assert.True( ReferenceEquals( expected, actual ) );
            }

            [Fact]
            public void when_array_type_clones_array()
            {
                var expected = source.ArrayProp = Guid.NewGuid().ToByteArray();
                property = typeof(Model).GetProperty( nameof(Model.ArrayProp) );
                invoke<byte[]>();
                var actual = target.ArrayProp;

                // values should match, but the reference should not be the same
                Assert.Equal( expected, actual );
                Assert.False( ReferenceEquals( expected, actual ) );
            }
        }

        public class Equal : PropertyReflector_T_V_Tests
        {
            public class Model
            {
                public Guid? ValueProp { get; set; }
                public string StringProp { get; set; }
                public object ReferenceProp { get; set; }
                public byte[] ArrayProp { get; set; }
            }

            Model source = new Model();
            Model comparer = new Model();
            bool invoke<TValue>() => instance<Model,TValue>().Equal( source, comparer );

            [Fact]
            public void requires_source()
            {
                source = null;
                property = typeof(Model).GetProperty( nameof(Model.ValueProp) );
                Assert.Throws<ArgumentNullException>( nameof(source), ()=>invoke<Guid?>() );
            }

            [Fact]
            public void requires_comparer()
            {
                comparer = null;
                property = typeof( Model ).GetProperty( nameof( Model.ValueProp ) );
                Assert.Throws<ArgumentNullException>( nameof( comparer ), () => invoke<Guid?>() );
            }

            public class matching_values : PropertyReflector_T_V_Tests, IEnumerable<object[]>
            {
                static readonly Type type = typeof(Model);
                static readonly Guid guid = Guid.NewGuid();

                IEnumerable<object[]> data()
                {
                    yield return new object[] { instance<Model,Guid?>( nameof(Model.ValueProp) ), new Model { ValueProp = guid }, new Model { ValueProp = guid } };
                    yield return new object[] { instance<Model,Guid?>( nameof(Model.ValueProp) ), new Model { ValueProp = null }, new Model { ValueProp = null } };
                    yield return new object[] { instance<Model,string>( nameof(Model.StringProp) ), new Model { StringProp = guid.ToString() }, new Model { StringProp = guid.ToString() } };
                    yield return new object[] { instance<Model,string>( nameof( Model.StringProp ) ), new Model { StringProp = null }, new Model { StringProp = null } };
                    yield return new object[] { instance<Model,object>( nameof(Model.ReferenceProp) ), new Model { ReferenceProp = this }, new Model { ReferenceProp = this } };
                    yield return new object[] { instance<Model, object>( nameof( Model.ReferenceProp ) ), new Model { ReferenceProp = null }, new Model { ReferenceProp = null } };
                    yield return new object[] { instance<Model, byte[]>( nameof( Model.ArrayProp ) ), new Model{ ArrayProp = guid.ToByteArray() }, new Model { ArrayProp = guid.ToByteArray() } };
                    yield return new object[] { instance<Model, byte[]>( nameof( Model.ArrayProp ) ), new Model { ArrayProp = null }, new Model { ArrayProp = null } };
                }

                public IEnumerator<object[]> GetEnumerator() => data().GetEnumerator();
                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            [Theory]
            [ClassData(typeof(matching_values))]
            public void when_value_type_match_returns_true( IPropertyReflector<Model> instance, Model source, Model comparer )
            {
                const bool expected = true;
                var actual = instance.Equal( source, comparer );
                Assert.Equal( expected, actual );
            }

            public class mismatching_values : PropertyReflector_T_V_Tests, IEnumerable<object[]>
            {
                static readonly Type type = typeof( Model );
                static readonly Guid guid1 = Guid.NewGuid();
                static readonly Guid guid2 = Guid.NewGuid();

                IEnumerable<object[]> data()
                {
                    yield return new object[] { instance<Model, Guid?>( nameof( Model.ValueProp ) ), new Model { ValueProp = guid1 }, new Model { ValueProp = guid2 } };
                    yield return new object[] { instance<Model, Guid?>( nameof( Model.ValueProp ) ), new Model { ValueProp = guid1 }, new Model { ValueProp = null } };
                    yield return new object[] { instance<Model, Guid?>( nameof( Model.ValueProp ) ), new Model { ValueProp = null }, new Model { ValueProp = guid2 } };
                    yield return new object[] { instance<Model, string>( nameof( Model.StringProp ) ), new Model { StringProp = guid1.ToString() }, new Model { StringProp = guid2.ToString() } };
                    yield return new object[] { instance<Model, string>( nameof( Model.StringProp ) ), new Model { StringProp = guid1.ToString() }, new Model { StringProp = null } };
                    yield return new object[] { instance<Model, string>( nameof( Model.StringProp ) ), new Model { StringProp = null }, new Model { StringProp = guid2.ToString() } };
                    yield return new object[] { instance<Model, object>( nameof( Model.ReferenceProp ) ), new Model { ReferenceProp = new object() }, new Model { ReferenceProp = new object() } };
                    yield return new object[] { instance<Model, object>( nameof( Model.ReferenceProp ) ), new Model { ReferenceProp = null }, new Model { ReferenceProp = new object() } };
                    yield return new object[] { instance<Model, object>( nameof( Model.ReferenceProp ) ), new Model { ReferenceProp = new object() }, new Model { ReferenceProp = null } };
                    yield return new object[] { instance<Model, byte[]>( nameof( Model.ArrayProp ) ), new Model { ArrayProp = guid1.ToByteArray() }, new Model { ArrayProp = guid2.ToByteArray() } };
                    yield return new object[] { instance<Model, byte[]>( nameof( Model.ArrayProp ) ), new Model { ArrayProp = guid1.ToByteArray() }, new Model { ArrayProp = null } };
                    yield return new object[] { instance<Model, byte[]>( nameof( Model.ArrayProp ) ), new Model { ArrayProp = null }, new Model { ArrayProp = guid2.ToByteArray() } };
                }

                public IEnumerator<object[]> GetEnumerator() => data().GetEnumerator();
                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            [Theory]
            [ClassData(typeof(mismatching_values))]
            public void when_value_type_mismatch_returns_false( IPropertyReflector<Model> instance, Model source, Model comparer )
            {
                const bool expected = false;
                var actual = instance.Equal( source, comparer );
                Assert.Equal( expected, actual );
            }
        }

        public class GetValue : PropertyReflector_T_V_Tests
        {
            public class Model
            {
                public object Value { get; set; }
            }

            Model source = new Model();
            object invoke() => instance<Model,object>( nameof(Model.Value) ).GetValue( source );

            [Fact]
            public void requires_source()
            {
                source = null;
                Assert.Throws<ArgumentNullException>( nameof(source), ()=>invoke() );
            }

            [Theory]
            [InlineData( null )]
            [InlineData( "Hello world" )]
            [InlineData( 42 )]
            public void returns_current_value( object expected )
            {
                source.Value = expected;
                var actual = invoke();
                Assert.Equal( expected, actual );
            }
        }

        public class SetValue : PropertyReflector_T_V_Tests
        {
            public class Model
            {
                public string ReadOnlyProp { get; } = Guid.NewGuid().ToString();
                public string Value { get; set; }
            }

            Model target = new Model();
            object value = "hello world";
            
            [Fact]
            public void requires_target()
            {
                void invoke() => instance<Model,string>( nameof(Model.Value) ).SetValue( null, value );
                Assert.Throws<ArgumentNullException>( nameof(target), ()=>invoke() );
            }

            [Fact]
            public void requires_writeable_property()
            {
                void invoke() => instance<Model,string>( nameof(Model.ReadOnlyProp) ).SetValue( target, value );
                Assert.Throws<InvalidOperationException>( ()=>invoke() );
            }

            public static IEnumerable<object[]> mismatching_property_type_values() => new object[][]
            {
                new object[] { Guid.NewGuid() },
                new object[] { new object() },
                new object[] { 12 },
            };

            [Theory]
            [MemberData(nameof(mismatching_property_type_values))]
            public void requires_matching_property_type( object value )
            {
                this.value = value;
                void invoke() => instance<Model, string>( nameof( Model.Value ) ).SetValue( target, value );
                Assert.Throws<InvalidCastException>( ()=>invoke() );
            }

            [Theory]
            [InlineData( null )]
            [InlineData( "hello world" )]
            public void sets_property_value( string expected )
            {
                value = expected;
                void invoke() => instance<Model,string>( nameof(Model.Value) ).SetValue( target, value );

                invoke();
                Assert.Equal( expected, target.Value );
            }
        }
    }
}