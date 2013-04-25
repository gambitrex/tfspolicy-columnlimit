using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColumnLimitPolicy
{
    public partial class SettingWindow : Form
    {
        public SettingWindow()
        {
            InitializeComponent();
            txtColumnLimit.Text = Properties.Settings.Default.MaxColumnSize.ToString();
            txtRegex.Text = Properties.Settings.Default.Regex;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            String regex = txtRegex.Text;
            String strLimit = txtColumnLimit.Text;
            int limit = 0;

            if (!int.TryParse(strLimit, out limit))
            {
                limit = 120;
            }

            Properties.Settings.Default.MaxColumnSize = limit;
            Properties.Settings.Default.Regex = regex;
            Properties.Settings.Default.Save();

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
