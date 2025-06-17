using AspAppLibrary.DTO;
using AspAppLibrary.EF;
using AutoMapper;

namespace AspAppLibrary
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Book, BookDTO>().ReverseMap();
            CreateMap<BorrowRecord, BorrowRecordDTO>().ReverseMap();
            CreateMap<Reader, ReaderDTO>().ReverseMap();
        }
    }
}
