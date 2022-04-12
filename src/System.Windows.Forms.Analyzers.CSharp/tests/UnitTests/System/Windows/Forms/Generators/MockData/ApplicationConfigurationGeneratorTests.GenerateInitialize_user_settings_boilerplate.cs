﻿// <auto-generated />

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace MyProject
{
    /// <summary>
    ///  Bootstrap the application configuration.
    /// </summary>
    [CompilerGenerated]
    internal static partial class ApplicationConfiguration
    {
        /// <summary>
        ///  Bootstrap the application as follows:
        ///  <code>
        ///  Application.EnableVisualStyles();
        ///  Application.SetCompatibleTextRenderingDefault(true);
        ///  Application.SetHighDpiMode(HighDpiMode.DpiUnawareGdiScaled);
        ///  Application.SetDefaultFont(new Font(new FontFamily("Microsoft Sans Serif"), 8.25f, (FontStyle)0, (GraphicsUnit)2));
        /// </code>
        /// </summary>
        public static void Initialize()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            Application.{|CS0117:SetHighDpiMode|}({|CS0103:HighDpiMode|}.DpiUnawareGdiScaled);
            Application.{|CS0117:SetDefaultFont|}(new Font(new FontFamily("Microsoft Sans Serif"), 8.25f, (FontStyle)0, (GraphicsUnit)2));
        }
    }
}