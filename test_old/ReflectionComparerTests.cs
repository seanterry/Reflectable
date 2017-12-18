using System;
using System.Collections.Generic;
using Xunit;

namespace Fidget.Extensions.Reflection
{
    public class ReflectionComparerTests
    {
        /// <summary>
        /// Model type for comparison.
        /// </summary>
        
        public class Model
        {
            public string Value { get; set; }
        }

        static string random() => Convert.ToBase64String( Guid.NewGuid().ToByteArray() );

        IEqualityComparer<Model> instance => ReflectionComparer<Model>.CompareProperties;

        public static IEnumerable<object[]> NotEqualCases = new object[][]
        {
            // one instance is null
            new object[] { new Model { Value = random() }, null },
            new object[] { null, new Model { Value = random() } },

            // instances have different property values
            new object[] { new Model { Value = random() }, new Model { Value = random() } },
        };

        [Theory]
        [MemberData(nameof(NotEqualCases))]
        public void Equals_whenNotEqual_returns_false( Model x, Model y )
        {
            var actual = instance.Equals( x, y );
            Assert.False( actual );
        }

        public static IEnumerable<object[]> EqualCases()
        {
            var model = new Model { Value = random() };

            // both instance are the same reference
            yield return new object[] { model, model };

            // both instances are null
            yield return new object[] { null, null };

            // both instances have the same property values
            yield return new object[] { new Model { Value = model.Value }, new Model { Value = model.Value } };
        }

        [Theory]
        [MemberData(nameof(EqualCases))]
        public void Equals_whenEqual_returns_true( Model x, Model y )
        {
            var actual = instance.Equals( x, y );
            Assert.True( actual );
        }
    }
}