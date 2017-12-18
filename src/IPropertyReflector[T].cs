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

using System.Reflection;

namespace Reflectable
{
    /// <summary>
    /// Defines a fast reflection provider for a property.
    /// </summary>
    /// <typeparam name="T">Type that declares the property.</typeparam>

    public interface IPropertyReflector<T>
    {
        /// <summary>
        /// Gets the reflection metadata that backs the current instance.
        /// </summary>

        PropertyInfo Property { get; }

        /// <summary>
        /// Gets whether the property is an array type.
        /// </summary>

        bool IsArray { get; }

        /// <summary>
        /// Gets whether the property is read-only.
        /// </summary>

        bool IsReadOnly { get; }

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

        /// <summary>
        /// Returns the current value of the property.
        /// </summary>
        /// <param name="source">Source instance containing the property value to return.</param>

        object GetValue( T source );

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        /// <param name="target">Target instance whose value to set.</param>
        /// <param name="value">Value to set.</param>

        void SetValue( T target, object value );
    }
}