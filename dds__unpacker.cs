using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

sealed class big2obj
{
		static void Main()
		{
				string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.big", SearchOption.AllDirectories) ; 

				foreach ( var file in files ) // ��� ������ ������ , ���������� ��� �����.big , ������������ � ������� ����� 
				{
						int dds_counter = 0 ; // ������� ��������� ������� � ����� , ������������ � �������� ��������� � ����� �������� 

						byte[] array1d = File.ReadAllBytes(file) ; // ������ ���� ���� � ������ ����

						for ( int i = 0 ; i < array1d.Length - 42 ; i++ ) // ����� ����� ����� , �������� ������ ����
						{
							//���� ����� "���������" texture � �������� �� ��� "���������" symlist , �� ��� ����� dds-���� 
							//if ( array1d[i+0] == 0x44 & array1d[i+1] == 0x44 & array1d[i+2] == 0x53 & array1d[i+3] == 0x20 & array1d[i+4] == 0x7C ) // ����� DDS 20 7C // �� ������ ��������
								if ( array1d[i+0] == 0x74 & array1d[i+1] == 0x65 & array1d[i+2] == 0x78 & array1d[i+3] == 0x74 & // text
										 array1d[i+4] == 0x75 & array1d[i+5] == 0x72 & array1d[i+6] == 0x65 & array1d[i+7] == 0x00 & // ure?
										 array1d[i+20] == 0x73 & array1d[i+21] == 0x79 & array1d[i+22] == 0x6D & array1d[i+23] == 0x6C & // syml
										 array1d[i+24] == 0x69 & array1d[i+25] == 0x73 & array1d[i+26] == 0x74 & array1d[i+27] == 0x00 ) // ist?
								{
										byte[] ddsSize_bytes = { array1d[i+40+0] ,	array1d[i+40+1] ,	array1d[i+40+2] ,	array1d[i+40+3] } ;	// ��������� ����� �������� ������ dds ����� 
										int ddsSize = BitConverter.ToInt32 ( ddsSize_bytes , 0 ) ; // = ������ dds ����� � ������ � ������� ���� // ������ ����� ����� long

										byte[] ddsArray = array1d.Skip(i+44).Take(ddsSize).Skip(0).Take(array1d.Length).SkipWhile(x =>(x==0xFF)).ToArray(); // ���� ���� - ����� :D

										string ddsWritePath = file + dds_counter + ".dds" ; // ������ ��� dds ����� 
																// � ���� filename.big1.dds

										File.WriteAllBytes( ddsWritePath , ddsArray ) ; // ���������� ��� ����� �� ������� � ����

										dds_counter++ ; // ����������� ������� ����� �����

								} // if 

						} // for 

		 		} // foreach 

		} // Main

} // class
