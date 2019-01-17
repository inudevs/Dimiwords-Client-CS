using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimiwords_Client_CS
{
    public partial class Learn : Form
    {
        private ToolTip buttonToolTip = new ToolTip();
        private Wordbooks[] wordbooks;
        private int page = 0, accept = 0, submit = 0;
        private Main main;
        private bool skip = false;

        public Learn(Wordbooks[] wordBooks, Main mAin)
        {
            InitializeComponent();
            wordbooks = wordBooks;
            main = mAin;
        }

        private void play()
        {
            for (var i = 0; i < wordbooks.Count(); i++)
            {
                Invoke((MethodInvoker)delegate () { label2.Text = $"{++page} / {wordbooks.Count()}"; });
                var ko = "";
                for (var count = 0; count < wordbooks[i].ko.Count(); count++)
                {
                    var ind = "";
                    if (count > 0)
                        ind = ", ";
                    ko += $"{ind}{wordbooks[i].ko[count]}";
                }
                Invoke((MethodInvoker)delegate () { label1.Text = ko; });
                while (true)
                {
                    if (textBox1.Text == wordbooks[i].en)
                    {
                        accept++;
                        break;
                    }
                    if (skip)
                    {
                        skip = false;
                        break;
                    }
                    Thread.Sleep(100);
                }
                Invoke((MethodInvoker)delegate () { textBox1.Text = ""; });
                submit++;
            }
            MessageBox.Show($"{submit}문제 중 {accept}개를 맞췄어요!\n총 {accept}포인트를 획득했습니다!", "Congratulation!", MessageBoxButtons.OK, MessageBoxIcon.None);
            Invoke((MethodInvoker)Close);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, $"{wordbooks[page].en.First()}로 시작하는 단어랍니다!");
        }

        private void button1_MouseHover(object sender, EventArgs e)
        {
            buttonToolTip.SetToolTip(button1, "이 버튼을 누르면 힌트를 알려줘요!");
        }

        private void Learn_Load(object sender, EventArgs e)
        {
            new Thread(play) { IsBackground = true }.Start();
        }

        private void Learn_FormClosed(object sender, FormClosedEventArgs e)
        {
            main.Show();
        }

        private void button2_MouseHover(object sender, EventArgs e)
        {
            buttonToolTip.SetToolTip(button2, "이 버튼을 누르면 모르는 단어를 스킵할 수 있어요!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            skip = true;
        }
    }
}
