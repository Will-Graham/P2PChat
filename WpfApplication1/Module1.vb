Module StoreIP
    Dim remIP As String
    Dim remPort As Integer
    Dim myIP As String
    Dim myPort As Integer
    Public Sub setRemIP(ip As String)
        remIP = ip
    End Sub
    Public Function getRemIP()
        Return remIP
    End Function
    Public Sub setRemPort(port As Integer)
        remPort = port
    End Sub
    Public Function getRemPort()
        Return remPort
    End Function
    Public Sub setMyIP(ip As String)
        myIP = ip
    End Sub
    Public Function getMyIP()
        Return myIP
    End Function
    Public Sub setMyPort(port As Integer)
        myPort = port
    End Sub
    Public Function getMyPort()
        Return myPort
    End Function
End Module
