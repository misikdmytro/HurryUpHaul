using AutoMapper;

using HurryUpHaul.Domain.Models.Database;

namespace HurryUpHaul.Domain.Profiles
{
    internal class AppProfile : Profile
    {
        public AppProfile()
        {
            CreateMap<Order, Contracts.Models.Order>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString("G")));
        }
    }
}