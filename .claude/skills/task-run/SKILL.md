---
name: task-run
description: Chạy một task đã có spec được duyệt qua Code → Test → Review → Bàn giao
argument-hint: <ID task, ví dụ M03 hoặc M03-1>
disable-model-invocation: true
---

Chạy task `$ARGUMENTS`. Làm tuần tự, không bỏ bước.

## Chuẩn bị
- Đọc `.claude/docs/roadmap.md`, `CLAUDE.md`, `.claude/rules/roadmap-sync.md`. Task phải có trong roadmap.
- Thư mục task: `.claude/docs/tasks/<ID>-<slug>/`. Phải có `02-coding-spec.md` với dòng "✅ đã duyệt". **Chưa có spec được duyệt thì dừng** và bảo tôi chạy `/ba` trước.
- Các task phụ thuộc phải đã xong.
<!-- TOOLING:BEGIN -->
- Nếu dùng CodeGraph: chạy `codegraph sync` để chỉ mục khớp với code hiện tại.
<!-- TOOLING:END -->

## Các bước
1. **Code** — làm theo skill `antigravity-dispatch`: giao spec cho công cụ code (Antigravity qua MCP), kiểm tra diff đúng phạm vi và build xanh, ghi `03-implementation-log.md`.
2. **Test** — gọi subagent `tester`, ghi `04-test-report.md`.
   - `FAIL`: quay lại bước 1 kèm `04-test-report.md`.
3. **Review** — gọi subagent `reviewer` (dùng skill `review-checklist`), ghi `05-review.md`.
   - `REQUEST_CHANGES`: quay lại bước 1 kèm `05-review.md`, rồi chạy lại Test và Review.
4. **Bàn giao** — chỉ khi Test `PASS` và Review `APPROVE`: làm theo skill `handover-writer`, ghi `06-handover.md`.
5. **Đồng bộ roadmap** — theo `roadmap-sync.md` Trigger 1.

**Giới hạn:** tối đa 2 vòng sửa cho mỗi task (gộp lỗi build, test, review). Vượt thì đánh 🟡 trong roadmap, ghi lý do vào `06-handover.md` mục "Việc tồn đọng", **dừng và báo tôi**. Không tự lặp tiếp.

## Kết thúc
Báo ngắn: kết quả test, verdict review, đường dẫn `06-handover.md`.
