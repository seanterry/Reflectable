using System;
using System.Collections;
using System.Reflection;

namespace Fidget.Extensions.Reflection.Internal
{
    /// <summary>
    /// Fast reflection provider for a property.
    /// </summary>
    /// <typeparam name="T">Declaring type of the property.</typeparam>
    /// <typeparam name="V">Type of the property value.</typeparam>

    class PropertyReflector<T, V> : IPropertyReflector<T>
    {
        /// <summary>
        /// Gets the property being reflected.
        /// </summary>

        public PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Gets whether the property is an array type.
        /// </summary>

        public bool IsArray { get; }

        /// <summary>
        /// Gets whether the property is read-only.
        /// </summary>

        public bool IsReadOnly { get; }

        /// <summary>
        /// Defines an unbound property accessor.
        /// </summary>
        /// <param name="instance">Instance whose property value to access.</param>

        delegate V AccessorDelegate( T instance );

        /// <summary>
        /// Unbound property accessor.
        /// </summary>

        readonly AccessorDelegate Accessor;

        /// <summary>
        /// Defines an unbound property mutator.
        /// </summary>
        /// <param name="instance">Instance whose property value to set.</param>
        /// <param name="value">New value of the property.</param>

        delegate void MutatorDelegate( T instance, V value );

        /// <summary>
        /// Unbound property mutator.
        /// </summary>

        readonly MutatorDelegate Mutator;
        
        /// <summary>
        /// Constructs a fast reflection provider for a property.
        /// </summary>
        /// <param name="propertyInfo">Property to reflect.</param>
        
        public PropertyReflector( PropertyInfo propertyInfo )
        {
            PropertyInfo = propertyInfo ?? throw new ArgumentNullException( nameof( propertyInfo ) );

            IsArray = PropertyInfo.PropertyType.IsArray;
            IsReadOnly = PropertyInfo.SetMethod == null;

            Accessor = (AccessorDelegate)PropertyInfo
                .GetMethod
                .CreateDelegate( typeof( AccessorDelegate ) );

            Mutator = (MutatorDelegate)PropertyInfo
                .SetMethod?
                .CreateDelegate( typeof( MutatorDelegate ) );
        }

        /// <summary>
        /// Copies the value of the property from the source instance to the target.
        /// </summary>
        /// <param name="source">Source instance containing the property value to copy.</param>
        /// <param name="target">Target instance into which to copy the property value.</param>

        public void Copy( T source, T target )
        {
            if ( source == null ) throw new ArgumentNullException( nameof( source ) );
            if ( target == null ) throw new ArgumentNullException( nameof( target ) );
            if ( IsReadOnly ) throw new InvalidOperationException( "Property is read-only" );

            var value = Accessor.Invoke( source );

            // for arrays, we want to copy the contents rather than the structure itself
            if ( IsArray && (object)value is Array array )
            {
                value = (V)array.Clone();
            }

            Mutator.Invoke( target, value );
        }

        /// <summary>
        /// Returns whether the property value of the source and target are equal.
        /// </summary>
        /// <param name="source">Source instance containing a property value to compare.</param>
        /// <param name="comparer">Target instance containing a property value to compare.</param>

        public bool Equal( T source, T comparer )
        {
            if ( source == null ) throw new ArgumentNullException( nameof( source ) );
            if ( comparer == null ) throw new ArgumentNullException( nameof( comparer ) );

            var sourceValue = Accessor.Invoke( source );
            var targetValue = Accessor.Invoke( comparer );

            // for arrays, use an element comparison instead
            if ( IsArray && sourceValue is IStructuralEquatable sv )
            {
                return sv.Equals( targetValue, StructuralComparisons.StructuralEqualityComparer );
            }

            return Equals( sourceValue, targetValue );
        }
    }
}