using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemManagement.Views.Base
{
    /// <summary>
    /// Form nền cho các form có thao tác ghi/IO (registry, file...) có thể chậm.
    /// Giải thích: gom phần "đang lưu" dùng chung (cờ, khóa form, chặn đóng form, chạy nền dưới loader)
    /// để form con chỉ cần gọi <see cref="RunSavingAsync"/> và định nghĩa control nào bị khóa
    /// qua <see cref="OnSavingStateChanged"/>, không phải khai báo lại.
    /// </summary>
    public class Frm_Base : Form
    {
        /// <summary>
        /// True khi form đang lưu. Dùng để thoát sớm ở các handler (chống bấm lặp) và chặn đóng form.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected bool IsSaving { get; private set; }

        /// <summary>
        /// Đổi trạng thái "đang lưu": đặt cờ, con trỏ chờ, rồi cho form con khóa/mở khóa control của nó.
        /// </summary>
        protected void SetSavingState(bool saving)
        {
            IsSaving = saving;
            UseWaitCursor = saving;
            ShowSavingBar(saving);
            OnSavingStateChanged(saving);
        }

        /// <summary>
        /// Thanh tiến trình riêng của form, tạo lần đầu khi cần (null nếu form chưa từng lưu).
        /// </summary>
        private ProgressBar? _savingBar;

        /// <summary>
        /// Hiện/ẩn thanh tiến trình chạy liên tục của chính form, giống <c>progressBar1</c> của <c>FrmMain</c>:
        /// 500x24, kiểu marquee, nằm giữa form và nổi lên trên các control khác.
        /// Giải thích: loader chung của ứng dụng là <c>progressBar1</c> của cửa sổ chính (<c>FrmMain</c>), nên khi form
        /// được mở bằng <c>ShowDialog</c> thanh đó nằm sau hộp thoại và người dùng không thấy. Mỗi form tự có thanh
        /// riêng để luôn thấy đang xử lý; tạo bằng code ở lớp nền (nội dung động) nên form con không phải thêm control nào.
        /// </summary>
        private void ShowSavingBar(bool show)
        {
            if (show)
            {
                if (_savingBar == null)
                {
                    _savingBar = new ProgressBar
                    {
                        Name = "pb_Saving",
                        // Anchor None: giữ nguyên kích thước và luôn ở giữa khi form đổi kích thước (như progressBar1 của FrmMain)
                        Anchor = AnchorStyles.None,
                        Style = ProgressBarStyle.Marquee,
                        MarqueeAnimationSpeed = 20,
                        Visible = false
                    };
                    Controls.Add(_savingBar);
                }

                // Kích thước 500x24 theo DPI, không rộng quá form (trừ lề 20px mỗi bên); đặt lại mỗi lần hiện vì form có thể đã đổi kích thước
                int width = Math.Min(LogicalToDeviceUnits(500), Math.Max(0, ClientSize.Width - LogicalToDeviceUnits(40)));
                int height = LogicalToDeviceUnits(24);
                _savingBar.Size = new Size(width, height);
                _savingBar.Location = new Point((ClientSize.Width - width) / 2, (ClientSize.Height - height) / 2);
                _savingBar.Visible = true;
                _savingBar.BringToFront();
            }
            else if (_savingBar != null)
            {
                _savingBar.Visible = false;
            }
        }

        /// <summary>
        /// Form con override để khóa (saving = true) hoặc mở khóa (saving = false) các nút và ô nhập của mình.
        /// </summary>
        protected virtual void OnSavingStateChanged(bool saving)
        {
        }

        /// <summary>
        /// Chạy một việc ghi/IO dưới loader và khóa form trong lúc chạy; luôn mở khóa trong finally.
        /// Giải thích: <c>FrmMain.RunWithLoadingAsync</c> chạy action trên luồng UI, nên việc nặng phải được
        /// form con bọc <c>Task.Run</c> trước khi truyền vào (đọc giá trị control ở luồng UI trước khi chạy nền).
        /// Ngoại lệ ném từ <paramref name="action"/> được ném lại cho form con tự xử lý.
        /// </summary>
        protected async Task RunSavingAsync(Func<Task> action)
        {
            SetSavingState(true);
            try
            {
                await FrmMain.RunWithLoadingAsync(action);
            }
            finally
            {
                // Mở khóa trước khi form con gọi Close() để OnFormClosing không chặn việc đóng của chính nó
                SetSavingState(false);
            }
        }

        /// <summary>
        /// Chặn đóng form (kể cả nút X, Alt+F4) khi đang lưu.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (IsSaving)
            {
                e.Cancel = true;
                return;
            }

            base.OnFormClosing(e);
        }
    }
}
