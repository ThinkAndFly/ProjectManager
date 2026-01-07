using AutoMapper;
using ProjectManager.Presentation.DTO;

namespace ProjectManager.Application.MapProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.Project, ProjectDTO>();
        }
    }
}
