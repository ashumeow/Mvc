// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace ModelBindingWebSite.Controllers
{
    [Route("ModelBinderAttribute_Product/[action]")]
    public class ModelBinderAttribute_ProductController : Controller
    {
        public string GetBinderType_UseModelBinderOnType(
            [ModelBinder(Name = "customPrefix")] ProductWithBinderOnType model)
        {
            return model.BinderType.FullName;
        }

        public string GetBinderType_UseModelBinderProviderOnType(
            [ModelBinder(Name = "customPrefix")] ProductWithBinderProviderOnType model)
        {
            return model.BinderType.FullName;
        }

        public string GetBinderType_UseModelBinder(
            [ModelBinder(BinderType = typeof(ProductModelBinder))] Product model)
        {
            return model.BinderType.FullName;
        }

        public string GetBinderType_UseModelBinderProvider(
            [ModelBinder(BinderType = typeof(ProductModelBinderProvider))] Product model)
        {
            return model.BinderType.FullName;
        }

        public class Product
        {
            public int ProductId { get; set; }

            // Will be set by the binder
            public Type BinderType { get; set; }
        }

        [ModelBinder(BinderType = typeof(ProductModelBinder))]
        public class ProductWithBinderOnType :  Product
        {
        }

        [ModelBinder(BinderType = typeof(ProductModelBinderProvider))]
        public class ProductWithBinderProviderOnType : Product
        {
        }

        private class ProductModelBinder : IModelBinder
        {
            public async Task<bool> BindModelAsync(ModelBindingContext bindingContext)
            {
                if (typeof(Product).IsAssignableFrom(bindingContext.ModelType))
                {
                    var model = (Product)Activator.CreateInstance(bindingContext.ModelType);

                    model.BinderType = GetType();

                    var key = 
                        string.IsNullOrEmpty(bindingContext.ModelName) ? 
                        "productId" : 
                        bindingContext.ModelName + "." + "productId";

                    var value = await bindingContext.ValueProvider.GetValueAsync(key);
                    model.ProductId = (int)value.ConvertTo(typeof(int));

                    bindingContext.Model = model;
                    return true;
                }

                return false;
            }
        }

        private class ProductModelBinderProvider : IModelBinderProvider
        {
            public IReadOnlyList<IModelBinder> ModelBinders
            {
                get
                {
                    return new[] { new ProductModelBinder() };
                }
            }
        }
    }
}