using System;
using System.ComponentModel;
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
            OnSavingStateChanged(saving);
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
