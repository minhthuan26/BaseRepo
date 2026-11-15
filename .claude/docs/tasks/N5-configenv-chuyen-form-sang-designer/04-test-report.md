# N5 — ConfigEnv: chuyển 4 form sang Designer · Test Report

- **Ngày:** 2026-11-16 · **Vòng:** 1
- Repo không có test project UI. Kiểm chứng bằng harness console tạm đặt ngoài repo (scratchpad). Harness không `Application.Run`, không `ShowDialog`, không gọi hàm chạm registry/env, không gọi handler Save/Generate. Không sửa code trong repo.

## 1. Đối chiếu tiêu chí nghiệm thu
| Tiêu chí | Cách kiểm | Kết quả |
|---|---|---|
| AC1 build | `dotnet build BaseRepo.slnx -m:1` | ✅ 0 Error; lọc theo `error`/`WFO1000` thì không có dòng nào (Warning 713, lần build tăng dần; Warning nền toàn là loại có sẵn) |
| AC2 không còn dựng control bằng code, có Designer | grep `new (Button\|Label\|TextBox\|ListBox\|Panel\|ComboBox\|NumericUpDown\|DataGridView\|GroupBox\|CheckBox)` trong 4 file `.cs` của form = rỗng; mỗi form có `.Designer.cs` với `InitializeComponent` | ✅ |
| AC3 chữ ký + `UC_ConfigEnvCustom.cs` | Constructor và `SelectedEntry` (kèm `DesignerSerializationVisibility.Hidden`) giữ nguyên so với `HEAD`; `git diff --numstat UC_ConfigEnvCustom.cs` = 0 thêm / 411 xóa, 1 hunk `@@ -325,411 +324,0 @@`, không có dòng `+`; `UC_ConfigEnvCustom.Designer.cs` không đổi | ✅ |
| AC4 bố cục Designer khớp bản cũ | Harness so sánh cây control cũ/mới (mục 3) | ✅ (kèm 4 khác biệt vô hại/cần lưu ý ở mục 4) |
| AC5 logic `FrmCustomEnvEditor` khớp từng nhánh | `diff` phần method sau constructor của bản cũ (đã đổi tên field/handler) và bản mới: giống hệt, chỉ thêm `btn_Cancel_Click` thay cho lambda cùng thân (`DialogResult = Cancel; Close();`). Logic ctor (`isEditMode`, `isCoreEdit`, `ReadOnly`, `BackColor`, ẩn nhóm sinh giá trị, tọa độ nút, tiêu đề, `ClientSize`) khớp trong harness ở cả 4 chế độ | ✅ |
| AC6 mở Designer trong Visual Studio | Cần người dùng | **Chưa chạy** |
| AC7 chạy lại luồng trên UI | Cần người dùng | **Chưa chạy** |
| AC8 `git diff` chỉ gồm file ở mục 3 | `git status`: 4 file sửa (3 form + `UC_ConfigEnvCustom.cs`), 6 file mới (3 Designer + `FrmCustomEnvEditor.cs/.Designer.cs/.resx`), không `.csproj`, 3 `.resx` có sẵn không bị sửa; grep `Console.`/`Debug.`/`Trace.`/`Log.` trong 4 form + Designer = rỗng | ✅ |

## 2. Kết quả chạy
- Build: `dotnet build BaseRepo.slnx -m:1` → Build succeeded, 0 Error.
- Harness: biên dịch bản cũ (lấy từ `git show HEAD:...`, riêng `FrmCustomEnvEditor` cắt từ dòng 326 của `UC_ConfigEnvCustom.cs`, hậu tố `Old`) cùng bản mới (compile trực tiếp file `.cs` + `.Designer.cs` hiện tại, kèm `EnvBackupService/EnvRegistryService/CustomEnvService/EnvListService/EnvValueGenerator`, `SystemConfig`, và stub `FrmMain` ném exception nếu bị gọi: không handler nào bị gọi). Chạy trên luồng STA, `HighDpiMode.PerMonitorV2` như `Program.cs`.
- 12 cặp so sánh: TypeKeyConfirm, RestorePicker (không có file backup, lưới rỗng), ListEditor (rỗng và 3 mục), CustomEnv (Thêm, Sửa, Sửa cốt lõi, Thêm+core (false,true)), CustomEnv với `cbo_GenType` = 0/1/2/3.
- Mỗi cặp so 2 giai đoạn: (1) sau constructor; (2) sau khi buộc tạo handle cho toàn bộ control + `PerformLayout` đệ quy từ lá lên.
- So sánh qua reflection toàn bộ property công khai kiểu đơn giản của từng control (gồm Text, Location, Size, Font, BackColor, ForeColor, Anchor, Dock, TabIndex, TabStop, Visible, Enabled, ReadOnly, Cursor, Multiline, ScrollBars, DialogResult, Padding, Margin...), Items/SelectedIndex của ComboBox/ListBox, Min/Max/Value/Increment của NumericUpDown, cột DataGridView (loại cột, HeaderText, DataPropertyName, Width, AutoSizeMode, ReadOnly...), số handler sự kiện gắn trên mỗi control, và property của Form (ClientSize, FormBorderStyle, StartPosition, MaximizeBox, MinimizeBox, ShowInTaskbar, Text, Font, BackColor), cùng chỉ số control của AcceptButton/CancelButton và DialogResult các nút.
- Ghép control cũ/mới theo (loại, Text, Location, cha) vì tên control đổi và thứ tự `Controls` khác nhau.
- Kết quả giai đoạn 2 (trạng thái thực khi hiển thị): mọi control đều khớp, số handler bằng nhau, ghép đủ 100% control (6 / 7 / 15 / 18 control tương ứng 4 form), AcceptButton/CancelButton trỏ đúng cùng nút. Khác biệt duy nhất còn lại là 4 mục ở mục 4.
- Không có lệnh `dotnet test` (không có test project); không phát sinh test mới trong repo.

## 3. Test mới đã thêm
Không thêm vào repo (spec cấm test project). Harness tạm ở scratchpad (xem mục 6 về dọn dẹp).

## 4. Khác biệt tìm thấy (không có khác biệt bố cục ở 100% DPI)
| # | Khác biệt | Mức | Ghi chú |
|---|---|---|---|
| D1 | Form: `AutoScaleMode` cũ `Inherit` (không scale) → mới `Font` với `AutoScaleDimensions` 7x15 (mẫu chuẩn của VS và của `FrmMain.Designer.cs`) | Không vô hại hoàn toàn ở DPI > 100%, vô hại ở 100% | Ở 100% DPI (máy chạy harness, `CurrentAutoScaleDimensions` = 7x15) mọi kích thước/vị trí khớp. Ở 125%/150% (app dùng PerMonitorV2) form mới sẽ scale control và `ClientSize`, bản cũ thì không scale. Không kiểm được trong harness: ghi vào danh sách thủ công. Riêng `FrmCustomEnvEditor` chế độ core: `ClientSize` 540x200 và vị trí nút (320/420,145) gán sau `InitializeComponent()`; cần xem ở DPI cao có lệch so với phần còn lại không |
| D2 | `lbl_RsaNote.Text`: bản cũ dùng `\n`, Designer dùng `\r\n` giữa 2 dòng | Vô hại | Cả hai đều hiển thị 1 lần xuống dòng. Chuỗi khác 1 ký tự `\r`, không có code đọc chuỗi này |
| D3 | `ComboBox.FormattingEnabled`: cũ `False` → mới `True` (`cbo_GenType`, `cbo_AesBits`) | Vô hại | Mặc định Designer của VS; chỉ ảnh hưởng định dạng khi gắn DataSource, ở đây Items cố định |
| D4 | Thứ tự `Controls` (z-order) của `FrmCustomEnvEditor` và nhóm `grp_Gen` đảo ngược so với bản cũ (Designer ghi theo thứ tự ngược) | Vô hại | `TabIndex` của mọi control khớp bản cũ (đã so ở giai đoạn 2), không control nào chồng lên nhau, không Dock nào phụ thuộc thứ tự |
| D5 | `lst_Items.ItemHeight = 15` được ghi tường minh trong Designer (cũ không gán) | Vô hại | Chiều cao dòng native của ListBox đo được là 15 cả hai bên (thử trực tiếp bằng `LB_GETITEMHEIGHT`); chênh 16/15 trong giai đoạn 1 chỉ là giá trị ước lượng trước khi có handle |
| D6 | `btn_Cancel` của `FrmEnvListEditor`/`FrmCustomEnvEditor`: lambda cũ thành method `btn_Cancel_Click` | Vô hại | Cùng thân hàm; số handler bằng nhau |
| D7 | Handler `cbo_GenType_SelectedIndexChanged` nay đã gắn từ Designer nên chạy khi `SelectedIndex = 0` trong constructor (bản cũ gắn sau khi gán) | Vô hại | Gọi `UpdateGenControlsVisibility()` idempotent; trạng thái Visible/Text sau constructor khớp ở cả 4 chế độ |

Các chênh lệch giai đoạn 1 khác (Location/Bounds form, ClientSize `pnl_Warning`/`lbl_Warning` 680x80 so 678x78, ItemHeight/PreferredHeight) đã được kiểm là artifact của harness: vị trí cửa sổ chưa hiển thị do Windows cascade 26px mỗi cửa sổ tạo; kích thước client trước khi có handle chưa trừ viền. Sang giai đoạn 2 không còn chênh lệch.

## 5. Phần chưa chạy / giới hạn
- **AC6** (mở Designer trong Visual Studio, thấy đủ control, không lỗi): chưa chạy, cần người dùng.
- **AC7** (chạy lại luồng Thêm biến, Sửa thường, Sửa cốt lõi, Khôi phục, Sửa `Path`): chưa chạy, cần người dùng. Handler Save/Generate/Khôi phục không được gọi trong harness (chạm registry/đĩa).
- Chưa kiểm hiển thị ở DPI 125%/150% (D1) và với bản sao lưu thật trong lưới của `FrmEnvRestorePicker` (harness chỉ có lưới rỗng; cột đã so sánh khớp).
- Không so được vẽ pixel (chỉ so thuộc tính), không kiểm thao tác bàn phím/Tab thực tế (chỉ `TabIndex`).

## 6. Dọn dẹp
- Harness và thí nghiệm nằm ngoài repo, trong `C:\Users\X1503ZA-L1418W\AppData\Local\Temp\claude\81b2bab8ed1c5b58\54f0e1d4-3bef-445c-ab54-f2537d0b86da\scratchpad\h` và `...\scratchpad\x`.
- **Chưa dọn được:** lệnh `rm -rf` hai thư mục này bị hook `Fact-Forcing Gate` chặn hai lần (đã nêu danh sách file, cách rollback và trích lời yêu cầu theo đúng đề nghị của hook, vẫn bị chặn). Hai thư mục vẫn còn trong scratchpad của phiên; không nằm trong repo, không ảnh hưởng `git status`. Cần người dùng xóa tay nếu muốn.
- Repo không bị thay đổi bởi harness (`git status` chỉ còn file của N5 và `05-review.md`, `bash.exe.stackdump` có sẵn).

## 7. Lỗi (nếu có)
Không có lỗi bố cục hay hành vi không vô hại ở 100% DPI. Mục D1 (DPI cao) cần xác nhận thủ công khi làm AC7.

VERDICT: PASS
