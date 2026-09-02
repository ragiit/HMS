using FluentValidation;
using MediatR;

namespace Identity.Application.Behaviours;

/// <summary>
/// Pipeline behavior MediatR yang mengeksekusi validators sebelum handler dipanggil.
/// Gagal validasi → melempar <see cref="ValidationException"/> (HTTP 400).
/// </summary>
public sealed class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = results.SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .Select(f => (f.PropertyName, f.ErrorMessage))
                .ToList();

            if (failures.Count > 0)
                throw new HMS.Shared.Abstractions.Exceptions.ValidationException(failures);
        }

        return await next();
    }
}