# N2-2 — ConfigEnv: trình sửa từng mục cho Path/PATHEXT/PSModulePath · Code Review

- **Reviewer:** Claude (chế độ B, tự review trên diff + đọc code N2-1) · **Ngày:** 2026-11-15 · **Vòng:** 1

## 1. Phát hiện
Không có blocker/major.

| Mức độ | File:dòng | Vấn đề | Gợi ý sửa |
|---|---|---|---|
| minor | FrmEnvListEditor.cs:253-291 (`BtnAdd_Click`, `BtnUpdate_Click`) | Nhập `a;b` vào ô nhập: ListBox hiện 1 mục, nhưng `Join` ghi `a;b` nên khi lưu/mở lại thành 2 mục. Trùng lặp/thiếu đường dẫn (và 2047) tính trên mục chưa tách nên cảnh báo hiển thị không khớp giá trị thực ghi. Không gây hỏng dữ liệu (chuỗi ghi ra vẫn hợp lệ), nhưng gây bất ngờ. Nên chặn: người dùng dán nguyên chuỗi Path vào ô nhập là kịch bản rất thật. | Cách rẻ nhất: nếu `text.Contains(';')` thì `ThrowWarning("Mỗi mục không được chứa ';' ...")` và return. Hoặc tự tách thành nhiều mục khi Thêm (Update: thay mục chọn bằng phần đầu, chèn phần còn lại). Khuyến nghị chặn. |
| minor | EnvListService.cs:116 + FrmEnvListEditor.cs:343-389 (`UpdateWarningDisplay`, gọi từ `LoadItems` và sau mọi thao tác Thêm/Sửa/Xóa/Lên/Xuống) + `BtnSave_Click:405` | `Directory.Exists` chạy trên UI thread cho **toàn bộ** mục, **sau mỗi thao tác** (kể cả Lên/Xuống chỉ đổi thứ tự) và ở constructor. Với đường dẫn UNC/ổ mạng offline, mỗi lệnh có thể treo vài giây đến hàng chục giây, nhân với số mục mạng, lặp lại mỗi lần bấm nút -> form đơ. Mức độ: thấp-trung bình — Path Machine thường toàn thư mục cục bộ nên hiếm, nhưng khi gặp thì UX rất tệ, không có cách thoát (Hủy cũng bị chặn vì UI thread). Không chặn merge vì spec chỉ yêu cầu "cảnh báo". | Cache kết quả theo chuỗi mục (`Dictionary<string,bool>`, so sánh OrdinalIgnoreCase) trong form để mỗi đường dẫn chỉ probe một lần; Lên/Xuống/Xóa không probe lại. Tốt hơn: chạy `FindMissingPaths` bằng `Task.Run` + cập nhật nhãn khi xong (và bỏ qua kết quả cũ). Tối thiểu: ghi vào backlog. |
| minor | FrmEnvListEditor.cs:391-440 | Lưu khi danh sách không đổi (chỉ mở rồi Lưu) vẫn tạo backup `Edit`, ghi registry, broadcast `WM_SETTINGCHANGE`. Ngoài ra Normalize có thể âm thầm đổi (trim, bỏ mục rỗng) mà người dùng không được báo. | So sánh `joinedValue` với `rawValue` gốc (lưu vào field `_originalRaw`); bằng nhau thì đóng form, không ghi. Nếu khác chỉ do trim/bỏ rỗng thì vẫn ghi — chấp nhận theo spec (Normalize là hành vi đã duyệt). |
| nit | EnvListService.cs:110-123 | Mục Path có dấu nháy `"C:\Program Files\X"` (Windows cho phép) bị `Directory.Exists` coi là không tồn tại -> cảnh báo sai. `try/catch` trần quanh `Directory.Exists` thừa (hàm này không ném với đường dẫn sai/không có quyền, chỉ trả false) và nuốt mọi lỗi; hậu quả chỉ là thêm cảnh báo nên chấp nhận được, nhưng nên bỏ hoặc comment lý do. | Bỏ cặp dấu `"` quanh mục trước khi kiểm; bỏ try/catch hoặc thêm comment "lỗi -> coi như không tồn tại, chỉ là cảnh báo". |
| nit | EnvListService.cs:62-86 | `FindDuplicates` dùng thêm `Dictionary seen` trong khi chỉ cần `HashSet`; `C:\A` và `C:\A\` (khác dấu `\` cuối) không bị coi là trùng. | Dùng `HashSet` + dictionary chỉ khi cần giữ casing; cân nhắc `TrimEnd('\\')` khi so sánh (không đổi giá trị lưu). |
| nit | FrmEnvListEditor.cs:345-351 và 393-405 | Logic lấy mục -> Normalize -> duplicates/missing (và test `isPathExt` bằng literal `"PATHEXT"`) lặp 2 lần. | Tách helper private `GetNormalizedItems()` và hằng/thuộc tính `_checkPaths`. |
| nit | FrmEnvListEditor.cs (nhiều `new Font(...)`) | Mỗi `new Font` gán cho control không được Dispose -> rò GDI handle nhỏ mỗi lần mở form. Form dựng bằng code, không partial/Designer. | Dùng chung một `Font` field và dispose trong `Dispose(bool)`; hoặc chấp nhận nếu các form N1/N2-1 cùng pattern (đã kiểm: cùng phong cách code-built). |
| nit | FrmEnvListEditor.cs:186 | Hủy/đóng form khi đã sửa dở không hỏi xác nhận; chọn mục khác ghi đè nội dung ô nhập đang gõ (`LstItems_SelectedIndexChanged:231`). | UX, ghi backlog. |

## 2. Đánh giá theo tiêu chí
- **Đúng spec:** Đủ mục 5-7: nhánh `IsListKey` nằm sau `IsElevated` và `FrmEnvTypeKeyConfirm`, rồi `GetCoreRawValue` (xử lý `KeyNotFoundException` đúng như nhánh cốt lõi); form có Thêm/Cập nhật/Xóa/Lên/Xuống, cảnh báo, Lưu/Hủy; Lưu theo thứ tự Normalize -> Validate -> cảnh báo trùng/thiếu + hỏi Yes/No -> `UpdateCoreValue`. Đúng 3 file, không đụng `CustomEnvService`; diff `UC_ConfigEnvCustom` gọn (bỏ chặn N2-1, thêm nhánh list). Không làm thừa. `PATHEXT` bỏ kiểm đường dẫn ở cả hiển thị lẫn lưu. Giới hạn 2047 chặn đúng (đúng 2047 hợp lệ, tính cả `;`). Thông báo thành công nhắc mở lại terminal, lưới tải lại. Hủy/đóng cửa sổ: chỉ `Cancel`, không ghi.
- **Độ chính xác Split/Join/Normalize:** `Split` giữ thứ tự + mục rỗng (`"a;b;;c"` -> 4 phần tử); `Join` không `;` cuối; `%...%` không bị mở rộng ở bất kỳ bước nào (chỉ thao tác chuỗi, giá trị thô từ `ReadRaw`); kiểu registry giữ nhờ `WriteRaw(key, value, current.Kind)` trong N2-1. Điểm cần biết: Normalize Trim từng mục (đổi mục có khoảng trắng đầu/cuối) và bỏ mục rỗng — đúng spec.
- **Đúng convention:** namespace `SystemManagement.Views.ConfigEnv`, field `_camelCase`, comment tiếng Việt giải thích mục đích, tên theo mục 6 spec. Ngoại lệ nhỏ: `Validate` trả `string?` đúng spec; một số chỗ trùng lặp/Font (xem nit). Không có `partial`/Designer property public nên không dính `WFO1000`. `catch (Exception ex) when (...)` cho Security/Unauthorized không dùng `ex` ngoài filter: ổn.
- **Hiệu quả:** Chi phí chính là I/O đồng bộ trên UI thread (minor #2). Các phép còn lại O(n) trên danh sách nhỏ.
- **An toàn:** Không log giá trị (không có `Console/Log`; thông báo lỗi chỉ dùng `ex.Message` của N2-1, chứa tên key, không chứa giá trị); mọi ghi qua `CustomEnvService.UpdateCoreValue` (elevated, `IsProtected`, backup `Edit` trước khi ghi, backup lỗi thì dừng, broadcast); form không chạm registry trực tiếp; không có đường ghi nào ngoài nút Lưu; Hủy không ghi. Luồng quyền: trước khi mở form đã kiểm elevated + gõ lại tên key; `UpdateCoreValue` còn tự kiểm lần nữa (defense in depth). Lỗi: `Unauthorized/Security` -> giữ form để thử lại; `KeyNotFound` -> `Abort` đóng form, UC tải lại lưới; lỗi khác -> báo, giữ form. Rủi ro nhỏ còn lại: nếu `WriteRaw` đã ghi mà `Broadcast` ném lỗi thì người dùng thấy lỗi dù registry đã đổi (thuộc N2-1, không thuộc phạm vi này).
- **Dễ bảo trì và đủ test:** `EnvListService` thuần, có 37 kiểm tra harness ngoài repo (AC1-5 PASS). AC6-AC11 chưa chạy (cần Administrator/UI) — theo test report còn chờ người dùng thủ công; chưa có test project (ngoài phạm vi). Cảnh báo `a;b` do tester nêu: xem minor #1.

<!-- TOOLING:BEGIN -->
## 2b. Phạm vi ảnh hưởng
Không dùng CodeGraph (chỉ mục không phủ SystemManagement/WinForms). Đã đọc `CustomEnvService` (N2-1): `IsListKey`, `GetCoreRawValue`, `UpdateCoreValue` không bị sửa; `UpdateCoreValue` chấp nhận list key. Nơi gọi duy nhất tới thứ mới: `UC_ConfigEnvCustom.btn_Edit_Click`; nhánh list `return` sớm nên luồng core không-list và biến thường không đổi. Chỉ có 3 file đổi, đúng spec mục 3.
<!-- TOOLING:END -->

## 3. Đề xuất sau (minor, không chặn)
- Chặn/tách `;` trong ô nhập (minor #1) — nên làm sớm vì rẻ, có thể gộp vào một lần sửa nhỏ trước khi đóng task.
- Cache/chạy nền `FindMissingPaths` để tránh treo UI với đường dẫn mạng (minor #2).
- Bỏ qua ghi khi không đổi (minor #3).
- Các nit: bỏ dấu `"` khi kiểm tồn tại, `HashSet` + so sánh dấu `\` cuối, tách helper trùng lặp, dispose `Font`, hỏi xác nhận khi Hủy sau chỉnh sửa.
- Người dùng cần chạy thủ công AC6-AC11 (nhớ `reg export` trước, khôi phục `Path` sau) trước khi đánh ✅ trong roadmap.

VERDICT: APPROVE
