using Bogus;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.ValueObjects;

namespace KaopizAuth.IntegrationTests.TestDataBuilders;

public class UserTestDataBuilder
{
    private readonly Faker<User> _faker;

    public UserTestDataBuilder()
    {
        _faker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Email, f => Email.Create(f.Internet.Email()).Value)
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.IsEmailVerified, f => f.Random.Bool())
            .RuleFor(u => u.CreatedAt, f => f.Date.Past())
            .RuleFor(u => u.UpdatedAt, f => f.Date.Recent());
    }

    public UserTestDataBuilder WithEmail(string email)
    {
        _faker.RuleFor(u => u.Email, Email.Create(email).Value);
        return this;
    }

    public UserTestDataBuilder WithVerifiedEmail()
    {
        _faker.RuleFor(u => u.IsEmailVerified, true);
        return this;
    }

    public UserTestDataBuilder WithUnverifiedEmail()
    {
        _faker.RuleFor(u => u.IsEmailVerified, false);
        return this;
    }

    public UserTestDataBuilder WithName(string firstName, string lastName)
    {
        _faker.RuleFor(u => u.FirstName, firstName);
        _faker.RuleFor(u => u.LastName, lastName);
        return this;
    }

    public User Build() => _faker.Generate();

    public List<User> BuildMany(int count) => _faker.Generate(count);
}