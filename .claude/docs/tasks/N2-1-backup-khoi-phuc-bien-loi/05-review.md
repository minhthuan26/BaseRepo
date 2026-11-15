# N2-1 — ConfigEnv: mở khóa sửa biến cốt lõi + backup + khôi phục · Code Review

- **Reviewer:** Claude (chế độ B, tự review, độc lập) · **Ngày:** 2026-11-15 · **Vòng:** 2 (vòng cuối cho phép)
- **VERDICT: APPROVE** (0 blocker, 0 major; còn minor/nit không chặn, xem mục 3)

## 1. Vấn đề vòng 1 → đã xử lý chưa (đối chiếu code thật)

| Vòng 1 | Mức | Trạng thái | Bằng chứng (file:dòng) |
|---|---|---|---|
| `ReadRaw` nuốt exception, `RestoreCoreValue` bỏ backup rồi vẫn ghi đè | major | **Đã xử lý** | EnvRegistryService.cs:40-56: không còn try/catch; `OpenSubKey` null thì ném `InvalidOperationException` (46-47); chỉ trả `null` khi `GetValue` ra null (51-52); lỗi khác (Security/IO/UnauthorizedAccess) lan ra. CustomEnvService.cs:155-164: `current == null` mới bỏ backup, còn lỗi đọc thì exception dừng trước `WriteRaw` (167). Thứ tự: ReadRaw, Kind, CreateBackup (lỗi thì dừng), WriteRaw, Broadcast. |
| Validate dữ liệu backup khi khôi phục | minor | **Đã xử lý** | CustomEnvService.cs:134-138 (Key rỗng, Key có khoảng trắng đầu/cuối), 146-148 (`ValidateValue` không rỗng), 150-151 (cấm `\r`/`\n`). |
| `TryRead` cắt `Value=` một dòng | minor | **Đã xử lý** | EnvBackupService.cs:80-99: tìm `Value=` ở đầu file hoặc sau `\n`, lấy `Substring` đến hết file. |
| Kiểm Kind String/ExpandString trước backup/ghi | minor | **Đã xử lý** | CustomEnvService.cs:111-112 (Update) và 159-160 (Restore), đều đặt trước `CreateBackup`. |
| TabIndex trùng | minor | **Đã xử lý** | Designer diff: Restore 3, Refresh 4, Add 5, Edit 6, Delete 7 (ToggleMask giữ 2), không còn trùng. |
| `ParseExact` / nhánh culture hiện hành | nit | Đã xử lý | EnvBackupService.cs:141 chỉ còn `TryParseExact` + `InvariantCulture`. |
| Ghi đè giá trị đã đổi giữa chừng, backup thừa khi giá trị không đổi | minor | Chấp nhận, để lại | Có backup bảo vệ, khôi phục được. |

## 2. Rà lại vòng 2 (tìm lỗi mới do chính các sửa đổi)

- **(a) `ReadRaw` giờ ném lỗi.** Nơi gọi: `GetCoreRawValue` (CustomEnvService.cs:77) lan lên; `btn_Edit_Click` (UC_ConfigEnvCustom.cs) có `catch KeyNotFoundException` (tải lại) và `catch Exception` (báo lỗi, return) trước khi mở form nên UI không ở trạng thái dở. `UpdateCoreValue` (106) được `FrmCustomEnvEditor.BtnSave_Click` bọc catch Security/Unauthorized, KeyNotFound (Abort + đóng), `Exception` chung (báo lỗi, form giữ nguyên để thử lại). `RestoreCoreValue` (155) được `btn_Restore_Click` bọc catch chung. Không có nơi gọi nào không được bắt. Kết luận: không crash, không để UI dở.
- **(b) `TryRead`.**
  - Marker `\nValue=`: `CreateBackup` chỉ ghi `Reason` là hằng nội bộ (`Edit`/`BeforeRestore`) và `Key` là một trong 12 biến cốt lõi nên header không thể chứa marker; chỉ file sửa tay mới đánh lừa được, và hậu quả bị chặn bởi `ListBackups` lọc theo Key, kiểm Kind, và validate lúc khôi phục.
  - CRLF: `AppendLine` ghi `\r\n`, marker `\nValue=` vẫn khớp, `headerPart` giữ `\r\n` và `StringReader.ReadLine` xử lý đúng. Đã có trong test 54/54.
  - BOM: `ReadAllText(UTF8)` bỏ BOM, nên `Key=` ở đầu file khớp đúng. Không vấn đề.
  - `Value=` rỗng: `TryRead` trả entry có `Value == ""`, `ListBackups` vẫn liệt kê nhưng `RestoreCoreValue` từ chối bằng `ValidateValue` (thông báo rõ). Chấp nhận được (nit: có thể coi là hỏng từ `TryRead` để khỏi liệt kê).
- **(c) Newline cuối do sửa tay.** File do `CreateBackup` tạo dùng `sb.Append` (không newline cuối) nên bình thường luôn khôi phục được. File sửa tay (editor tự thêm newline) sẽ bị từ chối với thông báo nói rõ "không được chứa ký tự xuống dòng". Đây là hướng fail-an-toàn (không ghi biến cốt lõi bằng giá trị có xuống dòng), không mất dữ liệu; mức **nit**, chấp nhận. Có thể cân nhắc trim đúng một newline cuối ở `TryRead` về sau nhưng không bắt buộc.
- **(d) `OpenSubKey` null.** Khóa `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment` luôn tồn tại trên Windows; chỉ null khi bị xóa hoặc quyền đọc lạ. Không xảy ra trong tình huống bình thường, và nếu xảy ra thì dừng an toàn, báo lỗi. Đúng ý đồ.
- **(e) Thứ tự bước spec mục 7.** Kiểm Kind nằm giữa ReadRaw và CreateBackup là một guard bổ sung trước bước ghi, không đảo thứ tự các bước có tác dụng phụ (backup rồi ghi rồi broadcast) và chỉ làm hệ thống từ chối sớm hơn. Không lệch về hành vi. Validate giá trị trước ReadRaw vẫn như vòng 1.
- Phụ: `ValidateValue(null)` an toàn (`IsNullOrWhiteSpace`), không NRE ở `newValue.Contains`.

## 3. Vấn đề còn lại / mới (không chặn)

| Mức | File:dòng | Vấn đề | Hậu quả | Gợi ý |
|---|---|---|---|---|
| minor | CustomEnvService.cs:129-167 | `RestoreCoreValue` không tự kiểm `entry.Kind` (chỉ `TryRead` kiểm). Entry dựng tay với Kind khác sẽ làm `SetValue` ném lỗi hoặc ghi sai kiểu. Qua UI không tới được. | Chỉ ảnh hưởng nếu N2-2 gọi trực tiếp với entry dựng tay. | Thêm một dòng kiểm `entry.Kind` thuộc String/ExpandString khi sửa lần sau. |
| minor | UC_ConfigEnvCustom.cs (`btn_Edit_Click`, `btn_Restore_Click`, `BtnSave_Click`) | `RunWithLoadingAsync` chạy đồng bộ trên luồng UI, `SendMessageTimeout` có thể treo UI vài giây. Cùng pattern đã chấp nhận ở N1. | UI đứng tạm khi ghi. | Chấp nhận. Để lại, không chặn (xử lý chung nếu sau này sửa `RunWithLoadingAsync`, không riêng task này). |
| nit | EnvBackupService.cs:131-132 | `Value=` rỗng vẫn được liệt kê; sắp xếp `FilePath` giảm dần làm `_10` đứng trước `_2`; `btn_Restore_Click(object? sender...)` khác style handler khác. | Không đáng kể. | Để lại. |
| nit | EnvBackupService.cs:80-99 | Newline cuối của file backup sửa tay làm lệch Value (xem 2c). | Khôi phục bị từ chối an toàn. | Để lại. |

Các mục Claude cố ý không làm ở log mục 6 (bọc `Task.Run`, các nit): **đánh giá là chấp nhận được cho N2-1, để lại, không chặn.**

## 4. Phạm vi và convention
- `git status`: đúng 7 file của spec mục 3 (3 sửa, 4 mới); phần còn lại là docs, `redis_8003.conf` bị xóa và `bash.exe.stackdump` nằm ngoài phạm vi review, cần người dùng xử lý riêng (không đưa vào commit của task).
- Không package mới, không code chết đáng kể, grep `Console./Debug./Trace./TODO` trong thư mục ConfigEnv rỗng; không log giá trị biến. Một class một file cho 4 file mới (record `EnvRegistryValue`/`EnvBackupEntry` chung file với service, cùng kiểu `CustomEnvItem` của N1, chấp nhận). PascalCase, `_camelCase`, comment giải thích lý do.

<!-- TOOLING:BEGIN -->
## Phạm vi ảnh hưởng
Không dùng CodeGraph (WinForms, không có chỉ mục). Grep: `ReadRaw` chỉ được gọi ở `CustomEnvService` (3 chỗ), các chỗ gọi `GetCoreRawValue`/`UpdateCoreValue`/`RestoreCoreValue` chỉ nằm trong `UC_ConfigEnvCustom.cs` và đều có catch. Không file CommonLibs bị sửa.
<!-- TOOLING:END -->

## 5. Đã kiểm / chưa kiểm được
- **Đã kiểm bằng đọc:** nguyên văn `EnvRegistryService.cs`, `EnvBackupService.cs`, `CustomEnvService.cs`, diff `UC_ConfigEnvCustom.cs` và Designer, `git status`, log mục 6. Tin kết quả test vòng 2 (54/54) của tester, không chạy lại; không đọc lại `FrmEnvTypeKeyConfirm.cs`/`FrmEnvRestorePicker.cs` (không đổi ở vòng 2).
- **Chưa kiểm được (không tính là đạt):** chạy UI thật, ghi registry thật và broadcast (AC cần Administrator), AC1 phần ExpandString thật, `AppContext.BaseDirectory` khi `PublishSingleFile`, hiển thị Designer bằng mắt.

## 6. Đề xuất sau
- Handover: backup là plaintext cạnh exe (`Path` có thể lộ tên thư mục người dùng), nên đặt exe ở nơi chỉ Administrator ghi được; cần test tay trên máy thật có quyền Administrator trước khi đánh ✅ roadmap.
- Gộp vào N2-2 hoặc sau: kiểm `entry.Kind` trong `RestoreCoreValue`, `Task.Run` quanh thao tác ghi, so giá trị gốc khi lưu.

VERDICT: APPROVE

---

# Lịch sử — Vòng 1 (2026-11-15): REQUEST_CHANGES (1 major)

- **Reviewer:** Claude (chế độ B, tự review, độc lập) · **Ngày:** 2026-11-15 · **Vòng:** 1
- **VERDICT: REQUEST_CHANGES** (1 major)

## 1. Phát hiện

| Mức | File:dòng | Vấn đề | Hậu quả | Gợi ý sửa |
|---|---|---|---|---|
| **major** | EnvRegistryService.cs:59-63 + CustomEnvService.cs:138-145 | `ReadRaw` nuốt mọi exception và trả `null`, nên "lỗi đọc registry" không phân biệt được với "biến không tồn tại". Ở `RestoreCoreValue`, `current == null` thì bỏ qua backup `BeforeRestore` nhưng vẫn `WriteRaw` đè lên giá trị hiện có. Vi phạm R5/R9 ("mọi đường ghi phải có backup thành công trước"): giá trị hiện tại có thể bị ghi đè mà không có bản lưu, tức không còn đường lùi. Xác suất thấp (đang elevated, key HKLM đọc được) nhưng đây là đúng đường ghi duy nhất thiếu bảo đảm. Ở `UpdateCoreValue`/`GetCoreRawValue` chỉ báo nhầm "biến không còn tồn tại" (an toàn, nhưng thông báo sai nguyên nhân). | Tách hai trường hợp: để `ReadRaw` chỉ trả `null` khi `baseKey` hoặc value thực sự không có, còn lỗi khác (`SecurityException`, `IOException`, `UnauthorizedAccessException`) thì ném tiếp, hoặc đổi thành `TryReadRaw(key, out value)` kèm cờ `exists`. Ở `RestoreCoreValue`, chỉ cho bỏ qua backup khi chắc chắn "không tồn tại"; mọi lỗi đọc khác thì dừng, không ghi. UI đã có catch chung nên nhận được exception. |
| minor | CustomEnvService.cs:123-149, EnvBackupService.cs:78-125 | `RestoreCoreValue` ghi nguyên `entry.Value` từ file, không validate. `TryRead` chỉ lấy một dòng `Value=` (file có giá trị nhiều dòng bị cắt, file sửa tay hoặc do kẻ khác đặt vào `Backups\Env\` cạnh exe có thể mang giá trị rỗng/lạ) và chấp nhận `Value=` rỗng. Có thể ghi `Path`/`ComSpec` thành chuỗi rỗng hoặc bị cắt. Có bảo vệ phần nào: có backup `BeforeRestore`, picker hiện 80 ký tự đầu, phải gõ lại key. | Ghi giá trị hỏng/độc hại cho biến cốt lõi toàn máy; thư mục backup nằm cạnh exe nên nếu exe ở nơi user thường ghi được thì là đường "gài" bản backup. | Trong `RestoreCoreValue` áp cùng luật như `UpdateCoreValue`: `ValidateValue` (không rỗng) và không chứa `\r`/`\n`. Hoặc trong `TryRead` coi các bản như vậy là hỏng (bỏ khỏi `ListBackups`). Tốt hơn nữa: `File.ReadAllText` rồi lấy phần sau `Value=` đến hết file, để không cắt. Khuyến nghị sửa cùng vòng với major vì rẻ. |
| minor | EnvRegistryService.cs:56-57, CustomEnvService.cs:105-113 | Chỉ kiểm kiểu `ExpandString`/`String` ở `TryRead`, không kiểm ở đường ghi. Nếu biến hiện có kiểu khác (`MultiString`, `DWord`...), `rawObj.ToString()` ra chuỗi sai (ví dụ `System.String[]`), backup "thành công" với `Kind=MultiString` nhưng `TryRead` sẽ loại bản đó khi khôi phục, và `SetValue(string, MultiString)` ném lỗi. | Backup không dùng được lúc cần; 12 biến cốt lõi trên thực tế đều là `String`/`ExpandString` nên hiếm. | `UpdateCoreValue`/`RestoreCoreValue`: nếu `current.Kind` không thuộc `String`/`ExpandString` thì từ chối kèm thông báo rõ, không backup, không ghi. |
| minor | CustomEnvService.cs:92-96,131-145 | `IsProtected` dùng `key.Trim()` nhưng `ReadRaw`/`WriteRaw` dùng key chưa trim. `RestoreCoreValue(entry)` với `entry.Key = " Path"` qua được `IsProtected` rồi `WriteRaw` tạo một biến mới tên có khoảng trắng. Qua UI không tới được (`ListBackups` so khớp `OrdinalIgnoreCase` không trim với key đang chọn) nên chỉ là phòng thủ chiều sâu; key chỉ ghi được là key trong file, nhưng đã bị chặn bởi `IsProtected` cho 12 key. | Tạo biến rác nếu sau này N2-2 gọi trực tiếp. | Chuẩn hóa: dùng `entry.Key.Trim()`/so sánh key đúng bằng `CoreProtectedKeys` không trim, hoặc từ chối key khác bản trim. |
| minor | UC_ConfigEnvCustom.cs:142-146, 641-645; EnvRegistryService.cs:85-102 | `FrmMain.RunWithLoadingAsync` chạy `action()` đồng bộ trên luồng UI (sau `Task.Yield`). `SendMessageTimeout(HWND_BROADCAST, ..., SMTO_ABORTIFHUNG, 5000)` có thể mất tới 5 giây mỗi cửa sổ phản hồi chậm, tức UI có thể đứng hàng chục giây và lớp loading không kịp vẽ. Cũng áp dụng cho backup/ghi registry. | Giao diện treo khi ghi biến. Cùng bản chất minor đã ghi ở N1; chữ ký P/Invoke đúng (`nint`/`nuint`, `CharSet.Unicode`, `out nuint`, `HWND_BROADCAST = 0xffff`, tham số `"Environment"`). | Bọc phần việc nền bằng `Task.Run` bên trong `RunWithLoadingAsync(() => Task.Run(() => ...))` (service không đụng control UI nên an toàn). Không bắt buộc vòng này. |
| minor | UC_ConfigEnvCustom.Designer.cs:121,106 | Trùng `TabIndex`: `btn_Refresh` bị đổi 3 thành 4 trong khi `btn_Add` vẫn là 4 (Edit 5, Delete 6). Thứ tự Tab giữa Làm mới và Thêm không xác định rõ. Vị trí `btn_Restore` (x=555, rộng 80, không chồng nút nào: Refresh bắt đầu x=640, ToggleMask kết thúc x=390) và Anchor/tooltip đều đúng. | Thứ tự Tab hơi mơ hồ. | Đánh lại `Add=5`, `Edit=6`, `Delete=7`. |
| minor | CustomEnvService.cs:105-113 | Không phát hiện "biến bị đổi giữa chừng" (R mục 6): form sửa đọc giá trị thô lúc mở; `UpdateCoreValue` đọc lại lúc lưu nhưng không so với giá trị ban đầu, nên ghi đè thay đổi của tiến trình khác (có backup bảo vệ). Trường hợp bị xóa thì đã xử lý (`KeyNotFoundException` rồi Abort, tải lại). Cũng chưa bỏ qua khi `newValue == current.Value` (vẫn tạo backup thừa). | Mất thay đổi của bên khác (khôi phục được từ backup). | Chấp nhận ở N2-1 hoặc truyền thêm giá trị gốc và so sánh trước khi ghi. |
| nit | EnvBackupService.cs:120-122 | Ba nhánh `DateTime.TryParse`, nhánh cuối dùng culture hiện hành; `CreatedAt` do app tự ghi định dạng cố định nên nhánh 2-3 gần như không cần. Không gây lỗi. | — | Giữ nhánh `ParseExact` (+ Invariant); bỏ nhánh culture hiện hành. |
| nit | UC_ConfigEnvCustom.cs:94 | `btn_Restore_Click(object? sender, ...)` khác kiểu khai báo `object sender` của các handler còn lại. | — | Thống nhất theo style file. |
| nit | EnvBackupService.cs:160-163 | Sắp xếp `FilePath` giảm dần: `_10` đứng trước `_2` khi >9 backup cùng giây. | Không đáng kể. | Bỏ qua. |

## 2. Đánh giá theo nhóm (đọc code thật)

- **Đúng spec:** đủ 7 file, không file ngoài danh sách (đối chiếu `git status`). Thứ tự `UpdateCoreValue` (Protected, Elevated, Validate/xuống dòng, ReadRaw, Backup `Edit`, WriteRaw kind cũ, Broadcast) đúng; validate đặt trước `ReadRaw` là hợp lý, lệch nhỏ vô hại. `RestoreCoreValue` đúng (trừ major trên). Luồng Sửa: biến list chỉ cảnh báo, không mở form; biến cốt lõi non-list: elevated, gõ key, giá trị thô, `isCoreEdit` ẩn `grpGen` (`Visible = !_isCoreEdit`), kiểm không xuống dòng; biến thường giữ nguyên N1; `btn_Delete_Click` vẫn chặn mọi biến cốt lõi; `btn_Add_Click` và nhánh thường dùng `isCoreEdit` mặc định `false`. Luồng Khôi phục đúng thứ tự: IsProtected, elevated, list backup rỗng thì báo, picker, gõ key, `RestoreCoreValue`.
- **An toàn dữ liệu/quyền:** mọi đường ghi biến thật đều qua IsElevated + `FrmEnvTypeKeyConfirm` (OK chỉ bật khi `Ordinal` khớp, kiểm lại lần hai trong `BtnOk_Click`); service cũng tự kiểm elevated. `entry.Key` lấy từ file chỉ có thể ghi khi `IsProtected` (12 key). `SanitizeFileName` thay mọi ký tự không hợp lệ của tên file (kể cả `\` `/` `:`), không có path traversal; key trong file khác key đang chọn bị `ListBackups` loại. Lỗ hổng: xem major và các minor trên.
- **Bảo mật:** không `Console/Log/Debug/Trace` (đã tự grep lại: rỗng); thông báo lỗi chỉ nêu key (IOException khi backup có thể nêu đường dẫn file backup, không có giá trị); backup lưu thô không mã hóa theo R10, cần cảnh báo ở handover rằng `Path` có thể lộ tên thư mục người dùng. P/Invoke đúng chữ ký (xem trên), lỗi broadcast bị nuốt có chủ ý theo comment. Lưu ý quyền: manifest `requireAdministrator` nên `IsElevated()` luôn true trên thực tế; vẫn đúng khi giữ làm lớp phòng thủ.
- **Convention:** PascalCase, `_camelCase`, một class một file với file mới, comment giải thích vì sao, không `TODO`, không using thừa mới đáng kể (`UC_ConfigEnvCustom.cs` còn using thừa có sẵn từ N1: `System.Data`, `System.ComponentModel`, `SystemManagement.Views`; `EnvRegistryService.cs`/`FrmEnvRestorePicker.cs` dùng `System.ComponentModel` cho attribute, hợp lý). `ThrowWarning/Info/Error` đúng style N1. `FrmCustomEnvEditor` vẫn chung file UC như N1 (nằm ngoài phạm vi tách).
- **Phạm vi:** mọi dòng đổi truy được về spec; không refactor ngoài phạm vi, không package mới; Designer chỉ thêm `btn_Restore` và đổi `TabIndex` của `btn_Refresh` (gây trùng TabIndex, xem minor).
- **UI WinForms:** handler async đều bọc try/catch, `RunWithLoadingAsync` dùng đúng pattern repo, `using var` cho form con (Dispose đúng), `Enabled`/AcceptButton/CancelButton hợp lý, `FrmEnvRestorePicker` bật nút Chọn chỉ khi có dòng được chọn.

<!-- TOOLING:BEGIN -->
## 2b. Phạm vi ảnh hưởng
Không dùng CodeGraph (chỉ mục không áp dụng cho dự án WinForms này). `Grep` toàn `SystemManagement/`: `CustomEnvService`/`FrmCustomEnvEditor` chỉ được `UC_ConfigEnvCustom.cs` dùng; hai chỗ gọi cũ (`btn_Add_Click`, nhánh biến thường của `btn_Edit_Click`) không đổi hành vi nhờ tham số `isCoreEdit = false`. `IsProtected`/`Delete`/`UpdateValue`/`Create` giữ nguyên. Không file CommonLibs nào bị sửa.
<!-- TOOLING:END -->

## 3. Đã kiểm / chưa kiểm được
- **Đã kiểm bằng đọc code:** toàn bộ 7 file (đọc nguyên văn), `git status`, diff Designer, `FrmMain.RunWithLoadingAsync`/`ThrowXxx`, tài liệu spec/yêu cầu/log/test report. Tin cậy kết quả harness 32/32 của tester ở các mục đã đối chiếu với code, không chạy lại.
- **Chưa kiểm được (không tính là đạt):** chạy thực tế UI, ghi registry thật (AC2, AC9, AC10), AC1 phần ExpandString, hành vi `AppContext.BaseDirectory` khi `PublishSingleFile`, hiển thị Designer bằng mắt. Chưa build lại (tin build 0 Error của tester/Claude).

## 4. Đề xuất sau
- Gộp các minor (validate khi khôi phục, kiểm kiểu registry, `Task.Run`, TabIndex, so sánh giá trị gốc khi lưu) vào vòng sửa này nếu rẻ; phần còn lại có thể ghi nhận cho N2-2.
- Handover nên nhắc: backup là plaintext cạnh exe, nên đặt exe ở thư mục chỉ Administrator ghi được.

VERDICT: REQUEST_CHANGES
