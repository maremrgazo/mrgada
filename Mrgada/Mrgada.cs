#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

public static partial class Mrgada
{
    private static string _ServerIp;
    private static Mrgada.MachineType _MachineType;
    private static bool _IsInitialized = false;

    public enum MachineType    
    {
        Server,
        Client
    }

    public static void Initialize(string ServerIp, Mrgada.MachineType MachineType)
    {
        Mrgada._MachineType = MachineType;
        Mrgada._ServerIp = ServerIp;

        Mrgada._IsInitialized = true;
    }

    public static void Start()
    {
        Console.WriteLine("Mrgada Started");
    }
}

