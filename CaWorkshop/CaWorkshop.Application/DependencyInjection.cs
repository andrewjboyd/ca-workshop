using AutoMapper;
using CaWorkshop.Application.Common.Behaviours;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace CaWorkshop.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>),
                typeof(ValidationBehavior<,>));

            services.AddAllRequestValidators();

            return services;
        }

        private static IServiceCollection AddAllRequestValidators(
            this IServiceCollection services)
        {
            var validatorType = typeof(IValidator<>);

            var validatorTypes = Assembly.GetExecutingAssembly()
                .GetExportedTypes()
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == validatorType))
                .ToList();

            foreach (var validator in validatorTypes)
            {
                var requestType = validator.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IValidator<>))
                    .Select(i => i.GetGenericArguments()[0])
                    .First();

                var validatorInterface = validatorType
                    .MakeGenericType(requestType);

                services.AddTransient(validatorInterface, validator);
            }

            return services;
        }
    }
}