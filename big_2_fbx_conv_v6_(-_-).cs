//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
	using System;using System.IO;using System.Linq;using System.Text;using System.Collections;using System.Collections.Generic;
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

sealed class big2fbx
{
		static string big_path  ; // хранит путь к файлу *.big
		static string writePath ; // хранит путь к файлу *[i].fbx

		static int v_count = 0 ; // хранит количество вершин , которому будет равно кол-во нормалей 

		static int fs_count = 0 ; // количество сабмешей и это же количество моделей в сцене 
		static int f_count_sum = 0 ; // хранит общее количество граней всех сабмешей

		static List<List<string>> subMeshFacesStr = new List<List<string>>() ;
		static List<List<int>>    subMeshFacesInt = new List<List<int>>() ;

		static int uv_index_count = 0; // чтобы UVIndex был виден в блоке LayerElementUV
		static List<string> uv_str_dub;	// дублирующее значение UVIndex для второго набора

		static int   i = 0 ; // индекс в массиве байт всего файла
		// используется в "вычислении" граней

		// это для того , чтобы расчитать конец блока модели и записать в файл теги идущие после её геометрии
		static int   i_begin = 0 ; // индекс начала блока модели
		static int   i_offset = 0 ; // индекс смещения блока модели
		static int   i_end = 0 ; // индекс конца блока модели

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

		// чтобы не обращаться в файл (и дисковой системе?) 
		// по многу раз , для записи каждой строки ,
		// мы будем записывать в неё список строк всего лишь один раз

		static void AppendAllTextToObjFile(string fileName, List<string> text)
		{
				using (StreamWriter writer = File.AppendText(fileName))
				{
				    foreach (string line in text)
				    writer.WriteLine(line);
				    text.Clear();
				}
		}

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
		static void Main()	{
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
				System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
				string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.big", SearchOption.AllDirectories)	;	// ищет файлы с расширением *.big в подпапках
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

				List<byte> vertexs_count = new List<byte>() ; 	List<string> vertex_str = new List<string>();		// вершины
				List<byte> normals_count = new List<byte>() ; 	List<string> normal_str = new List<string>();		// нормали
				List<byte> primsss_count = new List<byte>() ; 	List<string> prims_str = new List<string>();		// грани
				List<byte> vt_count = new List<byte>() ;				List<string> vt_str = new List<string>();				// uvs
				List<byte> uv_index = new List<byte>() ;				List<string> uv_str = new List<string>();				// uvs_index

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

				foreach ( var file in files )
				{
						Console.WriteLine(file);
						byte[] array1d = File.ReadAllBytes(file) ;
						int files_name_counter = 1 ; // счётчик моделей и имён файлов для них

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

						for ( i = 0 ; i < array1d.Length - 42 ; i++ ) // - 7 , потому что ищем до размер файла - 7 байт . ПОНЯТНО ? (нет) (да) // 22 // 40 // 42 // 44 
						{
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ НАЧАЛО МОДЕЛИ 69 00 00 00 64 65 66 61 75 6C 74 00
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x69 & array1d[i+1] == 0x00 & array1d[i+2]  == 0x00 & array1d[i+3]  == 0x00 &
										 array1d[i+4] == 0x64 & array1d[i+5] == 0x65 & array1d[i+6]  == 0x66 & array1d[i+7]  == 0x61 &
										 array1d[i+8] == 0x75 & array1d[i+9] == 0x6C & array1d[i+10] == 0x74 & array1d[i+11] == 0x00 )
								{
										// читаем размер "блока" модели
										byte[] f_size_four_bytes_int = { array1d[i+12+0] , array1d[i+12+1] , array1d[i+12+2] , array1d[i+12+3] } ; 
										i_offset = BitConverter.ToInt32 ( f_size_four_bytes_int , 0 ) ;
										i_begin = i ;
										i_end = i_begin + i_offset ;

										big_path = Path.GetDirectoryName(file);
										writePath = big_path + "/" + Path.GetFileNameWithoutExtension(file) + "___" + files_name_counter + ".fbx" ; //		.fbx		.obj
										files_name_counter++ ; // нашли "вершины" - увеличили счётчик файденных моделей // это может стоять после всех "блоков" модели?
										if (File.Exists(writePath)) File.Delete(writePath);
								} 

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ ВЕРШИНЫ // если нашли строку "position" = 00 00 00 00 70 6F 73 69 74 69 6F 6E 
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x00 & array1d[i+1] == 0x00 & array1d[i+2]  == 0x00 & array1d[i+3]  == 0x00 &
										 array1d[i+4] == 0x70 & array1d[i+5] == 0x6F & array1d[i+6]  == 0x73 & array1d[i+7]  == 0x69 &
										 array1d[i+8] == 0x74 & array1d[i+9] == 0x69 & array1d[i+10] == 0x6F & array1d[i+11] == 0x6E )
								{
										// через 20 байт (от начала "сигнатуры") записано количество вершин , считываем и запоминаем его
										byte[] v_count_four_bytes_int = { array1d[i+20+0] , array1d[i+20+1] , array1d[i+20+2] , array1d[i+20+3] } ; 
										v_count = BitConverter.ToInt32(v_count_four_bytes_int , 0) ; // количество вершин

										// эта цифра указывает сколько (байт*4) надо считать // например она равна 6 , значит нужно считать 6 пар типа [00 00 00 00]
										// Console.WriteLine("Количество вершин = " + v_count + "\t" + "Количество байт = "   + v_count*3*4 + "\n" ) ;
										// ещё через 4 начинается список координат вершин [i+22]
										// считываем все байты содержащие "значения" вершин в массив

										for ( int ii = 0 ; ii < v_count*3*4 ; ii++ ) // количество вершин * 3 координаты * 4 байта
												vertexs_count.Add ( array1d[i+24+ii] ) ; // где то после 22 стоит не float значение !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

								//--------------------------------------------------------------------------------------------------------------

									//vertex_str.Add("\nVertices: *" + v_count + " {" + "\na: ");

										for ( int ii = 0 ; ii < vertexs_count.Count ; ii+=12 ) // float занимает 4 байта и их по три координаты
										{		
												byte[] fourBytes1 = { vertexs_count[ii+0] , vertexs_count[ii+1] , vertexs_count[ii+2 ] , vertexs_count[ii+3 ] } ; 
												byte[] fourBytes2 = { vertexs_count[ii+4] , vertexs_count[ii+5] , vertexs_count[ii+6 ] , vertexs_count[ii+7 ] } ; 
												byte[] fourBytes3 = { vertexs_count[ii+8] , vertexs_count[ii+9] , vertexs_count[ii+10] , vertexs_count[ii+11] } ;

												float v1 = BitConverter.ToSingle(fourBytes1 , 0) ;
												float v2 = BitConverter.ToSingle(fourBytes2 , 0) ;
												float v3 = BitConverter.ToSingle(fourBytes3 , 0) ;

												string vertexs = String.Format ( "{0}, {1}, {2}" , v1 , v2 , v3 ) ;
												if (ii != vertexs_count.Count-12) vertexs = vertexs + ",";
												vertex_str.Add( vertexs ) ;
											//vertex_str.Add ( v1 + "," + v2 + "," + v3 + "," ) ;
										}

									//vertex_str.Add( "}" ) ; //	удаляем последнюю запятую	
									//vertexs_count.Clear() ;	//	очищаем "бинарный" список значений координат вершины
									//AppendAllTextToObjFile ( writePath , vertex_str ) ; 
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ Vn (нормали)
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x6E & array1d[i+1] == 0x6F & array1d[i+2]  == 0x72 & array1d[i+3]  == 0x6D &
										 array1d[i+4] == 0x61 & array1d[i+5] == 0x6C & array1d[i+6]  == 0x73 & array1d[i+7]  == 0x00 )
								{
										byte[] n_count_four_bytes_int = { array1d[i+16+0] , array1d[i+16+1] , array1d[i+16+2] , array1d[i+16+3] } ; 
										int n_count = BitConverter.ToInt32 ( n_count_four_bytes_int , 0 ) ;

										for ( int ii = 0 ; ii < n_count*3*4 ; ii++ )
										normals_count.Add ( array1d[i+20+ii] ) ;

									//normal_str.Add("\nNormals: *" + n_count + " {" + "\na: ");

										for ( int ii = 0 ; ii < normals_count.Count ; ii+=12 )
										{
												byte[] fourBytes1 = { normals_count[ii+0] , normals_count[ii+1] , normals_count[ii+2 ] , normals_count[ii+3 ] } ; 
												byte[] fourBytes2 = { normals_count[ii+4] , normals_count[ii+5] , normals_count[ii+6 ] , normals_count[ii+7 ] } ; 
												byte[] fourBytes3 = { normals_count[ii+8] , normals_count[ii+9] , normals_count[ii+10] , normals_count[ii+11] } ;

												float vn1 = BitConverter.ToSingle(fourBytes1 , 0) ;
												float vn2 = BitConverter.ToSingle(fourBytes2 , 0) ;
												float vn3 = BitConverter.ToSingle(fourBytes3 , 0) ;
												
												string normals = String.Format ( "{0}, {1}, {2}" , vn1 , vn2 , vn3 ) ;
												if (ii != normals_count.Count-12) normals = normals + ",";
												normal_str.Add( normals ) ;
											//normal_str.Add ( vn1 + "," + vn2 + "," + vn3 + "," ) ;
												
										}

									//int qq = 0;	foreach (var q in normal_str) {	Console.WriteLine( qq + " " + q );	qq++;	}

									//normal_str.Add( "}" ) ;
										normals_count.Clear() ;	
									//AppendAllTextToObjFile(writePath, normal_str); // переносится в слой после граней
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ ГРАНИ FACES ( prims ) // если нашли строку "prims..." = 70 72 69 6D 73 ( 00 00 00 )
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x70 & array1d[i+1] == 0x72 & array1d[i+2] == 0x69 & array1d[i+3] == 0x6D & array1d[i+4] == 0x73 )
								{
										int id = i + 24 ; // это было очень сложно
										int offset = 0  ; // 4 8 12 16 18 40
										fs_count = array1d[i+16] ; 	// читаем количество "саб"-мешей

										for ( int fi = 0 ; fi < fs_count ; fi++ )	// повторяем необходимое кол-во раз равное кол-ву саб-мешей
										{
												byte[] f_count_four_bytes_int = { // читаем количество строчек f в файле obj // c 24 по 27
												array1d[id+0+offset] ,	array1d[id+1+offset] ,	array1d[id+2+offset] ,	array1d[id+3+offset] } ;	
												int f_count = BitConverter.ToInt32 ( f_count_four_bytes_int , 0 ) ; // количество строк

												byte[] f_count_four_bytes_int_size = { // читаем количество байт , которых нужно умножить на два // c 28 по 31
												array1d[id+4+0+offset] ,	array1d[id+4+1+offset] ,	array1d[id+4+2+offset] ,	array1d[id+4+3+offset] } ;	
												int f_size_count = BitConverter.ToInt32 ( f_count_four_bytes_int_size , 0 ) ; // 

												//эта цифра указывает сколько байт*2 надо считать // например она равна 6 , значит нужно считать 6 пар типа [00 00]
												//Console.WriteLine("Количество граней = " + f_count/3 + "\t" + "Количество байт = "   + f_count*2 + "\n" ) ; 
										//--------------------------------------------------------------------------------------------------------------
												for ( int ii = 0 ; ii < f_size_count*2 ; ii++ ) 
														primsss_count.Add ( array1d[id + 8 + ii + offset] ) ; 
										//--------------------------------------------------------------------------------------------------------------
												// размер "промежутков" с какой то инфой между блока граней 
												offset = offset + 8 + f_size_count*2 + 44 ; 
												if ( f_count % 2 != 0 ) offset = offset + 2 ;
										//--------------------------------------------------------------------------------------------------------------

												for ( int iii = 0 ; iii < primsss_count.Count ; iii+=6 )
												{
														byte[] twoBytes1 = { primsss_count[iii+0] , primsss_count[iii+1] } ; // 00 00 
														byte[] twoBytes2 = { primsss_count[iii+2] , primsss_count[iii+3] } ; // 01 00
														byte[] twoBytes3 = { primsss_count[iii+4] , primsss_count[iii+5] } ; // 02 00

														Int16 f1 = (Int16)BitConverter.ToInt16(twoBytes1 , 0) ; // 0
														Int16 f2 = (Int16)BitConverter.ToInt16(twoBytes2 , 0) ; // 1
														Int16 f3 = (Int16)BitConverter.ToInt16(twoBytes3 , 0) ; // 2
														Int16 f3n= (Int16)BitConverter.ToInt16(twoBytes3 , 0) + 1 / (-1);

														string faces = String.Format ( "{0}, {1}, {2}" , f1 , f2 , ((f3 + 1) / (-1)) ) ;
														if (iii != primsss_count.Count-6) faces = faces + ",";
														prims_str.Add( faces ) ;
												}
												primsss_count.Clear() ;
										}
/*
										foreach (List<string> subList in subMeshFaces)
										{
										    foreach (string item in subList)
										    {
										        Console.WriteLine(item);
										    }
										}
*/
										primsss_count.Clear() ;

								} // if // ИЩЕМ ГРАНИ FACES ( prims )

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
// COLOR
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

/*
		LayerElementColor: 0 {
			Version: 101
			Name: "Col"
			MappingInformationType: "ByVertice"
			ReferenceInformationType: "IndexToDirect"

			Colors: *12 {
				a: 0.996078431606293,0.996078431606293,0.996078431606293,1,1,1,1,1,0.992156863212585,0.992156863212585,0.992156863212585,1
			} 

			ColorIndex: *4616 {
				a: ...
			} 
		}
*/
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ТЕКСТУРНЫЕ  КООРДИНАТЫ // uvs. // 75 76 73 00 00 00 00 00
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x75 & array1d[i+1] == 0x76 & array1d[i+2]  == 0x73 & array1d[i+3]  == 0x00 ) 
								{
										byte[] vt_count_four_bytes_int = { array1d[i+20+0] , array1d[i+20+1] , array1d[i+20+2] , array1d[i+20+3] } ; 
										int vt_uv = BitConverter.ToInt32(vt_count_four_bytes_int , 0) ;

										for (int ii = 0 ; ii < vt_uv*2*4 ; ii++ ) // количество вершин * 3 координаты * 4 байта
													vt_count.Add ( array1d[i+24+ii] ) ; // где то после 22 стоит не float значение !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

										for ( int ii = 0 ; ii < vt_count.Count ; ii+=8 )
										{
												byte[] fourBytes1 = { vt_count[ii+0] , vt_count[ii+1] , vt_count[ii+2 ] , vt_count[ii+3 ] } ; 
												byte[] fourBytes2 = { vt_count[ii+4] , vt_count[ii+5] , vt_count[ii+6 ] , vt_count[ii+7 ] } ; 

												float vu = BitConverter.ToSingle(fourBytes1 , 0) ;
												float vv = (-1)*BitConverter.ToSingle(fourBytes2 , 0) ;

												string vts = String.Format ( "{0}, {1}" , vu , vv ) ;
												if (ii != vt_count.Count-8) vts = vts + ",";
												vt_str.Add( vts ) ;
										}

										//int qq = 0;	foreach (var q in vt_str) {	Console.WriteLine( qq + " " + q );	qq++;	}
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// uvs index , индексы текстурных координат , "блок" textures , 00 00 FF FF 74 65 78 74 75 72 65 73
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x00 & array1d[i+1] == 0x00 & array1d[i+2]  == 0xFF & array1d[i+3]  == 0xFF &	//	00 00 FF FF
										 array1d[i+4] == 0x74 & array1d[i+5] == 0x65 & array1d[i+6]  == 0x78 & array1d[i+7]  == 0x74 &	//	74 65 78 74
										 array1d[i+8] == 0x75 & array1d[i+9] == 0x72 & array1d[i+10] == 0x65 & array1d[i+11] == 0x73 )	//	75 72 65 73
								{

										byte[] uv_index_int = { array1d[i+20+0] , array1d[i+20+1] , array1d[i+20+2] , array1d[i+20+3] } ; 
										uv_index_count = BitConverter.ToInt32(uv_index_int , 0) ; // получили количество индексов

										for (int ii = 0 ; ii < uv_index_count*2 ; ii++ )	//	одно число занимает два байта	//	прочитали если индексов 4 то 8 байт
													uv_index.Add ( array1d[i+24+ii] ) ;	//	добавляем индексы в список

										for ( int iii = 0 ; iii < uv_index.Count ; iii+=2 )	//	добавляем индексы в виде строк в файл
										{
												byte[] twoBytes1 = { uv_index[iii+0] , uv_index[iii+1] } ; // 00 00  
												Int16 st1 = (Int16)BitConverter.ToInt16(twoBytes1 , 0) ; // 0

												string uvs = String.Format ( "{0}" , st1 ) ;
												if (iii != uv_index.Count-2) uvs = uvs + ",";
												uv_str.Add( uvs ) ;	// uv_str.Add( st1 + "," ) ; 
										}
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								//uv_str_dub.Clear() ;
						
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( i == i_end - 42 ) // если нашли "конец" модели , то записываем один раз в конец файла
								{

/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

AppendAllTextToObjFile(	//	метод пишет
writePath , 						//	в файл
new List<string>() { 		//	список строк

//@"; " + file // <source_data>file:///C:/file%20name.big</source_data> 
//, 

@"
; ==================================================== 
; FBX 7.3.0 project file
; Copyright (C) 1997-2010 Autodesk Inc. and/or its licensors.
; All rights reserved.
; ==================================================== 

FBXHeaderExtension:  {
	FBXHeaderVersion: 1003
	FBXVersion: 7300
}

; ==================================================== 

GlobalSettings:  {
	Version: 1000
}

; ==================================================== 
; Documents Description
; ----------------------------------------------------

Documents:  {
	Count: 1
	Document: 33350048, ""Scene"", ""Scene"" {
		Properties70:  {
			P: ""SourceObject"", ""object"", """", """"
			P: ""ActiveAnimStackName"", ""KString"", """", """", """"
		}
		RootNode: 0
	}
}

; ==================================================== 
; Document References
; ----------------------------------------------------

References: {
}

; ==================================================== 
; Object definitions
; ----------------------------------------------------

Definitions: 
{
	Version: 100
	Count: " + ((fs_count*5)+1) + @"

	ObjectType: ""GlobalSettings"" {
		Count: 1
	}

	ObjectType: ""Model"" {
		Count: " + fs_count + 
		@"
		PropertyTemplate: ""FbxNode"" {
		}
	}

	ObjectType: ""Material"" {
		Count: " + fs_count + 
		@"
		PropertyTemplate: ""FbxSurfacePhong"" {
		}
	}

	ObjectType: ""Texture"" {
		Count: " + fs_count + 
		@"
		PropertyTemplate: ""FbxFileTexture"" {
		}
	}

	ObjectType: ""Video"" {
		Count: " + fs_count + 
		@"
		PropertyTemplate: ""FbxVideo"" {
		}
	}

	ObjectType: ""Geometry"" {
		Count: " + fs_count + 
		@"
		PropertyTemplate: ""FbxMesh"" {
		}
	}

}

; ==================================================== 
; Object properties
; ----------------------------------------------------

Objects: {"
}
)
;

/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

for (int smi = 0 ; smi < fs_count ; smi++ )	// для каждой сабмеши добавляем в файл отдельные Geometry , LayerElementNormal , LayerElementUV , LayerElementMaterial , Layer: 0 , Layer: 1
{

AppendAllTextToObjFile(	//	метод пишет
writePath , 						//	в файл
new List<string>() { 		//	список строк

@"
	Geometry: 33740376, ""Geometry::Cube.037"", ""Mesh"" {

	Vertices: *108 { a: }

	PolygonVertexIndex: *168 { a: }

	;Edges: *количество_рёбер {	a: ... }"

,

@"
	GeometryVersion: 124

	LayerElementNormal: 0 
	{
			Version: 101
			Name: """"
			MappingInformationType: ""ByVertice""
			ReferenceInformationType: ""Direct""

			Normals: *108 { a: }
			NormalsIndex: *108 { a: }
	}

	LayerElementUV: 0 
	{ 
		Version: 101
			Name: ""diffuse_uv_layer""
			MappingInformationType: ""ByVertice""
			ReferenceInformationType: ""IndexToDirect""
			UV: *96 { a: }
			UVIndex: *108 { a: }
	}

	LayerElementUV: 1 
	{ 
		Version: 101
			Name: ""diffuse_uv_layer""
			MappingInformationType: ""ByVertice""
			ReferenceInformationType: ""IndexToDirect""
			UV: *96 { a: }
			UVIndex: *108 { a: }
	}

	LayerElementMaterial: 0 
	{
		Version: 101
		Name: """"
		MappingInformationType: ""ByVertice""
														;	AllSame
		ReferenceInformationType: ""IndexToDirect""
		Materials: *1 {
			a: 0
		} 
	}"

,

@"
	Layer: 0 
	{
		Version: 100

		LayerElement:  {
			Type: ""LayerElementNormal""
			TypedIndex: 0
		}
		LayerElement:  {
			Type: ""LayerElementMaterial""
			TypedIndex: 0
		}
		LayerElement:  {
			Type: ""LayerElementColor""
			TypedIndex: 0
		}
		LayerElement:  {
			Type: ""LayerElementUV""
			TypedIndex: 0
		}
	}

	Layer: 1 {
		Version: 100

		LayerElement:  {
			Type: ""LayerElementUV""
			TypedIndex: 1
		}
	}
"
,

@"; ----------------------------------------------------

	Model: 33737576, ""Model::Castle"", ""Mesh"" {
		Version: 232
		Properties70:  {
			P: ""InheritType"", ""enum"", """", """",1
			P: ""DefaultAttributeIndex"", ""int"", ""Integer"", """",0
			P: ""Lcl Translation"", ""Lcl Translation"", """", ""A"",740.226989746094,0,-464.880859375
			P: ""Lcl Rotation"", ""Lcl Rotation"", """", ""A"",-89.999995674289,-89.999995674289,0
			P: ""Lcl Scaling"", ""Lcl Scaling"", """", ""A"",100,100,100
		}
		Culling: ""CullingOff""
	}

	Material: 33739120, ""Material::castle_map"", """" {
		Version: 102
		ShadingModel: ""phong""
		MultiLayer: 0
	}

	Video: 33739984, ""Video::final_castle_texture"", ""Clip"" {
		Type: ""Clip""
		Properties70:  {
			P: ""Path"", ""KString"", ""XRefUrl"", """", ""C:\Users\robto\Downloads\final_castle_texture.png""
		}
		UseMipMap: 0
		Filename: ""C:\Users\robto\Downloads\final_castle_texture.png""
		RelativeFilename: ""..\Users\robto\Downloads\final_castle_texture.png""
	}

	Texture: 33739496, ""Texture::texture_castle"", """" {
		Type: ""TextureVideoClip""
		Version: 202
		TextureName: ""Texture::texture_castle""
		Properties70:  {
			P: ""UVSet"", ""KString"", """", """", """"
			P: ""UseMaterial"", ""bool"", """", """",1
			P: ""UseMipMap"", ""bool"", """", """",1
			P: ""AlphaSource"", ""enum"", """", """",2
		}

		Media: ""Video::final_castle_texture""
		FileName: ""C:\Users\robto\Downloads\final_castle_texture.png""
		RelativeFilename: ""..\Users\robto\Downloads\final_castle_texture.png""
		ModelUVTranslation: 0,0
		ModelUVScaling: 1,1
		Texture_Alpha_Source: ""None""
		Cropping: 0,0,0,0
	}

}

; закрывает Geometry

; ===================================================="

}	//	список 
)	//	метод
;	//	;

} // for для добавления каждой сабмеши

/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

AppendAllTextToObjFile(	//	метод пишет
writePath , 						//	в файл
new List<string>() { 		//	список строк

@"
} 
; закрывает Objects

; ====================================================
; Object connections
;-----------------------------------------------------

Connections:  
{

"
}
)
;



AppendAllTextToObjFile(	//	метод пишет
writePath , 						//	в файл
new List<string>() { 		//	список строк
@"
	;Model::Castle, Model::RootNode
	C: ""OO"",33737576,0
	
	;Geometry::Cube.037, Model::Castle
	C: ""OO"",33740376,33737576
	
	;Material::castle_map, Model::Castle
	C: ""OO"",33739120,33737576
	
	;Texture::texture_castle, Material::castle_map
	C: ""OP"",33739496,33739120, ""DiffuseColor""
	
	;Video::final_castle_texture, Texture::texture_castle
	C: ""OO"",33739984,33739496
"
}
);

AppendAllTextToObjFile(	//	метод пишет
writePath , 						//	в файл
new List<string>() { 		//	список строк
@"
}
"
}
);

AppendAllTextToObjFile(	//	метод пишет
writePath , 						//	в файл
new List<string>() { 		//	список строк

@"
; ==================================================== 
;Takes section
;-----------------------------------------------------

Takes:  {
	Current: """"
}

"
}
);
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								} // если нашли конец модели 
								
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

						} // for // прошли по всему содержимому массива байт прочитанных из файла

		 		} // foreach // прошли по каждому файлу

		} // Main

} // class

// жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
