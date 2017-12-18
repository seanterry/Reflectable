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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Reflectable.Internal
{
    public class TypeReflector_T_Tests
    {
        public class Clone : TypeReflector_T_Tests
        {
            // use a base class to verify that inheritance works correctly
            public abstract class BaseModel
            {
                public Guid? Value { get; set; }
                public byte[] Array { get; set; }
                public string String { get; set; }
                public object Reference { get; set; }
            }

            public class Model : BaseModel {}

            Model source = new Model();
            Model invoke() => TypeReflector<Model>.Instance.Clone( source );

            [Fact]
            public void requires_source()
            {
                source = null;
                Assert.Throws<ArgumentNullException>( nameof(source), ()=>invoke() );
            }

            [Fact]
            public void returns_cloned_type()
            {
                var actual = invoke();
                Assert.IsType<Model>( actual );
            }

            [Fact]
            public void returns_different_instance()
            {
                var expected = source;
                var actual = invoke();
                Assert.NotSame( expected, actual );
            }

            public static IEnumerable<object[]> source_data()
            {
                yield return new object[] { new Model
                {
                    Value = Guid.NewGuid(),
                    String = Guid.NewGuid().ToString(),
                    Reference = new object(),
                    Array = Guid.NewGuid().ToByteArray(),
                }};

                yield return new object[] { new Model
                {
                    Value = null,
                    String = null,
                    Reference = null,
                    Array = null,
                }};
            }

            [Theory]
            [MemberData(nameof(source_data))]
            public void returns_equal_value_types( Model source )
            {
                this.source = source;
                var expected = source.Value;
                var actual = invoke().Value;
                Assert.Equal( expected, actual );
            }

            [Theory]
            [MemberData(nameof(source_data))]
            public void returns_equal_string_types( Model source )
            {
                this.source = source;
                var expected = source.String;
                var actual = invoke().String;
                Assert.Equal( expected, actual );
            }

            [Theory]
            [MemberData(nameof(source_data))]
            public void returns_same_reference_types( Model source )
            {
                this.source = source;
                var expected = source.String;
                var actual = invoke().String;
                Assert.Same( expected, actual );
            }

            [Theory]
            [MemberData(nameof(source_data))]
            public void returns_equal_but_not_same_arrays( Model source )
            {
                this.source = source;
                var expected = source.Array;
                var actual = invoke().Array;
                Assert.Equal( expected, actual );

                if ( expected != null ) Assert.NotSame( expected, actual );
            }
        }

        public class Copy : TypeReflector_T_Tests
        {
            public class Model
            {
                public Guid? Value { get; set; }
                public byte[] Array { get; set; }
                public object Reference { get; set; }

                public Guid? ReadOnlyValue { get; }
                public byte[] ReadOnlyArray { get; }
            }

            Model source = new Model {};
            Model target = new Model
            {
                Value = Guid.Empty,
                Reference = new object(),
                Array = new byte[0],
            };

            Model invoke() => TypeReflector<Model>.Instance.CopyTo( source, target );

            [Fact]
            public void requires_source()
            {
                source = null;
                Assert.Throws<ArgumentNullException>( nameof(source), ()=>invoke() );
            }

            [Fact]
            public void requires_target()
            {
                target = null;
                Assert.Throws<ArgumentNullException>( nameof(target), ()=>invoke() );
            }

            [Fact]
            public void returns_target()
            {
                var expected = target;
                var actual = invoke();
                Assert.Same( expected, actual );
            }

            public static IEnumerable<object[]> copy_data()
            {
                yield return new object[] { new Model
                {
                    Value = Guid.NewGuid(),
                    Reference = new object(),
                    Array = Guid.NewGuid().ToByteArray(),
                }};

                yield return new object[] { new Model
                {
                    Value = null,
                    Reference = null,
                    Array = null,
                }};
            }

            [Theory]
            [MemberData(nameof(copy_data))]
            public void copies_value_type_by_value( Model source )
            {
                this.source = source;
                var expected = source.Value;
                var actual = invoke().Value;
                Assert.Equal( expected, actual );
            }

            [Theory]
            [MemberData( nameof(copy_data) )]
            public void copies_reference_type_by_reference( Model source )
            {
                this.source = source;
                var expected = source.Reference;
                var actual = invoke().Reference;
                Assert.Same( expected, actual );
            }

            [Theory]
            [MemberData(nameof(copy_data))]
            public void copies_array_type_by_value( Model source )
            {
                this.source = source;
                var expected = source.Array;
                var actual = invoke().Array;
                Assert.Equal( expected, actual );
                if ( expected != null ) Assert.NotSame( expected, actual );
            }
        }

        public class Enumerable : TypeReflector_T_Tests
        {
            public class Model
            {
                public int Property1 { get; set; }
                public string Property2 { get; set; }
                internal int InternalProperty { get; set; }
            }

            public static IEnumerable<object[]> public_properties => typeof(Model)
                .GetProperties()
                .Select( _=> new object[] { _ } );

            ITypeReflector<Model> instance => TypeReflector<Model>.Instance;

            [Theory]
            [MemberData(nameof(public_properties))]
            public void contains_public_properties( PropertyInfo expected )
            {
                Assert.Contains( instance, _=> _.Property == expected );
            }

            [Theory]
            [InlineData(nameof(Model.InternalProperty))]
            public void does_not_contain_non_public_properties( string name )
            {
                Assert.DoesNotContain( instance, _=> _.Property.Name == name );
            }
        }

        public class Index : TypeReflector_T_Tests
        {
            public class Model
            {
                public int Property1 { get; set; }
                public string Property2 { get; set; }
                internal int InternalProperty { get; set; }
            }

            public static IEnumerable<object[]> public_properties => typeof( Model )
                .GetProperties()
                .Select( _ => new object[] { _.Name } );

            ITypeReflector<Model> instance => TypeReflector<Model>.Instance;

            [Theory]
            [MemberData(nameof(public_properties))]
            public void when_name_is_public_property_returns_reflector( string name )
            {
                var expected = typeof(Model).GetProperty( name );
                var actual = instance[name].Property;
                Assert.NotNull( actual );
                Assert.Equal( expected, actual );
            }

            [Fact]
            public void when_name_is_not_public_property_throws_key_not_found()
            {
                Assert.Throws<KeyNotFoundException>( ()=> instance[nameof(Model.InternalProperty)] );
            }
        }
    }
}