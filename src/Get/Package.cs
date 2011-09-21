namespace Get
{
    public class Package
    {
        public Package(string id, string version)
        {
            Id = id;
            Version = version;
        }

        public string Id { get; set; }
        public string Version { get; set; }
    }
}