using System;
using System.Drawing;
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
            var dummy = new ImageList
            {
                ImageSize = new Size(1, 20)
            };
            listView1.SmallImageList = dummy;
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
                    page++;
                //아니면
                else if (!(bool)next)
                {
                    //- 1을 해준다
                    page--;
                }
            }
            //인터넷에 연결한다 (GET형식이므로 주소에서 데이터를 넘겨준다)
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
                        //StreamReader로 stream의 데이터를 읽는다
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
            //성공 여부를 파싱
            var success = result.Split(new string[] { "\"success\":" }, StringSplitOptions.None)[1].Split(',')[0];
            //true 또는 false를 가지고 있는 string변수를 bool의 형태로 바꿔준다
            if (Convert.ToBoolean(success))
            {
                //전체 페이지 수를 파싱
                var pages_count = int.Parse(result.Split(new string[] { "\"pages\":" }, StringSplitOptions.None)[1].Split('}')[0]);
                //현재 페이지가 1이면 이전 페이지 비활성화
                if (page == 1)
                {
                    Invoke((MethodInvoker)delegate () { button1.Enabled = false; });
                }
                else
                {
                    //현재 페이지가 마지막 페이지면 다음 페이지 비활성화
                    if (page == pages_count)
                    {
                        Invoke((MethodInvoker)delegate () { button2.Enabled = false; });
                    }
                    else
                    {
                        //둘다 아니라면 둘다 활성화
                        Invoke((MethodInvoker)delegate () { button1.Enabled = button2.Enabled = true; });
                    }
                }
                if (page < 1)
                {
                    //page가 1보다 낮아지는 경우는 이전버튼을 연타하여 같은 메서드가 반복될때이다
                    //그러므로 page를 1로 고정하고 아래의 소스들은 반복하지 않는다
                    page = 1;
                    //첫 페이지면 이전 페이지 비활성화
                    Invoke((MethodInvoker)delegate () { button1.Enabled = false; });
                }
                else if (page > pages_count)
                {
                    //위와 같은 경우이다
                    page = pages_count;
                    //마지막 페이지면 다음 페이지 비활성화
                    Invoke((MethodInvoker)delegate () { button2.Enabled = false; });
                }
                else
                {
                    //전체 json 중에 유저 정보만 가져온다
                    var Userdata = result.Split(new string[] { "\"docs\":[" }, StringSplitOptions.None)[1].Split(']')[0];
                    //처음엔 전체 유저 수를 가져온다
                    var Usercount = int.Parse(result.Split(new string[] { "\"total\":" }, StringSplitOptions.None)[1].Split(',')[0]);
                    //페이지가 마지막 페이지라면
                    if (page == pages_count)
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
                    for (var i = 1; i <= Usercount; i++)
                    {
                        //json 읽기
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
                        //accept, submit 순서 맞춰주기
                        //accept, submit이 없는 경우 (플레이를 안한경우) accept, submit 모두 0
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
                    Invoke((MethodInvoker)delegate () {  listView1.EndUpdate(); label1.Text = page.ToString(); });
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
            //멀티 스레딩
            new Thread(new ParameterizedThreadStart(GetRank)) { IsBackground = true }.Start(null);
            //GetRank(null);
        }
    }
}
