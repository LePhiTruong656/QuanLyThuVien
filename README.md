# QuanLyThuVien

Hệ thống quản lý thư viện (WPF .NET 9): quản lý độc giả, sách, mượn/trả, phạt và báo cáo.

## Công nghệ

- **UI:** WPF (.NET 9)
- **Cơ sở dữ liệu:** Microsoft SQL Server (Entity Framework Core 9)

## Yêu cầu

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server hoặc [SQL Server LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) (mặc định trong `appsettings.json`)

## Cấu hình database

### Cách A — Docker (không cần cài SQL Server trên Windows)

```powershell
docker compose up -d
```

Tạo `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "LibraryDb": "Server=localhost,1433;Database=QuanLyThuVien;User Id=sa;Password=Library_Dev_Pass123!;TrustServerCertificate=True"
  }
}
```

### Cách B — SQL Server LocalDB / Express

1. Sao chép file mẫu (nếu dùng server riêng, có user/password):

   ```powershell
   copy appsettings.Development.json.example appsettings.Development.json
   ```

2. Chỉnh connection string trong `appsettings.json` hoặc `appsettings.Development.json`:

   ```json
   "ConnectionStrings": {
     "LibraryDb": "Server=(localdb)\\mssqllocaldb;Database=QuanLyThuVien;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

   Hoặc đặt biến môi trường `LIBRARY_DB_CONNECTION` (hữu ích trên CI / máy khác).

3. Tạo/cập nhật schema (chạy một lần sau khi clone):

   ```powershell
   cd QuanLyThuVien-main
   dotnet tool install --global dotnet-ef
   dotnet ef database update
   ```

   Ứng dụng cũng tự gọi `Database.Migrate()` khi khởi động lần đầu.

## Chạy ứng dụng

```powershell
cd QuanLyThuVien-main
dotnet run
```

## Cấu trúc dữ liệu

| Bảng SQL      | Mô tả              |
|---------------|--------------------|
| `Readers`     | Độc giả            |
| `Books`       | Sách               |
| `Borrows`     | Phiếu mượn/trả     |
| `Payments`    | Thanh toán phạt    |

Dữ liệu mẫu được tạo tự động lần đầu khi database trống (trong `BorrowService`).

## Đẩy lên GitHub

```powershell
git init
git add .
git commit -m "Add SQL Server database with EF Core"
git branch -M main
git remote add origin https://github.com/YOUR_USER/QuanLyThuVien.git
git push -u origin main
```

**Lưu ý:** Không commit `appsettings.Development.json` (đã có trong `.gitignore`). Chỉ commit `appsettings.json` với LocalDB hoặc connection string an toàn cho môi trường dev chung.

## Migration mới (khi đổi model)

```powershell
dotnet ef migrations add TenMigration
dotnet ef database update
```
