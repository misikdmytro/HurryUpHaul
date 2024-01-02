using AutoMapper;

using HurryUpHaul.Domain.Models.Database;

namespace HurryUpHaul.Domain.Profiles
{
    internal class AppProfile : Profile
    {
        public AppProfile()
        {
            CreateMap<Order, Contracts.Models.Order>();
            CreateMap<Restaurant, Contracts.Models.Restaurant>()
                .ForMember(dest => dest.Managers, opt => opt.MapFrom(src => src.Managers.Select(x => x.UserName)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}