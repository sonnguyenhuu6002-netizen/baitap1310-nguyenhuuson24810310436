using System;
using System.Windows.Forms;

namespace StudentManager
{
    /// <summary>
    /// Lớp cơ sở cho mọi MDI Child.
    /// - Con -> Cha: phát sự kiện StatusMessage (cha đăng ký nhận và ghi lên StatusStrip).
    /// - Cha -> Con: gọi hàm ảo RefreshData() trên form con đang active.
    /// </summary>
    public class ChildFormBase : Form
    {
        public event Action<string>? StatusMessage;

        protected void Report(string message) => StatusMessage?.Invoke(message);

        public virtual void RefreshData() { }
    }
}
