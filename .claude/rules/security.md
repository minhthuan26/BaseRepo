# Bảo mật

Áp dụng cho mọi thay đổi. Phân vân thì dừng và hỏi.

## Secret và dữ liệu nhạy cảm
- Không hardcode khóa API, token, mật khẩu, chuỗi kết nối. Đọc từ biến môi trường hoặc kho bí mật.
- Không ghi secret hay dữ liệu bệnh nhân/người hiến vào log, thông báo lỗi, file commit hay prompt gửi model ngoài.
- Trước khi commit, quét diff tìm thứ giống khóa. Secret đã từng commit thì phải **đổi khóa**; xóa ở commit mới không xóa khỏi lịch sử git.
- Chỉ dùng DB dev/test, dữ liệu giả hoặc đã ẩn danh.

## Đầu vào và ranh giới
- Validate đầu vào ở ranh giới (API, file tải lên, hệ thống ngoài).
- Truy vấn luôn tham số hóa; không nối chuỗi SQL, lệnh shell, đường dẫn từ đầu vào không tin cậy.
- Kiểm tra quyền ở **mỗi hành động được bảo vệ** phía server, không chỉ ở giao diện.

## Code do AI sinh ra
- Code "trông đúng" vẫn có thể sai ở phân quyền và trường hợp biên. Test luồng xác thực với token sai, token hết hạn, và người dùng không đủ quyền.
- Dependency mới phải kiểm tra là gói thật (đúng tên, đúng tác giả, lượt tải hợp lý); gõ sai tên có thể kéo về gói giả mạo. Cài gói mới cần hỏi người dùng (đã đặt `ask`).
- Mỗi thay đổi rủi ro cần đường lùi: revert commit, tắt cờ, hoặc rollback migration. Chưa có đường lùi thì chưa sẵn sàng.
