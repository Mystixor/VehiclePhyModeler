using GBX.NET;
using GBX.NET.Engines.Plug;
//using GBX.NET.LZO;
using System.Numerics;
//using System.Reflection.Metadata.Ecma335;
using System.Text.Json;


namespace VPM
{
    public class SurfaceData
    {
        public string? Name { get; set; }
        public Dictionary<string, float>? Position { get; set; }
        public Dictionary<string, float>? Rotation { get; set; }
        public string? Type { get; set; }
        public Dictionary<string, float>? Parameters { get; set; }
    }

    public class Axle
    {
        public float PositionZ { get; set; }
        public float WheelPositionX { get; set; }
        public float WheelRadius { get; set; }
        public float WheelWidthHalf { get; set; }
        public Dictionary<string, bool>? FlagsLeft { get; set; }
        public Dictionary<string, bool>? FlagsRight { get; set; }
    }

    public class Chassis
    {
        public float GroundHeight { get; set; }
        public Axle? AxleFront { get; set; }
        public Axle? AxleRear { get; set; }
    }

    public class VehiclePhyModelData
    {
        public string? Version { get; set; }
        public Chassis? Chassis { get; set; }
        public SurfaceData[]? BodySurfs { get; set; }
    }

    public enum WheelIndex : int
    {
        FrontLeft,
        FrontRight,
        RearRight,
        RearLeft
    }


    public class Program
    {
        public const string VERSION = "0.0.2";

        static void Main(string[] args)
        {
            //  Checking for user error
            if (args.Length == 0)
            {
                System.Console.WriteLine("Please drag a file on this executable or start it in the command line, e.g.");
                System.Console.WriteLine("\n\t\"VehiclePhyModeler.exe CanyonCar.VehiclePhyModel.Gbx\"\n");
                System.Console.WriteLine("\n\t\"VehiclePhyModeler.exe ValleyCar.VehiclePhyModel.json StadiumCar.VehiclePhyModel.Gbx\"\n");

                return;
            }

            // Reading GBX file
            GBX.NET.Lzo.SetLzo(typeof(GBX.NET.LZO.MiniLZO));

            try
            {
                if (args.Length == 1)
                {
                    GBX.NET.Node? vpm = GameBox.ParseNode(args[0]);

                    if (vpm is CPlugVehiclePhyModel vehiclePhyModel)
                    {
                        CPlugVehicleCarPhyShape? ps = vehiclePhyModel.PhyShape;
                        if (ps is not null)
                        {
                            if (ps.Chunk0910E000u is Chunk0910E000U phyShape)
                            {
                                CPlugSurface? surface = phyShape.U03;
                                CPlugVehicleWheelPhyModel[]? wheels = phyShape.U04;

                                List<Tuple<bool, string?>> surfs = new List<Tuple<bool, string?>>();

                                Chassis chassis = new Chassis
                                {
                                    GroundHeight = phyShape.U07,
                                    AxleFront = new Axle
                                    {
                                        PositionZ = phyShape.U08,
                                        WheelPositionX = phyShape.U10,
                                        WheelRadius = phyShape.U12,
                                        WheelWidthHalf = phyShape.U14
                                    },
                                    AxleRear = new Axle
                                    {
                                        PositionZ = phyShape.U09,
                                        WheelPositionX = phyShape.U11,
                                        WheelRadius = phyShape.U13,
                                        WheelWidthHalf = phyShape.U15
                                    }
                                };

                                List<SurfaceData> wheelDatas = new List<SurfaceData>();
                                if (wheels is not null)
                                {
                                    for (int i = 0; i < wheels.Length; i++)
                                    {
                                        Dictionary<string, bool> flags = new Dictionary<string, bool>
                                        {
                                            ["IsDriving"] =     wheels[i].IsDriving,
                                            ["IsSteering"] =    wheels[i].IsSteering
                                        };

                                        SurfaceData wheelSurf = new SurfaceData
                                        {
                                            Name = wheels[i].Id,
                                            Type = "Ellipsoid"
                                        };

                                        switch ((WheelIndex)i)
                                        {
                                            case WheelIndex.FrontLeft:
                                                chassis.AxleFront.FlagsLeft = flags;

                                                wheelSurf.Position = new Dictionary<string, float>
                                                {
                                                    ["x"] = chassis.AxleFront.WheelPositionX,
                                                    ["y"] = chassis.GroundHeight + chassis.AxleFront.WheelRadius,
                                                    ["z"] = chassis.AxleFront.PositionZ
                                                };
                                                wheelSurf.Parameters = new Dictionary<string, float>
                                                {
                                                    ["x"] = chassis.AxleFront.WheelWidthHalf,
                                                    ["y"] = chassis.AxleFront.WheelRadius,
                                                    ["z"] = chassis.AxleFront.WheelRadius
                                                };
                                                break;
                                            case WheelIndex.FrontRight:
                                                chassis.AxleFront.FlagsRight = flags;

                                                wheelSurf.Position = new Dictionary<string, float>
                                                {
                                                    ["x"] = -chassis.AxleFront.WheelPositionX,
                                                    ["y"] = chassis.GroundHeight + chassis.AxleFront.WheelRadius,
                                                    ["z"] = chassis.AxleFront.PositionZ
                                                };
                                                wheelSurf.Parameters = new Dictionary<string, float>
                                                {
                                                    ["x"] = chassis.AxleFront.WheelWidthHalf,
                                                    ["y"] = chassis.AxleFront.WheelRadius,
                                                    ["z"] = chassis.AxleFront.WheelRadius
                                                };
                                                break;
                                            case WheelIndex.RearRight:
                                                chassis.AxleRear.FlagsRight = flags;

                                                wheelSurf.Position = new Dictionary<string, float>
                                                {
                                                    ["x"] = -chassis.AxleRear.WheelPositionX,
                                                    ["y"] = chassis.GroundHeight + chassis.AxleRear.WheelRadius,
                                                    ["z"] = chassis.AxleRear.PositionZ
                                                };
                                                wheelSurf.Parameters = new Dictionary<string, float>
                                                {
                                                    ["x"] = chassis.AxleRear.WheelWidthHalf,
                                                    ["y"] = chassis.AxleRear.WheelRadius,
                                                    ["z"] = chassis.AxleRear.WheelRadius
                                                };
                                                break;
                                            case WheelIndex.RearLeft:
                                                chassis.AxleRear.FlagsLeft = flags;

                                                wheelSurf.Position = new Dictionary<string, float>
                                                {
                                                    ["x"] = chassis.AxleRear.WheelPositionX,
                                                    ["y"] = chassis.GroundHeight + chassis.AxleRear.WheelRadius,
                                                    ["z"] = chassis.AxleRear.PositionZ
                                                };
                                                wheelSurf.Parameters = new Dictionary<string, float>
                                                {
                                                    ["x"] = chassis.AxleRear.WheelWidthHalf,
                                                    ["y"] = chassis.AxleRear.WheelRadius,
                                                    ["z"] = chassis.AxleRear.WheelRadius
                                                };
                                                break;
                                        }

                                        wheelDatas.Add(wheelSurf);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("\nChunk0910E000.U04 (CPlugVehicleWheelPhyModel) is null.");
                                }

                                List<SurfaceData> bodyDatas = [];
                                if (surface is not null)
                                {
                                    CPlugSkel? skel = surface.Skel;
                                    CPlugSurface.ISurf? surf = surface.Surf;

                                    if (skel is not null)
                                    {
                                        string? name = skel.Name;

                                        CPlugSkel.Joint[] joints = skel.Joints;

                                        Console.WriteLine("\nSkeleton \"" + (name is not null ? name : "\\null") + "\":");

                                        for (int i = 0; i < joints.Length; i++)
                                        {
                                            string? jointName = joints[i].Name;
                                            short parentIndex = joints[i].ParentIndex;
                                            GBX.NET.Quat? rotation = joints[i].GlobalJoint;
                                            GBX.NET.Vec3? position = joints[i].U01;
                                            GBX.NET.Iso4? transform = joints[i].U02;

                                            Console.WriteLine("\nJoint \"" + (jointName is not null ? jointName : "\\null") + "\":");
                                            Console.WriteLine("parentIndex\t\t" + parentIndex);
                                            Console.WriteLine("rotation (quat.)\t" + (rotation is not null ? rotation : "\\null"));
                                            Console.WriteLine("position\t\t" + (position is not null ? position : "\\null"));
                                            Console.WriteLine("transform\t\t" + (transform is not null ? transform : "\\null"));

                                            bool isWheel = false;
                                            for (int j = 0; j < wheelDatas.Count; j++)
                                            {
                                                if (wheelDatas[j].Name == jointName)
                                                {
                                                    isWheel = true;
                                                    break;
                                                }
                                            }
                                            if (!isWheel)
                                            {
                                                bodyDatas.Add(new()
                                                {
                                                    Name = jointName,
                                                    Position = (transform.HasValue ? new Dictionary<string, float>
                                                    {
                                                        ["x"] = transform.Value.TX,
                                                        ["y"] = transform.Value.TY,
                                                        ["z"] = transform.Value.TZ
                                                    } : null),
                                                    Rotation = (rotation.HasValue ? new Dictionary<string, float>
                                                    {
                                                        ["x"] = rotation.Value.X,
                                                        ["y"] = rotation.Value.Y,
                                                        ["z"] = rotation.Value.Z,
                                                        ["w"] = rotation.Value.W
                                                    } : null)
                                                });
                                            }

                                            surfs.Add(new Tuple<bool, string?>(isWheel, jointName));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("\nNo CPlugSkel found.");
                                    }

                                    if (surf is not null)
                                    {
                                        int parseSurface(CPlugSurface.ISurf surface, int surfIndex)
                                        {
                                            int surfId = surface.Id;
                                            Console.WriteLine("Surface is " + surface.ToString() + " (Id = " + surfId + ").");

                                            switch (surface)
                                            {
                                                case CPlugSurface.Compound compound:
                                                    CPlugSurface.ISurf[] surfaces = compound.Surfaces;
                                                    GBX.NET.Vec3? compoundU01 = compound.U01;
                                                    GBX.NET.Iso4[] transforms = compound.U02;
                                                    ushort[] compoundU03 = compound.U03;

                                                    Console.WriteLine("Compound.U01 (Vec3)\t" + (compoundU01 is not null ? compoundU01 : "\\null"));

                                                    Console.WriteLine("\n" + surfaces.Length + " surfaces found in Surface Compound.");
                                                    for (int i = 0; i < surfaces.Length; i++)
                                                    {
                                                        Console.WriteLine("\nCompound surface #" + i + ":");
                                                        surfIndex = parseSurface(surfaces[i], surfIndex + 1);
                                                    }
                                                    for (int i = 0; i < transforms.Length; i++)
                                                    {
                                                        Console.WriteLine("\nTransform #" + i + ":");
                                                        Console.WriteLine(transforms[i]);
                                                    }
                                                    for (int i = 0; i < compoundU03.Length; i++)
                                                    {
                                                        Console.WriteLine("\nCompound.U03[" + i + "]:");
                                                        Console.WriteLine(compoundU03[i]);
                                                    }
                                                    break;

                                                case CPlugSurface.Ellipsoid ellipsoid:
                                                    Vec3 size = ellipsoid.Size;
                                                    Vec3? ellipsoidU01 = ellipsoid.U01;
                                                    ushort ellipsoidU02 = ellipsoid.U02;

                                                    Console.WriteLine("Size\t\t\t" + size);
                                                    Console.WriteLine("Ellipsoid.U01 (Vec3)\t" + (ellipsoidU01 is not null ? ellipsoidU01 : "\\null"));
                                                    Console.WriteLine("Ellipsoid.U02 (ushort)\t" + ellipsoidU02);

                                                    bool isWheel = surfs[surfIndex].Item1;
                                                    string? surfName = surfs[surfIndex].Item2;
                                                    if (isWheel)
                                                    {
                                                        break;  //  Wheel data here is duplicate data, seemingly taking no effect.
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < bodyDatas.Count; i++)
                                                        {
                                                            if (bodyDatas[i].Name == surfName)
                                                            {
                                                                bodyDatas[i].Type = "Ellipsoid";
                                                                bodyDatas[i].Parameters = new Dictionary<string, float>
                                                                {
                                                                    ["x"] = size.X,
                                                                    ["y"] = size.Y,
                                                                    ["z"] = size.Z
                                                                };
                                                                break;  //  Each name should only occur once, otherwise I'm already running into deeper trouble...
                                                            }
                                                        }
                                                    }

                                                    break;

                                                default:
                                                    Console.WriteLine("Parsing of this surface type is not yet implemented.");
                                                    break;
                                            }

                                            return surfIndex;
                                        }

                                        Console.WriteLine("\nCPlugSurface.Surf:");
                                        parseSurface(surf, -1); // This surfIndex = -1 assumes that the first Surface will ALWAYS be a Compound. This way, when parseSurface(... , surfIndex + 1) is called for the FIRST Surface of this Compound, it passes surfIndex = 0.
                                    }
                                    else
                                    {
                                        Console.WriteLine("\nCPlugSurface.Surf (CPlugSurface.ISurf) is null.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("\nChunk0910E000.U03 (CPlugSurface) is null.");
                                }

                                VehiclePhyModelData vehiclePhyModelData = new VehiclePhyModelData
                                {
                                    Version = VERSION,
                                    BodySurfs = bodyDatas.ToArray(),
                                    Chassis = chassis
                                    //WheelSurfs = wheelDatas.ToArray()
                                };

                                JsonSerializerOptions? jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                                string jsonString = JsonSerializer.Serialize(vehiclePhyModelData, jsonOptions);

                                Console.WriteLine("\n\n" + jsonString);
                                Console.WriteLine("\nSuccessfully parsed GBX-VehiclePhyModel and serialized as JSON-VehiclePhyModel. Saving JSON to disk...");

                                File.WriteAllText(args[0].Replace(".gbx", ".json", true, null), jsonString);
                            }
                            else
                            {
                                Console.WriteLine("\nNo CPlugVehicleCarPhyShape.Chunk0910E000 found.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nCPlugVehiclePhyModel.PhyShape is null.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nSupplied file is not a CPlugVehiclePhyModel.");
                    }
                }
                else if (args.Length == 2)
                {
                    try
                    {
                        bool firstIsJSON = args[0].EndsWith(".json", true, null);
                        string jsonString = File.ReadAllText(args[firstIsJSON ? 0 : 1]);
                        VehiclePhyModelData? vehiclePhyModelData = JsonSerializer.Deserialize<VehiclePhyModelData>(jsonString);

                        if (vehiclePhyModelData is not null)
                        {
                            Console.WriteLine("\nSuccessfully deserialized JSON-VehiclePhyModel. Building GBX-VehiclePhyModel...");

                            if(vehiclePhyModelData.Version != VERSION)
                            {
                                Console.WriteLine("\nThis is VehiclePhyModeler v" + VERSION + ", supplied file is v" +  vehiclePhyModelData.Version + ". Canceling build.");
                                return;
                            }

                            try
                            {
                                GBX.NET.Node? vpm = GameBox.ParseNode(args[firstIsJSON ? 1 : 0]);

                                if (vpm is CPlugVehiclePhyModel vehiclePhyModel)
                                {
                                    //CPlugVehicleCarPhyShape? ps = vehiclePhyModel.PhyShape;
                                    if (vehiclePhyModel.PhyShape is CPlugVehicleCarPhyShape ps)
                                    {
                                        // CPlugVehicleCarPhyShape.Chunk0910E000? phyShape = ps.GetChunk<CPlugVehicleCarPhyShape.Chunk0910E000>();

                                        if (ps.chunk0910E000u is Chunk0910E000U phyShape)
                                        {
                                            //CPlugSurface? surface = phyShape.U03;
                                            if (phyShape.U03 is CPlugSurface surface)
                                            {
                                                //  Building the skeleton   //////////////////////////////////////

                                                List<Tuple<bool, string?>> surfs = [];
                                                List<SurfaceData> bodyDatas = [];
                                                if (surface.Skel is CPlugSkel skel)     //  This is required to be true! CPlugSkel cannot be initialized so if the base input file have surface.Skel == null it will not work.
                                                {
                                                    skel.Name = "";

                                                    List<CPlugSkel.Joint> jointList = [];
                                                    List<Iso4> iso4List = [];
                                                    List<ushort> ushortList = [];

                                                    CPlugSkel.Joint getJoint(SurfaceData body)
                                                    {
                                                        CPlugSkel.Joint joint = new CPlugSkel.Joint();

                                                        joint.Name = body.Name;
                                                        joint.ParentIndex = -1; // Potentially different for Compounds of Compounds?

                                                        Quat rotation = new();
                                                        Vec3 position = new();
                                                        if (body.Rotation is Dictionary<string, float> rotDict)
                                                        {
                                                            try
                                                            {
                                                                rotation = new Quat(rotDict["x"], rotDict["y"], rotDict["z"], rotDict["w"]);
                                                            }
                                                            catch (KeyNotFoundException knfe)
                                                            {
                                                                rotation = new Quat(0.0f, 0.0f, 0.0f, 1.0f);
                                                            }
                                                        }
                                                        if (body.Position is Dictionary<string, float> posDict)
                                                        {
                                                            try
                                                            {
                                                                position = new Vec3(posDict["x"], posDict["y"], posDict["z"]);
                                                            }
                                                            catch (KeyNotFoundException knfe)
                                                            {
                                                                position = new Vec3(0.0f, 0.0f, 0.0f);
                                                            }
                                                        }
                                                        joint.GlobalJoint = rotation;
                                                        joint.U01 = position;

                                                        Matrix4x4 m = Matrix4x4.CreateFromQuaternion(new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W));
                                                        Iso4 iso4 = new Iso4(m.M11, m.M21, m.M31, m.M12, m.M22, m.M32, m.M13, m.M23, m.M33, position.X, position.Y, position.Z);

                                                        joint.U02 = iso4;
                                                        
                                                        iso4List.Add(iso4);
                                                        ushortList.Add((ushort)ushortList.Count);

                                                        return joint;
                                                    }


                                                    //  Wheels //////////////////////////////////////

                                                    List<SurfaceData> wheelSurfs = [];
                                                    if (vehiclePhyModelData.Chassis is Chassis chassis)
                                                    {
                                                        List<CPlugVehicleWheelPhyModel> wheelsList = new List<CPlugVehicleWheelPhyModel>();
                                                        for (int i = 0; i < 4; i++)
                                                        {
                                                            CPlugVehicleWheelPhyModel wheel = new();
                                                            SurfaceData wheelSurf = new SurfaceData
                                                            {
                                                                Type = "Ellipsoid"
                                                            };

                                                            switch ((WheelIndex)i)
                                                            {
                                                                case WheelIndex.FrontLeft:
                                                                    {
                                                                        wheel.Id = wheelSurf.Name = "FLSurf";
                                                                        if (chassis.AxleFront is Axle axle)
                                                                        {
                                                                            wheelSurf.Position = new Dictionary<string, float>
                                                                            {
                                                                                ["x"] = axle.WheelPositionX,
                                                                                ["y"] = chassis.GroundHeight + axle.WheelRadius,
                                                                                ["z"] = axle.PositionZ
                                                                            };
                                                                            wheelSurf.Parameters = new Dictionary<string, float>
                                                                            {
                                                                                ["x"] = axle.WheelWidthHalf,
                                                                                ["y"] = axle.WheelRadius,
                                                                                ["z"] = axle.WheelRadius
                                                                            };
                                                                            if (axle.FlagsLeft is not null)
                                                                            {
                                                                                wheel.IsDriving = axle.FlagsLeft["IsDriving"];
                                                                                wheel.IsSteering = axle.FlagsLeft["IsSteering"];
                                                                            }
                                                                        }
                                                                        break;
                                                                    }
                                                                case WheelIndex.FrontRight:
                                                                    {
                                                                        wheel.Id = wheelSurf.Name = "FRSurf";
                                                                        if (chassis.AxleFront is Axle axle)
                                                                        {
                                                                            wheelSurf.Position = new Dictionary<string, float>
                                                                            {
                                                                                ["x"] = -axle.WheelPositionX,
                                                                                ["y"] = chassis.GroundHeight + axle.WheelRadius,
                                                                                ["z"] = axle.PositionZ
                                                                            };
                                                                            wheelSurf.Parameters = new Dictionary<string, float>
                                                                            {
                                                                                ["x"] = axle.WheelWidthHalf,
                                                                                ["y"] = axle.WheelRadius,
                                                                                ["z"] = axle.WheelRadius
                                                                            };
                                                                            if (axle.FlagsRight is not null)
                                                                            {
                                                                                wheel.IsDriving = axle.FlagsRight["IsDriving"];
                                                                                wheel.IsSteering = axle.FlagsRight["IsSteering"];
                                                                            }
                                                                        }
                                                                        break;
                                                                    }
                                                                case WheelIndex.RearRight:
                                                                    {
                                                                        wheel.Id = wheelSurf.Name = "RRSurf";
                                                                        if (chassis.AxleRear is Axle axle)
                                                                        {
                                                                            wheelSurf.Position = new Dictionary<string, float>
                                                                            {
                                                                                ["x"] = -axle.WheelPositionX,
                                                                                ["y"] = chassis.GroundHeight + axle.WheelRadius,
                                                                                ["z"] = axle.PositionZ
                                                                            };
                                                                            wheelSurf.Parameters = new Dictionary<string, float>
                                                                            {
                                                                                ["x"] = axle.WheelWidthHalf,
                                                                                ["y"] = axle.WheelRadius,
                                                                                ["z"] = axle.WheelRadius
                                                                            };
                                                                            if (axle.FlagsRight is not null)
                                                                            {
                                                                                wheel.IsDriving = axle.FlagsRight["IsDriving"];
                                                                                wheel.IsSteering = axle.FlagsRight["IsSteering"];
                                                                            }
                                                                        }
                                                                        break;
                                                                    }
                                                                case WheelIndex.RearLeft:
                                                                    {
                                                                        wheel.Id = wheelSurf.Name = "RLSurf";
                                                                        if (chassis.AxleRear is Axle axle)
                                                                        {
                                                                            wheelSurf.Position = new Dictionary<string, float>
                                                                            {
                                                                                ["x"] = axle.WheelPositionX,
                                                                                ["y"] = chassis.GroundHeight + axle.WheelRadius,
                                                                                ["z"] = axle.PositionZ
                                                                            };
                                                                            wheelSurf.Parameters = new Dictionary<string, float>
                                                                            {
                                                                                ["x"] = axle.WheelWidthHalf,
                                                                                ["y"] = axle.WheelRadius,
                                                                                ["z"] = axle.WheelRadius
                                                                            };
                                                                            if (axle.FlagsLeft is not null)
                                                                            {
                                                                                wheel.IsDriving = axle.FlagsLeft["IsDriving"];
                                                                                wheel.IsSteering = axle.FlagsLeft["IsSteering"];
                                                                            }
                                                                        }
                                                                        break;
                                                                    }
                                                            }

                                                            wheelsList.Add(wheel);
                                                            wheelSurfs.Add(wheelSurf);
                                                            jointList.Add(getJoint(wheelSurf));
                                                        }

                                                        vehiclePhyModel.PhyShape!.chunk0910E000u.U04 = wheelsList.ToArray();
                                                        vehiclePhyModel.PhyShape!.chunk0910E000u.U07 = chassis.GroundHeight;
                                                        if (chassis.AxleFront is not null)
                                                        {
                                                            vehiclePhyModel.PhyShape!.chunk0910E000u.U08 = chassis.AxleFront.PositionZ;
                                                            vehiclePhyModel.PhyShape!.chunk0910E000u.U10 = chassis.AxleFront.WheelPositionX;
                                                            vehiclePhyModel.PhyShape!.chunk0910E000u.U12 = chassis.AxleFront.WheelRadius;
                                                            vehiclePhyModel.PhyShape!.chunk0910E000u.U14 = chassis.AxleFront.WheelWidthHalf;
                                                        }
                                                        if (chassis.AxleRear is not null)
                                                        {
                                                            vehiclePhyModel.PhyShape!.chunk0910E000u.U09 = chassis.AxleRear.PositionZ;
                                                            vehiclePhyModel.PhyShape!.chunk0910E000u.U11 = chassis.AxleRear.WheelPositionX;
                                                            vehiclePhyModel.PhyShape!.chunk0910E000u.U13 = chassis.AxleRear.WheelRadius;
                                                            vehiclePhyModel.PhyShape!.chunk0910E000u.U15 = chassis.AxleRear.WheelWidthHalf;
                                                        }
                                                    }


                                                    //  Body    //////////////////////////////////////

                                                    if (vehiclePhyModelData.BodySurfs is SurfaceData[] bodiesSurfs)
                                                    {
                                                        for (int i = 0; i < bodiesSurfs.Length; i++)
                                                        {
                                                            jointList.Add(getJoint(bodiesSurfs[i]));
                                                        }
                                                    }

                                                    skel.Joints = jointList.ToArray();


                                                    //  Building the Compound Surface   //////////////////////////////////

                                                    CPlugSurface.Compound compound = new CPlugSurface.Compound();

                                                    List<CPlugSurface.ISurf> surfaces = new List<CPlugSurface.ISurf>();

                                                    CPlugSurface.ISurf buildSurface(SurfaceData surf)
                                                    {
                                                        switch (surf.Type)
                                                        {
                                                            case ("Ellipsoid"):
                                                                CPlugSurface.Ellipsoid ellipsoid = new CPlugSurface.Ellipsoid();

                                                                Vec3 size = new();
                                                                if (surf.Parameters is Dictionary<string, float> sizeDict)
                                                                {
                                                                    try
                                                                    {
                                                                        size = new(sizeDict["x"], sizeDict["y"], sizeDict["z"]);
                                                                    }
                                                                    catch (KeyNotFoundException knfe)
                                                                    {
                                                                        size = new(0.0f, 0.0f, 0.0f);
                                                                        Console.WriteLine("\n[ERROR]\tIncorrect parameters for Ellipsoid.");
                                                                    }
                                                                }
                                                                ellipsoid.Size = size;
                                                                ellipsoid.U01 = null;   //  No idea what these 2 values do but I saw them like this a lot.
                                                                ellipsoid.U02 = 0;      //

                                                                return ellipsoid;

                                                            default:
                                                                Console.WriteLine("\n[ERROR]\tUnsupported Surface type \"" + surf.Type + "\".");
                                                                break;
                                                        }

                                                        CPlugSurface.Ellipsoid ellipsoidNull = new()
                                                        {
                                                            Size = new Vec3(0.0f, 0.0f, 0.0f),
                                                            U01 = null,
                                                            U02 = 0
                                                        };

                                                        return ellipsoidNull;
                                                    }

                                                    for (int i = 0; i < wheelSurfs.Count; i++)
                                                    {
                                                        surfaces.Add(buildSurface(wheelSurfs[i]));
                                                    }

                                                    if (vehiclePhyModelData.BodySurfs is SurfaceData[] bodySurfs)
                                                    {
                                                        for (int i = 0; i < bodySurfs.Length; i++)
                                                        {
                                                            surfaces.Add(buildSurface(bodySurfs[i]));
                                                        }
                                                    }

                                                    compound.Surfaces = surfaces.ToArray();

                                                    compound.U01 = null;
                                                    compound.U02 = iso4List.ToArray();
                                                    compound.U03 = ushortList.ToArray();

                                                    surface.Surf = compound;
                                                }
                                                else
                                                {
                                                    Console.WriteLine("\n[ERROR]\tNo CPlugSurface.Skel found.");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("\nNo Chunk0910E000U.U03 (CPlugSurface) found.");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("\nNo CPlugVehicleCarPhyShape.Chunk0910E000u found.");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("\nCPlugVehiclePhyModel.PhyShape is null.");
                                    }

                                    vehiclePhyModel.Save(args[firstIsJSON ? 0 : 1] + ".Gbx");
                                }
                                else
                                {
                                    Console.WriteLine("\nSupplied file is not a CPlugVehiclePhyModel.");
                                }
                            }
                            catch (Exception exc)
                            {
                                Console.WriteLine(exc.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("\n\n[ERROR]\tPlease supply 1 GBX-VehiclePhyModel file to export to JSON, or supply 1 JSON-VehiclePhyModel & 1 GBX-VehiclePhyModel as a base to import the changes into, e.g.");
                    Console.WriteLine("\n\t\"VehiclePhyModeler.exe CanyonCar.VehiclePhyModel.Gbx\"\n");
                    Console.WriteLine("\n\t\"VehiclePhyModeler.exe ValleyCar.VehiclePhyModel.json StadiumCar.VehiclePhyModel.Gbx\"\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\n\nFinished VehiclePhyModeler program.");

            return;
        }
    }
}