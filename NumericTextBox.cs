using System;
using System.Windows.Forms;

namespace CafeManager
{
    public class NumericTextBox : TextBox
    {
        private int _value = 0;
        private bool _isFormatting = false; // جلوگیری از حلقه

        public int Value
        {
            get
            {
                if (int.TryParse(Text.Replace(",", ""), out int result))
                    return result;
                return 0;
            }
            set
            {
                _value = value;
                Text = value.ToString("N0");
            }
        }

        public NumericTextBox()
        {
            this.TextAlign = HorizontalAlignment.Left;
            this.KeyPress += NumericTextBox_KeyPress;
            this.TextChanged += NumericTextBox_TextChanged;
            this.Leave += NumericTextBox_Leave;
            this.Enter += NumericTextBox_Enter;
        }

        // فقط اعداد و Backspace مجاز هستند
        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        // هنگام ورود به فیلد، کاماها را حذف می‌کنیم تا کاربر راحت تایپ کند
        private void NumericTextBox_Enter(object sender, EventArgs e)
        {
            Text = Text.Replace(",", "");
            Select(Text.Length, 0);
        }

        // هنگام تایپ، فرمت با جداکننده اعمال می‌شود
        private void NumericTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_isFormatting) return;
            if (!this.Focused) return; // فقط هنگام تایپ کاربر

            // حذف کاماهای قبلی
            string rawText = Text.Replace(",", "");

            if (string.IsNullOrEmpty(rawText))
                return;

            if (int.TryParse(rawText, out int value))
            {
                _isFormatting = true;
                // ذخیره موقعیت کورسور
                int cursorPos = this.SelectionStart;

                // اعمال فرمت
                Text = value.ToString("N0");

                // بازگرداندن کورسور به انتهای متن
                this.Select(Text.Length, 0);

                _isFormatting = false;
            }
        }

        // هنگام خروج از فیلد، اگر خالی بود مقدار صفر قرار می‌گیرد
        private void NumericTextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
            {
                Text = "0";
                return;
            }

            if (int.TryParse(Text.Replace(",", ""), out int value))
                Text = value.ToString("N0");
            else
                Text = "0";
        }
    }
}