using System.Text.Json;

public static partial class Mrgada
{
    public static bool MRP6Status = false;
    public static bool APISKIDStatus = false;
    public static bool CODESYSPPLCStatus = false;
    public static DateTime ServerDateTime = DateTime.Now;

    public static void OnBroadcastRecieved(byte[] Bytes)
    {
        //MRP6Status = status;
    }
    public static class Serializer
    {
        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
