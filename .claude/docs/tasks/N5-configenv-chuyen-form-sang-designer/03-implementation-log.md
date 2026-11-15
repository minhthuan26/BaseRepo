# N5 — ConfigEnv: chuyển 4 form sang Designer · Implementation Log

- **Người thực hiện:** Antigravity (MCP `use_antigravity`, `mode=accept-edits`, model yêu cầu `gemini-3.8-flash-medium`, `warning: null`), điều phối và kiểm tra bởi Claude · **Ngày:** 2026-11-16 · **Vòng:** 1
- Commit gốc: `f9011f54593b2615a8f0ed75fa3171e2604f0ee0` · Nhánh: `feature/N1-configenv-env-tuy-chinh`.
- Chuẩn bị: người dùng chọn "commit rồi chạy N5". Docs/rule commit ở `7b5022d`, 3 file `.resx` do Visual Studio tạo commit ở `f9011f5`. Cây sạch trước khi giao (chỉ còn `bash.exe.stackdump`, ngoài phạm vi). Build nền: 0 Error, 913 Warning.
- Model không có cách xác nhận độc lập model thực chạy ngoài việc agy nhận tham số.

## 1. File đã tạo/sửa
Đối chiếu `git status`/`git diff --stat`: đúng danh sách spec mục 3 (4 sửa, 6 tạo), không `.csproj`, không sửa 3 `.resx` có sẵn.

| Đường dẫn (trong `SystemManagement/Views/ConfigEnv/`) | Tạo/Sửa | Ghi chú |
|---|---|---|
| `FrmEnvTypeKeyConfirm.cs` | Sửa | `partial`, `InitializeComponent()` |
| `FrmEnvTypeKeyConfirm.Designer.cs` | Tạo | |
| `FrmEnvRestorePicker.cs` | Sửa | giữ `SelectedEntry` (có `DesignerSerializationVisibility(Hidden)`) |
| `FrmEnvRestorePicker.Designer.cs` | Tạo | |
| `FrmEnvListEditor.cs` | Sửa | |
| `FrmEnvListEditor.Designer.cs` | Tạo | |
| `FrmCustomEnvEditor.cs` | Tạo | tách từ `UC_ConfigEnvCustom.cs`, `internal partial` |
| `FrmCustomEnvEditor.Designer.cs` | Tạo | |
| `FrmCustomEnvEditor.resx` | Tạo | theo mẫu `.resx` có sẵn |
| `UC_ConfigEnvCustom.cs` | Sửa | **chỉ xóa** class `FrmCustomEnvEditor`: `git diff --numstat` = 0 thêm / 411 xóa |

## 2. Quyết định kỹ thuật
- Coder chuyển từng form một và build sau mỗi form (theo lời coder); Claude chỉ kiểm lại build cuối.
- Control đặt tên `prefix_Name` (`lbl_`, `txt_`, `btn_`, `pnl_`, `cbo_`, `nud_`...), sự kiện nối trong Designer, handler `<control>_<Event>`.

## 3. Điểm lệch so với spec
Coder báo 0 lệch, 0 thuộc tính không chắc. Claude kiểm bằng grep/diff: không còn `new Button|Label|TextBox|ListBox|Panel|ComboBox|NumericUpDown|DataGridView|CheckBox|GroupBox` trong 4 file `.cs` của form; không `Console./Debug./Trace./Log.`. **Chưa tự đối chiếu từng giá trị bố cục** (Location/Size/Font/màu/TabIndex): giao cho Tester (harness so sánh cây control bản cũ và mới) và Reviewer.

## 4. Kết quả build
- Coder tự báo: 0 Error, không `WFO1000`.
- Claude chạy lại độc lập: ✅ **0 Error**, 913 Warning (bằng nền), không `WFO1000`.

## 5. Việc cần chú ý cho Tester / Reviewer
- **Rủi ro chính:** Designer được viết tay (không có Visual Studio), dễ lệch layout hoặc thiếu thuộc tính (Anchor/Dock, TabIndex, DialogResult, Accept/CancelButton, Items ComboBox, Min/Max/Value của NumericUpDown, cột DataGridView/ListView của picker).
- Tester nên dựng harness ngoài repo: dựng bản **cũ** (`git show HEAD:<file>`, đổi tên namespace/class) và bản **mới**, khởi tạo form trên luồng STA (không `Application.Run`), duyệt cây `Controls` đệ quy so sánh Type, Text, Location, Size, Font, BackColor, ForeColor, Anchor, Dock, TabIndex, Visible, Enabled, ReadOnly, Items; và thuộc tính form. Không gọi hàm chạm registry.
- Logic ẩn/hiện/khóa theo chế độ của `FrmCustomEnvEditor` (`isEditMode`, `isCoreEdit`) phải giống bản cũ.
- AC6, AC7 (Designer mở được, luồng chạy tay) cần người dùng, Tester ghi "chưa chạy".
