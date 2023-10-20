using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ExcelPatternTool.Core.Helper;
using ExcelPatternTool.ViewModel;

namespace ExcelPatternTool.View
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        private static readonly string basePath = CommonHelper.AppBasePath;
        public SettingPage()
        {
            InitializeComponent();
            string path = Path.Combine(basePath, "Data", "_pattern.json");
            this.FileUrlTextBlock.Text = path;

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = this.DataContext as SettingPageViewModel;
            vm.RaiseSettingChanged();
        }


        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/jevonsflash/ExcelPatternTool");

        }

        private void Hyperlink_Click2(object sender, RoutedEventArgs e)
        {

            string path = Path.Combine(basePath, "Data", "_pattern.json");
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", path);

            }
            catch (Exception)
            {
                System.Diagnostics.Process.Start("explorer.exe", Path.Combine(basePath, "Data"));


            }

        }
    }
}
