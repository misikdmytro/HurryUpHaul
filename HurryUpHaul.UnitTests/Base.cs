using Bogus;


using HurryUpHaul.Domain.Databases;

using Microsoft.EntityFrameworkCore;

namespace HurryUpHaul.UnitTests.Commands
{
    public class Base : IAsyncDisposable
    {
        private protected readonly AppDbContext _appDbContext;
        protected readonly Faker _faker;

        public Base()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "HurryUpHaul")
                .Options;

            _appDbContext = new AppDbContext(options);

            _faker = new Faker();
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                await _appDbContext.DisposeAsync();
            }
        }
    }
}