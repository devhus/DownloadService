   M S B A M L   `   `   `  ���� X  TDevhus.DownloadService.Sample, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullM+clr-namespace:Devhus.DownloadService.SampleDevhus.DownloadService.Sample  .    (Devhus.DownloadService.Sample.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     =local+clr-namespace:Devhus.DownloadService.Sample     -     � Title$  
MainWindow��$	��520��$	��720��$	��520��$	��720�� � WindowStartupLocation$ CenterScreen=�.��� .+�� $��24q�.+��� .+��� $��0,0,0,8q�.+��� .��
File Url�� -    ��Name$ inputFileUrl���� $��0,0,0,8q�.+��� .��File Path (Not Folder)�  � ColumnDefinitions �� $��1* ��� $	��100 �.+��� -   $ inputFilePath��$��0��$
��Left=�$	��564���� -    ��Name$ btnFileBrowse��$��Browse��$��1��$��8,0,0,0q��� -   $ 
btnAddFile��$��Add File��$	��0,8q�]� $��1�����	#FF858585$��0,0,0,8q�&   System.Windows.Controls.DataGrid  -     Name$ downloadFileList��   AutoGenerateColumns .     
IsReadOnly .    CanUserAddRows .  $��
0,152,0,40q�	   Columns	 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn
   Width5  /System.Windows.Controls.DataGridLengthConverter$
 *    Header$
 Name��1  +System.Windows.Controls.DataGridBoundColumn   Binding ��  �� Path$ MyFile.Name�  $	
 100 $
 Size�� �� $ MyFile.RemoteSizeText�  $	
 100 $ Status��   HeaderStringFormat$  �� �� $ State�  $
 60 $ Chunks��$  �� �� $ 
ChunkParts�� .+���  �� Orientation$ 
Horizontal=�$��0,8,0,0q�$��Bottom=�$
��Left=�.+��� -   $ btnDownlodStartStop��$��Start��$	��8,4q�$��0,0,8,0q��� -   $ btnDownlodPauseResume��$��Pause��$	��8,4q�$��0,0,8,0q��� $ 
Horizontal=�$��0,8,0,0q�$��Bottom=�$��Right=�.+��� -	   $ btnDownloaderOptions��$��Options��$	��8,4q�$��8,0,0,0q�