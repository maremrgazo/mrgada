#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using Serilog;

public static partial class Mrgada
{
    private static string _ServerIp;
    private static Mrgada.MachineType _MachineType;
    private static bool _IsInitialized = false;
    private static int _MrgadaServerPort;
    private static int _MrgadaMainThreadSleep;

    public enum MachineType    
    {
        Server,
        Client
    }

    public static void Initialize(string ServerIp, Mrgada.MachineType MachineType, int MrgadaServerPort, int MrgadaMainThreadSleep)
    {
        Mrgada._MachineType = MachineType;
        Mrgada._ServerIp = ServerIp;
        Mrgada._IsInitialized = true;
        Mrgada._MrgadaServerPort = MrgadaServerPort;
        Mrgada._MrgadaMainThreadSleep = MrgadaMainThreadSleep;

        switch (Mrgada._MachineType)
        {

            case Mrgada.MachineType.Server:

                MrgadaServerServiceInit();

                break;

            case Mrgada.MachineType.Client:

                MrgadaClientServiceInit();

                break;

        }
    }

    public static void Start()
    {
        Log.Information("Mrgada Started");
    }
}

