//Console.WriteLine("*");
//int qq = 0 ; foreach ( var q in uvi_int ) { Console.WriteLine ( qq + " " + q ) ; qq++ ; }

#pragma warning disable 169, 414, 649


//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
  using System; using System.IO; using System.Linq; using System.Text; 
  using System.Text.RegularExpressions; using System.Collections; 
  using System.Globalization; using System.Collections.Generic; 
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


sealed class model2obj
{
	static int vertexCount;
	static int normalCount;
	static int uvsCount;
	static int uvIndexCount; static List<int> uvs_offset_int_list = new List<int>();
	static int jointidxCount;
	static int vtxweighCount;
	static int primsCount;

//static string big_path  ; // хранит путь к файлу *.model
//static string writePath ; // хранит путь к файлу *[i].obj

	static int i = 0 ; // индекс в массиве байт всего файла

	// это для того , чтобы расчитать конец блока модели  
	// и записать в файл теги идущие после её геометрии
	static  int  i_begin = 0 ; // индекс начала блока модели
	static  int  i_offset = 0 ; // индекс смещения блока модели
	static  int  i_end = 0 ; // индекс конца блока модели


//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


	static void Main()	{


//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


    SearchOption SOAD = SearchOption.AllDirectories;
    string[] filesName = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.model",  SOAD); 
    System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");


//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


    List<Point> positionListPoint = new List<Point>() ; // вершины
    List<Normal> normalsListNormal  = new List<Normal>() ; // нормали
    List<Point2> vtListPoint2 = new List<Point2>() ; // uvs
    List<ushort> texturesList = new List<ushort>() ; // uv index
    List<ushort> jointidxList = new List<ushort>() ; // jointidx
    List<int> vtxweighList = new List<int>() ; // vtxweigh_index 
    
    List<Point2> uvs0 = new List<Point2>(); 
    List<Point2> uvs1 = new List<Point2>(); 
    List<List<Point2>> uvsList = new List<List<Point2>>();
    
    List<TRI> face; // грань
    List<List<TRI>> subMeshfaces = new List<List<TRI>>();


//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


    foreach ( var file in filesName )
    {
      byte[] array1d = File.ReadAllBytes(file) ;

      using (BinaryReader br = new BinaryReader(File.Open(file, FileMode.Open)))
      {


//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


        for ( i = 0 ; i < array1d.Length ; i++ ) // костыль // не доходя до конца модели 
     // потому что читаем по 12 байт в виде заголовка блока, иначе Исключение // Это было только из-за & в условиях -_-
        {

          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
          // ИЩЕМ ВЕРШИНЫ // v // если нашли строку "position" = 00 00 00 00 70 6F 73 69 74 69 6F 6E 
          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


          if ( /*array1d[i+0] == 0x00 && array1d[i+1] == 0x00 && array1d[i+2] == 0x00 && array1d[i+3]  == 0x00 &&*/
               array1d[i+0] == 0x70 && array1d[i+1] == 0x6F && array1d[i+2] == 0x73 && array1d[i+3] == 0x69 &&
               array1d[i+4] == 0x74 && array1d[i+5] == 0x69 && array1d[i+6] == 0x6F && array1d[i+7] == 0x6E )
          {
              br.BaseStream.Position = i+8; // +12 // отступаем /*0000*/position
              int BlockSize = br.ReadInt32();
              br.ReadInt32(); // 00 00 00 00 
              vertexCount = br.ReadInt32();

              for(int vc = 0; vc < vertexCount; vc++)
                positionListPoint.Add ( new Point(br) ) ; 
          }


          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
          // ИЩЕМ Vn (нормали)
          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


          if ( array1d[i+0] == 0x6E && array1d[i+1] == 0x6F && array1d[i+2] == 0x72 && array1d[i+3] == 0x6D &&
               array1d[i+4] == 0x61 && array1d[i+5] == 0x6C && array1d[i+6] == 0x73 && array1d[i+7] == 0x00 )
          {
              br.BaseStream.Position = i+8;
              int BlockSize = br.ReadInt32();
              br.ReadInt32(); // 00 00 00 00 
              normalCount = br.ReadInt32();

              for(int nc = 0; nc < normalCount; nc++)
                normalsListNormal.Add(new Normal(br));
          }


          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
          // ТЕКСТУРНЫЕ  КООРДИНАТЫ // vt // uvs. // 75 76 73 00 00 00 00 00 // может попастся два набора
          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


          if(array1d[i+0] == 0x75 && array1d[i+1] == 0x76 && array1d[i+2] == 0x73 && array1d[i+3] == 0x00 ) 
            uvs_offset_int_list.Add(i);


          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
          // ИЩЕМ uvs index , индексы текстурных координат 
          // если нашли строку "textures" = // (00 00 FF FF) 74 65 78 74 75 72 65 73
          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

//        if ( array1d[i+0] == 0x00 && array1d[i+1] == 0x00 && array1d[i+2]  == 0x00 && array1d[i+3]  == 0x00 && // это необходимо ?
          if ( array1d[i+0] == 0x74 && array1d[i+1] == 0x65 && array1d[i+2] == 0x78 && array1d[i+3] == 0x74 && // если да
               array1d[i+4] == 0x75 && array1d[i+5] == 0x72 && array1d[i+6] == 0x65 && array1d[i+7] == 0x73 ) // то изменить индексы
          {
              br.BaseStream.Position = i+8; // +12
              int BlockSize = br.ReadInt32();
              br.ReadInt32(); // 00 00 00 00 
              uvIndexCount = br.ReadInt32();

              for(int uvic = 0; uvic < uvIndexCount; uvic++) 
                texturesList.Add (br.ReadUInt16()) ; 
          }

/*
          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
          // если нашли строку "texture*" = // (00 00 FF FF) 74 65 78 74 75 72 65 00
          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


          if ( array1d[i+0] == 0x74 && array1d[i+1] == 0x65 && array1d[i+2] == 0x78 && array1d[i+3] == 0x74 &&
               array1d[i+4] == 0x75 && array1d[i+5] == 0x72 && array1d[i+6] == 0x65 && array1d[i+7] == 0x00 )
          {
              br.BaseStream.Position = i+12;
              int BlockSize = br.ReadInt32();
              br.ReadInt32(); // 00 00 00 00 
              uvIndexCount = br.ReadInt32();

              for(int uvic = 0; uvic < uvIndexCount; uvic++) 
                texturesList.Add (br.ReadUInt16()) ; 
          }
*/

          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
          // ИЩЕМ ГРАНИ FACES ( prim.____ ) // если нашли строку "prims..." = 70 72 69 6D 73 ( 00 00 00 )
          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


          if ( array1d[i+0] == 0x70 && array1d[i+1] == 0x72 && array1d[i+2] == 0x69 && array1d[i+3] == 0x6D )
          {
            br.BaseStream.Position = i+8;
            int BlockSize = br.ReadInt32();  
            
            br.ReadInt32(); // 00 00 00 00 
            
            primsCount = br.ReadInt32();  
            
            for ( int fi = 0 ; fi < primsCount; fi++ )	
            {
              int faceNumber = br.ReadInt32();  
              int faceCount = br.ReadInt32();  
              int faceSize = br.ReadInt32();  

              face = new List<TRI>();

              for ( int f = 0 ; f < faceCount; f++ )	
                face.Add(new TRI(br));

              subMeshfaces.Add(face);

              for ( int j = 0 ; j < 10; j++ ) br.ReadInt32();
              if ((faceCount % 2) != 0) br.ReadUInt16();
            }
          }


          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
          // jointidx
          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


          if ( array1d[i+0] == 0x6A && array1d[i+1] == 0x6F && array1d[i+2] == 0x69 && array1d[i+3] == 0x6E &&
               array1d[i+4] == 0x74 && array1d[i+5] == 0x69 && array1d[i+6] == 0x64 && array1d[i+7] == 0x78 )
          {
              br.BaseStream.Position = i+8;
              int BlockSize = br.ReadInt32();
              br.ReadInt32(); // 00 00 00 00 
              jointidxCount = br.ReadInt32();
              for(int ii = 0; ii < jointidxCount; ii++)
                jointidxList.Add(br.ReadUInt16());
          }


          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
          // vtxweigh
          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


          if ( array1d[i+0] == 0x76 && array1d[i+1] == 0x74 && array1d[i+2] == 0x78 && array1d[i+3] == 0x77 &&
               array1d[i+4] == 0x65 && array1d[i+5] == 0x69 && array1d[i+6] == 0x67 && array1d[i+7] == 0x68 )
          {
              br.BaseStream.Position = i+8;
              int BlockSize = br.ReadInt32();
              br.ReadInt32(); // 00 00 00 00 
              vtxweighCount = br.ReadInt32();
              for(int ii = 0; ii < vtxweighCount; ii++)
                vtxweighList.Add(br.ReadInt32());
          }


          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
          // mdlattr*
          //жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

/*
          if ( array1d[i+0] == 0x6D && array1d[i+1] == 0x64 && array1d[i+2] == 0x6C && array1d[i+3] == 0x61 &&
               array1d[i+4] == 0x74 && array1d[i+5] == 0x74 && array1d[i+6] == 0x72 && array1d[i+7] == 0x00)
          {
              br.BaseStream.Position = i+8;
              int BlockSize = br.ReadInt32();
              br.ReadInt32(); // 00 00 00 00 
              
              br.ReadInt32(); // 00 00 00 00 
              br.ReadInt32(); // 00 00 00 00 
              int defaultBlockSize = br.ReadInt32();

              br.ReadInt32();
              br.ReadInt32();
          }
*/

///////////////////////////////////////////////////////////////////////////////////////////////////

        } // for // прошли по всему содержимому массива байт прочитанных из файла


///////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////

        if (uvs_offset_int_list.Count > 0)
        {
          br.BaseStream.Position = uvs_offset_int_list[0] + 8;
          int BlockSizeU0 = br.ReadInt32();  br.ReadInt32();
          int number0 = br.ReadInt32(); // 00 00 00 00
          uvsCount = br.ReadInt32();

          for (int uvc = 0 ; uvc < uvsCount ; uvc++ ) 
            uvs0.Add ( new Point2(br) ) ; 

          uvsList.Add(uvs0);
        }

///////////////////////////////////////////////////////////////////////////////////////////////////


        if (uvs_offset_int_list.Count > 1)
        {
          br.BaseStream.Position = uvs_offset_int_list[1] + 8;
          int BlockSizeU1 = br.ReadInt32();  br.ReadInt32();
          int number1 = br.ReadInt32(); // 01 00 00 00
          uvsCount = br.ReadInt32();

          for (int uvc = 0 ; uvc < uvsCount ; uvc++ ) 
            uvs1.Add ( new Point2(br) ) ; 

          uvsList.Add(uvs1);
        }

        uvs_offset_int_list.Clear();


///////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////



        string name = file.Replace(".model", "");
        
      //Console.WriteLine(name);

        using (StreamWriter objw = new StreamWriter(name + ".obj"))
      //using (Streamobjw objw = File.AppendText(writePath))

        {


///////////////////////////////////////////////////////////////////////////////////////////////////


          //objw.WriteLine("# " + vertexCount + " vertices ");
          foreach(var p in positionListPoint) objw.WriteLine(p.ToString());
          positionListPoint.Clear();
          objw.WriteLine();


///////////////////////////////////////////////////////////////////////////////////////////////////


          //objw.WriteLine("\n# " + normalCount + " normals ");
          foreach(var p in normalsListNormal) objw.WriteLine(p.ToString());
          normalsListNormal.Clear();
          objw.WriteLine();


///////////////////////////////////////////////////////////////////////////////////////////////////

/*
          foreach ( var q in uvsList ) 
          {
            foreach ( var qq in q ) 
            {
              objw.WriteLine(qq + " ");
            }
            objw.WriteLine();
          }
          objw.WriteLine();
*/
          uvsList.Clear();


///////////////////////////////////////////////////////////////////////////////////////////////////


          int qqq = 0; 

          foreach ( var q in subMeshfaces ) 
          {
            objw.WriteLine("g face_" + qqq);
            objw.WriteLine("# usemtl ...");
            objw.WriteLine("# s 1");

            foreach ( var qq in q ) 
            {
              objw.WriteLine(qq + " ");
            }
            objw.WriteLine();

            qqq++ ;
          }

          subMeshfaces.Clear();


///////////////////////////////////////////////////////////////////////////////////////////////////


        }


///////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////


      } // binary reader 

    } // foreach // прошли по каждому файлу

  } // Main


//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж


class Point
{
  public float x, y, z;

  public Point(BinaryReader br)
  {
      x = br.ReadSingle();
      y = br.ReadSingle();
      z = br.ReadSingle();
  }

  public override string ToString() 
  {
      return String.Format("v " + x + " " + y + " " + z ); 
  }
}


//=======================================================================================


class Normal
{
  public float n1, n2, n3;

  public Normal(BinaryReader br)
  {
      n1 = br.ReadSingle();
      n2 = br.ReadSingle();
      n3 = br.ReadSingle();
  }

  public override string ToString() 
  {
      return String.Format("vn " + n1 + " " + n2 + " " + n3 ); 
  }
}


//=======================================================================================


class Point2
{
  public float u, v;

  public Point2(BinaryReader br)
  {
      u = br.ReadSingle();
      v = br.ReadSingle();
  }
  
  public override string ToString() 
  {
      return String.Format("vt " + u + " " + v ); 
  }
}


//=======================================================================================


class TRI
{
  public ushort vi0, vi1, vi2; 

  public TRI(BinaryReader br)
  {
    vi0 = br.ReadUInt16();
    vi1 = br.ReadUInt16();
    vi2 = br.ReadUInt16();
  }
  
  public override string ToString() 
  {
      return String.Format("f " + (vi0+1) + " " + (vi1+1) + " " + (vi2+1) ); 
  }
}


//=======================================================================================

} // class
