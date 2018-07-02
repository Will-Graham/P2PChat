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
Imports System.Threading
Imports System.Environment

Public Class Window1
    ' Class FileSender


    Public SendingFilePath As String = String.Empty

    Private Const BufferSize As Integer = 1024
    Dim remIP As String = StoreIP.getRemIP
    Dim remport As Integer = StoreIP.getRemPort
    Private WithEvents sendfilename As New FileNameSender
    Dim filename As String
    Dim filelength As Long

    ' For Debugging Purposes
    'Private Sub onload() Handles Me.Loaded
    '    Console.WriteLine(remIP)
    'End Sub


    Dim bytearray As Array

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        If SendingFilePath <> Nothing Then

            sendfilename.SendData(filename, remIP, CInt(remport))
                MsgBox("a " + filename)

            Call sendfilelen()


        End If
    End Sub

    Private Sub sendFileLen()

        sendfilename.SendData(filelength, remIP, CInt(remport + 500))
        MsgBox("b " + Str(filelength))

        Call sendfileExt()

    End Sub
    Private Sub sendfileExt()
        sendfilename.SendData(fileExtension, remIP, CInt(remport + 1000))
        MsgBox("c " + fileExtension)

        Call filesender()
    End Sub
    Private Sub filesender()
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
                MsgBox("Error: " & ex.Message & vbCrLf & "File Did not send successfully!", vbExclamation)
                Exit Sub
            End Try
            MsgBox("File Successfully Sent!")
            SendingFilePath = Nothing
            tbkFileName.Text = "File Selected: None"
        End If
    End Sub
    Dim fileExtension As String
    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        Dim Dlg As OpenFileDialog = New OpenFileDialog()
        Dlg.Filter = "All Files (*.*)|*.*"
        Dlg.CheckFileExists = True
        Dlg.Title = "Choose a File"
        Dlg.InitialDirectory = "C:\Users\" + UserName + "\Desktop"
        If Dlg.ShowDialog() = True Then
            SendingFilePath = Dlg.FileName
            filename = Path.GetFileNameWithoutExtension(SendingFilePath)
            filelength = FileLen(SendingFilePath)
            fileExtension = Path.GetExtension(SendingFilePath)
            tbkFileName.Text = "File Selected: " + SendingFilePath
        End If

    End Sub

End Class
Public Class FileNameSender
    ''' <summary>
    ''' Event data send back to calling form
    ''' </summary>
    Public Event Datareceived(ByVal txt As String)
    ''' <summary>
    ''' connection status back to form True: ok
    ''' </summary>
    Public Event connection(ByVal cStatus As Boolean)
    ''' <summary>
    ''' data send successfull (True)
    ''' </summary>
    Public Event sendOK(ByVal sStatus As Boolean)
    ''' <summary>
    ''' data receive successfull (True)
    ''' </summary>
    Public Event recOK(ByVal sReceive As Boolean)

    Private serverRuns As Boolean
    Private server As TcpListener
    Private sc As SynchronizationContext
    Private isConnected, receiveStatus, sendStatus As Boolean
    Private iRemote, pLocal As EndPoint

    ''' <summary>
    ''' reads endpoints
    ''' </summary>
    Public ReadOnly Property Remote() As EndPoint
        Get
            Return iRemote
        End Get
    End Property
    ''' <summary>
    ''' reads local point
    ''' </summary>
    Public ReadOnly Property Local() As EndPoint
        Get
            Return pLocal
        End Get
    End Property
    ''' <summary>
    ''' TCP send data
    ''' </summary>
    Public Function SendData(ByVal txt As String, ByVal remoteAddress As String, ByVal remotePort As Integer)

        Dim clientSocket = New TcpClient
        Dim iP As IPAddress = IPAddress.Any
        Dim isIp As Boolean = IPAddress.TryParse(remoteAddress, iP)

        With clientSocket
            Try

                If isIp Then    ' ip address
                    .Connect(IPAddress.Parse(remoteAddress), remotePort)
                Else            ' DNS name
                    .Connect(remoteAddress, remotePort)
                End If

                Dim data() As Byte = Encoding.ASCII.GetBytes(txt)
                .NoDelay = True
                .GetStream().Write(data, 0, data.Length)
                .GetStream().Close()

                .Close()
                sendStatus = True

            Catch ex As Exception
                MsgBox("sendData: " & ex.Message, MsgBoxStyle.Exclamation)
                sendStatus = False
            Finally
                RaiseEvent sendOK(sendStatus)
            End Try
            Return sendStatus
        End With
    End Function

End Class
