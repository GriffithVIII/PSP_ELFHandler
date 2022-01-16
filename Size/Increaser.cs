using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;
using Yarhl.Media;
using Yarhl.Media.Text;
using System.Text.RegularExpressions;

namespace PspEbootToolkit.Size
{
    class Increaser
    {
        private string TargetExe, TargetOutput;
        private int iPhnum, iPhoff, iShoff, iShsizeH, iShnum,
            IncreaseSize, EBOOTSize, Aligment = 0x40;
        private long StartR, LenghtR;
        private Encoding encoding = Encoding.GetEncoding(65001);
        public Increaser(string targetExePassed, string outPathPassed, int IncreaseSizePassed)
        {
            TargetExe = targetExePassed;
            TargetOutput = outPathPassed + "EBOOT.BIN";
            IncreaseSize = IncreaseSizePassed;
        }

        public void ReadHeader()
        {
            using (DataStream ORDS = DataStreamFactory.FromFile(TargetExe, FileOpenMode.Read)) //# Original EBOOT
            {
                DataReader ORDR = new DataReader(ORDS)
                {
                    Endianness = EndiannessMode.LittleEndian,
                    DefaultEncoding = encoding
                };

                EBOOTSize = (int)ORDS.Length;
                ORDS.Seek(0x1C);
                iPhoff = ORDR.ReadInt32();//# Points to the start of the program header table
                ORDS.Seek(0x20);
                iShoff = ORDR.ReadInt32();//# Points to the start of the section header table
                ORDS.Seek(0x2C);
                iPhnum = ORDR.ReadInt16();//# Contains the number of entries in the program header table
                ORDS.Seek(0x2E);
                iShsizeH = ORDR.ReadInt16();//# Contains the size of a section header table entry
                ORDS.Seek(0x30);
                iShnum = ORDR.ReadInt16();//# Contains the number of entries in the section header table

                //# Header entries, iPhnum = Programm Headers quantity
                int[] iType = new int[iPhnum + 1];
                int[] iOffset = new int[iPhnum + 1];
                int[] iVaddr = new int[iPhnum + 1];
                int[] iPaddr = new int[iPhnum + 1];
                int[] iFilesz = new int[iPhnum + 1];
                int[] iMemsz = new int[iPhnum + 1];
                int[] iFlags = new int[iPhnum + 1];
                int[] iAlign = new int[iPhnum + 1];

                //# Header section entries
                int[] iShname = new int[iShnum];
                int[] iShtype = new int[iShnum];
                int[] iShflags = new int[iShnum];
                int[] iShaddr = new int[iShnum];
                int[] iShoffset = new int[iShnum];
                int[] iShsize = new int[iShnum];
                int[] iShlink = new int[iShnum];
                int[] iShinfo = new int[iShnum];
                int[] iShaddralign = new int[iShnum];
                int[] iShentsize = new int[iShnum];
                //# Must update -> iOffset, iPaddr

                ORDS.Seek(iPhoff);//# Go to Programm header table
                for(int i = 0; i < iPhnum; i++)
                {
                    iType[i] = ORDR.ReadInt32(); 
                    iOffset[i] = ORDR.ReadInt32();
                    iVaddr[i] = ORDR.ReadInt32();
                    iPaddr[i] = ORDR.ReadInt32();
                    iFilesz[i] = ORDR.ReadInt32();
                    iMemsz[i] = ORDR.ReadInt32();
                    iFlags[i] = ORDR.ReadInt32();
                    iAlign[i] = ORDR.ReadInt32();
                }

                ORDS.Seek(iShoff);//# Go to Section header table
                for (int i = 0; i < iShnum; i++)
                {
                    iShname[i] = ORDR.ReadInt32();
                    iShtype[i] = ORDR.ReadInt32();
                    iShflags[i] = ORDR.ReadInt32();
                    iShaddr[i] = ORDR.ReadInt32();
                    iShoffset[i] = ORDR.ReadInt32();
                    iShsize[i] = ORDR.ReadInt32();
                    iShlink[i] = ORDR.ReadInt32();
                    iShinfo[i] = ORDR.ReadInt32();
                    iShaddralign[i] = ORDR.ReadInt32();
                    iShentsize[i] = ORDR.ReadInt32();
                }

                using (DataStream DS = DataStreamFactory.FromFile(TargetOutput, FileOpenMode.Write)) //# Updated EBOOT
                {
                    DataWriter DW = new DataWriter(DS)
                    {
                        Endianness = EndiannessMode.LittleEndian,
                        DefaultEncoding = encoding
                    };

                    DW.WriteUntilLength(0x00, EBOOTSize + IncreaseSize + Aligment); //# Fill the new EBOOT

                    DS.Seek(0);
                    //# Update offset of section header
                    ORDS.WriteSegmentTo(0, 0x20, DS);
                    iShoff += IncreaseSize + Aligment;
                    DW.Write((Int32)iShoff);

                    DS.Seek(0x24);
                    //# Update number of programm headers
                    ORDS.WriteSegmentTo(0x24, 0x8, DS);
                    DW.Write((Int16)iPhnum + 1);

                    //# Write until programm header beginnt
                    DS.Seek(0x2E);
                    ORDS.WriteSegmentTo(DS.Position, iPhoff - DS.Position, DS);

                    //# Old pointers update
                    for(int i = 0; i < iPhnum - 1; i++)
                    {
                        iOffset[i] = iOffset[i] + Aligment;
                        if(i == 0)
                        {
                            iPaddr[i] = iPaddr[i] + Aligment;
                        }
                        else
                        {
                            iVaddr[i] = iVaddr[i] + Aligment;
                        }
                    }
                    
                    //# New section
                    iType[iPhnum] = 0x01;
                    iOffset[iPhnum] = iOffset[iPhnum - 1] + iFilesz[iPhnum - 1];//# End of previous section
                    iVaddr[iPhnum] = iVaddr[iPhnum - 1] + iFilesz[iPhnum - 1];//# Virtual address of previous section
                    iPaddr[iPhnum] = 0x0;
                    iFilesz[iPhnum] = IncreaseSize;
                    iMemsz[iPhnum] = IncreaseSize;
                    iFlags[iPhnum] = 0x7;
                    iAlign[iPhnum] = 0x40;

                    DS.Seek(iPhoff);
                    for (int i = 0; i <= iPhnum; i++)
                    {
                        DW.Write(iType[i]);
                        DW.Write(iOffset[i]);
                        DW.Write(iVaddr[i]);
                        DW.Write(iPaddr[i]);
                        DW.Write(iFilesz[i]);
                        DW.Write(iMemsz[i]);
                        DW.Write(iFlags[i]);
                        DW.Write(iAlign[i]);
                    }

                    StartR = DS.Position - Aligment;
                    LenghtR = iOffset[iPhnum] - (DS.Position - Aligment);
                    ORDS.WriteSegmentTo(StartR, LenghtR, DS);

                    DS.Seek(DS.Position + IncreaseSize);

                    StartR = iOffset[iPhnum];
                    LenghtR = iShoff - iOffset[iPhnum];
                    ORDS.WriteSegmentTo(StartR, LenghtR, DS);//# Until section header table

                    //# Old pointers section update
                    for (int i = 0; i < iShnum - 1; i++)
                    {
                        if (iShoffset[i] < iOffset[iPhnum])
                        {
                            iShoffset[i] += Aligment;
                        }
                        else
                        {
                            iShoffset[i] += Aligment + IncreaseSize;
                        }
                    }

                    DS.Seek(iShoff);//# Go to Section header table
                    for (int i = 0; i < iShnum; i++)
                    {
                        DW.Write(iShname[i]);
                        DW.Write(iShtype[i]);
                        DW.Write(iShflags[i]);
                        DW.Write(iShaddr[i]);
                        DW.Write(iShoffset[i]);
                        DW.Write(iShsize[i]);
                        DW.Write(iShlink[i]);
                        DW.Write(iShinfo[i]);
                        DW.Write(iShaddralign[i]);
                        DW.Write(iShentsize[i]);
                    }

                    StartR = (iShsizeH * iShnum) + (iShoff - (IncreaseSize + Aligment));
                    LenghtR = ORDS.Length - StartR;
                    ORDS.WriteSegmentTo(StartR, LenghtR, DS);
                }
            }
        }
    }
}
