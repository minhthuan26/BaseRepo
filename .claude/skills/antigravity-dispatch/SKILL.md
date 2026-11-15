---
name: antigravity-dispatch
description: Giao một task đã có coding-spec được duyệt cho công cụ code ngoài (Antigravity qua MCP), rồi kiểm tra kết quả (diff đúng phạm vi, build xanh) và ghi implementation log. Dùng ở bước Code của /task-run.
---

# Giao task cho Antigravity (Antigravity) và kiểm tra kết quả

## Cấu hình cần có
Tên MCP server và tool dùng để giao việc nằm trong `.claude/docs/multi-model.md`, mục "Cấu hình thực tế". Chưa điền thì **dừng và hỏi người dùng**, hoặc chuyển sang chế độ tự code (xem cuối). Tool MCP phải được cho phép trong `.claude/settings.json` (mục `permissions.allow`, dạng `mcp__<server>__<tool>`).

## Quy trình

### 1. Kiểm tra trước khi giao
- `02-coding-spec.md` có dòng "✅ đã duyệt". Chưa duyệt thì dừng.
- Các task phụ thuộc đã xong (xem roadmap).
- `git status` sạch (không có thay đổi dở của người dùng). Nếu không sạch: dừng, hỏi người dùng commit/stash trước.
- Ghi lại commit gốc: `git rev-parse HEAD`.
- Build nền xanh: `dotnet build BaseRepo.slnx`. Đỏ sẵn thì báo người dùng, không giao.

### 2. Đóng gói prompt
Prompt gồm (dùng đường dẫn, không dán nguyên file dài nếu công cụ đọc được file):
```
Bạn là lập trình viên. Thực thi đúng tài liệu sau, không làm ngoài phạm vi.
- Spec: .claude/docs/tasks/<ID>-<slug>/02-coding-spec.md
- Conventions: các file `.claude/rules/convention-*.md` khớp loại file được sửa (backend, sql, testing, git)
- Chỉ được tạo/sửa các file trong mục "File được phép tạo/sửa" của spec.
- Không sửa migration/script SQL đã chạy; chỉ thêm script mới.
- Không đọc/ghi file secret; không dùng DB production.
- Xong: chạy dotnet build BaseRepo.slnx, báo kết quả và liệt kê file đã đổi.
```
Nếu là vòng sửa lỗi: đính kèm `04-test-report.md` hoặc `05-review.md` của vòng trước và nói rõ chỉ sửa các điểm được nêu.
Không đưa vào prompt: chuỗi kết nối, secret, dữ liệu bệnh nhân thật.

### 3. Gọi công cụ
Gọi tool MCP đã cấu hình với prompt trên. Tool có thể chạy lâu; nếu lỗi/time-out, báo người dùng, **không tự gọi lặp**.

### 4. Kiểm tra kết quả (không tin lời tự báo cáo)
- `git status` và `git diff --stat`: đối chiếu từng file với danh sách được phép.
- File ngoài danh sách: liệt kê cho người dùng và hỏi hoàn tác hay chấp nhận (ghi lý do vào log). Không tự xóa file chưa theo dõi.
- Có sửa script SQL đã chạy, file secret, hoặc xóa test hiện có không → coi là lỗi, báo người dùng.
- Chạy lại build: `dotnet build BaseRepo.slnx`. Đỏ thì gửi lại cho công cụ code kèm lỗi (tính vào giới hạn 2 vòng sửa).
- Đọc lướt diff theo `02-coding-spec.md`: có làm đủ các mục chưa, tên đặt đúng convention chưa.

### 5. Ghi nhật ký
Viết `03-implementation-log.md` theo template: file đã đổi, quyết định kỹ thuật, điểm lệch so với spec, kết quả build, việc cần Tester/Reviewer chú ý. Ghi rõ công cụ nào đã code và số vòng.

### 6. Chuyển bước
Build xanh và diff đúng phạm vi thì mới chuyển cho Tester. Không tự đánh ✅ trong roadmap.

## Chế độ tự code (khi chưa có MCP)
Chỉ khi người dùng đồng ý: Claude tự thực thi spec theo các quy tắc trên, vẫn qua bước 4–5.

## Giới hạn
Tối đa 2 vòng sửa cho mỗi task (gộp lỗi build, test, review). Vượt thì đánh 🟡, ghi lý do, dừng và báo người dùng.
