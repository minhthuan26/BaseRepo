---
name: roadmap-init
description: Quét dự án hiện có và dựng .claude/docs/roadmap.md ban đầu (module nào xong, nào dở, nào chưa làm)
disable-model-invocation: true
---

Dựng `.claude/docs/roadmap.md` ban đầu cho dự án này.

<!-- TOOLING:BEGIN -->
0. Nếu dự án dùng CodeGraph: kiểm tra `.codegraph/` đã có chưa (`codegraph status`). Chưa có thì đề nghị người dùng chạy `codegraph init -i` (không tự cài đặt). Có rồi thì dùng nó để liệt kê module/tính năng nhanh hơn khi quét.
<!-- TOOLING:END -->
1. Đọc `CLAUDE.md`, `.claude/docs/roadmap.md` (hiện là bản mẫu), `.claude/rules/roadmap-sync.md`.
2. Quét cấu trúc mã nguồn (BE, DB) để liệt kê các module/tính năng đang có. Dùng subagent `ba-analyst` nếu dự án lớn.
3. Hỏi tôi bằng danh sách ngắn: các module dự kiến, thứ tự ưu tiên, tính năng nào cần 🔑 hoặc là 🏗️. Nếu tôi đã đưa danh sách trong `$ARGUMENTS`, dùng luôn.
4. Điền roadmap đúng cấu trúc 5 mục. Đánh ✅ chỉ khi thấy bằng chứng tính năng chạy được (có test hoặc code hoàn chỉnh); nghi ngờ thì đánh 🟡.
5. Đặt mã `M<số>` cho module, `N<số>` cho tính năng mới; cập nhật số `N` kế tiếp ở mục 4.
6. Trình bày bản roadmap cho tôi duyệt. **Không bắt đầu code.**

Ghi thêm dòng "Khởi tạo roadmap" vào `.claude/docs/changelog.md`.
