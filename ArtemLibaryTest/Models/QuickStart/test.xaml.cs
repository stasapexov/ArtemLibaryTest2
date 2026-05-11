using ArtemLibaryTest.QuickStart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArtemLibaryTest.Models.QuickStart
{
    /// <summary>
    /// Логика взаимодействия для test.xaml
    /// </summary>
    public partial class test : Window
    {
        private readonly AuthUiContext _context;

        internal test(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
            Title = $"{_context.Options.AppTitle} - Вход";
        }
    }
}
