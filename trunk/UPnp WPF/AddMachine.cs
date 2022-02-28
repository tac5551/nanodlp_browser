using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NanoDLP_Browser;

namespace NanoDLP_Browser
{
    public partial class AddMachine : Form
    {
        public AddMachine()
        {
            InitializeComponent();
        }
        private Dto _dto;
        public void setDto(Dto value) {
            _dto = value;
        }
    }
}
