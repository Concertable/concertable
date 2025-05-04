using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Core.ModelBinders
{
    /*
     *   Create me custom model binder in .NET that binds a full model object from query parameters. 
     *   The binder should handle int[] properties by splitting comma-separated values, and use default conversion for all other property types. 
     *   This will be applied to a full object within a controller
     */
    public class SearchParamsModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelType = bindingContext.ModelType;
            var modelInstance = Activator.CreateInstance(modelType);

            foreach (var property in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var valueProviderResult = bindingContext.ValueProvider.GetValue(property.Name);
                if (valueProviderResult == ValueProviderResult.None)
                    continue;

                var rawValue = valueProviderResult.FirstValue;

                try
                {
                    object convertedValue;
                    if (property.PropertyType == typeof(int[]))
                    {
                        var parts = rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        convertedValue = parts.Select(int.Parse).ToArray();
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(rawValue, property.PropertyType);
                    }

                    property.SetValue(modelInstance, convertedValue);
                }
                catch
                {
                    bindingContext.ModelState.AddModelError(property.Name, $"Invalid value for {property.Name}");
                }
            }
            bindingContext.Result = ModelBindingResult.Success(modelInstance);
            return Task.CompletedTask;
        }
    }
    }
