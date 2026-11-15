---
name: init-project
description: Chạy một lần sau khi copy bộ .claude vào dự án mới. Phỏng vấn kiến trúc BE/FE/DB, điền mọi placeholder {{...}}, chỉnh rules theo stack, bật/tắt CodeGraph và RTK, điền cấu hình Antigravity MCP.
argument-hint: (không cần tham số)
disable-model-invocation: true
---

# init-project — điền thông tin dự án vào bộ `.claude/`

Bộ `.claude/` đã nằm sẵn trong dự án (do `install.mjs` copy). Skill này chỉ **hỏi, điền, chỉnh, kiểm tra**. Không copy file từ nơi khác, không bắt đầu code tính năng.

## 1. Tự dò trước, hỏi sau
Chỉ đọc, bỏ qua file secret (`.env`, `appsettings.Production.json`):
- `*.sln`, `*.csproj` → phiên bản .NET, cấu trúc project, EF Core/Dapper/MediatR.
- `package.json` → React, TypeScript, bundler, thư viện state/UI/data, script `build`/`test`/`lint`.
- Thư mục script SQL, `DbContext`, tên chuỗi kết nối (không đọc giá trị) → loại DB.
- Shell: `codegraph --version`, `rtk --version`, `rtk gain` (phát hiện cài nhầm gói "rtk" khác).
- `git rev-parse --show-toplevel` để biết gốc dự án; `.claude/` phải nằm ở gốc đó.

## 2. Phỏng vấn (AskUserQuestion, tối đa 4 câu mỗi lần, đưa giá trị dò được lên đầu)
- **Backend:** stack, pattern (Clean Architecture, CQRS, N-tier…), cách truy cập DB, framework test.
- **Frontend:** có hay không; stack, thư viện chính, framework test.
- **CSDL:** loại DB, có dùng stored procedure không, quy ước migration.
- **Lệnh:** build/test BE và FE (đề xuất từ bước 1).
- **Công cụ agent:** công cụ code (mặc định Antigravity qua MCP), công cụ review (mặc định GPT qua Codex plugin/MCP). Nếu đã biết cách gọi Antigravity: tên MCP server, tên tool, cách truyền spec (đường dẫn file hay nội dung), ghi chú.
- **Tiết kiệm token (tùy chọn):** CodeGraph, RTK; hỏi riêng từng cái, nói rõ `rtk init -g` sửa cấu hình toàn cục của máy.
- **Dữ liệu nhạy cảm** (y tế, cá nhân): có/không → nếu có, hỏi thêm file secret của dự án để thêm vào `deny` trong `settings.json` và `hooks/guard-secrets.mjs`.
Tóm tắt toàn bộ và **xin xác nhận một lần** trước khi ghi.

## 3. Điền và chỉnh
- Thay `{{PROJECT_NAME}}`, `{{BE_STACK}}`, `{{BE_PATTERN}}`, `{{BE_BUILD_CMD}}`, `{{BE_TEST_CMD}}`, `{{FE_STACK}}`, `{{FE_LIBS}}`, `{{FE_BUILD_CMD}}`, `{{FE_TEST_CMD}}`, `{{DB_ENGINE}}`, `{{DB_ACCESS}}`, `{{CODER_TOOL}}`, `{{REVIEW_TOOL}}` trong **mọi** file dưới `.claude/` (kể cả `skills/*/SKILL.md` và `agents/*.md`), trừ chính skill này.
- `.claude/rules/convention-*.md`: sửa nội dung và `paths:` cho khớp stack và cấu trúc thư mục thật (ví dụ không phải `.cs` hoặc FE nằm ở `ClientApp/**`). Nếu dự án đã có quy ước riêng (editorconfig, tài liệu team) thì dùng nội dung đó. Cho người dùng xem phần đã chỉnh trước khi ghi.
- Không có FE: xóa `convention-frontend.md`, bỏ dòng FE trong CLAUDE.md, templates và các skill; ghi "không áp dụng".
- `.claude/docs/multi-model.md` mục "Cấu hình thực tế": ghi MCP server, tool, cách truyền spec, ghi chú. Thêm `mcp__<server>__<tool>` vào `permissions.allow` của `settings.json`. Chỉ thêm server vào `.mcp.json` khi người dùng đưa cấu hình; **không tự bịa**. Chưa biết thì để trống và ghi "chạy chế độ Claude tự làm".
- `.claude/docs/roadmap.md` header: ngày hôm nay và tên dự án.

## 4. CodeGraph và RTK
Phần liên quan nằm giữa `<!-- TOOLING:BEGIN -->` và `<!-- TOOLING:END -->` trong CLAUDE.md, `docs/`, agents, skills, templates.
- **Dùng cả hai:** giữ nguyên; thêm `.codegraph/` vào `.gitignore` của dự án.
- **Chỉ một:** xóa phần của công cụ còn lại trong các khối; bỏ `Bash(rtk:*)` hoặc `Bash(codegraph:*)` + `mcp__codegraph__*` khỏi `settings.json`; bỏ mục `codegraph` khỏi `.mcp.json`; bỏ tool `mcp__codegraph__*` khỏi dòng `tools:` của agent nếu bỏ CodeGraph.
- **Không dùng:** xóa mọi khối TOOLING (cả dấu), xóa `docs/tooling.md`, `.mcp.json` để `"mcpServers": {}`, dọn `settings.json` và `tools:` như trên.
- Không tự cài phần mềm. Chưa cài thì đưa lệnh trong `docs/tooling.md` và hỏi có muốn chạy hộ. Không dùng `curl | sh`, `irm | iex`. Không cài gói `rtk` trên crates.io (gói khác).

## 5. Kiểm tra
- Không còn `{{` (trừ trong skill này); không còn `TOOLING:` nếu đã chọn không dùng.
- Mọi đường dẫn nhắc trong CLAUDE.md, skills, agents trỏ tới file có thật.
- `settings.json` và `.mcp.json` là JSON hợp lệ; `.mcp.json` ở **gốc dự án** (Claude Code chỉ đọc `.mcp.json` ở gốc).
- `.claude/docs/` không bị `.gitignore` bỏ qua; `settings.local.json` thì có trong `.gitignore`.
- Thử Grep/Glob trên `.claude/docs/` (ví dụ tìm `roadmap`); nếu bị bỏ qua vì thư mục ẩn, báo người dùng, vẫn đọc được bằng đường dẫn rõ ràng.

## 6. Báo kết quả
Nêu ngắn: đã điền gì, cái gì còn để trống và vì sao; việc người dùng cần làm (cài CodeGraph/RTK, nối Antigravity/GPT, bổ sung file secret); khởi động lại Claude Code rồi `/mcp` để kiểm tra tên tool CodeGraph thật. Bước kế: `/roadmap-init`, `/ba <yêu cầu>`, `/task-run <ID>`. Có thể xóa `.claude/skills/init-project/` sau khi xong.

## Nguyên tắc
Không đọc/ghi file secret. Không bịa thông tin kiến trúc: chưa biết thì để placeholder và báo. Không ghi đè nội dung người dùng đã sửa mà chưa hỏi.
