//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
	using System;using System.IO;using System.Linq;using System.Text;using System.Collections;using System.Collections.Generic;
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

sealed class big2obj
{
		static int v_count ;
		static int n_count ;
		static int vt_uv ;

		static string big_path  ; // хранит путь к файлу *.big
		static string writePath ; // хранит путь к файлу *[i].obj
		static int i = 0 ; // индекс в массиве байт всего файла
		static int uv_index_count = 0; // чтобы UVIndex был виден в блоке LayerElementUV
	//static int st1 ;

		// это для того , чтобы расчитать конец блока модели 
		// и записать в файл теги идущие после её геометрии
		static int   i_begin = 0 ; // индекс начала блока модели
		static int   i_offset = 0 ; // индекс смещения блока модели
		static int   i_end = 0 ; // индекс конца блока модели

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

		static void AppendAllTextToObjFile(string fileName, List<string> text)
		{
				using (StreamWriter writer = File.AppendText(fileName))
				{
				    foreach (string line in text)
				    writer.WriteLine(line);
				    text.Clear();
				}
		}

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

		static void Main()	{

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

				// ищет файлы с расширением *.big в подпапках
				string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.model",  SearchOption.AllDirectories) ; 

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

				List<byte> vertexs_count = new List<byte>() ; 	List<string> vertex_str = new List<string>();		// вершины
				List<byte> normals_count = new List<byte>() ; 	List<string> normal_str = new List<string>();		// нормали
				List<byte> primsss_count = new List<byte>() ; 	List<string> prims_str = new List<string>();		// грани

				List<byte> vt_count = new List<byte>() ;				List<string> vt_str = new List<string>();	// uvs
				List<byte> uv_index = new List<byte> ( ) ; 			List<int> uvi_int = new List<int> ( ) ; 	// uvs_index
				
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

				foreach ( var file in files )
				{
						Console.WriteLine(file);
						byte[] array1d = File.ReadAllBytes(file) ;
						int files_name_counter = 1 ; // счётчик моделей и имён файлов для них

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

						// - 7 , потому что ищем до размер файла - 7 байт . ПОНЯТНО ? (нет) (да) // 22 // 40 // 42 // 44 

						for ( i = 0 ; i < array1d.Length - 42 ; i++ ) 
						{
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ НАЧАЛО МОДЕЛИ 69 00 00 00 64 65 66 61 75 6C 74 00
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x69 & array1d[i+1] == 0x00 & array1d[i+2]  == 0x00 & array1d[i+3]  == 0x00 &
										 array1d[i+4] == 0x64 & array1d[i+5] == 0x65 & array1d[i+6]  == 0x66 & array1d[i+7]  == 0x61 &
										 array1d[i+8] == 0x75 & array1d[i+9] == 0x6C & array1d[i+10] == 0x74 & array1d[i+11] == 0x00 )
								{
										// читаем размер блока модели
										byte[] f_size_four_bytes_int = { array1d[i+12+0] , array1d[i+12+1] , array1d[i+12+2] , array1d[i+12+3] } ; 
										i_offset = BitConverter.ToInt32 ( f_size_four_bytes_int , 0 ) ;
										i_begin = i ;
										i_end = i_begin + i_offset ;

										big_path = Path.GetDirectoryName(file);
										writePath = big_path + "/" + Path.GetFileNameWithoutExtension(file) + "___" + files_name_counter + ".obj" ;
										files_name_counter++ ; // нашли "вершины" - увеличили счётчик файденных моделей
										if (File.Exists(writePath)) File.Delete(writePath);
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ ВЕРШИНЫ // v // если нашли строку "position" = 00 00 00 00 70 6F 73 69 74 69 6F 6E 
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x00 & array1d[i+1] == 0x00 & array1d[i+2]  == 0x00 & array1d[i+3]  == 0x00 &
										 array1d[i+4] == 0x70 & array1d[i+5] == 0x6F & array1d[i+6]  == 0x73 & array1d[i+7]  == 0x69 &
										 array1d[i+8] == 0x74 & array1d[i+9] == 0x69 & array1d[i+10] == 0x6F & array1d[i+11] == 0x6E )
								{
										// через 20 байт (от начала "сигнатуры") записано количество вершин , считываем и запоминаем его
										byte[] v_count_four_bytes_int = { array1d[i+20+0] , array1d[i+20+1] , array1d[i+20+2] , array1d[i+20+3] } ; 
										v_count = BitConverter.ToInt32(v_count_four_bytes_int , 0) ;

										for ( int ii = 0 ; ii < v_count*3*4 ; ii++ ) // количество вершин * 3 координаты * 4 байта
												vertexs_count.Add ( array1d[i+24+ii] ) ; // где то после 22 стоит не float значение !!!!!!!!!!!!!!!!!!!!!

								//	vertex_str.Add("# " + v_count + " vertices ");

										for ( int ii = 0 ; ii < vertexs_count.Count ; ii+=12 ) // float занимает 4 байта и их по три координаты
										{		
												byte[] fourBytes1 = { vertexs_count[ii+0] , vertexs_count[ii+1] , vertexs_count[ii+2 ] , vertexs_count[ii+3 ] } ; 
												byte[] fourBytes2 = { vertexs_count[ii+4] , vertexs_count[ii+5] , vertexs_count[ii+6 ] , vertexs_count[ii+7 ] } ; 
												byte[] fourBytes3 = { vertexs_count[ii+8] , vertexs_count[ii+9] , vertexs_count[ii+10] , vertexs_count[ii+11] } ;

												float x = BitConverter.ToSingle(fourBytes1 , 0) ;
												float y = BitConverter.ToSingle(fourBytes2 , 0) ;
												float z = BitConverter.ToSingle(fourBytes3 , 0) ;

												vertex_str.Add ( "v " + y + " " + z + " " + x ) ; // "v " + y + " " + z + " " + x
										}
												vertexs_count.Clear() ;
									}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ Vn (нормали)
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x6E & array1d[i+1] == 0x6F & array1d[i+2]  == 0x72 & array1d[i+3]  == 0x6D &
										 array1d[i+4] == 0x61 & array1d[i+5] == 0x6C & array1d[i+6]  == 0x73 & array1d[i+7]  == 0x00 )
								{
										byte[] n_count_four_bytes_int = { array1d[i+16+0] , array1d[i+16+1] , array1d[i+16+2] , array1d[i+16+3] } ; 
										n_count = BitConverter.ToInt32 ( n_count_four_bytes_int , 0 ) ;

										for ( int ii = 0 ; ii < n_count*3*4 ; ii++ )
										normals_count.Add ( array1d[i+20+ii] ) ;

								//	normal_str.Add("\n# " + n_count + " normals ");

										for ( int ii = 0 ; ii < normals_count.Count ; ii+=12 )
										{
												byte[] fourBytes1 = { normals_count[ii+0] , normals_count[ii+1] , normals_count[ii+2 ] , normals_count[ii+3 ] } ; 
												byte[] fourBytes2 = { normals_count[ii+4] , normals_count[ii+5] , normals_count[ii+6 ] , normals_count[ii+7 ] } ; 
												byte[] fourBytes3 = { normals_count[ii+8] , normals_count[ii+9] , normals_count[ii+10] , normals_count[ii+11] } ;

												float vn1 = BitConverter.ToSingle(fourBytes1 , 0) ;
												float vn2 = BitConverter.ToSingle(fourBytes2 , 0) ;
												float vn3 = BitConverter.ToSingle(fourBytes3 , 0) ;
												
												normal_str.Add("vn " + vn1 + " " + vn2 + " " + vn3);
										}
												normals_count.Clear() ;	
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ТЕКСТУРНЫЕ  КООРДИНАТЫ // vt // uvs. // 75 76 73 00 00 00 00 00
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x75 & array1d[i+1] == 0x76 & array1d[i+2]  == 0x73 & array1d[i+3]  == 0x00 ) 
								{
										byte[] vt_count_four_bytes_int = { array1d[i+20+0] , array1d[i+20+1] , array1d[i+20+2] , array1d[i+20+3] } ; 
										vt_uv = BitConverter.ToInt32(vt_count_four_bytes_int , 0) ;

										for (int ii = 0 ; ii < vt_uv*2*4 ; ii++ ) // количество вершин * 3 координаты * 4 байта
													vt_count.Add ( array1d[i+24+ii] ) ; // где то после 22 стоит не float значение !!!!!!!!!!!!!!!!!!!!!!!!

								//	vt_str.Add("\n# " + vt_uv + " vt" + " = " + "UVSET " + array1d[i+16]);
								//	vt_str.Add("# offset = " + i );

										for ( int ii = 0 ; ii < vt_count.Count ; ii+=8 )
										{
												byte[] fourBytes1 = { vt_count[ii+0] , vt_count[ii+1] , vt_count[ii+2 ] , vt_count[ii+3 ] } ; 
												byte[] fourBytes2 = { vt_count[ii+4] , vt_count[ii+5] , vt_count[ii+6 ] , vt_count[ii+7 ] } ; 
												float vu = BitConverter.ToSingle(fourBytes1 , 0) ;
												float vv = ( -1 )*BitConverter.ToSingle(fourBytes2 , 0) ;
												vt_str.Add( "vt " + vu + " " + vv ) ; // записываем строку в файл *.obj
										}
												vt_count.Clear() ;
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ ГРАНИ FACES ( prims ) // если нашли строку "prims..." = 70 72 69 6D 73 ( 00 00 00 )
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x70 & array1d[i+1] == 0x72 & array1d[i+2] == 0x69 & array1d[i+3] == 0x6D & array1d[i+4] == 0x73 )
								{
										// читаем размер блока // не знаю зачем	//	пригодилось	//	для расчёта смещения блока textures , содержащего UVIndex
										byte[] f_size_four_bytes_int = { array1d[i+8+0] , array1d[i+8+1] , array1d[i+8+2] , array1d[i+8+3] } ; 
										int fb_size = BitConverter.ToInt32 ( f_size_four_bytes_int , 0 ) ;

										//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
										// uvs index , индексы текстурных координат , "блок" textures , 00 00 FF FF 74 65 78 74 75 72 65 73
										//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

										int o = i + fb_size + 16 ;	//	начало блока prism/faces + его размер + размер "заголовока" блока textures
										byte[] uv_index_int = { array1d[ o + 0 ] , array1d[ o + 1 ] , array1d[ o + 2 ] , array1d[ o + 3 ] } ; 
										uv_index_count = BitConverter.ToInt32 ( uv_index_int , 0 ) ; // получили количество индексов

										for ( int ii = 0 ; ii < uv_index_count*2 ; ii++ )	//	одно число = два байта	//	4 индекса это 8 байт
													uv_index.Add ( array1d[ o + ii + 4] ) ; //	добавляем индексы в список

								//	uvi_int.Add ("#"); // это необходимо , чтобы "индекс не был нолём"

										for ( int iii = 0 ; iii < uv_index.Count ; iii += 2 )	//	добавляем индексы в виде строк в файл
										{
												byte[] twoBytes1 = { uv_index[ iii + 0 ] , uv_index[ iii + 1 ] } ; // 00 00  
												int st1 = ( Int16 )BitConverter.ToInt16 ( twoBytes1 , 0 ) + 1 ; // +1 потому что в obj индексация идёт с 1, а не с 0
												uvi_int.Add ( st1 ) ; 
										}

								//--------------------------------------------------------------------------------------------------------------------------

										int id = i + 24 ; // это было очень сложно
										int offset = 0  ; // 4 8 12 16 18 40
										int fs_count = array1d[i+16] ; 	// читаем количество "саб"-мешей

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
										//----------------------------------------------------------------------------------------------------------------------
												for ( int ii = 0 ; ii < f_size_count*2 ; ii++ ) 
														primsss_count.Add ( array1d[id + 8 + ii + offset] ) ; 
										//----------------------------------------------------------------------------------------------------------------------
												// размер "промежутков" с какой то инфой между блока граней 
												offset = offset + 8 + f_size_count*2 + 44 ; 
												if ( f_count % 2 != 0 ) offset = offset + 2 ;
										//----------------------------------------------------------------------------------------------------------------------

												prims_str.Add( "\ng faces" + fi ) ; 
												prims_str.Add( "# " + f_count + " faces" ) ; 

												for ( int iii = 0 ; iii < primsss_count.Count ; iii+=6 )
												{
														byte[] twoBytes1 = { primsss_count[iii+0] , primsss_count[iii+1] } ; // 00 00 // 03 00 
														byte[] twoBytes2 = { primsss_count[iii+2] , primsss_count[iii+3] } ; // 01 00 // 02 00
														byte[] twoBytes3 = { primsss_count[iii+4] , primsss_count[iii+5] } ; // 02 00 // 01 00

														UInt16 f1 = (UInt16)BitConverter.ToUInt16(twoBytes1 , 0) ; // 0
														UInt16 f2 = (UInt16)BitConverter.ToUInt16(twoBytes2 , 0) ; // 1
														UInt16 f3 = (UInt16)BitConverter.ToUInt16(twoBytes3 , 0) ; // 2

														prims_str.Add( "f " + (f1+1) + "/" + uvi_int[f1] + "/" + (f1+1) + " "
														                    + (f2+1) + "/" + uvi_int[f2] + "/" + (f2+1) + " "
														                    + (f3+1) + "/" + uvi_int[f3] + "/" + (f3+1) ) ; 
														
												}
														primsss_count.Clear();
										}
												uvi_int.Clear();
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( i == i_end - 42 ) // записываем один раз в конец файла
								{
										AppendAllTextToObjFile(writePath, new List<string>() { "# " + v_count + " vertices " } );
										AppendAllTextToObjFile(writePath, vertex_str); 
										vertex_str.Clear();

										AppendAllTextToObjFile(writePath, new List<string>() { "\n# " + n_count + " normals " } );
										AppendAllTextToObjFile(writePath, normal_str); 
										normal_str.Clear();

										AppendAllTextToObjFile(writePath, new List<string>() { "\n# " + vt_uv + " vt" + " = " + "UVSET " + array1d[i+16] } );
										AppendAllTextToObjFile(writePath, vt_str); 
										vt_str.Clear();

										AppendAllTextToObjFile(writePath, prims_str);
										primsss_count.Clear();
										prims_str.Clear();

										uvi_int.Clear();
										uv_index.Clear();
								}

							//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

						} // for // прошли по всему содержимому массива байт прочитанных из файла
		 		} // foreach // прошли по каждому файлу
		} // Main
} // class

// жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
