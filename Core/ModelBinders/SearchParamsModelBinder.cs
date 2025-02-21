using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ModelBinders
{
    public class SearchParamsModelBinder : IModelBinder
    {
        //c
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var values = bindingContext.ValueProvider;
            var model = Activator.CreateInstance(bindingContext.ModelType);

            foreach (var property in bindingContext.ModelType.GetProperties())
            {
                var value = values.GetValue(property.Name).FirstValue;
                if (string.IsNullOrEmpty(value)) continue;

                // Convert genreIds to int[]
                if (property.Name.Equals("genreIds", StringComparison.OrdinalIgnoreCase) && property.PropertyType == typeof(int[]))
                {
                    var genreIdsArray = value.Split(',')
                        .Select(x => int.TryParse(x, out var num) ? num : (int?)null)
                        .Where(x => x.HasValue)
                        .Select(x => x.Value)
                        .ToArray();

                    property.SetValue(model, genreIdsArray);
                }
                else
                {
                    // Bind everything else normally
                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(model, convertedValue);
                }
            }

            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
}
