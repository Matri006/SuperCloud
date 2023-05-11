using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

using General;

namespace Super_Cloud
{
    public partial class Form1 : Form
    {
        private readonly TcpClient _client;
        private readonly List<Share> _elements = new List<Share>();
        
        public Form1()
        {
            StreamReader inp = new StreamReader("ip.txt");
            string ip = inp.ReadLine();
            _client = new TcpClient(ip, 5000);

            AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
            {
                _client.SendCode(13);
                _client.Close();
                _client.Dispose();
            };
            
            InitializeComponent();
        }
        string back = "";
        private void Form1_Load(object sender, EventArgs e)
        {
            
            _client.SendCode(5);

            button2.Enabled = Convert.ToBoolean(_client.ReceiveCodeUntil());
        }

        private void button3_Click(object sender, EventArgs e) //First Login
        {
            
            _client.SendCode(1);
            _client.SendJson(new Worker {Name = textBox1.Text, Password = textBox2.Text});
            back = "admin";
            tabControl1.SelectTab(Admin);
        }

        private void button4_Click(object sender, EventArgs e) //Login
        {
            var type = "";

            if (radioButton1.Checked) {
                radioButton2.Checked = false;
                type = "admin";
            }
            else {
                radioButton2.Checked = true;
                type = "user";
            }
            back = type;
            
            _client.SendCode(2);
            _client.SendJson(new Worker {Type = type, Name = textBox3.Text, Password = textBox4.Text});

            if (_client.ReceiveCodeUntil() == 1) {
                tabControl1.SelectTab(type == "admin" ? Admin : User);
            }
            else {
                radioButton1.Checked = false;
                radioButton2.Checked = false;
                textBox3.Text = "";
                textBox4.Text = "";
                label12.Text = "Совпадений не найдено!!! Введите другие данные.";
            }
        }

        private void button14_Click(object sender, EventArgs e) //Add
        {
            var type = "";

            if (radioButton3.Checked) {
                radioButton4.Checked = false;
                type = "admin";
            }
            else {
                radioButton4.Checked = true;
                type = "user";
            }

            
            _client.SendCode(3);
            _client.SendJson(new Worker {Type = type, Name = textBox5.Text, Password = textBox6.Text});

            if (_client.ReceiveCodeUntil() == 1) {
                tabControl1.SelectTab(EmployeeManagement);
            }
            else {
                label13.Text = "Пользователь с таким именем уже существует!!! Введите другие данные.";
                radioButton3.Checked = false;
                radioButton4.Checked = false;
                textBox5.Text = "";
                textBox6.Text = "";
            }
        }

        private void button15_Click(object sender, EventArgs e) //Delete
        {
            var type = "";

            if (radioButton5.Checked) {
                radioButton6.Checked = false;
                type = "admin";
            }
            else {
                radioButton6.Checked = true;
                type = "user";
            }

            
            _client.SendCode(4);
            _client.SendJson(new Worker {Type = type, Name = textBox8.Text, Password = textBox7.Text});

            if (_client.ReceiveCodeUntil() == 0) {
                label14.Text = "Данные введены неверно!!!";
                radioButton5.Checked = false;
                radioButton6.Checked = false;
                textBox7.Text = "";
                textBox8.Text = "";
            }
            else {
                tabControl1.SelectTab(EmployeeManagement);
            }
        }

#region Navigation btns of tab
        
        private void button1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(Login);
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(FirstLogin);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(EmployeeManagement);
        }

        private void button12_Click(object sender, EventArgs e) //to Add
        {
            tabControl1.SelectTab(Add);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(Delete);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(PublicCloud);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(PublicCloud);
        }

#endregion

        private void button18_Click(object sender, EventArgs e) //обновить данные
        {
            listBox1.Items.Clear();

            
            _client.SendCode(6);

            var workers = _client.ReceiveJson<IEnumerable<Worker>>();
            foreach (var worker in workers) {
                listBox1.Items.Add($"{worker.Type}\t{worker.Name}\t{worker.Password}");
            }
        }

        private void button17_Click(object sender, EventArgs e) //public cloud
        {
            listBox2.Items.Clear();
            _elements.Clear();

            
            _client.SendCode(7);
            _elements.AddRange(_client.ReceiveJson<IEnumerable<Share>>());

            foreach (var i in _elements) {
                if (i.Folder == "publicfiles") {
                    listBox2.Items.Add(i.Name);
                    var v = new Share();
                    v.Name = i.Name;
                    v.Folder = i.Folder;
                    v.Type = i.Type;
                }
            }
        }

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var imia = listBox2.SelectedItem.ToString();

            foreach (var i in _elements) {
                if (i.Name == imia && i.Type == "folder") {
                    listBox2.Items.Clear();
                    button19.Enabled = true;

                    foreach (var j in _elements) {
                        if (j.Folder == imia) {
                            listBox2.Items.Add(j.Name);
                            var v = new Share();
                            v.Name = i.Name;
                            v.Folder = i.Folder;
                            v.Type = i.Type;
                        }
                    }
                }
            }
        }

        private void button20_Click(object sender, EventArgs e) // save
        {
            var selectedShare = _elements.First(x => x.Name == (string)listBox2.SelectedItem);
            saveFileDialog1.FileName = (string)listBox2.SelectedItem;
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) {
                return;
            }
            
            _client.SendCode(10);
            _client.SendJson(selectedShare);

            File.WriteAllBytes(saveFileDialog1.FileName, _client.ReceiveUntil());
        }

        private void button19_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();

            foreach (var i in _elements) {
                if (i.Folder == "publicfiles") {
                    listBox2.Items.Add(i.Name);
                    var v = new Share();
                    v.Name = i.Name;
                    v.Folder = i.Folder;
                    v.Type = i.Type;
                }
            }
        }

        private void button16_Click(object sender, EventArgs e) // Delete
        {
            if (listBox2.SelectedItem == null) {
                return; 
            }

            
            _client.SendCode(8);
            _client.SendJson(_elements.First(x => x.Name == listBox2.SelectedItem));
        }

        private void button21_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(AddFolder);
            label16.Text = "";
        }

        private void button24_Click(object sender, EventArgs e)
        {
            
            _client.SendCode(9);
            _client.Send(Encoding.UTF8.GetBytes(textBox9.Text));
            var r = Encoding.UTF8.GetString(_client.ReceiveUntil());
            
            if (r == "yes")
                label16.Text = "Папка с таким именем уже сществует!!!";
            else if (r == "no")
                tabControl1.SelectTab(PublicCloud);
        }

        private void button22_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(_elements.Where(x => x.Type == "folder").Select(x => x.Path).ToArray());
            comboBox1.Items.Add("Не выбирать папку");
            
            tabControl1.SelectTab(Upload);
        }

        private void button26_Click(object sender, EventArgs e) //find file
        {
            openFileDialog1.FileName = String.Empty;
            if (openFileDialog1.ShowDialog() != DialogResult.OK) {
                return;
            }

            textBox10.Text = openFileDialog1.FileName;

            if (comboBox1.Text == string.Empty || textBox10.Text == string.Empty) {
                label17.Text = "Введите данные!!!";
            }

            
            _client.SendCode(11);
            _client.SendJson(new Share { Type = "file", Name = openFileDialog1.SafeFileName, Folder = comboBox1.SelectedItem.ToString()});

            if (_client.ReceiveCodeUntil() == 1) {
                label17.Text = "Файл с таким именем уже существует в выбранной папке!!!";
            }
        }

        private void button25_Click(object sender, EventArgs e) //upload
        {
            if (comboBox1.SelectedItem == null) {
                return; 
            }
            
            
            _client.Send(new byte[] {12});
            _client.SendJson(new Share {Name = new FileInfo(textBox10.Text).Name, Folder = comboBox1.SelectedItem.ToString() == "Не выбирать папку" ? "publicfiles" : (string)comboBox1.SelectedItem, Type = "file"});

            if (_client.ReceiveUntil()[0] == 0) {
                return; 
            }
            
            _client.Send(File.ReadAllBytes(textBox10.Text));
            tabControl1.SelectTab(PublicCloud);
        }

        private void button28_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(Begin);
            textBox3.Text = "";
            textBox4.Text = "";
            listBox1.Items.Clear();
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            listBox2.Items.Clear();
            textBox9.Text = "";
            comboBox1.Items.Clear();
            textBox10.Text = "";
            label17.Text = "";
            label16.Text = "";
            label14.Text = "";
            label13.Text = "";
            label12.Text = "";
        }

        private void button27_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(Begin);
            textBox3.Text = "";
            textBox4.Text = "";
            listBox1.Items.Clear();
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            listBox2.Items.Clear();
            textBox9.Text = "";
            comboBox1.Items.Clear();
            textBox10.Text = "";
            label17.Text = "";
            label16.Text = "";
            label14.Text = "";
            label13.Text = "";
            label12.Text = "";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(back == "admin" ? Admin : User);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(Admin);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(PublicCloud);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(PublicCloud);
        }

        private void button23_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(EmployeeManagement);
        }

        private void button30_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(Begin);
        }

        private void button29_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(EmployeeManagement);
        }

        private void PublicCloud_Click(object sender, EventArgs e)
        {

        }
    }
}