# 🐋 Docker Basics - Khusus untuk SonarQube

## 🎯 Anda TIDAK Perlu Jadi Expert Docker!

Panduan ini cukup untuk menjalankan SonarQube. Hanya 5-10 menit belajar!

---

## 📚 Apa itu Docker? (30 detik penjelasan)

**Docker** = Aplikasi yang menjalankan software dalam "container" (seperti box/kotak virtual).

**Keuntungan:**

- ✅ Install sekali jalan (tidak ada "works on my machine")
- ✅ Tidak bentrok dengan aplikasi lain
- ✅ Mudah hapus kalau tidak pakai lagi
- ✅ Setup database otomatis

**Analogi Sederhana:**

- Docker seperti **microwave** 🍱
- Container seperti **makanan dalam kotak**
- Anda tinggal tekan tombol, jadi!

---

## 🚀 Install Docker (5 menit)

### **macOS:**

1. Download [Docker Desktop for Mac](https://www.docker.com/products/docker-desktop)
2. Install seperti aplikasi biasa (drag & drop)
3. Buka Docker Desktop
4. Tunggu sampai icon whale muncul di menu bar atas

### **Windows:**

1. Download [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop)
2. Install dan restart komputer
3. Buka Docker Desktop
4. Pastikan WSL2 sudah enable (biasanya otomatis)

### **Linux (Ubuntu/Debian):**

```bash
sudo apt update
sudo apt install docker.io docker-compose
sudo systemctl start docker
sudo usermod -aG docker $USER
# Logout dan login lagi
```

### **Verifikasi Instalasi:**

```bash
docker --version
# Output: Docker version 24.x.x

docker-compose --version
# Output: Docker Compose version 2.x.x
```

✅ Kalau muncul versi, berarti sudah berhasil!

---

## 🎓 5 Command Docker Yang Perlu Anda Tahu

Hanya 5 command ini yang akan Anda pakai untuk SonarQube:

### 1️⃣ **Start Container** (Jalankan SonarQube)

```bash
docker-compose up -d
```

- `up` = jalankan
- `-d` = jalankan di background (detached mode)

**Kapan pakai:** Setiap kali mau nyalain SonarQube

---

### 2️⃣ **Stop Container** (Matikan SonarQube)

```bash
docker-compose down
```

- Mematikan dan hapus container (tapi data tetap ada)

**Kapan pakai:** Selesai kerja, mau matikan SonarQube

---

### 3️⃣ **Lihat Status** (Apakah SonarQube jalan?)

```bash
docker-compose ps
```

**Output kalau jalan:**

```
NAME         STATUS    PORTS
sonarqube    Up        0.0.0.0:9000->9000/tcp
sonardb      Up        5432/tcp
```

**Kapan pakai:** Untuk cek apakah SonarQube sudah nyala

---

### 4️⃣ **Lihat Log** (Troubleshooting)

```bash
docker-compose logs -f sonarqube
```

- `-f` = follow (terus update real-time)
- Tekan `Ctrl+C` untuk keluar

**Kapan pakai:** Kalau SonarQube error atau lambat startup

---

### 5️⃣ **Restart Container** (Kalau ada masalah)

```bash
docker-compose restart
```

**Kapan pakai:** Kalau SonarQube hang atau bermasalah

---

## 🔧 File Yang Anda Butuhkan

### **docker-compose.yml** (Copy-paste saja!)

Buat file ini di root project Anda:

```yaml
version: "3.7"

services:
  sonarqube:
    image: sonarqube:community
    container_name: sonarqube
    ports:
      - "9000:9000"
    environment:
      - SONAR_JDBC_URL=jdbc:postgresql://db:5432/sonar
      - SONAR_JDBC_USERNAME=sonar
      - SONAR_JDBC_PASSWORD=sonar
    depends_on:
      - db
    volumes:
      - sonarqube_data:/opt/sonarqube/data
      - sonarqube_extensions:/opt/sonarqube/extensions
      - sonarqube_logs:/opt/sonarqube/logs
    restart: unless-stopped
    mem_limit: 4g

  db:
    image: postgres:15
    container_name: sonardb
    environment:
      - POSTGRES_USER=sonar
      - POSTGRES_PASSWORD=sonar
      - POSTGRES_DB=sonar
    volumes:
      - postgresql_data:/var/lib/postgresql/data
    restart: unless-stopped

volumes:
  sonarqube_data:
  sonarqube_extensions:
  sonarqube_logs:
  postgresql_data:
```

**Penjelasan singkat:**

- `image: sonarqube:community` → Download SonarQube versi gratis
- `ports: 9000:9000` → Akses lewat http://localhost:9000
- `db: postgres:15` → Database otomatis setup
- `volumes:` → Data disimpan permanen (tidak hilang kalau restart)
- `mem_limit: 4g` → Batasi RAM maksimal 4GB

---

## 📝 Step-by-Step: Jalankan SonarQube

### **Langkah 1: Siapkan File**

```bash
cd /Users/krisnafirdaus/Desktop/default-project
# Pastikan ada file docker-compose.yml di sini
ls docker-compose.yml
```

### **Langkah 2: Start SonarQube**

```bash
docker-compose up -d
```

**Output yang diharapkan:**

```
[+] Running 2/2
 ✔ Container sonardb     Started
 ✔ Container sonarqube   Started
```

### **Langkah 3: Tunggu Startup (2-3 menit)**

```bash
# Cara 1: Cek status
docker-compose ps

# Cara 2: Lihat log sampai muncul "SonarQube is operational"
docker-compose logs -f sonarqube
```

**Tunggu sampai muncul:**

```
SonarQube is operational
```

### **Langkah 4: Akses Dashboard**

1. Buka browser
2. Ketik: `http://localhost:9000`
3. Login dengan:
   - Username: `admin`
   - Password: `admin`
4. Ganti password (wajib di first login)

### **Langkah 5: Setup Token**

1. Di dashboard, klik nama Anda (kanan atas)
2. Pilih **My Account** → **Security**
3. Klik **Generate Token**
4. Nama: `MyApp`
5. Klik **Generate**
6. **COPY TOKEN** (hanya muncul 1x!)
7. Paste ke `sonar-scan.sh` (ganti `your-token-here`)

---

## 🎯 Workflow Sehari-hari

### **Pagi - Mulai Kerja:**

```bash
cd /Users/krisnafirdaus/Desktop/default-project
docker-compose up -d
# Tunggu 2 menit
# Buka http://localhost:9000
```

### **Analisis Kode:**

```bash
./sonar-scan.sh
```

### **Sore - Selesai Kerja:**

```bash
docker-compose down
```

---

## 🔧 Troubleshooting

### **Problem 1: Port 9000 sudah dipakai**

**Error:**

```
Error: bind: address already in use
```

**Solusi:**

```bash
# Cari siapa yang pakai port 9000
lsof -i :9000

# Kill process tersebut
kill -9 <PID>

# Atau ganti port di docker-compose.yml
# Ubah "9000:9000" jadi "9001:9000"
```

---

### **Problem 2: Docker tidak jalan**

**Error:**

```
Cannot connect to Docker daemon
```

**Solusi:**

```bash
# macOS/Windows: Buka Docker Desktop dulu!

# Linux: Start docker service
sudo systemctl start docker
```

---

### **Problem 3: SonarQube lambat startup**

**Solusi:**

```bash
# Lihat log untuk cek progress
docker-compose logs -f sonarqube

# Biasanya karena:
# - RAM kurang (butuh minimal 4GB)
# - Disk penuh
# - Database belum ready
```

---

### **Problem 4: Lupa token**

**Solusi:**

```bash
# Generate token baru di dashboard:
# http://localhost:9000 → My Account → Security → Generate Token
```

---

### **Problem 5: Data hilang setelah restart**

**Solusi:**

```bash
# Pastikan pakai volumes di docker-compose.yml
# Jangan pakai 'docker-compose down -v' (ini hapus data!)
# Pakai 'docker-compose down' saja
```

---

## 📊 Monitoring

### **Cek Resource Usage:**

```bash
docker stats
```

**Output:**

```
NAME       CPU %   MEM USAGE / LIMIT
sonarqube  15%     2.5GB / 4GB
sonardb    5%      150MB / unlimited
```

### **Lihat Disk Space:**

```bash
docker system df
```

---

## 🧹 Cleanup (Kalau Tidak Pakai Lagi)

### **Hapus Container (Data Tetap Ada):**

```bash
docker-compose down
```

### **Hapus Container + Data:**

```bash
docker-compose down -v
# WARNING: Ini hapus semua data SonarQube!
```

### **Hapus Image (Hemat Disk):**

```bash
docker images
docker rmi sonarqube:community
docker rmi postgres:15
```

---

## 🎓 Konsep Penting

### **Container vs Image**

- **Image** = Template/cetakan (seperti file installer)
- **Container** = Running instance (seperti aplikasi yang jalan)

Analogi:

- Image = Resep kue 📄
- Container = Kue yang sudah jadi 🍰

### **Volumes**

- Tempat simpan data permanen
- Data tidak hilang walau container dihapus
- Seperti external hard disk untuk container

### **Network**

- Container bisa komunikasi satu sama lain
- SonarQube container bisa connect ke PostgreSQL container
- Otomatis di-setup oleh Docker Compose

---

## ✅ Checklist: Apakah Anda Sudah Siap?

- [ ] Docker Desktop terinstall
- [ ] `docker --version` menampilkan versi
- [ ] `docker-compose --version` menampilkan versi
- [ ] File `docker-compose.yml` sudah ada
- [ ] `docker-compose up -d` berhasil jalan
- [ ] `http://localhost:9000` bisa diakses
- [ ] Login admin berhasil
- [ ] Token sudah di-generate

✅ Kalau semua checklist ✓, Anda siap analisis kode dengan SonarQube!

---

## 🚀 Next Steps

Setelah SonarQube jalan:

1. Generate token di dashboard
2. Update token di `sonar-scan.sh`
3. Run: `./sonar-scan.sh`
4. Lihat hasil analisis di dashboard

---

## 📚 Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Docs](https://docs.docker.com/compose/)
- [SonarQube Docker Image](https://hub.docker.com/_/sonarqube)

---

## 💡 Tips Pro

1. **Jangan hapus volumes** kalau mau keep data
2. **Restart Docker Desktop** kalau ada masalah aneh
3. **Tutup aplikasi lain** kalau RAM kurang
4. **Gunakan SSD** untuk performa lebih baik
5. **Backup token** di tempat aman (password manager)

---

## 🎉 Kesimpulan

**Yang Perlu Anda Kuasai:**

- ✅ `docker-compose up -d` → Start
- ✅ `docker-compose down` → Stop
- ✅ `docker-compose ps` → Status
- ✅ `docker-compose logs -f` → Lihat log

**Hanya 4 command!** Sisanya akan otomatis. 🚀

**Waktu belajar:** 10 menit  
**Waktu setup:** 5 menit  
**Complexity:** ⭐⭐☆☆☆ (Mudah!)

---

**Selamat! Anda sekarang bisa jalankan SonarQube dengan Docker! 🎉**
