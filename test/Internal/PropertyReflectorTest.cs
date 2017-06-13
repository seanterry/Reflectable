using System;
using System.Reflection;
using Xunit;

namespace Fidget.Extensions.Reflection.Internal
{
    /// <summary>
    /// Tests of the PropertyReflector type.
    /// </summary>

    public class PropertyReflectorTest
    {
        /// <summary>
        /// Tests of the PropertyInfo property.
        /// </summary>
        
        public class PropertyInfo
        {
            /// <summary>
            /// Model type for testing.
            /// </summary>
            
            class Model
            {
                public Guid? Value { get; set; }
            }

            [Fact]
            public void Returns_propertyInfo_fromReflection()
            {
                var expected = typeof(Model).GetProperty( nameof(Model.Value) );
                var actual = TypeReflector<Model>.Instance[nameof(Model.Value)].PropertyInfo;

                Assert.Equal( expected, actual );
            }
        }

        /// <summary>
        /// Tests of the IsArray property.
        /// </summary>
        
        public class IsArray
        {
            /// <summary>
            /// Model type for testing.
            /// </summary>
            
            class Model
            {
                public Guid? Value { get; set; }
                public string String { get; set; }
                public object Reference { get; set; }
                public byte[] Array { get; set; }
            }

            [Theory]
            [InlineData(nameof(Model.Value))]
            [InlineData(nameof(Model.String))]
            [InlineData(nameof(Model.Reference))]
            public void Returns_false_whenNonArray( string propertyName )
            {
                var actual = TypeReflector<Model>.Instance[propertyName];
                Assert.False( actual.IsArray );
            }

            [Theory]
            [InlineData(nameof(Model.Array))]
            public void Returns_true_whenArray( string propertyName )
            {
                var actual = TypeReflector<Model>.Instance[propertyName];
                Assert.True( actual.IsArray );
            }
        }

        /// <summary>
        /// Tests of the IsReadOnly property.
        /// </summary>
        
        public class IsReadOnly
        {
            /// <summary>
            /// Model type for testing.
            /// </summary>
            
            class Model
            {
                public Guid? Value { get; set; }
                public Guid? ReadOnlyValue { get; }
            }

            [Fact]
            public void Returns_false_whenWriteableProperty()
            {
                var actual = TypeReflector<Model>.Instance[nameof(Model.Value)];
                Assert.False( actual.IsReadOnly );
            }

            [Fact]
            public void Returns_true_whenReadOnlyProperty()
            {
                var actual = TypeReflector<Model>.Instance[nameof( Model.ReadOnlyValue )];
                Assert.True( actual.IsReadOnly );
            }
        }

        /// <summary>
        /// Tests of the copy method.
        /// </summary>
        
        public class Copy
        {
            /// <summary>
            /// Model type for testing.
            /// </summary>
            
            class Model
            {
                public Guid? ReadOnlyValue { get; }
                public byte[] ReadOnlyArray { get; }
                public object ReadOnlyReference { get; }

                public Guid? Value { get; set; }
                public byte[] Array { get; set; }
                public object Reference { get; set; }
            }

            Model source = new Model();
            Model target = new Model
            {
                Value = Guid.Empty,
                Array = new byte[0],
                Reference = new object(),
            };

            void Invoke( string propertyName ) => TypeReflector<Model>.Instance[propertyName].Copy( source, target ); 
            
            [Fact]
            public void Requires_source()
            {
                source = null;
                Assert.Throws<ArgumentNullException>( nameof(source), () => Invoke( nameof(Model.Value) ) );
            }

            [Fact]
            public void Requires_target()
            {
                target = null;
                Assert.Throws<ArgumentNullException>( nameof( target ), () => Invoke( nameof( Model.Value ) ) );
            }

            [Theory]
            [InlineData(nameof(Model.ReadOnlyValue))]
            [InlineData( nameof( Model.ReadOnlyReference ) )]
            [InlineData( nameof( Model.ReadOnlyArray ) )]
            public void Throws_invalidOperation_whenReadOnly( string propertyName )
            {
                Assert.Throws<InvalidOperationException>( () => Invoke( propertyName ) );
            }

            [Theory]
            [InlineData( false )]
            [InlineData( true )]
            public void Copies_valueType_byValue( bool useNull )
            {
                var expected = source.Value = useNull ? (Guid?)null : Guid.NewGuid();
                Invoke( nameof(Model.Value) );
                var actual = target.Value;

                Assert.Equal( expected, actual );
            }

            [Theory]
            [InlineData( false )]
            [InlineData( true )]
            public void Copies_referenceType_byReference( bool useNull )
            {
                var expected = source.Reference = useNull ? null : new object();
                Invoke( nameof( Model.Reference ) );
                var actual = target.Reference;

                Assert.Equal( expected, actual );
            }

            [Theory]
            [InlineData( false )]
            [InlineData( true )]
            public void Copies_arrayType_byValue( bool useNull )
            {
                var expected = source.Array = useNull ? null : Guid.NewGuid().ToByteArray();
                Invoke( nameof(Model.Array) );
                var actual = target.Array;

                Assert.Equal( expected, actual );

                // ensure non-null values are not the same instances
                if ( expected != null ) Assert.NotSame( expected, actual );
            }
        }

        /// <summary>
        /// Tests of the Equal method.
        /// </summary>
        
        public class Equal
        {
            /// <summary>
            /// Model for testing.
            /// </summary>

            class Model
            {
                public Guid? Value { get; set; }
                public string String { get; set; }
                public byte[] Array { get; set; }
                public object Reference { get; set; }
            }

            Model source = new Model();
            Model comparer = new Model();
            bool Invoke( string propertyName ) => TypeReflector<Model>.Instance[propertyName].Equal( source, comparer );

            [Fact]
            public void Requires_source()
            {
                source = null;
                Assert.Throws<ArgumentNullException>( nameof(source), () => Invoke( nameof(Model.Value) ) );
            }

            [Fact]
            public void Requires_comparer()
            {
                comparer = null;
                Assert.Throws<ArgumentNullException>( nameof(comparer), () => Invoke( nameof(Model.Value) ) );
            }

            [Theory]
            [InlineData( null )]
            [InlineData( "feb276f3-114f-4ecd-a04d-7199b96fe91d" )]
            public void Returns_true_whenValueTypesEqual( string valueText )
            {
                source.Value = Guid.TryParse( valueText, out Guid value ) ? value : (Guid?)null;
                comparer.Value = source.Value;
                var actual = Invoke( nameof(Model.Value) );

                Assert.True( actual );
            }

            [InlineData( "a9604fcc-7c28-4c23-b9f5-81e86c864c71", null )]
            [InlineData( null, "d3def891-8488-44b8-946e-cc79e418187b" )]
            [InlineData( "0ef9019d-79a1-4a9e-a799-9f4af8f93762", "3e3dcee7-ce45-4bc4-bdd4-c9c3cbfc6a5d" )]
            [Theory]
            public void Returns_false_whenValueTypesUnequal( string sourceValue, string comparerValue )
            {
                Guid? toGuid( string valueText ) => Guid.TryParse( valueText, out Guid value ) ? value : (Guid?)null;
                source.Value = toGuid( sourceValue );
                comparer.Value = toGuid( comparerValue );
                var actual = Invoke( nameof(Model.Value) );

                Assert.False( actual );
            }

            [Theory]
            [InlineData( null )]
            [InlineData( "fa98c43d-42d1-4673-abfe-8f6baecac50d" )]
            public void Returns_true_whenStringTypesEqual( string valueText )
            {
                source.String = 
                comparer.String = valueText;
                var actual = Invoke( nameof( Model.String ) );

                Assert.True( actual );
            }

            [InlineData( "a9604fcc-7c28-4c23-b9f5-81e86c864c71", null )]
            [InlineData( null, "d3def891-8488-44b8-946e-cc79e418187b" )]
            [InlineData( "0ef9019d-79a1-4a9e-a799-9f4af8f93762", "3e3dcee7-ce45-4bc4-bdd4-c9c3cbfc6a5d" )]
            [Theory]
            public void Returns_false_whenStringTypesUnequal( string sourceValue, string comparerValue )
            {
                source.String = sourceValue;
                comparer.String = comparerValue;
                var actual = Invoke( nameof( Model.String ) );

                Assert.False( actual );
            }

            [InlineData( true )]
            [InlineData( false )]
            [Theory]
            public void Returns_true_whenReferenceTypesSame( bool useNull )
            {
                var reference = !useNull ? new object() : null;
                source.Reference =
                comparer.Reference = reference;
                var actual = Invoke( nameof(Model.Reference) );
                
                Assert.True( actual );
            }

            /// <summary>
            /// False should be returned when string values differ.
            /// </summary>

            [InlineData( true, false )]
            [InlineData( false, false )]
            [InlineData( false, true )]
            [Theory]
            public void Returns_false_whenReferenceTypesDifferent( bool sourceNull, bool comparerNull )
            {
                source.Reference = !sourceNull ? new object() : null;
                comparer.Reference = !comparerNull ? new object() : null;
                var actual = Invoke(nameof(Model.Reference));
                
                Assert.False( actual );
            }

            [InlineData( null )]
            [InlineData( "feb276f3-114f-4ecd-a04d-7199b96fe91d" )]
            [Theory]
            public void Returns_true_whenArrayTypesMatchElements( string value )
            {
                byte[] toArray() => value != null
                    ? new Guid( value ).ToByteArray()
                    : null;

                source.Array = toArray();
                comparer.Array = toArray();
                
                // sanity check
                if ( value != null ) Assert.NotSame( source.Array, comparer.Array );
                
                var actual = Invoke( nameof(Model.Array) );
                Assert.True( actual );
            }

            [InlineData( "a9604fcc-7c28-4c23-b9f5-81e86c864c71", null )]
            [InlineData( null, "d3def891-8488-44b8-946e-cc79e418187b" )]
            [InlineData( "0ef9019d-79a1-4a9e-a799-9f4af8f93762", "3e3dcee7-ce45-4bc4-bdd4-c9c3cbfc6a5d" )]
            [Theory]
            public void Returns_false_whenArrayTypesDifferentElements( string sourceValue, string comparerValue )
            {
                byte[] toArray( string value ) => value != null
                    ? new Guid( value ).ToByteArray()
                    : null;

                source.Array = toArray( sourceValue );
                comparer.Array = toArray( comparerValue );
                var actual = Invoke(nameof(Model.Array));

                Assert.False( actual );
            }
        }
    }
}