// issue : некоторые модели не работают , потому что почему то "дублируется" блок PolygonVertexIndex без закрывающихся скобок }

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
	using System;using System.IO;using System.Linq;using System.Text;using System.Collections;using System.Collections.Generic;
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

sealed class big2fbx
{
		static string big_path  ; // хранит путь к файлу *.big
		static string writePath ; // хранит путь к файлу *[i].fbx

		static int v_count; // хранит количество вершин , которому будет равно кол-во нормалей 

		static int uv_index_count; // чтобы UVIndex был виден в блоке LayerElementUV
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
										// читаем размер блока модели
										byte[] f_size_four_bytes_int = { array1d[i+12+0] , array1d[i+12+1] , array1d[i+12+2] , array1d[i+12+3] } ; 
										i_offset = BitConverter.ToInt32 ( f_size_four_bytes_int , 0 ) ;
										i_begin = i ;
										i_end = i_begin + i_offset ;
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

										big_path = Path.GetDirectoryName(file);
										writePath = big_path + "/" + Path.GetFileNameWithoutExtension(file) + "___" + files_name_counter + ".fbx" ;
										files_name_counter++ ; // нашли "вершины" - увеличили счётчик файденных моделей // это может стоять после всех "блоков" модели?
										if (File.Exists(writePath)) File.Delete(writePath);

/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

AppendAllTextToObjFile(	//	метод пишет
writePath , 						//	в файл
new List<string>() { 		//	список строк

@"; " + file // <source_data>file:///C:/file%20name.big</source_data> 

, 

@"
; ----------------------------------------------------  

FBXHeaderExtension:  
{
	FBXHeaderVersion: 1003
	FBXVersion: 7300
}

; ==================================================== 

GlobalSettings:  
{
	Version: 1000
}

; ----------------------------------------------------

Documents:  
{
	Count: 1
	Document: 1, """", ""Scene"" 
	{
		RootNode: 0
	}
}

; ==================================================== 

; ----------------------------------------------------

References:  {
}

; ==================================================== 
; Object definitions
; ----------------------------------------------------

Definitions:  
{
	Version: 100

	; Count: 3 ; 

	ObjectType: ""GlobalSettings"" {
		Count: 1
	}

	ObjectType: ""Model"" {
		Count: 1 ; 1 nodes в сцене
	}

	ObjectType: ""Geometry"" {
		Count: 1 ; количество источников геометрии
	}

	;ObjectType: ""Material"" {
	;Count: 1 ; количество материалов
	;}
}

; ==================================================== 
; Object properties
; ----------------------------------------------------

Objects:  
{
	Geometry: 2, ""Geometry::"", ""Mesh""		;		номер блока , тип блока , имя блока
	{
"
}	//	список 
)	//	метод
;	//	;

/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

										vertex_str.Add("\nVertices: *" + v_count + " {" + "\na: ");

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

										vertex_str.Add( "}" ) ; //	удаляем последнюю запятую	
										vertexs_count.Clear() ;	//	очищаем "бинарный" список значений координат вершины

										AppendAllTextToObjFile ( writePath , vertex_str ) ; 
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

										normal_str.Add("\nNormals: *" + n_count + " {" + "\na: ");

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

										normal_str.Add( "}" ) ;
										normals_count.Clear() ;	
										//AppendAllTextToObjFile(writePath, normal_str); // переносится в слой после граней
								}



								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ ГРАНИ FACES ( prims ) // если нашли строку "prims..." = 70 72 69 6D 73 ( 00 00 00 )
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x70 & array1d[i+1] == 0x72 & array1d[i+2] == 0x69 & array1d[i+3] == 0x6D & array1d[i+4] == 0x73 )
								{
										// читаем размер блока // не знаю зачем
										// byte[] f_size_four_bytes_int = { array1d[i+8+0] , array1d[i+8+1] , array1d[i+8+2] , array1d[i+8+3] } ; 
										// int fb_size = BitConverter.ToInt32 ( f_size_four_bytes_int , 0 ) ;

								//--------------------------------------------------------------------------------------------------------------

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
										//--------------------------------------------------------------------------------------------------------------
												for ( int ii = 0 ; ii < f_size_count*2 ; ii++ ) 
														primsss_count.Add ( array1d[id + 8 + ii + offset] ) ; 
										//--------------------------------------------------------------------------------------------------------------
												// размер "промежутков" с какой то инфой между блока граней 
												offset = offset + 8 + f_size_count*2 + 44 ; 
												if ( f_count % 2 != 0 ) offset = offset + 2 ;
										//--------------------------------------------------------------------------------------------------------------

												prims_str.Add("\nPolygonVertexIndex: *" + f_count + " {" + "\na: ");

												for ( int iii = 0 ; iii < primsss_count.Count ; iii+=6 )
												{
														byte[] twoBytes1 = { primsss_count[iii+0] , primsss_count[iii+1] } ; // 00 00 // 03 00 
														byte[] twoBytes2 = { primsss_count[iii+2] , primsss_count[iii+3] } ; // 01 00 // 02 00
														byte[] twoBytes3 = { primsss_count[iii+4] , primsss_count[iii+5] } ; // 02 00 // 01 00

														Int16 f1 = (Int16)BitConverter.ToInt16(twoBytes1 , 0) ; // 0
														Int16 f2 = (Int16)BitConverter.ToInt16(twoBytes2 , 0) ; // 1
														Int16 f3 = (Int16)BitConverter.ToInt16(twoBytes3 , 0) ; // 2

														string faces = String.Format ( "{0}, {1}, {2}" , f1 , f2 , ((f3 + 1) / (-1)) ) ;
														if (iii != primsss_count.Count-6) faces = faces + ",";
														prims_str.Add( faces ) ;
													//prims_str.Add( f1 + "," + f2+ "," + ((f3 + 1) / (-1)) + "," ) ; 
												}
											primsss_count.Clear() ;
										}

										prims_str.Add( "}\n" ) ;
										primsss_count.Clear() ;
										AppendAllTextToObjFile(writePath, prims_str);
										AppendAllTextToObjFile(writePath, new List<string>() { "GeometryVersion: 124" , "" } ) ;

/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

// после граней в блоке PolygonVertexIndex в fbx идёт "слой" нормалей // добавляем их

AppendAllTextToObjFile(
writePath,
new List<string>() 
{ 
@"	LayerElementNormal: 0 {
			Version: 101
			Name: """"
			MappingInformationType: ""ByVertex""
			ReferenceInformationType: ""IndexToDirect""
"
}
);

/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

AppendAllTextToObjFile(writePath, normal_str); 

AppendAllTextToObjFile(
writePath,
new List<string>() 
{
@"
NormalsIndex: *" + v_count + @" { a: 
"
}
);

/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

// я предпологаю , что индексы нормалей равны индексам вершин , потому что они нигде не задаются 
// а может вообще их убрать ?

string index_of_normals = "";
for (int ion = 0 ; ion < v_count ; ion++ )
{
	index_of_normals = index_of_normals + ion ;
	if (ion != v_count-1) index_of_normals = index_of_normals + ",";
}

string i_normals = new string(index_of_normals.ToArray());
AppendAllTextToObjFile(writePath, 
new List<string>() { i_normals,""});


AppendAllTextToObjFile(
writePath, 
new List<string>() 
{ 
@"			} ; закрывает NormalsIndex", 
@"	}  ; закрывает LayerElementNormal" 
}
);



								} // if // ИЩЕМ ГРАНИ FACES ( prims )



								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ТЕКСТУРНЫЕ  КООРДИНАТЫ // uvs. // 75 76 73 00 00 00 00 00
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( array1d[i+0] == 0x75 & array1d[i+1] == 0x76 & array1d[i+2]  == 0x73 & array1d[i+3]  == 0x00 ) 
								{
										byte[] vt_count_four_bytes_int = { array1d[i+20+0] , array1d[i+20+1] , array1d[i+20+2] , array1d[i+20+3] } ; 
										int vt_uv = BitConverter.ToInt32(vt_count_four_bytes_int , 0) ;

										for (int ii = 0 ; ii < vt_uv*2*4 ; ii++ ) // количество вершин * 3 координаты * 4 байта
													vt_count.Add ( array1d[i+24+ii] ) ; // где то после 22 стоит не float значение !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


AppendAllTextToObjFile(	//	метод пишет
writePath , 						//	в файл
new List<string>() { 		//	список строк

@"
		LayerElementUV: " + array1d[i+16]
,

@"
		 {
			Version: 101
			Name: """"
			MappingInformationType: ""ByPolygonVertex""
			ReferenceInformationType: ""IndexToDirect""
			UV: *" + vt_uv + @" { a: "
}
)
;
									//vt_str.Add("\n# " + vt_uv + " vt" + " = " + "UVSET " + array1d[i+16]);

										for ( int ii = 0 ; ii < vt_count.Count ; ii+=8 )
										{
												byte[] fourBytes1 = { vt_count[ii+0] , vt_count[ii+1] , vt_count[ii+2 ] , vt_count[ii+3 ] } ; 
												byte[] fourBytes2 = { vt_count[ii+4] , vt_count[ii+5] , vt_count[ii+6 ] , vt_count[ii+7 ] } ; 
												float vu = BitConverter.ToSingle(fourBytes1 , 0) ;
												float vv = BitConverter.ToSingle(fourBytes2 , 0) ;

												string vts = String.Format ( "{0}, {1}" , vu , vv ) ;
												if (ii != vt_count.Count-8) vts = vts + ",";
												vt_str.Add( vts ) ;
											//vt_str.Add( vu + "," + vv  + "," ) ; // записываем строку в файл *.fbx
										}

										vt_str.Add( "}" ) ;
										vt_count.Clear() ;
										AppendAllTextToObjFile(writePath, vt_str); 

AppendAllTextToObjFile(	//	метод пишет
writePath , 						//	в файл
new List<string>() { 		//	список строк
@"
          UVIndex: *" + uv_index_count + @" { a: 
"
}
)
;
										uv_index.Clear() ;

										// добавляем индексы в файл
										uv_str_dub = uv_str.ToList(); // странно , но работает
										if (array1d[i+16]==0x01) AppendAllTextToObjFile(writePath, uv_str); 
										if (array1d[i+16]==0x00) AppendAllTextToObjFile(writePath, uv_str_dub); 

AppendAllTextToObjFile(	//	метод пишет
writePath , 						//	в файл
new List<string>() { 		//	список строк
@"
          }   ;  // закрываем скобки UVIndex
      }  ; закрываем скобки ""тега"" LayerElementUV
"
}
)
;
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
												uv_str.Add( uvs ) ;
											//uv_str.Add( st1 + "," ) ; 
										}
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								//uv_str_dub.Clear() ;
						
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
						
								if ( i == i_end - 42 ) // записываем один раз в конец файла
								{

AppendAllTextToObjFile(	//	метод пишет
writePath , 						//	в файл
new List<string>() { 		//	список строк

@"
		LayerElementMaterial: 0 
		{
			Version: 101
			Name: """"
			MappingInformationType: ""AllSame""
			ReferenceInformationType: ""IndexToDirect""
			Materials: *1 {
				a: 0
			} 
		}

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
		}

		Layer: 1 
		{
			Version: 100
			LayerElement:  {
				Type: ""LayerElementUV""
				TypedIndex: 0
			}
		}

		Layer: 2 
		{
			Version: 100
			LayerElement:  {
				Type: ""LayerElementUV""
				TypedIndex: 1
			}
		}
	}	; закрывает Geometry


	Model: 3, ""Model::Mesh0"", ""Mesh"" 
	{
		Version: 232
		Properties70:  
		{
			P: ""DefaultAttributeIndex"", ""int"", ""Integer"", """", 0
		}
	}


}	; закрывает Objects
"
,

@"

; ====================================================
; Object connections  
; ----------------------------------------------------  

Connections:  
{
	;""Model::Mesh0"", ""Model::RootNode""
	C: ""OO"",3,0
	;""Geometry::"", ""Model::Mesh0""
	C: ""OO"",2,3
}

; ====================================================
;Takes section  
; ----------------------------------------------------  

Takes:  {
}
"
}
);
								}

						} // for // прошли по всему содержимому массива байт прочитанных из файла


//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

		 		} // foreach // прошли по каждому файлу
		} // Main
} // class

// жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
