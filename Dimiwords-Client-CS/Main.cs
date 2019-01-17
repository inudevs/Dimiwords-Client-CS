﻿using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace Dimiwords_Client_CS
{
    public partial class Main : Form
    {
        private int rankpage = 1, wordbookspage = 1;

        private User user_data;
        private Login loginform;

        public Main(User user, Login login)
        {
            InitializeComponent();
            //유저 정보를 넘겨받는다
            user_data = user;
            //로그인 창을 제대로 종료하기 위해 인자로 넘겨받는다
            loginform = login;
            //유저 정보
            label4.Text = user.name;
            var department = "";
            switch (user.department)
            {
                case "0":
                    department = "EB";
                    label5.ForeColor = Color.FromArgb(0x424242);
                    break;
                case "1":
                    department = "DC";
                    label5.ForeColor = Color.FromArgb(0xFF0080);
                    break;
                case "2":
                    department = "WP";
                    label5.ForeColor = Color.FromArgb(0x9A2EFE);
                    break;
                case "3":
                    department = "HD";
                    label5.ForeColor = Color.FromArgb(0x3A01DF);
                    break;
                default:
                    Close();
                    return;
            }
            label5.Text = department;
            //이 아래는 리스트뷰의 크기를 늘리기 위해 쓰레기값을 가진 이미지 크기를 넣어준다
            var dummy = new ImageList
            {
                ImageSize = new Size(1, 20)
            };
            var dummy2 = new ImageList
            {
                ImageSize = new Size(1, 45)
            };
            listView1.SmallImageList = dummy;
            listView2.SmallImageList = dummy2;
        }

        //창을 껐을때 실행
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //로그인 창은 숨겨두기만 한 것이므로 제대로 종료한다
            loginform.Close();
        }

        private object wordbookslock = new object();

        private void GetWordbooks(object next)
        {
            Monitor.Enter(wordbookslock);
            var result = "";
            if (next != null)
            {
                if ((bool)next)
                {
                    wordbookspage++;
                }
                else
                {
                    wordbookspage--;
                }
            }
            var req = (HttpWebRequest)WebRequest.Create($"https://dimiwords.tk:5000/api/list/wordbooks?page={wordbookspage}");
            using (var res = (HttpWebResponse)req.GetResponse())
            {
                //너의 상태가 괜찮아 보이는군!
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    //받을 준비를 할께!
                    using (var resStream = res.GetResponseStream())
                    {
                        //StreamReader로 stream의 데이터를 읽는다
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
            var json = JObject.Parse(result);
            //;
            //MessageBox.Show(json["success"].ToString());
            //var success = result.Split(new string[] { "\"success\":" }, StringSplitOptions.None)[1].Split(',')[0];
            var success = (bool)json["success"];
            if (success)
            {
                //var pages_count = int.Parse(result.Split(new string[] { "\"pages\":" }, StringSplitOptions.None)[1].Split('}')[0]);
                var pages_count = (int)json["result"]["pages"];
                //현재 페이지가 1이면 이전 페이지 비활성화
                if (wordbookspage == 1)
                {
                    Invoke((MethodInvoker)delegate () { button5.Enabled = false; });
                }
                else
                {
                    //현재 페이지가 마지막 페이지면 다음 페이지 비활성화
                    if (wordbookspage == pages_count)
                    {
                        Invoke((MethodInvoker)delegate () { button4.Enabled = false; });
                    }
                    else
                    {
                        //둘다 아니라면 둘다 활성화
                        Invoke((MethodInvoker)delegate () { button5.Enabled = button4.Enabled = true; });
                    }
                }
                if (wordbookspage < 1)
                {
                    //page가 1보다 낮아지는 경우는 이전버튼을 연타하여 같은 메서드가 반복될때이다
                    //그러므로 page를 1로 고정하고 아래의 소스들은 반복하지 않는다
                    wordbookspage = 1;
                    //첫 페이지면 이전 페이지 비활성화
                    Invoke((MethodInvoker)delegate () { button5.Enabled = false; });
                }
                else if (rankpage > pages_count)
                {
                    //위와 같은 경우이다
                    wordbookspage = pages_count;
                    //마지막 페이지면 다음 페이지 비활성화
                    Invoke((MethodInvoker)delegate () { button4.Enabled = false; });
                }
                else
                {
                    //var Booksdata = result.Split(new string[] { "\"docs\":[" }, StringSplitOptions.None)[1].Split(new string[] { "}],\"total\"" }, StringSplitOptions.None)[0];
                    var Booksdata = JArray.FromObject(json["result"]["docs"]);
                    //처음엔 전체 단어장 수를 가져온다
                    //var Bookscount = int.Parse(result.Split(new string[] { "\"total\":" }, StringSplitOptions.None)[1].Split(',')[0]);
                    var Bookscount = (int)json["result"]["total"];
                    //페이지가 마지막 페이지라면
                    if (wordbookspage == pages_count)
                    {
                        //마지막 페이지의 단어장 수를 계산한다
                        Bookscount -= (pages_count - 1) * 9;
                    }
                    else
                    {
                        //아니면 그냥 9개
                        Bookscount = 9;
                    }
                    //단어장을 그냥 갱신하면 순차적으로 업데이트 되므로 한번에 업데이트 되도록 BeginUpdate 메서드 사용
                    //페이지 형식이기 때문에 Clear를 사용해 전 페이지는 삭제
                    Invoke((MethodInvoker)delegate () { listView2.BeginUpdate(); listView2.Items.Clear(); });
                    //MessageBox.Show(Booksdata[0].ToString());
                    //단어장 페이지의 단어장 수 만큼 반복
                    for (var i = 0; i < Bookscount; i++)
                    {
                        var name = Booksdata[i]["name"].ToString();
                        var intro = Booksdata[i]["intro"].ToString();
                        var wordscount = Booksdata[i]["len"].ToString();
                        var user = Booksdata[i]["user"].ToString();
                        var id = Booksdata[i]["_id"].ToString();
                        var item = new ListViewItem(new string[] { "    口    ", string.IsNullOrEmpty(name) ? "제목없음" : name, string.IsNullOrEmpty(intro) ? "설명없음" : intro, wordscount, user, id });
                        Invoke((MethodInvoker)delegate () { listView2.Items.Add(item); });
                    }
                    Invoke((MethodInvoker)delegate () { listView2.EndUpdate(); label3.Text = wordbookspage.ToString(); });
                }
            }
            else
            {
                MessageBox.Show(this, "단어장을 얻어오는데 실패했습니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (Monitor.IsEntered(wordbookslock))
                Monitor.Exit(wordbookslock);
        }

        //스레드를 한개씩만 돌리기 위한 오브젝트
        private object ranklock = new object();

        /// <summary>
        /// 랭크를 얻어오는 메서드
        /// </summary>
        /// <param name="Next">
        /// true일 경우 다음페이지
        /// false일 경우 전페이지
        /// </param>
        private void GetRank(object next)
        {
            //스레드 잠금 (한번에 한번씩만 작동)
            Monitor.Enter(ranklock);
            //결과값을 받을 변수를 string형태로 비워진 변수를 만듬
            var result = "";
            //처음 페이지, 마지막 페이지 구분
            //이전, 다음 구분
            //json 읽기
            //var pages_count = int.Parse(result.Split(new string[] { "\"pages\":" }, StringSplitOptions.None)[1].Split('}')[0]);
            //if (next == null)
            //{
            //    if (pages_count == page)
            //    {
            //        //Invoke = 멀티 스레딩에선 메인 스레드를 건들일 수 없으므로 메인 스레드에서 동작
            //        Invoke((MethodInvoker)delegate () { button2.Enabled = false; });
            //    }
            //    else if (1 == page)
            //    {
            //        Invoke((MethodInvoker)delegate () { button1.Enabled = false; });
            //    }
            //    else
            //    {
            //        Invoke((MethodInvoker)delegate () { button1.Enabled = button2.Enabled = true; });
            //    }
            //}
            //else
            //{
            //    var Next = (bool)next;
            //    if (Next && pages_count - 1 == page)
            //    {
            //        Invoke((MethodInvoker)delegate () { button2.Enabled = false; });
            //        page++;
            //    }
            //    else if (!Next && 2 == page)
            //    {
            //        Invoke((MethodInvoker)delegate () { button1.Enabled = false; });
            //        page--;
            //    }
            //    else if (Next && page != pages_count)
            //    {
            //        Invoke((MethodInvoker)delegate () { button1.Enabled = button2.Enabled = true; });
            //        page++;
            //    }
            //    else if (!Next && page != 1)
            //    {
            //        Invoke((MethodInvoker)delegate () { button1.Enabled = button2.Enabled = true; });
            //        page--;
            //    }
            //}
            //var now_page = int.Parse(result.Split(new string[] { "\"page\":\"" }, StringSplitOptions.None)[1].Split('"')[0]);
            //if (page != 1 && page != pages && next != null)
            //{
            //    return;
            //}
            //데이터 갱신
            //원래는 next 변수를 bool형태로 받아야 하지만 멀티스레딩을 위해서 인자를 넘겨받기 위해 object(모든 변수의 시작, 모든 형태로 변할 수 있음. ex)줄기세포)로 인자를 넘긴다
            //N이라는 변수를 만들어서 아래 (HttpWebRequest)WebRequest.Create($"https://dimiwords.tk:5000/api/list/rank?page={page}"); 부분을 한번만 써도 되도록 만든다
            //var N = 0;
            ////next가 null이 아닐 경우 (처음 로드되는 것이 아닐 경우)
            //if (next != null)
            //{
            //    //true면
            //    if ((bool)next)
            //        //N을 1로 만들어 더해준다
            //        N = 1;
            //    //아니면
            //    else if (!(bool)next)
            //    {
            //        //N을 -1로 만들어 빼준다
            //        N = -1;
            //    }
            //}
            //위에 소스를 만들고 나는 바보라는 것을 깨달았다
            if (next != null)
            {
                //true면
                if ((bool)next)
                    //+ 1을 해준다
                    rankpage++;
                //아니면
                else
                {
                    //- 1을 해준다
                    rankpage--;
                }
            }
            //인터넷에 연결한다 (GET형식이므로 주소에서 데이터를 넘겨준다)
            var req = (HttpWebRequest)WebRequest.Create($"https://dimiwords.tk:5000/api/list/rank?page={rankpage}");
            //데이터 받기
            using (var res = (HttpWebResponse)req.GetResponse())
            {
                //너의 상태가 괜찮아 보이는군!
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    //받을 준비를 할께!
                    using (var resStream = res.GetResponseStream())
                    {
                        //StreamReader로 stream의 데이터를 읽는다
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
            var json = JObject.Parse(result);
            //성공 여부를 파싱
            //var success = result.Split(new string[] { "\"success\":" }, StringSplitOptions.None)[1].Split(',')[0];
            var success = (bool)json["success"];
            //true 또는 false를 가지고 있는 string변수를 bool의 형태로 바꿔준다
            if (success)
            {
                //전체 페이지 수를 파싱
                //var pages_count = int.Parse(result.Split(new string[] { "\"pages\":" }, StringSplitOptions.None)[1].Split('}')[0]);
                var pages_count = (int)json["result"]["pages"];
                //현재 페이지가 1이면 이전 페이지 비활성화
                if (rankpage == 1)
                {
                    Invoke((MethodInvoker)delegate () { button1.Enabled = false; });
                }
                else
                {
                    //현재 페이지가 마지막 페이지면 다음 페이지 비활성화
                    if (rankpage == pages_count)
                    {
                        Invoke((MethodInvoker)delegate () { button2.Enabled = false; });
                    }
                    else
                    {
                        //둘다 아니라면 둘다 활성화
                        Invoke((MethodInvoker)delegate () { button1.Enabled = button2.Enabled = true; });
                    }
                }
                if (rankpage < 1)
                {
                    //page가 1보다 낮아지는 경우는 이전버튼을 연타하여 같은 메서드가 반복될때이다
                    //그러므로 page를 1로 고정하고 아래의 소스들은 반복하지 않는다
                    rankpage = 1;
                    //첫 페이지면 이전 페이지 비활성화
                    Invoke((MethodInvoker)delegate () { button1.Enabled = false; });
                }
                else if (rankpage > pages_count)
                {
                    //위와 같은 경우이다
                    rankpage = pages_count;
                    //마지막 페이지면 다음 페이지 비활성화
                    Invoke((MethodInvoker)delegate () { button2.Enabled = false; });
                }
                else
                {
                    //전체 json 중에 유저 정보만 가져온다
                    //var Userdata = result.Split(new string[] { "\"docs\":[" }, StringSplitOptions.None)[1].Split(']')[0];
                    var Userdata = JArray.FromObject(json["result"]["docs"]);
                    //처음엔 전체 유저 수를 가져온다
                    //var Usercount = int.Parse(result.Split(new string[] { "\"total\":" }, StringSplitOptions.None)[1].Split(',')[0]);
                    var Usercount = (int)json["result"]["total"];
                    //페이지가 마지막 페이지라면
                    if (rankpage == pages_count)
                    {
                        //마지막 페이지의 유저 수를 계산한다
                        Usercount -= (pages_count - 1) * 20;
                    }
                    else
                    {
                        //아니면 그냥 20명
                        Usercount = 20;
                    }
                    //랭킹을 그냥 갱신하면 순차적으로 업데이트 되므로 한번에 업데이트 되도록 BeginUpdate 메서드 사용
                    //페이지 형식이기 때문에 Clear를 사용해 전 페이지는 삭제
                    Invoke((MethodInvoker)delegate () { listView1.BeginUpdate(); listView1.Items.Clear(); });
                    //랭킹 페이지의 유저 수 만큼 반복
                    for (var i = 0; i < Usercount; i++)
                    {
                        //json 읽기
                        var rank = (rankpage - 1) * 20 + i + 1;
                        var name = Userdata[i]["name"].ToString();
                        var intro = Userdata[i]["intro"].ToString();
                        var department = Userdata[i]["department"].ToString();
                        var points = Userdata[i]["points"].ToString();
                        //비어 있는 경우가 있어 null체크
                        var accept = ((int?)Userdata[i]["accept"]).HasValue ? (int)Userdata[i]["accept"] : 0;
                        var submit = ((int?)Userdata[i]["submit"]).HasValue ? (int)Userdata[i]["submit"] : 0;
                        //Userdata = Userdata.Substring(Userdata.IndexOf("\"name\":\"") + 8);
                        //var name = Userdata.Split('"')[0];
                        //Userdata = Userdata.Substring(Userdata.IndexOf("\"intro\":\"") + 9);
                        //var intro = Userdata.Split('"')[0];
                        //Userdata = Userdata.Substring(Userdata.IndexOf("\"department\":") + 13);
                        //var department = Userdata.Split(',')[0];
                        //Userdata = Userdata.Substring(Userdata.IndexOf("\"points\":") + 9);
                        //var points = Userdata.Split(',')[0];
                        //var accept = 0;
                        //var submit = 0;

                        //if (Userdata.Split(new string[] { "\"accept\":" }, StringSplitOptions.None).Length == i + 1)
                        //{
                        //accept, submit 순서 맞춰주기
                        //accept, submit이 없는 경우 (플레이를 안한경우) accept, submit 모두 0
                        //if (Userdata.Contains("\"accept\":") || Userdata.Contains("\"submit\":"))
                        //{
                        //    if (Userdata.Substring(Userdata.IndexOf("\"submit\":") + 9).Split('}')[0].Contains("accept"))
                        //    {
                        //        Userdata = Userdata.Substring(Userdata.IndexOf("\"submit\":") + 9);
                        //        submit = int.Parse(Userdata.Split(',')[0]);
                        //        Userdata = Userdata.Substring(Userdata.IndexOf("\"accept\":") + 9);
                        //        accept = int.Parse(Userdata.Split('}')[0]);
                        //    }
                        //    else if (Userdata.Substring(Userdata.IndexOf("\"accept\":") + 9).Split('}')[0].Contains("submit"))
                        //    {
                        //        Userdata = Userdata.Substring(Userdata.IndexOf("\"accept\":") + 9);
                        //        accept = int.Parse(Userdata.Split(',')[0]);
                        //        Userdata = Userdata.Substring(Userdata.IndexOf("\"submit\":") + 9);
                        //        submit = int.Parse(Userdata.Split('}')[0]);
                        //    }
                        //}
                        //}
                        switch (department)
                        {
                            case "0":
                                department = "EB";
                                break;
                            case "1":
                                department = "DC";
                                break;
                            case "2":
                                department = "WP";
                                break;
                            case "3":
                                department = "HD";
                                break;
                            default:
                                return;
                        }
                        //랭킹 갱신용 변수
                        var item = new ListViewItem(new string[] { rank.ToString(), $"{name} {department}", intro, points, $"{(submit != 0 ? (Math.Round((double)accept / submit * 10000) / 100).ToString() : "100")}%" });
                        Invoke((MethodInvoker)delegate ()
                        {
                            //갱신
                            listView1.Items.Add(item);
                        });
                    }
                    //업데이트가 끝남을 알려 데이터 업데이트를 완료함
                    //label에 현재 페이지를 알려줌
                    Invoke((MethodInvoker)delegate () {  listView1.EndUpdate(); label1.Text = rankpage.ToString(); });
                }
            }
            else
            {
                MessageBox.Show(this, "랭크를 얻어오는데 실패했습니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //잠금 해제
            if (Monitor.IsEntered(ranklock))
                Monitor.Exit(ranklock);
        }

        private void Playwordbooks(object isEn)
        {
            Discord.StateUpdate("단어 외우는 중...");
            //영단어 맞추기
            if ((bool)isEn)
            {
                var result = "";
                var text = "";
                Invoke((MethodInvoker)delegate () { text = listView2.SelectedItems[0].SubItems[5].Text; });
                var id = text;
                //Console.WriteLine(id);
                //var Data = Encoding.UTF8.GetBytes($"{{\"token\":\"{user_data.token}\"}}");
                //인터넷에 연결한다 (GET형식이므로 주소에서 데이터를 넘겨준다)
                var req = (HttpWebRequest)WebRequest.Create($"https://dimiwords.tk:5000/api/get/wordbook/{id}");
                //데이터 받기
                using (var res = (HttpWebResponse)req.GetResponse())
                {
                    //너의 상태가 괜찮아 보이는군!
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        //받을 준비를 할께!
                        using (var resStream = res.GetResponseStream())
                        {
                            //StreamReader로 stream의 데이터를 읽는다
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
                var json = JObject.Parse(result);
                var words = JArray.FromObject(json["wordbook"]["words"]);
                var Wordbooks = new Wordbooks[words.Count];
                for (var i = 0; i < words.Count; i++)
                {
                    Wordbooks[i] = new Wordbooks(JArray.FromObject(words[i]["ko"]).ToObject<string[]>(), words[i]["en"].ToString());
                }
                var ran = new Random();
                Wordbooks = Wordbooks.OrderBy(x => ran.Next()).ToArray();
                for (var i = 0; i < words.Count; i++)
                {
                    var ko = "";
                    for (var ia = 0; ia < Wordbooks[i].ko.Count(); ia++)
                    {
                        var ind = "";
                        if (ia > 0)
                            ind = ", ";
                        ko += $"{ind}{Wordbooks[i].ko[ia]}";
                    }
                    //이 아래부턴 대충 해보는 코딩 (알고리즘 구현)
                    /*
                        bool Skip = false;
                        ///////////////////////////////
                        Skip = false;
                        while (!Skip)
                        {
                            label = ko;
                            if (textBox.Text == en)
                            {
                                progress++;
                                submit++;
                                accept++;
                                break;
                            }
                        }
                        ///////////////////////////////
                        button click
                        {
                            submit++;
                            Skip = true;
                        }
                    */
                    //MessageBox.Show($"한국어 : {ko}\n영어 : {Wordbooks[i].en}");
                }
            }
            //한글 뜻 맞추기
            else
            {

            }
            //단어장 가져오고 단어 랜덤으로 띄우기
        }

        struct Wordbooks
        {
            public string[] ko;
            public string en;

            public Wordbooks(string[] Ko, string En)
            {
                ko = Ko;
                en = En;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //멀티 스레딩
            new Thread(new ParameterizedThreadStart(GetRank)) { IsBackground = true }.Start(false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //멀티 스레딩
            new Thread(new ParameterizedThreadStart(GetRank)) { IsBackground = true }.Start(true);
            //GetRank(true);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Discord.StateUpdate("시간을 버리는 중...");
            //멀티 스레딩
            new Thread(new ParameterizedThreadStart(GetRank)) { IsBackground = true }.Start(null);
            new Thread(new ParameterizedThreadStart(GetWordbooks)) { IsBackground = true }.Start(null);
            //GetRank(null);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    Discord.StateUpdate("단어 살펴 보는 중...");
                    break;
                case 1:
                    Discord.StateUpdate("단어장 살펴 보는 중...");
                    break;
                case 2:
                    Discord.StateUpdate("랭킹 살펴 보는 중...");
                    break;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            new Thread(new ParameterizedThreadStart(GetWordbooks)) { IsBackground = true }.Start(false);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new Thread(new ParameterizedThreadStart(GetWordbooks)) { IsBackground = true }.Start(true);
        }

        private void listView2_ItemActivate(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, $"단어장 \"{listView2.SelectedItems[0].SubItems[1].Text}\"를 공부하시겠습니까?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                new Thread(new ParameterizedThreadStart(Playwordbooks)) { IsBackground = true }.Start(MessageBox.Show(this, $"영단어 맞추기로 공부하시겠습니까?\n(Yes : 영단어 맞추기 No : 한글 뜻 맞추기)", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //단어 추가
        }
    }
}
