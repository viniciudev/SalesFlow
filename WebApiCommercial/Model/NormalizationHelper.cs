using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class NormalizationHelper
    {
        public static void NormalizeEntity(object entity)
        {
            if (entity == null) return;

            var properties = entity.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.CanWrite);

            foreach (var prop in properties)
            {
                var value = (string)prop.GetValue(entity);

                if (string.IsNullOrWhiteSpace(value)) continue;

                // Verifica atributos
                if (Attribute.IsDefined(prop, typeof(UppercaseAttribute)))
                {
                    prop.SetValue(entity, value.Trim().ToUpperInvariant());
                }
                else if (Attribute.IsDefined(prop, typeof(LowercaseAttribute)))
                {
                    prop.SetValue(entity, value.Trim().ToLowerInvariant());
                }
                // Campos sem atributo não são modificados
            }
        }

        public static void NormalizeEntities(IEnumerable<object> entities)
        {
            foreach (var entity in entities)
            {
                NormalizeEntity(entity);
            }
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UppercaseAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LowercaseAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NoNormalizationAttribute : Attribute { }
}
