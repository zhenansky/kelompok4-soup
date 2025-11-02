using MyApp.BlazorUI.DTOs;
using System.Threading.Tasks;

namespace MyApp.BlazorUI.Services.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileDto?> GetProfileAsync();
    }
}
