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
                .ForMember(dest => dest.Managers, opt =>
                {
                    opt.Condition(src => src.Managers != null);
                    opt.MapFrom(src => src.Managers.Select(x => x.UserName));
                })
                .ForMember(dest => dest.CreatedAt, opt =>
                {
                    opt.Condition(src => src.CreatedAt != default);
                    opt.MapFrom(src => src.CreatedAt);
                });
        }
    }
}