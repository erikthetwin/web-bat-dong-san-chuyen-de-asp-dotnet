# Báo cáo kiểm thử trực tiếp

Ngày: 19/08/2026
Phương pháp: thao tác thật trên trình duyệt (Playwright + Brave qua CDP) với server chạy tại http://localhost:5000; kiểm chứng kèm truy vấn SQLite (realestate.db).
Kết quả chung: toàn bộ kịch bản ĐẠT qua 3 lượt kiểm thử (v1, v2 toàn diện, v3 sau redesign). Ba lỗi phát hiện trong quá trình test đều đã sửa (xem phần cuối).

## Tài khoản

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Admin | admin@demo.com | Admin@123 |
| Seller | seller@demo.com | Seller@123 |
| Buyer | anhnha@demo.com | Test@12345 |

## Kết quả theo vai trò

### Khách

| Hạng mục | Kết quả |
|---|---|
| Trang chủ: hero, 6 tin nổi bật | Đạt |
| Tìm kiếm và lọc (từ khóa, quận, loại, giá, diện tích, phòng, mua/thuê), phân trang, sắp xếp | Đạt |
| Chi tiết tin: gallery, bản đồ Leaflet, thông số, breadcrumb | Đạt |
| Liên hệ / lưu tin bị chặn đúng khi chưa đăng nhập | Đạt |
| Dự đoán giá ML hiển thị kết quả và chỉ số (R2 0.963, RMSE 2.428.848.273, MAE 1.638.812.770) | Đạt |

### Người mua

| Hạng mục | Kết quả |
|---|---|
| Đăng ký, tự đăng nhập sau đăng ký | Đạt |
| Lưu / bỏ lưu tin; trang Tin đã lưu cập nhật đúng | Đạt |
| Gửi liên hệ (bắt buộc số điện thoại), lưu vào DB | Đạt |
| Sửa hồ sơ | Đạt |

### Người bán

| Hạng mục | Kết quả |
|---|---|
| Đăng tin kèm ảnh, trạng thái Chờ duyệt, chưa hiện công khai | Đạt |
| Sửa tin, quay về trạng thái Chờ duyệt | Đạt |
| Xóa tin (có hộp thoại xác nhận) | Đạt |

### Quản trị

| Hạng mục | Kết quả |
|---|---|
| Dashboard: số liệu khớp 100% với DB | Đạt |
| Duyệt / từ chối / khóa tin | Đạt |
| Quản lý người dùng: khóa, đổi vai trò, xóa (dòng admin được bảo vệ) | Đạt |
| Quản lý loại bất động sản: thêm, ẩn, hiện | Đạt |

### Vòng đời một tin (liên vai trò)

Đăng (seller) -> Chờ duyệt -> Duyệt (admin) -> Công khai -> Xóa (seller). Toàn bộ ĐẠT.

## Ma trận toàn diện (21 nhóm, dùng làm checklist cho lần test sau)

| # | Nhóm | Nội dung | Kết quả |
|---|---|---|---|
| 1 | Khách - trang chủ | Hero + 6 tin nổi bật | Đạt |
| 2 | Khách - danh sách | Phân trang, sort, lọc phòng ngủ | Đạt |
| 3 | Khách - chi tiết | Bản đồ Leaflet, gallery; chưa đăng nhập không có nút lưu tin | Đạt |
| 4 | Buyer - đăng ký | Form đầy đủ, tự đăng nhập sau đăng ký | Đạt |
| 5 | Buyer - yêu thích | Lưu 2 tin -> hiện 2 -> bỏ lưu 1 -> còn 1 | Đạt |
| 6 | Buyer - liên hệ | Gửi kèm SĐT -> lưu DB đúng | Đạt |
| 7 | Buyer - validate | Gửi không có SĐT -> bị chặn, không ghi DB | Đạt |
| 8 | Buyer - hồ sơ | Sửa tên/SĐT/địa chỉ -> lưu và hiển thị lại đúng | Đạt |
| 9 | Seller - đăng tin | Chờ duyệt, không hiện công khai | Đạt |
| 10 | Seller - sửa tin | Về Chờ duyệt, không lộ công khai | Đạt |
| 11 | Seller - xóa tin | Có dialog xác nhận, biến mất khỏi DB | Đạt |
| 12 | Admin - duyệt tin | Duyệt/ từ chối / khóa; enum 0=Pending 1=Approved 2=Rejected 3=Banned | Đạt |
| 13 | Admin - quyền | /Admin chặn người không phải admin; /MyListings/Create chặn admin | Đạt |
| 14 | Công khai - lifecycle | Tin duyệt hiện ở /Listings; tin từ chối/khóa trả 404 | Đạt |
| 15 | Admin - loại BĐS | Thêm/ẩn/hiện loại, đồng bộ với filter và form đăng tin | Đạt |
| 16 | Admin - người dùng | Khóa/mở khóa/đổi vai trò; dòng admin không có nút Khóa/Xóa | Đạt |
| 17 | Admin - xóa user | Xóa user không dữ liệu -> biến mất khỏi bảng + DB | Đạt |
| 18 | Admin - dashboard | Mọi số liệu khớp 100% truy vấn DB | Đạt |
| 19 | ML - dự đoán | Mua 11.268.080.000 đ; Thuê 11.200.080.000 đ | Đạt |
| 20 | ML - chỉ số | R2 0.963 / RMSE 2.428.848.273 / MAE 1.638.812.770 | Đạt |
| 21 | Toàn vẹn DB cuối | Dữ liệu không bị mất sau mọi thao tác | Đạt |

## Kiểm chứng dữ liệu cuối (realestate.db)

- 26 tin: 24 đã duyệt, 2 chờ duyệt (đúng trạng thái trước lượt test); các tin tạo để test đã xóa sạch sau vòng đời
- 5 tài khoản người dùng, 2 yêu cầu liên hệ, 2 tin yêu thích
- Quy ước trạng thái tin: 0 Chờ duyệt, 1 Đã duyệt, 2 Từ chối, 3 Bị khóa
- Trạng thái sau test: server vẫn chạy, DB sạch, mã nguồn đã commit

## Lỗi đã phát hiện và sửa qua các lượt test

### Lượt 1: Admin không xóa được tài khoản

Triệu chứng: bấm Xóa ở /Admin/Users -> lỗi 500, vi phạm khóa ngoại SQLite.

Nguyên nhân: DeleteUser chỉ xóa tài khoản, không xóa dữ liệu liên quan (yêu thích, liên hệ, tin, ảnh).

Cách sửa (AdminController.cs): xóa theo thứ tự yêu thích/liên hệ -> ảnh -> tin -> tài khoản; tài khoản admin được bảo vệ.

Xác minh: 3 ca kiểm thử cô lập (buyer có yêu thích + liên hệ, seller có tin + ảnh, admin) đều đúng.

### Lượt 2: Chỉ số ML bằng 0 sau khi khởi động lại

Triệu chứng: dự đoán vẫn đúng nhưng hiển thị R2 0, RMSE 0, MAE 0.

Nguyên nhân: chỉ số chỉ tính trong lúc huấn luyện (lần chạy đầu), không lưu; khi tải model có sẵn thì mặc định 0.

Cách sửa (PricePredictionService.cs): ghi ML/metrics.json khi huấn luyện xong, đọc lại khi tải model.

Xác minh: xóa model.zip -> huấn luyện lại -> số liệu đúng; khởi động lại -> vẫn đúng.

### Lượt 3 (sau redesign): CSP chặn script khởi tạo bản đồ

Chi tiết trong SECURITY.md mục 3. Sửa: chuyển script inline vào site.js, thêm cache-bust asp-append-version.

## Ghi chú môi trường (cho lần test sau)

- dotnet watch có thể chết âm thầm giữa chừng: kiểm tra `Invoke-WebRequest http://localhost:5000` trả 200 trước mỗi lượt, chết thì chạy lại
- Playwright kết nối Brave qua CDP cổng 9222; nếu báo ECONNREFUSED nghĩa là Brave đã đóng, chạy lại lệnh khởi động
- Hộp thoại xác nhận (xóa tin/user): đăng ký `page.on("dialog")` trước khi click; MCP báo "Modal state" treo là bình thường, xác nhận kết quả bằng DB
- Nút Sửa ở bảng Tin của tôi là thẻ `<a>`; nút Xóa là submit form kèm confirm()
- Trang Duyệt tin và Loại BĐS dùng div.card, không phải bảng
- Form dự đoán giá không có trường Giá; có PropertyType; thuê/bán qua checkbox IsForRent