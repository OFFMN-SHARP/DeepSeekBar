using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deepseek_Bar
{
    public partial class Movebutton : Form
    {
        private bool isBarVisible = true;

        public Movebutton()
        {
            InitializeComponent();
        }

        private void Movebutton_Load(object sender, EventArgs e)
        {
            label1.Text = ">";
            this.Width = 20;
            this.Height = 40;
            this.Location = new Point(
                Screen.PrimaryScreen.Bounds.Width - (this.Width + BarSize.width),
                0
            );
            label1.Click += label1_Click;
        }

        private async void label1_Click(object? sender, EventArgs e)
        {
            if (WindowManager.BarWindow == null || WindowManager.BarWindow.IsDisposed) return;

            // 开始动画，禁止 Form1 的位置锁定
            Form1.IsAnimating = true;

            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int barWidth = WindowManager.BarWindow.Width;
            int btnWidth = this.Width;

            if (isBarVisible)
            {
                // 隐藏动画：滑出屏幕
                int startX = screenWidth - barWidth;
                int endX = screenWidth;
                for (int x = startX; x <= endX; x += 10)
                {
                    WindowManager.BarWindow.Location = new Point(x, 0);
                    this.Location = new Point(x - btnWidth, 0);
                    await Task.Delay(1);
                }
                WindowManager.BarWindow.Location = new Point(screenWidth, 0);
                this.Location = new Point(screenWidth - btnWidth, 0);
                label1.Text = "<";
                isBarVisible = false;

                // 注销 AppBar
                AppBar.UnregisterAppBar(WindowManager.BarWindow.Handle);
                // 注销后强制将侧边栏和按钮放在屏幕外（但已在循环中完成）
            }
            else
            {
                // 显示动画：滑入屏幕
                int startX = screenWidth;
                int endX = screenWidth - barWidth;
                for (int x = startX; x >= endX; x -= 10)
                {
                    WindowManager.BarWindow.Location = new Point(x, 0);
                    this.Location = new Point(x - btnWidth, 0);
                    await Task.Delay(1);
                }
                WindowManager.BarWindow.Location = new Point(endX, 0);
                this.Location = new Point(endX - btnWidth, 0);
                label1.Text = ">";
                isBarVisible = true;

                // 重新注册 AppBar
                AppBar.RegisterAppBar(WindowManager.BarWindow.Handle, barWidth);
                // 注册后强制将侧边栏归位（防止系统偏移）
                WindowManager.BarWindow.Location = new Point(screenWidth - barWidth, 0);
                this.Location = new Point(screenWidth - barWidth - btnWidth, 0);
            }

            // 动画结束，恢复位置锁定
            Form1.IsAnimating = false;
        }
    }
}