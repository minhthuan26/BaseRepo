# N2-1 — ConfigEnv: mở khóa sửa biến cốt lõi + backup + khôi phục · Test Report

**VERDICT (vòng 2): PASS** (chỉ cho phần tự động/đọc code kiểm được; các AC cần UI hoặc quyền Administrator ở mục "AC chưa chạy" không tính là PASS)

- **Ngày:** 2026-11-15 · **Vòng:** 2 (sau Review vòng 1). Vòng 1 giữ nguyên ở dưới làm lịch sử.
- Harness vòng 2 (ngoài repo, scratchpad `n2-1-harness`, biên dịch trực tiếp 3 file service + CommonLibs). Tiến trình **không elevated** (`Elevated=False`), nên không ghi registry; không gọi `WriteRaw`/`BroadcastSettingChange`/`UpdateCoreValue`/`RestoreCoreValue` với key hệ thống thật.
- **Kết quả: 54 assertion · Đạt 54 · Lỗi 0** (vòng 1 là 32; vòng 2 bỏ 1 assertion "multiline bị cắt" vì hành vi đã sửa, thêm 23 assertion mới). `dotnet build BaseRepo.slnx -m:1`: **0 Error**, 913 Warning, không có `WFO1000`.

## Vòng 2 — test cho phần sửa
| Nhóm | Case | Kết quả |
|---|---|---|
| (a) TryRead | Value nhiều dòng LF: đọc đủ `line1
line2
line3
` | PASS |
| (a) | Header CRLF + Value nhiều dòng CRLF: đọc đủ `a
b
c`, Key/Reason đúng | PASS |
| (a) | Value rỗng, file LF (ExpandString) và CRLF | PASS (2) |
| (a) | Value chứa `Value=` ở dòng sau: giữ nguyên toàn bộ | PASS |
| (a) | CreatedAt sai: `notadate`, `2026-01-01 00:00:00`, `01/02/2026 00:00:00`, `2026-13-45T00:00:00`, `2026-01-01T00:00` bị bỏ qua (null) | PASS (5) |
| (a) | Đọc dưới culture `ar-SA` vẫn parse đúng CreatedAt (InvariantCulture) | PASS |
| (b) ListBackups | Mới nhất trước; bỏ file rác/thiếu Reason/Kind=DWord/Kind=Foo/CreatedAt sai; file tên A nhưng Key=B bị loại khỏi A, vào B; key hoa/thường; key lạ/rỗng; key M chỉ trả 2 file nhiều dòng | PASS |
| (c) ReadRaw | Key thử không tồn tại: không ném, trả null (chỉ đọc registry); đọc `OS` (chỉ đọc) trả giá trị | PASS (2) |
| (d) Restore key có khoảng trắng | `" Path"`, `"Path "`, `"	Path"`, `"   "` đều ném `ArgumentException`; số file backup không đổi | PASS (5) |
| (d) | `RestoreCoreValue` entry.Key=`N2_TEST_X` và `UpdateCoreValue("N2_TEST_X")` ném `InvalidOperationException`, không tạo backup, `ReadRaw("N2_TEST_X")` vẫn null | PASS (3) |
| Vòng 1 | Toàn bộ AC3, AC4, AC5, AC6 (CreateBackup lỗi), AC1 (đọc key thiếu) chạy lại | PASS |

## Đối chiếu bằng đọc code (không chạy được vì cần elevated/ghi registry)
- `UpdateCoreValue` (`CustomEnvService.cs:88-122`): thứ tự key rỗng -> `IsProtected` -> `IsElevated` -> `ValidateValue` -> kiểm xuống dòng (`:103`) -> `ReadRaw` -> kiểm `Kind` String/ExpandString (`:111`) -> `CreateBackup` (`:115`) -> `WriteRaw` (`:118`) -> Broadcast. Mọi validate Value/xuống dòng/Kind nằm **trước** `CreateBackup` và `WriteRaw`; không có try/catch nuốt lỗi backup.
- `RestoreCoreValue` (`:129-171`): null -> key rỗng -> key có khoảng trắng đầu/cuối (`:137`) -> `IsProtected` -> `IsElevated` -> `ValidateValue(entry.Value)` -> kiểm xuống dòng (`:150`) -> `ReadRaw` (không nuốt lỗi) -> kiểm Kind hiện tại (`:159`) -> `CreateBackup` -> `WriteRaw` -> Broadcast. Validate dữ liệu backup nằm trước backup và ghi.
- `ReadRaw` (`EnvRegistryService.cs:40-56`) không còn try/catch: key thiếu trả null, lỗi mở khóa ném `InvalidOperationException`, nên `RestoreCoreValue` không bỏ qua backup trong trường hợp lỗi đọc (quan sát vòng 1 #1 đã xử lý).
- `TryRead` (`EnvBackupService.cs:71-152`): Value lấy từ sau `Value=` đến hết file (quan sát vòng 1 #2 đã xử lý); CreatedAt `TryParseExact` + `InvariantCulture`.

## Kiểm tĩnh
- AC14: grep `Console.|Log.|Debug.|Trace.` trên 7 file mã (gồm Designer): 0 kết quả.
- AC15: `git status`/`git diff --stat` chỉ có đúng 7 file mục 3 spec (3 sửa, 4 tạo) cùng `redis_8003.conf` bị xóa, `bash.exe.stackdump` và 3 file tài liệu task (có sẵn, ngoài phạm vi). Không có thư mục `Backups` trong repo.
- Designer: TabIndex trong `pnl_Toolbar` là 0..7 (lbl_Search 0, txt_Search 1, btn_ToggleMask 2, btn_Restore 3, btn_Refresh 4, btn_Add 5, btn_Edit 6, btn_Delete 7), không trùng.
- Build: 0 Error, không WFO1000.

## Bug
Không có bug.

Quan sát nhỏ (không phải fail): vì Value đọc đến hết file, file backup sửa tay có newline cuối (ví dụ editor tự thêm) sẽ cho Value có `
` cuối. `RestoreCoreValue` từ chối giá trị chứa xuống dòng (`:150`) nên không ghi dữ liệu sai, nhưng người dùng sẽ không khôi phục được từ file đó cho tới khi xóa newline cuối. File do `CreateBackup` tạo không có newline cuối (`sb.Append`). Harness đã đối chiếu hành vi này (`old
`, `fakeB
`).

## AC chưa chạy (không tính PASS)
- AC1 (phần ExpandString thô), AC2, AC9, AC10: cần ghi/tạo key ở HKLM, tiến trình không elevated, không tự nâng quyền.
- AC7, AC8 (chạy thật), AC11, AC12, AC13: cần UI WinForms và/hoặc quyền Administrator. Chỉ đối chiếu bằng đọc code. Checklist chạy tay ở mục 7 của vòng 1 vẫn áp dụng.
- Các nhánh `RestoreCoreValue`/`UpdateCoreValue` sau bước `IsElevated` (Kind hiện tại sai, ghi thật): chỉ xác nhận thứ tự qua đọc code.

## Dọn dẹp
- Không tạo `N2_TEST_*` ở registry (không ghi registry). Đã xóa thư mục `Backups` do harness tạo (xác nhận 0). Repo không có `Backups/`. Harness còn ở scratchpad, có thể xóa.

VERDICT: PASS

---

# Lịch sử — Báo cáo vòng 1

**(Vòng 1) VERDICT: PASS** (chỉ áp dụng cho phần tự động/đọc code kiểm được; các AC cần UI hoặc quyền Administrator ở mục 7 chưa chạy, không tính là PASS)

- **Ngày:** 2026-11-15 · **Vòng:** 1
- Phương pháp: repo chưa có test project xUnit và spec cấm thêm. Dùng harness console tạm ở scratchpad (ngoài repo): `C:\Users\X1503Z~1\AppData\Local\Temp\claude\81b2bab8ed1c5b58\54f0e1d4-3bef-445c-ab54-f2537d0b86da\scratchpad\n2-1-harness` (biên dịch trực tiếp `EnvRegistryService.cs`, `EnvBackupService.cs`, `CustomEnvService.cs` + ProjectReference `CommonLibs`, `net10.0-windows`). Không tạo/sửa file nào trong repo ngoài báo cáo này.
- **Tiến trình harness KHÔNG elevated** (`IsElevated() = False`). Vì vậy mọi test ghi registry bị bỏ qua (xem mục 7). Không gọi `UpdateCoreValue`/`RestoreCoreValue` trên key hệ thống thật, không gọi `WriteRaw`/`BroadcastSettingChange`.

## 1. Build (AC15)
- `dotnet build BaseRepo.slnx -m:1`: **0 Error**, 664 Warning (build tăng dần), không có `WFO1000`.
- `git status`/`git diff --stat`: đúng 7 file của mục 3 spec (3 sửa: `CustomEnvService.cs`, `UC_ConfigEnvCustom.cs`, `UC_ConfigEnvCustom.Designer.cs`; 4 tạo: `EnvRegistryService.cs`, `EnvBackupService.cs`, `FrmEnvTypeKeyConfirm.cs`, `FrmEnvRestorePicker.cs`). Ngoài phạm vi N2-1 có sẵn từ trước: `redis_8003.conf` bị xóa, `bash.exe.stackdump`, file `03-implementation-log.md` của task. Không có `Backups/` trong repo.

## 2. Đối chiếu tiêu chí nghiệm thu
| AC | Kiểm bằng | Kết quả | Bằng chứng |
|---|---|---|---|
| AC1 | Harness | ✅ một phần | `ReadRaw` key không tồn tại → `null` (PASS); key rỗng/trắng → `null` (PASS). Phần ExpandString `%SystemRoot%\x` thô: **chưa chạy** (cần tạo key thử ở HKLM, không elevated) → mục 7 |
| AC2 | Cần ghi HKLM | ⏸ chưa chạy | Không elevated → mục 7 |
| AC3 | Harness | ✅ | Thư mục `Backups\Env` tự tạo, nằm dưới `AppContext.BaseDirectory`; tên khớp regex `^N2_TEST_A_\d{8}_\d{6}(_\d+)?\.txt$`; nội dung đúng 5 dòng `Key/Kind/CreatedAt(yyyy-MM-ddTHH:mm:ss)/Reason/Value`, giá trị chứa `%SystemRoot%\x=y é` ghi/đọc nguyên vẹn (UTF-8); 3 lần gọi liên tiếp cùng giây cho 3 file khác nhau, file đầu không bị ghi đè; key chứa `:`/`/` được thay ký tự hợp lệ |
| AC4 | Harness | ✅ | Tạo file hỏng tay: rác, thiếu `Reason`, `Kind=DWord`, `Kind=Foo`, `CreatedAt` sai, file tên `N2_TEST_A_fake.txt` nhưng `Key=N2_TEST_B`. `ListBackups("N2_TEST_A")` trả đúng 4 bản hợp lệ, mới nhất trước (bản 2020 cuối), loại các file hỏng và file `Key` khác; `ListBackups` khớp key không phân biệt hoa thường; key lạ/rỗng → danh sách rỗng; `TryRead` roundtrip đúng Value/Kind/Reason, file không tồn tại → `null`, `Value=` rỗng được chấp nhận |
| AC5 | Harness | ✅ | `IsListKey`: `path`/`PATHEXT`/`pSmoDulePath` true; `TEMP`/rỗng/`N2_TEST_X` false. `IsProtected`: đủ 12 key (và `temp` không phân biệt hoa thường), `N2_TEST_X` false. `UpdateCoreValue("N2_TEST_X","v")` → `InvalidOperationException`, số file backup không đổi, `ReadRaw("N2_TEST_X")` vẫn `null`. `RestoreCoreValue` với key không phải cốt lõi → `InvalidOperationException`; entry `null` → `ArgumentNullException`; `GetCoreRawValue` key thiếu → `KeyNotFoundException` |
| AC6 | Harness (CreateBackup lỗi) + đọc code | ✅ | Đặt `Backups` là file thường → `CreateBackup` ném exception (PASS). Đọc code `CustomEnvService.cs:110-113`: `CreateBackup` nằm **trước** `WriteRaw`, không có try/catch bao quanh nên exception thoát ra trước khi ghi. Không chạy `UpdateCoreValue` đầu-cuối vì không elevated và cần key thật |
| AC7 | Đọc code | ✅ (đọc) / ⏸ thủ công | `FrmEnvTypeKeyConfirm.cs:56-67` OK `Enabled=false` ban đầu; `:94` bật chỉ khi `string.Equals(..., StringComparison.Ordinal)`; `:99-103` kiểm lại lần nữa trong `BtnOk_Click`; Cancel `DialogResult.Cancel`. Chưa chạy UI |
| AC8 | Đọc code | ⏸ chưa chạy | Đọc: `btn_Edit_Click:179-183` và `btn_Restore_Click:109-113` chặn `!IsElevated()` bằng cảnh báo trước khi mở hộp thoại/backup; `UpdateCoreValue:95` và `RestoreCoreValue:134` ném `UnauthorizedAccessException` trước khi `CreateBackup`/`WriteRaw`. Chưa chạy thực tế (không gọi hàm trên key thật) |
| AC9–AC10 | Cần Administrator + UI + ghi registry | ⏸ chưa chạy | Mục 7. Đọc code: thứ tự `UpdateCoreValue` (Protected → Elevated → Validate/xuống dòng → ReadRaw → Backup `Edit` → `WriteRaw` kind cũ → Broadcast) và `RestoreCoreValue` (Protected → Elevated → ReadRaw; nếu có thì Backup `BeforeRestore` → `WriteRaw(entry.Value, entry.Kind)` → Broadcast) đúng spec mục 7; form sửa nhận `GetCoreRawValue` (thô), `isCoreEdit` ẩn `grpGen` (`Visible = !_isCoreEdit`) và thu nhỏ form |
| AC11 | Đọc code | ⏸ thủ công / ✅ đọc | `btn_Edit_Click:173-177` list key → cảnh báo "chưa hỗ trợ", không mở form; `btn_Delete_Click:245-249` vẫn chặn mọi biến cốt lõi |
| AC12 | Đọc code | ⏸ thủ công / ✅ đọc | `btn_Restore_Click:103-107` biến thường → cảnh báo; `:115-120` chưa có backup → `ThrowInfo`, không đổi gì |
| AC13 | Đọc code | ⏸ thủ công | Nhánh biến thường (`:219-233`, `BtnSave_Click` sau nhánh `_isCoreEdit`) giữ nguyên logic N1; `isCoreEdit` mặc định `false` nên `btn_Add_Click` và form sinh value không đổi. Chưa chạy UI |
| AC14 | grep | ✅ | `grep -nE "Console\.|Log\.|Debug\.|Trace\."` trên 6 file mã (`EnvRegistryService`, `EnvBackupService`, `CustomEnvService`, `FrmEnvTypeKeyConfirm`, `FrmEnvRestorePicker`, `UC_ConfigEnvCustom`): 0 kết quả. Thông báo lỗi chỉ nêu key (trừ `ex.Message` của exception tự ném, cũng chỉ chứa key) |
| AC15 | Build + git | ✅ | Mục 1 |

Thêm: Designer `UC_ConfigEnvCustom.Designer.cs` khai báo `btn_Restore` đúng spec (`Point(555, 8)`, `Size(80, 32)`, `Click += btn_Restore_Click`, tooltip).

## 3. Kết quả chạy
- Harness: **32 assertion · Đạt: 32 · Lỗi: 0** (`dotnet run`, exit code 0). Trong đó 1 assertion ghi nhận hành vi đã biết (giá trị nhiều dòng bị cắt, xem mục 5).
- `dotnet test BaseRepo.slnx`: không chạy (chưa có test project, ngoài phạm vi theo spec).

## 4. Test mới đã thêm
Không thêm file vào repo. Harness chỉ nằm ở scratchpad (ngoài repo).

## 5. Lỗi / quan sát (không có bug chặn)
Không có lỗi ở phần kiểm được. Các điểm cần Reviewer/BA cân nhắc (không phải fail theo AC):
1. **`EnvRegistryService.ReadRaw` nuốt mọi exception → `null`** (`EnvRegistryService.cs:59-63`). Hệ quả đáng chú ý nhất: trong `RestoreCoreValue` (`CustomEnvService.cs:138-142`), nếu đọc lỗi (không phải "không tồn tại") thì bước backup `BeforeRestore` bị bỏ qua nhưng vẫn ghi đè giá trị hiện tại; backup không được bảo đảm trong trường hợp lỗi đọc hiếm gặp. Ở `UpdateCoreValue` thì chỉ báo nhầm "biến không còn tồn tại" (an toàn).
2. **`TryRead` chỉ đọc `Value=` một dòng** (`EnvBackupService.cs:103-106`): file backup bị sửa tay có giá trị nhiều dòng sẽ bị cắt (harness xác nhận: đọc ra `line1`), và `RestoreCoreValue` sẽ ghi giá trị bị cắt. File do chính `CreateBackup` tạo thì giá trị nhiều dòng của biến thật rất hiếm; `UpdateCoreValue` đã cấm xuống dòng.
3. `ListBackups` sắp xếp hậu tố `_10` vs `_2` theo chuỗi (chỉ khi > 9 backup cùng giây): bỏ qua được.
4. `AppContext.BaseDirectory` khi `PublishSingleFile`: chưa kiểm (giả định trong spec).

## 6. Dọn dẹp
- Không tạo giá trị `N2_TEST_*` nào ở registry/biến môi trường (mọi test ghi registry bị bỏ qua vì không elevated); `ReadRaw("N2_TEST_X")` trả `null` trước và sau.
- Đã xóa thư mục `...\scratchpad\n2-1-harness\bin\Debug\net10.0-windows\Backups` do harness tạo (xác nhận không còn thư mục `Backups` nào trong harness). Repo không có `Backups/`.
- Harness (mã nguồn + obj/bin) còn ở scratchpad, có thể xóa.

## 7. Chưa kiểm được (không tính là PASS)
- **AC1 (phần ExpandString thô), AC2**: cần tạo/ghi/xóa key `N2_TEST_*` ở HKLM `...\Session Manager\Environment` → cần Administrator; tiến trình không elevated, không tự nâng quyền.
- **AC7 (chạy UI), AC8 (chạy thật), AC9, AC10, AC11, AC12, AC13**: cần UI WinForms và/hoặc quyền Administrator và ghi biến cốt lõi thật. Chỉ đã đối chiếu bằng đọc code (mục 2).
- Checklist cho người chạy tay (chạy với quyền Administrator, trước đó `reg export "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" env.reg`):
  1. Mở tab Tùy chỉnh, chọn `NUMBER_OF_PROCESSORS`, Sửa: hộp gõ-lại-key chỉ bật OK khi gõ đúng hoa/thường; form sửa hiện giá trị thô, không có nhóm "Sinh giá trị"; Lưu → có file mới trong `Backups\Env\` cạnh exe, lưới hiện giá trị mới sau Làm mới, kiểu registry không đổi.
  2. Khôi phục bản vừa tạo: giá trị/kiểu về như cũ, có thêm backup `BeforeRestore`; sau test giá trị thật đúng như trước.
  3. Sửa `Path`/`PATHEXT`/`PSModulePath`: cảnh báo "chưa hỗ trợ"; Xóa mọi biến cốt lõi vẫn bị chặn; Khôi phục biến chưa có backup / chọn biến thường: thông báo đúng, không đổi gì.
  4. Chạy không Administrator: Sửa/Khôi phục biến cốt lõi bị chặn, không có file backup.
  5. Hồi quy N1: Thêm/Sửa/Xóa `N1_TEST_*`, form Thêm hiện đủ nhóm "Sinh giá trị".
  6. Khi xong: dọn `Backups\Env\`, khôi phục registry bằng `env.reg` nếu cần.

