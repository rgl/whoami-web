namespace whoami.Models
{
    public class IntegratedAuthenticationModel
    {
        public class Group
        {
            public string Name;
            public string Sid;
        }
        public string Name;
        public string Sid;
        public string ImpersonationLevel;
        public Group[] Groups;
        public string Whoami;
    }
}
