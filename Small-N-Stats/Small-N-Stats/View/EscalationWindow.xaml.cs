using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Small_N_Stats.View
{
    /// <summary>
    /// Interaction logic for EscalationWindow.xaml
    /// </summary>
    public partial class EscalationWindow : MetroWindow
    {
        public EscalationWindow()
        {
            InitializeComponent();
        }

        public static implicit operator EscalationWindow(DiscountingWindow v)
        {
            throw new NotImplementedException();
        }
    }
}
