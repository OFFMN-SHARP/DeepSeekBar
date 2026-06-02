namespace Deepseek_Bar
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Form1 mainForm = new Form1();
            WindowManager.BarWindow = mainForm;
            Application.Run(mainForm);
        }
    }
    public static class WindowManager
    {
        public static Form1 BarWindow { get; set; }
    }
}