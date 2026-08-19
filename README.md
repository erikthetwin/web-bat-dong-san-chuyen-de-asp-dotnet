# Website mua bán bất động sản (Chuyên đề ASP.NET)

Đồ án xây dựng website mua bán bất động sản - ASP.NET Core MVC (.NET 8), EF Core + SQLite, ASP.NET Identity, Bootstrap 5, Leaflet, ML.NET.

## Chạy ứng dụng

1. Yêu cầu: .NET 8 SDK.
2. `dotnet restore`
3. `dotnet run` (tạo DB `realestate.db` và dữ liệu mẫu tự động; lần đầu huấn luyện mô hình ML ~30-60s)
4. Mở `http://localhost:5000` (hoặc cổng in ra console).

## Tài khoản mặc định

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Admin | admin@demo.com | Admin@123 |
| Seller | seller@demo.com | Seller@123 |
| Buyer | đăng ký mới | — |

## Chức năng

- Khách: xem tin, tìm kiếm + lọc (từ khóa, quận, loại, giá, diện tích, phòng, mua/thuê), xem chi tiết, bản đồ, dự đoán giá ML.
- Người mua: lưu tin yêu thích, gửi liên hệ người bán, quản lý hồ sơ.
- Người bán: đăng/chỉnh sửa/xóa tin, tải lên hình ảnh (tin phải được admin duyệt).
- Quản trị: duyệt tin, quản lý người dùng (khóa/đổi vai trò/xóa), quản lý loại bất động sản, thống kê báo cáo.

## Kiểm thử

`dotnet test Tests/Tests.csproj` — kiểm thử dịch vụ tìm kiếm/lọc (5 test).