namespace GBX.NET.Engines.Plug;

public struct Chunk0910E000U
{
    public float U01;
    public CPlugSolid? U02;
    public CPlugSurface? U03;
    public CPlugVehicleWheelPhyModel[] U04;// = Array.Empty<CPlugVehicleWheelPhyModel>();
    public Box U05;
    public bool U06;
    public float U07;
    public float U08;
    public float U09;
    public float U10;
    public float U11;
    public float U12;
    public float U13;
    public float U14;
    public float U15;
    public Vec4[] U16 = Array.Empty<Vec4>();
    public Box U17;
    public bool U18;
    public bool U19;
    public float U20;
    public float U21;
    public bool U22;
    public float U23;
    public bool U24;
    public float U25;
    public bool U26;
    public float U27;
    public float U28;
    public float U29;
    public float? U30;
    public float? U31;
    public float? U32;
    public string? U33;
    public CPlugSurface? U34;
    public Vec3[]? U35;
    public CPlugSurface? U36;
    public CPlugSurface? U37;
    public CPlugSurface? U38;

    public Chunk0910E000U()
    {
    }
}

[Node(0x0910E000)]
public class CPlugVehicleCarPhyShape : CMwNod
{
    public Chunk0910E000U chunk0910E000u;

    [NodeMember]
    [AppliedWithChunk<Chunk0910E000>]
    public Chunk0910E000U Chunk0910E000u { get => chunk0910E000u; set => chunk0910E000u = value; }

    internal CPlugVehicleCarPhyShape()
    {
    }

    #region 0x000 chunk

    /// <summary>
    /// CPlugVehicleCarPhyShape 0x000 chunk
    /// </summary>
    [Chunk(0x0910E000)]
    public class Chunk0910E000 : Chunk<CPlugVehicleCarPhyShape>, IVersionable
    {
        public int Version { get; set; }

        public override void ReadWrite(CPlugVehicleCarPhyShape n, GameBoxReaderWriter rw)
        {
            rw.VersionInt32(this);
            
            rw.Single(ref n.chunk0910E000u.U01);
            rw.NodeRef<CPlugSolid>(ref n.chunk0910E000u.U02);

            if (Version >= 1 && n.chunk0910E000u.U02 is null)
            {
                rw.NodeRef<CPlugSurface>(ref n.chunk0910E000u.U03);
            }

            rw.ArrayArchive<CPlugVehicleWheelPhyModel>(ref n.chunk0910E000u.U04!);

            rw.Box(ref n.chunk0910E000u.U05);
            rw.Boolean(ref n.chunk0910E000u.U06);
            rw.Single(ref n.chunk0910E000u.U07);
            rw.Single(ref n.chunk0910E000u.U08);
            rw.Single(ref n.chunk0910E000u.U09);
            rw.Single(ref n.chunk0910E000u.U10);
            rw.Single(ref n.chunk0910E000u.U11);
            rw.Single(ref n.chunk0910E000u.U12);
            rw.Single(ref n.chunk0910E000u.U13);
            rw.Single(ref n.chunk0910E000u.U14);
            rw.Single(ref n.chunk0910E000u.U15);
            rw.Array<Vec4>(ref n.chunk0910E000u.U16!);
            rw.Box(ref n.chunk0910E000u.U17);
            rw.Boolean(ref n.chunk0910E000u.U18);
            rw.Boolean(ref n.chunk0910E000u.U19);
            rw.Single(ref n.chunk0910E000u.U20);
            rw.Single(ref n.chunk0910E000u.U21);
            rw.Boolean(ref n.chunk0910E000u.U22);
            rw.Single(ref n.chunk0910E000u.U23);
            rw.Boolean(ref n.chunk0910E000u.U24);
            rw.Single(ref n.chunk0910E000u.U25);
            rw.Boolean(ref n.chunk0910E000u.U26);
            rw.Single(ref n.chunk0910E000u.U27);
            rw.Single(ref n.chunk0910E000u.U28);
            rw.Single(ref n.chunk0910E000u.U29);

            if (Version >= 2)
            {
                rw.Single(ref n.chunk0910E000u.U30);
                rw.Single(ref n.chunk0910E000u.U31);
                rw.Single(ref n.chunk0910E000u.U32);

                if (Version >= 3)
                {
                    rw.String(ref n.chunk0910E000u.U33);

                    if (n.chunk0910E000u.U33 is null || n.chunk0910E000u.U33.Length == 0)
                    {
                        rw.NodeRef<CPlugSurface>(ref n.chunk0910E000u.U34);
                    }
                    
                    if (Version >= 4)
                    {
                        rw.Array<Vec3>(ref n.chunk0910E000u.U35);
                        
                        if (Version >= 5)
                        {
                            rw.NodeRef<CPlugSurface>(ref n.chunk0910E000u.U36);
                            
                            if (Version >= 6)
                            {
                                rw.NodeRef<CPlugSurface>(ref n.chunk0910E000u.U37);
                                rw.NodeRef<CPlugSurface>(ref n.chunk0910E000u.U38);
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion
}
