# Website mua bán bất động sản (Chuyên đề ASP.NET)

Đồ án xây dựng website mua bán bất động sản — ASP.NET Core MVC (.NET 8), EF Core + SQLite, ASP.NET Identity, Bootstrap 5, Leaflet, ML.NET.

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

## Kiểm thử trực tiếp (Live Usage Test)

Kiểm thử thao tác thật trên trình duyệt (Playwright) chống lại server đang chạy. Báo cáo kết quả
đầy đủ tại `LIVE-USAGE-TEST.md` (kịch bản + hướng dẫn thao tác + kết quả mong đợi/thực tế + kiểm chứng DB).

### 1. Chạy server với live reload

```powershell
dotnet watch run --non-interactive --urls http://localhost:5000
```

- Lần đầu chạy tự tạo DB `realestate.db` + dữ liệu mẫu + huấn luyện mô hình ML (~30-60s, lưu `ML/model.zip` + `ML/metrics.json`).
- Mọi thay đổi code được biên dịch lại và tải lại tự động; không cần khởi động lại.
- Nếu server chết âm thầm: tắt mọi tiến trình `dotnet`/`webapp demo` rồi chạy lại lệnh trên.

### 2. Cấu hình trình duyệt (Brave + CDP)

```powershell
Start-Process "C:\Users\erikthetwin\AppData\Local\BraveSoftware\Brave-Browser\Application\brave.exe" `
  -ArgumentList "--remote-debugging-port=9222","--user-data-dir=C:\Users\erikthetwin\AppData\Local\Temp\brave-cdp-test","--no-first-run","--no-default-browser-check","about:blank"
```

Kiểm tra cổng CDP: `Invoke-WebRequest http://localhost:9222/json/version` → `200`.

### 3. Kịch bản kiểm thử (theo `LIVE-USAGE-TEST.md`)

| Vai trò | Kịch bản | URL chính |
|---|---|---|
| Khách | Tìm kiếm + lọc + xem chi tiết + bản đồ; liên hệ/lưu tin bị chặn | `/`, `/Listings`, `/Listings/Details/1`, `/Ml/Predict` |
| Người mua | Đăng ký, lưu/bỏ lưu tin, gửi liên hệ, sửa hồ sơ | `/Account/Register`, `/Favorites`, `/Account/Profile` |
| Người bán | Đăng/sửa/xóa tin kèm ảnh → chờ duyệt, ẩn khỏi công khai | `/MyListings/Create`, `/MyListings` |
| Quản trị | Duyệt/từ chối/khóa tin, quản lý người dùng (khóa/đổi vai trò/xóa), loại BĐS, dashboard, ML | `/Admin`, `/Admin/Moderation`, `/Admin/Users`, `/Admin/Types` |

Mẹo thao tác (đã nghiệm ra khi test):

- Xóa tin/user có hộp thoại xác nhận: đăng ký `page.on('dialog', d => d.accept())` **trước** khi click.
- Nút "Sửa" ở bảng "Tin của tôi" là thẻ `<a>` (không phải `<button>`); nút "Xóa" là submit của form.
- Trang Duyệt tin / Loại BĐS hiển thị bằng `div.card`, không phải bảng.
- Form dự đoán giá không có trường giá — có `PropertyType`; bật/tắt "cho thuê" bằng checkbox `IsForRent`.
- Sau mỗi nhóm kịch bản, kiểm chứng bằng truy vấn `realestate.db` (vd `python -c "import sqlite3; ..."`).

### 4. Đặt lại dữ liệu demo (khi test làm hỏng dữ liệu)

```powershell
# Tắt server, xóa DB, chạy lại → tự tạo lại + seed + (nếu cần) huấn luyện lại ML
Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue
Remove-Item realestate.db -Force
dotnet watch run --non-interactive --urls http://localhost:5000
```

- Dữ liệu chuẩn sau reset: 2 user (admin, seller), 23 tin (20 đã duyệt + 3 chờ duyệt), 6 loại BĐS, 1 ảnh/tin.
- `ML/model.zip` + `ML/metrics.json` tồn tại thì mô hình không huấn luyện lại; xóa cả hai để huấn luyện lại từ đầu.
- Tài khoản buyer dùng trong báo cáo (`anhnha@demo.com` / `Test@12345`) không nằm trong seed — đăng ký mới hoặc tạo qua UI.

### 5. Kiểm thử toàn diện đa tài khoản

Ma trận 21 nhóm kịch bản đã chạy và đạt (khách/buyer/seller/admin + vòng đời tin) nằm ở
**Phụ lục B** của `LIVE-USAGE-TEST.md` — dùng làm checklist cho mỗi lượt test lại.
Gồm: tìm kiếm/lọc, yêu thích, liên hệ, hồ sơ, đăng/sửa/xóa tin, duyệt/từ chối/khóa, quản lý
người dùng (khóa/đổi vai trò/xóa), loại BĐS, dashboard đối chiếu DB, dự đoán giá ML.

Quy ước trạng thái tin trong DB (`Properties.Status`): `0` Chờ duyệt, `1` Đã duyệt, `2` Từ chối, `3` Bị khóa.

### 6. Lưu ý môi trường

- `dotnet watch` có thể chết âm thầm giữa phiên (không có log lỗi) — trước mỗi lượt test kiểm tra
  `Invoke-WebRequest http://localhost:5000` trả `200`, chết thì chạy lại lệnh ở mục 1.
- Playwright kết nối Brave qua CDP: `cdp_url=http://localhost:9222`; nếu báo
  `connect ECONNREFUSED ::1:9222` nghĩa là Brave đã bị đóng — chạy lại lệnh ở mục 2.
- Khi dùng Playwright MCP, dialog xác nhận đã được `page.on('dialog')` xử lý thì báo "Modal state"
  còn treo là hiện tượng bình thường — bỏ qua, xác nhận kết quả bằng dữ liệu DB.
