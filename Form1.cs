using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Deepseek_Bar
{
    public partial class Form1 : Form
    {
        // 全局动画标志，Movebutton 动画期间设为 true
        public static bool IsAnimating = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Height = Screen.PrimaryScreen.Bounds.Height;
            this.Width = BarSize.width;
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width, 0);
            BarSize.width = this.Width;
            BarSize.height = this.Height;

            if (!AppBar.RegisterAppBar(this.Handle, this.Width))
            {
                MessageBox.Show("注册 AppBar 失败，可能已有其他程序占用相同边缘");
            }
            else
            {
                // 注册成功后强制刷新位置（防止系统偏移）
                this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width, 0);
                this.TopMost = true;
            }

            Movebutton MB = new Movebutton();
            MB.Show();
        }

        // 拦截系统位置变更，非动画期间强制锁定水平位置
        protected override void WndProc(ref Message m)
        {
            const int WM_WINDOWPOSCHANGING = 0x0046;
            const int WM_MOVE = 0x0003;

            if (!IsAnimating && m.Msg == WM_WINDOWPOSCHANGING)
            {
                var wp = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
                int expectedX = Screen.PrimaryScreen.Bounds.Width - this.Width;
                if (wp.x != expectedX)
                {
                    wp.x = expectedX;
                    Marshal.StructureToPtr(wp, m.LParam, true);
                }
            }

            base.WndProc(ref m);

            if (!IsAnimating && m.Msg == WM_MOVE)
            {
                int expectedX = Screen.PrimaryScreen.Bounds.Width - this.Width;
                if (this.Location.X != expectedX)
                {
                    this.Location = new Point(expectedX, this.Location.Y);
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppBar.UnregisterAppBar(this.Handle);
            base.OnFormClosed(e);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPOS
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public uint flags;
    }

    public static class BarSize
    {
        public static int width { get; set; } = 350;
        public static int height { get; set; } = 450;
    }

    public class AppBar
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const uint ABM_NEW = 0x00;
        private const uint ABM_REMOVE = 0x01;
        private const uint ABM_QUERYPOS = 0x02;
        private const uint ABM_SETPOS = 0x03;
        private const uint ABE_RIGHT = 0x02;

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        public static bool RegisterAppBar(IntPtr hwnd, int barWidth)
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
            abd.hWnd = hwnd;
            abd.uEdge = ABE_RIGHT;

            uint ret = SHAppBarMessage(ABM_NEW, ref abd);
            if (ret == 0) return false;

            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);
            abd.rc = new RECT
            {
                Left = screenWidth - barWidth,
                Top = 0,
                Right = screenWidth,
                Bottom = screenHeight
            };

            SHAppBarMessage(ABM_QUERYPOS, ref abd);
            SHAppBarMessage(ABM_SETPOS, ref abd);
            return true;
        }

        public static bool UnregisterAppBar(IntPtr hwnd)
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
            abd.hWnd = hwnd;
            uint ret = SHAppBarMessage(ABM_REMOVE, ref abd);
            return ret != 0;
        }
    }
}