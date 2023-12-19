using AutoMapper;

using HurryUpHaul.Domain.Models.Database;

namespace HurryUpHaul.Domain.Profiles
{
    internal class AppProfile : Profile
    {
        public AppProfile()
        {
            CreateMap<Order, Contracts.Models.Order>();
        }
    }
}