# 🍲 Kelompok 4 – SOUP App

Selamat datang di proyek **SOUP** 👋 — aplikasi modern berbasis **.NET Blazor** dan **ASP.NET Core Web API** yang dirancang untuk mempermudah pengelolaan menu, transaksi, dan data restoran.

---

## 🚀 Tentang Proyek

**SOUP** adalah sistem manajemen restoran yang dikembangkan oleh **Kelompok 4**. Aplikasi ini menggabungkan teknologi **frontend Blazor**, **backend Web API**, dan **Docker** untuk memberikan pengalaman cepat, aman, dan responsif.

### 🧩 Fitur Utama

- 🖥️ **Blazor UI Modern** – Dibangun dengan **MudBlazor** & **Tailwind-style design**.
- ⚙️ **ASP.NET Core Web API** – Backend modular & efisien.
- 📊 **ApexCharts.Blazor** – Visualisasi data interaktif.
- 💾 **Blazored.LocalStorage** – Penyimpanan sisi klien.
- 🐳 **Docker Support** – Siap dijalankan di container.

---

## 🗂️ Struktur Direktori

```
src/
 ├── 06.WebAPI/        # Backend (ASP.NET Core Web API)
 ├── 08.BlazorUI/       # Frontend (Blazor UI)
 └── ...                # Folder pendukung lain
```

---

## 🧰 Prasyarat

Pastikan kamu sudah menginstal:

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/)
- [Git](https://git-scm.com/)

Opsional:

- Visual Studio / VS Code untuk development.

---

## 💻 Cara Menjalankan

### 🔹 Backend (Web API)

```bash
cd src/06.WebAPI
dotnet restore
dotnet build
dotnet ef database update  # jika menggunakan migrasi EF Core
dotnet run
```

➡️ Akses di: `https://localhost:5001` atau `http://localhost:5000`

### 🔹 Frontend (Blazor UI)

```bash
cd src/08.BlazorUI
dotnet restore
dotnet build
dotnet run
```

➡️ Akses di: `http://localhost:5099` atau `http://localhost:5099/swagger/index.html`

---

## 🐋 Menjalankan Menggunakan Docker

> Jalankan semua service (frontend + backend) dengan sekali perintah.

```bash
docker compose -f docker-compose.custom-domain.yml up --build
```

Setelah build selesai, buka browser di `http://localhost`.

---

## ⚙️ Konfigurasi Environment

Buat file `.env` di root proyek:

```env
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:80
ConnectionStrings__DefaultConnection=Server=localhost;Database=soupdb;User Id=sa;Password=YourPassword;
ApiBaseUrl=http://localhost:5000
```

> 💡 **Tips:** Jangan commit file `.env` ke repository publik.

---

## 🧾 Perintah Migrasi Database (EF Core)

```bash
cd src/06.WebAPI
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Jika error “exclusive lock for migration”, pastikan tidak ada proses `dotnet` lain yang mengakses DB.

---

## 💡 Tips & Solusi Cepat

| Masalah                   | Solusi                                                                          |
| ------------------------- | ------------------------------------------------------------------------------- |
| 🔒 _Migration lock_       | Tutup semua instance Visual Studio & jalankan ulang `dotnet ef database update` |
| 🐳 _Dockerfile not found_ | Pastikan path `context` dan `dockerfile` di `docker-compose.yml` benar          |
| ⚠️ _Blazor UI blank page_ | Cek `ApiBaseUrl` di `.env` sudah mengarah ke alamat Web API                     |

---

## 👥 Cara Berkontribusi

1. Fork repository ini 🍴
2. Buat branch baru: `git checkout -b feature/nama-fitur`
3. Lakukan perubahan & commit: `git commit -m "Tambah fitur X"`
4. Push ke branch: `git push origin feature/nama-fitur`
5. Buka Pull Request 🧩

---

## 📅 Rencana Pengembangan

- ***

## 👨‍💻 Tim Pengembang

**Kelompok 4 – SOUP Project**
Kontributor utama: [ZhenanSky](https://github.com/zhenansky). [AdiDharma] (https://github.com/adidarma24). [Dean] (https://github.com/Dean-Tr)

---

## 📜 Lisensi

Proyek ini dilisensikan di bawah **MIT License** — bebas digunakan & dimodifikasi dengan tetap mencantumkan kredit.

---

> Dibuat dengan ❤️ oleh Kelompok 4
> 🚀 Powered by .NET, Blazor, dan Docker
