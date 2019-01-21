using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
        private Thread t;
        private User user;

        public Learn(Wordbooks[] wordBooks, Main mAin, User userdata)
        {
            InitializeComponent();
            wordbooks = wordBooks;
            main = mAin;
            user = userdata;
        }

        private void play()
        {
            for (var i = 0; i < wordbooks.Count(); i++)
            {
                Invoke((MethodInvoker)delegate () { label2.Text = $"{++page} / {wordbooks.Count()}"; });
                var ko = string.Join(", ", wordbooks[i].ko);
                Invoke((MethodInvoker)delegate () { label1.Text = ko; });
                while (true)
                {
                    if (textBox1.Text == wordbooks[i].en)
                    {
                        //결과값 변수를 비어져 있는 string자료형으로 선언
                        var result = "";
                        //json형태로 Byte[]자료형 선언
                        var Data = Encoding.UTF8.GetBytes(new JObject()
                        {
                            { "word", wordbooks[i].id },
                            { "answer", textBox1.Text },
                            { "token", user.token }
                        }.ToString());
                        //로그인 서버
                        var req = (HttpWebRequest)WebRequest.Create("https://dimiwords.tk:5000/api/auth/check");
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
                            MessageBox.Show($"서버에 제대로 연결하지 못했습니다.\n{ex.Message}\n잠시 후 다시 시도해주세요.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                            accept++;
                            break;
                        }
                        else if (json["message"].ToString() == "Error")
                        {
                            MessageBox.Show("서버에 제대로 연결하지 못했습니다.\n잠시 후 다시 시도해주세요.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Invoke((MethodInvoker)Close);
                            return;
                        }
                        else
                        {
                            MessageBox.Show("잘못된 요청입니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Invoke((MethodInvoker)Close);
                            return;
                        }
                    }
                    if (skip)
                    {
                        //결과값 변수를 비어져 있는 string자료형으로 선언
                        var result = "";
                        //json형태로 Byte[]자료형 선언
                        var Data = Encoding.UTF8.GetBytes(new JObject()
                        {
                            { "word", wordbooks[i].id },
                            { "answer", textBox1.Text },
                            { "token", user.token }
                        }.ToString());
                        //로그인 서버
                        var req = (HttpWebRequest)WebRequest.Create("https://dimiwords.tk:5000/api/auth/check");
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
                            MessageBox.Show($"서버에 제대로 연결하지 못했습니다.\n{ex.Message}\n잠시 후 다시 시도해주세요.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Invoke((MethodInvoker)Close);
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
                        skip = false;
                        //json 읽기
                        var json = JObject.Parse(result);
                        var success = (bool)json["success"];
                        if (success)
                        {
                            MessageBox.Show("잘못된 요청입니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Invoke((MethodInvoker)Close);
                            return;
                        }
                        else if (json["message"].ToString() == "Error")
                        {
                            MessageBox.Show("서버에 제대로 연결하지 못했습니다.\n잠시 후 다시 시도해주세요.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Invoke((MethodInvoker)Close);
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Thread.Sleep(10);
                }
                Invoke((MethodInvoker)delegate () { textBox1.Text = ""; });
                submit++;
            }
            Invoke((MethodInvoker)Close);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, $"{wordbooks[page - 1].en.First()}로 시작하는 단어랍니다!");
        }

        private void button1_MouseHover(object sender, EventArgs e)
        {
            buttonToolTip.SetToolTip(button1, "이 버튼을 누르면 힌트가 나와요");
        }

        private void Learn_Load(object sender, EventArgs e)
        {
            t = new Thread(play) { IsBackground = true };
            t.Start();
        }

        private void Learn_FormClosed(object sender, FormClosedEventArgs e)
        {
            //버튼 위에 마우스 올리면 뜨는 설명을 삭제
            buttonToolTip.Dispose();
            //스레드를 강제 종료
            t.Abort();
            //메세지박스를 띄워 사용자에게 알려줌
            MessageBox.Show($"{wordbooks.Count()}문제 중 {accept}개를 맞췄어요!\n총 {accept}포인트를 획득했습니다!", "Congratulation!", MessageBoxButtons.OK, MessageBoxIcon.None);
            //디스코드 상태 업데이트
            Discord.StateUpdate("단어장 살펴 보는 중...");
            //메인창을 띄워준다
            main.Show();
        }

        private void button2_MouseHover(object sender, EventArgs e)
        {
            buttonToolTip.SetToolTip(button2, "이 버튼을 누르면 모르는 단어를 패스할 수 있어요");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            skip = true;
        }
    }
}
