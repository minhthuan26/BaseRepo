# N5 — ConfigEnv: chuyển 4 form sang Designer · Coding Spec

- **Trạng thái duyệt:** ✅ đã duyệt bởi Minh Thuận ngày 2026-11-16
- **Yêu cầu gốc:** `01-requirement.md` cùng thư mục · **Phụ thuộc:** N2-1, N2-2 (đã ✅) · **Làm trước N4-2**

## 1. Phạm vi
Chuyển `FrmEnvTypeKeyConfirm`, `FrmEnvRestorePicker`, `FrmEnvListEditor` và `FrmCustomEnvEditor` từ dựng control bằng code sang **Designer** (`X.cs` + `X.Designer.cs` + `X.resx`), **không đổi hành vi, giao diện, chữ ký công khai**. Tách `FrmCustomEnvEditor` khỏi `UC_ConfigEnvCustom.cs` thành file riêng. Ghi theo `.claude/rules/convention-winforms.md`.

## 2. Ngoài phạm vi
- `UC_ConfigEnv*`, `FrmMain`, form khác; phần còn lại của `UC_ConfigEnvCustom.cs` (chỉ **xóa** class `FrmCustomEnvEditor`, không sửa gì khác).
- Đổi layout/màu/chữ, thêm tính năng, đổi logic, thêm package, test project.

<!-- TOOLING:BEGIN -->
## 2b. Phân tích ảnh hưởng (CodeGraph)
Không dùng CodeGraph. Nơi gọi: `FrmEnvTypeKeyConfirm` được `UC_ConfigEnvCustom.cs` (`btn_Edit_Click`, `btn_Restore_Click`) gọi bằng `new FrmEnvTypeKeyConfirm(key, title, message)`; `FrmEnvRestorePicker` được `btn_Restore_Click` gọi `new FrmEnvRestorePicker(key)` và đọc `SelectedEntry`; `FrmEnvListEditor` được `btn_Edit_Click` gọi `new FrmEnvListEditor(key, rawValue)`; `FrmCustomEnvEditor` được `btn_Add_Click`, `btn_Edit_Click` gọi `new FrmCustomEnvEditor(isEditMode, key, currentValue, isCoreEdit)`. Cả 4 giữ nguyên chữ ký nên nơi gọi không phải sửa; `FrmCustomEnvEditor` vẫn cùng namespace nên không cần `using` mới.

| Đối tượng đổi | Bị ảnh hưởng | Cần cập nhật? |
|---|---|---|
| 4 form (cách dựng control) | Nơi gọi trong `UC_ConfigEnvCustom.cs` | Không (chữ ký giữ nguyên) |
| `FrmCustomEnvEditor` chuyển sang file riêng | `UC_ConfigEnvCustom.cs` (xóa class ở cuối file) | Có: chỉ xóa class |
<!-- TOOLING:END -->

## 3. File được phép tạo/sửa
Thư mục `SystemManagement/Views/ConfigEnv/`:

| Đường dẫn | Tạo/Sửa | Nội dung thay đổi |
|---|---|---|
| `FrmEnvTypeKeyConfirm.cs` | Sửa | Bỏ dựng control; `partial`, gọi `InitializeComponent()`, giữ logic và constructor |
| `FrmEnvTypeKeyConfirm.Designer.cs` | Tạo | `InitializeComponent`, khai báo control |
| `FrmEnvRestorePicker.cs` | Sửa | như trên; giữ `SelectedEntry` |
| `FrmEnvRestorePicker.Designer.cs` | Tạo | như trên |
| `FrmEnvListEditor.cs` | Sửa | như trên |
| `FrmEnvListEditor.Designer.cs` | Tạo | như trên |
| `FrmCustomEnvEditor.cs` | Tạo | Logic của `FrmCustomEnvEditor` chuyển từ `UC_ConfigEnvCustom.cs` (nguyên logic sinh giá trị AES/GUID/ngẫu nhiên/RSA, ẩn/hiện theo `isEditMode`/`isCoreEdit`, `BtnSave_Click`), `internal partial class` |
| `FrmCustomEnvEditor.Designer.cs` | Tạo | `InitializeComponent`, khai báo control |
| `FrmCustomEnvEditor.resx` | Tạo | `.resx` tối thiểu cùng định dạng 3 `.resx` đã có (mẫu: `FrmEnvListEditor.resx`) |
| `UC_ConfigEnvCustom.cs` | Sửa | **Chỉ xóa** class `FrmCustomEnvEditor` ở cuối file (đã chuyển sang file riêng); không đổi dòng nào khác |
| `FrmEnvTypeKeyConfirm.resx`, `FrmEnvRestorePicker.resx`, `FrmEnvListEditor.resx` | Giữ nguyên | Đã do Visual Studio tạo (untracked); **không xóa, không sửa nội dung**; chỉ đưa vào commit |

> Coder không được sửa file ngoài danh sách này nếu chưa báo lại. Không sửa `.csproj`. Mức độ: 8 file `.cs`/`.Designer.cs` + 4 `.resx`; chuyển từng form một, build sau mỗi form.

## 4. Thay đổi cơ sở dữ liệu
Không có.

## 5. API / giao diện
Không đổi (chữ ký constructor và property như mục 2b). Giao diện giống hệt bản hiện tại.

## 6. Tên theo convention
- `partial class` cho cả 4 form; `components`/`Dispose` theo mẫu `UC_ConfigEnvCustom.Designer.cs` (đọc file này làm mẫu). `FrmCustomEnvEditor` giữ `internal`.
- Control đặt tên `prefix_Name` (`btn_Save`, `btn_Cancel`, `btn_Add`, `btn_Update`, `btn_Delete`, `btn_Up`, `btn_Down`, `txt_Input`, `txt_Key`, `txt_Value`, `lst_Items`, `lbl_Title`, `lbl_Desc`, `lbl_Warning`, `pnl_Warning`, `cbo_GenType`, `cbo_AesBits`, `nud_RandomLength`, `txt_GuidPrefix`...). Field `_camelCase` private hiện có (như `_key`, `_isEditMode`, `_isCoreEdit`) giữ cho dữ liệu, không cho control.
- Sự kiện nối trong Designer (`this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click)`); handler đổi tên `<control>_<Event>`.
- Property `public` (nếu còn, ví dụ `SelectedEntry`) giữ `[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]`; không thêm property public mới.
- Không `Console.*`/`Log.*`/`Debug.*`.

## 7. Luồng xử lý dự kiến
1. Với mỗi form: đọc `.cs` hiện tại, ghi lại **toàn bộ** thuộc tính từng control (Text, Location, Size, Font, Color, Anchor/Dock, TabIndex, Cursor, DialogResult, ReadOnly, Items của ComboBox...) và thuộc tính form (`ClientSize`, `FormBorderStyle`, `StartPosition`, `AcceptButton`, `CancelButton`, `MaximizeBox`, `MinimizeBox`, `ShowInTaskbar`, font, `BackColor`).
2. Chuyển sang `InitializeComponent()` với **đúng giá trị đó**; trong `.cs` chỉ còn logic và phần gán dữ liệu/ẩn-hiện theo chế độ sau `InitializeComponent()`.
3. Đổi tham chiếu control trong logic sang tên mới; giữ nguyên thứ tự xử lý sự kiện.
4. `FrmCustomEnvEditor`: cắt class khỏi `UC_ConfigEnvCustom.cs`, dán sang `FrmCustomEnvEditor.cs` rồi chuyển dựng control sang Designer; `UC_ConfigEnvCustom.cs` chỉ mất đoạn class.
5. Build sau mỗi form, đối chiếu từng giá trị với bản cũ (`git diff`), không đổi hành vi.

## 8. Tiêu chí nghiệm thu
| # | Tiêu chí (kiểm tra được) | Loại test |
|---|---|---|
| AC1 | `dotnet build BaseRepo.slnx -m:1`: 0 Error, không `WFO1000` | Build |
| AC2 | Trong 4 file `.cs` của form không còn `new Button`/`new Label`/`new TextBox`/`new ListBox`/`new Panel`/`new ComboBox`/`new NumericUpDown` để dựng bố cục (grep rỗng); mỗi form có `.Designer.cs` với `InitializeComponent` | Review/grep |
| AC3 | Chữ ký constructor và property `SelectedEntry` giữ nguyên; `UC_ConfigEnvCustom.cs` **chỉ mất** class `FrmCustomEnvEditor` (diff chỉ có dòng xóa); `UC_ConfigEnvCustom.Designer.cs` không đổi | Review |
| AC4 | Mỗi giá trị bố cục (ClientSize, Location, Size, Font, màu, Text, TabIndex, Accept/CancelButton, DialogResult, Items) trong Designer khớp bản code cũ: Reviewer đối chiếu theo bảng so sánh | Review |
| AC5 | Logic `FrmCustomEnvEditor` (sinh AES/GUID/ngẫu nhiên/RSA, validate, lưu Thêm/Sửa/Sửa cốt lõi) khớp bản cũ từng nhánh (đối chiếu diff, không tự sửa logic) | Review |
| AC6 | Mở từng form trong Visual Studio Designer thấy đủ control, không lỗi | Thủ công |
| AC7 | Chạy lại luồng: Thêm biến, Sửa biến thường, Sửa biến cốt lõi (hộp gõ lại tên key), Khôi phục (picker), Sửa `Path` (trình sửa danh sách): giao diện và hành vi như trước | Thủ công |
| AC8 | `git diff` chỉ gồm các file ở mục 3 (+ 3 `.resx` có sẵn thêm vào, không sửa nội dung); không log giá trị | Build/Review |

## 9. Lệnh kiểm tra
- Build: `dotnet build BaseRepo.slnx -m:1`.
- Tester/Reviewer: không có test tự động cho UI; đối chiếu theo mục 7 và ghi rõ AC6, AC7 là "chưa chạy, cần thủ công".
