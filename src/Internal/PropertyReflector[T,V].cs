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
using System.Reflection;

namespace Reflectable.Internal
{
    /// <summary>
    /// Fast reflection provider for a property.
    /// </summary>
    /// <typeparam name="T">Type that declares the property.</typeparam>
    /// <typeparam name="V">Type of the property value.</typeparam>

    public class PropertyReflector<T,V> : IPropertyReflector<T>
    {
        /// <summary>
        /// Gets the reflection metadata that backs the current instance.
        /// </summary>
        
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets whether the property is an array type.
        /// </summary>
        
        public bool IsArray { get; }

        /// <summary>
        /// Gets whether the property is read-only.
        /// </summary>
        
        public bool IsReadOnly { get; }

        /// <summary>
        /// Delegate that represents an unbound property accessor.
        /// </summary>
        /// <param name="instance">Instance whose property value to access.</param>
        
        delegate V AccessorDelegate( T instance );

        /// <summary>
        /// Defines an unbound property mutator.
        /// </summary>
        /// <param name="instance">Instance whose property value to set.</param>
        /// <param name="value">New value of the property.</param>

        delegate void MutatorDelegate( T instance, V value );

        // lazy-initialize delegates
        readonly Lazy<AccessorDelegate> accessor;
        readonly Lazy<MutatorDelegate> mutator;

        /// <summary>
        /// Unbound property accessor.
        /// </summary>

        AccessorDelegate Accessor => accessor.Value;

        /// <summary>
        /// Unbound property mutator.
        /// </summary>
        
        MutatorDelegate Mutator => mutator?.Value;

        /// <summary>
        /// Constructs a fast reflection provider for a property.
        /// </summary>
        /// <param name="property">Reflection metadata that will back the instance.</param>
        
        public PropertyReflector( PropertyInfo property )
        {
            Property = property ?? throw new ArgumentNullException( nameof(property) );

            if ( property.DeclaringType != typeof(T) ) throw new ArgumentException( $"Declaring type {property.DeclaringType} does not match the expected type {typeof(T)}", nameof(property) );
            if ( property.PropertyType != typeof(V) ) throw new ArgumentException( $"Property type {property.PropertyType} does not match the expected type {typeof(V)}", nameof(property) );

            IsArray = property.PropertyType.IsArray;
            IsReadOnly = property.SetMethod == null;

            accessor = new Lazy<AccessorDelegate>( () => (AccessorDelegate)property.GetMethod.CreateDelegate( typeof(AccessorDelegate) ) );
            mutator = new Lazy<MutatorDelegate>( () => (MutatorDelegate)property.SetMethod?.CreateDelegate( typeof(MutatorDelegate) ) );
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

        /// <summary>
        /// Returns the current value of the property.
        /// </summary>
        /// <param name="source">Source instance containing the property value to return.</param>

        public object GetValue( T source )
        {
            if ( source == null ) throw new ArgumentNullException( nameof( source ) );

            return Accessor.Invoke( source );
        }

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        /// <param name="target">Target instance whose value to set.</param>
        /// <param name="value">Value to set.</param>
        
        public void SetValue( T target, object value )
        {
            if ( target == null ) throw new ArgumentNullException( nameof( target ) );
            if ( IsReadOnly ) throw new InvalidOperationException( "Property is read-only" );

            Mutator.Invoke( target, (V)value );
        }
    }
}