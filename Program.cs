using System;
using System.Threading;
using System.Windows.Forms;

namespace CafeManager
{
    static class Program
    {
        private static Mutex mutex = new Mutex(true, "CafeManager_Unique_App_Name_12345");

        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    using (LoginForm login = new LoginForm())
                    {
                        if (login.ShowDialog() == DialogResult.OK)
                        {
                            // ==========================================
                            // ✅ استفاده از FormMain (فرم اصلی شما)
                            // ==========================================
                            Application.Run(new FormMain());
                            // ==========================================
                        }
                        else
                        {
                            Application.Exit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطا در اجرای برنامه: {ex.Message}",
                        "خطا",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                MessageBox.Show("برنامه در حال حاضر در حال اجرا است.",
                    "توجه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}