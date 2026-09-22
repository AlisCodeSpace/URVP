using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace FEA.URVP.Api.Configuration;

/// <summary>
/// Marks an action that exists for local Development only. Outside Development the convention
/// drops its selectors, so the route is not part of the deployed endpoint set.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class DevelopmentOnlyAttribute : Attribute;

public sealed class DevelopmentOnlyConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        var marked = action.Attributes.OfType<DevelopmentOnlyAttribute>().Any()
            || action.ActionMethod.IsDefined(typeof(DevelopmentOnlyAttribute), inherit: true);

        if (!marked)
        {
            return;
        }

        action.Selectors.Clear();
        action.ApiExplorer.IsVisible = false;
    }
}
