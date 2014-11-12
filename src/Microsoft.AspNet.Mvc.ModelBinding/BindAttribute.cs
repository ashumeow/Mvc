// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// This attribute can be used on action parameters and types, to indicate model level metadata.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class BindAttribute : Attribute, IModelNameProvider, IModelPropertyFilterProvider
    {
        /// <summary>
        /// Creates a new instace of <see cref="BindAttribute"/>.
        /// </summary>
        public BindAttribute()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="BindAttribute"/>.
        /// </summary>
        /// <param name="filterProviderType">The type which implements <see cref="IModelPropertyFilterProvider"/>.
        /// </param>
        public BindAttribute([NotNull] Type filterProviderType)
        {
            if (!typeof(IModelPropertyFilterProvider).IsAssignableFrom(filterProviderType))
            {
                var message = Resources.FormatTypeMustDeriveFromType(filterProviderType.FullName,
                                                                     typeof(IModelPropertyFilterProvider).FullName);
                throw new ArgumentException(message, nameof(filterProviderType));
            }

            PropertyFilterProviderType = filterProviderType;
        }

        /// <inheritdoc />
        public Type PropertyFilterProviderType { get; }

        /// <inheritdoc />
        public string[] Include { get; set; }

        // This property is exposed for back compat reasons.
        /// <summary>
        /// Allows a user to specify a particular prefix to match during model binding.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Represents the model name used during model binding.
        /// </summary>
        string IModelNameProvider.Name
        {
            get
            {
                return Prefix;
            }
        }

        public Func<ModelBindingContext, string, bool> PropertyFilter
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Checks if a given <paramref name="propertyName"/> is allowed.
        /// </summary>
        /// <param name="propertyName">Name of the property to check.</param>
        /// <param name="includeProperties"></param>
        /// <returns><c>true</c> if <paramref name="propertyName"/> exists in <paramref name="includeProperties"/>
        /// or if <paramref name="includeProperties"/> is null or empty. <c>false</c> otherwise.</returns>
        public static bool IsPropertyAllowed(string propertyName,
                                             IReadOnlyList<string> includeProperties)
        {
            var includeProperty = (includeProperties == null) ||
                                   (includeProperties.Count == 0) ||
                                   includeProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
            return includeProperty;
        }
    }
}
