---
name: sprint-close
description: Đóng sprint — tổng kết, kiểm tra roadmap, chờ tôi OK rồi gắn git tag version
argument-hint: [số sprint]
disable-model-invocation: true
---

Đóng sprint `$ARGUMENTS` (mặc định là sprint đang mở trong `.claude/docs/tasks/SPRINT-*.md`).

1. Đối chiếu từng task của sprint: roadmap, `.claude/docs/tasks/<ID>-*/06-handover.md`, kết quả test và review.
2. Chạy toàn bộ test một lần cuối: `dotnet test BaseRepo.slnx`.
3. Chạy checklist cuối lượt trong `.claude/rules/roadmap-sync.md`; sửa lệch nếu có.
4. Viết tổng kết sprint vào `SPRINT-<số>.md`: task xong, task 🟡 và lý do, rủi ro, đề xuất sprint kế tiếp.
5. Cập nhật `.claude/docs/changelog.md` với version mới (theo `vMAJOR.MINOR.PATCH`) và header roadmap.
6. **Dừng chờ tôi xác nhận.** Chỉ sau khi tôi nói OK mới tạo commit và tag:
   - `git add -A`, rồi dùng skill `commit-helper` để soạn và xác nhận message "Sprint <số>: <tóm tắt>" (skill quét secret trong diff trước khi commit)
   - `git tag sprint-<số>`
   - Không `git push`; việc đẩy code do tôi quyết định.

Rollback khi cần: `git revert` các commit của sprint hoặc quay về tag trước đó; ghi lại việc rollback vào `changelog.md`.
