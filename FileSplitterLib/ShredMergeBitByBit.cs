using FileSplitterCommon;
using FileSplitterDef;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplitterLib
{
    [Export(typeof(IFileMerger))]
    [Export(typeof(IFileSpliter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShredMergeBitByBit : IFileMerger, IFileSpliter
    {
        public string UserName
        {
            get { return "ByBit"; }
        }

        public Guid Protocol
        {
            get { return new Guid("0A8228B2-2F3F-4340-A6F3-91E070BAACEC"); }
        }

        static readonly byte[] BYTE_MASK = new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 };

        const byte LESS_SIG_MASK = 0x1;

        public void Merge(Stream target, Stream[] sources, int position, byte numberOfPart, long totalLength)
        {

            if (sources.Length < 2)
                throw new ArgumentException("You need at least 2 sources");

            //ensure all the source stream positions are after the header.
            foreach (var source in sources)
                if (source.Position < position)
                    source.Position = position;


            /*byte numberOfPart = Utils.GetQtyTotal(header);
            byte indexOfSource = Utils.GetIndex(header);
            long length = Utils.GetLengthFromHeader(header);*/

            bool oneEndOfFile = false;
            byte[] oneByteBuffer = new byte[1];
            byte[] curRead = new byte[numberOfPart];
            byte[] writeBuffer = new byte[numberOfPart];
            long totalWrited = 0;

            while (!oneEndOfFile)
            {
                //6 retrouver 1 byte de toutes les parties
                for (byte i = 0; i < numberOfPart; i++)
                {
                    if (sources[i].Read(oneByteBuffer, 0, 1) < 1)
                    {
                        curRead[i] = 0;
                        oneEndOfFile = true;
                    }
                    else
                        curRead[i] = oneByteBuffer[0];
                }
                //7réunir les éléments dans le buffer.
                int pos = 0;
                for (byte i = 0; i < 8; i++)
                    for (byte j = 0; j < numberOfPart; j++)
                    {
                        //Console.WriteLine((i * j) / 8 + "<->" + position / 8);
                        if ((curRead[j] & BYTE_MASK[i]) > 0)
                            writeBuffer[pos / 8] += BYTE_MASK[pos % 8];
                        pos++;
                    }
                //8écriture du buffer
                if (!oneEndOfFile)
                {
                    byte qtyToWrite = totalWrited + numberOfPart > totalLength ? (byte)(totalLength - totalWrited) : numberOfPart;

                    target.Write(writeBuffer, 0, qtyToWrite);
                    totalWrited += qtyToWrite;
                }

                //9 réinit shorter than reassign ? 
                for (int i = 0; i < writeBuffer.Length; i++)
                    writeBuffer[i] = 0;
            }
        }

        public void Split(Stream[] targets, Stream source, byte numberOfPart)
        {
            const int READ_SIZE = 1024;
            byte[] curRead = new byte[READ_SIZE];

            int qtyRead = 0;
            long cursor = 0;
            byte[] curWrite = new byte[numberOfPart];
            byte[] curWriteIndex = new byte[numberOfPart];
            Utils.InitByteArray(ref curWrite);
            Utils.InitByteArray(ref curWriteIndex);
            long sourceLength = 0;

            while ((qtyRead = source.Read(curRead, 0, READ_SIZE)) > 0)
            {
                sourceLength += qtyRead;
                for (int i = 0; i < qtyRead; i++)
                {
                    byte cur = curRead[i];
                    for (byte j = 0; j < 8; j++)
                    {
                        byte part = (byte)(cursor % numberOfPart);
                        //1 copier le bit plus a droite dans curWrite part
                        if ((cur & LESS_SIG_MASK) > 0)
                            curWrite[part] += (byte)Math.Pow(2, curWriteIndex[part]);
                        //2incrémenter la partie a écrire
                        curWriteIndex[part]++;
                        //3 décaler le byte cur en fonction
                        cur = (byte)(cur >> 1);

                        //4 si curwriteindex == 7 alors écrire writer[part].Buffer dans writer[part]
                        if (curWriteIndex[part] == 8)
                        {
                            targets[part].Write(Utils.OneByteArray(curWrite[part]), 0, 1);
                            //puis réinitialiser curwrite[part] et curWriteIndexDePart
                            curWrite[part] = 0;
                            curWriteIndex[part] = 0;
                        }
                        cursor++;
                    }
                }
            }
            //5 écrire le reste (s'il y en a un) de curWrite dans writer 
            if (sourceLength % numberOfPart > 0)
                for (byte i = 0; i < numberOfPart; i++)
                    targets[i].Write(Utils.OneByteArray(curWrite[i]), 0, 1);

        }
    }
}
