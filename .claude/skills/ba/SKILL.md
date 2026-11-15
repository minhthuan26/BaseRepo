---
name: ba
description: Trao đổi với BA để làm rõ và chốt yêu cầu, rồi tách thành task kèm coding-spec (chưa code)
argument-hint: <yêu cầu thô hoặc tên tính năng>
disable-model-invocation: true
---

Bật chế độ BA cho yêu cầu: `$ARGUMENTS`

Dùng skill `ba-requirements` (hoặc đọc `.claude/skills/ba-requirements/SKILL.md`) và làm theo đúng quy trình, **ngay trong hội thoại này**:
1. Nắm bối cảnh từ roadmap và code, rồi hỏi tôi theo vòng.
2. Nháp `01-requirement.md`, chờ tôi **chốt yêu cầu**.
3. Tách task, viết `02-coding-spec.md` cho từng task, cập nhật roadmap, chờ tôi **duyệt spec**.
4. Dừng. Không code. Sau khi tôi duyệt, tôi sẽ chạy `/task-run <ID>`.

Nếu `$ARGUMENTS` trống, hỏi tôi muốn làm tính năng gì.
