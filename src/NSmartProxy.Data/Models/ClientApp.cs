namespace NSmartProxy.Data
{
    public class ClientApp
    {
        public int AppId { get; set; }
        public string IP { get; set; }
        public int TargetServicePort { get; set; }
        public int ConsumerPort { get; set; }
    }
}
