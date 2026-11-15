# N5 — ConfigEnv: chuyển 4 form dựng bằng code sang Visual Studio Designer · Phân tích yêu cầu

- **Người viết:** BA (skill `ba-requirements`) · **Ngày:** 2026-11-16 · **Module:** 6 (SystemManagement)
- **Trạng thái:** ✅ đã chốt bởi Minh Thuận ngày 2026-11-16 (phạm vi mở rộng thành 4 form theo yêu cầu "làm N5 trước để ổn định cấu trúc")

## 1. Bối cảnh và mục tiêu
Bốn form của ConfigEnv dựng toàn bộ control bằng code, không mở được trong Designer, không giống `UC_ConfigEnvCustom` (có `.Designer.cs`): `FrmEnvTypeKeyConfirm`, `FrmEnvRestorePicker`, `FrmEnvListEditor` (do N2-1/N2-2 tạo) và `FrmCustomEnvEditor` (của N1, đang là class nằm cuối `UC_ConfigEnvCustom.cs`). Người dùng muốn chỉnh layout bằng Designer, cấu trúc nhất quán và ổn định **trước** khi làm N4 (N4-2 sửa các form này). Mục tiêu: chuyển 4 form sang Designer **không đổi hành vi**, tách `FrmCustomEnvEditor` ra file riêng, và đặt quy tắc "form mới dùng Designer".

## 2. Yêu cầu gốc
> "kết hợp 1 và 3" — (1) chuyển form sang Designer, làm trước N4-2; (3) mọi form mới (kể cả N3) bắt buộc dùng Designer.
> "có thể làm N5 trước để ổn định cấu trúc không" — gộp `FrmCustomEnvEditor` vào N5, làm N5 trước N4.

## 3. Hiện trạng
- `FrmEnvTypeKeyConfirm.cs` (3.7K), `FrmEnvRestorePicker.cs` (6.6K), `FrmEnvListEditor.cs` (16K): constructor tạo `Label/TextBox/Button/ListBox/Panel...` bằng `new` và gán `Location/Size/Font/Color`; field `readonly` kiểu `_camelCase`.
- `FrmCustomEnvEditor`: `internal class` cuối `UC_ConfigEnvCustom.cs` (~410 dòng, dòng 326-735), dựng control bằng code (ô key, ô value, ô sinh giá trị AES/GUID/chuỗi ngẫu nhiên/RSA, nút Lưu/Hủy). Constructor `(bool isEditMode, string key, string currentValue, bool isCoreEdit = false)`; chỉ được `UC_ConfigEnvCustom.cs` dùng.
- Visual Studio đã tự tạo 3 file `.resx` (chưa theo dõi bởi git) cho 3 form đầu khi mở Designer.

## 4. Luồng nghiệp vụ
Không đổi. Người dùng thấy đúng giao diện và hành vi như hiện tại, chỉ khác cách dựng.

## 5. Quy tắc nghiệp vụ
- R1: Mỗi form gồm `X.cs` + `X.Designer.cs` + `X.resx`, `partial class`, `InitializeComponent()`; mở được trong Designer.
- R2: **Giữ nguyên** chữ ký constructor công khai, property `SelectedEntry`, `DialogResult`, `AcceptButton/CancelButton`, kích thước, vị trí, font, màu, văn bản, thứ tự Tab, hành vi mọi nút.
- R3: Đổi tên control theo quy tắc `convention-winforms.md` (`btn_`, `txt_`, `lbl_`, `lst_`, `pnl_`, `cbo_`, `nud_`...). Không còn `new <Control>` để dựng bố cục trong file `.cs`.
- R4: `FrmCustomEnvEditor` **tách ra file riêng** `FrmCustomEnvEditor.cs` (cùng namespace `SystemManagement.Views.ConfigEnv`, vẫn `internal`) và bị xóa khỏi `UC_ConfigEnvCustom.cs`; `UC_ConfigEnvCustom.cs` không đổi gì khác.
- R5: Quy tắc WinForms mới ở `.claude/rules/convention-winforms.md` (đã tạo 2026-11-16); sau N5 không còn form code-only nào trong ConfigEnv.

## 6. Trường hợp biên và lỗi
| Tình huống | Hành vi mong đợi |
|---|---|
| Designer sinh lại code làm lệch layout | Layout phải giống bản code cũ (đối chiếu số liệu `Location/Size`) |
| Control chỉ hiện theo chế độ (vd `isCoreEdit` ẩn nhóm sinh giá trị; `isEditMode` khóa ô key) | Giữ nguyên logic ẩn/hiện/khóa trong `.cs` sau `InitializeComponent()` |
| Property public trên form | Gắn `[DesignerSerializationVisibility(Hidden)]` để tránh `WFO1000` |
| `.resx` do Visual Studio tạo sẵn | Giữ nguyên, đưa vào commit, không xóa |
| `.resx` của `FrmCustomEnvEditor` chưa có | Tạo `.resx` tối thiểu cùng định dạng 3 `.resx` đã có |

## 7. Dữ liệu và bảo mật
Không đổi. Không thêm log; không chạm registry; không in giá trị biến vào thông báo.

## 8. Phụ thuộc
- Sau N2-1/N2-2 (đã ✅). **Làm trước N4-2** (N4-2 sửa `FrmEnvListEditor`, `FrmEnvRestorePicker`, `FrmCustomEnvEditor`).

## 9. Câu hỏi còn mở
- (trống)

## 10. Ngoài phạm vi
- `UC_ConfigEnv*`, `FrmMain`, các form khác; đổi giao diện hay hành vi; thêm package; test project.
