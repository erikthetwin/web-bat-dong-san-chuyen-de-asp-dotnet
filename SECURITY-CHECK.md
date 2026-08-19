# Báo cáo kiểm tra bảo mật (Security Check) — 19/08/2026

- **Căn cứ checklist:** OWASP Developer Guide — Web Application Checklist (10 kiểm soát theo Top 10 Proactive Controls) + OWASP ASVS Level 1 (tra cứu: devguide.owasp.org/en/04-design/02-web-app-checklist/, owasp.org/www-project-application-security-verification-standard/).
- **Phương pháp:** Kiểm thử trực tiếp trên server live (`http://localhost:5000`, Playwright + Brave CDP) + rà soát tĩnh mã nguồn + `dotnet list package --vulnerable`.
- **Kết quả chung:** ĐẠT sau khi khắc phục — 3 lỗ hổng High (dependency) đã vá, bổ sung 5 header bảo mật thiếu, CSP bẻ gãy 1 inline script (đã chuyển sang file tĩnh có cache-bust), 0 cảnh báo console, 5/5 unit test.

## Tóm tắt kết quả

| Mục | Kiểm tra | Kết quả |
|---|---|---|
| Dependency | `dotnet list package --vulnerable --include-transitive` | ✅ 0 vulnerable (đã vá, xem dưới) |
| CSRF | Mọi form POST đều có `__RequestVerificationToken`; POST không token → 400 | ✅ |
| SQL Injection | `keyword=' OR 1=1--` → 0 kết quả, không lỗi | ✅ (EF Core tham số hóa) |
| XSS | `keyword=<script>alert(1)</script>` → không thực thi, không phản chiếu | ✅ (Razor encode) |
| Phân quyền | Guest → /Admin: chuyển Login; Buyer → /Admin, /MyListings: AccessDenied; `[Authorize(Roles="Admin")]` cấp controller | ✅ |
| IDOR | Edit/Delete/DeleteImage lọc theo `OwnerId`; Edit tin của người khác → 404 | ✅ |
| Open redirect | `Url.IsLocalUrl(returnUrl)` trước khi redirect | ✅ |
| Khóa tài khoản | `MaxFailedAccessAttempts=5`; 5 lần sai → `LockoutEnd` ghi DB (xác minh thực tế) | ✅ |
| Cookie | Auth: `HttpOnly` + `SameSite=Lax`; Antiforgery: `HttpOnly` + `SameSite=Strict` | ✅ |
| Upload | Giới hạn ≤5MB + whitelist đuôi .jpg/.jpeg/.png/.webp; tên file GUID | ✅ |
| Lộ thông tin | `/uploads/`, `/.git/config`, `*.cs`, `*.csproj`, `realestate.db` → đều 404 | ✅ |
| Lỗi & 404 | Tin không tồn tại → 404; route lạ → 404; không rò stack trace | ✅ |
| Bí mật | Không có secret/password trong source & appsettings; chuỗi kết nối không mật khẩu | ✅ |
| Header bảo mật | X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Permissions-Policy, CSP — đã bổ sung (xem dưới) | ✅ |
| Chức năng sau vá | Home/search/details+bản đồ/đăng nhập/admin — hoạt động, 0 lỗi console | ✅ |

## Vấn đề đã phát hiện và khắc phục

### 1. (High) 3 CVE trong dependency — đã vá

| Gói (transitive) | Phiên bản cũ | Severity | Advisory | Bản vá |
|---|---|---|---|---|
| Microsoft.Extensions.Caching.Memory | 8.0.0 | High | GHSA-qj66-m88j-hmgj | EF Core 8.0.8 → **8.0.30** (main + Tests) |
| SQLitePCLRaw.lib.e_sqlite3 | 2.1.6 | High | GHSA-2m69-gcr7-jv3q | như trên |
| System.Text.Json | 8.0.4 | High | GHSA-8g4q-xg66-9fp4 | như trên |

Sau vá: `dotnet list package --vulnerable` → **0 vulnerable**. Build 0 lỗi, 5/5 test, toàn bộ chức năng chạy lại bình thường (ML dự đoán 11.753.690.000₫, R² 0.963).

### 2. (Trung bình) Thiếu header bảo mật — đã bổ sung

Trước: chỉ có `Server: Kestrel`, không header bảo mật nào. Thêm middleware trong `Program.cs` (đầu pipeline):

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

### 3. (Trung bình) CSP bẻ gãy bản đồ Leaflet — đã sửa

CSP `script-src` không cho phép inline script → script khởi tạo bản đồ trong `Details.cshtml` bị chặn, bản đồ không tải tile.

- Sửa: chuyển khởi tạo Leaflet vào `wwwroot/js/site.js` (đọc `data-lat/lng/title` từ `#map`, chạy khi DOMContentLoaded); xóa inline script khỏi `Details.cshtml`.
- Kèm: thêm `asp-append-version="true"` cho `site.js` — phát hiện trình duyệt giữ cache cũ (heuristic caching, không có cache-bust) khiến bản đồ vẫn chết dù server đã có code mới.
- Xác minh: bản đồ hiển thị 8 tile + marker, 0 lỗi console trên mọi trang.

### 4. (Thấp, giữ nguyên) Chính sách mật khẩu yếu

`RequireDigit + min 6 ký tự` — không yêu cầu chữ hoa/ký tự đặc biệt (ASVS L1 khuyến nghị mạnh hơn, NIST ≥ 8 ký tự). Đây là đồ án demo — giữ nguyên để không phá tài khoản mẫu; nâng cấp khi chuyển production.

### 5. (Thấp, giữ nguyên) Upload không kiểm tra nội dung file

Chỉ kiểm tra đuôi mở rộng + kích thước, không đọc magic bytes. File `.html`/`.svg` độc hại với đuôi hợp lệ không thể tải lên (whitelist chỉ cho 4 đuôi ảnh), nhưng nên kiểm tra `ContentType`/magic bytes khi production. Ngoài ra: tên file GUID + thư mục `uploads/` ngoài wwwroot nếu production.

### 6. (Thông tin) HTTPS / HSTS

Chạy HTTP local (dev) — không có HTTPS redirect/HSTS. Production bắt buộc: bật HTTPS, thêm `UseHsts`, `Secure` flag cho cookie (hiện `secure=false` do HTTP).

## Chi tiết kiểm tra đã chạy

- **CSRF:** quét 14 trang (chính, MyListings, Favorites, Details, Profile, ML, 4 trang Admin, Login, Register) — mọi form POST (logout, 24 nút xóa, create, edit, contact, favorite, login, register, predict) đều kèm token. `POST /Favorites/Toggle` không token → **400**.
- **Injection:** `keyword=' OR 1=1--` → 0 kết quả, không lỗi, không đổi hành vi; `keyword=<script>alert(1)</script>` → không có thẻ script, không phản chiếu vào HTML.
- **Phân quyền:** Guest `/Admin/Users` → 302 `/Account/Login?ReturnUrl=...`; Buyer `/MyListings` và `/Admin` → `/Account/AccessDenied`; Admin không vào được `/MyListings/Edit/{id}` → AccessDenied; `AdminController` có `[Authorize(Roles = "Admin")]` cấp lớp.
- **IDOR (tĩnh):** `Edit` (GET/POST), `Delete`, `DeleteImage` đều truy vấn `x.Id == id && x.OwnerId == userId` → tin của người khác trả 404.
- **Khóa tài khoản (thực tế):** 5 lần đăng nhập sai liên tiếp `anhnha@demo.com` → DB ghi `AccessFailedCount` và `LockoutEnd` (khóa ~5 phút); thông báo lỗi không tiết lộ trạng thái khóa.
- **Cookie:** auth `.AspNetCore.Identity.Application` = HttpOnly + SameSite=Lax; antiforgery = HttpOnly + SameSite=Strict.
- **Lộ thông tin:** `/uploads/` (liệt kê thư mục), `/.git/config`, `/Program.cs`, `/webapp demo.csproj`, `/realestate.db` → tất cả 404 (chỉ phục vụ wwwroot, không có directory browsing).
- **Xử lý lỗi:** `/Listings/Details/9999` → 404; `/NoSuchRoute/xyz` → 404; không có DeveloperExceptionPage trong pipeline → không rò stack trace.
- **Upload (tĩnh):** ≤5MB, đuôi ∈ {.jpg,.jpeg,.png,.webp}, tên file = GUID, lưu vào `wwwroot/uploads` — không theo tên người dùng.
- **Bí mật (tĩnh):** không tìm thấy password/secret/api-key trong source + appsettings; connection string SQLite không mật khẩu (đúng cho demo; production dùng bí mật riêng).
- **Chức năng sau vá:** trang chủ (6 card), tìm kiếm "Thủ Đức" (4 kết quả), chi tiết + bản đồ Leaflet (tile + marker), đăng nhập admin, dashboard (8 stat), duyệt tin, người dùng, loại BĐS, đăng xuất — tất cả OK, 0 lỗi console.

## Rủi ro còn lại

- Không kiểm thử được: brute-force ở quy mô lớn (không chạy tấn công thực), TLS ở production, magic-byte upload validation, tấn công phụ thuộc môi trường (header nào cấu hình ở proxy/reverse-proxy).
- Mật khẩu yếu + cookie không Secure chỉ lành mạnh khi deploy qua HTTPS (mục 4-6).
- Đồ án demo chạy `AllowedHosts: *` — hạn chế host khi production.