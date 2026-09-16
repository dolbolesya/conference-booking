using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConferenceBooking.API.Infrastructure;

/// <summary>
/// Знаходить валідатор для кожного аргументу дії й застосовує його до виконання методу.
/// Клієнт отримує всі помилки одразу, а не по одній.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _services;

    public ValidationFilter(IServiceProvider services) => _services = services;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var argument in context.ActionArguments.Values.Where(a => a is not null))
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(argument!.GetType());

            if (_services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument));

            if (result.IsValid)
                continue;

            foreach (var group in result.Errors.GroupBy(e => e.PropertyName))
                errors[group.Key] = group.Select(e => e.ErrorMessage).ToArray();
        }

        if (errors.Count > 0)
        {
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors)
            {
                Title = "Помилка валідації запиту",
                Instance = context.HttpContext.Request.Path
            });

            return;
        }

        await next();
    }
}