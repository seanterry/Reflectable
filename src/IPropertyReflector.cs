using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Fidget.Extensions.Reflection
{
    /// <summary>
    /// Defines a fast reflection provider for a property.
    /// </summary>
    
    public interface IPropertyReflector
    {
        /// <summary>
        /// Gets the property being reflected.
        /// </summary>

        PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Gets whether the property is an array type.
        /// </summary>

        bool IsArray { get; }

        /// <summary>
        /// Gets whether the property is read-only.
        /// </summary>

        bool IsReadOnly { get; }
    }

    /// <summary>
    /// Defines a fast reflection provider for a property.
    /// </summary>
    /// <typeparam name="T">Declaring type of the property.</typeparam>

    public interface IPropertyReflector<T> : IPropertyReflector
    {
        /// <summary>
        /// Copies the value of the property from the source instance to the target.
        /// </summary>
        /// <param name="source">Source instance containing the property value to copy.</param>
        /// <param name="target">Target instance into which to copy the property value.</param>

        void Copy( T source, T target );

        /// <summary>
        /// Returns whether the property value of the source and target are equal.
        /// </summary>
        /// <param name="source">Source instance containing a property value to compare.</param>
        /// <param name="comparer">Target instance containing a property value to compare.</param>

        bool Equal( T source, T comparer );
    }
}