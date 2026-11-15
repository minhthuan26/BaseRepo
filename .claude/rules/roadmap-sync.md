# Roadmap Sync — Luôn giữ `.claude/docs/roadmap.md` đúng thực tế

**Level: MANDATORY.** Mỗi khi (1) code xong một tính năng có trong roadmap, hoặc (2) người dùng yêu cầu thêm tính năng/ý tưởng mới → PHẢI cập nhật `.claude/docs/roadmap.md` và `.claude/docs/changelog.md` **trong cùng lượt làm việc**. Không để roadmap lệch với code.

## Cấu trúc roadmap (giữ nguyên, không tự đổi)

| Mục | Nội dung |
|---|---|
| Header | Ngày cập nhật, version mới nhất, ký hiệu |
| 1. Tổng quan tiến độ | Bảng module: trạng thái + "Còn lại" |
| 2. Việc còn lại | 2.1 Làm được ngay (mã `M<số>`), 2.2 Tính năng mới (mã `N<số>`), 2.3 Cần thứ bên ngoài 🔑, 2.4 Subsystem lớn 🏗️ |
| 3. Ưu tiên tiếp theo | Thứ tự gợi ý, chỉ chứa việc CHƯA xong |
| 4. Cách thêm tính năng | Mã `N` kế tiếp (hiện tại: ghi rõ số) |
| 5. Đã xong | Theo module, mỗi module có anchor `xong-<số>` |

Ký hiệu: ✅ xong · 🟡 làm một phần · ⬜ chưa làm · 🔑 cần credential/hạ tầng ngoài · 🏗️ subsystem lớn.

## Trigger 1 — Code xong một tính năng

Chỉ áp dụng khi tính năng **đã chạy được và đã verify** (build xanh, test xanh, review APPROVE). Chưa verify → KHÔNG đánh ✅.

1. Xóa dòng khỏi mục 2.x (backlog).
2. Thêm vào mục 5 "Đã xong" đúng module, kèm link tới `.claude/docs/tasks/<ID>-<slug>/06-handover.md`.
3. Cập nhật trạng thái module ở mục 1 (⬜ → 🟡 → ✅) và cột "Còn lại".
4. Xóa khỏi mục 3 "Ưu tiên".
5. Ghi một dòng vào `.claude/docs/changelog.md` (ngày, ID, mô tả ngắn).

## Trigger 2 — Người dùng thêm tính năng/ý tưởng mới

1. Lấy mã `N` kế tiếp ở mục 4, thêm dòng vào mục 2.2 (hoặc 2.3/2.4 nếu cần 🔑/🏗️).
2. Tăng số `N` ở mục 4.
3. Đề xuất vị trí trong mục 3 "Ưu tiên", để người dùng quyết định.
4. Không bắt đầu làm ngay trừ khi người dùng yêu cầu.

## Quy tắc chung
- Mỗi tính năng có đúng một mã duy nhất, không đổi mã sau khi đã cấp.
- Không xóa lịch sử: tính năng xong chuyển sang mục 5, không biến mất.
- Subsystem lớn 🏗️ mỗi cái một sprint riêng.
- Tính năng tách thành nhiều task con (`M03-1`, `M03-2`…): dòng cha giữ 🟡 cho đến khi mọi task con xong, rồi mới ✅.

## Checklist trước khi kết thúc lượt
- [ ] Roadmap phản ánh đúng code hiện tại?
- [ ] Không có tính năng nào ✅ mà chưa verify?
- [ ] Yêu cầu mới phát sinh đã có mã `N` chưa?
- [ ] `changelog.md` đã có dòng mới?
- [ ] Ngày cập nhật ở header đã đổi?
