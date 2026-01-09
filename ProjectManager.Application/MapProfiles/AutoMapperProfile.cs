using AutoMapper;
using ProjectManager.Application.DTO;
using ProjectManager.Domain.Entities;

namespace ProjectManager.Application.MapProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Project, ProjectDTO>();
        }
    }
}
