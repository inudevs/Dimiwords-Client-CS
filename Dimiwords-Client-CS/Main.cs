using System.Windows.Forms;

namespace Dimiwords_Client_CS
{
    public partial class Main : Form
    {
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
    }
}
