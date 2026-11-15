# BaseRepo

## Kiến trúc
- Backend: .NET 10 / ASP.NET Core Web API — pattern: Clean Architecture 5-layer + CQRS (MediatR)
- Frontend: không áp dụng (repo chỉ có backend)
- CSDL: SQL Server — truy cập qua: Dapper qua BaseDbContext

## Lệnh
- Build BE: `dotnet build BaseRepo.slnx` · Test BE: `dotnet test BaseRepo.slnx`

## Nguyên tắc làm việc (bắt buộc)
1. **Roadmap là nguồn sự thật.** Trước khi làm bất cứ việc gì, đọc `.claude/docs/roadmap.md`. Chỉ làm tính năng có trong roadmap; yêu cầu mới phải được thêm vào roadmap trước (mã `N<số>`).
2. **Đồng bộ roadmap** theo `.claude/rules/roadmap-sync.md` ngay trong cùng lượt làm việc.
3. **Làm theo sprint.** Mỗi sprint tối đa N task (mặc định 5). Làm xong dừng lại, chờ người xác nhận. Không tự chạy sang sprint kế tiếp.
4. **Không loop vô hạn.** Test hoặc review thất bại tối đa 2 vòng sửa cho mỗi task. Quá giới hạn: đánh dấu 🟡 trong roadmap, ghi lý do vào task, dừng và báo người dùng.
5. **Tuân thủ conventions** trong `.claude/rules/convention-*.md` (backend, sql, testing, git). Chúng tự nạp theo loại file đang sửa. Không tự đặt quy ước mới.
6. **Mỗi task phải có đủ 6 file** trong `.claude/docs/tasks/<ID>-<slug>/` theo template ở `.claude/docs/templates/`.
7. Luôn theo `.claude/rules/agent-guidelines.md` (nghĩ trước, đơn giản, sửa đúng chỗ, kiểm chứng) và `.claude/rules/security.md`.
8. Chỉ đánh ✅ khi tính năng **đã chạy và đã verify** (build xanh, test xanh, review APPROVE).

## Quy trình một task
`/ba` (BA trao đổi với người dùng, **chốt yêu cầu**, tách task, **duyệt spec**) → `/task-run <ID>`: Code (`coder`, giao Antigravity qua MCP) → Test (`tester`) → Review (`reviewer`) → Bàn giao. Chi tiết: `.claude/docs/workflow.md`.

Điểm vào (gõ `/tên`): `roadmap-init`, `ba`, `sprint-plan`, `task-run`, `sprint-close`. Skill hỗ trợ: `ba-requirements`, `antigravity-dispatch`, `review-checklist`, `handover-writer`, `commit-helper`. Dự án mới chạy `/init-project` một lần để điền thông tin. Không code khi spec chưa được người dùng duyệt.

<!-- TOOLING:BEGIN -->
## Công cụ tiết kiệm token (CodeGraph + RTK)
Cài đặt và kiểm tra: `.claude/docs/tooling.md`.
- **CodeGraph** (MCP, chỉ mục trong `.codegraph/`): cần hiểu cấu trúc, tìm nơi một hàm/class được gọi, hoặc đánh giá ảnh hưởng của thay đổi → **dùng tool `mcp__codegraph__*` trước**, rồi mới Read các file mà nó chỉ ra. Không quét cả thư mục bằng Grep/Glob khi CodeGraph trả lời được.
- **RTK**: lọc output lệnh shell trước khi vào context; hook tự viết lại lệnh Bash (ví dụ `git status` → `rtk git status`). Tool Read/Grep/Glob có sẵn của Claude Code **không** đi qua hook: với file hoặc kết quả lớn, dùng `rtk read`, `rtk grep`, `rtk find`.
- RTK chỉ rút gọn output. Nếu phần bị lược làm mất thông tin cần để debug, chạy lại cách khác hoặc xem `.claude/docs/tooling.md`; không đoán lỗi từ output thiếu.
- Index CodeGraph có thể cũ nếu file watcher tắt: chạy `codegraph status` hoặc `codegraph sync` trước khi tin kết quả. Tool trả về trống hoặc lỗi thì quay lại Grep/Read, không dừng task.
<!-- TOOLING:END -->

## Đường lùi và compact
- Mỗi thay đổi rủi ro phải có đường lùi (revert commit, tắt cờ, rollback migration). Chưa có thì chưa làm. Mỗi sprint đóng bằng git tag để quay lại.
- Khi compact, luôn giữ: mục tiêu và tiêu chí nghiệm thu của task đang làm, danh sách file đã sửa, kết quả test gần nhất, lỗi đang điều tra. Dùng `/compact` kèm chủ đề cần giữ.
- Việc cần nhớ lâu thì ghi vào file này hoặc `.claude/docs/`, không để trong hội thoại.

## Tuyệt đối không
- Không đọc/ghi file chứa secret (`.env`, `appsettings.Production.json`, chuỗi kết nối).
- Không kết nối DB production hay dùng dữ liệu bệnh nhân thật.
- Không sửa migration/script SQL đã chạy; chỉ thêm script mới.
- Không xóa hoặc đổi tên file public API khi chưa có trong spec của task.
