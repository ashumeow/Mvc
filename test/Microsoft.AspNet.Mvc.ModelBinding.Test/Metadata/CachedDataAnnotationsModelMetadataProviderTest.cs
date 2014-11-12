// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class CachedDataAnnotationsModelMetadataProviderTest
    {
        [Fact]
        public void DataAnnotationsModelMetadataProvider_ReadsIncludedAndPropertyFilterProviderType_ForTypes()
        {
            // Arrange
            var type = typeof(User);
            var provider = new DataAnnotationsModelMetadataProvider();
            var expectedIncludedPropertyNames = new[] { "IsAdmin", "UserName" };
            var expectedExcludedPropertyNames = new[] { "IsAdmin", "Id" };

            // Act
            var metadata = provider.GetMetadataForType(null, type);

            // Assert
            Assert.Equal(expectedIncludedPropertyNames, metadata.BinderIncludeProperties);
            Assert.Equal(typeof(ExcludePropertiesAtType), metadata.PropertyFilterProviderType);
        }

        [Fact]
        public void 
            ModelMetadataProvider_ReadsIncludedAndPropertyFilterProviderType_OnlyAtParameterLevel_ForParameters()
        {
            // Arrange
            var type = typeof(User);
            var methodInfo = type.GetMethod("ActionWithBindAttribute");
            var provider = new DataAnnotationsModelMetadataProvider();

            // Note it does an intersection for included and a union for excluded.
            var expectedIncludedPropertyNames = new[] { "Property1", "Property2", "IncludedAndExcludedExplicitly1" };
            var expectedExcludedPropertyNames = new[] {
                "Property3", "Property4", "IncludedAndExcludedExplicitly1" };

            // Act
            var metadata = provider.GetMetadataForParameter(
                modelAccessor: null,
                methodInfo: methodInfo,
                parameterName: "param");

            // Assert
            Assert.Equal(expectedIncludedPropertyNames, metadata.BinderIncludeProperties);
            Assert.Equal(typeof(ExcludePropertiesAtParameter), metadata.PropertyFilterProviderType);
        }

        [Fact]
        public void ModelMetadataProvider_ReadsPrefixProperty_OnlyAtParameterLevel_ForParameters()
        {
            // Arrange
            var type = typeof(User);
            var methodInfo = type.GetMethod("ActionWithBindAttribute");
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            var metadata = provider.GetMetadataForParameter(
                modelAccessor: null,
                methodInfo: methodInfo,
                parameterName: "param");

            // Assert
            Assert.Equal("ParameterPrefix", metadata.BinderModelName);
        }
   
        [Fact]
        public void DataAnnotationsModelMetadataProvider_ReadsModelNameProperty_ForParameters()
        {
            // Arrange
            var type = typeof(User);
            var methodInfo = type.GetMethod("ActionWithBindAttribute");
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            var metadata = provider.GetMetadataForParameter(
                modelAccessor: null, 
                methodInfo: methodInfo, 
                parameterName: "param");

            // Assert
            Assert.Equal("TypePrefix", metadata.BinderModelName);
        }

        [Fact]
        public void DataAnnotationsModelMetadataProvider_ReadsModelNameProperty_ForTypes()
        {
            // Arrange
            var type = typeof(User);
            var methodInfo = type.GetMethod("ActionWithBindAttribute");
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            var metadata = provider.GetMetadataForParameter(
                modelAccessor: null,
                methodInfo: methodInfo,
                parameterName: "param");

            // Assert
            Assert.Equal("ParameterPrefix", metadata.BinderModelName);
        }

        [Fact]
        public void DataAnnotationsModelMetadataProvider_ReadsScaffoldColumnAttribute_ForShowForDisplay()
        {
            // Arrange
            var type = typeof(ScaffoldColumnModel);
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act & Assert
            Assert.True(provider.GetMetadataForProperty(null, type, "NoAttribute").ShowForDisplay);
            Assert.True(provider.GetMetadataForProperty(null, type, "ScaffoldColumnTrue").ShowForDisplay);
            Assert.False(provider.GetMetadataForProperty(null, type, "ScaffoldColumnFalse").ShowForDisplay);
        }

        [Fact]
        public void DataAnnotationsModelMetadataProvider_ReadsScaffoldColumnAttribute_ForShowForEdit()
        {
            // Arrange
            var type = typeof(ScaffoldColumnModel);
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act & Assert
            Assert.True(provider.GetMetadataForProperty(null, type, "NoAttribute").ShowForEdit);
            Assert.True(provider.GetMetadataForProperty(null, type, "ScaffoldColumnTrue").ShowForEdit);
            Assert.False(provider.GetMetadataForProperty(null, type, "ScaffoldColumnFalse").ShowForEdit);
        }

        [Fact]
        public void HiddenInputWorksOnProperty()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();
            var metadata = provider.GetMetadataForType(modelAccessor: null, modelType: typeof(ClassWithHiddenProperties));
            var property = metadata.Properties.First(m => string.Equals("DirectlyHidden", m.PropertyName));

            // Act
            var result = property.HideSurroundingHtml;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HiddenInputWorksOnPropertyType()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();
            var metadata = provider.GetMetadataForType(modelAccessor: null, modelType: typeof(ClassWithHiddenProperties));
            var property = metadata.Properties.First(m => string.Equals("OfHiddenType", m.PropertyName));

            // Act
            var result = property.HideSurroundingHtml;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetMetadataForProperty_WithNoBinderMetadata_GetsItFromType()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            var propertyMetadata = provider.GetMetadataForProperty(null, typeof(Person), nameof(Person.Parent));

            // Assert
            Assert.NotNull(propertyMetadata.BinderMetadata);
            var attribute = Assert.IsType<TypeBasedBinderAttribute>(propertyMetadata.BinderMetadata);
            Assert.Equal("PersonType", propertyMetadata.BinderModelName);
            Assert.Equal(new[] { "IncludeAtType" }, propertyMetadata.BinderIncludeProperties.ToArray());
        }

        [Fact]
        public void GetMetadataForProperty_WithBinderMetadataOnPropertyAndType_GetsNameFromProperty()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            var propertyMetadata = provider.GetMetadataForProperty(null, typeof(Person), nameof(Person.GrandParent));

            // Assert
            Assert.NotNull(propertyMetadata.BinderMetadata);
            var attribute = Assert.IsType<NonTypeBasedBinderAttribute>(propertyMetadata.BinderMetadata);
            Assert.Equal("GrandParentProperty", propertyMetadata.BinderModelName);
        }

#if ASPNET50
        [Fact]
        public void GetMetadataForParameter_WithNoBinderMetadata_GetsItFromType()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            var parameterMetadata = provider.GetMetadataForParameter(null,
                                                                    typeof(Person).GetMethod("Update"),
                                                                    "person");

            // Assert
            Assert.NotNull(parameterMetadata.BinderMetadata);
            var attribute = Assert.IsType<TypeBasedBinderAttribute>(parameterMetadata.BinderMetadata);
            Assert.Equal("PersonType", parameterMetadata.BinderModelName);
            Assert.Equal(new[] { "IncludeAtType" }, parameterMetadata.BinderIncludeProperties.ToArray());
        }

        [Fact]
        public void GetMetadataForParameter_WithBinderDataOnParameterAndType_GetsMetadataFromParameter()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            var parameterMetadata = provider.GetMetadataForParameter(null,
                                                                    typeof(Person).GetMethod("Save"),
                                                                    "person");

            // Assert
            Assert.NotNull(parameterMetadata.BinderMetadata);
            var attribute = Assert.IsType<NonTypeBasedBinderAttribute>(parameterMetadata.BinderMetadata);
            Assert.Equal("PersonParameter", parameterMetadata.BinderModelName);
            Assert.Empty(parameterMetadata.BinderIncludeProperties);
        }
#endif
        public class TypeBasedBinderAttribute : Attribute, IBinderMetadata, IModelNameProvider, IPropertyBindingInfo
        {
            public string Name { get; set; }

            public string[] Include { get; set; }

            public Type PropertyFilterProviderType
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class NonTypeBasedBinderAttribute : Attribute, IBinderMetadata, IModelNameProvider, IPropertyBindingInfo
        {
            public string Name { get; set; }

            public string[] Include { get; set; }

            public Type PropertyFilterProviderType
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        [TypeBasedBinder(Name = "PersonType", Include = new string[] { "IncludeAtType" })]
        public class Person
        {
            public Person Parent { get; set; }

            [NonTypeBasedBinder(Name = "GrandParentProperty", Include = new string[] { "IncludeAtProperty" })]
            public Person GrandParent { get; set; }

            public void Update(Person person)
            {
            }

            public void Save([NonTypeBasedBinder(Name = "PersonParameter", Include = new string[] { "IncludeAtParameter" })] Person person)
            {
            }
        }

        private class ScaffoldColumnModel
        {
            public int NoAttribute { get; set; }

            [ScaffoldColumn(scaffold: true)]
            public int ScaffoldColumnTrue { get; set; }

            [ScaffoldColumn(scaffold: false)]
            public int ScaffoldColumnFalse { get; set; }
        }

        [HiddenInput(DisplayValue = false)]
        private class HiddenClass
        {
            public string Property { get; set; }
        }

        private class ClassWithHiddenProperties
        {
            [HiddenInput(DisplayValue = false)]
            public string DirectlyHidden { get; set; }

            public HiddenClass OfHiddenType { get; set; }
        }

        [Bind(typeof(ExcludePropertiesAtType), Include = new[] { nameof(IsAdmin), nameof(UserName) },
             Prefix = "TypePrefix")]
        private class User
        {
            public int Id { get; set; }

            public bool IsAdmin { get; set; }

            public int UserName { get; set; }

            public int NotIncludedOrExcluded { get; set; }

            public void ActionWithBindAttribute(
                          [Bind(typeof(ExcludePropertiesAtParameter) ,
                                Include = new[] { "Property1", "Property2", "IsAdmin" },
                                Prefix = "ParameterPrefix")]
                            User param)
            {
            }
        }

        private class ExcludePropertiesAtType : IModelPropertyFilterProvider
        {
            public Func<ModelBindingContext, string, bool> PropertyFilter
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        private class ExcludePropertiesAtParameter : IModelPropertyFilterProvider
        {
            public Func<ModelBindingContext, string, bool> PropertyFilter
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}