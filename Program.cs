using GBX.NET;
using GBX.NET.Engines.Plug;
using GBX.NET.LZO;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace VPM
{
    public class SurfaceData
    {
        public Dictionary<string, float>? Position {  get; set; }
        public Dictionary<string, float>? Rotation {  get; set; }
        public string? Type { get; set; }
        public Dictionary<string, float>? Parameters { get; set; }
    }

    public class WheelData : BodyData
    {
        //public string? Name { get; set; }
        public bool IsDriving { get; set; }
        public bool IsSteering { get; set; }
        //public SurfaceData? Surface { get; set; }
    }

    public class BodyData
    {
        public string? Name { get; set; }
        public SurfaceData? Surface { get; set; }
    }

    public class VehiclePhyModelData
    {
        public string? Version { get; set; }
        public BodyData[]? BodySurfs { get; set; }
        public WheelData[]? WheelSurfs { get; set; }
    }

    public class Program
    {
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

                                List<WheelData> wheelDatas = new List<WheelData>();
                                if (wheels is not null)
                                {
                                    for (int i = 0; i < wheels.Length; i++)
                                    {
                                        string id = wheels[i].Id;
                                        bool isDriving = wheels[i].IsDriving;
                                        bool isSteering = wheels[i].IsSteering;

                                        Console.WriteLine("\nWheel #" + i + ":");
                                        Console.WriteLine("id\t\t\t" + id);
                                        Console.WriteLine("isDriving\t\t" + isDriving);
                                        Console.WriteLine("isSteering\t\t" + isSteering);

                                        wheelDatas.Add(new WheelData
                                        {
                                            Name = id,
                                            IsDriving = isDriving,
                                            IsSteering = isSteering,
                                            Surface = null
                                        });
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("\nChunk0910E000.U04 (CPlugVehicleWheelPhyModel) is null.");
                                }

                                List<BodyData> bodyDatas = new List<BodyData>();
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

                                            SurfaceData surfaceData = new SurfaceData
                                            {
                                                Position = null,
                                                Rotation = null,
                                                Type = null,
                                                Parameters = null
                                            };

                                            if (position.HasValue)
                                            {
                                                surfaceData.Position = new Dictionary<string, float>
                                                {
                                                    ["x"] = position.Value.X,
                                                    ["y"] = position.Value.Y,
                                                    ["z"] = position.Value.Z
                                                };
                                            }
                                            if (rotation.HasValue)
                                            {
                                                surfaceData.Rotation = new Dictionary<string, float>
                                                {
                                                    ["x"] = rotation.Value.X,
                                                    ["y"] = rotation.Value.Y,
                                                    ["z"] = rotation.Value.Z,
                                                    ["w"] = rotation.Value.W
                                                };
                                            }

                                            bool isWheel = false;
                                            for (int j = 0; j < wheelDatas.Count; j++)
                                            {
                                                if (wheelDatas[j].Name == jointName)
                                                {
                                                    isWheel = true;
                                                    wheelDatas[j].Surface = surfaceData;
                                                    break;
                                                }
                                            }
                                            if (!isWheel)
                                            {
                                                bodyDatas.Add(new BodyData
                                                {
                                                    Name = jointName,
                                                    Surface = surfaceData
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
                                                        for (int i = 0; i < wheelDatas.Count; i++)
                                                        {
                                                            if (wheelDatas[i].Name == surfName)
                                                            {
                                                                if (wheelDatas[i].Surface is SurfaceData surfData)
                                                                {
                                                                    surfData.Type = "Ellipsoid";
                                                                    surfData.Parameters = new Dictionary<string, float>
                                                                    {
                                                                        ["x"] = size.X,
                                                                        ["y"] = size.Y,
                                                                        ["z"] = size.Z
                                                                    };
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("[ERROR]\tSurfaceData was not yet initialized for wheel #" + i + "!");
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < bodyDatas.Count; i++)
                                                        {
                                                            if (bodyDatas[i].Name == surfName)
                                                            {
                                                                if (bodyDatas[i].Surface is SurfaceData surfData)
                                                                {
                                                                    surfData.Type = "Ellipsoid";
                                                                    surfData.Parameters = new Dictionary<string, float>
                                                                    {
                                                                        ["x"] = size.X,
                                                                        ["y"] = size.Y,
                                                                        ["z"] = size.Z
                                                                    };
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("[ERROR]\tSurfaceData was not yet initialized for body #" + i + "!");
                                                                }
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
                                    Version = "0.0.1",
                                    BodySurfs = bodyDatas.ToArray(),
                                    WheelSurfs = wheelDatas.ToArray()
                                };

                                JsonSerializerOptions? jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                                string jsonString = JsonSerializer.Serialize(vehiclePhyModelData, jsonOptions);

                                Console.WriteLine("\n\n" + jsonString);
                                //vpm.Save("null.VehiclePhyModel.Gbx");
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
                                                //CPlugVehicleWheelPhyModel[]? wheels = [];

                                                List<Tuple<bool, string?>> surfs = new List<Tuple<bool, string?>>();

                                                List<BodyData> bodyDatas = new List<BodyData>();
                                                if (surface.Skel is CPlugSkel skel)
                                                {
                                                    //CPlugSkel? skel = surface.Skel;
                                                    //CPlugSurface.ISurf? surf = surface.Surf;

                                                    //if (skel is not null)
                                                    //{
                                                    skel.Name = "";

                                                    //Console.WriteLine("\nSkeleton \"" + (name is not null ? name : "\\null") + "\":");

                                                    List<CPlugSkel.Joint> jointList = new List<CPlugSkel.Joint>();
                                                    List<Iso4> iso4List = new List<Iso4>();
                                                    List<ushort> ushortList = new List<ushort>();

                                                    CPlugSkel.Joint getJoint(BodyData body)
                                                    {
                                                        CPlugSkel.Joint joint = new CPlugSkel.Joint();

                                                        joint.Name = body.Name;
                                                        joint.ParentIndex = -1; // Potentially different for Compounds of Compounds?

                                                        Quat rotation = new Quat();
                                                        Vec3 position = new Vec3();
                                                        if (body.Surface is SurfaceData surfData)
                                                        {
                                                            if (surfData.Rotation is Dictionary<string, float> rotDict)
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
                                                            if (surfData.Position is Dictionary<string, float> posDict)
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
                                                        }
                                                        joint.GlobalJoint = rotation;
                                                        joint.U01 = position;

                                                        Matrix4x4 m = Matrix4x4.CreateFromQuaternion(new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W));
                                                        joint.U02 = new Iso4(m.M11, m.M21, m.M31, m.M12, m.M22, m.M32, m.M13, m.M23, m.M33, position.X, position.Y, position.Z);
                                                        if (joint.U02 is Iso4 jointU02)
                                                        {
                                                            iso4List.Add(jointU02);
                                                        }
                                                        ushortList.Add((ushort)ushortList.Count);

                                                        return joint;
                                                    }

                                                    if (vehiclePhyModelData.WheelSurfs is WheelData[] wheelsSurfs)
                                                    {
                                                        for (int i = 0; i < wheelsSurfs.Length; i++)
                                                        {
                                                            jointList.Add(getJoint(wheelsSurfs[i]));
                                                        }
                                                    }

                                                    if (vehiclePhyModelData.BodySurfs is BodyData[] bodiesSurfs)
                                                    {
                                                        for (int i = 0; i < bodiesSurfs.Length; i++)
                                                        {
                                                            jointList.Add(getJoint(bodiesSurfs[i]));
                                                        }
                                                    }

                                                    skel.Joints = jointList.ToArray();
                                                    //}
                                                    //else
                                                    //{
                                                    //    Console.WriteLine("\nNo CPlugSkel found.");
                                                    //}


                                                    CPlugSurface.Compound compound = new CPlugSurface.Compound();

                                                    List<CPlugSurface.ISurf> surfaces = new List<CPlugSurface.ISurf>();

                                                    CPlugSurface.ISurf buildSurface(BodyData body)
                                                    {
                                                        if (body.Surface is SurfaceData surfData)
                                                        {
                                                            switch (surfData.Type)
                                                            {
                                                                case ("Ellipsoid"):
                                                                    CPlugSurface.Ellipsoid ellipsoid = new CPlugSurface.Ellipsoid();

                                                                    Vec3 size = new Vec3();
                                                                    if (surfData.Parameters is Dictionary<string, float> sizeDict)
                                                                    {
                                                                        try
                                                                        {
                                                                            size = new Vec3(sizeDict["x"], sizeDict["y"], sizeDict["z"]);
                                                                        }
                                                                        catch (KeyNotFoundException knfe)
                                                                        {
                                                                            size = new Vec3(0.0f, 0.0f, 0.0f);
                                                                            Console.WriteLine("\nIncorrect parameters for Ellipsoid");
                                                                        }
                                                                    }
                                                                    ellipsoid.Size = size;
                                                                    ellipsoid.U01 = null;   //  No idea what these 2 values do but I saw them like this a lot.
                                                                    ellipsoid.U02 = 0;      //

                                                                    return ellipsoid;

                                                                default:
                                                                    Console.WriteLine("[ERROR]\tUnsupported Surface type \"" + surfData.Type + "\".");
                                                                    break;
                                                            }
                                                        }

                                                        CPlugSurface.Ellipsoid ellipsoidNull = new CPlugSurface.Ellipsoid();
                                                        ellipsoidNull.Size = new Vec3(0.0f, 0.0f, 0.0f);
                                                        ellipsoidNull.U01 = null;
                                                        ellipsoidNull.U02 = 0;

                                                        return ellipsoidNull;
                                                    }

                                                    if (vehiclePhyModelData.WheelSurfs is WheelData[] wheelsSurf)
                                                    {
                                                        for (int i = 0; i < wheelsSurf.Length; i++)
                                                        {
                                                            surfaces.Add(buildSurface(wheelsSurf[i]));
                                                        }
                                                    }

                                                    if (vehiclePhyModelData.BodySurfs is BodyData[] bodiesSurf)
                                                    {
                                                        for (int i = 0; i < bodiesSurf.Length; i++)
                                                        {
                                                            surfaces.Add(buildSurface(bodiesSurf[i]));
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
                                                    Console.WriteLine("\nNo CPlugSurface.Skel found.");
                                                }
                                            }

                                            if (phyShape.U04 is CPlugVehicleWheelPhyModel[] wheels)
                                            {
                                                List<CPlugVehicleWheelPhyModel> wheelsList = new List<CPlugVehicleWheelPhyModel>();
                                                if (vehiclePhyModelData.WheelSurfs is WheelData[] wheelSurfs)
                                                {
                                                    for (int i = 0; i < wheelSurfs.Length; i++)
                                                    {
                                                        CPlugVehicleWheelPhyModel wheel = new CPlugVehicleWheelPhyModel();

                                                        wheel.Id = wheelSurfs[i].Name is string name ? name : (i + "Surf");
                                                        wheel.IsDriving = wheelSurfs[i].IsDriving;
                                                        wheel.IsSteering = wheelSurfs[i].IsSteering;

                                                        wheelsList.Add(wheel);
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("\nJSON-VehiclePhyModel has no wheels.");
                                                }

                                                wheels = wheelsList.ToArray();
                                                vehiclePhyModel.PhyShape!.chunk0910E000u.U04 = wheels;
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
                                }
                                else
                                {
                                    Console.WriteLine("\nSupplied file is not a CPlugVehiclePhyModel.");
                                }

                                vpm.Save(args[firstIsJSON ? 0 : 1] + ".Gbx");

                                //Console.WriteLine("Success?");
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