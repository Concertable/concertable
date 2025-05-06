using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Core.ModelBinders
{
    /*
     *   I acknowledge the use of OpenAI's ChatGPT (https://chat.openai.com/) to generate materials for background research and independent study, 
     *   which I have adapted and included within the work submitted for assessment. I confirm that all use of AI content is acknowledged and referenced appropriately
     *   
     *   The following prompts were input into ChatGPT:
     *   Create a custom model binder in .NET 8 that binds a full model object from query parameters. 
     *   It should loop through all attributes, while handling properties of type int[] by splitting CSVs into int[]. 
     *   For all other property types, leave them as is 
     *   
     *   The output obtained was:
     *   public Task BindModelAsync(ModelBindingContext bindingContext)
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
     *  Full detail of how the output was adapted:
     *  Added a null fallback for other attributes in the Model that AI didnt consider
                        
        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        OpenAI (2025) ChatGPT [online]. Available from: https://chat.openai.com/ [Accessed 10 Apr 2025].
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
                        // Determine if the property is nullable and extract its underlying type
                        // Otherwise, keep as is
                        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                        // Convert the value to the appropriate type
                        convertedValue = Convert.ChangeType(rawValue, targetType);
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
