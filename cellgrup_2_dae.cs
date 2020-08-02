//Console.WriteLine("*");Console.WriteLine(.Count);
//int qq = 0 ; foreach ( var q in uvi_int ) { Console.WriteLine ( qq + " " + q ) ; qq++ ; }
#pragma warning disable 169, 414, 649

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
  using System; using System.IO; using System.Linq; using System.Text; 
  using System.Text.RegularExpressions; using System.Collections; 
  using System.Globalization; using System.Collections.Generic; 
  using Collada141; using System.Xml;using System.Xml.Serialization;
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

class Model
{
  public long BaseStreamPosition; // смещение модели от начала файла

  public int index; // порядковый номер появления в файле

  public int type; // может быть rdm***** или collmodc или mdlattr*

  public List<byte> content = new List<byte>(); // избыточность 

  public List<Vector3> positionList = new List<Vector3>() ; // вершины
  public List<Vector3> normalsList  = new List<Vector3>() ; // нормали
  public List<List<TRI>> subMeshFaces = new List<List<TRI>>(); // грани

  public List<Vector2> uvs0 = new List<Vector2>(); 
  public List<Vector2> uvs1 = new List<Vector2>(); 
  public List<List<Vector2>> uvsList = new List<List<Vector2>>();

  public List<float> vtxweighList = new List<float>() ;   // vtxweigh_index 
  public List<ushort> jointidxList = new List<ushort>();  // jointidx

  public List<ushort> texturesList = new List<ushort>();  // uv index
  public List<Texture> texture_List = new List<Texture>(); // 

  public List<Mlyrcolr> mlyrcolrList = new List<Mlyrcolr>();
  public List<Mlyrctrl> mlyrctrlList = new List<Mlyrctrl>();
  public List<Mtlctrl>  mtlctrlList  = new List<Mtlctrl>();
}

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

class Vector3   //  вершина
{
  public float x, y, z;

  public Vector3(BinaryReader br)
  {
    x = br.ReadSingle();
    y = br.ReadSingle();
    z = br.ReadSingle();
  }

  public override string ToString()   {    return String.Format( x + " " + y + " " + z );   }
}

//===============================================================================================

class Vector2
{
  public float u, v;

  public Vector2(BinaryReader br)
  {
      u = br.ReadSingle();
      v = br.ReadSingle();
  }
  
  public override string ToString()   {    return String.Format("uv " + u + " " + v );   }
}

//===============================================================================================

class TRI  //  грань
{
  public ushort vi0, vi1, vi2; 

  public TRI(BinaryReader br)
  {
    vi0 = br.ReadUInt16();
    vi1 = br.ReadUInt16();
    vi2 = br.ReadUInt16();
  }
  
  public override string ToString()  {
    return String.Format("f " + (vi0+1) + " " + (vi1+1) + " " + (vi2+1) );   }
}

//===============================================================================================

class Mlyrcolr // непонятная структура
{
  public byte r1, g1, b1, a1;
  public byte r2, g2, b2, a2;
  public byte r3, g3, b3, a3;
  public float alpha; // ???
  
  public Mlyrcolr(BinaryReader br)
  {
    r1=br.ReadByte(); g1=br.ReadByte(); b1=br.ReadByte(); a1=br.ReadByte(); 
    r2=br.ReadByte(); g2=br.ReadByte(); b2=br.ReadByte(); a2=br.ReadByte(); 
    r3=br.ReadByte(); g3=br.ReadByte(); b3=br.ReadByte(); a3=br.ReadByte(); 
    alpha=br.ReadSingle(); // как будто всегда 1.0f
  }
}

//===============================================================================================

class Mlyrctrl 
{
  public ushort us1, us2; // ?
  public float f;
  public int zero;
  
  public Mlyrctrl(BinaryReader br)
  {
    us1=br.ReadUInt16(); // ?
    us2=br.ReadUInt16(); // ?
    f = br.ReadSingle(); // 0.0f или 1.0f
    zero=br.ReadInt32(); // как будто всегда ноль
  }
}

//===============================================================================================

class Mtlctrl
{
  public int int_01234; // 3 в psntcnf.model // 4 в 
  public ushort z_4_or_8; // ??? что это ???
  public int int_1_or_2; 
  
  public Mtlctrl(BinaryReader br)
  {
    int_01234 = br.ReadInt32(); // 4 в 
    z_4_or_8 = br.ReadUInt16(); // 00 4C 00 00 или 00 8C 00 00 
    int_1_or_2 = br.ReadInt32(); // 0 или 1
  }
}

//===============================================================================================

class Texture
{
  // public int g_count; // ???????? откуда здесь это ????????
  public int count; 
  public List<ushort> index = new List<ushort>();
  public ushort ffff;

  public Texture(BinaryReader br)
  {
    count = br.ReadInt32(); 

    for (int i = 0; i < count; i++)
      index.Add(br.ReadUInt16()); 

    if (count % 2 != 0) ffff = br.ReadUInt16(); 
  }
}

//===============================================================================================

class CELLINST
{
  public int four; // 04 00 00 00 
  public ushort link1;
  public ushort link2;
  public int number; 
  public Vector3 position; 
  public Vector3 pivot; 
  public Vector3 rotation; 
  public int minus256; 

  public CELLINST(BinaryReader br)
  {
    four = br.ReadInt32(); 
    link1 = br.ReadUInt16();
    link2 = br.ReadUInt16();
    number = br.ReadInt32(); 
    position = new Vector3(br);
    pivot    = new Vector3(br); // ???
    rotation = new Vector3(br); // ???
    minus256 = br.ReadInt32();  // 00 FF FF FF
  }

  public override string ToString()
  {
    return String.Format(link1 + " " + link2 + " " + number 
    + " {{ " + position.ToString() 
    + " }} {{ " + pivot.ToString() 
    + " }} {{ " + rotation.ToString() + " }}");
  }
}

//===============================================================================================

class CELLMARK
{
  public int four; // 04 00 00 00 
  public int number1; // номер на сцене
  public int number2; // 
  public int number3; // 
  public int number4; // 
  public Vector3 position; 
  public Vector3 rotation; 
  public int hz1; // 0 или 1
  public int hz2; // 0 или 1
  public int hz3; // 0 или 1

  public CELLMARK(BinaryReader br)
  {
    four = br.ReadInt32(); 
    number1 = br.ReadInt32(); 
    number2 = br.ReadInt32(); 
    number3 = br.ReadInt32(); 
    number4 = br.ReadInt32(); 
    position = new Vector3(br);
    rotation = new Vector3(br);
    hz1 = br.ReadInt32(); 
    hz2 = br.ReadInt32(); 
    hz3 = br.ReadInt32(); 
  }
}

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

sealed class Scene
{
  static List<double> float_coords;

  static List<int> uvs_offset_int_list = new List<int>();     // для текущей модели
  static List<CELLINST> cellinst_List = new List<CELLINST>(); // для текущей сцены
  static List<CELLMARK> cellmark_List = new List<CELLMARK>(); // для текущей сцены

  static void Main()  
  {
    var filesName = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.cellgrup",  SearchOption.AllDirectories); // в конце можно будет юзать для *.big

    System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

    List<Model> Models = new List<Model>(); // список мешей на сцене
    
    foreach ( var file in filesName )
    {
      byte[] array1d = File.ReadAllBytes(file) ;

      int ci = 0; // счётчик вхождений 

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

      using (var br = new BinaryReader(File.Open(file, FileMode.Open)))
      {
        for ( int i = 0 ; i < array1d.Length ; i++ ) // проходим по всем байтам файла *.cellgrup
        {
          if (array1d[i+0] == 0x69 && array1d[i+1] == 0x00 && array1d[i+2] == 0x00 && array1d[i+3] == 0x00 &&  // i***
              array1d[i+4] == 0x64 && array1d[i+5] == 0x65 && array1d[i+6] == 0x66 && array1d[i+7] == 0x61 &&  // defa
              array1d[i+8] == 0x75 && array1d[i+9] == 0x6C && array1d[i+10]== 0x74 && array1d[i+11]== 0x00 )   // ult*
          {
            br.BaseStream.Position = i+12; // отступаем от "вхождения" на i***default* байт
            int BlockSize = br.ReadInt32(); // размер блока 
            br.ReadInt32(); // пропускаем пустые байты [00 00 00 00]

            int type = br.ReadInt32(); // "тип" модели

            if (type == 1819045731) ci++; // coll[modc]

            if (type == 1634493549) ci++; // mdla[ttr*]

            if (type == 6581618) // только для rmd*[****]
            {
              br.BaseStream.Position = i+4; // "возвращаемся", чтобы скопировать модель
              Model mesh = new Model(); // создаём модель
              mesh.BaseStreamPosition = br.BaseStream.Position;
              mesh.type = type;
              mesh.index = ci++; // присваиваем и увеличиваем индекс
              mesh.content = br.ReadBytes(BlockSize).ToList();
              Models.Add(mesh); // добавляем её в список моделей на "сцене"
            }
              i = i + BlockSize; // ускоряем поиск? 
          }

          // 63 65 6C 6C 69 6E 73 74 (места моделей)

          if (array1d[i+0] == 0x63 && array1d[i+1] == 0x65 && array1d[i+2] == 0x6C && array1d[i+3] == 0x6C && // cell
              array1d[i+4] == 0x69 && array1d[i+5] == 0x6E && array1d[i+6] == 0x73 && array1d[i+7] == 0x74)   // inst
          {
            br.BaseStream.Position = i+16;

            int count = br.ReadInt32(); // кол-во координат для расстановки моделей

            for(int j = 0; j < count; j++)
            {
              CELLINST cellinst = new CELLINST(br);
              cellinst_List.Add(cellinst);
            }
          }
          
          // 63 65 6C 6C 6D 61 72 6B cellmark

          if (array1d[i+0] == 0x63 && array1d[i+1] == 0x65 && array1d[i+2] == 0x6C && array1d[i+3] == 0x6C && // cell
              array1d[i+4] == 0x6D && array1d[i+5] == 0x61 && array1d[i+6] == 0x72 && array1d[i+7] == 0x6B)   // mark
          {
            br.BaseStream.Position = i+16;

            int count = br.ReadInt32(); // кол-во координат для расстановки моделей

            for(int j = 0; j < count; j++)
            {
              CELLMARK cellmark = new CELLMARK(br);
              cellmark_List.Add(cellmark);
            }
          }

        }

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

        foreach (var mesh in Models)
        {
            for ( int ji = 0 ; ji < mesh.content.Count ; ji++ )
            {
              // ИЩЕМ ВЕРШИНЫ // v // если нашли строку "position" = 00 00 00 00 70 6F 73 69 74 69 6F 6E 

              if (mesh.content[ji+0] == 0x70 && mesh.content[ji+1] == 0x6F && mesh.content[ji+2] == 0x73 && mesh.content[ji+3] == 0x69 && // 70 6F 73 69 
                  mesh.content[ji+4] == 0x74 && mesh.content[ji+5] == 0x69 && mesh.content[ji+6] == 0x6F && mesh.content[ji+7] == 0x6E )  // 74 69 6F 6E 
              {
                br.BaseStream.Position = mesh.BaseStreamPosition + ji+ 16; // +20, если отступаем OOOO_position

                int count = br.ReadInt32();
                for(int j = 0; j < count; j++)
                  mesh.positionList.Add ( new Vector3(br) ) ; 
              }

///////////// ИЩЕМ Vn (нормали) 

              if ( mesh.content[ji+0] == 0x6E && mesh.content[ji+1] == 0x6F && mesh.content[ji+2] == 0x72 && mesh.content[ji+3] == 0x6D && // 6E 6F 72 6D 
                   mesh.content[ji+4] == 0x61 && mesh.content[ji+5] == 0x6C && mesh.content[ji+6] == 0x73 && mesh.content[ji+7] == 0x00 )  // 61 6C 73 00
              {
                br.BaseStream.Position = mesh.BaseStreamPosition + ji + 16;
                int count = br.ReadInt32();
                for(int j = 0; j < count; j++)
                  mesh.normalsList.Add(new Vector3(br));
              }

///////////// ИЩЕМ ГРАНИ FACES // 70 72 69 6D // 73 ( 00 00 00 )

              if ( mesh.content[ji+0] == 0x70 && mesh.content[ji+1] == 0x72 && mesh.content[ji+2] == 0x69 && mesh.content[ji+3] == 0x6D ) // 70 72 69 6D 73
              {
                br.BaseStream.Position = mesh.BaseStreamPosition + ji + 16;
                
                int primsCount = br.ReadInt32();

                for ( int j = 0 ; j < primsCount; j++ )  
                {
                  int faceNumber = br.ReadInt32();  
                  int faceCount = br.ReadInt32();  
                  int faceSize = br.ReadInt32();  

                  List<TRI> face = new List<TRI>();

                  for ( int f = 0 ; f < faceCount; f++ )  
                    face.Add(new TRI(br));

                  mesh.subMeshFaces.Add(face);

                  for ( int jj = 0 ; jj < 10; jj++ ) br.ReadInt32(); // непонятные данные, мб для наложения текстур

                  if ((faceCount % 2) != 0) br.ReadUInt16(); // для "выравнивания"
                }
              }

///////////// ИЩЕМ uvs index , индексы текстурных координат // строку "textures" = // (00 00 FF FF) 74 65 78 74 75 72 65 73

    //        if ( mesh.content[ji+0] == 0x00 && mesh.content[ji+1] == 0x00 && mesh.content[ji+2]  == 0x00 && mesh.content[ji+3]  == 0x00 && // это необходимо ?
              if ( mesh.content[ji+0] == 0x74 && mesh.content[ji+1] == 0x65 && mesh.content[ji+2] == 0x78 && mesh.content[ji+3] == 0x74 && // если да
                   mesh.content[ji+4] == 0x75 && mesh.content[ji+5] == 0x72 && mesh.content[ji+6] == 0x65 && mesh.content[ji+7] == 0x73 ) // то изменить индексы
              {
                  br.BaseStream.Position = mesh.BaseStreamPosition + ji + 16; // или +20 в big файле 
                  int count = br.ReadInt32();
                  for(int j = 0; j < count; j++) 
                    mesh.texturesList.Add (br.ReadUInt16()) ; 
              }

///////////// ТЕКСТУРНЫЕ  КООРДИНАТЫ // vt // uvs. // 75 76 73 00 00 00 00 00 
              // может попастся два набора, поэтому пока что сохраняем смещение, а затем читаем что надо

              if(mesh.content[ji+0] == 0x75 && mesh.content[ji+1] == 0x76 && mesh.content[ji+2] == 0x73 && mesh.content[ji+3] == 0x00 ) // 75 76 73 00 
                uvs_offset_int_list.Add(ji);

///////////////
/*
              // mdlattr*

              if ( mesh.content[ji+0] == 0x6D && mesh.content[ji+1] == 0x64 && mesh.content[ji+2] == 0x6C && mesh.content[ji+3] == 0x61 &&
                   mesh.content[ji+4] == 0x74 && mesh.content[ji+5] == 0x74 && mesh.content[ji+6] == 0x72 && mesh.content[ji+7] == 0x00)
              {
                  br.BaseStream.Position = mesh.BaseStreamPosition + ji + 8;
                  int BlockSize = br.ReadInt32(); br.ReadInt32(); // 00 00 00 00 
                  
                  br.ReadInt32(); // 00 00 00 00 
                  br.ReadInt32(); // 00 00 00 00 

                  int defaultBlockSize = br.ReadInt32();

                  br.ReadInt32();
                  br.ReadInt32();
              }
*/
///////////// mtlctrl* // 6D 74 6C 63 74 72 6C 00 

              if ( mesh.content[ji+0] == 0x6D && mesh.content[ji+1] == 0x74 && mesh.content[ji+2] == 0x6C && mesh.content[ji+3] == 0x63 && // 6D 74 6C 63 
                   mesh.content[ji+4] == 0x74 && mesh.content[ji+5] == 0x72 && mesh.content[ji+6] == 0x6C && mesh.content[ji+7] == 0x00)   // 74 72 6C 00 
              {
                  br.BaseStream.Position = mesh.BaseStreamPosition + ji + 16;
                  int count = br.ReadInt32(); 
                  for ( int j = 0 ; j < count; j++ ) 
                  {
                      Mtlctrl mtlctrl = new Mtlctrl(br);
                      mesh.mtlctrlList.Add(mtlctrl);
                  }
              }

///////////// mlyrcolr ??? привильно ли я добавляю в список? по "сомнению" надо выделить три "строки" в один объект

              if ( mesh.content[ji+0] == 0x6D && mesh.content[ji+1] == 0x6C && mesh.content[ji+2] == 0x79 && mesh.content[ji+3] == 0x72 && // 6D 6C 79 72 
                   mesh.content[ji+4] == 0x63 && mesh.content[ji+5] == 0x6F && mesh.content[ji+6] == 0x6C && mesh.content[ji+7] == 0x72 )  // 63 6F 6C 72 
              {
                  br.BaseStream.Position = mesh.BaseStreamPosition + ji + 20;
                  int count = br.ReadInt32(); 

                  for ( int j = 0 ; j < count; j++ ) 
                  {
                      for ( int jj = 0 ; jj < 3; jj++ ) 
                      {
                        Mlyrcolr mlyrcolr = new Mlyrcolr(br);
                        mesh.mlyrcolrList.Add(mlyrcolr);
                      }
                  }
              }

///////////// mlyrctrl +++

              if ( mesh.content[ji+0] == 0x6D && mesh.content[ji+1] == 0x6C && mesh.content[ji+2] == 0x79 && mesh.content[ji+3] == 0x72 && // 6D 6C 79 72 
                   mesh.content[ji+4] == 0x63 && mesh.content[ji+5] == 0x74 && mesh.content[ji+6] == 0x72 && mesh.content[ji+7] == 0x6C )  // 63 74 72 6C 
              {
                  br.BaseStream.Position = mesh.BaseStreamPosition + ji + 16;
                  int flag = br.ReadInt32(); // 0 или 1 
                  int count = br.ReadInt32(); 

                  for ( int j = 0 ; j < count; j++ ) 
                  {
                      Mlyrctrl m = new Mlyrctrl(br);
                      mesh.mlyrctrlList.Add(m);
                  }
              }

///////////// texture* +++

              if ( mesh.content[ji+0] == 0x74 && mesh.content[ji+1] == 0x65 && mesh.content[ji+2] == 0x78 && mesh.content[ji+3] == 0x74 && // 74 65 78 74 
                   mesh.content[ji+4] == 0x75 && mesh.content[ji+5] == 0x72 && mesh.content[ji+6] == 0x65 && mesh.content[ji+7] == 0x00 )  // 75 72 65 00 
              {
                  br.BaseStream.Position = mesh.BaseStreamPosition + ji + 20; // br.BaseStream.Position = mesh.BaseStreamPosition + ji + 20;

                  int count = br.ReadInt32();

                  for(int j = 0; j < count; j++) 
                  {
                      Texture thz = new Texture(br);
                      mesh.texture_List.Add(thz);
                  }
              }

///////////// vtxweigh +++

              if ( mesh.content[ji+0] == 0x76 && mesh.content[ji+1] == 0x74 && mesh.content[ji+2] == 0x78 && mesh.content[ji+3] == 0x77 && // 76 74 78 77 
                   mesh.content[ji+4] == 0x65 && mesh.content[ji+5] == 0x69 && mesh.content[ji+6] == 0x67 && mesh.content[ji+7] == 0x68 )  // 65 69 67 68
              {
                  br.BaseStream.Position = mesh.BaseStreamPosition + ji + 16;
                  int count = br.ReadInt32();
                  for(int ii = 0; ii < count; ii++)
                    mesh.vtxweighList.Add(br.ReadSingle());
              }

///////////// jointidx +++

              if ( mesh.content[ji+0] == 0x6A && mesh.content[ji+1] == 0x6F && mesh.content[ji+2] == 0x69 && mesh.content[ji+3] == 0x6E && // 6A 6F 69 6E // join
                   mesh.content[ji+4] == 0x74 && mesh.content[ji+5] == 0x69 && mesh.content[ji+6] == 0x64 && mesh.content[ji+7] == 0x78 )  // 74 69 64 78 // tidx
              {
                  br.BaseStream.Position = mesh.BaseStreamPosition + ji + 16;

                  int count = br.ReadInt32();

                  for(int j = 0; j < count; j++) 
                    mesh.jointidxList.Add(br.ReadUInt16());

                  int ffff = 0;
                  if (count % 2 != 0) ffff = br.ReadUInt16(); 
              }

///////////// collmodc

            } // для массива внутри модели 

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

            if (uvs_offset_int_list.Count > 0)
            {
              br.BaseStream.Position = mesh.BaseStreamPosition + uvs_offset_int_list[0] + 8;
              int BlockSizeU0 = br.ReadInt32();  br.ReadInt32();
              int number0 = br.ReadInt32(); // 00 00 00 00
              int uvsCount = br.ReadInt32();

              for (int uvc = 0 ; uvc < uvsCount ; uvc++ ) 
                mesh.uvs0.Add ( new Vector2(br) ) ; 

              mesh.uvsList.Add(mesh.uvs0);
            }

            if (uvs_offset_int_list.Count > 1)
            {
              br.BaseStream.Position = mesh.BaseStreamPosition + uvs_offset_int_list[1] + 8;
              int BlockSizeU1 = br.ReadInt32();  br.ReadInt32();
              int number1 = br.ReadInt32(); // 01 00 00 00
              int uvsCount = br.ReadInt32();

              for (int uvc = 0 ; uvc < uvsCount ; uvc++ ) 
                mesh.uvs1.Add ( new Vector2(br) ) ; 

              mesh.uvsList.Add(mesh.uvs1);
            }

            uvs_offset_int_list.Clear(); // обязательно очищаем для другой модели

        } // для каждой модели

      } //  binary reader

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

        string name = file.Replace(".cellgrup", "");

        library_geometries lgeom = new library_geometries() // создаём библиотеку мешей
        {
            geometry = new geometry[Models.Count] // в библиотеке геометрии Models.Count мешей
        };

/////////////////////////////////////////////////////////////////////////////////////////////////

        int qqq = 0; // шагаем по списку геометрий в файле

        foreach (var mesh in Models) // для каждой модели
        {

//-----------------------------------------------------------------------------------------------

        //{ создаём массив координат для вершин модели

            float_array xyz_N_array = new float_array() // массив для координат
            {
              count = (ulong)mesh.positionList.Count * 3,
              id    = "mesh_" + mesh.index + "_positions_array"
            };

            float_coords = new List<double>();

            foreach(var fl in mesh.positionList) {
              float_coords.Add(fl.x);
              float_coords.Add(fl.y);
              float_coords.Add(fl.z);             }

            xyz_N_array.Values = float_coords.ToArray();

//----------------------------------

            source pos_source = new source()  //  источник для координат
            {
                Item  =  xyz_N_array,
                id    =  "mesh_" + mesh.index + "_positions",

                technique_common = new sourceTechnique_common()
                {
                    accessor = new accessor()
                    {
                        count  = (ulong)mesh.positionList.Count,
                        offset = 0L,
                        source = "#" + xyz_N_array.id,
                        stride = 3L,
                        param  = new param[]
                        {
                            new param()  {  name = "X",  type = "float" }, 
                            new param()  {  name = "Y",  type = "float" }, 
                            new param()  {  name = "Z",  type = "float" }
                        }
                    }
                }
            };
        //}

//-----------------------------------------------------------------------------------------------

/*
        //{ создаём массив координат для нормалей модели

            float_array xyz_Normals = new float_array()
            {
                count = (ulong)mesh.normalsList.Count * 3;
                id    = "mesh_" + mesh.index + "_normals_array";
            }

            float_coords = new List<double>();

            foreach(var fl in mesh.positionList)
            {
                float_coords.Add(fl.x);
                float_coords.Add(fl.y);
                float_coords.Add(fl.z);
            }

            xyz_Normals.Values = float_coords.ToArray();

//----------------------------------

            source norm_source = new source()
            {
                Item  =  xyz_N_array, 
                id    =  "mesh_" + mesh.index + "_positions", 

                technique_common = new sourceTechnique_common()
                {
                    accessor = new accessor()
                    {
                      count = (ulong)mesh.positionList.Count,
                      offset = 0L,
                      source = "#" + xyz_N_array.id,
                      stride = 3L,

                      param = new param[]
                      {
                        new param()  {  name = "X",  type = "float" }, 
                        new param()  {  name = "Y",  type = "float" }, 
                        new param()  {  name = "Z",  type = "float" }
                      }
                    }
                }
            };
        //}
*/

//-----------------------------------------------------------------------------------------------

            vertices v = new vertices()  //  вершины = часть объекта mesh
            {
                id    = "mesh_" + mesh.index + "_vertices", 

                input = new InputLocal[]
                {
                    new InputLocal() // пока что только коорднаты
                    {
                        semantic  =  "POSITION",
                        source    =  "#" + pos_source.id
                    }
                }
            };

//-----------------------------------------------------------------------------------------------

            int faceNumber = 0; 

            foreach ( var q in mesh.subMeshFaces)  
              foreach ( var qq in q )  
                faceNumber++;

            triangles tres = new triangles()  //  треугольники = часть объекта mesh
            {
              count = (ulong)faceNumber,
              input = new InputLocalOffset[]
              {
                new InputLocalOffset()  //  пока что только для координат
                {
                  semantic = "VERTEX",
                  offset = 0L,
                  source = "#" + v.id
                }
              }
            };

//----------------------------------

            StringBuilder all_TRI = new StringBuilder();

            foreach ( var q in mesh.subMeshFaces ) 
            {
              foreach ( var qq in q )  
              {
                string str = qq.vi0 + " " + qq.vi1 + " " + qq.vi2 + " ";
                all_TRI.Append(str);
              }
            }

            tres.p = all_TRI.ToString();

//-----------------------------------------------------------------------------------------------

            mesh m = new mesh()  //  создаём объект меша
            {
                vertices = v, 

                source = new source[1]  //  пока что только 1 источник для position
                {
                    pos_source
                },

                Items = new object[1]  //  для треугольников
                { 
                    tres
                }
            };

//-----------------------------------------------------------------------------------------------

            geometry geom = new geometry() // создаём оболочку для меши
            {
                id   = "mesh_" + mesh.index, // задаём ей имя mesh_№
                Item = m
            };

            lgeom.geometry[qqq++] = geom;

        } // для каждой модели в файле cellgrup создаём блоки с геометрией

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

        library_visual_scenes lvs = new library_visual_scenes(); // создаём библиотеку сцен

        visual_scene vs = new visual_scene()  //  создаём сцену, вроде всегда одна
        {
            id = "MyScene",                   //  обзываем её
            node = new node[Models.Count]     //  добавляем узлы для моделей на сцене
        };

//===============================================================================================

        qqq = 0; // шагаем по списку мешей, создаём им ноды, задаём расположение

        foreach (var mesh in Models)
        {

//----------------------------------

            node n = new node()
            {
                id = "mesh" + mesh.index, 

                instance_geometry = new instance_geometry[]
                {
                    new instance_geometry()
                    {
                        url = "#" + lgeom.geometry[qqq].id
                    }
                }
            } ;

//----------------------------------

            n.ItemsElementName = new ItemsChoiceType2[5]
            {
                ItemsChoiceType2.translate,
                ItemsChoiceType2.rotate,
                ItemsChoiceType2.rotate,
                ItemsChoiceType2.rotate,
                ItemsChoiceType2.scale
            };

//----------------------------------

            float xx = 0.0f; 
            float yy = 0.0f; 
            float zz = 0.0f; 

            float rx = 0.0f; 
            float ry = 0.0f; 
            float rz = 0.0f; 

            for (int ccc = 0; ccc < cellinst_List.Count; ccc++) 
            {
                if (mesh.index == cellinst_List[ccc].number) 
                {
                  xx = cellinst_List[ccc].position.x; 
                  yy = cellinst_List[ccc].position.y; 
                  zz = cellinst_List[ccc].position.z; 

                  rx = cellinst_List[ccc].rotation.x;
                  ry = cellinst_List[ccc].rotation.y;
                  rz = cellinst_List[ccc].rotation.z;
                }
            }

            for (int ccc = 0; ccc < cellmark_List.Count; ccc++) 
            {
                if (mesh.index == cellmark_List[ccc].number1)
                {
                  xx = cellmark_List[ccc].position.x; 
                  yy = cellmark_List[ccc].position.y; 
                  zz = cellmark_List[ccc].position.z; 

                  rx = cellmark_List[ccc].rotation.x; 
                  ry = cellmark_List[ccc].rotation.y; 
                  rz = cellmark_List[ccc].rotation.z; 
                }
            }

//----------------------------------

            n.Items = new object[5]
            {
                new TargetableFloat3() 
                {
                    sid    = "location", // translate
                    Values = new double[3] { xx, yy, zz } 
                },

                new rotate() { sid = "rotationX", Values = new double[4] { 0, 0, 1, rx } },
                new rotate() { sid = "rotationY", Values = new double[4] { 0, 1, 0, ry } },
                new rotate() { sid = "rotationZ", Values = new double[4] { 1, 0, 0, rz } },

                new TargetableFloat3() { sid    = "scale",  Values = new double[3] {1, 1, 1} }
            };

//----------------------------------

            vs.node[qqq] = n;

            qqq++;

        } // для каждой модели в файле cellgrup

//-----------------------------------------------------------------------------------------------

        lvs.visual_scene = new visual_scene[1] // создаём массив для сцен
        {
            vs // добавляем visual_scene в library_visual_scenes
        };

//-----------------------------------------------------------------------------------------------

        COLLADA collada = new COLLADA()
        {
            asset = new asset()
            {
                up_axis = UpAxisType.Z_UP
            }, 

            Items = new object[] // для библиотеки мешей и сцен
            {
                lgeom, // присваиваем колладе библиотеку геометрию
                lvs    // в массив Item добавляем библиотеку сцен
            },

            scene = new COLLADAScene()
            {
                instance_visual_scene = new InstanceWithExtra()
                {
                    url = "#" + vs.id
                }
            } 
        } ;

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

        collada.Save(name + ".dae");

        Models.Clear();

        cellinst_List.Clear();
        cellmark_List.Clear();

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

    } // для каждого cellgrup файла

  } // void Main()

} // class 


//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
