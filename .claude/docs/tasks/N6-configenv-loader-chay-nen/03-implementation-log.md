# N6 — ConfigEnv: loader và chạy nền cho Lưu/Xóa/Khôi phục · Implementation Log

- **Người thực hiện:** Antigravity (MCP `use_antigravity`, `mode=accept-edits`, model yêu cầu `gemini-3.8-flash-medium`, `warning: null`), điều phối và kiểm tra bởi Claude · **Ngày:** 2026-11-16 · **Vòng:** 1
- Commit gốc: `3bb6d1cd35e6d9983435bbdaa0da3b7b2854feae` · Nhánh: `feature/N1-configenv-env-tuy-chinh`.
- Chuẩn bị: người dùng yêu cầu "commit N5 rồi chạy N6". N5 đã commit (`bc960f9` code, `3bb6d1c` docs), cây sạch trước khi giao (chỉ còn `bash.exe.stackdump`). Build nền: 0 Error, 913 Warning.
- Model không có cách xác nhận độc lập model thực chạy ngoài việc agy nhận tham số.

## 1. File đã sửa
Đối chiếu `git status`/`git diff --stat`: đúng 3 file spec mục 3 (220 thêm, 39 xóa), không `.Designer.cs`/`.resx`/`FrmMain.cs`/`CustomEnvService.cs`.

| Đường dẫn (trong `SystemManagement/Views/ConfigEnv/`) | Tóm tắt |
|---|---|
| `FrmEnvListEditor.cs` | `btn_Save_Click` → `async void`; `FindMissingPaths` và `UpdateCoreValue` chạy `RunWithLoadingAsync(() => Task.Run(...))`; `_isSaving`, `SetSavingState`, `OnFormClosing`; `btn_Cancel_Click` thoát khi đang lưu; `Close()` gọi sau `finally` (đã mở khóa) |
| `FrmCustomEnvEditor.cs` | 3 nhánh `UpdateCoreValue`/`UpdateValue`/`Create` đổi sang `Task.Run`; `_isSaving`, `SetSavingState`, `OnFormClosing`; đọc `txt_Key`/`txt_Value`/`EnvKeys` vào biến cục bộ trước khi chạy nền; coder thêm guard `_isSaving` cho `btn_Generate_Click` (nhỏ, ngoài spec) |
| `UC_ConfigEnvCustom.cs` | Xóa (`Delete`) và Khôi phục (`RestoreCoreValue`) đổi lambda sang `Task.Run` (17 dòng) |

## 2. Quyết định kỹ thuật
- Không đổi `FrmMain.RunWithLoadingAsync` (chạy action trên luồng UI, được `UC_ConfigEnv.cs`/`FrmMain.cs` dùng với action chạm UI); việc nặng tự bọc `Task.Run` ở nơi gọi.
- Khi lỗi `UnauthorizedAccessException`/`SecurityException`: `SetSavingState(false)` rồi form ở lại để thử lại; `KeyNotFoundException`: `abortRequired` → `DialogResult.Abort` + `Close()` sau khi mở khóa.
- `grep Console./Debug./Trace./Log.` trong 2 form: rỗng.

## 3. Điểm lệch so với spec
Coder báo 0 lệch. Claude đọc diff `FrmEnvListEditor.cs` đầy đủ: khớp spec (thứ tự kiểm tra, nhánh catch, `finally` mở khóa trước `Close()`). **Chưa đọc kỹ diff `FrmCustomEnvEditor.cs`** (135 dòng): giao cho Reviewer. Điểm ngoài spec đã thấy: guard `_isSaving` ở `btn_Generate_Click` (vô hại, nên cho Reviewer xác nhận).

## 4. Kết quả build
- Coder tự báo: 0 Error, không `WFO1000`.
- Claude chạy lại độc lập: lần 1 có **1 lỗi môi trường** `Insufficient system resources` ở `AdministrativeUnit.Infrastructure.GeneratedMSBuildEditorConfig.editorconfig` (không liên quan code đã sửa); chạy lại ngay: ✅ **0 Error**, 698 Warning (build tăng dần), không `WFO1000`.

## 5. Việc cần chú ý cho Tester / Reviewer
- Mọi lời gọi `UpdateCoreValue/UpdateValue/Create/Delete/RestoreCoreValue/FindMissingPaths` ở 3 file phải nằm trong `Task.Run` bên trong `RunWithLoadingAsync`; lambda nền không được chạm control UI.
- `_isSaving`: luôn trả về `false` trong `finally`; `Close()` không bị `OnFormClosing` chặn; Hủy/X bị chặn khi đang lưu.
- Khe giữa hai pha của `FrmEnvListEditor.btn_Save_Click` (pha `FindMissingPaths` rồi hộp Yes/No rồi pha ghi): `_isSaving` về `false` giữa hai pha nhưng hộp thoại modal chặn bấm; xác nhận không có đường chạy song song.
- Không gọi hàm chạm registry/env thật khi test; AC6–AC8 (loader chạy, UI không đứng, bấm lặp) cần người dùng, ghi "chưa chạy".
