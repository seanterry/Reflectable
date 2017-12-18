using Fidget.Extensions.Reflection.Internal;
using Xunit;

namespace Fidget.Extensions.Reflection
{
    /// <summary>
    /// Tests of the type extensions methods.
    /// </summary>

    public class TypeReflectorExtensionsTest
    {
        /// <summary>
        /// Tests of the reflect method.
        /// </summary>
        
        public class Reflect
        {
            /// <summary>
            /// Model type to reflect.
            /// </summary>
            
            class Model {}

            ITypeReflector<Model> invoke() => TypeReflectorExtensions.Reflect<Model>();

            [Fact]
            public void Returns_singletonTypeReflector()
            {
                var expected = TypeReflector<Model>.Instance;
                var actual = invoke();
                Assert.Equal( expected, actual );
            }
        }

        /// <summary>
        /// Tests of the reflect method.
        /// </summary>

        public class Reflect_Instance
        {
            /// <summary>
            /// Model type to reflect.
            /// </summary>

            class Model { }

            Model instance;
            ITypeReflector<Model> invoke() => instance.Reflect();

            [Theory]
            [InlineData( false )]
            [InlineData( true )]
            public void Returns_singletonTypeReflector( bool useNull )
            {
                instance = useNull ? null : new Model();
                var expected = TypeReflector<Model>.Instance;
                var actual = invoke();
                Assert.Equal( expected, actual );
            }
        }
    }
}