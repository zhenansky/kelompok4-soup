using AutoMapper;
using MyApp.WebAPI.DTOs;
using MyApp.WebAPI.Models;

namespace MyApp.WebAPI.Mappings
{
  public class MappingProfile : Profile
  {
    public MappingProfile()
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

      // === Pemetaan untuk MenuCourse (READ, CREATE, UPDATE) ===
      CreateMap<MenuCourse, MenuCourseDto>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MenuCourseId))
          .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
          .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image));

      CreateMap<CreateMenuCourseDto, MenuCourse>()
        .ForMember(dest => dest.Image, opt => opt.Ignore());

      CreateMap<UpdateMenuCourseDto, MenuCourse>();
      // === Pemetaan untuk MyClass ===
      CreateMap<MyClass, MyClassDTO>()
          .ForMember(dest => dest.MenuCourseId, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.MenuCourseId))
          .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.Name))
          .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.Image))
          .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.MenuCourseSchedule.MenuCourse.Category.Name))
          .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => src.MenuCourseSchedule.Schedule.ScheduleDate));

      // === Pemetaan untuk Category (READ, CREATE, UPDATE) ===
      CreateMap<Category, CategoryDto>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CategoryId))
        .ForMember(dest => dest.MenuCourseCount, opt => opt.MapFrom(src => src.MenuCourses.Count))
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

      CreateMap<CreateCategoryDto, Category>()
      .ForMember(dest => dest.Image, opt => opt.Ignore());

      CreateMap<UpdateCategoryDto, Category>()
      .ForMember(dest => dest.Image, opt => opt.Ignore());

      // === Pemetaan untuk Schedule ===
      CreateMap<Schedule, ScheduleDto>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ScheduleId));
      CreateMap<CreateScheduleDto, Schedule>();
      CreateMap<UpdateScheduleDto, Schedule>();

      // === Pemetaan untuk MenuCourseSchedule ===
      CreateMap<MenuCourseSchedule, MenuCourseScheduleDto>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MSId))
          .ForMember(dest => dest.MenuCourseName, opt => opt.MapFrom(src => src.MenuCourse.Name))
          .ForMember(dest => dest.ScheduleDate, opt => opt.MapFrom(src => src.Schedule.ScheduleDate))
          .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

      CreateMap<CreateMenuCourseScheduleDto, MenuCourseSchedule>()
          .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<MSStatus>(src.Status, true)));

      CreateMap<UpdateMenuCourseScheduleDto, MenuCourseSchedule>()
          .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<MSStatus>(src.Status, true)));
    }
  }
}