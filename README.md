# Hệ thống Quản lý Thư viện - UIT

Ứng dụng quản lý thư viện được xây dựng bằng .NET 9.0 WPF và SQL Server.

## Tính năng

- **Quản lý sách**: Thêm, sửa, xóa, tìm kiếm sách
- **Quản lý độc giả**: Đăng ký, cập nhật thông tin độc giả
- **Mượn/Trả sách**: Quản lý phiếu mượn, tính phạt tự động
- **Báo cáo thống kê**: Thống kê mượn/trả, xuất báo cáo Excel/CSV
- **Quy định**: Cấu hình các quy định nghiệp vụ
- **Xác thực**: Đăng nhập/đăng ký với mã hóa password

## Yêu cầu hệ thống

- Windows 10/11
- SQL Server Express hoặc LocalDB
- .NET 9.0 Runtime (tự động có trong file .exe)

## Cách chạy

### Option 1: Chạy từ Visual Studio

```bash
dotnet run --project LibraryManagementFE.csproj
```

### Option 2: Chạy file .exe

1. Build single-file executable:
```bash
build-exe.bat
```

2. Double-click file `LibraryManagementFE.exe`

## Cấu hình Database

Chỉnh sửa `appsettings.json`:

**SQL Server Express:**
```json
{
  "ConnectionStrings": {
    "LibraryDb": "Server={YOUR SERVER NAME},{YOUR PORT};Database=QuanLyThuVien;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

**LocalDB:**
```json
{
  "ConnectionStrings": {
    "LibraryDb": "Server=(localdb)\\MSSQLLocalDB;Database=QuanLyThuVien;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

## Công nghệ sử dụng

- .NET 9.0 WPF
- Entity Framework Core 9.0
- SQL Server
- BCrypt.Net (mã hóa password)
- MVVM Pattern

## Cấu trúc dự án

```
├── Models/          # Data models
├── Views/           # WPF UI
├── ViewModels/      # MVVM ViewModels
├── Services/        # Business logic
├── Data/            # EF Core DbContext
├── Policies/        # Quy định nghiệp vụ
└── Themes/          # XAML styles
```

## Tác giả

UIT - University of Information Technology

## License

MIT License
