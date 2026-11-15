# Convention WinForms (SystemManagement, `net10.0-windows`)

> Áp dụng cho mọi `Form`/`UserControl` trong `SystemManagement/`. Quy tắc do người dùng chốt ngày 2026-11-16.

## Dựng giao diện bằng Designer (bắt buộc cho form/control mới)
- Mỗi form/UserControl gồm **3 file**: `X.cs` (logic), `X.Designer.cs` (`InitializeComponent`, khai báo control), `X.resx`. Class là `partial`, gọi `InitializeComponent()` trong constructor.
- **Không** dựng bố cục bằng code (`new Button`, `new Label`, `Location = ...`, `Size = ...`) trong file `.cs`. Chỉ tạo control bằng code khi nội dung **động** (số lượng control không biết trước).
- Form phải mở được trong Visual Studio Designer. `.resx` được commit cùng form.
- Form cũ dựng bằng code (ví dụ `FrmCustomEnvEditor` trong `UC_ConfigEnvCustom.cs`) để nguyên cho tới khi có task chuyển; không viết thêm form mới theo kiểu đó.

## Đặt tên control
Tiền tố + `_` + tên, như `UC_ConfigEnvCustom.Designer.cs`: `btn_Save`, `txt_Search`, `lbl_Title`, `dgv_Envs`, `lst_Items`, `cbo_Type`, `pnl_Warning`, cột `col_Key`. Handler: `<control>_<Event>` (`btn_Save_Click`).

## Chung
- Nối sự kiện trong Designer, không `+=` trong constructor trừ khi control tạo động.
- Property `public` trên form/UserControl phải gắn `[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]` (analyzer `WFO1000` là lỗi build trên .NET 10).
- Lỗi/cảnh báo hiển thị qua `FrmMain.ThrowError/ThrowWarning/ThrowInfo/ThrowQuestion`; không log giá trị nhạy cảm.
- Chuỗi giao diện tiếng Việt; tên biến/hàm tiếng Anh.

## Thao tác ghi/IO không được làm đứng UI
- Ghi registry, ghi file, gọi dịch vụ có thể chậm (broadcast `WM_SETTINGCHANGE` chờ tới 5 giây) phải chạy qua `await FrmMain.RunWithLoadingAsync(() => Task.Run(() => ...))`. `RunWithLoadingAsync` chỉ hiện loader và **chạy action trên luồng UI**, nên việc nặng phải tự bọc `Task.Run`; không đổi hàm đó vì nơi khác dùng nó với action chạm control UI.
- Đọc giá trị control ở luồng UI **trước** `Task.Run`; không truy cập control bên trong lambda nền.
- Khi đang lưu: khóa nút và ô nhập, bật `UseWaitCursor`, chặn đóng form, cờ `_isSaving` chống bấm lặp, trả về trạng thái cũ trong `finally`.
