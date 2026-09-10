using AutoMapper;
using Data.Entities;
using Services.DTOs.Parameters;

namespace Services.Mappings
{
    public class ParameterProfile : Profile
    {
        public ParameterProfile()
        {
            CreateMap<Parameter, ParameterDto>();
            CreateMap<CreateParameterDto, Parameter>();
        }
    }
}
