using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private MainWindow _parent;
        private Dto _dto;
        private bool _new;
        public AddMachine(MainWindow parent,Dto dto)
        {
            InitializeComponent();
            _parent = parent;
            if (dto == null) { 
                _new = true;
                dto = new Dto() ;
                dto.Name = "NanoDLP";
                dto.URI = "http://";
                dto.ManualAdd = true;
            }
            _dto = dto;
        }

        private void AddMachine_Load(object sender, EventArgs e)
        {
            this.textBox1.Text = _dto.Name;
            this.textBox2.Text = _dto.URI;
            this.textBox3.Text = _dto.Discription;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            _dto.Name = this.textBox1.Text;
            _dto.URI = this.textBox2.Text;
            _dto.Discription = this.textBox3.Text;
            _dto.UUID = _dto.Discription;
            if (_new)
            {
                _parent._dtos.Add(_dto);
            }
            _parent.MyListBox.Items.Refresh();

            FileIO.SaveFile(_parent._dtos);

            this.Close();
        }
    }
}
