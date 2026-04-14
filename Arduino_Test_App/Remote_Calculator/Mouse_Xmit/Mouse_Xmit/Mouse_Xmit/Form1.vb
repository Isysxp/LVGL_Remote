Imports System.Net.Sockets

Public Class Form1

    Dim udpClient As New UdpClient("calculator", 1000)
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub PictureBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove
        Dim dtr(2) As Integer

        dtr(0) = e.X
        dtr(1) = e.Y
        If (e.Button = MouseButtons.Left) Then
            dtr(0) = -e.X
        End If
        Dim xmitBytes As New List(Of Byte)
        For Each i As Integer In dtr
            Dim b() As Byte = BitConverter.GetBytes(i)
            xmitBytes.AddRange(b)
        Next

        Dim xmitBuffer() As Byte = xmitBytes.ToArray
        udpClient.Send(xmitBuffer, 8)

    End Sub

    Private Sub PictureBox1_MouseUp(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseUp
        PictureBox1_MouseMove(sender, e)
    End Sub

    Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown
        PictureBox1_MouseMove(sender, e)
    End Sub
End Class
