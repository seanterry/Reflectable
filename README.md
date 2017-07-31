# Fidget.Extensions.Reflection
Extensions and helpers for common reflection operations:
* Shallow-copies of entire instances.
* Shallow-copies of property values between instances.
* Property equality comparison.
* Minimize overhead imposed by reflection.

### ReflectionComparer<T>
An implementation of `IEqualityComparer<T>` that performs an equality comparison using property values (see the `Equal` method of `IPropertyReflector` below for how property values are compared).

### TypeReflectorExtensions.Reflect
Returns an `ITypeReflector<T>` that provides access to reflected characteristics of a type. Currently, this is limited to the following methods:
* `Clone` - creates a shallow copy of the source object into a new instance.
* `CopyTo` - shallow-copies all writable public property values from one instance to another.

A note about the shallow copies: These operate almost identically to the `Object.MemberwiseClone` method:
> If a field is a value type, a bit-by-bit copy of the field is performed. If a field is a reference type, the reference is copied but the referred object is not; therefore, the original object and its clone refer to the same object.

Arrays, however, are copied using the `Array.Clone` method, so that the resulting array can have its elements modified independently.

Additionally, `ITypeReflector<T>` is also a collection of `IPropertyReflector<T>` and includes a default property for accessing the collection by property name.

### IPropertyReflector
Instances of `IPropertyReflector<T>` provide access to reflected characteristics of a property on its declaring type `T`.
* `PropertyInfo` - Reflected metadata for the property.
* `Copy` - Shallow-copies the property value from a source instance of the declaring type to a target instance (See note about shallow copies above).
* `Equal` - Returns whether the two property values are equal between two instances of the declaring type. This is done via `Object.Equals` except for arrays, which use `IStructuralEquatable.Equals` for a comparison of the array elements.

### ModelReflectorExtensions.Model
Returns an `IModelReflector<T>` that provides attribute-driven property information useful for the construction of database statements. These are driven by the standard attributes found in System.ComponentModel.Annotations:
* `NotMappedAttribute`: Disregards the property.
* `KeyAttribute`: Indicates the property participates in the primary key.
* `DatabaseGeneratedAttribute`: Indicates whether the property is a database-generated value.
* `ConcurrencyCheckAttribute`: Indicates the property participates in concurrency checking.
* `ColumnAttribute`: Controls column position in their collections using the `Order` property. When the attribute is not present, they are sorted alphabetically.

`IModelReflector<T>` members:
* `Table`: Gets the table attribute on the model if one exists, otherwise null.
* `Keys`: Gets the collection of properties decorated by `KeyAttribute`.
* `Generated`: Gets the collection of properties decorated by `DatabaseGeneratedAttribute` with any option other than `None`.
* `Concurrency`: Gets the collection of properties decorated by `ConcurrencyCheckAttribute`.
* `Selectable`: Gets the collection of properties that are neither read-only nor decorated by `NotMappedAttribute`.
* `Insertable`: Gets the collection of properties that are Selectable, but excluding those in the Generated and Concurrency collections.
* `Updatable`: Gets the collection of properties that are Insertable, but excluding those in the Keys collection.
* `TryGetChanges`: Detects changes to updatable properties and fills a dictionary with the names of the changed properties and their values.