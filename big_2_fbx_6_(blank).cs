//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
using System; using System.IO; using System.Linq; using System.Text; using System.Collections; using System.Collections.Generic; 
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

sealed class big2fbx
{
		static int   i = 0; // индекс в массиве байт всего файла	// используется в "вычислении" граней

//	это для того , чтобы расчитать конец блока модели и записать в файл теги идущие после её геометрии
		static int   i_begin = 0; // индекс начала блока модели
		static int   i_offset = 0; // индекс смещения блока модели
		static int   i_end = 0; // индекс конца блока модели

		static string big_path; // хранит путь к файлу *.big
		static string writePath; // хранит путь к файлу *[i ].fbx

		static int v_count = 0; // хранит количество вершин , которому будет равно кол-во нормалей 
		static int fs_count = 0; // количество сабмешей и это же количество моделей в сцене 
		static int fi = 0; // используется как индекс в списке списков 
		static int rootNode = 0; // всегда индекс этого узла равен нолю для удобства в блоке connections
		
		static string[] files; // зачем я это сюда вынес ?

		static bool uvs_flag = false; // у некоторых моделей отсутствует блок uv // например detector.model

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

		static void AppendAllTextToObjFile ( string fileName, List<string> text )
		{
				using ( StreamWriter writer = File.AppendText ( fileName ) )
				{
				    foreach ( string line in text )
				    writer.WriteLine ( line ); 
				    text.Clear(); 
				}
		}

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
		static void Main ( )	
		{
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
				System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo ( "en-US" );	//	чтобы в float числах - была точка вместо запятой
				files = Directory.GetFiles ( Directory.GetCurrentDirectory ( ), "*.model", SearchOption.AllDirectories )	; // ищет файлы с расширением *.big в подпапках
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

//			https://docs.microsoft.com/ru-ru/visualstudio/code-quality/ca1006-do-not-nest-generic-types-in-member-signatures?view=vs-2015

				List<byte> vx_Hex_List = new List<byte>()	;	List<string> vx_Str_List  = new List<string>();	// вершины
				List<byte> vn_Hex_List = new List<byte>()	;	List<string> vn_Str_List = new List<string>();	// нормали
				List<byte> f_Hex_List  = new List<byte>()	;	List<string> f_Str_List  = new List<string>();	// грани

				List<List<string>> subMeshFacesStr = new List<List<string>>(); 
				// хранит наборы граней для каждой сабмеши , где [номер_сабмеши][номер_грани]

				     List<int>	vx_index_SubM = new List<int>()	;	//	содержит уникальные индексы вершин для одной сабмеши
				List<List<int>> vx_index_List = new List<List<int>>();	//	исп~ся для выборки вершин, из общего списка, для каждой сабмеши

				List<byte> vt_Hex_List = new List<byte>()	; List<string> vt_uv___str_List1	= new List<string>(); // uvs1
																									; List<string> vt_uv___str_List2	= new List<string>(); // uvs2
				List<byte> uv_Hex_List = new List<byte>()	; List<string> vt_uv_i_str_List = new List<string>(); // uvs_index

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

						 List<int>       VertexIndexList = new      List<int>();	// сюда будем добавлять появляющиеся/считываемые индексы вершин одной сабмеши
				List<List<int>> List_VertexIndexList = new List<List<int>>();	// сюда будем добавлять сформированные списки индексов вершин каждой сабмеши

						 List<int>       UniqVertexIndexList = new      List<int>();	//	сюда будем помещать уникальные индексы вершин одной сабмеши
				List<List<int>> List_UniqVertexIndexList = new List<List<int>>();	//	сюда будем помещать сформированные списки индексов вершин каждой сабмеши

						List<string> faceIndexString_List = new List<string>();	//	список строк содержащих PolygonVertexIndex-блок каждой сабмеши

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

				foreach ( var file in files )	//	для всех имён файлов из списка имён файлов
				{
						Console.WriteLine ( file ); // пишем имя файла в консоль
						byte[] AllBytes = File.ReadAllBytes ( file ); //	читаем из файла все байты в массив
						int files_name_counter = 1; // счётчик моделей и имён файлов для них

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

						// приходится проходить по всем байтам в файле , потому что я не разбирался с ReadBytes

						for ( i = 0; i < AllBytes.Length - 42; i++ ) // - 7 , потому что ищем до размер файла - 7 байт . ПОНЯТНО ? ( нет ) ( да ) // 22 // 40 // 42 // 44 
						{
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ НАЧАЛО МОДЕЛИ 69 00 00 00 64 65 66 61 75 6C 74 00
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( AllBytes[i + 0] == 0x69 & AllBytes[i + 1] == 0x00 & AllBytes[i + 2] == 0x00 & AllBytes[i + 3] == 0x00 &
										 AllBytes[i + 4] == 0x64 & AllBytes[i + 5] == 0x65 & AllBytes[i + 6] == 0x66 & AllBytes[i + 7] == 0x61 &
										 AllBytes[i + 8] == 0x75 & AllBytes[i + 9] == 0x6C & AllBytes[i + 10] == 0x74 & AllBytes[i + 11] == 0x00 )
								{
										// если в модели нет блока вершин , то break // например pmarker.model 
										if ( AllBytes[i + 56] != 0x70 ) break; 

								// 	читаем размер "блока" модели
										byte[] ModelBlockSize = { AllBytes[i + 12 + 0], AllBytes[i + 12 + 1], AllBytes[i + 12 + 2], AllBytes[i + 12 + 3] }; 
										i_offset = BitConverter.ToInt32 ( ModelBlockSize , 0 ); 
										i_begin = i; i_end = i_begin + i_offset; 

										big_path = Path.GetDirectoryName ( file ); 
										writePath = big_path + "/" + Path.GetFileNameWithoutExtension ( file ) + "_" + files_name_counter + ".fbx"; //		.fbx		.obj
										files_name_counter++; // нашли "вершины" - увеличили счётчик найденных моделей // это может стоять после всех "блоков" модели?
										if ( File.Exists ( writePath ) ) File.Delete ( writePath ); 
										//	если файл с таким именем существует , то удаляем его , чтобы потом создать новый с тем же именем
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ ВЕРШИНЫ // если нашли строку "position" = 00 00 00 00 70 6F 73 69 74 69 6F 6E 
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( AllBytes[i + 0] == 0x00 & AllBytes[i + 1] == 0x00 & AllBytes[i + 2] == 0x00 & AllBytes[i + 3] == 0x00 &
										 AllBytes[i + 4] == 0x70 & AllBytes[i + 5] == 0x6F & AllBytes[i + 6] == 0x73 & AllBytes[i + 7] == 0x69 &
										 AllBytes[i + 8] == 0x74 & AllBytes[i + 9] == 0x69 & AllBytes[i + 10] == 0x6F & AllBytes[i + 11] == 0x6E )
								{
										// через 20 байт ( от начала "сигнатуры" ) записано количество вершин , считываем и запоминаем его
										byte[] vSize = { AllBytes[i + 20 + 0], AllBytes[i + 20 + 1], AllBytes[i + 20 + 2], AllBytes[i + 20 + 3] }; 
										v_count = BitConverter.ToInt32 ( vSize , 0 ); // количество вершин

										// эта цифра указывает сколько ( байт*4 ) надо считать // например она равна 6 , значит нужно считать 6 пар типа [ 00 00 00 00 ]
										// Console.WriteLine ( "Количество вершин = " + v_count + "\t" + "Количество байт = "   + v_count*3*4 + "\n" ); 
										// ещё через 4 начинается список координат вершин [i + 22 ]	// считываем все байты содержащие "значения" вершин в массив

										for ( int ii = 0; ii < v_count*3*4; ii++ ) // количество вершин * 3 координаты * 4 байта
												vx_Hex_List.Add ( AllBytes[i + 24 + ii] ); // где то после 22 стоит не float значение !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

								//--------------------------------------------------------------------------------------------------------------

										for ( int ii = 0; ii < vx_Hex_List.Count; ii += 12 ) // float занимает 4 байта и их по три координаты
										{	
												byte[] fB1 = { vx_Hex_List[ii + 0], vx_Hex_List[ii + 1], vx_Hex_List[ii + 2], vx_Hex_List[ii + 3] }; 
												byte[] fB2 = { vx_Hex_List[ii + 4], vx_Hex_List[ii + 5], vx_Hex_List[ii + 6], vx_Hex_List[ii + 7] }; 
												byte[] fB3 = { vx_Hex_List[ii + 8], vx_Hex_List[ii + 9], vx_Hex_List[ii + 10], vx_Hex_List[ii + 11] }; 

												float v1 = BitConverter.ToSingle ( fB1 , 0 ); 
												float v2 = BitConverter.ToSingle ( fB2 , 0 ); 
												float v3 = BitConverter.ToSingle ( fB3 , 0 ); 

												vx_Str_List.Add ( v1 + "	,	" + v2 + "	,	" + v3 ); 
										}
										
										vx_Hex_List.Clear();
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ Vn ( нормали )
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( AllBytes[i + 0] == 0x6E & AllBytes[i + 1] == 0x6F & AllBytes[i + 2] == 0x72 & AllBytes[i + 3] == 0x6D &
										 AllBytes[i + 4] == 0x61 & AllBytes[i + 5] == 0x6C & AllBytes[i + 6] == 0x73 & AllBytes[i + 7] == 0x00 )
								{
										byte[] vnSize = { AllBytes[i + 16 + 0], AllBytes[i + 16 + 1], AllBytes[i + 16 + 2], AllBytes[i + 16 + 3] }; 
										int vn_count = BitConverter.ToInt32 ( vnSize , 0 ); 

										for ( int ii = 0; ii < vn_count*3*4; ii++ )
										vn_Hex_List.Add ( AllBytes[i + 20 + ii] ); 

										for ( int ii = 0; ii < vn_Hex_List.Count; ii += 12 )
										{
												byte[] fB1 = { vn_Hex_List[ii + 0], vn_Hex_List[ii + 1], vn_Hex_List[ii + 2], vn_Hex_List[ii + 3] }; 
												byte[] fB2 = { vn_Hex_List[ii + 4], vn_Hex_List[ii + 5], vn_Hex_List[ii + 6], vn_Hex_List[ii + 7] }; 
												byte[] fB3 = { vn_Hex_List[ii + 8], vn_Hex_List[ii + 9], vn_Hex_List[ii + 10], vn_Hex_List[ii + 11] }; 

												float vn1 = BitConverter.ToSingle ( fB1 , 0 ); 
												float vn2 = BitConverter.ToSingle ( fB2 , 0 ); 
												float vn3 = BitConverter.ToSingle ( fB3 , 0 ); 

												vn_Str_List.Add ( vn1 + "	,	" + vn2 + "	,	" + vn3 ); 
										}
										vn_Hex_List.Clear();
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ИЩЕМ ГРАНИ FACES ( prims ) // если нашли строку "prims..." = 70 72 69 6D 73 ( 00 00 00 )
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( AllBytes[i + 0] == 0x70 & AllBytes[i + 1] == 0x72 & AllBytes[i + 2] == 0x69 
								   & AllBytes[i + 3] == 0x6D & AllBytes[i + 4] == 0x73 )
								{
										int id = i + 24; // это было очень сложно 
										int offset = 0; // 4 8 12 16 18 40
										fs_count = AllBytes[i + 16]; // читаем количество "саб"-мешей

/*sf*/							for ( fi = 0; fi < fs_count; fi++ )	// повторяем необходимое кол-во раз равное кол-ву саб-мешей
										{
												byte[] f_count_four_bytes_int = { // читаем количество строчек f в файле obj // c 24 по 27
												AllBytes[id+0+offset] , AllBytes[id+1+offset] , AllBytes[id+2+offset] , AllBytes[id+3+offset] }; 
												int f_count = BitConverter.ToInt32 ( f_count_four_bytes_int , 0 ); // количество строк

												byte[] f_count_four_bytes_int_size = { // читаем количество байт , которых нужно умножить на два // c 28 по 31
												AllBytes[id+4+0+offset] , AllBytes[id+4+1+offset] , AllBytes[id+4+2+offset] , AllBytes[id+4+3+offset] }; 
												int f_size_count = BitConverter.ToInt32 ( f_count_four_bytes_int_size , 0 ); // 

										//	эта цифра указывает сколько байт*2 надо считать // например она равна 6 , значит нужно считать 6 пар типа [ 00 00 ]
										//	Console.WriteLine ( "Количество граней = " + f_count/3 + "\t" + "Количество байт = "   + f_count*2 + "\n" ); 

										//--------------------------------------------------------------------------------------------------------------
												for ( int ii = 0; ii < f_size_count*2; ii++ ) 
														f_Hex_List.Add ( AllBytes[id + 8 + ii + offset] ); 
										//--------------------------------------------------------------------------------------------------------------

										//	размер "промежутков" с какой то инфой между блока граней 
												offset = offset + 8 + f_size_count*2 + 44; 
												if ( f_count % 2 != 0 ) offset = offset + 2; 

										//--------------------------------------------------------------------------------------------------------------

												for ( int iii = 0; iii < f_Hex_List.Count; iii += 6 )
												{
														byte[] twoBytes1 = { f_Hex_List[iii + 0], f_Hex_List[iii + 1] }; // 00 00 
														byte[] twoBytes2 = { f_Hex_List[iii + 2], f_Hex_List[iii + 3] }; // 01 00
														byte[] twoBytes3 = { f_Hex_List[iii + 4], f_Hex_List[iii + 5] }; // 02 00

												//	convert vertex index из числа в строку

														Int16 vi1		=	( Int16 )BitConverter.ToInt16 ( twoBytes1 , 0 ); // 0
														Int16 vi2		=	( Int16 )BitConverter.ToInt16 ( twoBytes2 , 0 ); // 1
														Int16 vi3_	=	( Int16 )BitConverter.ToInt16 ( twoBytes3 , 0 ); // 1
														Int32 vi3		=	( ( Int16 )BitConverter.ToInt16 ( twoBytes3 , 0 ) + 1 ) / ( -1 ); // 2 или -3

												//	List<int> хранит "уникальные" vertex index из граней сабмеша
												//	чтобы потом выбирать вершины из общего списка для каждой сабмеши

												//	List<int>.Add(int);
														if (!vx_index_SubM.Contains(vi1 )) vx_index_SubM.Add(vi1);	
														if (!vx_index_SubM.Contains(vi2 )) vx_index_SubM.Add(vi2);
														if (!vx_index_SubM.Contains(vi3_)) vx_index_SubM.Add(vi3_);		

												//	создаём строки вида f v1 v2 ((vi3 + 1)/( -1 ))
														string faces = String.Format ( "{0}, {1}, {2}" , vi1 , vi2 , vi3 );

												//	if ( iii != f_Hex_List.Count - 6 ) faces = faces + ",";
												//	это не работает , да и запятая не нужна, потому что отрицательное значение индекса вершины , говорит об окончании

													//List<string>.Add(String);
														f_Str_List.Add ( faces ); // добавляем строку вида 1 2 -2 в список граней

//\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

if (!UniqVertexIndexList.Contains(vi1 )) UniqVertexIndexList.Add (vi1 ); 
if (!UniqVertexIndexList.Contains(vi2 )) UniqVertexIndexList.Add (vi2 ); 
if (!UniqVertexIndexList.Contains(vi3_)) UniqVertexIndexList.Add (vi3_); 

VertexIndexList.Add(vi1 );	
VertexIndexList.Add(vi2 );	
VertexIndexList.Add(vi3_);

///////////////////////////////////////////////////////////

												} // внутренний цикл

List_UniqVertexIndexList.Add(UniqVertexIndexList.ToList());
														 UniqVertexIndexList.Clear();

List_VertexIndexList.Add(VertexIndexList.ToList());
												 VertexIndexList.Clear();

										//--------------------------------------------------------------------------------------------------------------

											//List<List<string>>.Add( List<string>.ToList() )
												subMeshFacesStr.Add ( f_Str_List.ToList() ); // добавляем строки в subMeshFacesStr
												f_Str_List.Clear(); // очищаем для следущей сабмеши

											//List<List<int>>.Add(List<int>.ToList());
												vx_index_List.Add ( vx_index_SubM.ToList() );
												vx_index_SubM.Clear(); 
												f_Hex_List.Clear(); 

										} // внешний цикл

	int[][] NewVertexIndex = new int[fs_count][];

	for (int iiii = 0; iiii < List_VertexIndexList.Count; iiii++ )
	{	
			NewVertexIndex[iiii] = List_VertexIndexList[iiii].ToArray();	//	инициализируем значениями
	}

	for (int i_ = 0; i_ < List_VertexIndexList.Count; i_++)										{
		for (int j = 0; j < List_UniqVertexIndexList.Count; j++)								{
			for (int k = 0; k < List_VertexIndexList[i_].Count; k++)							{
				for (int l = 0; l < List_UniqVertexIndexList[j].Count; l++)					{
					if (List_VertexIndexList[i_][k] == List_UniqVertexIndexList[j][l]){
						NewVertexIndex[i_][k] = l; break; 											}	}	}	}	}

string faceIndexString="";

for (int i__ = 0; i__ < fs_count; i__++) {
		for (int j = 0 , j2 = 1; j < NewVertexIndex[i__].Length; j++, j2++)	{
			if ( j2 % 3 == 0 ) NewVertexIndex[i__][j] = (NewVertexIndex[i__][j] + 1 ) / ( -1 );
			var stringsArray = NewVertexIndex[i__].Select(ind => ind.ToString()).ToArray();
			faceIndexString = string.Join(",", stringsArray);
}			faceIndexString_List.Add(faceIndexString);
}

								}	// if // ИЩЕМ ГРАНИ FACES ( prims )

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// ТЕКСТУРНЫЕ  КООРДИНАТЫ // uvs. // 75 76 73 00 00 00 00 00
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( AllBytes[i + 0] == 0x75 & AllBytes[i + 1] == 0x76 & AllBytes[i + 2] == 0x73 & AllBytes[i + 3] == 0x00 ) 
								{
										uvs_flag = true; 

										byte[] vtSize = { AllBytes[i + 20 + 0], AllBytes[i + 20 + 1], AllBytes[i + 20 + 2], AllBytes[i + 20 + 3] }; 
										int vt_uv = BitConverter.ToInt32 ( vtSize , 0 ); 

										for ( int ii = 0; ii < vt_uv*2*4; ii++ ) // количество_вершин * 3 координаты * 4 байта
													vt_Hex_List.Add ( AllBytes[i + 24 + ii] ); // где то после 22 стоит не float значение !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

										for ( int ii = 0; ii < vt_Hex_List.Count; ii += 8 )
										{
												byte[] fB1 = { vt_Hex_List[ii + 0], vt_Hex_List[ii + 1], vt_Hex_List[ii + 2], vt_Hex_List[ii + 3] }; 
												byte[] fB2 = { vt_Hex_List[ii + 4], vt_Hex_List[ii + 5], vt_Hex_List[ii + 6], vt_Hex_List[ii + 7] }; 

												float vu = BitConverter.ToSingle ( fB1 , 0 ); 
												float vv = ( -1 )*BitConverter.ToSingle ( fB2 , 0 ); 

												vt_uv___str_List1.Add ( vu + "	,	" + vv ); 
										}

										vt_Hex_List.Clear();
								//	if (AllBytes[i + 0 ]найдено второе увс) то
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
								// uvs index , индексы текстурных координат , "блок" textures , 00 00 FF FF 74 65 78 74 75 72 65 73
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( AllBytes[i + 0] == 0x00 & AllBytes[i + 1] == 0x00 & AllBytes[i + 2] == 0xFF & AllBytes[i + 3] == 0xFF &	//	00 00 FF FF
										 AllBytes[i + 4] == 0x74 & AllBytes[i + 5] == 0x65 & AllBytes[i + 6] == 0x78 & AllBytes[i + 7] == 0x74 &	//	74 65 78 74
										 AllBytes[i + 8] == 0x75 & AllBytes[i + 9] == 0x72 & AllBytes[i + 10] == 0x65 & AllBytes[i + 11] == 0x73 )	//	75 72 65 73
								{
										byte[] uvSize = { AllBytes[i + 20 + 0], AllBytes[i + 20 + 1], AllBytes[i + 20 + 2], AllBytes[i + 20 + 3] }; 
										int uvCount = BitConverter.ToInt32 ( uvSize , 0 ); // получили количество индексов

										for ( int ii = 0; ii < uvCount*2; ii++ )	//	одно число занимает два байта	//	прочитали если индексов 4 то 8 байт
													uv_Hex_List.Add ( AllBytes[i + 24 + ii] );	//	добавляем индексы в список

										for ( int iii = 0; iii < uv_Hex_List.Count; iii += 2 )	//	добавляем индексы в виде строк в файл
										{
												byte[] twoBytes1 = { uv_Hex_List[iii + 0], uv_Hex_List[iii + 1] }; // 00 00  
												Int16 st1 = ( Int16 )BitConverter.ToInt16 ( twoBytes1 , 0 ); // 0

												string uvs = String.Format ( "{0}" , st1 ); 
												if ( iii != uv_Hex_List.Count-2 ) uvs = uvs + ","; 
												vt_uv_i_str_List.Add ( uvs ); // vt_uv_i_str_List.Add ( st1 + "," ); 
										}

										uv_Hex_List.Clear();	//	очищаем байтовый-список uvs index блока, которые уже добавлены в виде строк
								}

								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								//vt_uv_i_str_List_dub.Clear(); 
						
								//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

								if ( i == i_end - 42 ) // если нашли "конец" модели , то записываем один раз в конец файла
								{

/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

AppendAllTextToObjFile ( writePath , new List<string> ( ) {

//@"; " + file // <source_data>file:///C:/file%20name.big</source_data> , 

@"
FBXHeaderExtension:  {
	FBXHeaderVersion: 1003
	FBXVersion: 7300
}

GlobalSettings:  {
	Version: 1000
}

; ==================================================== 
; Documents Description
; ----------------------------------------------------

Documents:  
{
	Count: 1
	Document: 1, """", ""Scene""
	{
		RootNode: " + rootNode + @"
	}
}

; ==================================================== 
; Object definitions
; ----------------------------------------------------

Definitions: 
{
	Version: 100
	Count: " + (  ( fs_count*5 ) + 1 ) + @"

	ObjectType: ""GlobalSettings"" {	
	Count: 1	
	}
	ObjectType: ""Model""	{
		Count: " + fs_count + @"	
		PropertyTemplate: ""FbxNode"" { 	
		}
	}
	ObjectType: ""Material""	{
		Count: " + fs_count + @"	
		PropertyTemplate: ""FbxSurfacePhong"" { 	
		}
	}
	ObjectType: ""Texture""	{
		Count: " + fs_count + @"	
		PropertyTemplate: ""FbxFileTexture"" { 	
		}
	}
	ObjectType: ""Video""	{
		Count: " + fs_count + @"
		PropertyTemplate: ""FbxVideo"" { 	
		}
	}
	ObjectType: ""Geometry""	{
		Count: " + fs_count + @"
		PropertyTemplate: ""FbxMesh"" { 	
		}
	}
}

; ==================================================== 
; Object properties
; ----------------------------------------------------

Objects: 
{
"
});

/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

int index = 0;	// увеличивает номера блоков	//	делая их уникальными	//	для секции Connections

for ( int smi = 0; smi < fs_count; smi++ )	//	для каждой сабмеши добавляем в файл отдельные блоки Geometry , Normal , UV , Material 
{

AppendAllTextToObjFile ( writePath , new List<string> ( ) {

@"
	Geometry: " + (++index) + @", ""Geometry::SMg"", ""Mesh"" 
	{
"
});

//*****************************************************************************
//	ДОБАВЛЯЕМ СПИСОК ВЕРШИН vx_index_List
//*****************************************************************************

AppendAllTextToObjFile ( writePath , new List<string> ( ) {
@"	Vertices: *" + vx_index_List[smi].Count + @" { 
a: "
});

for ( int w = 0; w < vx_index_List[smi].Count; w++ )
{
		if (w != vx_index_List[smi].Count-1 )	
				vx_Str_List[vx_index_List[smi][w]] = 
				vx_Str_List[vx_index_List[smi][w]] + ",";

		AppendAllTextToObjFile ( writePath , new List<string> ( ) 
		{
		@"	" + vx_Str_List[vx_index_List[smi][w]]
		});
}

AppendAllTextToObjFile ( writePath , new List<string> ( ) {
@"	}
"});

//*****************************************************************************
//	ДОБАВЛЯЕМ СПИСОК ГРАНЕЙ subMeshFacesStr
//*****************************************************************************

AppendAllTextToObjFile ( writePath , new List<string> ( ) {
@"	PolygonVertexIndex: *" + subMeshFacesStr[smi].Count + @" { 
a: "
});

//for ( int w = 0; w < subMeshFacesStr[smi].Count; w++ ){
		AppendAllTextToObjFile ( writePath , new List<string> ( ) 
		{
				//@"	" + subMeshFacesStr[smi][w]
				@"	" + faceIndexString_List[smi]
		});
//}

AppendAllTextToObjFile ( writePath , new List<string> ( ) {@"	} "});

//*****************************************************************************
//	просто нужные строчки
//*****************************************************************************

AppendAllTextToObjFile ( writePath , new List<string> ( ) 
{
@"	
	GeometryVersion: 124"
});

//*****************************************************************************
//	ДОБАВЛЯЕМ СПИСОК НОРМАЛЕЙ vn_Str_List
//*****************************************************************************

AppendAllTextToObjFile ( writePath , new List<string> ( ) {
@"
	LayerElementNormal: 0 	{
	Version: 101
	Name: """"
	MappingInformationType: ""ByVertex""
	ReferenceInformationType: ""IndexToDirect""

	Normals: *" + vx_index_List[smi].Count + @" { 
	a: "
});

for ( int w = 0; w < vx_index_List[smi].Count; w++ )
{
		if (w != vx_index_List[smi].Count-1 )	
				vn_Str_List[vx_index_List[smi][w]] = 
				vn_Str_List[vx_index_List[smi][w]] + ",";

		AppendAllTextToObjFile ( writePath , new List<string> ( ) 
		{
		@"			" + vn_Str_List[vx_index_List[smi][w]]
		});
}

AppendAllTextToObjFile ( writePath , new List<string> ( ) {
@"
		; NormalsIndex: *" + vx_index_List[smi].Count + @" { a: ...}" +
@"
		}
	}
"
});

//*****************************************************************************
// vt_uv___str_List1
//*****************************************************************************

// if ( есть блок uv и число uv совпадает с числом вершин ) 
// например в psntanm_lod.model их в два раза меньше 
if ( uvs_flag && ( vt_uv___str_List1.Count == vx_Str_List.Count ) )
{

AppendAllTextToObjFile ( writePath , new List<string> ( ) {
@"	LayerElementUV: 0 	{ 
		Version: 101
		Name: ""diffuse_uv_layer""
		MappingInformationType: ""ByVertex""
		ReferenceInformationType: ""IndexToDirect""
		UV: *" + vx_index_List[smi].Count + @" { 
		a: "
});

for ( int w = 0; w < vx_index_List[smi].Count; w++ )
{
		if (w != vx_index_List[smi].Count-1 )	
				vt_uv___str_List1[vx_index_List[smi][w]] = 
				vt_uv___str_List1[vx_index_List[smi][w]] + ",";

		AppendAllTextToObjFile 
		( writePath , new List<string> ( ) 
		{
		@"		" + vt_uv___str_List1[vx_index_List[smi][w]]		
		});
}

AppendAllTextToObjFile ( writePath , new List<string> ( ) {
@"		}" 
});

//*****************************************************************************
//*****************************************************************************
//*****************************************************************************

// UVIndex

AppendAllTextToObjFile ( writePath , new List<string> ( ) {
@"

 UVIndex: *4 {
a:
0,1,2,3
 }
"
});

//*****************************************************************************
//*****************************************************************************
//*****************************************************************************

AppendAllTextToObjFile ( writePath , new List<string> ( ) // закрываем LayerElementUV
{
@"
		}
"
});

}	//	if (uvs_flag)

uvs_flag = false;

//*****************************************************************************
//*****************************************************************************
//*****************************************************************************

/*
AppendAllTextToObjFile ( writePath , new List<string> ( ) {
@"	LayerElementMaterial: 0 	{
		Version: 101
		Name: """"
		MappingInformationType: ""ByVertex""; AllSame
		ReferenceInformationType: ""IndexToDirect""
		Materials: *1 {
			a: 0
		}
	}
"});
*/

//*****************************************************************************
//*****************************************************************************
//*****************************************************************************

AppendAllTextToObjFile ( writePath , new List<string> ( ) {
@"	Layer: 0 	{

		Version: 100

		LayerElement:  	{		
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
}
"
,

@"; ----------------------------------------------------

	Model: " + (++index) + @", ""Model::SM"", ""Mesh"" {
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

	Material: " + (++index) + @", ""Material::MaterialMap"", """" {
		Version: 102
		ShadingModel: ""phong""
		MultiLayer: 0
	}

	Video: " + (++index) + @", ""Video::videoIMAGE"", ""Clip"" {
		Type: ""Clip""
		Properties70:  {
			P: ""Path"", ""KString"", ""XRefUrl"", """", ""C:\Users\robto\Downloads\videoIMAGE.png""
		}
		UseMipMap: 0
		Filename: ""C:\Users\robto\Downloads\videoIMAGE.png""
		RelativeFilename: ""..\Users\robto\Downloads\videoIMAGE.png""
	}

	Texture: " + (++index) + @", ""Texture::IMAGE"", """" {
		Type: ""TextureVideoClip""
		Version: 202
		TextureName: ""Texture::IMAGE""
		Properties70:  {
			P: ""UVSet"", ""KString"", """", """", """"
			P: ""UseMaterial"", ""bool"", """", """",1
			P: ""UseMipMap"", ""bool"", """", """",1
			P: ""AlphaSource"", ""enum"", """", """",2
		}

		Media: ""Video::videoIMAGE""
		FileName: ""C:\Users\robto\Downloads\videoIMAGE.png""
		RelativeFilename: ""..\Users\robto\Downloads\videoIMAGE.png""
		ModelUVTranslation: 0,0
		ModelUVScaling: 1,1
		Texture_Alpha_Source: ""None""
		Cropping: 0,0,0,0
	}

; ====================================================
"

});

}//	for для добавления каждой сабмеши

/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/
/**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

AppendAllTextToObjFile ( writePath , new List<string> ( ) {

@"}; закрывает Objects

; ====================================================
; Object connections
; -----------------------------------------------------

Connections:; " + fs_count + @"
{"
}); 

//*****************************************************************************

for ( int coni = 0 , coniplus = 0; coni < fs_count; coni++ , coniplus += 4 )
{
		AppendAllTextToObjFile ( writePath , new List<string> ( ) {

@"	; -----------------------------------------------------

		C: ""OO"",	" + (coni+coniplus+2) + @"	,	" + rootNode 					+ @"											;	Model::RootNode, Model::SM
		C: ""OO"",	" + (coni+coniplus+1) + @"	,	" + (coni+coniplus+2) + @"											; Geometry::SMg, Model::SM
		C: ""OO"",	" + (coni+coniplus+3) + @"	,	" + (coni+coniplus+2) + @"											; Material::MaterialMap, Model::SM
		C: ""OP"",	" + (coni+coniplus+5) + @"	,	" + (coni+coniplus+3) + @"	, ""DiffuseColor""	; Texture::IMAGE, Material::MaterialMap
		C: ""OO"",	" + (coni+coniplus+4) + @"	,	" + (coni+coniplus+5) + @"											; Video::videoIMAGE, Texture::IMAGE
		"

	}); 

}

//*****************************************************************************

AppendAllTextToObjFile ( writePath , new List<string> ( ) {

@"		; -----------------------------------------------------
}"

});

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

vx_index_List.Clear();
vx_Str_List.Clear();
vn_Str_List.Clear();
vt_uv___str_List1.Clear();
vt_uv_i_str_List.Clear();
subMeshFacesStr.Clear();	//	очищаем список саб-мешей данной модели

List_VertexIndexList.Clear();
List_UniqVertexIndexList.Clear();
faceIndexString_List.Clear();

								} // если нашли конец модели 
								
							//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж

						}// for // прошли по всему содержимому массива байт прочитанных из файла

		 		}// foreach // прошли по каждому файлу

		}// Main

}// class

//жжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжжж
