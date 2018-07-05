Public Class Window2
    Public Event PortSet(ByVal myport As Integer, ByVal remport As Integer)

    ' raises event defining ports
    Public Sub cmdSetPort_Click(sender As Object, e As RoutedEventArgs) Handles cmdSetPort.Click
        RaiseEvent PortSet(CInt(txtMyPort.Text), CInt(txtRemPort.Text))
        Me.Hide()
    End Sub
End Class
