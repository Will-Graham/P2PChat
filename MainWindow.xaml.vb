' This project uses components from Ellen Ramcke's "TCP Communication in VB.NET"
' The original files are availiable from https://code.msdn.microsoft.com/TCP-Communication-in-VBNET-f6c48ca0/

Imports System.Environment
Imports System.Net
Imports System.IO
Imports System.Net.Sockets
Imports System.Threading
Imports System.Text
Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports Microsoft.Win32

Class MainWindow
    Dim hostname As String = Dns.GetHostName()
    Public hostAddress As System.Net.IPAddress = CType(Dns.GetHostByName(hostname).AddressList.GetValue(0), IPAddress) '.ToString
    Private WithEvents myChat As New TCPChat
    Private WithEvents FileNameRecieved As New TCPChat
    Private WithEvents fileLenReceiver As New TCPChat
    Private WithEvents fileExtReceiver As New TCPChat
    'Private myAdapterName, myPhysicalAddress, myGateway, myDNS, strHostName As String
    Private addr() As IPAddress
    Public remPort As Integer = 5000
    Public myPort As Integer = 5000
    Dim connected As Boolean = False
    Dim timeconnect As String
    Dim User As String = UserName
    Public remIP As String
    Dim remFilePort As Integer = remPort + 500
    Dim myFilePort As Integer = remPort + 500

    Private Sub MainWindow_load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Loaded
        txtMyIP.Text = (hostAddress).ToString
        Call TextLoader()
        StoreIP.setMyPort(myPort)
        'Dim Ts As ThreadStart = New ThreadStart(AddressOf StartReceiving)
        'T = New Thread(Ts)
        'T.Start()
        'ReceiveTCP(myFilePort)

        Dim IPHost As IPHostEntry
        IPHost = Dns.GetHostByName(Dns.GetHostName())

        IPHost.AddressList(0).ToString()
        nSockets = New ArrayList()
        Dim thdListener As New Thread(New ThreadStart(AddressOf listenerThread))
        thdListener.Start()
    End Sub
    Private Sub TextLoader()
        Try
            Dim sr As New StreamReader(My.Application.Info.DirectoryPath & "\textsave.txt")
            txtNotes.Text = sr.ReadLine()
            While Not sr.EndOfStream
                txtNotes.Text = txtNotes.Text & vbCrLf & sr.ReadLine()
            End While
            sr.Close()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub
    'Private Sub MainWindow_Close(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Closed
    '    Dim sw As New StreamWriter(My.Application.Info.DirectoryPath & "\textsave.txt")
    '    sw.WriteLine(txtNotes.Text)
    '    sw.Close()
    'End Sub

    Private Sub CmdSend_Click(sender As Object, e As RoutedEventArgs) Handles CmdSend.Click
        ' Driver for debug purposes

        ''   Dim b As New BaseEncrypt
        ''   b.Show()
        ''   b.Encryptor(TxtSend.Text)
        If Not TxtSend.Text = "" Then
            Dim sent = Encryptor(User + ": " + TxtSend.Text)
            If sent Then
                TxtDisplay.Text = TxtDisplay.Text + vbCrLf + "Me: " + TxtSend.Text
            Else
                TxtDisplay.Text = TxtDisplay.Text + vbCrLf + "Message failed to send"
            End If
        Else
            MsgBox("Please Enter a Message", vbExclamation, "Missing Text")
        End If

    End Sub
    Public Function Encryptor(strOriginal As String)
        Dim byt As Byte() = System.Text.Encoding.UTF8.GetBytes(strOriginal)
        Dim strModified As String

        ' convert the byte array to a Base64 string


        strModified = Convert.ToBase64String(byt)
        Dim sent As Boolean = txtOutSend(strModified)
        If sent Then
            Return True
        Else
            Return False
        End If
    End Function
    Private Sub MainWindow_Closing() Handles MyBase.Closing
        If connected Then
            myChat.disconnect()
        End If
        Dim sw As New StreamWriter(My.Application.Info.DirectoryPath & "\textsave.txt")
        sw.WriteLine(txtNotes.Text)
        sw.Close()
        Dim ExitMsg = MsgBox("Do you wish to save your messages?", MsgBoxStyle.YesNo, "Save Messages")
        If ExitMsg = 6 Then
            If Not Directory.Exists(My.Application.Info.DirectoryPath + "\Conversations") Then
                Directory.CreateDirectory(My.Application.Info.DirectoryPath & "\Conversations")
            End If
            Dim MessageWriter As New StreamWriter(My.Application.Info.DirectoryPath & "\Conversations\Conversation_" + timeconnect + ".txt")
            MessageWriter.WriteLine(txtNotes.Text)
            MessageWriter.Close()
            MsgBox("Saved as " + My.Application.Info.DirectoryPath & "\Conversations\Conversation_" + timeconnect + ".txt")
        End If
        End
    End Sub
    '-

    '  input from Textbox
    '
    'Private Sub TxtSendKD(ByVal sender As Object, ByVal e As KeyEventArgs) Handles TxtSend.KeyDown
    '    If e.IsDown = Key.Enter Then
    '        With CType(sender, TextBox)
    '            If .Text.Length > 0 Then
    '                StatusLabel_send.Image = My.Resources.ledCornerGray
    '                myChat.SendData(.Text, txtIP.Text, CInt(remPort))
    '                txtOutSend(.Text)
    '                .Text = String.Empty
    '            End If
    '        End With
    '    End If
    'End Sub
    '
    ' output to listbox
    '
    Public Sub txtOut(ByVal txt As String) Handles myChat.Datareceived
        Dim strOriginal As String
        Dim b As Byte() = Convert.FromBase64String(txt)
        strOriginal = Encoding.UTF8.GetString(b)
        TxtDisplay.Text = TxtDisplay.Text + vbCrLf + strOriginal
    End Sub

    Dim filesize As Integer
    Dim acceptfile
    Public Sub fileNameReceiver(ByVal txt As String) Handles FileNameRecieved.Datareceived
        acceptfile = MsgBox("Do you wish to recieve this file: " + txt, vbQuestion + vbYesNo, "Accept File?")
        If acceptfile = vbYes Then
            filename = txt
            '  MsgBox("a2 " + filename) 'Debug Tools
        End If
    End Sub
    Public Sub filesizereceiver(ByVal txt As String) Handles fileLenReceiver.Datareceived
        If acceptfile = vbYes Then
            filesize = Val(txt)

            '  MsgBox("b2 " + Str(filesize))'Debug Tools
        End If
    End Sub
    Dim fileExt As String
    Public Sub fileExtreceive(ByVal txt As String) Handles fileExtReceiver.Datareceived
        If acceptfile = vbYes Then
            fileExt = txt
            ' MsgBox(fileExt & vbCrLf & txt)
            '  MsgBox("b2 " + Str(filesize))'Debug Tools
        End If
    End Sub
    Private Function txtOutSend(ByVal txt As String)
        Dim sent As Boolean = myChat.SendData(txt, txtIP.Text, CInt(remPort))
        If sent Then
            Return True
        Else
            Return False
        End If
    End Function
    '
    '  senda data OK NOK
    '
    ' Private Sub sendata(ByVal sendStatus As Boolean) Handles myChat.sendOK
    ' If sendStatus Then
    '  StatusLabel_send.Image = My.Resources.ledCornerGreen
    ' Else
    '      StatusLabel_send.Image = My.Resources.ledCornerRed
    '   End If
    ' End Sub
    '
    ' receive data OK NOK

    '
    ' connect
    '
    Private Sub CMDIPCONN_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdIPCONN.Click

        connected = myChat.connect(txtMyIP.Text, CInt(myPort))
        FileNameRecieved.connect(txtMyIP.Text, CInt(myFilePort))
        fileLenReceiver.connect(txtMyIP.Text, CInt(myFilePort + 500))
        fileExtReceiver.connect(txtMyIP.Text, CInt(myFilePort + 1000))
        TextLoader()
        timeconnect = DateAndTime.Now.ToString("dd.MM.yy HH-MM")
        '   MsgBox(timeconnect)
        If connected = True Then
            Dim usertemp = InputBox("What is your name?", "Enter Name", User)
            User = usertemp
            TxtDisplay.Text = TxtDisplay.Text + vbCrLf + "System: Listening port connected"
            TxtSend.IsEnabled = True
            CmdSend.IsEnabled = True
            cmdDisconnect.IsEnabled = True
            cmdIPCONN.IsEnabled = False
            cmdSelectPort.IsEnabled = False
        End If
        remIP = txtIP.Text
        StoreIP.setRemIP(remIP)
        StoreIP.setRemPort(remFilePort)
        StoreIP.setMyIP((hostAddress).ToString())
        StoreIP.setMyPort(myFilePort)

        cmdFileSend.IsEnabled = True
    End Sub


    '
    ' connection status
    '
    Private Sub connection(ByVal status As Boolean) Handles myChat.connection
        If status Then
            txtIP.IsEnabled = False
            '  tbx_remotePort.Enabled = False
            ' txtMyIP.Enabled = False
            '  tbx_hostPort.Enabled = False
            'StatusLabel_adapter.Image = My.Resources.ledCornerGreen
            'StatusLabel_receive.Image = My.Resources.ledCornerOrange
        Else
            txtIP.IsEnabled = True
            '  tbx_remotePort.Enabled = True
            '  tbx_hostIP.Enabled = True
            ' tbx_hostPort.Enabled = True
            'StatusLabel_adapter.Image = My.Resources.ledCornerGray
            'StatusLabel_receive.Image = My.Resources.ledCornerGray
            'StatusLabel_send.Image = My.Resources.ledCornerGray
        End If
    End Sub
    '
    '  disconnect socket
    '
    Private Sub btnDisconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdDisconnect.Click
        myChat.disconnect()
        fileLenReceiver.disconnect()
        FileNameRecieved.disconnect()
        fileExtReceiver.disconnect()
        cmdFileSend.IsEnabled = False
        cmdSelectPort.IsEnabled = True
        FileNameRecieved.disconnect()
        cmdIPCONN.IsEnabled = True
        cmdSelectPort.IsEnabled = False
    End Sub
    '
    '  clear listbox
    '
    Private Sub btn_clear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClear.Click
        TxtDisplay.Text = ""
    End Sub

    'Private Sub cmdIPCONN_Click(sender As Object, e As RoutedEventArgs) Handles cmdIPCONN.Click
    '    myChat.connect(txtMyIP.Text, CInt(myPort))
    'End Sub




    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim b As New Window1
        b.Show()
    End Sub
    Private nSockets As ArrayList
    Public Sub listenerThread()
        Dim tcpListener As New TcpListener(8080)
        Dim handlerSocket As Socket
        Dim thdstHandler As ThreadStart
        Dim thdHandler As Thread
        tcpListener.Start()
        Do
            handlerSocket = tcpListener.AcceptSocket()
            If handlerSocket.Connected Then
                SyncLock (Me)
                    nSockets.Add(handlerSocket)
                End SyncLock
                thdstHandler = New ThreadStart(AddressOf _
                handlerThread)
                thdHandler = New Thread(thdstHandler)
                thdHandler.Start()
            End If
        Loop
    End Sub
    Dim filename As String = Nothing
    Public Sub handlerThread()
        If acceptfile = vbYes Then
            Dim receivingFilePath As String
            Dim handlerSocket As Socket = nSockets(nSockets.Count - 1)
            Dim networkStream As NetworkStream = New NetworkStream(handlerSocket)
            Dim blockSize As Int16 = 1024
            Dim thisRead As Int16
            Dim dataByte(blockSize) As Byte
            Dim Dlg As SaveFileDialog = New SaveFileDialog()

            Dlg.Filter = "Original |*" + fileExt + "|All Files (*.*)|*.*"
            Dlg.CheckFileExists = False
            Dlg.Title = "Where would you like to save?"
            Dlg.InitialDirectory = "C:\Users\" + UserName + "\Desktop"
            Dlg.FileName = filename
            Dlg.DefaultExt = fileExt
            If Dlg.ShowDialog() = True Then
                receivingFilePath = Dlg.FileName
                Dim a = File.Create(receivingFilePath)
                a.Close()
            End If
            If receivingFilePath <> Nothing Then
                Try
                    SyncLock Me
                        ' Only one process can access the
                        ' same file at any given time
                        Dim fileStream As Stream = File.OpenWrite(receivingFilePath)
                        While FileLen(receivingFilePath) < filesize
                            While (True)
                                thisRead = networkStream.Read(dataByte, 0, dataByte.Length)
                                fileStream.Write(dataByte, 0, dataByte.Length)
                                If thisRead = 0 Then Exit While
                            End While
                        End While
                        fileStream.Close()
                    End SyncLock
                Catch ex As Exception
                    MsgBox("Error: " & ex.Message)
                End Try


            End If

            handlerSocket = Nothing
        End If
    End Sub
    Public WithEvents portselector As New Window2
    Private Sub cmdSelectPort_Click(sender As Object, e As RoutedEventArgs) Handles cmdSelectPort.Click
        portselector.Show()
    End Sub
    Shared Event fileportset()
    Private Sub portsetter(ByVal setmyport As Integer, ByVal setremport As Integer) Handles portselector.PortSet
        myPort = setmyport
        myFilePort = myPort + 500
        remPort = setremport
        remFilePort = remPort + 500
        StoreIP.setMyPort(myFilePort)
        StoreIP.setRemPort(remFilePort)
        RaiseEvent fileportset()
    End Sub
End Class

Public Class TCPChat
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
    ''' TCP connect with server
    ''' </summary>
    Public Function connect(ByVal hostAdress As String, ByVal hostPort As Integer)

        sc = SynchronizationContext.Current

        Try
            server = New TcpListener(IPAddress.Parse(hostAdress), hostPort)
        Catch ex As Exception
            MsgBox("server create: " & ex.Message, MsgBoxStyle.Exclamation)
        End Try

        Try
            With server
                .Start()
                .BeginAcceptTcpClient(New AsyncCallback(AddressOf DoAccept), server)
                isConnected = True
            End With
        Catch ex As Exception
            MsgBox("server listen: " & ex.Message, MsgBoxStyle.Exclamation)
            isConnected = False
        Finally
            RaiseEvent connection(isConnected)
        End Try
        Return isConnected
    End Function
    ''' <summary>
    ''' disConnect server
    ''' </summary>
    Public Sub disconnect()
        Try
            isConnected = False
            server.Stop()
        Catch ex As Exception
            MsgBox("disConnect server: " & ex.Message, MsgBoxStyle.Exclamation)
            isConnected = True
        Finally
            RaiseEvent connection(isConnected)
        End Try
    End Sub
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
    ''' <summary>
    ''' TCP asynchronous receive on secondary thread
    ''' last update 10.5.2011
    ''' </summary>
    Private Sub DoAccept(ByVal ar As IAsyncResult)

        Dim sb As New StringBuilder
        Dim buf() As Byte
        Dim datalen As Integer

        Dim listener As TcpListener
        Dim clientSocket As TcpClient
        If Not isConnected Then Exit Sub
        Try
            listener = CType(ar.AsyncState, TcpListener)
            clientSocket = listener.EndAcceptTcpClient(ar)
            clientSocket.ReceiveTimeout = 5000
            'update 10.5.2011
            iRemote = clientSocket.Client.RemoteEndPoint
            pLocal = clientSocket.Client.LocalEndPoint

        Catch ex As ObjectDisposedException
            MsgBox("DoAccept ObjectDisposedException " & ex.Message, MsgBoxStyle.Exclamation)
            ' after server.stop() AsyncCallback is also active, but the object server is disposed
            Exit Sub
        End Try

        Try
            With clientSocket
                datalen = 0
                ' somtimes it occurs that .available returns the value 0 also data in buffer exists
                While datalen = 0
                    ' data in read Buffer
                    datalen = .Available
                End While
                buf = New Byte(datalen - 1) {}
                'get entire bytes at once
                .GetStream().Read(buf, 0, buf.Length)
                sb.Append(Encoding.ASCII.GetString(buf, 0, buf.Length))
                .Close()
            End With
            receiveStatus = True
        Catch ex As TimeoutException
            MsgBox("doAcceptData timeout: " & ex.Message, MsgBoxStyle.Exclamation)
            receiveStatus = False
            clientSocket.Close()
            Exit Sub
        Catch ex As Exception
            MsgBox("doAcceptData: " & ex.Message, MsgBoxStyle.Exclamation)
            receiveStatus = False
            clientSocket.Close()
            Exit Sub
        Finally
            RaiseEvent recOK(receiveStatus)
        End Try
        ' post data
        sc.Post(New SendOrPostCallback(AddressOf OnDatareceived), sb.ToString)
        ' start new read
        server.BeginAcceptTcpClient(New AsyncCallback(AddressOf DoAccept), server)
    End Sub
    '
    ' now data to calling class and UI thread
    '
    Private Sub OnDatareceived(ByVal state As Object)
        RaiseEvent Datareceived(state.ToString)
    End Sub

End Class