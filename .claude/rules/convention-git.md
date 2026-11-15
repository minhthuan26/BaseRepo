---
description: "Quy ước đặt tên task, nhánh, commit"
---

# Convention Git và task

- Thư mục task: `<ID>-<slug-kebab-case>`, ví dụ `M03-dang-ky-nguoi-hien`.
- Nhánh: `feature/<ID>-<slug>`.
- Commit: Conventional Commits, mã task làm scope: `<type>(<ID>): <mô tả ngắn tiếng Anh>`, ví dụ `feat(M03): add donor registration API`. `type` ∈ `feat`, `fix`, `refactor`, `chore`, `docs`, `test`.
- Không `git push` khi người dùng chưa đồng ý (đã đặt `ask` trong `settings.json`).
