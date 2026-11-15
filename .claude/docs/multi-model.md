# Kết nối đa model (Claude · Antigravity · GPT)

Các model không dùng chung context. Điểm nối duy nhất là **file trong repo** (spec, log, review) và lệnh gọi qua CLI hoặc MCP. Claude Code là bộ điều phối.

## Vai trò mặc định
- **Claude** (Claude Code): BA, Tester, điều phối, bàn giao.
- **Antigravity**: Coder.
- **GPT**: Reviewer.

## Cách nối — các lựa chọn

| Cách | Mô tả | Khi nào dùng |
|---|---|---|
| Plugin | Cài CLI của model khác chạy trong Claude Code. Ví dụ plugin chính thức của OpenAI cho Codex cung cấp lệnh review như `/codex:review`. | Reviewer bằng GPT |
| MCP | Chạy một MCP server bọc model/công cụ ngoài, Claude gọi như tool. Khai báo trong `.mcp.json`. Ví dụ server đa model như `pal-mcp-server`. | Cần gọi nhiều model |
| CLI | Subagent `coder` / `reviewer` dùng `Bash` để chạy CLI của công cụ ngoài, truyền prompt từ file. | Công cụ chỉ có CLI |
| Router | Đổi endpoint API của Claude Code sang nhà cung cấp khác. | Chỉ khi muốn đổi model cho chính Claude |

> **Cần xác nhận trước khi cấu hình:** Antigravity trên máy bạn có cung cấp MCP server hoặc CLI hay không, và tên lệnh chính xác là gì. Điền kết quả vào mục "Cấu hình thực tế" bên dưới. Trong lúc chưa có, `coder` và `reviewer` chạy ở **chế độ tự làm** (Claude làm hộ) nên workflow vẫn chạy được.

## Cấu hình thực tế

Skill `antigravity-dispatch` đọc mục này để biết cách gọi công cụ code.

```
Coder  (Antigravity):
  MCP server  = antigravity (khai báo trong .mcp.json: npx mcp-server-google-antigravity)
  Tool giao việc = mcp__antigravity__use_antigravity
                   (tham số: prompt, add_dirs = ["D:\\Mitalab_Testing\\BaseRepo"], mode = "accept-edits")
  Lấy kết quả = mcp__antigravity__antigravity_result (jobId trả về ngay; poll với wait_ms tối đa 300000)
  Cách truyền spec = đường dẫn file trong prompt (.claude/docs/tasks/<ID>-<slug>/02-coding-spec.md), không dán nội dung
  Ghi chú = print_timeout mặc định 10m; job lỗi/treo thì antigravity_cancel. Diff phải kiểm tra lại bằng git diff.

Review (Claude tự review):
  Cách gọi = subagent `reviewer` chế độ B (Claude tự review theo skill review-checklist).
             Chưa nối GPT/Codex; khi nối thì cập nhật mục này.
```

Nhớ cho phép tool MCP của công cụ code trong `.claude/settings.json` (`permissions.allow`, dạng `mcp__<server>__<tool>`), nếu không mỗi lần gọi sẽ bị hỏi quyền.

## Mẫu prompt giao việc cho Coder ngoài

```
Bạn là lập trình viên. Thực thi đúng tài liệu sau, không làm thêm ngoài phạm vi.
- Spec: .claude/docs/tasks/<ID>-<slug>/02-coding-spec.md
- Conventions: .claude/rules/convention-*.md
- Chỉ được sửa các file liệt kê trong spec.
- Sau khi xong: chạy dotnet build BaseRepo.slnx, báo kết quả và liệt kê file đã đổi.
```

## Mẫu prompt giao việc cho Reviewer ngoài

```
Review diff dưới đây theo .claude/rules/convention-*.md và .claude/docs/tasks/<ID>-<slug>/02-coding-spec.md.
Chỉ đọc, không sửa code. Trả về từng phát hiện: mức độ (blocker/major/minor), file:dòng, vấn đề, gợi ý.
Dòng cuối: VERDICT: APPROVE hoặc VERDICT: REQUEST_CHANGES.
```

## Bảo mật
Diff và spec được gửi sang dịch vụ bên ngoài. Không đưa vào: chuỗi kết nối, secret, dữ liệu bệnh nhân thật. Kiểm tra chính sách của tổ chức trước khi bật.
