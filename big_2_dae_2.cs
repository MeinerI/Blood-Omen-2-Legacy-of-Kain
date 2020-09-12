//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
  using System; using System.IO; using System.Linq; using System.Text; 
  using System.Text.RegularExpressions; using System.Collections; 
  using System.Globalization; using System.Collections.Generic; 
  using Collada141; using System.Xml;using System.Xml.Serialization;
  using System.Runtime.Serialization.Formatters.Binary;
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

sealed class Scene
{
  static List<double> float_coords;

  static int index_in_big = 0; // индекс появления "сущности" в big файле

	static List<int> uvs_offset_int_list = new List<int>(); // для текущей модели

	static bool region_flag   = true; // ищем только первое вхождение
	static bool planinfo_flag = true; // чтобы считать блок целиком

	static List<CELLINST> cellinst_List = new List<CELLINST>(); // для текущей сцены
	static List<CELLMARK> cellmark_List = new List<CELLMARK>(); // для текущей сцены

	static List<Model> MESHES_IN_CELLGRUP_LIST = new List<Model>(); // список мешей на сцене
	static List<Model> MODELS_IN_BIG_FILE_LIST = new List<Model>(); // список моделей на сцене

	static byte[] cellgrupArray; // массив для хранения содержимого cellgrup файла
	static byte[] planinfoArray; // массив для хранения содержимого planinfo файла
	static byte[] regionArray;   // массив для хранения содержимого region   файла

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

	static void Main()	
	{
    var filesName = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.big",  SearchOption.AllDirectories); 

    System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

    foreach ( var file in filesName )
    {
      byte[] array1d = File.ReadAllBytes(file) ;

      int number_in_cellgrup = 0; // счётчик вхождений 
      int number_of_model = 0;

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

      using (var br = new BinaryReader(File.Open(file, FileMode.Open)))
      {
        for ( int i = 0 ; i < array1d.Length ; i++ ) // проходим по всем байтам файла *.big
        {

////////  model*** 6D 6F 64 65 6C 00 00 00 (это модели на уровне)

          if (array1d[i+0] == 0x6D && array1d[i+1] == 0x6F && array1d[i+2] == 0x64 && array1d[i+3] == 0x65 && // mode
              array1d[i+4] == 0x6C && array1d[i+5] == 0x00 && array1d[i+6] == 0x00 && array1d[i+7] == 0x00 )  // l***
          {
              br.BaseStream.Position = i+8;    // отсупаем 8 байт 
              int BlockSize = br.ReadInt32();  // читаем размер блока
              br.BaseStream.Position = i;      // "возвращаемся", чтобы скопировать модель
              Model model = new Model();       // создаём объект класса модель

              model.BaseStreamPositionOffset = br.BaseStream.Position; // 

              model.type = "model";                   // тип model
              model.model_index = number_of_model++;  // присваиваем и увеличиваем индекс
              model.index_in_big = index_in_big++;    // 
              model.content = br.ReadBytes(BlockSize).ToList();  // 
              MODELS_IN_BIG_FILE_LIST.Add(model);                // добавляем "модель" в список
          }

////////  cellgrup 63 65 6C 6C 67 72 75 70 (это блок геометрии статичных объектов на уровне)

          if (array1d[i+0] == 0x63 && array1d[i+1] == 0x65 && array1d[i+2] == 0x6C && array1d[i+3] == 0x6C && // cell
              array1d[i+4] == 0x67 && array1d[i+5] == 0x72 && array1d[i+6] == 0x75 && array1d[i+7] == 0x70 )  // grup
          {
              br.BaseStream.Position = i+8;
              int BlockSize = br.ReadInt32();
              br.BaseStream.Position = i;

              if (BlockSize > 100000)
              {
                cellgrupArray = new byte[BlockSize];     // создаём массив нужного размера
                cellgrupArray = br.ReadBytes(BlockSize); // читаем в него байты из файла с (текущей позиции+8)

                using (MemoryStream memory = new MemoryStream(cellgrupArray)) // сохраняем cellgrup во "временный" файл
                {
                  using (BinaryReader brm = new BinaryReader(memory)) //  читаем из этого файла 
                  {
                    for ( int ii = 0 ; ii < cellgrupArray.Length ; ii++ ) // вдоль всего массива байт
                    {
                      if (cellgrupArray[ii+0] == 0x69 && cellgrupArray[ii+1] == 0x00 && cellgrupArray[ii+2] == 0x00 && cellgrupArray[ii+3] == 0x00 &&  // i***
                          cellgrupArray[ii+4] == 0x64 && cellgrupArray[ii+5] == 0x65 && cellgrupArray[ii+6] == 0x66 && cellgrupArray[ii+7] == 0x61 &&  // defa
                          cellgrupArray[ii+8] == 0x75 && cellgrupArray[ii+9] == 0x6C && cellgrupArray[ii+10]== 0x74 && cellgrupArray[ii+11]== 0x00 )   // ult*
                      {
                        brm.BaseStream.Position = ii+12; // отступаем от "вхождения" на i***default* байт
                        BlockSize = brm.ReadInt32(); // размер блока 
                        brm.ReadInt32(); // пропускаем пустые байты [00 00 00 00]

                        int type = brm.ReadInt32(); // "тип" модели

                        if (type == 1819045731) number_in_cellgrup++; // coll[modc]

                        if (type == 1634493549) number_in_cellgrup++; // mdla[ttr*]

                        if (type == 6581618) // только для rmd*[****]
                        {
                          brm.BaseStream.Position = ii+4; // "возвращаемся", чтобы скопировать модель
                          Model mesh = new Model(); // создаём модель
                          mesh.BaseStreamPositionOffset = brm.BaseStream.Position + i; // какая важная строчка! :)
                          if (mesh.type != "model") mesh.type = "rmd";

                          mesh.cells_index = number_in_cellgrup++; // присваиваем и увеличиваем индекс
                          mesh.index_in_big = index_in_big++;

                          mesh.content = brm.ReadBytes(BlockSize).ToList();
                          MESHES_IN_CELLGRUP_LIST.Add(mesh); // добавляем её в список моделей на "сцене"
                        }
                          // i = i + BlockSize; // ускоряем поиск? 
                      }
                    }
                  }
                } // in memory
              }
          }

////////  cellinst 63 65 6C 6C 69 6E 73 74 (места растановки статичных объектов на уровне)

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
          
////////  cellmark 63 65 6C 6C 6D 61 72 6B (места расстановки моделей на уровне)

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

////////  planinfo 70 6C 61 6E 69 6E 66 6F (тоже нужные координаты)

          if (array1d[i+0] == 0x70 && array1d[i+1] == 0x6C && array1d[i+2] == 0x61 && array1d[i+3] == 0x6E &&
              array1d[i+4] == 0x69 && array1d[i+5] == 0x6E && array1d[i+6] == 0x66 && array1d[i+7] == 0x6F && planinfo_flag == true)
          {
              planinfo_flag = false; // нашли
              br.BaseStream.Position = i+8;
              int BlockSize = br.ReadInt32();
              br.BaseStream.Position = i;

              br.ReadInt32();br.ReadInt32(); // planinfo // 70 6C 61 6E 69 6E 66 6F
              br.ReadInt32();br.ReadInt32(); // size

              int digit = br.ReadInt32();    // хз
          //--------------------------------
              br.ReadInt32();br.ReadInt32(); // symlist* // 73 79 6D 6C 69 73 74 00
              br.ReadInt32();br.ReadInt32(); // size
              br.ReadInt32(); // содержимое
          //--------------------------------
              int size = br.ReadInt32(); 
              
    //костыль{
              long offset = br.BaseStream.Position;
              int ffffffff1 = br.ReadInt32(); // бывает встречается 
              int ffffffff2 = 0;
              if (ffffffff1 == -1) ffffffff2 = br.ReadInt32(); 
              else br.BaseStream.Position = offset;
    //костыль}

              br.ReadInt32();br.ReadInt32(); // planinfo // 70 6C 61 6E 69 6E 66 6F
              br.ReadInt32(); // size
              br.ReadInt32(); // хз 
              
              br.ReadInt32();br.ReadInt32(); // plannode // 70 6C 61 6E 6E 6F 64 65
              br.ReadInt32();br.ReadInt32(); // size

              ushort hz1 = br.ReadUInt16(); // хз
              ushort cnt = br.ReadUInt16(); // количество 
              ushort hz3 = br.ReadUInt16(); // хз
              ushort ffff= br.ReadUInt16(); // FF FF // "выравниватель"
          //--------------------------------
              int one_hundred = br.ReadInt32(); // 64 00 00 00

              for (int h = 0; h < one_hundred; h++)
              {
                plannode_stuff_1 pns1 = new plannode_stuff_1(br);
              }
          //--------------------------------
              int five_hundred = br.ReadInt32(); // F4 01 00 00 

              for (int h = 0; h < five_hundred; h++)
              {
                br.ReadInt32();br.ReadInt32();
              }
          //--------------------------------
              int blocks_count = br.ReadInt32(); 
              
              for (int h = 0; h < blocks_count; h++)
              {
                int count = br.ReadInt32(); 
                
                for (int hh = 0; hh < count; hh++)
                {
                  plannode_stuff_2 pns2 = new plannode_stuff_2(br);
                }
              }
          //--------------------------------
              br.ReadInt32();br.ReadInt32(); // planblok
              br.ReadInt32();br.ReadInt32(); // size

              int planblok_count = br.ReadInt32(); 

              for (int h = 0; h < planblok_count; h++)
              {
                planblok_stuff pbs = new planblok_stuff(br); 
              }
          }

        } // for 

      } // using

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

    int mesh1count = MESHES_IN_CELLGRUP_LIST.Count; 
    int mesh2count = MODELS_IN_BIG_FILE_LIST.Count; 

    foreach (var model in MODELS_IN_BIG_FILE_LIST)
      model.model_index = model.model_index + MESHES_IN_CELLGRUP_LIST[mesh1count-1].cells_index + 1; // без +1 "крайние" индексы совпадают

    MESHES_IN_CELLGRUP_LIST.AddRange(MODELS_IN_BIG_FILE_LIST); // 17 + 26 = 43

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

      using (var br = new BinaryReader(File.Open(file, FileMode.Open)))
      {
        foreach (var mesh in MESHES_IN_CELLGRUP_LIST) // для каждой модели
        {
            for ( int ji = 0 ; ji < mesh.content.Count ; ji++ )
            {

////////////  ИЩЕМ ВЕРШИНЫ // v // если нашли строку "position" = 00 00 00 00 70 6F 73 69 74 69 6F 6E 

              if (mesh.content[ji+0] == 0x70 && mesh.content[ji+1] == 0x6F && mesh.content[ji+2] == 0x73 && mesh.content[ji+3] == 0x69 && // 70 6F 73 69 
                  mesh.content[ji+4] == 0x74 && mesh.content[ji+5] == 0x69 && mesh.content[ji+6] == 0x6F && mesh.content[ji+7] == 0x6E )  // 74 69 6F 6E 
              {
                br.BaseStream.Position = mesh.BaseStreamPositionOffset + ji + 16; // +20, если отступаем OOOO_position

                int count = br.ReadInt32();
                for(int j = 0; j < count; j++)
                  mesh.positionList.Add ( new Vector3(br) ) ; 
              }

////////////  ИЩЕМ Vn (нормали) 

              if ( mesh.content[ji+0] == 0x6E && mesh.content[ji+1] == 0x6F && mesh.content[ji+2] == 0x72 && mesh.content[ji+3] == 0x6D && // 6E 6F 72 6D 
                   mesh.content[ji+4] == 0x61 && mesh.content[ji+5] == 0x6C && mesh.content[ji+6] == 0x73 && mesh.content[ji+7] == 0x00 )  // 61 6C 73 00
              {
                br.BaseStream.Position = mesh.BaseStreamPositionOffset + ji + 16;
                int count = br.ReadInt32();
                for(int j = 0; j < count; j++)
                  mesh.normalsList.Add(new Vector3(br));
              }

////////////  ИЩЕМ ГРАНИ FACES // 70 72 69 6D 73 00 00 00

              if ( mesh.content[ji+0] == 0x70 && mesh.content[ji+1] == 0x72 && mesh.content[ji+2] == 0x69 && mesh.content[ji+3] == 0x6D && // 70 72 69 6D
                   mesh.content[ji+4] == 0x73 && mesh.content[ji+5] == 0x00 && mesh.content[ji+6] == 0x00 && mesh.content[ji+7] == 0x00 )  // 73 00 00 00
              {
                br.BaseStream.Position = mesh.BaseStreamPositionOffset + ji + 16;

                int primsCount = br.ReadInt32();

                for ( int j = 0 ; j < primsCount; j++ )	
                {
                  int faceNumber = br.ReadInt32();  
                  int faceCount = br.ReadInt32();  
                  int faceSize = br.ReadInt32(); 

                  List<TRI> face = new List<TRI>();

                  for ( int f = 0 ; f < faceCount; f++ )	
                    face.Add(new TRI(br, faceNumber));

                  mesh.subMeshFaces.Add(face);

                  if ((faceCount % 2) != 0) br.ReadUInt16(); // для "выравнивания" читается FF FF

              //  непонятные данные, мб для наложения текстур, не проверял
              //  {
                    int hz_count0 = br.ReadUInt16();
                    br.ReadUInt16(); // всегда 01 00 ?

                    int hz_count1 = br.ReadInt32(); 
                    for ( int jj = 0 ; jj < hz_count1; jj++ ) 
                      br.ReadInt32();

                    int hz_count01 = br.ReadInt32(); // обычно равно hz_count0
                    for ( int jj = 0 ; jj < hz_count01; jj++ ) 
                    {
                      br.ReadSingle(); br.ReadSingle(); 
                    }
                    
                    int hz_count02 = br.ReadInt32(); // обычно равно hz_count0
                    for ( int jj = 0 ; jj < hz_count02; jj++ ) 
                    {
                      br.ReadSingle(); br.ReadSingle(); br.ReadSingle(); br.ReadSingle(); 
                    }
                    
                    br.ReadSingle(); br.ReadSingle(); 
                    br.ReadSingle(); br.ReadSingle(); 
                    
                    br.ReadInt32(); // 00 00 00 00 
                    br.ReadInt32(); // 00 00 FF FF 
              //  }
                }

              }

////////////  ИЩЕМ uvs index , индексы текстурных координат // строку "textures" = // (00 00 FF FF) 74 65 78 74 75 72 65 73

    //        if ( mesh.content[ji+0] == 0x00 && mesh.content[ji+1] == 0x00 && mesh.content[ji+2]  == 0x00 && mesh.content[ji+3]  == 0x00 && // это необходимо ?
              if ( mesh.content[ji+0] == 0x74 && mesh.content[ji+1] == 0x65 && mesh.content[ji+2] == 0x78 && mesh.content[ji+3] == 0x74 && // если да
                   mesh.content[ji+4] == 0x75 && mesh.content[ji+5] == 0x72 && mesh.content[ji+6] == 0x65 && mesh.content[ji+7] == 0x73 ) // то изменить индексы
              {
                  br.BaseStream.Position = mesh.BaseStreamPositionOffset + ji + 16; // или +20 в big файле 
                  int count = br.ReadInt32();
                  for(int j = 0; j < count; j++) 
                    mesh.texturesList.Add (br.ReadUInt16()) ; 
              }

////////////  ТЕКСТУРНЫЕ  КООРДИНАТЫ // vt // uvs. // 75 76 73 00 00 00 00 00 
              // может попастся два набора, поэтому пока что сохраняем смещение, а затем читаем что надо

              if(mesh.content[ji+0] == 0x75 && mesh.content[ji+1] == 0x76 && mesh.content[ji+2] == 0x73 && mesh.content[ji+3] == 0x00 ) // 75 76 73 00 
                uvs_offset_int_list.Add(ji);

////////////  mdlattr*

              if ( mesh.content[ji+0] == 0x6D && mesh.content[ji+1] == 0x64 && mesh.content[ji+2] == 0x6C && mesh.content[ji+3] == 0x61 &&
                   mesh.content[ji+4] == 0x74 && mesh.content[ji+5] == 0x74 && mesh.content[ji+6] == 0x72 && mesh.content[ji+7] == 0x00)
              {
                  br.BaseStream.Position = mesh.BaseStreamPositionOffset + ji + 8;
                  int BlockSize = br.ReadInt32(); br.ReadInt32(); // 00 00 00 00 
                  
                  br.ReadInt32(); // 00 00 00 00 
                  br.ReadInt32(); // 00 00 00 00 

                  int defaultBlockSize = br.ReadInt32();

                  br.ReadInt32();
                  br.ReadInt32();
              }

////////////  mtlctrl* // 6D 74 6C 63 74 72 6C 00 

              if ( mesh.content[ji+0] == 0x6D && mesh.content[ji+1] == 0x74 && mesh.content[ji+2] == 0x6C && mesh.content[ji+3] == 0x63 && // 6D 74 6C 63 
                   mesh.content[ji+4] == 0x74 && mesh.content[ji+5] == 0x72 && mesh.content[ji+6] == 0x6C && mesh.content[ji+7] == 0x00)   // 74 72 6C 00 
              {
                  br.BaseStream.Position = mesh.BaseStreamPositionOffset + ji + 16;
                  int count = br.ReadInt32(); 
                  for ( int j = 0 ; j < count; j++ ) 
                  {
                      Mtlctrl mtlctrl = new Mtlctrl(br);
                      mesh.mtlctrlList.Add(mtlctrl);
                  }
              }

////////////  mlyrcolr ??? привильно ли я добавляю в список? по "сомнению" надо выделить три "строки" в один объект

              if ( mesh.content[ji+0] == 0x6D && mesh.content[ji+1] == 0x6C && mesh.content[ji+2] == 0x79 && mesh.content[ji+3] == 0x72 && // 6D 6C 79 72 
                   mesh.content[ji+4] == 0x63 && mesh.content[ji+5] == 0x6F && mesh.content[ji+6] == 0x6C && mesh.content[ji+7] == 0x72 )  // 63 6F 6C 72 
              {
                  br.BaseStream.Position = mesh.BaseStreamPositionOffset + ji + 20;
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

////////////  mlyrctrl +++

              if ( mesh.content[ji+0] == 0x6D && mesh.content[ji+1] == 0x6C && mesh.content[ji+2] == 0x79 && mesh.content[ji+3] == 0x72 && // 6D 6C 79 72 
                   mesh.content[ji+4] == 0x63 && mesh.content[ji+5] == 0x74 && mesh.content[ji+6] == 0x72 && mesh.content[ji+7] == 0x6C )  // 63 74 72 6C 
              {
                  br.BaseStream.Position = mesh.BaseStreamPositionOffset + ji + 16;
                  int flag = br.ReadInt32(); // 0 или 1 
                  int count = br.ReadInt32(); 

                  for ( int j = 0 ; j < count; j++ ) 
                  {
                      Mlyrctrl m = new Mlyrctrl(br);
                      mesh.mlyrctrlList.Add(m);
                  }
              }

////////////  texture* +++
/*
              if ( mesh.content[ji+0] == 0x74 && mesh.content[ji+1] == 0x65 && mesh.content[ji+2] == 0x78 && mesh.content[ji+3] == 0x74 && // 74 65 78 74 
                   mesh.content[ji+4] == 0x75 && mesh.content[ji+5] == 0x72 && mesh.content[ji+6] == 0x65 && mesh.content[ji+7] == 0x00 )  // 75 72 65 00 
              {
                  br.BaseStream.Position = mesh.BaseStreamPositionOffset + ji + 20; 

                  int count = br.ReadInt32();

                  for(int j = 0; j < count; j++) 
                  {
                      Texture thz = new Texture(br);
                      mesh.texture_List.Add(thz);
                  }
              }
*/
////////////  vtxweigh +++

              if ( mesh.content[ji+0] == 0x76 && mesh.content[ji+1] == 0x74 && mesh.content[ji+2] == 0x78 && mesh.content[ji+3] == 0x77 && // 76 74 78 77 
                   mesh.content[ji+4] == 0x65 && mesh.content[ji+5] == 0x69 && mesh.content[ji+6] == 0x67 && mesh.content[ji+7] == 0x68 )  // 65 69 67 68
              {
                  br.BaseStream.Position = mesh.BaseStreamPositionOffset + ji + 16;
                  int count = br.ReadInt32();
                  for(int ii = 0; ii < count; ii++)
                    mesh.vtxweighList.Add(br.ReadSingle());
              }

////////////  jointidx +++

              if ( mesh.content[ji+0] == 0x6A && mesh.content[ji+1] == 0x6F && mesh.content[ji+2] == 0x69 && mesh.content[ji+3] == 0x6E && // 6A 6F 69 6E // join
                   mesh.content[ji+4] == 0x74 && mesh.content[ji+5] == 0x69 && mesh.content[ji+6] == 0x64 && mesh.content[ji+7] == 0x78 )  // 74 69 64 78 // tidx
              {
                  br.BaseStream.Position = mesh.BaseStreamPositionOffset + ji + 16;

                  int count = br.ReadInt32();

                  for(int j = 0; j < count; j++) 
                    mesh.jointidxList.Add(br.ReadUInt16());

                  int ffff = 0;
                  if (count % 2 != 0) ffff = br.ReadUInt16(); 
              }

////////////  collmodc

            } // для массива внутри модели 

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

              if (uvs_offset_int_list.Count > 0) // читаем первое UV
              {
                br.BaseStream.Position = mesh.BaseStreamPositionOffset + uvs_offset_int_list[0] + 8;
                int BlockSizeU0 = br.ReadInt32();  br.ReadInt32();
                int number0 = br.ReadInt32(); // 00 00 00 00
                int uvsCount = br.ReadInt32();

                for (int uvc = 0 ; uvc < uvsCount ; uvc++ ) 
                  mesh.uvs0.Add ( new Vector2(br) ) ; 

                mesh.uvsList.Add(mesh.uvs0);
              }

              if (uvs_offset_int_list.Count > 1) // читаем второе UV
              {
                br.BaseStream.Position = mesh.BaseStreamPositionOffset + uvs_offset_int_list[1] + 8;
                int BlockSizeU1 = br.ReadInt32();  br.ReadInt32();
                int number1 = br.ReadInt32(); // 01 00 00 00
                int uvsCount = br.ReadInt32();

                for (int uvc = 0 ; uvc < uvsCount ; uvc++ ) 
                  mesh.uvs1.Add ( new Vector2(br) ) ; 

                mesh.uvsList.Add(mesh.uvs1);
              }

              uvs_offset_int_list.Clear(); // обязательно очищаем для другой модели

        } // для каждой модели
      }
    

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

        string name = file.Replace(".big", "");

        //+++MESHES_IN_CELLGRUP_LIST.AddRange(MODELS_IN_BIG_FILE_LIST); // 17 + 26 = 43

        library_geometries lgeom = new library_geometries() // создаём библиотеку мешей
        {
            geometry = new geometry[MESHES_IN_CELLGRUP_LIST.Count + MODELS_IN_BIG_FILE_LIST.Count] // в библиотеке геометрии MESHES_IN_CELLGRUP_LIST.Count мешей
        };

/////////////////////////////////////////////////////////////////////////////////////////////////

        int qqq = 0; // шагаем по списку геометрий в файле

        int m_index = 0;

        foreach (var mesh in MESHES_IN_CELLGRUP_LIST) // для каждой модели
        {
            if (mesh.type == "model") m_index = mesh.model_index;
            if (mesh.type == "rmd"  ) m_index = mesh.cells_index;

//===============================================================================================

        //{ создаём массив координат для вершин модели

            float_array xyz_N_array = new float_array() // массив для координат
            {
              count = (ulong)mesh.positionList.Count * 3,
              id    = "mesh_" + m_index + "_positions_array"
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
                id    =  "mesh_" + m_index + "_positions",

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

//===============================================================================================

        //{ создаём массив координат для нормалей модели

            float_array xyz_Normals = new float_array()
            {
                count = (ulong)mesh.normalsList.Count * 3, 
                id    = "mesh_" + m_index + "_normals_array"
            };

            float_coords = new List<double>();

            foreach(var fl in mesh.normalsList)
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
                id    =  "mesh_" + m_index + "_normals", 

                technique_common = new sourceTechnique_common()
                {
                    accessor = new accessor()
                    {
                      count = (ulong)mesh.normalsList.Count,
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

//===============================================================================================

            vertices v = new vertices()  //  вершины = часть объекта mesh
            {
                id    = "mesh_" + m_index + "_vertices", 

                input = new InputLocal[]
                {
                    new InputLocal() // пока что только коорднаты
                    {
                        semantic  =  "POSITION",
                        source    =  "#" + pos_source.id
                    }
                }
            };

//===============================================================================================

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

//===============================================================================================

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

//===============================================================================================

            geometry geom = new geometry() // создаём оболочку для меши
            {
                id   = "mesh_" + m_index, // задаём ей имя mesh_№
                Item = m
            };

            lgeom.geometry[qqq++] = geom;

        } // для каждой модели в файле cellgrup создаём блоки с геометрией

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

        library_visual_scenes lvs = new library_visual_scenes(); // создаём библиотеку сцен

        visual_scene vs = new visual_scene()  //  создаём сцену, вроде всегда одна
        {
            id = "MyScene",                   //  обзываем её
            
            node = new node[MESHES_IN_CELLGRUP_LIST.Count + MODELS_IN_BIG_FILE_LIST.Count]     //  добавляем узлы для моделей на сцене
        };

//===============================================================================================
//===============================================================================================
//===============================================================================================

        qqq = 0; // шагаем по списку мешей, создаём им ноды, задаём расположение

        foreach (var mesh in MESHES_IN_CELLGRUP_LIST)
        {

            if (mesh.type == "model") m_index = mesh.model_index + MESHES_IN_CELLGRUP_LIST[mesh1count-1].cells_index;
            if (mesh.type == "rmd"  ) m_index = mesh.cells_index;

//----------------------------------

            node n = new node()
            {
                id = "mesh" + m_index, 

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

            if (mesh.type == "rmd") // для rdm*****
            {
              for (int ccc = 0; ccc < cellinst_List.Count; ccc++) 
              {
                  if (m_index == cellinst_List[ccc].number) 
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
                  if (m_index == cellmark_List[ccc].number1)
                  {
                    xx = cellmark_List[ccc].position.x; 
                    yy = cellmark_List[ccc].position.y; 
                    zz = cellmark_List[ccc].position.z; 

                    rx = cellmark_List[ccc].rotation.x; 
                    ry = cellmark_List[ccc].rotation.y; 
                    rz = cellmark_List[ccc].rotation.z; 
                  }
              }
            }

            if (mesh.type == "model") // для model***
            {
              for (int ccc = 0; ccc < cellmark_List.Count; ccc++) 
              {
                  if (m_index == cellmark_List[ccc].number2)
                  {
                    xx = cellmark_List[ccc].position.x; 
                    yy = cellmark_List[ccc].position.y; 
                    zz = cellmark_List[ccc].position.z; 

                    rx = cellmark_List[ccc].rotation.x; 
                    ry = cellmark_List[ccc].rotation.y; 
                    rz = cellmark_List[ccc].rotation.z; 
                  }
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

                new rotate() { sid = "rotationX", Values = new double[4] { 0, 0, 1, rz*57.5 } }, // Z почему такой "угол" ?
                new rotate() { sid = "rotationY", Values = new double[4] { 0, 1, 0, ry*57.5 } }, // Y 
                new rotate() { sid = "rotationZ", Values = new double[4] { 1, 0, 0, rx*57.5 } }, // X 

                new TargetableFloat3() { sid    = "scale",  Values = new double[3] {1, 1, 1} }
            };

//----------------------------------

            vs.node[qqq] = n;

            qqq++;

        } // для каждой модели в big-файле 

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

        File.Delete(name + ".txt");

        foreach (var mesh in MESHES_IN_CELLGRUP_LIST) // для каждой модели
        {
          using (StreamWriter objw = File.AppendText(name + ".txt"))
          {
              objw.WriteLine(mesh.ToString());
          }
        }

        MESHES_IN_CELLGRUP_LIST.Clear();
        MODELS_IN_BIG_FILE_LIST.Clear();

        cellinst_List.Clear();
        cellmark_List.Clear();

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

    } // для каждого big-файла

  } // void Main()

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

  public static string ReadString(BinaryReader reader) 
  {
    string result = "";
    char c;
    for (int i = 0; i < reader.BaseStream.Length; i++) 
    {
      if ((c = (char) reader.ReadByte()) == 0) 
      {
        break;
      }
      result += c.ToString();
    }
    return result;
  }

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

} // class

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

// StringExtension.ReadString(brm);
public static class StringExtension
{
  public static string ReadString(this BinaryReader input)
  {
      List<byte> strBytes = new List<byte>();
      int b;
      while ((b = input.ReadByte()) != 0x00)
          strBytes.Add((byte)b);
      return Encoding.ASCII.GetString(strBytes.ToArray());
  }
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
  public int subMeshContainer;

  public TRI(BinaryReader br, int faceNumber)
  {
    vi0 = br.ReadUInt16();
    vi1 = br.ReadUInt16();
    vi2 = br.ReadUInt16();
    subMeshContainer = faceNumber;
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

//===============================================================================================

class Planinfo
{
  public long offset;
}

//===============================================================================================

class plannode_stuff_1
{
  public Vector3 position; 
  public ushort number1; // 
  public ushort number2; // 
//public int number3; // 
//public int number4; // 

  public plannode_stuff_1(BinaryReader br)
  {
    position = new Vector3(br);
    number1 = br.ReadUInt16(); 
    number2 = br.ReadUInt16(); 
    br.ReadInt32(); 
    br.ReadInt32(); 
  }
}

//===============================================================================================

class plannode_stuff_2
{
  public Vector3 position; 
  public int number3; // 

  public plannode_stuff_2(BinaryReader br)
  {
    position = new Vector3(br);
    number3 = br.ReadInt32(); 
  }
}

//===============================================================================================

class planblok_stuff
{
  public int number1; // 
  public int number2; // 
  public int number3; // hz
  public float number4; // 
  
  public planblok_stuff(BinaryReader br)
  {
    number1 = br.ReadInt32(); 
    number2 = br.ReadInt32(); 
    number3 = br.ReadInt32(); 
    number4 = br.ReadSingle(); 
  }

  public override string ToString()
  {
    return String.Format(number1 + " " + number2 + " " + number3 + " " + number4);
  }
}

//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888
//88888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888

class Model
{
  public long BaseStreamPositionOffset; // смещение модели от начала файла

  public int index_in_big; // порядковый номер появления в файле
  public int model_index; // порядковый номер появления модели в файле
  public int cells_index; // порядковый номер появления модели в блоке cellgrup

  public string type;    // может быть rdm***** или collmodc или mdlattr*

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
  
  public override string ToString() 
  {
    return String.Format(
    "offset= "     + BaseStreamPositionOffset + "\t\t\t" + 
    "index_in_big "+ index_in_big+ "\t\t\t" + 
    "model_index " + model_index + "\t\t\t" + 
    "cells_index " + cells_index + "\t\t\t" + 
    "type "        + type        + "\t\t\t" + 
    "content "     + content.Count
    );
  }
}
