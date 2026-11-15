# N5 — ConfigEnv: chuyển 4 form sang Designer · Code Review

- **Reviewer:** Claude (tự review, chế độ B) · **Ngày:** 2026-11-16 · **Vòng:** 1

## 1. Phát hiện
Không có blocker hay major. Phát hiện còn lại đều là nit/minor, không đổi hành vi nghiệp vụ.

| Mức độ | File:dòng | Vấn đề | Gợi ý sửa |
|---|---|---|---|
| minor | `FrmEnvTypeKeyConfirm/FrmEnvRestorePicker/FrmEnvListEditor/FrmCustomEnvEditor.Designer.cs` (khối cuối `InitializeComponent`) | Designer thêm `AutoScaleDimensions=(7,15)` + `AutoScaleMode=Font`. Code cũ không đặt (Form mặc định `Inherit`, không scale). `Program.cs` dùng PerMonitorV2: ở 100% DPI giống hệt; ở DPI cao form mới sẽ scale, bản cũ thì không. Cùng mẫu với `UC_ConfigEnvCustom.Designer.cs:228-229`. | Chấp nhận (chuẩn Designer, hợp lý hơn). Ghi nhận là lệch có chủ đích; kiểm bằng AC6/AC7 ở màn hình DPI cao. |
| nit | `FrmCustomEnvEditor.Designer.cs` (`lbl_RsaNote.Text`) | Chuỗi dùng `\r\n`, bản cũ dùng `\n`. Label hiển thị như nhau. | Không cần sửa. |
| nit | `FrmCustomEnvEditor.Designer.cs` (`btn_Cancel`) | Thêm `DialogResult = Cancel` trên nút (cũ chỉ gán trong handler). Kết quả cuối như nhau (Cancel + Close). Ở 3 form còn lại cũ đã có sẵn. | Không cần sửa. |
| nit | `FrmCustomEnvEditor.cs` (using) | Còn `using System.Collections.Generic` (dùng cho `KeyNotFoundException`, cần), nhưng không thừa đáng kể; không có `using` nào sai. Handler Cancel đặt tên thành `btn_Cancel_Click` thay vì lambda (đúng convention). | Không cần sửa. |
| nit | `FrmCustomEnvEditor.resx` | Khác `FrmEnvListEditor.resx` đúng một điểm: có BOM đầu file. Nội dung tương đương. | Không cần sửa. |
| nit | `FrmCustomEnvEditor.Designer.cs` (`Controls.Add`) | Thứ tự `Controls.Add` ngược bản cũ (z-order đảo). Không có control chồng lên nhau, TabIndex khớp thứ tự cũ, nên không ảnh hưởng. | Không cần sửa. |
| nit | `FrmCustomEnvEditor.Designer.cs` | `FormattingEnabled = true` trên 2 ComboBox (mặc định Designer VS, không đổi hành vi). | Không cần sửa. |

## 2. Đánh giá theo tiêu chí

### 2.1 Hành vi (đối chiếu bản cũ ở HEAD)
- `FrmCustomEnvEditor`: phần thân từ `CboGenType_SelectedIndexChanged` đến hết `BtnSave_Click` (AES/GUID/ngẫu nhiên/RSA, `GenerateRsaKeyPair`, validate, nhánh core/edit/add, `SecurityException`/`KeyNotFoundException`/`DialogResult.Abort`, thông báo) được kiểm bằng `diff` sau khi chuẩn hóa tên control: **giống từng dòng**, chỉ thêm `btn_Cancel_Click` thay cho lambda cũ (cùng nội dung `DialogResult=Cancel; Close();`).
- `FrmEnvListEditor`: toàn bộ logic từ `LoadItems` đến `BtnSave_Click` giống từng dòng sau chuẩn hóa tên; lambda hủy thành `btn_Cancel_Click`.
- `FrmEnvRestorePicker`, `FrmEnvTypeKeyConfirm`: logic giữ nguyên (xem `git diff`); `SelectedEntry` còn `[DesignerSerializationVisibility(Hidden)]`, chữ ký constructor không đổi.
- Thứ tự khởi tạo FrmCustomEnvEditor: cũ = set Text/ClientSize/control rồi `SelectedIndex=0`, hook sự kiện, `UpdateGenControlsVisibility()`. Mới = `InitializeComponent()` (sự kiện đã nối trong Designer) rồi Text, nhánh core (ClientSize 540x200, `grp_Gen.Visible=false`, nút Lưu/Hủy ở y=145), key/value, `SelectedIndex=0`, `UpdateGenControlsVisibility()`. Khác biệt duy nhất: `cbo_GenType.SelectedIndex = 0` giờ kích hoạt handler một lần (cũ hook sau nên không kích). Handler chỉ gọi `UpdateGenControlsVisibility()` (idempotent, ngay sau đó cũng gọi lại) nên kết quả như nhau.
- Ẩn/hiện/khóa theo `isEditMode`/`isCoreEdit`: `ReadOnly`, `BackColor` (`SystemColors.Control`/trắng), ẩn group, ClientSize, vị trí nút giữ đúng.
- Các text động (Text form, `lbl_Message`, `lbl_Prompt`, `lbl_Header`, `lbl_Title`) vẫn gán trong constructor sau `InitializeComponent()`; không logic nào bị mất.

### 2.2 Bảng đối chiếu layout (cũ -> Designer): tất cả KHỚP trừ mục lệch đã nêu ở mục 1
| Form | Form-level | Control |
|---|---|---|
| FrmEnvTypeKeyConfirm | ClientSize 520x270, FixedDialog, Max/Min=false, CenterParent, BackColor White, Font Segoe UI 9, Accept=btn_Ok, Cancel=btn_Cancel | lbl_Message (20,15) 480x110 DarkRed Regular; lbl_Prompt (20,135) 480x20 Bold; txt_Input (20,160) 480x23; btn_Ok (295,210) 100x34 Coral/White Bold Hand Enabled=false; btn_Cancel (405,210) 95x34 WhiteSmoke Hand DialogResult=Cancel; TabIndex 0-4 theo thứ tự add cũ |
| FrmEnvRestorePicker | ClientSize 760x430, như trên, Accept=btn_Select, Cancel=btn_Cancel | lbl_Header (15,15) AutoSize Bold; dgv_Backups (15,42) 730x320, AllowUser*=false, AutoGenerateColumns=false, BackgroundColor White, header AutoSize, MultiSelect=false, ReadOnly, RowHeadersVisible=false, FullRowSelect; 4 cột (140/120/110/Fill, ReadOnly, DataPropertyName cùng giá trị `nameof`); btn_Select (535,380) 100x34 LimeGreen Bold Enabled=false; btn_Cancel (645,380) 100x34 |
| FrmEnvListEditor | ClientSize 720x500, FixedDialog, CenterParent, White, Font 9, Cancel=btn_Cancel (không AcceptButton, như cũ) | lbl_Title (20,15) 680x24 Bold 10; lbl_Desc (20,40) 680x20 DimGray; lst_Items (20,68) 570x240 IntegralHeight=false, HScroll=true; btn_Up/Down/Delete (600, 68/108/148) 100x32 WhiteSmoke (Delete DarkRed); lbl_Input (20,322) 75x23 Bold; txt_Input (100,320) 400x23; btn_Add (510,318) 90x27 LightSkyBlue; btn_Update (610,318) 90x27 LightYellow; pnl_Warning (20,355) 680x80 FixedSingle (240,253,244); lbl_Warning Fill, 8.5pt, DarkGreen, Padding 6, TopLeft; btn_Save (500,450) 95x34 LimeGreen Bold; btn_Cancel (605,450) 95x34 DialogResult=Cancel; TabIndex 0-12 |
| FrmCustomEnvEditor | ClientSize 540x460 (core chỉnh trong code 540x200), FixedDialog, CenterParent, White, Font 9, Cancel=btn_Cancel | lbl_Key (20,20) Bold; txt_Key (130,17) 380x23; lbl_Value (20,55) Bold; txt_Value (130,52) 380x75 Multiline VScroll; grp_Gen (20,140) 490x240; lbl_GenType (15,30); cbo_GenType (110,27) 180x23 DropDownList, 4 Items đúng chữ; lbl_Param (15,68); cbo_AesBits (110,65) 120x23 Items 512/256/128; txt_GuidPrefix (110,65) 220x23 "urn:api:"; nud_RandomLength (110,65) 120x23 Min 8 Max 256 Value 32; lbl_RsaNote (15,65) 460x60; btn_Generate (110,145) 120x32 Gold Bold; btn_Save (320,400) 90x35 LimeGreen Bold; btn_Cancel (420,400) 90x35 WhiteSmoke; TabIndex trong/ngoài group khớp thứ tự add cũ |

### 2.3 Convention
- `partial class`, `components`/`Dispose` đúng mẫu `UC_ConfigEnvCustom.Designer.cs`; tên `prefix_Name`; sự kiện nối trong Designer; handler `<control>_<Event>`; `SelectedEntry` giữ attribute Hidden; không property public mới.
- `InitializeComponent` chỉ dùng literal/khởi tạo control, không truy cập biến ngoài hay gọi hàm logic; `BeginInit/EndInit` cho `DataGridView`/`NumericUpDown`, `SuspendLayout/ResumeLayout` đúng cặp.
- Không `new Button|Label|...` còn trong 4 file `.cs`; 3 form có `.resx` đã có, `FrmCustomEnvEditor.resx` mới.
- Không xác nhận được bằng đọc code: Visual Studio mở Designer (AC6), vì `FrmCustomEnvEditor`/`FrmEnvListEditor`/... không có constructor không tham số. Đây là điểm cần người dùng thử.

### 2.4 Phạm vi
- `UC_ConfigEnvCustom.cs`: `git diff --numstat` = 0 thêm / 411 xóa, hunk chỉ là class `FrmCustomEnvEditor`. Không sửa `.csproj`, `UC_ConfigEnvCustom.Designer.cs`, và 3 `.resx` cũ (`git diff --stat -- '*.resx'` rỗng). File untracked ngoài spec: `bash.exe.stackdump` (đã có từ trước, không thuộc task, không nên commit).

### 2.5 An toàn
Không `Console./Debug./Trace./Log.`, không registry mới. Thông báo lỗi nguyên bản (key trong `ThrowInfo`, `ex.Message` từ service) không đổi; không log giá trị.

## 2b. Phạm vi ảnh hưởng
Không dùng CodeGraph. Nơi gọi `new FrmEnvTypeKeyConfirm/FrmEnvRestorePicker/FrmEnvListEditor/FrmCustomEnvEditor(...)` nằm ở `UC_ConfigEnvCustom.cs`, chữ ký không đổi, cùng namespace nên không vỡ. Build do coder/Claude xác nhận 0 Error (log 03); reviewer không chạy lại build.

## 3. Đề xuất sau (minor, không chặn)
- Người dùng chạy AC6 (mở 4 form trong Designer) và AC7 (Thêm, Sửa, Sửa cốt lõi, Khôi phục, Sửa Path), ưu tiên thử ở DPI > 100% để xác nhận độ lệch do `AutoScaleMode=Font` là chấp nhận được.
- Không commit `bash.exe.stackdump`.

VERDICT: APPROVE
