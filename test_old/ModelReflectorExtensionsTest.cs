using Fidget.Extensions.Reflection.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Fidget.Extensions.Reflection
{
    /// <summary>
    /// Tests of the model reflector extensions type.
    /// </summary>

    public class ModelReflectorExtensionsTest
    {
        /// <summary>
        /// Tests of the base model method.
        /// </summary>
        
        public class Model
        {
            IModelReflector<Model> Invoke() => ModelReflectorExtensions.Model<Model>();

            [Fact]
            public void Returns_singletonModelReflector()
            {
                var exepcted = ModelReflector<Model>.Instance;
                var actual = Invoke();
                Assert.Equal( exepcted, actual );
            }
        }

        /// <summary>
        /// Tests of the model method with an instance argument overload.
        /// </summary>
        
        public class Model_Instance
        {
            Model_Instance instance;
            IModelReflector<Model_Instance> Invoke() => instance.Model();

            [Theory]
            [InlineData( false )]
            [InlineData( true )]
            public void Returns_singletonModelReflector( bool useNull )
            {
                instance = useNull ? null : this;
                var expected = ModelReflector<Model_Instance>.Instance;
                var actual = Invoke();
                Assert.Equal( expected, actual );
            }
        }
    }
}