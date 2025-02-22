using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Core.ModelBinders
{
    //c
    public class SearchParamsModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var model = Activator.CreateInstance(bindingContext.ModelType);
            var values = bindingContext.ValueProvider;

            foreach (var property in bindingContext.ModelType.GetProperties())
            {
                var rawValue = values.GetValue(property.Name).FirstValue;
                if (string.IsNullOrEmpty(rawValue)) continue;

                object convertedValue = rawValue;

                // Handle int[] (e.g., GenreIds)
                if (property.PropertyType == typeof(int[]))
                {
                    convertedValue = rawValue.Split(',')
                        .Select(x => int.TryParse(x, out var num) ? num : (int?)null)
                        .Where(x => x.HasValue)
                        .Select(x => x.Value)
                        .ToArray();
                }
                else
                {
                    // Determine if the property is nullable and extract its underlying type
                    // Otherwise, keep as is
                    var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    // Convert the value to the appropriate type
                    convertedValue = Convert.ChangeType(rawValue, targetType);
                }

                property.SetValue(model, convertedValue);
            }

            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
}
