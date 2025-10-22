using FluentValidation;
using Microsoft.AspNetCore.Http;
using MyApp.WebAPI.DTOs;
using System.Linq;

namespace MyApp.WebAPI.Validators
{
    //==================== CATEGORY VALIDATORS ====================
    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".svg" };

        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nama kategori tidak boleh kosong.")
                .Length(2, 100).WithMessage("Nama kategori harus antara 2 dan 100 karakter.")
                .Must(BeOnlyLettersAndSpaces).WithMessage("Nama kategori hanya boleh berisi huruf dan spasi.");
                
            RuleFor(x => x.ImageFile)
                .Must(BeAValidImageFile).WithMessage("File harus berupa gambar (jpg, jpeg, png, svg).")
                .Must(BeWithinFileSizeLimit).WithMessage($"Ukuran file maksimal adalah {MaxFileSizeInBytes / 1024 / 1024} MB.")
                .When(x => x.ImageFile != null);
        }
        
        private bool BeOnlyLettersAndSpaces(string name) => !string.IsNullOrWhiteSpace(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));

        // --- VALIDATOR BARU UNTUK FILE ---
        private bool BeAValidImageFile(IFormFile? file)
        {
            if (file == null) return true;
            var extension = Path.GetExtension(file.FileName);
            return _allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        private bool BeWithinFileSizeLimit(IFormFile? file)
        {
            if (file == null) return true;
            return file.Length <= MaxFileSizeInBytes;
        }
    }

    public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
    {
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; 
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".svg" };

        public UpdateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nama kategori tidak boleh kosong.")
                .Length(2, 100).WithMessage("Nama kategori harus antara 2 dan 100 karakter.")
                .Must(BeOnlyLettersAndSpaces).WithMessage("Nama kategori hanya boleh berisi huruf dan spasi.");

            // --- KODE BARU (SINKRONISASI) ---
            RuleFor(x => x.ImageFile) 
                .Must(BeAValidImageFile).WithMessage("File harus berupa gambar (jpg, jpeg, png, svg).")
                .Must(BeWithinFileSizeLimit).WithMessage($"Ukuran file maksimal adalah {MaxFileSizeInBytes / 1024 / 1024} MB.")
                .When(x => x.ImageFile != null);
        }
        
        private bool BeOnlyLettersAndSpaces(string name) => !string.IsNullOrWhiteSpace(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        
        private bool BeAValidImageFile(IFormFile? file)
        {
            if (file == null) return true;
            var extension = Path.GetExtension(file.FileName);
            return _allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        private bool BeWithinFileSizeLimit(IFormFile? file)
        {
            if (file == null) return true;
            return file.Length <= MaxFileSizeInBytes;
        }
    }

    //==================== MENUCOURSE VALIDATORS ====================
    public class CreateMenuCourseDtoValidator : AbstractValidator<CreateMenuCourseDto>
    {
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".svg" };

        public CreateMenuCourseDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nama course tidak boleh kosong.")
                .Length(2, 255).WithMessage("Nama course harus antara 2 dan 255 karakter.")
                .Must(BeOnlyLettersAndSpaces).WithMessage("Nama course hanya boleh berisi huruf dan spasi.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Deskripsi tidak boleh melebihi 1000 karakter.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Harga harus lebih besar dari 0.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Kategori yang valid harus dipilih.");
            RuleFor(x => x.ImageFile) 
                .Must(BeAValidImageFile).WithMessage("File harus berupa gambar (jpg, jpeg, png, svg).")
                .Must(BeWithinFileSizeLimit).WithMessage($"Ukuran file maksimal adalah {MaxFileSizeInBytes / 1024 / 1024} MB.")
                .When(x => x.ImageFile != null);
        }
        
        private bool BeOnlyLettersAndSpaces(string name) => !string.IsNullOrWhiteSpace(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        
        private bool BeAValidImageFile(IFormFile? file)
        {
            if (file == null) return true;
            var extension = Path.GetExtension(file.FileName);
            return _allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        private bool BeWithinFileSizeLimit(IFormFile? file)
        {
            if (file == null) return true;
            return file.Length <= MaxFileSizeInBytes;
        }
    }

    public class UpdateMenuCourseDtoValidator : AbstractValidator<UpdateMenuCourseDto>
    {
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; 
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".svg" };
        
        public UpdateMenuCourseDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nama course tidak boleh kosong.")
                .Length(2, 255).WithMessage("Nama course harus antara 2 dan 255 karakter.")
                .Must(BeOnlyLettersAndSpaces).WithMessage("Nama course hanya boleh berisi huruf dan spasi.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Deskripsi tidak boleh melebihi 1000 karakter.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Harga harus lebih besar dari 0.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Kategori yang valid harus dipilih.");

            RuleFor(x => x.ImageFile) 
                .Must(BeAValidImageFile).WithMessage("File harus berupa gambar (jpg, jpeg, png, svg).")
                .Must(BeWithinFileSizeLimit).WithMessage($"Ukuran file maksimal adalah {MaxFileSizeInBytes / 1024 / 1024} MB.")
                .When(x => x.ImageFile != null);
        }
        
        private bool BeOnlyLettersAndSpaces(string name) => !string.IsNullOrWhiteSpace(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));

        private bool BeAValidImageFile(IFormFile? file)
        {
            if (file == null) return true;
            var extension = Path.GetExtension(file.FileName);
            return _allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        private bool BeWithinFileSizeLimit(IFormFile? file)
        {
            if (file == null) return true;
            return file.Length <= MaxFileSizeInBytes;
        }
    }

    //==================== SCHEDULE VALIDATORS ====================
    public class CreateScheduleDtoValidator : AbstractValidator<CreateScheduleDto>
    {
        public CreateScheduleDtoValidator()
        {
            RuleFor(x => x.ScheduleDate)
                .NotEmpty().WithMessage("Tanggal jadwal tidak boleh kosong.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Tanggal jadwal harus di masa depan.");
        }
    }

    //==================== MENUCOURSE_SCHEDULE VALIDATORS ====================
    public class CreateMenuCourseScheduleDtoValidator : AbstractValidator<CreateMenuCourseScheduleDto>
    {
        public CreateMenuCourseScheduleDtoValidator()
        {
            RuleFor(x => x.AvailableSlot)
                .GreaterThanOrEqualTo(0).WithMessage("Jumlah slot tidak boleh negatif.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status tidak boleh kosong.")
                .Length(1, 50).WithMessage("Status maksimal 50 karakter.");

            RuleFor(x => x.MenuCourseId)
                .GreaterThan(0).WithMessage("MenuCourseId harus valid.");

            RuleFor(x => x.ScheduleId)
                .GreaterThan(0).WithMessage("ScheduleId harus valid.");
        }
    }

    public class UpdateMenuCourseScheduleDtoValidator : AbstractValidator<UpdateMenuCourseScheduleDto>
    {
        public UpdateMenuCourseScheduleDtoValidator()
        {
            RuleFor(x => x.AvailableSlot)
                .GreaterThanOrEqualTo(0).WithMessage("Jumlah slot tidak boleh negatif.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status tidak boleh kosong.")
                .Length(1, 50).WithMessage("Status maksimal 50 karakter.");
        }
    }
}