


using Opc.UaFx.Client;

Mrgada.Initialize("192.168.64.121", Mrgada.MachineType.Client);


Mrgada.MRP6 MRP6 = new("MRP6", Mrgada.AcquisitorType.S71500, "192.168.64.177", 61100, 200);
Mrgada.Acquisitor APISKID = new("APISKID", Mrgada.AcquisitorType.S71500, "192.168.64.188", 61101, 200);
Mrgada.CODESYSPLC CODESYSPLC = new("CODESYSPLC", Mrgada.AcquisitorType.OPCUA, "192.168.64.107", 61102, 200);

MRP6.StartAcquisition();
APISKID.StartAcquisition();
CODESYSPLC.StartAcquisition();

Console.ReadLine();