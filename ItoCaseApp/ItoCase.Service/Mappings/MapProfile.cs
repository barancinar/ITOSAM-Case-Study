using AutoMapper;
using ItoCase.Core.DTOs;
using ItoCase.Core.Entities;

namespace ItoCase.Service.Mappings
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            // Book Table and BookDto Mapping
            CreateMap<Book, BookDto>().ReverseMap();
        }
    }
}