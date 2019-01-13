using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Dimiwords_Client_CS
{
    public partial class Signup : Form
    {
        private int department = 10;


        public Signup()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.FromKnownColor(KnownColor.Control);
            button2.BackColor = Color.Gray;
            button3.BackColor = Color.Gray;
            button4.BackColor = Color.Gray;
            department = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.Gray;
            button2.BackColor = Color.FromKnownColor(KnownColor.Control);
            button3.BackColor = Color.Gray;
            button4.BackColor = Color.Gray;
            department = 1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.Gray;
            button2.BackColor = Color.Gray;
            button3.BackColor = Color.FromKnownColor(KnownColor.Control);
            button4.BackColor = Color.Gray;
            department = 2;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.Gray;
            button2.BackColor = Color.Gray;
            button3.BackColor = Color.Gray;
            button4.BackColor = Color.FromKnownColor(KnownColor.Control);
            department = 3;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "이메일")
            {
                textBox1.Text = "";
                textBox1.ForeColor = SystemColors.WindowText;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                textBox1.Text = "이메일";
                textBox1.ForeColor = SystemColors.GrayText;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "비밀번호")
            {
                textBox2.Text = "";
                textBox2.ForeColor = SystemColors.WindowText;
                textBox2.UseSystemPasswordChar = true;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (textBox2.Text.Length == 0)
            {
                textBox2.Text = "비밀번호";
                textBox2.ForeColor = SystemColors.GrayText;
                textBox2.UseSystemPasswordChar = false;
            }
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            if (textBox3.Text == "이름(실명)")
            {
                textBox3.Text = "";
                textBox3.ForeColor = SystemColors.WindowText;
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (textBox3.Text.Length == 0)
            {
                textBox3.Text = "이름(실명)";
                textBox3.ForeColor = SystemColors.GrayText;
            }
        }

        private void textBox4_Enter(object sender, EventArgs e)
        {
            if (textBox4.Text == "한 줄 소개")
            {
                textBox4.Text = "";
                textBox4.ForeColor = SystemColors.WindowText;
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            if (textBox4.Text.Length == 0)
            {
                textBox4.Text = "한 줄 소개";
                textBox4.ForeColor = SystemColors.GrayText;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains(" ") || textBox1.Text == "이메일" || textBox2.Text.Contains(" ") || textBox2.Text == "비밀번호" || textBox3.Text.Contains(" ") || textBox3.Text == "이름(실명)" || textBox4.Text.Contains(" ") || textBox4.Text == "한 줄 소개")
            {
                MessageBox.Show(this, "모든 필드에 값을 제대로 입력해 주세요.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (department == 10)
            {
                MessageBox.Show(this, "과를 선택해 주세요.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //결과값 변수를 비어져 있는 string자료형으로 선언
            var result = string.Empty;
            //json형태로 Byte[]자료형 선언
            var Data = Encoding.UTF8.GetBytes($"{{\"name\":\"{textBox1.Text}\",\"password\":\"{textBox2.Text}\",\"email\":\"{textBox3.Text}\",\"intro\":\"{textBox4.Text}\",\"department\":{department}}}");
            //로그인 서버
            var req = (HttpWebRequest)WebRequest.Create("https://dimiwords.tk:5000/api/create/user");
            //Post 형태로
            req.Method = "POST";
            //json 보낸다
            req.ContentType = "application/json";
            //길이는 요만큼
            req.ContentLength = Data.Length;
            //using = 용량이 큰 자료형에 존재하는 함수인 Dispose를 자동으로 실행
            //보낼 준비를 할께!
            using (var reqStream = req.GetRequestStream())
            {
                //보낸다!
                reqStream.Write(Data, 0, Data.Length);
                //다 보냈으니 나머지는 정리할께
                reqStream.Close();
            }
            //이제 받을 준비를 할께!
            using (var res = (HttpWebResponse)req.GetResponse())
            {
                //너의 상태가 괜찮아 보이는군!
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    //받을 준비를 할께!
                    using (var resStream = res.GetResponseStream())
                    {
                        //받았다!
                        result = new StreamReader(resStream).ReadToEnd();
                        //다 받았으니 나머지는 정리할께
                        resStream.Close();
                    }
                }
                //이것도 정리
                res.Close();
            }
            var success = result.Split(new string[] { "\"success\":" }, StringSplitOptions.None)[1].Split(',')[0];
            if (Convert.ToBoolean(success))
            {
                MessageBox.Show(this, "회원가입에 성공했습니다! 로그인해 주세요.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var message = result.Split(new string[] { "\"message\":\"" }, StringSplitOptions.None)[1].Split(new string[] { "\"}" }, StringSplitOptions.None)[0];
                MessageBox.Show(this, $"계정 생성에 실패했습니다. {(message == "User exits" ? "같은 이메일의 사용자가 이미 존재합니다." : "회원가입 도중 에러가 발생했습니다.")}", "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
