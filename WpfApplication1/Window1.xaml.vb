Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text

Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports Microsoft.Win32



Public Class Window1
    ' Class FileSender


    Public SendingFilePath As String = String.Empty

    Private Const BufferSize As Integer = 1024
    Dim remIP As String = StoreIP.getRemIP
    Dim remport As Integer = StoreIP.getRemPort
    'Public Sub New()
    '    InitializeComponent()
    'End Sub

    'Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs)
    '    progressBar1.Visible = True
    '    progressBar1.Minimum = 1
    '    progressBar1.Value = 1
    '    progressBar1.[Step] = 1
    'End Sub


    Private Sub onload() Handles Me.Loaded
        Console.WriteLine(remIP)
    End Sub

    'Public Sub SendTCP(ByVal M As String, ByVal IPA As String, ByVal PortN As Int32)
    '    Dim SendingBuffer As Byte() = Nothing
    '    Dim client As TcpClient = Nothing
    '    '  lblStatus.Text = ""
    '    Dim netstream As NetworkStream = Nothing
    '    Try
    '        client = New TcpClient(IPA, PortN)
    '        '          lblStatus.Text = "Connected to the Server..." & vbLf
    '        netstream = client.GetStream()
    '        Dim Fs As FileStream = New FileStream(M, FileMode.Open, FileAccess.Read)
    '        Dim NoOfPackets As Integer = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Fs.Length) / Convert.ToDouble(BufferSize)))
    '        '     progressBar1.Maximum = NoOfPackets
    '        Dim CurrentPacketLength As Integer, TotalLength As Integer = CInt(Fs.Length), counter As Integer = 0
    '        For i As Integer = 0 To NoOfPackets - 1
    '            If TotalLength > BufferSize Then
    '                CurrentPacketLength = BufferSize
    '                TotalLength = TotalLength - CurrentPacketLength
    '            Else
    '                CurrentPacketLength = TotalLength
    '            End If

    '            SendingBuffer = New Byte(CurrentPacketLength - 1) {}
    '            Fs.Read(SendingBuffer, 0, CurrentPacketLength)
    '            netstream.Write(SendingBuffer, 0, CInt(SendingBuffer.Length))
    '            '  If progressBar1.Value >= progressBar1.Maximum Then progressBar1.Value = progressBar1.Minimum
    '            '  progressBar1.PerformStep()
    '        Next

    '        ' lblStatus.Text = lblStatus.Text & "Sent " & Fs.Length.ToString() & " bytes to the server"
    '        Fs.Close()
    '    Catch ex As Exception
    '        Console.WriteLine(ex.Message)
    '    Finally
    '        '  netstream.Close()
    '        'client.Close()
    '    End Try
    'End Sub
    ''End Class
    Dim bytearray As Array
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        If SendingFilePath <> Nothing Then
            Dim filebuffer As Byte()
            Dim fileStream As Stream

            Try
                fileStream = File.OpenRead(SendingFilePath)
                ' Alocate memory space for the file
                ReDim filebuffer(fileStream.Length)
                fileStream.Read(filebuffer, 0, fileStream.Length)
                ' Open a TCP/IP Connection and send the data
                Dim clientSocket As New TcpClient(remIP, 8080)
                Dim networkStream As NetworkStream
                networkStream = clientSocket.GetStream()
                networkStream.WriteTimeout = Val(9999999)
                networkStream.Write(filebuffer, 0, fileStream.Length)
                networkStream.Close()
            Catch ex As Exception
                MsgBox("Error: " & ex.Message)
            End Try

        End If
    End Sub


    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        Dim Dlg As OpenFileDialog = New OpenFileDialog()
        Dlg.Filter = "All Files (*.*)|*.*"
        Dlg.CheckFileExists = True
        Dlg.Title = "Choose a File"
        Dlg.InitialDirectory = "C:\"
        If Dlg.ShowDialog() = True Then
            SendingFilePath = Dlg.FileName
        End If
    End Sub
End Class
