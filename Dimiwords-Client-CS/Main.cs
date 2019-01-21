using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Dimiwords_Client_CS
{
    public partial class Main : Form
    {
        private int rankpage = 1, wordbookspage = 1, wordspage = 1;
        private List<WordSchema> wordsList = new List<WordSchema>();
        private User user_data;

        //스레드를 한개씩만 돌리기 위한 오브젝트
        private object ranklock = new object(), wordslock = new object(), wordbookslock = new object();

        public Main(User user)
        {
            InitializeComponent();
            //유저 정보를 넘겨받는다
            user_data = user;
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
            listView2.SmallImageList = listView3.SmallImageList = dummy2;
        }

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
            var success = (bool)json["success"];
            if (success)
            {
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
                else if (wordbookspage > pages_count)
                {
                    //위와 같은 경우이다
                    wordbookspage = pages_count;
                    //마지막 페이지면 다음 페이지 비활성화
                    Invoke((MethodInvoker)delegate () { button4.Enabled = false; });
                }
                else
                {
                    var Booksdata = JArray.FromObject(json["result"]["docs"]);
                    //처음엔 전체 단어장 수를 가져온다
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
                    //단어장 페이지의 단어장 수 만큼 반복
                    for (var i = 0; i < Bookscount; i++)
                    {
                        var name = Booksdata[i]["name"].ToString();
                        var intro = Booksdata[i]["intro"].ToString();
                        var wordscount = Booksdata[i]["len"].ToString();
                        var user = Booksdata[i]["user"].ToString();
                        var id = Booksdata[i]["_id"].ToString();
                        var item = new ListViewItem(new string[] { "    □    ", string.IsNullOrEmpty(name) ? "제목없음" : name, string.IsNullOrEmpty(intro) ? "설명없음" : intro, wordscount, user, id });
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
            var success = (bool)json["success"];
            //true 또는 false를 가지고 있는 string변수를 bool의 형태로 바꿔준다
            if (success)
            {
                //전체 페이지 수를 파싱
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
                    var Userdata = JArray.FromObject(json["result"]["docs"]);
                    //처음엔 전체 유저 수를 가져온다
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
                    Invoke((MethodInvoker)delegate () { listView1.EndUpdate(); label1.Text = rankpage.ToString(); });
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

        private void GetWords(object next)
        {
            Monitor.Enter(wordslock);
            Invoke((MethodInvoker)delegate () { button9.Enabled = false; });
            var result = "";
            //처음 페이지, 마지막 페이지 구분
            //이전, 다음 구분
            //json 읽기
            if (next != null)
            {
                //true면
                if ((bool)next)
                    //+ 1을 해준다
                    wordspage++;
                //아니면
                else
                {
                    //- 1을 해준다
                    wordspage--;
                }
            }
            //인터넷에 연결한다 (GET형식이므로 주소에서 데이터를 넘겨준다)
            var req = (HttpWebRequest)WebRequest.Create($"https://dimiwords.tk:5000/api/list/words?page={wordspage}");
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
            var success = (bool)json["success"];
            //true 또는 false를 가지고 있는 string변수를 bool의 형태로 바꿔준다
            if (success)
            {
                //전체 페이지 수를 파싱
                var pages_count = (int)json["result"]["pages"];
                //현재 페이지가 1이면 이전 페이지 비활성화
                if (wordspage == 1)
                {
                    Invoke((MethodInvoker)delegate () { button7.Enabled = false; });
                }
                else
                {
                    //현재 페이지가 마지막 페이지면 다음 페이지 비활성화
                    if (wordspage == pages_count)
                    {
                        Invoke((MethodInvoker)delegate () { button6.Enabled = false; });
                    }
                    else
                    {
                        //둘다 아니라면 둘다 활성화
                        Invoke((MethodInvoker)delegate () { button7.Enabled = button6.Enabled = true; });
                    }
                }
                if (wordspage < 1)
                {
                    //page가 1보다 낮아지는 경우는 이전버튼을 연타하여 같은 메서드가 반복될때이다
                    //그러므로 page를 1로 고정하고 아래의 소스들은 반복하지 않는다
                    wordspage = 1;
                    //첫 페이지면 이전 페이지 비활성화
                    Invoke((MethodInvoker)delegate () { button7.Enabled = false; });
                }
                else if (wordspage > pages_count)
                {
                    //위와 같은 경우이다
                    wordspage = pages_count;
                    //마지막 페이지면 다음 페이지 비활성화
                    Invoke((MethodInvoker)delegate () { button6.Enabled = false; });
                }
                else
                {
                    //전체 json 중에 단어 정보만 가져온다
                    var Wordsdata = JArray.FromObject(json["result"]["docs"]);
                    //처음엔 전체 단어 수를 가져온다
                    var Wordscount = (int)json["result"]["total"];
                    //페이지가 마지막 페이지라면
                    if (wordspage == pages_count)
                    {
                        //마지막 페이지의 단어 수를 계산한다
                        Wordscount -= (pages_count - 1) * 9;
                    }
                    else
                    {
                        //아니면 그냥 9개
                        Wordscount = 9;
                    }
                    //단어를 그냥 갱신하면 순차적으로 업데이트 되므로 한번에 업데이트 되도록 BeginUpdate 메서드 사용
                    //페이지 형식이기 때문에 Clear를 사용해 전 페이지는 삭제
                    Invoke((MethodInvoker)delegate () { listView3.BeginUpdate(); listView3.Items.Clear(); });
                    var check = new List<int>();
                    //단어 페이지의 단어 수 만큼 반복
                    for (var i = 0; i < Wordscount; i++)
                    {
                        //json 읽기
                        var ko = string.Join(", ", JArray.FromObject(Wordsdata[i]["ko"]).ToObject<string[]>());
                        var en = Wordsdata[i]["en"].ToString();
                        var userid = Wordsdata[i]["userId"].ToString();
                        //단어 갱신용 변수
                        var item = new ListViewItem(new string[] { en, ko, userid });
                        var wordsArray = wordsList.ToArray();
                        Invoke((MethodInvoker)delegate ()
                        {
                            //갱신
                            listView3.Items.Add(item);
                            if (wordsArray.ContainsEx(new WordSchema(en, ko.Split(new string[] { ", " }, StringSplitOptions.None), userid, 0, 0)))
                            {
                                check.Add(i);
                            }
                        });
                    }
                    //업데이트가 끝남을 알려 데이터 업데이트를 완료함
                    //label에 현재 페이지를 알려줌
                    Invoke((MethodInvoker)delegate ()
                    {
                        var check_arr = check.ToArray();
                        for (var i = 0; i < check_arr.Count(); i++)
                        {
                            listView3.Items[check_arr[i]].Checked = true;
                        }
                        listView3.EndUpdate();
                        label2.Text = wordspage.ToString();
                    });
                }
            }
            if (Monitor.IsEntered(wordslock))
                Monitor.Exit(wordslock);
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
                    Wordbooks[i] = new Wordbooks(JArray.FromObject(words[i]["ko"]).ToObject<string[]>(), words[i]["en"].ToString(), words[i]["_id"].ToString());
                }
                var ran = new Random();
                Wordbooks = Wordbooks.OrderBy(x => ran.Next()).ToArray();
                Invoke((MethodInvoker)delegate () { new Learn(Wordbooks, this, user_data).Show(); Hide(); });
            }
            //한글 뜻 맞추기
            else
            {

            }
        }

        private void SearchWords()
        {
            if (!(textBox1.Text == "" || textBox1.Text == "영문으로 단어를 검색해보세요." || textBox1.Text.Contains("\\") || textBox1.Text.Contains(" ")))
            {
                Invoke((MethodInvoker)delegate () { button9.Enabled = true; });
                //결과값 변수를 비어져 있는 string자료형으로 선언
                var result = "";
                //json형태로 Byte[]자료형 선언
                var Data = Encoding.UTF8.GetBytes(new JObject
                {
                    { "token", user_data.token },
                    { "query", textBox1.Text }
                }.ToString());
                //로그인 서버
                var req = (HttpWebRequest)WebRequest.Create("https://dimiwords.tk:5000/api/search/words");
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
                    //전체 json 중에 단어 정보만 가져온다
                    var Wordsdata = JArray.FromObject(json["result"]["docs"]);
                    //처음엔 단어 수를 가져온다
                    var Wordscount = (int)json["result"]["total"];
                    if (Wordscount > 9)
                    {
                        Wordscount = 9;
                    }
                    //단어를 그냥 갱신하면 순차적으로 업데이트 되므로 한번에 업데이트 되도록 BeginUpdate 메서드 사용
                    //페이지 형식이기 때문에 Clear를 사용해 전 페이지는 삭제
                    Invoke((MethodInvoker)delegate () { listView3.BeginUpdate(); listView3.Items.Clear(); });
                    var check = new List<int>();
                    //단어 페이지의 단어 수 만큼 반복
                    for (var i = 0; i < Wordscount; i++)
                    {
                        //json 읽기
                        var ko = string.Join(", ", JArray.FromObject(Wordsdata[i]["ko"]).ToObject<string[]>());
                        var en = Wordsdata[i]["en"].ToString();
                        var userid = Wordsdata[i]["userId"].ToString();
                        //단어 갱신용 변수
                        var item = new ListViewItem(new string[] { en, ko, userid });
                        var wordsArray = wordsList.ToArray();
                        Invoke((MethodInvoker)delegate ()
                        {
                            //갱신
                            listView3.Items.Add(item);
                            if (wordsArray.ContainsEx(new WordSchema(en, ko.Split(new string[] { ", " }, StringSplitOptions.None), userid, 0, 0)))
                            {
                                check.Add(i);
                            }
                        });
                    }
                    //업데이트가 끝남을 알려 데이터 업데이트를 완료함
                    //label을 비움
                    Invoke((MethodInvoker)delegate ()
                    {
                        var check_arr = check.ToArray();
                        for (var i = 0; i < check_arr.Count(); i++)
                        {
                            listView3.Items[check_arr[i]].Checked = true;
                        }
                        listView3.EndUpdate();
                        label2.Text = "";
                    });
                }
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
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Discord.StateUpdate("시간을 버리는 중...");
            //멀티 스레딩
            new Thread(new ParameterizedThreadStart(GetRank)) { IsBackground = true }.Start(null);
            new Thread(new ParameterizedThreadStart(GetWordbooks)) { IsBackground = true }.Start(null);
            new Thread(new ParameterizedThreadStart(GetWords)) { IsBackground = true }.Start(null);
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

        private void button7_Click(object sender, EventArgs e)
        {
            new Thread(new ParameterizedThreadStart(GetWords)) { IsBackground = true }.Start(false);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            new Thread(new ParameterizedThreadStart(GetWords)) { IsBackground = true }.Start(true);
        }

        private void listView3_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var en = e.Item.SubItems[0].Text;
            var ko = e.Item.SubItems[1].Text.Split(new string[] { ", " }, StringSplitOptions.None);
            var page = wordspage.ToString();
            var wordsArray = wordsList.ToArray();
            if (wordsArray.ContainsEx(new WordSchema(en, ko, e.Item.SubItems[2].Text, 0, 0)) && !e.Item.Checked)
            {
                wordsList.Remove(new WordSchema(en, ko, e.Item.SubItems[2].Text, 0, 0));
            }
            else if (!wordsArray.ContainsEx(new WordSchema(en, ko, e.Item.SubItems[2].Text, 0, 0)) && e.Item.Checked)
            {
                wordsList.Add(new WordSchema(en, ko, e.Item.SubItems[2].Text, 0, 0));
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "영문으로 단어를 검색해보세요.")
            {
                textBox1.Text = "";
                textBox1.ForeColor = SystemColors.WindowText;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                textBox1.Text = "영문으로 단어를 검색해보세요.";
                textBox1.ForeColor = SystemColors.GrayText;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            new Thread(SearchWords) { IsBackground = true }.Start();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //엔터 키를 누르면 검색 버튼이 자동으로 눌리게 만듬
            if (e.KeyChar == (char)Keys.Enter)
            {
                button8.PerformClick();
                //이게 없으면 엔터를 누르면 에러 소리가 난다
                e.Handled = true;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            new Thread(new ParameterizedThreadStart(GetWords)) { IsBackground = true }.Start(null);
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
            if (wordsList.Count > 0)
            {
                Hide();
                new CreateWordbook(wordsList.ToArray(), user_data, this).Show();
            }
        }
    }

    public static class ListEx
    {
        public static bool ContainsEx(this WordSchema[] list, WordSchema wordSchema)
        {
            for (var i = 0; i < list.Count(); i++)
            {
                if (list[i].en == wordSchema.en && string.Join(", ", list[i].ko) == string.Join(", ", wordSchema.ko) && list[i].userid == wordSchema.userid)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
