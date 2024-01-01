using AutoMapper;

using Bogus;

using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Profiles;

using Microsoft.EntityFrameworkCore;

namespace HurryUpHaul.UnitTests
{
    public class Base : IAsyncDisposable
    {
        protected readonly Faker _faker;

        private protected readonly AppDbContext _appDbContext;
        private protected readonly IMapper _mapper;

        public Base()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "HurryUpHaul")
                .Options;

            _appDbContext = new AppDbContext(options);
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<AppProfile>()).CreateMapper();

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