// �������� - �������� 

//�������������������������������������������������������������������������������������������������������������������������������

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

//�������������������������������������������������������������������������������������������������������������������������������

sealed class big2obj
{

//�������������������������������������������������������������������������������������������������������������������������������

		static string big_path  ; // ������ ���� � ����� *.big
		static string writePath ; // ������ ���� � ����� *[i].obj
		static int   i = 0 ; // ������ � ������� ���� ����� �����

//�������������������������������������������������������������������������������������������������������������������������������

		static void Main()
		{

//�������������������������������������������������������������������������������������������������������������������������������

				// ���� ����� � ����������� *.big � ���������
				string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.big", SearchOption.AllDirectories) ; 

//�������������������������������������������������������������������������������������������������������������������������������

				List<byte> vertexs_count = new List<byte>() ; // �������� ������ �������� ����������
				List<byte> normals_count = new List<byte>() ; // �������� ������ �������� ����������
				List<byte> primsss_count = new List<byte>() ; // �������� ������ �������� ����������
				List<byte> ddsList = new List<byte>() ;

//�������������������������������������������������������������������������������������������������������������������������������

				foreach ( var file in files ) // ��� ������ ������ , ���������� ��� �����.big , ������������ � ������� ����� 
				{
						Console.WriteLine(file) ; // ����� ��� ����� � �������
						byte[] array1d = File.ReadAllBytes(file) ; // ������ ��� ����� �� ����� 
						int files_name_counter = 1 ; // ������� ������� � ��� ������ ��� ���

//�������������������������������������������������������������������������������������������������������������������������������

						for ( i = 0 ; i < array1d.Length - 42 ; i++ ) // - 7 , ������ ��� ���� �� ������ ����� - 7 ���� . ������� ? (���) (��) // 22 // 40 // 42 // 44 
						{
								//��������������������������������������������������������������������������������������������������������������
								// ���� �������
								//��������������������������������������������������������������������������������������������������������������

								//---position---position---position---position---position---position---position

								// ���� ����� ������ "position" = 00 00 00 00 70 6F 73 69 74 69 6F 6E 

								if ( array1d[i+0] == 0x00 & array1d[i+1] == 0x00 & array1d[i+2]  == 0x00 & array1d[i+3]  == 0x00 &
										 array1d[i+4] == 0x70 & array1d[i+5] == 0x6F & array1d[i+6]  == 0x73 & array1d[i+7]  == 0x69 &
										 array1d[i+8] == 0x74 & array1d[i+9] == 0x69 & array1d[i+10] == 0x6F & array1d[i+11] == 0x6E )
								{
										// ����� 20 ���� (�� ������ "���������") �������� ���������� ������ , ��������� � ���������� ���
//*
										byte[] v_count_four_bytes_int = { array1d[i+20+0] , array1d[i+20+1] , array1d[i+20+2] , array1d[i+20+3] } ; 
										int v_count = BitConverter.ToInt32(v_count_four_bytes_int , 0) ;

										// ��� ����� ��������� ������� (����*4) ���� ������� // �������� ��� ����� 6 , ������ ����� ������� 6 ��� ���� [00 00 00 00]

										// Console.WriteLine("���������� ������ = " + v_count + "\t" + "���������� ���� = "   + v_count*3*4 + "\n" ) ;

										// ��� ����� 4 ���������� ������ ��������� ������ [i+22]
										// ��������� ��� ����� ���������� "��������" ������ � ������

										for ( int ii = 0 ; ii < v_count*3*4 ; ii++ ) // ���������� ������ * 3 ���������� * 4 �����
										{
												vertexs_count.Add ( array1d[i+24+ii] ) ; // ��� �� ����� 22 ����� �� float �������� !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
										}
//*/
										// --------------------------------------------------------------------------------------------------------------

										big_path = System.IO.Path.GetDirectoryName(file);
										writePath = big_path + "/" + Path.GetFileNameWithoutExtension(file) + "___" + files_name_counter + ".obj" ;
										if (File.Exists(writePath)) File.Delete(writePath);

										// --------------------------------------------------------------------------------------------------------------
//*
										using ( StreamWriter sw = new StreamWriter ( writePath , true , System.Text.Encoding.Default ) )
										{
//!!!!!!!!!!!!!!!!!!
												sw.WriteLine( "# {0} vertices " , v_count ) ; // ���������� ������ � ���� *.obj
										}

										// --------------------------------------------------------------------------------------------------------------

										for ( int ii = 0 ; ii < vertexs_count.Count ; ii+=12 )
										{
												// float �������� 4 ����� � �� �� ��� ����������

												byte[] fourBytes1 = { vertexs_count[ii+0] , vertexs_count[ii+1] , vertexs_count[ii+2 ] , vertexs_count[ii+3 ] } ; 
												byte[] fourBytes2 = { vertexs_count[ii+4] , vertexs_count[ii+5] , vertexs_count[ii+6 ] , vertexs_count[ii+7 ] } ; 
												byte[] fourBytes3 = { vertexs_count[ii+8] , vertexs_count[ii+9] , vertexs_count[ii+10] , vertexs_count[ii+11] } ;

												float v1 = BitConverter.ToSingle(fourBytes1 , 0) ;
												float v2 = BitConverter.ToSingle(fourBytes2 , 0) ;
												float v3 = BitConverter.ToSingle(fourBytes3 , 0) ;
												
												// --------------------------------------------------------------------------------------------------------------

												using ( StreamWriter sw = new StreamWriter ( writePath , true , System.Text.Encoding.Default ) )
												{
//!!!!!!!!!!!!!!!!!!
														sw.WriteLine( "v " + v1 + " " + v2 + " " + v3 ) ; // ���������� ������ � ���� *.obj
												}

										} // for // ��� ���� ������
//*/
										files_name_counter++ ; // ����� "�������" - ��������� ������� ��������� ������� // ��� ����� ������ ����� ���� "������" ������

								} // if // ���� ����� , �� �������� � ����
//*
								vertexs_count.Clear() ; // �������� ������ , ����� �� ���������� ������� �������

								//�������������������������������������������������������������������������������������������������������������������������������
								// ���� Vn (�������)
								//�������������������������������������������������������������������������������������������������������������������������������

								if ( array1d[i+0] == 0x6E & array1d[i+1] == 0x6F & array1d[i+2]  == 0x72 & array1d[i+3]  == 0x6D &
										 array1d[i+4] == 0x61 & array1d[i+5] == 0x6C & array1d[i+6]  == 0x73 & array1d[i+7]  == 0x00 )
								{
										// ����� 16 ���� (�� ������ "���������") �������� ���������� �������� , ���������� ���
										byte[] n_count_four_bytes_int = { array1d[i+16+0] , array1d[i+16+1] , array1d[i+16+2] , array1d[i+16+3] } ; 

										int n_count = BitConverter.ToInt32 ( n_count_four_bytes_int , 0 ) ;

										for ( int ii = 0 ; ii < n_count*3*4 ; ii++ ) 
												normals_count.Add ( array1d[i+20+ii] ) ;

										// --------------------------------------------------------------------------------------------------------------


										using ( StreamWriter sw = new StreamWriter ( writePath , true , System.Text.Encoding.Default ) )
										{
//!!!!!!!!!!!!!!!!!!
												sw.WriteLine( "\n# {0} normals " , n_count ) ; // 
										}

										// --------------------------------------------------------------------------------------------------------------

										for ( int ii = 0 ; ii < normals_count.Count ; ii+=12 )
										{
												// float �������� 4 ����� � �� �� ��� ����������

												byte[] fourBytes1 = { normals_count[ii+0] , normals_count[ii+1] , normals_count[ii+2 ] , normals_count[ii+3 ] } ; 
												byte[] fourBytes2 = { normals_count[ii+4] , normals_count[ii+5] , normals_count[ii+6 ] , normals_count[ii+7 ] } ; 
												byte[] fourBytes3 = { normals_count[ii+8] , normals_count[ii+9] , normals_count[ii+10] , normals_count[ii+11] } ;

												float vn1 = BitConverter.ToSingle(fourBytes1 , 0) ;
												float vn2 = BitConverter.ToSingle(fourBytes2 , 0) ;
												float vn3 = BitConverter.ToSingle(fourBytes3 , 0) ;
												
												// --------------------------------------------------------------------------------------------------------------

												using ( StreamWriter sw = new StreamWriter ( writePath , true , System.Text.Encoding.Default ) )
												{
//!!!!!!!!!!!!!!!!!!
														sw.WriteLine( "vn " + vn1 + " " + vn2 + " " + vn3 ) ; 
												}
										}

								} // if // ���� ����� ������� , �� �������� � ����

								normals_count.Clear() ; // �������� ������ , ����� �� ���������� ������� �������
//*/
								//�������������������������������������������������������������������������������������������������������������������������������
								// ���� ����� FACES ( prims )
								//�������������������������������������������������������������������������������������������������������������������������������

								// ���� ����� ������ "prims..." = 70 72 69 6D 73 ( 00 00 00 )

								if ( array1d[i+0] == 0x70 & array1d[i+1] == 0x72 & array1d[i+2] == 0x69 & array1d[i+3] == 0x6D & array1d[i+4] == 0x73 )
								{
										int id = i + 24 ; // ��� ���� ����� ������
										int offset = 0  ; // 4 8 12 16 18 40

										// ������ ������ ����� // �� ���� �����
										byte[] f_size_four_bytes_int = { array1d[i+8+0] , array1d[i+8+1] , array1d[i+8+2] , array1d[i+8+3] } ; 
										int fb_size = BitConverter.ToInt32 ( f_size_four_bytes_int , 0 ) ;

										int fs_count = array1d[i+16] ; 	// ������ ���������� "���"-�����

										// --------------------------------------------------------------------------------------------------------------

										for ( int fi = 0 ; fi < fs_count ; fi++ )	// ��������� ����������� ���-�� ��� ������ ���-�� ���-�����
										{
												byte[] f_count_four_bytes_int = { // ������ ���������� ������� f � ����� obj // c 24 �� 27
												array1d[id+0+offset] ,	array1d[id+1+offset] ,	array1d[id+2+offset] ,	array1d[id+3+offset] } ;	
												int f_count = BitConverter.ToInt32 ( f_count_four_bytes_int , 0 ) ; // ���������� �����

												byte[] f_count_four_bytes_int_size = { // ������ ���������� ���� , ������� ����� �������� �� ��� // c 28 �� 31
												array1d[id+4+0+offset] ,	array1d[id+4+1+offset] ,	array1d[id+4+2+offset] ,	array1d[id+4+3+offset] } ;	
												int f_size_count = BitConverter.ToInt32 ( f_count_four_bytes_int_size , 0 ) ; // 

												//��� ����� ��������� ������� ����*2 ���� ������� // �������� ��� ����� 6 , ������ ����� ������� 6 ��� ���� [00 00]
												//Console.WriteLine("���������� ������ = " + f_count/3 + "\t" + "���������� ���� = "   + f_count*2 + "\n" ) ; 

												// --------------------------------------------------------------------------------------------------------------

												for ( int ii = 0 ; ii < f_size_count*2 ; ii++ ) 
												{		// �������� ��� ����� � ������
														primsss_count.Add ( array1d[id + 8 + ii + offset] ) ; 
												}

												// --------------------------------------------------------------------------------------------------------------

												// ������ "�����������" � ����� �� ����� ����� ����� ������ 
												offset = offset + 8 + f_size_count*2 + 44 ; 
												if ( f_count % 2 != 0 ) offset = offset + 2 ;

												// --------------------------------------------------------------------------------------------------------------

												using ( StreamWriter sw = new StreamWriter ( writePath , true , System.Text.Encoding.Default ) )
												{
//!!!!!!!!!!!!!!!!!!
														sw.WriteLine( "\ng faces{0}" , fi ) ; 
														sw.WriteLine( "# {0} faces " , f_count ) ; 
												}

												// --------------------------------------------------------------------------------------------------------------

												for ( int iii = 0 ; iii < primsss_count.Count ; iii+=6 )
												{
														byte[] twoBytes1 = { primsss_count[iii+0] , primsss_count[iii+1] } ; // 00 00 // 03 00 
														byte[] twoBytes2 = { primsss_count[iii+2] , primsss_count[iii+3] } ; // 01 00 // 02 00
														byte[] twoBytes3 = { primsss_count[iii+4] , primsss_count[iii+5] } ; // 02 00 // 01 00

														UInt16 f1 = (UInt16)BitConverter.ToUInt16(twoBytes1 , 0) ; // 0
														UInt16 f2 = (UInt16)BitConverter.ToUInt16(twoBytes2 , 0) ; // 1
														UInt16 f3 = (UInt16)BitConverter.ToUInt16(twoBytes3 , 0) ; // 2

														// --------------------------------------------------------------------------------------------------------------

														using ( StreamWriter sw = new StreamWriter ( writePath , true , System.Text.Encoding.Default ) )
														{
//!!!!!!!!!!!!!!!!!!
																sw.WriteLine( "f " + (f1+1) + " " + (f2+1) + " " + (f3+1) ) ; 
														}
												}

												primsss_count.Clear() ; // �������� ������ , ����� �� ���������� ������� �������
										}

										primsss_count.Clear() ; // �������� ������ , ����� �� ���������� ������� �������

										// --------------------------------------------------------------------------------------------------------------

								}	// if // ���� ����� ����� , �� �������� � ����

								primsss_count.Clear() ; // �������� ������ , ����� �� ���������� ������� �������

							// ��������������������������������������������������������������������������������������������������������������������������������

						} // for // ������ �� ����� ����������� ������� ���� ����������� �� �����

		 		} // foreach // ������ �� ������� �����

		} // Main

} // class

// ��������������������������������������������������������������������������������������������������������������������������������
