namespace Dimiwords_Client_CS
{
    //유저 정보를 담는 자료형을 만듬
    public struct User
    {
        public string name, intro, email, department, points, submit, accept, token;

        public User(string Name, string Intro, string Email, string Department, string Points, string Submit, string Accept, string Token)
        {
            name = Name;
            intro = Intro;
            email = Email;
            department = Department;
            points = Points;
            submit = Submit;
            accept = Accept;
            token = Token;
        }
    }
}
