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

using System.Collections.Generic;

namespace Reflectable
{
    /// <summary>
    /// Defines a fast reflection provider for a type.
    /// </summary>
    /// <typeparam name="T">Type to reflect.</typeparam>

    public interface ITypeReflector<T> : IEnumerable<IPropertyReflector<T>>
    {
        /// <summary>
        /// Gets the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>

        IPropertyReflector<T> this[string propertyName] { get; }

        /// <summary>
        /// Creates and returns a shallow copy of the source instance.
        /// </summary>
        /// <param name="source">Source instance to clone.</param>

        T Clone( T source );

        /// <summary>
        /// Copies the property values from the source instance to the target.
        /// </summary>
        /// <param name="source">Source instance whose property values to copy.</param>
        /// <param name="target">Target instance into which to copy the values.</param>
        /// <returns>The target instance.</returns>

        T CopyTo( T source, T target );
    }
}