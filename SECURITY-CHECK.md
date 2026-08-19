# Báo cáo kiểm tra bảo mật

Ngày: 19/08/2026
Phạm vi: website bất động sản (ASP.NET Core MVC, chạy tại http://localhost:5000)

## Nguồn checklist

- OWASP Developer Guide, mục Web Application Checklist (10 kiểm soát theo Top 10 Proactive Controls): devguide.owasp.org/en/04-design/02-web-app-checklist/
- OWASP Application Security Verification Standard (ASVS), mức 1: owasp.org/www-project-application-security-verification-standard/

## Phương pháp

- Kiểm thử trực tiếp trên server live
- Rà soát tĩnh mã nguồn: controllers, Program.cs, cấu hình
- Quét lỗ hổng dependency: `dotnet list package --vulnerable --include-transitive`

## Kết quả tổng quan

Trạng thái: ĐẠT. Không còn lỗ hổng chưa xử lý trong phạm vi kiểm thử. Ba vấn đề mức High phát hiện trong đợt này đã được vá (xem phần dưới).

| Nhóm | Kiểm tra | Kết quả |
|---|---|---|
| Dependency | Không gói nào có CVE sau khi nâng EF Core lên 8.0.30 | Đạt |
| CSRF | Mọi form POST có token chống giả mạo; POST không token trả về 400 | Đạt |
| SQL injection | Tải trọng `OR 1=1` không làm đổi hành vi (EF Core tham số hóa) | Đạt |
| XSS | Thẻ `<script>` không thực thi, không phản chiếu vào HTML (Razor tự encode) | Đạt |
| Phân quyền | Khách bị chuyển sang trang đăng nhập; sai vai trò vào trang AccessDenied | Đạt |
| IDOR | Sửa/xóa tin lọc theo chủ sở hữu; tin của người khác trả về 404 | Đạt |
| Open redirect | ReturnUrl được kiểm tra bằng `Url.IsLocalUrl` | Đạt |
| Khóa tài khoản | 5 lần đăng nhập sai sẽ khóa (xác minh thực tế trên DB) | Đạt |
| Cookie | Auth: HttpOnly + SameSite=Lax; Antiforgery: HttpOnly + SameSite=Strict | Đạt |
| Upload ảnh | Giới hạn 5MB, whitelist 4 loại đuôi, tên file GUID | Đạt |
| Lộ thông tin | .git, mã nguồn, DB, danh mục upload đều trả về 404 | Đạt |
| Header bảo mật | Đủ 5 header (đã bổ sung trong đợt này) | Đạt |
| Chức năng sau vá | Tất cả trang hoạt động, không lỗi console | Đạt |

## Vấn đề đã sửa trong đợt này

### 1. Ba CVE mức High trong dependency

| Gói (transitive) | Phiên bản cũ | Advisory |
|---|---|---|
| Microsoft.Extensions.Caching.Memory | 8.0.0 | GHSA-qj66-m88j-hmgj |
| SQLitePCLRaw.lib.e_sqlite3 | 2.1.6 | GHSA-2m69-gcr7-jv3q |
| System.Text.Json | 8.0.4 | GHSA-8g4q-xg66-9fp4 |

Cách sửa: nâng Microsoft.EntityFrameworkCore.Sqlite và Microsoft.AspNetCore.Identity.EntityFrameworkCore từ 8.0.8 lên 8.0.30 (cả dự án chính lẫn Tests).

Xác minh: danh sách vulnerable rỗng, build 0 lỗi, 5/5 kiểm thử, ML vẫn dự đoán đúng.

### 2. Thiếu header bảo mật

Trước đây phản hồi không có header bảo mật nào (chỉ có `Server: Kestrel`). Đã thêm middleware đầu pipeline trong Program.cs:

```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: camera=(), microphone=(), geolocation=()
Content-Security-Policy: default-src 'self'; script-src 'self' https://cdn.jsdelivr.net https://unpkg.com;
  style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net https://unpkg.com;
  font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; img-src 'self' data: https:;
  connect-src 'self' https:; frame-ancestors 'none'; object-src 'none'; base-uri 'self'; form-action 'self'
```

### 3. CSP làm hỏng bản đồ Leaflet

Triệu chứng: CSP chặn script inline khởi tạo bản đồ, tile không tải.

Cách sửa:
- Chuyển khởi tạo bản đồ vào `wwwroot/js/site.js` (đọc tọa độ từ `data-lat/lng/title` của `#map`, chạy khi DOMContentLoaded), xóa script inline khỏi `Details.cshtml`
- Thêm `asp-append-version="true"` cho `site.js` để tránh trình duyệt dùng cache cũ

Xác minh: bản đồ hiển thị tile và marker, không còn lỗi console.

## Vấn đề còn lại (chấp nhận cho bản demo)

| Vấn đề | Ghi chú |
|---|---|
| Chính sách mật khẩu yếu | Chỉ yêu cầu 6 ký tự và có chữ số. Nâng chuẩn khi chuyển production |
| Upload không kiểm tra nội dung | Chỉ kiểm tra đuôi file, không đọc magic bytes. Bổ sung khi production |
| Chưa có HTTPS / HSTS | Đang chạy HTTP local. Production cần HTTPS, UseHsts, cookie Secure |
| AllowedHosts để dấu * | Nên giới hạn host khi production |

## Phạm vi chưa kiểm thử

- Brute-force quy mô lớn (không chạy tấn công thực)
- Cấu hình TLS, proxy, reverse-proxy
- Kiểm tra nội dung file upload thực tế