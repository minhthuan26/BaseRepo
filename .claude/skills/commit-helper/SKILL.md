---
name: commit-helper
description: Soạn message commit từ diff đã stage theo quy ước `<type>(<ID>): mô tả` (Conventional Commits) của dự án, quét secret trước khi commit. Dùng khi người dùng nói "commit" hoặc sau khi stage thay đổi. Không bao giờ push.
---

# commit-helper

Biến thay đổi đã stage thành **một** message commit rõ ràng. Không push, không commit secret.

## Các bước
1. Chạy `git diff --staged`. Chưa stage gì thì bảo người dùng `git add` trước rồi dừng.
2. Đọc diff, xác định thay đổi chính. Nhiều thay đổi không liên quan thì đề xuất tách thành nhiều commit thay vì một message mơ hồ.
3. Soạn message theo `.claude/rules/convention-git.md`:
   - Dòng đầu: `<type>(<ID>): <mô tả ngắn tiếng Anh>` (ví dụ `feat(M03-1): add donor registration API`, `type` ∈ feat/fix/refactor/chore/docs/test), thể mệnh lệnh/ngắn gọn, không quá ~72 ký tự, không dấu chấm cuối.
   - Chưa rõ ID task thì hỏi người dùng, không tự đặt.
   - Thân (tùy chọn): 1–3 dòng giải thích **vì sao**, không lặp lại **cái gì** (diff đã cho thấy).
4. Quét diff tìm thứ không được commit: khóa API, token, mật khẩu, chuỗi kết nối, nội dung `.env`, dữ liệu bệnh nhân thật. Có thì **dừng và cảnh báo**, không commit.
5. Hiển thị message đề xuất; chỉ chạy `git commit -m "..."` sau khi người dùng xác nhận (`git commit` đang ở `ask` trong `settings.json`).

## Quy tắc
- Một thay đổi logic một commit.
- Không `git push` từ skill này.
- Không thêm tiền tố/phạm vi khi không chắc.

_Dựa trên `commit-helper` của thenguyenvn90/claude-code-starter (MIT), đổi sang quy ước `<type>(<ID>): mô tả`._
