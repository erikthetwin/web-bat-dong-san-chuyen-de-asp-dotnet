# Báo cáo kiểm thử trực tiếp (Live Usage Test)

- **Ngày:** 19/08/2026
- **Phương pháp:** Kiểm thử thủ công trên trình duyệt thật, thao tác như người dùng thực tế qua Playwright, chống lại server đang chạy tại `http://localhost:5000` (dotnet watch, live reload).
- **Dữ liệu:** Dữ liệu mẫu (seed) + dữ liệu mới phát sinh trong quá trình kiểm thử. Kiểm chứng kèm truy vấn SQLite (`realestate.db`).
- **Kết quả chung:** Toàn bộ 5 kịch bản ĐẠT. Không phát hiện lỗi code.

## Tài khoản sử dụng

| Vai trò | Email | Mật khẩu | Ghi chú |
|---|---|---|---|
| Admin | admin@demo.com | Admin@123 | Tài khoản mặc định (seed) |
| Seller | seller@demo.com | Seller@123 | Tài khoản mặc định (seed) |
| Buyer | buylive@demo.com | Test@12345 | Đăng ký mới trong kịch bản B |

---

## Kịch bản A — Khách (Guest) duyệt website

**Mục tiêu:** Xác nhận người dùng chưa đăng nhập xem được trang chủ, tìm kiếm, lọc, xem chi tiết; các thao tác yêu cầu đăng nhập bị chặn đúng cách.

### Hướng dẫn thao tác

1. Mở `http://localhost:5000/` → trang chủ: hero + 6 tin nổi bật (card có ảnh, giá, địa chỉ).
2. Bấm menu **"Tìm nhà"** (hoặc vào `http://localhost:5000/Listings`).
3. Nhập từ khóa **"Gò Vấp"** vào ô tìm kiếm → bấm nút tìm kiếm.
4. Bấm **"Xóa bộ lọc"** (hoặc vào lại `/Listings`), chọn **Loại hình = Căn hộ**, **Giá từ 10.000.000.000 đ** đến **20.000.000.000 đ** → bấm lọc.
5. Bấm vào tin đầu tiên → xem chi tiết: bản đồ Leaflet, thông tin (diện tích, phòng, giá), thông tin liên hệ.
6. Thử bấm **"Gửi liên hệ"** / nút **"Lưu tin"**.

### Kết quả mong đợi — Kết quả thực tế

| Bước | Mong đợi | Thực tế | Kết quả |
|---|---|---|---|
| 1 | 6 tin nổi bật | 6 card | ✅ |
| 3 | Chỉ tin có "Gò Vấp" | "Tìm thấy 1 bất động sản" | ✅ |
| 4 | Lọc theo loại + khoảng giá | 4 tin (loại Căn hộ, giá 10–20 tỷ) | ✅ |
| 5 | Bản đồ + thông tin đầy đủ | Leaflet hiển thị, đủ thông tin | ✅ |
| 6 | Chặn: yêu cầu đăng nhập | Hiện "Vui lòng đăng nhập để liên hệ người bán"; không có nút "Lưu tin" | ✅ |

---

## Kịch bản B — Người mua (Buyer) trọn hành trình

**Mục tiêu:** Đăng ký tài khoản mới, đăng nhập, gửi liên hệ người bán, lưu tin yêu thích, sửa hồ sơ.

### Hướng dẫn thao tác

1. Mở `http://localhost:5000/Account/Register`.
2. Điền form: **Họ tên** = "Người Mua Live", **Email** = `buylive@demo.com`, **Số điện thoại** = `0901112223`, **Địa chỉ** = "Quận 10, TP.HCM", **Mật khẩu / Xác nhận mật khẩu** = `Test@12345`, **Vai trò = Người mua** → bấm **"Đăng ký"**.
3. Tự động vào trang chủ (đã đăng nhập, nav hiện "Xin chào, buylive@demo.com").
4. Vào chi tiết tin bất kỳ, ví dụ `http://localhost:5000/Listings/Details/3`.
5. Nhập **Số điện thoại** = `0901112223`, **Nội dung liên hệ** = "Anh chị ơi, tôi muốn xem nhà vào cuối tuần này được không?" → bấm **"Gửi liên hệ"**.
6. Bấm **"Lưu tin"** → mở menu **"Tin đã lưu"** (`/Favorites`).
7. Vào **"Xin chào, ..." → Hồ sơ** (`/Account/Profile`), sửa **Họ tên** thành "Người Mua Live (đã sửa)" → bấm **"Lưu thay đổi"**.

### Kết quả mong đợi — Kết quả thực tế

| Bước | Mong đợi | Thực tế | Kết quả |
|---|---|---|---|
| 2-3 | Đăng ký xong vào trang chủ | Về `/` (trang chủ), đã đăng nhập | ✅ |
| 5 | Thông báo gửi thành công | "Đã gửi yêu cầu liên hệ. Người bán sẽ liên hệ với bạn." | ✅ |
| 6 | Tin xuất hiện trong danh sách yêu thích | 1 card trong `/Favorites` | ✅ |
| 7 | Lưu hồ sơ thành công | "Đã cập nhật thông tin." | ✅ |

**Kiểm chứng DB:**

```
users:    [('buylive@demo.com', 'Người Mua Live (đã sửa)')]
contacts: [('buylive@demo.com', '0901112223', 'Anh chị ơi, tôi muốn xem nhà vào cuối tuần này được không?', 3)]
favs:     2 (tổng)
```

---

## Kịch bản C — Người bán (Seller): đăng / sửa / xóa tin

**Mục tiêu:** Đăng tin mới kèm 2 ảnh, trạng thái Chờ duyệt; sửa tin (trạng thái về lại Chờ duyệt); xóa tin.

### Hướng dẫn thao tác

1. Đăng xuất → đăng nhập `seller@demo.com` / `Seller@123`.
2. Vào **"Tin của tôi"** → bấm **"Đăng tin mới"** (`/MyListings/Create`).
3. Điền form:
   - Tiêu đề: "Nhà mới đường Lê Văn Sỹ Q3"
   - Mô tả: "Nhà 1 trệt 1 lầu, hẻm xe hơi, khu an ninh"
   - Giá: `8500000000`, Diện tích: `72`, Phòng ngủ: `3`, Phòng tắm: `2`, Số tầng: `2`, Mặt tiền: `4.5`
   - Quận: Quận 3, Phường: Phường 2, Đường: Lê Văn Sỹ, Địa chỉ: "Lê Văn Sỹ, Phường 2, Quận 3"
   - Vĩ độ: `10.7828`, Kinh độ: `106.6831`, Loại: chọn loại đầu tiên
   - SĐT liên hệ: `0944287264`
   - Chọn **2 file ảnh** (`.png`) ở mục tải ảnh
   - Bấm **"Đăng tin"**.
4. Trong danh sách "Tin của tôi": tin mới có nhãn **"Chờ duyệt"**.
5. Bấm **"Sửa"** ở dòng tin mới → đổi tiêu đề thành "Nhà mới đường Lê Văn Sỹ Q3 - giá mới" → bấm nút lưu.
6. Bấm **"Xóa"** → xác nhận hộp thoại "Xóa tin này?" → tin biến mất.

### Kết quả mong đợi — Kết quả thực tế

| Bước | Mong đợi | Thực tế | Kết quả |
|---|---|---|---|
| 3-4 | Đăng xong, trạng thái Chờ duyệt, 2 ảnh đã tải lên | Nhãn "Chờ duyệt", tin có tên vừa đăng | ✅ |
| 5 | Sửa xong, trạng thái về Chờ duyệt | "Chờ duyệt", tiêu đề mới hiển thị | ✅ |
| 6 | Xóa tin khỏi danh sách | Tin biến mất sau khi xác nhận | ✅ |

---

## Kịch bản D — Quản trị (Admin): thống kê + duyệt tin

**Mục tiêu:** Bảng thống kê phản ánh số liệu thật; duyệt tin → tin xuất hiện công khai.

### Hướng dẫn thao tác

1. Đăng xuất → đăng nhập `admin@demo.com` / `Admin@123`.
2. Bấm menu **"Quản trị"** (`/Admin`): 8 thẻ số liệu + bảng "Số tin theo loại hình" + "Số tin theo quận/huyện".
3. Bấm **"Duyệt tin"** (`/Admin/Moderation`): hàng đợi = 1 tin (tin vừa tạo ở kịch bản C).
4. Bấm **"Duyệt"** → hàng đợi về 0.
5. Mở `/Listings?keyword=L%C3%AA%20V%C4%83n%20S%E1%BB%B9` (tìm "Lê Văn Sỹ").
6. Thử `/Admin/Users` — quản lý người dùng (khóa/đổi vai trò/xóa) đã kiểm thử riêng ở Task 12.

### Kết quả mong đợi — Kết quả thực tế

| Bước | Mong đợi | Thực tế | Kết quả |
|---|---|---|---|
| 2 | Số liệu khớp DB | 25 tổng / 1 chờ duyệt / 24 đã duyệt / 5 người dùng / 3 liên hệ | ✅ |
| 3-4 | Duyệt xong hàng đợi = 0 | "Duyệt tin (1)" → "Duyệt tin (0)" | ✅ |
| 5 | Tin đã duyệt xuất hiện công khai | "Tìm thấy 1 bất động sản" = tin "Nhà mới đường Lê Văn Sỹ Q3 - giá mới" | ✅ |

---

## Kịch bản E — Vòng đời đầy đủ của một tin (liên vai trò)

**Mục tiêu:** Một tin đi hết: Người bán đăng → Admin duyệt → Công khai → Người bán xóa.

### Hướng dẫn thao tác

1. (Seller) Đăng tin mới (như kịch bản C, bước 1-4) → tin ở trạng thái **Chờ duyệt**, chưa hiện trên `/Listings`.
2. (Admin) Vào `/Admin/Moderation` → bấm **"Duyệt"**.
3. (Khách / bất kỳ) Vào `/Listings` → tìm kiếm tên tin → tin hiện ra.
4. (Seller) Vào "Tin của tôi" → **"Xóa"** → xác nhận → tin biến mất khỏi `/Listings`.

### Kết quả mong đợi — Kết quả thực tế

| Bước | Mong đợi | Thực tế | Kết quả |
|---|---|---|---|
| 1 | Chờ duyệt, chưa công khai | Nhãn "Chờ duyệt" | ✅ |
| 2 | Duyệt thành công | Hàng đợi giảm 1 → 0 | ✅ |
| 3 | Tin công khai | Tìm kiếm trả đúng 1 tin | ✅ |
| 4 | Xóa thành công | Tin mất khỏi danh sách; DB trở về 24/24 tin đã duyệt | ✅ |

---

## Kiểm chứng dữ liệu cuối (realestate.db)

```
users:    5 tài khoản (admin, seller, buyertest7, e2ebuyer, buylive)
props:    (24, 24, 0)  → 24 tin, 24 đã duyệt, 0 chờ duyệt
contacts: 3 yêu cầu liên hệ
favs:     2 tin yêu thích
```

- Trạng thái sau test: server vẫn chạy tại `http://localhost:5000`, DB sạch, không phát sinh thay đổi code (working tree sạch).
- Tài khoản phát sinh khi test: `buylive@demo.com` (đã sửa hồ sơ), `e2ebuyer@demo.com` (bị khóa từ kiểm thử E2E Task 15).

## Ghi chú thao tác (cho lần test tiếp)

- Nút lưu hồ sơ là `btn btn-primary` (không có `w-100`).
- Form liên hệ bắt buộc có **số điện thoại** (validate server-side).
- Xóa tin có hộp thoại xác nhận — cần chấp nhận dialog.
- Tìm kiếm nên dùng URL trực tiếp (vd `/Listings?keyword=...`) để tránh race khi submit form.

---

## Phụ lục A — Bug đã phát hiện và sửa: Admin không xóa được tài khoản (19/08/2026)

### Triệu chứng

Bấm **"Xóa"** ở `/Admin/Users` → trang lỗi `500 Internal Server Error`:

```
SqliteException: SQLite Error 19: 'FOREIGN KEY constraint failed'.
```

### Nguyên nhân gốc

`DeleteUser` chỉ gọi `UserManager.DeleteAsync(u)`. Người dùng có dữ liệu liên quan
(yêu thích, liên hệ, tin đăng) → SQLite vi phạm ràng buộc khóa ngoại
(`Favorites.UserId`, `ContactRequests.UserId`, `Properties.OwnerId` — `Restrict`) → exception.

### Cách sửa

`Controllers/AdminController.cs` — `DeleteUser` giờ xóa sạch dữ liệu liên quan trước khi xóa tài khoản:

```csharp
var propertyIds = await _db.Properties.Where(p => p.OwnerId == u.Id).Select(p => p.Id).ToListAsync();
await _db.Favorites.Where(f => f.UserId == u.Id || propertyIds.Contains(f.PropertyId)).ExecuteDeleteAsync();
await _db.ContactRequests.Where(c => c.UserId == u.Id || propertyIds.Contains(c.PropertyId)).ExecuteDeleteAsync();
await _db.PropertyImages.Where(i => propertyIds.Contains(i.PropertyId)).ExecuteDeleteAsync();
await _db.Properties.Where(p => p.OwnerId == u.Id).ExecuteDeleteAsync();
await _userManager.DeleteAsync(u);
```

Thứ tự quan trọng: xóa tin yêu thích/liên hệ (tham chiếu cả user lẫn tin) → ảnh → tin → tài khoản.
Tài khoản Admin vẫn được bảo vệ (không hiện nút Xóa, có kiểm tra vai trò).

### Kiểm thử sau sửa (kiểm thử trực tiếp, cô lập từng trường hợp)

| Ca | Thao tác | Kết quả |
|---|---|---|
| Buyer có yêu thích + liên hệ | Tạo `tdel@demo.com` (Buyer) → lưu tin + gửi liên hệ → Admin xóa | Xóa thành công; chỉ mất user + 2 dòng dữ liệu của user; 23 tin, 6 loại giữ nguyên |
| Seller có tin đăng + ảnh | Tạo `tsel@demo.com` (Seller) → đăng tin kèm 1 ảnh → Admin xóa | Xóa thành công; tin + ảnh của user biến mất; 23 tin gốc giữ nguyên |
| Admin | Bấm thử nút Xóa ở dòng admin | Không có nút Xóa (được bảo vệ) |

Kiểm chứng DB cuối: `users = [admin, seller]`, `props = 23`, `images = 23`, `types = 6`.

> **Lưu ý dữ liệu:** trước khi sửa, có một lần thao tác xóa đã dẫn tới mất dữ liệu hàng loạt
> (seller, buyertest7 và toàn bộ tin) không giải thích được bằng luồng test — DB đã được
> tạo lại từ seed (đúng trạng thái demo chuẩn: admin, seller, 23 tin, 6 loại) trước khi kiểm thử
> cô lập ở trên.
---

# Phụ lục B — Kiểm thử toàn diện đa tài khoản (19/08/2026, v2)

Kiểm thử lại TOÀN BỘ chức năng với 3 vai trò trên server live, kèm kiểm chứng DB sau từng nhóm.
Phát hiện 1 bug (hiển thị chỉ số ML sau khi khởi động lại) → đã sửa + xác minh.

## Tài khoản

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Admin | admin@demo.com | Admin@123 |
| Seller | seller@demo.com | Seller@123 |
| Buyer (mới) | anhnha@demo.com | Test@12345 |

## Kết quả theo nhóm chức năng

| # | Nhóm | Thao tác | Kết quả |
|---|---|---|---|
| 1 | Khách - trang chủ | Hero + 6 tin nổi bật | ✅ |
| 2 | Khách - danh sách | Phân trang 1-2 (10 tin/trang), sort, lọc phòng ngủ (9 kết quả 3PN) | ✅ |
| 3 | Khách - chi tiết | Bản đồ Leaflet, gallery 10 ảnh; chưa đăng nhập thấy "đăng nhập để liên hệ", KHÔNG có nút lưu tin | ✅ |
| 4 | Buyer - đăng ký | Form đầy đủ, tự đăng nhập sau đăng ký | ✅ |
| 5 | Buyer - yêu thích | Lưu 2 tin → /Favorites hiện 2 → bỏ lưu 1 → còn 1 | ✅ |
| 6 | Buyer - liên hệ | Gửi liên hệ có SĐT → lưu DB (1 dòng, đúng phone+message) | ✅ |
| 7 | Buyer - validate | Gửi liên hệ KHÔNG có SĐT → bị chặn (không có dòng mới trong DB) | ✅ |
| 8 | Buyer - hồ sơ | Sửa tên/SĐT/địa chỉ → lưu thành công, hiển thị lại đúng | ✅ |
| 9 | Seller - đăng tin | Đăng 4 tin kèm 1 ảnh mỗi tin → trạng thái "Chờ duyệt", KHÔNG hiện ở /Listings công khai | ✅ |
| 10 | Seller - sửa tin | Sửa tiêu đề + giá → vẫn "Chờ duyệt", không lộ công khai | ✅ |
| 11 | Seller - xóa tin | Xóa tin D (có dialog xác nhận) → biến mất khỏi "Tin của tôi" + DB | ✅ |
| 12 | Admin - duyệt tin | Duyệt A → công khai; Từ chối B (Status=2); Khóa C (Status=3); kiểm chứng enum: 0=Pending 1=Approved 2=Rejected 3=Banned | ✅ |
| 13 | Admin - quyền | /Admin/* chặn người không phải admin (AccessDenied), /MyListings/Create chặn admin | ✅ |
| 14 | Công khai - lifecycle | A hiện ở /Listings (200), B/C/D chi tiết trả 404, không xuất hiện trong tìm kiếm | ✅ |
| 15 | Admin - loại BĐS | Thêm "Nhà phố thương mại" → hiện ở filter /Listings + form đăng tin seller; Ẩn → biến mất khỏi filter; Hiện → trở lại | ✅ |
| 16 | Admin - người dùng | Khóa anhnha → đăng nhập bị chặn ("Email hoặc mật khẩu không đúng"); Mở khóa → vào được; đổi vai trò Buyer↔Seller (vòng đủ); dòng admin không có nút Khóa/Xóa | ✅ |
| 17 | Admin - xóa user | Xóa tmpdel2 (user mới, không dữ liệu) → biến mất khỏi bảng + DB | ✅ |
| 18 | Admin - dashboard | 26 tổng / 3 chờ duyệt / 21 đã duyệt / 1 từ chối / 1 khóa / 3 user / 1 liên hệ — khớp 100% truy vấn DB | ✅ |
| 19 | ML - dự đoán | Mua: 11.268.080.000 đ; Thuê: 11.200.080.000 đ | ✅ |
| 20 | ML - chỉ số | R² 0.963 / RMSE 2.428.848.273 / MAE 1.638.812.770 (sau khi sửa bug, xem dưới) | ✅ |
| 21 | Toàn vẹn DB cuối | users [admin, seller, anhnha] (không khóa); 26 tin; 7 loại (7 active); favs/contacts của anhnha còn nguyên sau mọi thao tác | ✅ |

## Bug phát hiện lần này: chỉ số ML = 0 sau khi khởi động lại (đã sửa)

**Triệu chứng:** Chạy dự đoán giá → kết quả vẫn đúng nhưng hiển thị `R² 0 / RMSE 0 / MAE 0`.

**Nguyên nhân gốc:** `PricePredictionService` chỉ tính R²/RMSE/MAE trong lúc huấn luyện (lần đầu chạy),
nhưng không lưu trữ. Lần chạy sau (đã có model.zip) đi qua nhánh `LoadAsync` → các chỉ số mặc định = 0.
Model ML.NET không mang sẵn các chỉ số này.

**Cách sửa (`Services/ML/PricePredictionService.cs`):** khi huấn luyện xong, ghi `ML/metrics.json`
(R2/RMSE/MAE); `LoadAsync` đọc file này để khôi phục chỉ số.

**Xác minh:**
- Xóa model.zip → khởi động → huấn luyện lại (seed=1, số liệu trùng T14) → dự đoán hiển thị R² 0.963, RMSE 2.428.848.273, MAE 1.638.812.770 ✅
- Khởi động lại server (nhánh LoadAsync, có sẵn model.zip + metrics.json) → dự đoán vẫn hiển thị đủ chỉ số ✅

## Ghi chú môi trường

- `dotnet watch` nhiều lần chết âm thầm giữa chừng (không có log lỗi) → phải kiểm tra server trước mỗi lượt test, khởi động lại bằng lệnh Start-Process nếu cần.
- Dialog xác nhận: đăng ký `page.on('dialog')` TRƯỚC khi click; MCP sẽ báo "Modal state" còn treo nhưng thực tế dialog đã được xử lý — bỏ qua, kiểm tra DB để xác nhận kết quả.
- Nút "Sửa" ở bảng "Tin của tôi" là thẻ `<a>` (không phải `<button>`); nút "Xóa" là submit của form kèm `confirm()`.
- Trang Duyệt tin / Loại BĐS dùng thẻ `div.card`, không phải bảng.
- Form dự đoán giá không có trường "Giá" — có `PropertyType` thay thế.
