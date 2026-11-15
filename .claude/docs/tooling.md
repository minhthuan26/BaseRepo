<!-- TOOLING:BEGIN -->
# Công cụ tiết kiệm token: CodeGraph + RTK

> Hai công cụ này **không bắt buộc** và độc lập nhau. Thông tin dưới đây tổng hợp từ tài liệu của từng dự án tại thời điểm soạn; phiên bản/lệnh có thể đổi, hãy đối chiếu README mới nhất.
> Có nhiều dự án tên "codegraph" và hai gói tên "rtk". Tài liệu này giả định:
> - **CodeGraph** = `colbymchenry/codegraph` (đồ thị mã nguồn + MCP server)
> - **RTK** = `rtk-ai/rtk` (Rust Token Killer)
> Nếu bạn dùng dự án khác cùng tên, cần sửa lại file này.

## 1. CodeGraph — hiểu mã nguồn mà không phải đọc cả thư mục

**Là gì:** quét mã bằng tree-sitter, lưu đồ thị (hàm, class, quan hệ gọi nhau) vào SQLite cục bộ `.codegraph/codegraph.db`, và phơi ra cho Claude qua MCP. Xử lý cục bộ, không cần API key. Hỗ trợ C# và TypeScript/TSX (đã kiểm tra trên README, riêng phần React nên tự thử trên dự án của bạn).

**Cài (Windows):** cần Node.js 18+ (README nêu không dùng Node 25.x).
```powershell
npm i -g @colbymchenry/codegraph
```
Có thêm trình cài PowerShell trong README của dự án; nếu dùng cách `irm ... | iex` (chạy script tải về), hãy đọc script trước. Mở terminal mới sau khi cài để `codegraph` nằm trong PATH.

**Dùng trong dự án:**
```powershell
cd <thư-mục-gốc-dự-án>
codegraph init -i      # tạo .codegraph/ và dựng chỉ mục
codegraph status       # xem thống kê
codegraph sync         # cập nhật tăng dần (watcher thường tự làm)
```
Thêm `.codegraph/` vào `.gitignore` (chỉ mục là dữ liệu cục bộ).

**Nối vào Claude Code:** dùng **một** trong hai cách, không dùng cả hai:
- `codegraph install` (trình cài ghi MCP + hướng dẫn + quyền tự cho phép ở mức người dùng), hoặc
- giữ mục `codegraph` trong `.mcp.json` của dự án (đã có sẵn trong `.mcp.json` của bộ mẫu).
Khởi động lại Claude Code, rồi chạy `/mcp` để kiểm tra server `codegraph` đã kết nối và xem **tên tool thực tế**.

**Tool thường gặp:** `codegraph_explore`, `codegraph_impact`, `codegraph_callers`, `codegraph_callees`, `codegraph_node`, `codegraph_search`, `codegraph_files`, `codegraph_status`. Có bản chỉ bật sẵn `codegraph_explore`, các tool khác bật qua biến môi trường `CODEGRAPH_MCP_TOOLS`. Danh sách `tools:` trong `.claude/agents/*.md` đang liệt kê `explore`, `impact`, `callers`; nếu tên thật khác hoặc bạn bật thêm tool, sửa dòng `tools:` cho khớp, nếu không subagent sẽ không thấy tool.

**Dùng ở đâu trong workflow:**
| Vai | Dùng để |
|---|---|
| `ba-analyst` (cho skill `ba-requirements`) | Khảo sát code và lập danh sách file bị ảnh hưởng, làm đầu vào cho mục "Phân tích ảnh hưởng" của `02-coding-spec.md` |
| `coder` | Xem nơi gọi và phụ thuộc trước khi sửa một hàm/class chung |
| `reviewer` | Kiểm tra phạm vi ảnh hưởng của diff (có chỗ gọi nào bị vỡ không) |

## 2. RTK — rút gọn output lệnh shell

**Là gì:** CLI proxy lọc output lệnh (git, test, build, ls, grep…) trước khi vào context của Claude. README nêu mức giảm 60–90% token, tùy dự án.

**Cài (Windows):** tải `rtk-x86_64-pc-windows-msvc.zip` ở trang Releases của `rtk-ai/rtk`, giải nén, thêm thư mục chứa `rtk.exe` vào PATH. Hoặc có Rust toolchain:
```powershell
cargo install --git https://github.com/rtk-ai/rtk
```
**Không** cài gói `rtk` trên crates.io (đó là "Rust Type Kit", gói khác). Kiểm tra: `rtk --version` rồi `rtk gain`; nếu `rtk gain` lỗi nghĩa là cài nhầm gói.

**Nối vào Claude Code:**
```powershell
rtk init -g            # cài hook + RTK.md (toàn cục, ghi vào cấu hình Claude của bạn)
rtk init --show        # kiểm tra
```
Khởi động lại Claude Code. Hook viết lại lệnh Bash (`git status` → `rtk git status`). Lưu ý `-g` sửa cấu hình **toàn cục** của máy bạn (ảnh hưởng mọi dự án); gỡ bằng `rtk init -g --uninstall`. Có tùy chọn `--hook-only` nếu không muốn thêm `RTK.md`.

**Giới hạn quan trọng:** hook chỉ viết lại lệnh của tool **Bash**. Tool Read/Grep/Glob có sẵn của Claude Code bỏ qua hook, nên với kết quả lớn hãy dùng `rtk read`, `rtk grep`, `rtk find`.

**Lệnh hay dùng:**
```powershell
rtk gain                   # thống kê token đã tiết kiệm (--daily, --graph, --history)
rtk git status | diff | log -n 10
rtk test <lệnh test>       # chỉ hiện phần lỗi
rtk err <lệnh build>       # chỉ hiện lỗi/cảnh báo
rtk read <file> -l aggressive   # chỉ chữ ký (signature)
```
Mặc định cấu hình RTK nằm ở thư mục cấu hình của người dùng; README chưa nêu đường dẫn riêng cho Windows, xem TROUBLESHOOTING của dự án khi cần.

**Dùng ở đâu trong workflow:**
| Vai | Dùng để |
|---|---|
| `coder` | Build (`rtk err ...`) và `rtk git diff` để xem thay đổi gọn |
| `tester` | `rtk test ...` để chỉ thấy test lỗi; vẫn ghi đủ số liệu vào `04-test-report.md` |
| `reviewer` | `rtk git diff` |

**Cảnh báo:** output rút gọn có thể làm mất ngữ cảnh khi debug lỗi lạ. Khi nghi ngờ, chạy lại lệnh gốc theo hướng dẫn trong tài liệu RTK. Số liệu trong `04-test-report.md` (tổng/đạt/lỗi) phải lấy từ kết quả thật, không suy từ bản rút gọn.

## 3. Kiểm tra sau khi cài
- [ ] `codegraph status` hiện số node/file > 0
- [ ] `/mcp` trong Claude Code thấy `codegraph` kết nối
- [ ] `rtk gain` chạy được; sau vài lệnh thì có số liệu
- [ ] `.codegraph/` đã nằm trong `.gitignore`
- [ ] Hỏi Claude "ai gọi hàm X?" và thấy nó dùng tool `codegraph_*` thay vì Grep hàng loạt

## 4. Bảo mật
- CodeGraph xử lý cục bộ. RTK chỉ lọc output trên máy bạn.
- Không cài bằng cách chạy script tải về mà chưa đọc. Ưu tiên `npm`, tệp release hoặc `cargo`.
- Hook RTK toàn cục và quyền tự cho phép (`mcp__codegraph__*`) làm giảm số lần hỏi quyền; xem lại bạn có muốn mức đó không, nhất là với dự án có dữ liệu y tế.
<!-- TOOLING:END -->
