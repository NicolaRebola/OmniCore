namespace CatalogService.Domain.CatalogTemplates;

public static class AttributeResolver
{
    public static IReadOnlyList<ResolvedAttributeValue> Resolve(
        CatalogTemplate template,
        IReadOnlyList<AttributeValue> itemValues,
        IReadOnlyList<AttributeValue> variantValues)
    {
        var resolved = new List<ResolvedAttributeValue>();

        foreach (var definition in template.Attributes)
        {
            var variantValue = FindValue(variantValues, definition.Key);
            if (variantValue is not null)
            {
                resolved.Add(new ResolvedAttributeValue(definition.Key, variantValue.Value, "variant"));
                continue;
            }

            var itemValue = FindValue(itemValues, definition.Key);
            if (itemValue is not null)
            {
                resolved.Add(new ResolvedAttributeValue(definition.Key, itemValue.Value, "item"));
                continue;
            }

            if (!string.IsNullOrWhiteSpace(definition.DefaultValue))
            {
                resolved.Add(new ResolvedAttributeValue(definition.Key, definition.DefaultValue, "default"));
            }
        }

        return resolved.AsReadOnly();
    }

    private static AttributeValue? FindValue(IReadOnlyList<AttributeValue> values, string key)
    {
        return values.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
    }
}
