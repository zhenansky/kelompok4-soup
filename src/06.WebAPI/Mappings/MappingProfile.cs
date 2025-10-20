using AutoMapper;
using MyApp.WebAPI.DTOs;
using MyApp.WebAPI.Models;

namespace MyApp.WebAPI.Mappings
{
  public class InvoiceProfile : Profile
  {
    public InvoiceProfile()
    {
      CreateMap<Invoice, InvoiceDTO>()
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

      CreateMap<Invoice, DetailInvoiceDTO>()
          .ForMember(dest => dest.ListCourse, opt => opt.MapFrom(src => src.InvoiceMenuCourses));

      CreateMap<InvoiceMenuCourse, CourseItemDTO>()
          .ForMember(dest => dest.MenuCourseId, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.MenuCourseId))
          .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.Name))
          .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.Category.Name))
          .ForMember(dest => dest.ScheduleDate, opt => opt.MapFrom(src => src.MenuCourseSchedule.Schedule.ScheduleDate))
          .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.Price));

      CreateMap<MyClass, MyClassDTO>()
          .ForMember(dest => dest.MenuCourseId, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.MenuCourseId))
          .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.Name))
          .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.Image))
          .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.Category.Name))
          .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => src.MenuCourseSchedule.Schedule.ScheduleDate));
    }
  }
}
