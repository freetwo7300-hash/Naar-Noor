using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NaarNoor.Application.Common.Behaviours;
using NaarNoor.Application.Contact.Commands.SubmitInquiry;
using NaarNoor.Application.Orders.Commands.CreateOrder;
using NaarNoor.Application.Reservations.Commands.CreateReservation;
using Xunit;

namespace NaarNoor.Application.Tests;

public class ApplicationDependencyInjectionTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddApplication_RegistersMediatR()
    {
        using var provider = BuildProvider();

        var mediator = provider.GetService<IMediator>();

        mediator.Should().NotBeNull("MediatR should be registered by AddApplication");
    }

    [Fact]
    public void AddApplication_RegistersValidatorsFromAssembly()
    {
        using var provider = BuildProvider();

        var validator = provider.GetService<IValidator<SubmitInquiryCommand>>();

        validator.Should().NotBeNull("FluentValidation validators should be registered by AddApplication");
    }

    [Fact]
    public void AddApplication_RegistersCreateOrderValidator()
    {
        using var provider = BuildProvider();

        var validator = provider.GetService<IValidator<CreateOrderCommand>>();

        validator.Should().NotBeNull("CreateOrderCommandValidator should be registered");
    }

    [Fact]
    public void AddApplication_RegistersCreateReservationValidator()
    {
        using var provider = BuildProvider();

        var validator = provider.GetService<IValidator<CreateReservationCommand>>();

        validator.Should().NotBeNull("CreateReservationCommandValidator should be registered");
    }

    [Fact]
    public void AddApplication_RegistersValidationBehaviourPipeline()
    {
        using var provider = BuildProvider();

        var behaviors = provider.GetServices(typeof(IPipelineBehavior<,>));

        behaviors.Should().NotBeNull();
    }

    [Fact]
    public void AddApplication_ReturnsServiceCollection_ForChaining()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var result = services.AddApplication();

        result.Should().BeSameAs(services, "AddApplication should return the same IServiceCollection for chaining");
    }

    [Fact]
    public void AddApplication_RegistersISender()
    {
        using var provider = BuildProvider();

        var sender = provider.GetService<ISender>();

        sender.Should().NotBeNull("ISender should be available via MediatR registration");
    }

    [Fact]
    public void AddApplication_CalledMultipleTimes_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        Action act = () =>
        {
            services.AddApplication();
            services.AddApplication();
        };

        act.Should().NotThrow("Calling AddApplication multiple times should be idempotent");
    }
}
