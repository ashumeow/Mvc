// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An attribute that can specify a model name or type of <see cref="IModelBinder"/> or 
    /// <see cref="IModelBinderProvider"/> to use for binding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ModelBinderAttribute : Attribute, IModelNameProvider, IBinderTypeProviderMetadata
    {
        private Type _binderType;

        /// <inheritdoc />
        public Type BinderType
        {
            get
            {
                return _binderType;
            }
            set
            {
                if (value != null)
                {
                    if (!typeof(IModelBinder).IsAssignableFrom(value) &&
                        !typeof(IModelBinderProvider).IsAssignableFrom(value))
                    {
                        throw new InvalidOperationException(
                            Resources.FormatBinderType_MustBeIModelBinderOrIModelBinderProvider(
                                value.FullName,
                                typeof(IModelBinder).FullName,
                                typeof(IModelBinderProvider).FullName));
                    }
                }

                _binderType = value;
            }
        }

        /// <inheritdoc />
        public string Name { get; set; }
    }
}