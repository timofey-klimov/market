using Auth.Domain;
using Auth.Domain.Entities;
using FluentAssertions;

namespace Auth.Tests
{
    public class ResultShould
    {
        [Fact]
        public void Result_ThowOnValue_WhenErrors()
        {
            var customerResult = User.Create(Guid.NewGuid(), "", "Test", "Test", UserType.Customer);
            customerResult.Invoking(x => x.Value).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Result_Value_WhenNoErrors()
        {
            var id = Guid.NewGuid();
            var login = "test";
            var password = "tets";
            var salt = "test";

            var customerResult = User.Create(id, login, password, salt, UserType.Customer);

            customerResult.Value.Id.Should().Be(id);
            customerResult.Value.Login.Should().Be(login);
            customerResult.Value.Password.Should().Be(password);
            customerResult.Value.Salt.Should().Be(salt);
        }
    }
}