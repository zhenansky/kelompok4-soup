using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.WebAPI.Models;
using MyApp.WebAPI.DTO.Users;
using MyApp.WebAPI.Services.Interfaces;

namespace MyApp.WebAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UserService(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ======================
        // GET ALL USERS
        // ======================
        public async Task<object> GetAllUsersAsync(int page, int pageSize)
        {
            var query = _userManager.Users.AsNoTracking();
            var total = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var usersWithRoles = new List<UserResponse>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var role = roles.FirstOrDefault() ?? "User";
                usersWithRoles.Add(UserResponse.FromEntity(u, role));
            }

            return new
            {
                success = true,
                message = "Data user berhasil diambil",
                data = new { total, page, pageSize, users = usersWithRoles },
                timestamp = DateTime.UtcNow
            };
        }

        // ======================
        // GET USER BY ID
        // ======================
        public async Task<object> GetUserByIdAsync(int id)
        {
            var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return new { success = false, message = $"User dengan ID {id} tidak ditemukan", data = (object?)null, timestamp = DateTime.UtcNow };

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            return new
            {
                success = true,
                message = "Data user berhasil diambil",
                data = new { user = UserResponse.FromEntity(user, role) },
                timestamp = DateTime.UtcNow
            };
        }

        // ======================
        // CREATE USER
        // ======================
        public async Task<object> CreateUserAsync(CreateUserRequest request)
        {
            var email = request.Email?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(email))
                return new { success = false, message = "Email tidak boleh kosong", timestamp = DateTime.UtcNow };

            if (await _userManager.FindByEmailAsync(email) != null)
                return new { success = false, message = "Email sudah digunakan", timestamp = DateTime.UtcNow };

            var role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim();
            if (role != "Admin" && role != "User")
                return new { success = false, message = "Role tidak valid", timestamp = DateTime.UtcNow };

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole<int>(role));

            var user = new User
            {
                UserName = email,
                Email = email,
                Name = request.Name,
                Status = request.Status, // ✅ langsung dari enum
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return new { success = false, message = "Gagal membuat user", error = result.Errors.Select(e => e.Description), timestamp = DateTime.UtcNow };

            await _userManager.AddToRoleAsync(user, role);

            return new
            {
                success = true,
                message = $"User dengan role '{role}' berhasil dibuat",
                data = new { user = UserResponse.FromEntity(user, role) },
                timestamp = DateTime.UtcNow
            };
        }

        // ======================
        // UPDATE USER
        // ======================
        public async Task<object> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return new { success = false, message = $"User dengan ID {id} tidak ditemukan", timestamp = DateTime.UtcNow };

            var newEmail = request.Email?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(newEmail))
                return new { success = false, message = "Email tidak boleh kosong", timestamp = DateTime.UtcNow };

            var existingUser = await _userManager.FindByEmailAsync(newEmail);
            if (existingUser != null && existingUser.Id != user.Id)
                return new { success = false, message = "Email sudah digunakan oleh user lain", timestamp = DateTime.UtcNow };

            user.Name = request.Name;
            user.Email = newEmail;
            user.UserName = newEmail;
            user.Status = request.Status; // ✅ gunakan enum dari DTO
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return new { success = false, message = "Gagal memperbarui user", error = result.Errors.Select(e => e.Description), timestamp = DateTime.UtcNow };

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            return new
            {
                success = true,
                message = "User berhasil diperbarui",
                data = new { user = UserResponse.FromEntity(user, role) },
                timestamp = DateTime.UtcNow
            };
        }

        // ======================
        // DELETE USER
        // ======================
        public async Task<object> DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return new { success = false, message = $"User dengan ID {id} tidak ditemukan", timestamp = DateTime.UtcNow };

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return new { success = false, message = "Gagal menghapus user", error = result.Errors.Select(e => e.Description), timestamp = DateTime.UtcNow };

            return new
            {
                success = true,
                message = "User berhasil dihapus",
                data = new { user = UserResponse.FromEntity(user, role) },
                timestamp = DateTime.UtcNow
            };
        }
    }
}
