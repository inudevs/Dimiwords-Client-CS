using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Dimiwords_Client_CS
{
    public partial class CreateWordbook : Form
    {
        private WordSchema[] Words;
        private User user_data;
        private Main mainform;

        public CreateWordbook(WordSchema[] words, User user, Main main)
        {
            InitializeComponent();
            Words = words;
            user_data = user;
            mainform = main;
            Discord.StateUpdate("단어장 만드는 중...");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //결과값 변수를 비어져 있는 string자료형으로 선언
            var result = "";
            //json형태로 Byte[]자료형 선언
            var Data = Encoding.UTF8.GetBytes(new JObject()
            {
                { "name", textBox1.Text != "" && textBox1.Text != "이름" ? textBox1.Text : "" },
                { "intro", textBox2.Text != "" && textBox2.Text != "설명" ? textBox2.Text : "" },
                { "words", JArray.FromObject(Words) },
                { "token", user_data.token }
            }.ToString());
            var req = (HttpWebRequest)WebRequest.Create("https://dimiwords.tk:5000/api/create/wordbook");
            //Post 형태로
            req.Method = "POST";
            //json 보낸다
            req.ContentType = "application/json";
            //길이는 요만큼
            req.ContentLength = Data.Length;
            //using = 용량이 큰 자료형에 존재하는 함수인 Dispose를 자동으로 실행
            //보낼 준비를 할께!
            try
            {
                using (var reqStream = req.GetRequestStream())
                {
                    //보낸다!
                    reqStream.Write(Data, 0, Data.Length);
                    //다 보냈으니 나머지는 정리할께
                    reqStream.Close();
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show(this, $"서버에 제대로 연결하지 못했습니다.\n{ex.Message}\n잠시 후 다시 시도해주세요.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
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
                        using (var sr = new StreamReader(resStream))
                        {
                            //받았다!
                            result = sr.ReadToEnd();
                            //다 받았으니 나머지는 정리할께
                            resStream.Close();
                        }
                    }
                }
                //이것도 정리
                res.Close();
            }
            //json 읽기
            var json = JObject.Parse(result);
            var success = (bool)json["success"];
            if (success)
            {
                MessageBox.Show(this, "단어장을 성공적으로 추가했습니다!", "Congratulation!", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else
            {
                MessageBox.Show(this, $"단어장을 추가하는 도중 에러가 발생했습니다.\n{json["message"].ToString()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Close();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "이름")
            {
                textBox1.Text = "";
                textBox1.ForeColor = SystemColors.WindowText;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                textBox1.Text = "이름";
                textBox1.ForeColor = SystemColors.GrayText;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "설명")
            {
                textBox2.Text = "";
                textBox2.ForeColor = SystemColors.WindowText;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (textBox2.Text.Length == 0)
            {
                textBox2.Text = "설명";
                textBox2.ForeColor = SystemColors.GrayText;
            }
        }

        private void CreateWordbook_FormClosing(object sender, FormClosingEventArgs e)
        {
            Discord.StateUpdate("단어 살펴 보는 중...");
            mainform.Show();
        }
    }
}
