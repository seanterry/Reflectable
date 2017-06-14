using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Fidget.Extensions.Reflection.Internal
{
    /// <summary>
    /// Tests of the ModelReflector type.
    /// </summary>

    public class ModelReflectorTests
    {
        /// <summary>
        /// Tests of the instance static property.
        /// </summary>
        
        public class Instance
        {
            class Model {}

            [Fact]
            public void Returns_singleton()
            {
                var expected = ModelReflector<Model>.Instance;

                for ( var i = 0; i < 3; i++ )
                {
                    var actual = ModelReflector<Model>.Instance;
                    Assert.Equal( expected, actual );
                }
            }
        }

        /// <summary>
        /// Tests of the table property.
        /// </summary>
        
        public class Table
        {
            const string tableName = "4b78a87b-5ec6-4d12-94ad-7cd25dd4292d";
            const string schemaName = "a87e329c-aa94-46f2-9c84-6c920d223b76";
            
            [Table( tableName, Schema = schemaName )]
            class TableModel {}
            
            class NonTableModel {}

            [Fact]
            public void Returns_tableAttribute_whenModelDecorated()
            {
                var actual = ModelReflector<TableModel>.Instance.Table;
                Assert.IsType<TableAttribute>( actual );
                Assert.Equal( tableName, actual.Name );
                Assert.Equal( schemaName, actual.Schema );
            }

            [Fact]
            public void Returns_null_whenModelNotDecorated()
            {
                var actual = ModelReflector<NonTableModel>.Instance.Table;
                Assert.Null( actual );
            }
        }

        /// <summary>
        /// Tests of the keys property.
        /// </summary>
        
        public class Keys
        {
            class Model
            {
                [Key,Column(Order=1)]
                public int Key1 { get; }

                [Key]
                public int Key2 { get; }

                [Key,Column(Order=0)]
                public int Key3 { get; }

                [Key]
                public int Key4 { get; }

                [Column(Order = 0)]
                public int NonKey { get; }

                [Key,NotMapped]
                public int NotMapped { get; }
            }

            ITypeReflector<Model> TypeReflector = TypeReflectorExtensions.Reflect<Model>();

            IEnumerable<IPropertyReflector<Model>> actual = ModelReflector<Model>.Instance.Keys;

            /// <summary>
            /// The collection should contain all properties decorated by the key attribute, an in order by their column attribute, then name.
            /// </summary>
            
            [Fact]
            public void Contains_decoratedProperties_inColumnThenNameOrder()
            {
                var expected = new IPropertyReflector<Model>[]
                {
                    TypeReflector[nameof(Model.Key3)],
                    TypeReflector[nameof(Model.Key1)],
                    TypeReflector[nameof(Model.Key2)],
                    TypeReflector[nameof(Model.Key4)],
                };
               
                Assert.Equal( expected, actual );
            }

            [Fact]
            public void DoesNotContain_nonDecoratedProperties()
            {
                var expected = TypeReflector[nameof(Model.NonKey)];
                Assert.DoesNotContain( expected, actual );
            }

            [Fact]
            public void DoesNotContain_notMappedProperties()
            {
                var expected = TypeReflector[nameof(Model.NotMapped)];
                Assert.DoesNotContain( expected, actual );
            }
        }

        /// <summary>
        /// Tests of the generated property.
        /// </summary>
        
        public class Generated
        {
            class Model
            {
                [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
                public int Identity1 { get; }
                
                [DatabaseGenerated(DatabaseGeneratedOption.Identity), Column( Order = 0)]
                public int Identity2 { get; }

                [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
                public int Computed1 { get; }

                [DatabaseGenerated(DatabaseGeneratedOption.Computed),Column(Order = 1)]
                public int Computed2 { get; }

                [DatabaseGenerated(DatabaseGeneratedOption.None)]
                public int NonGenerated { get; }

                public int NonDecorated { get; }

                [DatabaseGenerated(DatabaseGeneratedOption.Identity),NotMapped]
                public int NotMappedIdentity { get; }

                [DatabaseGenerated(DatabaseGeneratedOption.Computed),NotMapped]
                public int NotMappedComputed { get; }

                [DatabaseGenerated(DatabaseGeneratedOption.None),NotMapped]
                public int NotMappedNone { get; }
            }

            ITypeReflector<Model> TypeReflector = TypeReflectorExtensions.Reflect<Model>();
            IEnumerable<IPropertyReflector<Model>> actual = ModelReflector<Model>.Instance.Generated;

            [Fact]
            public void DoesNotContain_nonDecoratedProperties()
            {
                var expected = TypeReflector[nameof(Model.NonDecorated)];
                Assert.DoesNotContain( expected, actual );
            }

            [Fact]
            public void DoesNotContain_nonGeneratedProperties()
            {
                var expected = TypeReflector[nameof(Model.NonGenerated)];
                Assert.DoesNotContain( expected, actual );
            }

            [Fact]
            public void Contains_identityAndComputed_inColumnThenNameOrder()
            {
                var expected = new IPropertyReflector<Model>[]
                {
                    TypeReflector[nameof(Model.Identity2)],
                    TypeReflector[nameof(Model.Computed2)],
                    TypeReflector[nameof(Model.Computed1)],
                    TypeReflector[nameof(Model.Identity1)],
                };

                Assert.Equal( expected, actual );
            }

            [Theory]
            [InlineData(nameof(Model.NotMappedComputed))]
            [InlineData( nameof( Model.NotMappedIdentity ) )]
            [InlineData( nameof( Model.NotMappedNone ) )]
            public void DoesNotContain_notMappedProperties( string propertyName )
            {
                var expected = TypeReflector[propertyName];
                Assert.DoesNotContain( expected, actual );
            }
        }

        /// <summary>
        /// Tests of the concurrency property.
        /// </summary>
        
        public class Concurrency
        {
            class Model
            {
                [ConcurrencyCheck]
                public int Concurrency1 { get; }
                
                [ConcurrencyCheck,Column(Order =1)]
                public int Concurrency2 { get; }
                
                [ConcurrencyCheck,Column(Order =0)]
                public int Concurrency3 { get; }
                
                [ConcurrencyCheck]
                public int Concurrency4 { get; }

                public int NonDecorated { get; }

                [ConcurrencyCheck,NotMapped]
                public int NotMapped { get; }
            }

            ITypeReflector<Model> TypeReflector = TypeReflectorExtensions.Reflect<Model>();
            IEnumerable<IPropertyReflector<Model>> actual = ModelReflector<Model>.Instance.Concurrency;

            [Fact]
            public void DoesNotContain_nonDecoratedProperties()
            {
                var expected = TypeReflector[nameof( Model.NonDecorated )];
                Assert.DoesNotContain( expected, actual );
            }

            [Fact]
            public void Contains_concurrencyCheck_inColumnThenNameOrder()
            {
                var expected = new IPropertyReflector<Model>[]
                {
                    TypeReflector[nameof(Model.Concurrency3)],
                    TypeReflector[nameof(Model.Concurrency2)],
                    TypeReflector[nameof(Model.Concurrency1)],
                    TypeReflector[nameof(Model.Concurrency4)],
                };

                Assert.Equal( expected, actual );
            }

            [Fact]
            public void DoesNotContain_notMappedProperties()
            {
                var expected = TypeReflector[nameof( Model.NotMapped )];
                Assert.DoesNotContain( expected, actual );
            }
        }

        /// <summary>
        /// Tests of the insertable property.
        /// </summary>
        
        public class Insertable
        {
            class Model
            {
                [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
                public int IdentityKey { get; set; }

                [Key,DatabaseGenerated(DatabaseGeneratedOption.Computed)]
                public int ComputedKey { get; set; }

                [Key]
                public int InsertableKey { get; set; }

                [ConcurrencyCheck]
                public int Concurrency { get; set; }

                public int InsertableField1 { get; set; }

                [Column(Order =0)]
                public int InsertableField2 { get; set; }

                [NotMapped]
                public int NotMapped { get; set; }

                public int ReadOnlyField { get; }
            }

            ITypeReflector<Model> TypeReflector = TypeReflectorExtensions.Reflect<Model>();
            IEnumerable<IPropertyReflector<Model>> actual = ModelReflector<Model>.Instance.Insertable;

            [Fact]
            public void Contains_insertableProperties_inColumnThenNameOrder()
            {
                var expected = new IPropertyReflector<Model>[]
                {
                    TypeReflector[nameof(Model.InsertableField2)],
                    TypeReflector[nameof(Model.InsertableField1)],
                    TypeReflector[nameof(Model.InsertableKey)],
                };

                Assert.Equal( expected, actual );
            }

            /// <summary>
            /// Ineligible properties (generated, concurrency, read-only, not mapped) should not be in the insert list.
            /// </summary>
            
            [Theory]
            [InlineData( nameof( Model.ComputedKey ) )]
            [InlineData( nameof( Model.Concurrency ) )]
            [InlineData( nameof( Model.IdentityKey ) )]
            [InlineData( nameof( Model.NotMapped ) )]
            [InlineData( nameof( Model.ReadOnlyField ) )]
            
            public void DoesNotContain_ineligibleProperies( string propertyName )
            {
                var expected = TypeReflector[propertyName];
                Assert.DoesNotContain( expected, actual );
            }
        }

        /// <summary>
        /// Tests of the updatable property.
        /// </summary>

        public class Updatable
        {
            class Model
            {
                [Key, DatabaseGenerated( DatabaseGeneratedOption.Identity )]
                public int IdentityKey { get; set; }

                [Key, DatabaseGenerated( DatabaseGeneratedOption.Computed )]
                public int ComputedKey { get; set; }

                [Key]
                public int InsertableKey { get; set; }

                [ConcurrencyCheck]
                public int Concurrency { get; set; }

                public int UpdatableField1 { get; set; }

                [Column( Order = 0 )]
                public int UpdatableField2 { get; set; }

                public int UpdatableField3 { get; set; }

                [NotMapped]
                public int NotMapped { get; set; }

                public int ReadOnlyField { get; }
            }

            ITypeReflector<Model> TypeReflector = TypeReflectorExtensions.Reflect<Model>();
            IEnumerable<IPropertyReflector<Model>> actual = ModelReflector<Model>.Instance.Updatable;

            [Fact]
            public void Contains_updatableProperties_inColumnThenNameOrder()
            {
                var expected = new IPropertyReflector<Model>[]
                {
                    TypeReflector[nameof(Model.UpdatableField2)],
                    TypeReflector[nameof(Model.UpdatableField1)],
                    TypeReflector[nameof(Model.UpdatableField3)],
                };

                Assert.Equal( expected, actual );
            }

            /// <summary>
            /// Ineligible properties (keys, generated, concurrency, read-only, not mapped) should not be in the update list.
            /// </summary>

            [Theory]
            [InlineData( nameof( Model.ComputedKey ) )]
            [InlineData( nameof( Model.Concurrency ) )]
            [InlineData( nameof( Model.IdentityKey ) )]
            [InlineData( nameof( Model.InsertableKey ) )]
            [InlineData( nameof( Model.NotMapped ) )]
            [InlineData( nameof( Model.ReadOnlyField ) )]

            public void DoesNotContain_ineligibleProperies( string propertyName )
            {
                var expected = TypeReflector[propertyName];
                Assert.DoesNotContain( expected, actual );
            }
        }

        /// <summary>
        /// Tests of the selectable property.
        /// </summary>

        public class Selectable
        {
            class Model
            {
                [Key, DatabaseGenerated( DatabaseGeneratedOption.Identity )]
                public int IdentityKey { get; set; }

                [Key, DatabaseGenerated( DatabaseGeneratedOption.Computed )]
                public int ComputedKey { get; set; }

                [Key]
                public int InsertableKey { get; set; }

                [ConcurrencyCheck]
                public int Concurrency { get; set; }

                public int UpdatableField1 { get; set; }

                [Column( Order = 0 )]
                public int UpdatableField2 { get; set; }

                public int UpdatableField3 { get; set; }

                [NotMapped]
                public int NotMapped { get; set; }

                public int ReadOnlyField { get; }
            }

            ITypeReflector<Model> TypeReflector = TypeReflectorExtensions.Reflect<Model>();
            IEnumerable<IPropertyReflector<Model>> actual = ModelReflector<Model>.Instance.Selectable;

            [Fact]
            public void Contains_selectableProperties_inColumnThenNameOrder()
            {
                var expected = new IPropertyReflector<Model>[]
                {
                    TypeReflector[nameof(Model.UpdatableField2)],
                    TypeReflector[nameof(Model.ComputedKey)],
                    TypeReflector[nameof(Model.Concurrency)],
                    TypeReflector[nameof(Model.IdentityKey)],
                    TypeReflector[nameof(Model.InsertableKey)],
                    TypeReflector[nameof(Model.UpdatableField1)],
                    TypeReflector[nameof(Model.UpdatableField3)],
                };

                Assert.Equal( expected, actual );
            }

            /// <summary>
            /// Ineligible properties (read-only, not mapped) should not be in the update list.
            /// </summary>

            [Theory]
            [InlineData( nameof( Model.NotMapped ) )]
            [InlineData( nameof( Model.ReadOnlyField ) )]

            public void DoesNotContain_ineligibleProperies( string propertyName )
            {
                var expected = TypeReflector[propertyName];
                Assert.DoesNotContain( expected, actual );
            }
        }
    }
}