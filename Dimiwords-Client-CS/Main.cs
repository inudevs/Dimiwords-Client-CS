using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace Dimiwords_Client_CS
{
    public partial class Main : Form
    {
        private int page = 1;

        private User user_data;
        private Login loginform;
        
        public Main(User user, Login login)
        {
            InitializeComponent();
            //유저 정보를 넘겨받는다
            user_data = user;
            //로그인 창을 제대로 종료하기 위해 인자로 넘겨받는다
            loginform = login;
        }

        //창을 껐을때 실행
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //로그인 창은 숨겨두기만 한 것이므로 제대로 종료한다
            loginform.Close();
        }

        //스레드를 한개씩만 돌리기 위한 오브젝트
        private object lockob = new object();

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
            Monitor.Enter(lockob);
            //결과값을 받을 변수를 string형태로 비워진 변수를 만듬
            var result = string.Empty;
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
            var N = 0;
            if (next != null)
            {
                if ((bool)next)
                    N = 1;
                else if (!(bool)next)
                {
                    N = -1;
                }
            }
            page += N;
            var req2 = (HttpWebRequest)WebRequest.Create($"https://dimiwords.tk:5000/api/list/rank?page={page}");
            //데이터 받기
            using (var res2 = (HttpWebResponse)req2.GetResponse())
            {
                //너의 상태가 괜찮아 보이는군!
                if (res2.StatusCode == HttpStatusCode.OK)
                {
                    //받을 준비를 할께!
                    using (var resStream2 = res2.GetResponseStream())
                    {
                        using (var sr2 = new StreamReader(resStream2))
                        {
                            //받았다!
                            result = sr2.ReadToEnd();
                            //다 받았으니 나머지는 정리할께
                            resStream2.Close();
                        }
                    }
                }
                //이것도 정리
                res2.Close();
            }
            var success = result.Split(new string[] { "\"success\":" }, StringSplitOptions.None)[1].Split(',')[0];
            if (Convert.ToBoolean(success))
            {
                var pages_count = int.Parse(result.Split(new string[] { "\"pages\":" }, StringSplitOptions.None)[1].Split('}')[0]);
                if (page == 1)
                {
                    Invoke((MethodInvoker)delegate () { button1.Enabled = false; });
                }
                else
                {
                    if (page == pages_count)
                    {
                        Invoke((MethodInvoker)delegate () { button2.Enabled = false; });
                    }
                    else
                    {
                        Invoke((MethodInvoker)delegate () { button1.Enabled = button2.Enabled = true; });
                    }
                }
                if (page < 1)
                {
                    page = 1;
                    Invoke((MethodInvoker)delegate () { button1.Enabled = false; });
                }
                else if (page > pages_count)
                {
                    page = pages_count;
                    Invoke((MethodInvoker)delegate () { button2.Enabled = false; });
                }
                else
                {
                    var Userdata = result.Split(new string[] { "\"docs\":[" }, StringSplitOptions.None)[1].Split(']')[0];
                    var Usercount = int.Parse(result.Split(new string[] { "\"total\":" }, StringSplitOptions.None)[1].Split(',')[0]);
                    if (page == pages_count)
                    {
                        Usercount -= (pages_count - 1) * 20;
                    }
                    else
                    {
                        Usercount = 20;
                    }
                    Invoke((MethodInvoker)delegate () { listView1.BeginUpdate(); listView1.Items.Clear(); });
                    for (var i = 1; i <= Usercount; i++)
                    {
                        var rank = (page - 1) * 20 + i;
                        Userdata = Userdata.Substring(Userdata.IndexOf("\"name\":\"") + 8);
                        var name = Userdata.Split('"')[0];
                        Userdata = Userdata.Substring(Userdata.IndexOf("\"intro\":\"") + 9);
                        var intro = Userdata.Split('"')[0];
                        Userdata = Userdata.Substring(Userdata.IndexOf("\"department\":") + 13);
                        var department = Userdata.Split(',')[0];
                        Userdata = Userdata.Substring(Userdata.IndexOf("\"points\":") + 9);
                        var points = Userdata.Split(',')[0];
                        var accept = 0;
                        var submit = 0;
                        
                        //if (Userdata.Split(new string[] { "\"accept\":" }, StringSplitOptions.None).Length == i + 1)
                        //{
                        if (Userdata.Contains("\"accept\":") || Userdata.Contains("\"submit\":"))
                        {
                            if (Userdata.Substring(Userdata.IndexOf("\"submit\":") + 9).Split('}')[0].Contains("accept"))
                            {
                                Userdata = Userdata.Substring(Userdata.IndexOf("\"submit\":") + 9);
                                submit = int.Parse(Userdata.Split(',')[0]);
                                Userdata = Userdata.Substring(Userdata.IndexOf("\"accept\":") + 9);
                                accept = int.Parse(Userdata.Split('}')[0]);
                            }
                            else if (Userdata.Substring(Userdata.IndexOf("\"accept\":") + 9).Split('}')[0].Contains("submit"))
                            {
                                Userdata = Userdata.Substring(Userdata.IndexOf("\"accept\":") + 9);
                                accept = int.Parse(Userdata.Split(',')[0]);
                                Userdata = Userdata.Substring(Userdata.IndexOf("\"submit\":") + 9);
                                submit = int.Parse(Userdata.Split('}')[0]);
                            }
                        }
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
                        var item = new ListViewItem(new string[] { rank.ToString(), name + department, intro, points, $"{(submit != 0 ? (Math.Round((double)accept / submit * 10000) / 100).ToString() : "100")}%" });
                        Invoke((MethodInvoker)delegate ()
                        {
                            listView1.Items.Add(item);
                        });
                    }
                    Invoke((MethodInvoker)delegate () {  listView1.EndUpdate(); });
                }
            }
            else
            {
                MessageBox.Show(this, "랭크를 얻어오는데 실패했습니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //잠금 해제
            if (Monitor.IsEntered(lockob))
                Monitor.Exit(lockob);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                //멀티 스레딩
                new Thread(new ParameterizedThreadStart(GetRank)) { IsBackground = true }.Start(null);
                //GetRank(null);
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
    }
}
