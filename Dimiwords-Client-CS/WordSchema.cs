namespace Dimiwords_Client_CS
{
    public struct WordSchema
    {
        public string en, userid;
        public string[] ko;
        public int accept, submit;

        public WordSchema(string En, string[] Ko, string Userid, int Accept, int Submit)
        {
            en = En;
            ko = Ko;
            userid = Userid;
            accept = Accept;
            submit = Submit;
        }
    }
}
