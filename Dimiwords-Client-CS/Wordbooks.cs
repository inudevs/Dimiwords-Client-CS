namespace Dimiwords_Client_CS
{
    public struct Wordbooks
    {
        public string[] ko;
        public string en, id;

        public Wordbooks(string[] Ko, string En, string Id)
        {
            ko = Ko;
            en = En;
            id = Id;
        }
    }
}
