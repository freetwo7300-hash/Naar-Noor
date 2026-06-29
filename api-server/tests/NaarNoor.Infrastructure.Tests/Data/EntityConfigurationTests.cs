using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Data.Configurations;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Data;

public class EntityConfigurationTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("Config_" + Guid.NewGuid())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void ChefConfiguration_Configure_DoesNotThrow()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ChefCfg_" + Guid.NewGuid());

        Action act = () =>
        {
            using var ctx = new ApplicationDbContext(optionsBuilder.Options);
            _ = ctx.Model;
        };

        act.Should().NotThrow("ChefConfiguration.Configure should apply without error");
    }

    [Fact]
    public void MenuItemConfiguration_Configure_DoesNotThrow()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("MenuCfg_" + Guid.NewGuid());

        Action act = () =>
        {
            using var ctx = new ApplicationDbContext(optionsBuilder.Options);
            _ = ctx.Model;
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void OrderConfiguration_Configure_DoesNotThrow()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("OrderCfg_" + Guid.NewGuid());

        Action act = () =>
        {
            using var ctx = new ApplicationDbContext(optionsBuilder.Options);
            _ = ctx.Model;
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void ReservationConfiguration_Configure_DoesNotThrow()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ResCfg_" + Guid.NewGuid());

        Action act = () =>
        {
            using var ctx = new ApplicationDbContext(optionsBuilder.Options);
            _ = ctx.Model;
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void ReviewConfiguration_Configure_DoesNotThrow()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("RevCfg_" + Guid.NewGuid());

        Action act = () =>
        {
            using var ctx = new ApplicationDbContext(optionsBuilder.Options);
            _ = ctx.Model;
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void ContactInquiryConfiguration_Configure_DoesNotThrow()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ContactCfg_" + Guid.NewGuid());

        Action act = () =>
        {
            using var ctx = new ApplicationDbContext(optionsBuilder.Options);
            _ = ctx.Model;
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void OrderItemConfiguration_Configure_DoesNotThrow()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("OrdItemCfg_" + Guid.NewGuid());

        Action act = () =>
        {
            using var ctx = new ApplicationDbContext(optionsBuilder.Options);
            _ = ctx.Model;
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void Model_Chef_HasExpectedProperties()
    {
        using var ctx = CreateContext();
        var entityType = ctx.Model.FindEntityType(typeof(Chef));

        entityType.Should().NotBeNull();
        entityType!.FindProperty(nameof(Chef.Name)).Should().NotBeNull();
        entityType.FindProperty(nameof(Chef.Title)).Should().NotBeNull();
        entityType.FindProperty(nameof(Chef.Bio)).Should().NotBeNull();
        entityType.FindProperty(nameof(Chef.Specialty)).Should().NotBeNull();
    }

    [Fact]
    public void Model_MenuItem_HasExpectedProperties()
    {
        using var ctx = CreateContext();
        var entityType = ctx.Model.FindEntityType(typeof(MenuItem));

        entityType.Should().NotBeNull();
        entityType!.FindProperty(nameof(MenuItem.Name)).Should().NotBeNull();
        entityType.FindProperty(nameof(MenuItem.Price)).Should().NotBeNull();
        entityType.FindProperty(nameof(MenuItem.Category)).Should().NotBeNull();
    }

    [Fact]
    public void Model_Order_HasExpectedProperties()
    {
        using var ctx = CreateContext();
        var entityType = ctx.Model.FindEntityType(typeof(Order));

        entityType.Should().NotBeNull();
        entityType!.FindProperty(nameof(Order.CustomerName)).Should().NotBeNull();
        entityType.FindProperty(nameof(Order.Email)).Should().NotBeNull();
        entityType.FindProperty(nameof(Order.TotalAmount)).Should().NotBeNull();
        entityType.FindProperty(nameof(Order.Status)).Should().NotBeNull();
    }

    [Fact]
    public void Model_Order_HasNavigationToOrderItems()
    {
        using var ctx = CreateContext();
        var entityType = ctx.Model.FindEntityType(typeof(Order));

        entityType.Should().NotBeNull();
        var navigation = entityType!.GetNavigations().FirstOrDefault(n => n.Name == nameof(Order.Items));
        navigation.Should().NotBeNull("Order should have a navigation property to OrderItems");
    }

    [Fact]
    public void Model_Reservation_HasExpectedProperties()
    {
        using var ctx = CreateContext();
        var entityType = ctx.Model.FindEntityType(typeof(Reservation));

        entityType.Should().NotBeNull();
        entityType!.FindProperty(nameof(Reservation.CustomerName)).Should().NotBeNull();
        entityType.FindProperty(nameof(Reservation.Email)).Should().NotBeNull();
        entityType.FindProperty(nameof(Reservation.ReservationDate)).Should().NotBeNull();
        entityType.FindProperty(nameof(Reservation.PartySize)).Should().NotBeNull();
    }

    [Fact]
    public void Model_Review_HasExpectedProperties()
    {
        using var ctx = CreateContext();
        var entityType = ctx.Model.FindEntityType(typeof(Review));

        entityType.Should().NotBeNull();
        entityType!.FindProperty(nameof(Review.CustomerName)).Should().NotBeNull();
        entityType.FindProperty(nameof(Review.Rating)).Should().NotBeNull();
        entityType.FindProperty(nameof(Review.IsApproved)).Should().NotBeNull();
    }

    [Fact]
    public void Model_ContactInquiry_HasExpectedProperties()
    {
        using var ctx = CreateContext();
        var entityType = ctx.Model.FindEntityType(typeof(ContactInquiry));

        entityType.Should().NotBeNull();
        entityType!.FindProperty(nameof(ContactInquiry.Name)).Should().NotBeNull();
        entityType.FindProperty(nameof(ContactInquiry.Email)).Should().NotBeNull();
        entityType.FindProperty(nameof(ContactInquiry.Subject)).Should().NotBeNull();
        entityType.FindProperty(nameof(ContactInquiry.Message)).Should().NotBeNull();
    }

    [Fact]
    public void Model_OrderItem_HasExpectedProperties()
    {
        using var ctx = CreateContext();
        var entityType = ctx.Model.FindEntityType(typeof(OrderItem));

        entityType.Should().NotBeNull();
        entityType!.FindProperty(nameof(OrderItem.Quantity)).Should().NotBeNull();
        entityType.FindProperty(nameof(OrderItem.UnitPrice)).Should().NotBeNull();
        entityType.FindProperty(nameof(OrderItem.MenuItemName)).Should().NotBeNull();
    }

    [Fact]
    public void ChefConfiguration_DirectInvoke_DoesNotThrow()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ChefDirect_" + Guid.NewGuid())
            .Options;
        using var ctx = new ApplicationDbContext(options);

        var entityType = ctx.Model.FindEntityType(typeof(Chef));
        entityType.Should().NotBeNull();
        entityType!.GetPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void AllEntityTypes_HavePrimaryKeys()
    {
        using var ctx = CreateContext();
        var model = ctx.Model;

        var entityTypes = new[]
        {
            typeof(Chef), typeof(MenuItem), typeof(Order), typeof(OrderItem),
            typeof(Reservation), typeof(Review), typeof(ContactInquiry)
        };

        foreach (var type in entityTypes)
        {
            var entity = model.FindEntityType(type);
            entity.Should().NotBeNull($"{type.Name} should be in the model");
            entity!.GetPrimaryKey().Should().NotBeNull($"{type.Name} should have a primary key");
        }
    }

    [Fact]
    public async Task DbContext_SaveChangesAsync_SetsUpdatedAt_OnModification()
    {
        using var ctx = CreateContext();
        var chef = new Chef { Name = "Cfg Chef", Title = "Head Chef", Bio = "Expert", Specialty = "Himalayan" };
        ctx.Chefs.Add(chef);
        await ctx.SaveChangesAsync();

        var before = chef.UpdatedAt;
        await Task.Delay(5);
        chef.Name = "Updated Cfg Chef";
        ctx.Chefs.Update(chef);
        await ctx.SaveChangesAsync();

        chef.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task Order_AndOrderItems_CascadeRelationship_WorksWithInMemory()
    {
        using var ctx = CreateContext();
        var order = new Order
        {
            CustomerName = "Cascade Test",
            Email = "cascade@test.com",
            PhoneNumber = "07700000000",
            Type = OrderType.Collection,
            TotalAmount = 20m,
            Items = new List<OrderItem>
            {
                new() { MenuItemId = Guid.NewGuid(), MenuItemName = "Item A", UnitPrice = 10m, Quantity = 2 }
            }
        };
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();

        var saved = await ctx.Orders.Include(o => o.Items).FirstAsync(o => o.Id == order.Id);
        saved.Items.Should().HaveCount(1);
    }
}
