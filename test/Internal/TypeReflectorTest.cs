using System;
using System.Collections;
using Xunit;

namespace Fidget.Extensions.Reflection.Internal
{
    /// <summary>
    /// Tests of the type reflector.
    /// </summary>

    public class TypeReflectorTest
    {
        /// <summary>
        /// Tests of the Clone method.
        /// </summary>
        
        public class Clone
        {
            /// <summary>
            /// Model class for testing.
            /// </summary>
            
            class Model
            {
                public Guid? Value { get; set; }
                public byte[] Array { get; set; }
                public string String { get; set; }
                public object Reference { get; set; }
            }

            /// <summary>
            /// Method instance argument.
            /// </summary>
            
            Model source = new Model();

            /// <summary>
            /// Invokes the method under test with the supplied argument(s).
            /// </summary>
                        
            Model Invoke() => TypeReflector<Model>.Instance.Clone( source );

            [Fact]
            public void Requires_source()
            {
                source = null;
                Assert.Throws<ArgumentNullException>( nameof(source), () => Invoke() );
            }

            [Fact]
            public void Returns_cloned_type()
            {
                var actual = Invoke();
                Assert.IsType<Model>( actual );
            }

            [Fact]
            public void Returns_different_instance()
            {
                var expected = source;
                var actual = Invoke();
                Assert.NotEqual( expected, actual );
            }

            [Theory]
            [InlineData( true )]
            [InlineData( false )]
            public void Returns_property_whenValueType_equal( bool useNull )
            {
                source.Value = useNull ? (Guid?)null : Guid.NewGuid();
                var expected = source.Value;
                var actual = Invoke().Value;

                Assert.Equal( expected, actual );
            }

            [Theory]
            [InlineData( true )]
            [InlineData( false )]
            public void Returns_property_whenStringType_equal( bool useNull )
            {
                var expected = source.String = useNull ? (string)null : Guid.NewGuid().ToString();
                var actual = Invoke().String;

                Assert.Equal( expected, actual );
            }

            [Theory]
            [InlineData( true )]
            [InlineData( false )]
            public void Returns_property_whenReferenceType_refernceEqual( bool useNull )
            {
                var expected = source.Reference = useNull ? null : new object();
                var actual = Invoke().Reference;

                Assert.Same( expected, actual );
            }

            [Theory]
            [InlineData( true )]
            [InlineData( false )]
            public void Returns_property_whenArrayType_equal( bool useNull )
            {
                var expected = source.Array = useNull ? null : Guid.NewGuid().ToByteArray();
                var actual = Invoke().Array;

                Assert.Equal( expected, actual );
            }

            /// <summary>
            /// Array types should be cloned, rather than reference the same instance.
            /// </summary>
            
            [Fact]
            public void Returns_property_whenArrayType_notReferenceEqual()
            {
                var expected = source.Array = Guid.NewGuid().ToByteArray();
                var actual = Invoke().Array;

                Assert.NotSame( expected, actual );
            }
        }

        /// <summary>
        /// Tests of the CopyTo method.
        /// </summary>
        
        public class CopyTo
        {
            /// <summary>
            /// Model type for testing.
            /// </summary>
            
            class Model
            {
                public Guid? Value { get; set; }
                public byte[] Array { get; set; }
                public object Reference { get; set; }

                public Guid? ReadOnlyValue { get; }
                public byte[] ReadOnlyArray { get; }
            }

            /// <summary>
            /// Source argument.
            /// </summary>
            
            Model source = new Model {};

            /// <summary>
            /// Target argument.
            /// </summary>
            
            Model target = new Model
            {
                Value = Guid.Empty,
                Reference = new object(),
                Array = new byte[0],
            };

            /// <summary>
            /// Invokes the method under test.
            /// </summary>
            
            Model Invoke() => TypeReflector<Model>.Instance.CopyTo( source, target );
            
            [Fact]
            public void Requires_source()
            {
                source = null;
                Assert.Throws<ArgumentNullException>( nameof(source), () => Invoke() );
            }

            [Fact]
            public void Requires_target()
            {
                target = null;
                Assert.Throws<ArgumentNullException>( nameof(target), () => Invoke() );
            }

            [Fact]
            public void Returns_target()
            {
                var expected = target;
                var actual = Invoke();

                Assert.Equal( expected, actual );
            }

            [Theory]
            [InlineData( false )]
            [InlineData( true )]
            public void Copies_valueType_byValue( bool useNull )
            {
                var expected = source.Value = useNull ? (Guid?)null : Guid.NewGuid();
                var actual = Invoke().Value;

                Assert.Equal( expected, actual );
            }

            [Theory]
            [InlineData( false )]
            [InlineData( true )]
            public void Copies_referenceType_byReference( bool useNull )
            {
                var expected = source.Reference = useNull ? null : new object();
                var actual = Invoke().Reference;

                Assert.Same( expected, actual );
            }

            [Theory]
            [InlineData( false )]
            [InlineData( true )]
            public void Copies_arrayType_byValue( bool useNull )
            {
                var expected = source.Array = useNull ? null : Guid.NewGuid().ToByteArray();
                var actual = Invoke().Array;

                Assert.Equal( expected, actual );

                // ensure non-null values are not the same instances
                if ( expected != null ) Assert.NotSame( expected, actual );
            }
        }

        /// <summary>
        /// Tests of the class as a collection.
        /// </summary>
        
        public class Enumerable
        {
            /// <summary>
            /// Model type for testing.
            /// </summary>
            
            class Model
            {
                public int Property1 { get; set; }
                public string Property2 { get; set; }
                internal int InternalProperty { get; set; }
            }

            [Theory]
            [InlineData(nameof(Model.Property1))]
            [InlineData(nameof(Model.Property2))]
            public void Contains_publicProperties( string propertyName )
            {
                var actual = TypeReflector<Model>.Instance;
                Assert.Contains( actual, _=> _.PropertyInfo.Name == propertyName );
            }

            [Theory]
            [InlineData(nameof(Model.InternalProperty))]
            public void DoesNotContain_nonPublicProperties( string propertyName )
            {
                var actual = TypeReflector<Model>.Instance;
                Assert.DoesNotContain( actual, _=> _.PropertyInfo.Name == propertyName );
            }
        }

        /// <summary>
        /// Tests of the index method.
        /// </summary>
        
        public class Index
        {
            /// <summary>
            /// Model type for testing.
            /// </summary>

            class Model
            {
                public int Property1 { get; set; }
                public string Property2 { get; set; }
                internal int InternalProperty { get; set; }
            }

            [Theory]
            [InlineData( nameof( Model.Property1 ) )]
            [InlineData( nameof( Model.Property2 ) )]
            public void Returns_propertyReflector_whenPublicProperty( string propertyName )
            {
                var actual = TypeReflector<Model>.Instance[propertyName];
                Assert.IsAssignableFrom<IPropertyReflector<Model>>( actual );
                Assert.Equal( propertyName, actual.PropertyInfo.Name );
            }

            [Theory]
            [InlineData( nameof( Model.InternalProperty ) )]
            public void Throws_outOfRange_whenNonPublicProperty( string propertyName )
            {
                Assert.Throws<IndexOutOfRangeException>( () => TypeReflector<Model>.Instance[propertyName] );
            }
        }
    }
}