﻿


using Opc.UaFx.Client;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("C:\\Users\\lazar\\Desktop\\mrgadaV2\\Mrgada\\logs\\log-.txt", rollingInterval: RollingInterval.Hour)
    .MinimumLevel.Debug()
    .CreateLogger();

Mrgada.Initialize("192.168.64.107", Mrgada.MachineType.Server, 61101, 200);

Mrgada.MRP6 MRP6 = new("MRP6", Mrgada.AcquisitorType.S71500, "192.168.64.177", 61102, 200);
Mrgada.Acquisitor APISKID = new("APISKID", Mrgada.AcquisitorType.S71500, "192.168.64.188", 61103, 200);
Mrgada.CODESYSPLC CODESYSPLC = new("CODESYSPLC", Mrgada.AcquisitorType.OPCUA, "192.168.64.107", 61104, 200);

MRP6.StartAcquisition();
APISKID.StartAcquisition();
CODESYSPLC.StartAcquisition();

Console.ReadLine();