using Bogus;
using KaopizAuth.Application.DTOs.Auth;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.ValueObjects;

namespace KaopizAuth.UnitTests.TestDataBuilders;

public static class TestDataBuilders
{
    public static class Users
    {
        private static readonly Faker<User> UserFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Email, f => Email.Create(f.Internet.Email()).Value)
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.IsEmailVerified, f => f.Random.Bool())
            .RuleFor(u => u.CreatedAt, f => f.Date.Past())
            .RuleFor(u => u.UpdatedAt, f => f.Date.Recent());

        public static User Valid() => UserFaker.Generate();
        
        public static User WithEmail(string email)
        {
            var user = UserFaker.Generate();
            user.GetType().GetProperty(nameof(User.Email))?.SetValue(user, Email.Create(email).Value);
            return user;
        }

        public static User Verified()
        {
            var user = UserFaker.Generate();
            user.GetType().GetProperty(nameof(User.IsEmailVerified))?.SetValue(user, true);
            return user;
        }

        public static List<User> Multiple(int count) => UserFaker.Generate(count);
    }

    public static class DTOs
    {
        private static readonly Faker<RegisterRequestDto> RegisterRequestFaker = new Faker<RegisterRequestDto>()
            .RuleFor(d => d.Email, f => f.Internet.Email())
            .RuleFor(d => d.Password, f => f.Internet.Password(8, false, "", "@Aa1"))
            .RuleFor(d => d.FirstName, f => f.Name.FirstName())
            .RuleFor(d => d.LastName, f => f.Name.LastName());

        public static RegisterRequestDto ValidRegisterRequest() => RegisterRequestFaker.Generate();
    }
}