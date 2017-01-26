using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadRawRTP
{
   

    public partial class Form1 : Form
    {
        public Form1()
        {
          
            InitializeComponent();
          //  textBox1.Text = "G:/MPFS file/MPFS file/test.bin";
          //  button2_Click(null,null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = openFileDialog1.FileName;
                }
            }
            catch (Exception ec)
            {
                
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        private static int Bytes2Int(byte b1, byte b2, byte b3)
        {
            int r = 0;
            byte b0 = 0xff;

            if ((b1 & 0x80) != 0) r |= b0 << 24;
            r |= b1 << 16;
            r |= b2 << 8;
            r |= b3;
            return r;
        }

        public void GetNEWBYTES(byte[] binFile,string changedFile)
        {


        }

        Dictionary<string,FileInformation> oldInformation= new Dictionary<string, FileInformation>(); 
        private void button2_Click(object sender, EventArgs e)
        {
            string outputfile = textBox1.Text.Substring(0, textBox1.Text.LastIndexOf(".")) + "_"  + ".bin";
            Boolean folder = false;
           

            string changedFile = textBox2.Text;
           
            string changedFileName = Path.GetFileName(changedFile); ;
            try
            {
                oldInformation.Clear();
                byte[] binFile =  File.ReadAllBytes(textBox1.Text);
                byte[] header  = new byte[4];

                Buffer.BlockCopy(binFile, 0, header, 0, 4);
               string headS= Encoding.ASCII.GetString(header);
                if (headS != "MPFS")
                {
                    MessageBox.Show("not a valid MPFS file type");
                    return;
                }
               // byte[] FileNames = new byte[668];
                //1472 2140 names
            //    Buffer.BlockCopy(binFile,1472, FileNames, 0, 668);

                byte[] FilesInfo = new byte[1342];
                //1472 2140 names
                Buffer.BlockCopy(binFile, 130, FilesInfo, 0, 1342);

                for (int i = 0; i < FilesInfo.Length; i=i+22)
                {
                    int countingDots = 0;
                    byte[] oneFile = new byte[22];
                    //1472 2140 names
                    Buffer.BlockCopy(FilesInfo,i, oneFile, 0, 22);
                    int Value1 = FilesInfo[0+i];
                    int Value2 = FilesInfo[1+i];
                   // string hex1 = Value1.ToString("X");
                  //  string hex2 = Value2.ToString("X");
                    var test = BitConverter.ToInt16(new byte[] {(byte)Value1, (byte)Value2 }, 0);
                    bool running = true;
                    int tempInt = 0;

                    int increment = 0;
                    StringBuilder sb= new StringBuilder();
                    while (tempInt<2)
                    {
                        byte[]
                        temp = new byte[1];
                        temp[0]=  binFile[test+ increment];
                        string charcters  = Encoding.ASCII.GetString(temp);
                      Console.WriteLine(charcters );
                        Console.WriteLine(temp[0].ToString("X"));
                        if (temp[0].ToString("X") == "2E")
                     {
                            tempInt++;
                        }
                        if (temp[0] == 0)
                        {
                            break;
                        }

                        sb.Append(charcters);
                        increment++;



                    }
                    //if (sb.ToString() != "") { 
                    string FileName = sb.ToString();
                   byte[] ttt  =
                        new byte[]
                        {
                            //  oneFile[2],
                            //  oneFile[3], 
                            oneFile[4], oneFile[5], oneFile[6],oneFile[7]
                        };
                       

                        int offsetInt=   BitConverter.ToInt32(ttt, 0);

                        int size = BitConverter.ToInt32(new byte[] {oneFile[8],
                          oneFile[9], oneFile[10], oneFile[11]
                        }, 0);
                        //2141
                     oldInformation.Add(Guid.NewGuid()+FileName,new FileInformation(offsetInt, size, 130+i));
                        string hex = "0";
                        //if (offsetInt.ToString("X").Contains("FFF"))
                        //{
                        //    hex= offsetInt.ToString("X").Remove(0,3);
                        //}
                        //else
                        //{
        hex = offsetInt.ToString("X");
                        //}
                        Console.WriteLine(hex + "    "+ size+"   "+FileName);
                        

                    }
                    //  string fileNameindex =
                    //string fileName=
                    //   Int32 offset=

                //}
                byte[] newInfo = (byte[])binFile.Clone();
                byte[] chang = File.ReadAllBytes(changedFile);
                byte[] intSize = BitConverter.GetBytes(chang.Length);
                int changeInSize = 0;
                bool flagOffSetChanged = false;
                int offsetFirstFile = 0;
                int offsetSize = 0;

                foreach (var vl in oldInformation) {
                    if (flagOffSetChanged)
                    {
             
                   byte[] newof    = BitConverter.GetBytes(vl.Value.offset + changeInSize);
                        newInfo[vl.Value.fileInformationOneLine +4] = newof[0];
                        newInfo[vl.Value.fileInformationOneLine +5] = newof[1];
                        newInfo[vl.Value.fileInformationOneLine +6] = newof[2];
                        newInfo[vl.Value.fileInformationOneLine +7] = newof[3];
                        continue;
                       
                    }
                    if (vl.Key.Contains(changedFileName)) {

                        flagOffSetChanged = true;
                        changeInSize = chang.Length - vl.Value.size ;

                        offsetFirstFile = vl.Value.offset;
                        offsetSize = vl.Value.size;

                        byte[] newof = BitConverter.GetBytes(chang.Length);
                        newInfo[vl.Value.fileInformationOneLine + 8] = newof[0];
                        newInfo[vl.Value.fileInformationOneLine + 9] = newof[1];
                        newInfo[vl.Value.fileInformationOneLine + 10] = newof[2];
                        newInfo[vl.Value.fileInformationOneLine + 11] = newof[3];
                        //  // FilesInfo
                        //byte[] part1 = new byte[vl.Value.fileInformationOneLine - 1];
                        ////1472 2140 names
                        //Buffer.BlockCopy(FilesInfo, 0, part1, 0, vl.Value.fileInformationOneLine - 1);

                        //  byte[] part2 = new byte[22];
                        //  Buffer.BlockCopy(FilesInfo, 0, part2, 0, 22);
                        //  part2[8] = intSize[0];
                        //  part2[9] = intSize[1];
                        //  part2[10] = intSize[2];
                        //  part2[11] = intSize[3];
                        //byte[] newInfo= (byte[]) FilesInfo.Clone();


                        //  for (int i = vl.Value.fileInformationOneLine+22;
                        //      i < FilesInfo.Length; i = i + 22)
                        //  {
                        //      byte[] oneFile = new byte[22];
                        //      //1472 2140 names
                        //      Buffer.BlockCopy(newInfo, i, oneFile, 0, 22);


                        //  }

                        //      byte[] lastPart = new byte[22];
                        //  Buffer.BlockCopy(FilesInfo, vl.Value.offset,
                        //      lastPart, 0, FilesInfo.Length-(vl.Value.offset + vl.Value.size));

                    }

              
                }
                byte[] part1 = new byte[offsetFirstFile ];
                //1472 2140 names
                Buffer.BlockCopy(newInfo, 0, part1, 0, offsetFirstFile );



                byte[] part2 = new byte[newInfo.Length - (offsetFirstFile + offsetSize)];
                //1472 2140 names
                Buffer.BlockCopy(newInfo, offsetFirstFile + offsetSize, part2, 0, newInfo.Length - (offsetFirstFile + offsetSize));
                byte[] mission = Combine(Combine(part1, chang), part2);
                // byte[] final = new byte[offsetFirstFile + offsetSize)];
                //   Buffer.BlockCopy(newInfo, offsetFirstFile + offsetSize, final, 0, newInfo.Length - (offsetFirstFile + offsetSize));
                string outputfile2 = textBox1.Text.Substring(0, textBox1.Text.LastIndexOf(".")) + "_" + GetTimestamp(DateTime.Now) + ".bin";
                File.WriteAllBytes(@outputfile2, mission);


            }
            catch (Exception ee)
            {

                Console.WriteLine( ee.ToString() );
            }
       

            string[] Lines = System.IO.File.ReadAllLines(textBox1.Text);



            //int packetCount = 0;
            //try
            //{
            //    string streamid = textBox4.Text.Trim().ToUpper();
            //    string outputfile = textBox1.Text.Substring(0, textBox1.Text.LastIndexOf(".")) + "_" + streamid + ".bit";
            //    // int Payload = Convert.ToInt32(textBox2.Text);
            //    string[] sPayload = textBox2.Text.Split(',');
            //    ArrayList Payloads = new ArrayList();
            //    foreach (string py in sPayload)
            //    {
            //        Payloads.Add(Convert.ToInt32(py));
            //    }
            //    int headerlen = Convert.ToInt32(textBox3.Text);

            //    string[] Lines = System.IO.File.ReadAllLines(textBox1.Text);
            //    if (Lines.Length > 0)
            //    {
            //        using (BinaryWriter writer = new BinaryWriter(File.Open(outputfile, FileMode.Create)))
            //        {
            //            writer.Write(new byte[] { 0x23, 0x21, 0x53, 0x49, 0x4C, 0x4B, 0x5F, 0x56, 0x33 });
            //            for (int i = 0; i < Lines.Length; i++)
            //            {
            //                try
            //                {
            //                    byte[] hex = StringToByteArray(Lines[i]);
            //                    if (Payloads.Contains((int)hex[1]))
            //                    {

            //                        byte[] sid = new byte[4];
            //                        Buffer.BlockCopy(hex, 8, sid, 0, 4);
            //                        string id = BitConverter.ToString(sid).ToUpper().Replace("-", "");
            //                        if (streamid.Length == 0 || id == streamid)
            //                        {

            //                            Int16 lenteg = (Int16) (hex.Length - headerlen);
            //                            byte[] len = BitConverter.GetBytes(lenteg);
            //                            // byte[] len = new byte[] { 0x00, 0x00 };
            //                            //len[0] = (byte) ((hex.Length - headerlen) + 0);
            //                            writer.Write(len);
            //                           // writer.Write(new byte[] {0x00, 0x00, 0x00, 0x01});
            //                            writer.Write(hex, headerlen, hex.Length - headerlen);
            //                            packetCount++;
            //                        }
            //                    }
            //                }
            //                catch (Exception)
            //                {

            //                }
            //            }
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{

            //}
            MessageBox.Show("Completed sucessfully");
            System.Windows.Forms.Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
        public static String GetTimestamp( DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssfff");
        }
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    textBox2.Text = openFileDialog1.FileName;
                }
            }
            catch (Exception ec)
            {

            }
        }

       
        //private void button2_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        string streamid = textBox4.Text.Trim().ToUpper();
        //        string outputfile = textBox1.Text.Substring(0, textBox1.Text.LastIndexOf(".")) + "_" + streamid +".bit";
        //        int Payload = Convert.ToInt32(textBox2.Text);
        //        int headerlen = Convert.ToInt32(textBox3.Text);

        //        string[] Lines= System.IO.File.ReadAllLines(textBox1.Text);
        //        if (Lines.Length > 0)
        //        {
        //            using (BinaryWriter writer = new BinaryWriter(File.Open(outputfile, FileMode.Create)))
        //            {
        //                writer.Write(new byte[] {0x23, 0x21, 0x53, 0x49, 0x4C, 0x4B, 0x5F, 0x56, 0x33});
        //                for (int i = 0; i < Lines.Length; i++)
        //                {
        //                    try
        //                    {
        //                        byte[] hex = StringToByteArray(Lines[i]);
        //                        if (hex[1] == Payload)
        //                        {

        //                            byte[] sid = new byte[4];
        //                            Buffer.BlockCopy(hex, 8, sid, 0, 4);
        //                            string id = BitConverter.ToString(sid).ToUpper().Replace("-", "");
        //                            if (streamid.Length > 0 || id == streamid)
        //                            {
        //                                byte[] len = new byte[] {0x00, 0x00};
        //                                len[0] = (byte) (hex.Length - headerlen);
        //                                writer.Write(len);
        //                                writer.Write(hex, headerlen, hex.Length - headerlen);
        //                            }
        //                        }
        //                    }
        //                    catch (Exception)
        //                    {

        //                    }
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    MessageBox.Show("Complete");
        //}
        //private void button3_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        richTextBox1.Clear();
        //        string[] sPayload = textBox2.Text.Split(',');
        //        ArrayList Payloads = new ArrayList();
        //        foreach (string py in sPayload)
        //        {
        //            Payloads.Add(Convert.ToInt32(py));
        //        }
        //        string[] Lines = System.IO.File.ReadAllLines(textBox1.Text);
        //        ArrayList list = new ArrayList();
        //        if (Lines.Length > 0)
        //        {
        //            for (int i = 0; i < Lines.Length; i++)
        //            {
        //                try
        //                {
        //                    byte[] hex = StringToByteArray(Lines[i]);
        //                    byte[] len = new byte[4];
        //                        Buffer.BlockCopy(hex, 8, len, 0, 4);
        //                        string id = BitConverter.ToString(len).ToUpper().Replace("-", "");
        //                        if (!list.Contains(id))
        //                        {
        //                            list.Add(id);
        //                            richTextBox1.AppendText(id + Environment.NewLine);
        //                        }

        //                }
        //                catch (Exception)
        //                {

        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    MessageBox.Show("Complete");
        //}

        //private void button4_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        string streamid = textBox4.Text.Trim().ToUpper();
        //        string outputfile = textBox1.Text.Substring(0, textBox1.Text.LastIndexOf(".")) + "_" + streamid + ".bit";
        //        int Payload = Convert.ToInt32(104);
        //        int headerlen = Convert.ToInt32(textBox3.Text);

        //        string[] Lines = System.IO.File.ReadAllLines(textBox1.Text);
        //        if (Lines.Length > 0)
        //        {
        //            using (BinaryWriter writer = new BinaryWriter(File.Open(outputfile, FileMode.Create)))
        //            {
        //                writer.Write(new byte[] { 0x23, 0x21, 0x53, 0x49, 0x4C, 0x4B, 0x5F, 0x56, 0x33 });
        //                for (int i = 0; i < Lines.Length; i++)
        //                {
        //                    try
        //                    {
        //                        byte[] hex = StringToByteArray(Lines[i]);
        //                        if (hex[1] == Payload)
        //                        {

        //                            byte[] sid = new byte[4];
        //                            Buffer.BlockCopy(hex, 8, sid, 0, 4);
        //                            string id = BitConverter.ToString(sid).ToUpper().Replace("-", "");
        //                            if (streamid.Length == 0 || id == streamid)
        //                            {

        //                                Int16 lenteg = (Int16)(hex.Length - headerlen);
        //                                byte[] len = BitConverter.GetBytes(lenteg);
        //                                // byte[] len = new byte[] { 0x00, 0x00 };
        //                                //len[0] = (byte) ((hex.Length - headerlen) + 0);
        //                             //   writer.Write(len);
        //                               writer.Write(new byte[] {0x00, 0x00, 0x00, 0x00});
        //                                writer.Write(hex, headerlen, hex.Length - headerlen);
        //                            }
        //                        }
        //                    }
        //                    catch (Exception)
        //                    {

        //                    }
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    MessageBox.Show("Complete ");
    }

        //private void button5_Click(object sender, EventArgs e)
        //{
        //    string[] Lines = System.IO.File.ReadAllLines(textBox1.Text);
        //}

        //private void button6_Click(object sender, EventArgs e)
        //{
        //    ArrayList Payloads = new ArrayList();
        //    int packetCount = 0;
        //    try
        //    {
        //       string streamid = textBox4.Text.Trim().ToUpper();
        //      string outputfile = textBox1.Text.Substring(0, textBox1.Text.LastIndexOf(".")) + "_" + streamid + ".bit";
        //        // int Payload = Convert.ToInt32(textBox2.Text);
        //      //  string[] sPayload = textBox2.Text.Split(',');
             
        //    //    foreach (string py in sPayload)
        //      //  {
        //        //    Payloads.Add(Convert.ToInt32(py));
        //      //  }
        //    //    int headerlen = Convert.ToInt32(textBox3.Text);

        //        string[] Lines = System.IO.File.ReadAllLines(textBox1.Text);
        //        if (Lines.Length > 0)
        //        {
        //           // using (BinaryWriter writer = new BinaryWriter(File.Open(outputfile, FileMode.Create)))
        //           // {
        //               // writer.Write(new byte[] { 0x23, 0x21, 0x53, 0x49, 0x4C, 0x4B, 0x5F, 0x56, 0x33 });
        //                for (int i = 0; i < Lines.Length; i++)
        //                {
        //                    try
        //                    {
        //                        byte[] hex = StringToByteArray(Lines[i]);
        //                        Payloads.Add((int)hex[1])
        //                        ;

        //                    }
        //                    catch (Exception ee)
        //                    {
        //                        MessageBox.Show("" + ee);
        //                    }
        //                }
        //           // }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("" + ex);
        //    }
        //    richTextBox1.Clear();
        //    StringBuilder sb= new StringBuilder();
        //    foreach (var payload in Payloads)
        //    {
        //        if(!sb.ToString().Contains(payload.ToString()))
        //            richTextBox1.AppendText("\n"+payload);  //  sb.Append(payload+ ",");
        //    }

        //   // MessageBox.Show("payloads types are " + sb.ToString());
        //}
        //private void button3_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        richTextBox1.Clear();
        //        int Payload = Convert.ToInt32(textBox2.Text);
        //        string[] Lines = System.IO.File.ReadAllLines(textBox1.Text);
        //        ArrayList list = new ArrayList();
        //        if (Lines.Length > 0)
        //        {
        //            for (int i = 0; i < Lines.Length; i++)
        //            {
        //                try
        //                {
        //                    byte[] hex = StringToByteArray(Lines[i]);
        //                    if (hex[1] == Payload)
        //                    {
        //                        byte[] len = new byte[4];
        //                        Buffer.BlockCopy(hex, 8, len, 0, 4);
        //                        string id = BitConverter.ToString(len).ToUpper().Replace("-", "");
        //                        if (!list.Contains(id))
        //                        {
        //                            list.Add(id);
        //                            richTextBox1.AppendText(id + Environment.NewLine);
        //                        }
        //                    }
        //                }
        //                catch (Exception)
        //                {

        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    MessageBox.Show("Complete");
        //}
    }

