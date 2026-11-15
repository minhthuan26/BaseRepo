# N5 — ConfigEnv: chuyển 4 form sang Designer · Bàn giao

- **Ngày:** 2026-11-16 · **Trạng thái:** ✅ xong (người dùng chạy tay đạt 2026-11-16, kể cả DPI cao) · **Sprint:** chưa gán

## 1. Tóm tắt
Bốn form của ConfigEnv đã chuyển từ dựng control bằng code sang **Designer** (`X.cs` + `X.Designer.cs` + `X.resx`): `FrmEnvTypeKeyConfirm`, `FrmEnvRestorePicker`, `FrmEnvListEditor` và `FrmCustomEnvEditor`. `FrmCustomEnvEditor` được **tách khỏi `UC_ConfigEnvCustom.cs`** thành file riêng (`internal partial`); `UC_ConfigEnvCustom.cs` chỉ mất class đó (0 dòng thêm, 411 dòng xóa). Hành vi, chữ ký constructor và `SelectedEntry` giữ nguyên. Quy tắc mới `.claude/rules/convention-winforms.md` (form mới dùng Designer) đã có từ trước task. Không đổi DB, registry, `.csproj`, không thêm package.

## 2. Giải thích luồng xử lý
Giao diện và luồng nghiệp vụ không đổi so với N1/N2; chỉ đổi **cách dựng control**.

1. Mỗi form: constructor gọi `InitializeComponent()` (trong `.Designer.cs`: khai báo control, thuộc tính, nối sự kiện); phần `.cs` còn lại là logic và gán dữ liệu sau khi dựng.
2. Control đặt tên `prefix_Name` (`btn_Save`, `txt_Key`, `lst_Items`, `cbo_GenType`, `nud_RandomLength`, `pnl_Warning`...), handler `<control>_<Event>`.
3. `FrmCustomEnvEditor` vẫn được `UC_ConfigEnvCustom.cs` gọi bằng cùng chữ ký `(isEditMode, key, currentValue, isCoreEdit)`; cùng namespace nên không cần `using` mới. Logic ẩn/hiện/khóa theo `isEditMode`/`isCoreEdit` và nhóm sinh giá trị (AES/GUID/ngẫu nhiên/RSA) vẫn nằm trong `.cs`.
4. Nơi gọi (`UC_ConfigEnvCustom.cs`) không phải sửa.

## 3. File đã thay đổi
| Đường dẫn (trong `SystemManagement/Views/ConfigEnv/`) | Vai trò |
|---|---|
| `FrmEnvTypeKeyConfirm.cs` (sửa) + `.Designer.cs` (tạo) | Hộp xác nhận gõ lại tên key |
| `FrmEnvRestorePicker.cs` (sửa) + `.Designer.cs` (tạo) | Chọn bản backup để khôi phục |
| `FrmEnvListEditor.cs` (sửa) + `.Designer.cs` (tạo) | Trình sửa danh sách `Path`/`PATHEXT`/`PSModulePath` |
| `FrmCustomEnvEditor.cs`, `.Designer.cs`, `.resx` (tạo) | Form thêm/sửa biến, tách khỏi `UC_ConfigEnvCustom.cs` |
| `UC_ConfigEnvCustom.cs` (sửa) | Chỉ xóa class `FrmCustomEnvEditor` |
| 3 file `.resx` có sẵn | Không đổi (đã commit ở `f9011f5`) |

## 4. Thay đổi DB
Không có.

## 5. Cách chạy và kiểm tra
- Build: `dotnet build BaseRepo.slnx -m:1` → **0 Error**, 913 Warning, không `WFO1000` (Claude chạy lại độc lập).
- Test tự động: repo chưa có test project. Tester dựng harness ngoài repo, dựng bản **cũ** (từ git HEAD) và **mới** của từng form trên luồng STA, so 12 cặp (cả các chế độ Thêm/Sửa/Sửa cốt lõi, 4 kiểu sinh giá trị, picker và list editor rỗng/3 mục): ghép 100% control theo (loại, Text, vị trí, cha), mọi thuộc tính khớp ở 100% DPI; phần logic sau constructor giống hệt bản cũ.
- **Kiểm tra thủ công (CHƯA CHẠY, cần bạn):**
  1. Mở từng form trong Visual Studio Designer (4 form), xác nhận không lỗi và đủ control.
  2. Chạy ứng dụng (Administrator): Thêm biến thường, Sửa biến thường, Sửa biến cốt lõi (hộp gõ lại tên key), Khôi phục (picker), Sửa `Path` (trình sửa danh sách); giao diện và hành vi như trước.
  3. **Thử ở DPI 125%/150%** (xem mục 7).

## 6. Kết quả kiểm chứng
- Test: **PASS** cho phần chạy được (`04-test-report.md`); AC6, AC7 chưa chạy, không tính PASS.
- Review: **APPROVE** vòng 1, 0 blocker/major (`05-review.md`).
- Đã kiểm độc lập: diff đúng danh sách mục 3 spec; grep `new Button|Label|TextBox|ListBox|Panel|ComboBox|NumericUpDown...` trong 4 file `.cs` rỗng; không log; không `.csproj`.

## 7. Rủi ro và lưu ý
- **DPI cao (khác biệt duy nhất đáng kể):** Designer mới đặt `AutoScaleMode=Font` (7x15, theo mẫu `UC_ConfigEnvCustom.Designer.cs`/`FrmMain.Designer.cs`); bản cũ không có. Ở 100% DPI giống hệt; app dùng PerMonitorV2 nên ở 125%/150% form mới **sẽ co giãn** còn bản cũ thì không, chưa kiểm bằng mắt. Riêng chế độ sửa cốt lõi của `FrmCustomEnvEditor` đặt lại `ClientSize` 540x200 và vị trí nút trong code sau `InitializeComponent`, cần xem kỹ ở DPI cao.
- Khác biệt vô hại ghi nhận: `lbl_RsaNote` dùng `\r\n` thay `\n`; `ComboBox.FormattingEnabled` true; thứ tự z-order `Controls` đảo (TabIndex vẫn khớp, không control chồng nhau); `lst_Items.ItemHeight=15` ghi tường minh; lambda Hủy thành method `btn_Cancel_Click`; `cbo_GenType.SelectedIndex=0` giờ kích handler một lần (idempotent).
- `FrmEnvRestorePicker` chỉ kiểm với lưới rỗng trong harness (cột đã khớp), chưa kiểm với file backup thật.
- Designer viết tay không qua Visual Studio nên có thể được VS định dạng lại khi bạn lưu lần đầu (không ảnh hưởng hành vi).
- Chỉ **một vòng code**, không vòng sửa. Model `gemini-3.8-flash-medium` được nhận (`warning: null`), không xác nhận độc lập model thực chạy.
- Thư mục harness tạm của Tester **chưa dọn** (lệnh xóa bị chặn), nằm ngoài repo trong scratchpad của phiên: `...\scratchpad\h` và `...\scratchpad\x`; xóa tay được.

## 8. Việc tồn đọng / đề xuất tiếp theo
- **Chạy tay checklist** mục 5 (đặc biệt DPI 125%/150%), rồi mới đánh ✅ cho N5.
- Nếu DPI cao làm lệch form: chỉnh `AutoScaleMode` hoặc kích thước trong Designer (task nhỏ).
- **N4-1 → N4-2** làm tiếp (N4-2 sửa `FrmEnvListEditor`, `FrmEnvRestorePicker`, `FrmCustomEnvEditor` đã ở file riêng).
- Thay đổi chưa commit (4 file sửa, 6 file tạo + docs N5); nhánh `feature/N1-configenv-env-tuy-chinh`, commit gốc `f9011f5`. Cần bạn quyết định commit (gợi ý `refactor(N5): move ConfigEnv forms to designer` kèm một commit `docs(N5)`). `bash.exe.stackdump` không commit.

## 9. Liên kết
- Yêu cầu: `01-requirement.md` · Spec: `02-coding-spec.md` · Log: `03-implementation-log.md` · Test: `04-test-report.md` · Review: `05-review.md`
