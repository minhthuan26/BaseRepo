# Nguyên tắc làm việc của agent

Thiên về cẩn thận hơn tốc độ. Việc quá nhỏ thì dùng phán đoán.

## 1. Nghĩ trước khi code
- Nêu rõ giả định. Không chắc thì hỏi; có nhiều cách hiểu thì trình bày, không âm thầm chọn một.
- Có cách đơn giản hơn thì nói ra.
- **Phạm vi ảnh hưởng:** liệt kê nơi gọi, hợp đồng API, migration, cache, cờ cấu hình bị đụng tới (dùng `codegraph_impact` nếu có). Không liệt kê được thì chưa đủ cơ sở để sửa.
- **Biết cái đã có:** validation, phân quyền, xử lý lỗi hiện hữu. Không viết trùng, không lách qua; cần đổi thì đổi rõ ràng.

## 2. Đơn giản trước
- Chỉ làm điều được yêu cầu: không tính năng thừa, không trừu tượng cho code dùng một lần, không "cấu hình linh hoạt" chưa ai cần.
- Không xử lý lỗi cho tình huống không thể xảy ra.
- Viết 200 dòng mà làm được trong 50 thì viết lại.
- Cỡ một lần thay đổi: mục tiêu ~200 dòng, tối đa ~400. Quá thì tách; refactor tách khỏi tính năng.

## 3. Sửa đúng chỗ
- Không "tiện tay" sửa code, comment, định dạng bên cạnh. Theo style hiện có.
- Thấy code chết không liên quan thì ghi chú, đừng xóa.
- Chỉ dọn thứ **chính thay đổi của bạn** làm thừa (import, biến, hàm).
- Phép thử: mỗi dòng đổi phải truy được về yêu cầu hoặc spec.

## 4. Có tiêu chí kiểm chứng
- "Thêm validation" → viết test cho đầu vào sai rồi làm cho pass. "Sửa lỗi" → viết test tái hiện rồi làm cho pass. "Refactor" → test xanh trước và sau.
- Việc nhiều bước: nêu kế hoạch ngắn, mỗi bước kèm cách kiểm tra.
- **Kiểm chứng mọi khẳng định:** hàm có tồn tại thì mở file xem; test pass thì chạy; API trích dẫn thì đọc tài liệu.
- Mỗi lần sửa lại là một thay đổi mới, kiểm tra với cùng độ kỹ.
- Lặp để đạt kết quả xanh **nhưng trong giới hạn 2 vòng sửa mỗi task** (xem CLAUDE.md). Hết giới hạn thì dừng và báo, không lặp tiếp.

_Ý lấy cảm hứng từ `.claude/rules/AGENT.md` của repo thenguyenvn90/claude-code-starter (MIT), viết lại cho workflow này._
