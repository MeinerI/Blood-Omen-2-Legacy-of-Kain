using System; using System.IO; 
//using System.Linq;
using System.Collections.Generic; 

class Finder
{
  static void Main()  
  {
    var big_file_names = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.big",  SearchOption.AllDirectories); 

    foreach ( var fname in big_file_names )
    {
      int dds_counter = 0 ; 
      byte[] BigFileBytesArray = File.ReadAllBytes(fname); 

      using (var br = new BinaryReader(File.Open(fname, FileMode.Open)))
      {
        for ( int offset = 0 ; offset < BigFileBytesArray.Length ; offset++ ) 
        {
          if (FindHexString("texture\0", BigFileBytesArray, offset    ) && 
              FindHexString("symlist\0", BigFileBytesArray, offset+20))
          {
            br.BaseStream.Position = offset + 40;
            int ddsSize = br.ReadInt32(); 

            int ff_skip = 0; //надо скипнуть FF

            if (FindHexString("DDS |\0\0\0", BigFileBytesArray, offset+44)) ff_skip = 44;
            if (FindHexString("DDS |\0\0\0", BigFileBytesArray, offset+48)) ff_skip = 48;
            if (FindHexString("DDS |\0\0\0", BigFileBytesArray, offset+52)) ff_skip = 52;
            if (FindHexString("DDS |\0\0\0", BigFileBytesArray, offset+56)) ff_skip = 56;

            br.BaseStream.Position = offset + ff_skip;

// array1d.Skip(offset+44).Take(ddsSize).Skip(0).Take(BigFileBytesArray.Length).SkipWhile(x =>(x==0xFF)).ToArray();

            byte[] ddsByteArray = new byte[ddsSize];
                    ddsByteArray = br.ReadBytes(ddsSize); 

            string ddsWritePath = fname + dds_counter + ".dds" ; 
            File.WriteAllBytes( ddsWritePath , ddsByteArray ) ; 

            dds_counter++ ; // увеличиваем префикс имени файла
          }
        }
      }
    }
  }

  static bool FindHexString(string str, IReadOnlyList<byte> list, int i)
  {
    if (list[i+0] == str[0] && list[i+1] == str[1] && list[i+2] == str[2] && list[i+3] == str[3] &&
        list[i+4] == str[4] && list[i+5] == str[5] && list[i+6] == str[6] && list[i+7] == str[7] )
    return true;
    return false;
  }
}
