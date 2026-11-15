---
name: sprint-plan
description: Chọn N tính năng từ mục Ưu tiên của roadmap để làm trong một sprint, rồi chờ xác nhận
argument-hint: [số task, mặc định 5]
disable-model-invocation: true
---

Lập kế hoạch sprint mới.

1. Đọc `.claude/docs/roadmap.md` và `.claude/docs/changelog.md` để biết sprint gần nhất.
2. Số task: `$ARGUMENTS` nếu có, mặc định 5. Không vượt quá 10.
3. Chỉ chọn task đã có `02-coding-spec.md` được duyệt (dòng "✅ đã duyệt"); mục nào chưa có spec thì đề nghị tôi chạy `/ba` trước. Lấy N tính năng đầu mục "3. Ưu tiên tiếp theo". Bỏ qua tính năng 🔑 chưa có credential và 🏗️ (mỗi subsystem lớn là một sprint riêng).
4. Kiểm tra phụ thuộc giữa các task, sắp xếp lại nếu cần.
5. Tạo `.claude/docs/tasks/SPRINT-<số>.md` gồm: mục tiêu, danh sách task (ID, tên, phụ thuộc), tiêu chí xong sprint.
6. Trình bày cho tôi và **dừng chờ xác nhận**. Chưa được OK thì không chạy `/task-run`.
