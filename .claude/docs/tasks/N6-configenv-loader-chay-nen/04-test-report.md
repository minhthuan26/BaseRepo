# N6 — ConfigEnv: loader và chạy nền cho Lưu/Xóa/Khôi phục · Test Report

- **Ngày:** 2026-11-16 · **Vòng:** 1

## 1. Đối chiếu tiêu chí nghiệm thu
| Tiêu chí | Test / cách kiểm | Kết quả |
|---|---|---|
| AC1 | `dotnet build BaseRepo.slnx -m:1`: 0 Error, 664 Warning, không có WFO1000 | ✅ |
| AC2 | Đọc code + grep: `FindMissingPaths`, `UpdateCoreValue` (FrmEnvListEditor); `UpdateCoreValue`/`UpdateValue`/`Create` (FrmCustomEnvEditor, 3 nhánh); `RestoreCoreValue`/`Delete` (UC_ConfigEnvCustom dòng 145, 274): đều trong `Task.Run` bên trong `RunWithLoadingAsync` | ✅ |
| AC3 | Lambda nền chỉ dùng biến cục bộ (`normalized`, `joinedValue`, `_key`, `key`, `val`, `sysDefaultKeys`, `selectedEntry`, `deleteKey`); `txt_*`/`FrmMain.SysConfig` đọc trước khi chạy nền. `_key` là readonly field string, không phải control | ✅ |
| AC4 | `git status`: FrmMain.cs, .Designer.cs, .resx, CustomEnvService.cs không đổi | ✅ |
| AC5 | Các catch giữ nguyên; `_isSaving` trả về false trong `finally`; `Close()` gọi sau `finally`; không có Console./Debug./Trace./Log. | ✅ |
| AC6 | Loader chạy, kéo cửa sổ được, X bị chặn khi ghi | chưa chạy (cần Administrator, thủ công) |
| AC7 | Bấm Lưu 2 lần chỉ 1 backup; lỗi thì mở khóa lại | chưa chạy (thủ công). Phần tĩnh: guard `_isSaving` đã kiểm bằng harness |
| AC8 | Thêm/Sửa/Xóa/Khôi phục trên UI | chưa chạy (thủ công) |
| AC9 | `git diff --stat`: đúng 3 file (FrmCustomEnvEditor, FrmEnvListEditor, UC_ConfigEnvCustom), 220 thêm / 39 xóa | ✅ |

## 2. Kết quả chạy
- Lệnh: `dotnet build BaseRepo.slnx -m:1` (không có test project tự động cho UI).
- Build: 0 Error, 664 Warning.
- Harness tạm (console net10.0-windows, STA, ngoài repo): 52 kiểm tra, 52 đạt, 0 lỗi. FrmMain chỉ là stub, không chạm registry/env, không gọi dịch vụ ghi thật.

## 3. Test mới đã thêm
Không thêm file vào repo. Harness tạm kiểm bằng reflection, cho cả 2 form:
- `SetSavingState(true)`: `_isSaving` true, `UseWaitCursor` true, khóa btn_Save/btn_Cancel/txt_*/lst_* và các nút còn lại (ListEditor: Up/Down/Delete/Add/Update; CustomEditor: grp_Gen, btn_Generate, cbo_*, txt_GuidPrefix, nud_RandomLength).
- `SetSavingState(false)`: mở khóa, tắt wait cursor; ListEditor khôi phục đúng theo `UpdateButtonStates`.
- `OnFormClosing`: `e.Cancel` true khi `_isSaving`, false khi không.
- `btn_Cancel_Click` khi `_isSaving`: DialogResult vẫn None. `btn_Save_Click` khi `_isSaving`: thoát ngay, không thông báo, không đổi DialogResult.

## 4. Lỗi (nếu có)
Không có lỗi sản phẩm. Ghi chú: khe giữa hai pha của `FrmEnvListEditor.btn_Save_Click` (`_isSaving` về false giữa FindMissingPaths và hộp Yes/No) được che bằng hộp thoại modal; harness không kiểm được, cần quan sát thủ công khi chạy AC6/AC7.

## 5. Phần chưa chạy
AC6, AC7, AC8 (cần người dùng Administrator, thử biến an toàn và khôi phục ngay). Không chạm registry trong lượt test này.

## 6. Dọn file tạm
CHƯA dọn: lệnh `rm -rf` thư mục harness tạm bị hook "Fact-Forcing Gate" chặn (2 lần). Thư mục còn lại ngoài repo: `C:\Users\X1503ZA-L1418W\AppData\Local\Temp\claude\81b2bab8ed1c5b58\54f0e1d4-3bef-445c-ab54-f2537d0b86da\scratchpad\n6h` (h.csproj, Program.cs, bin, obj). Cần người dùng tự xóa hoặc cho phép.

VERDICT: PASS
