Imports System.Buffers.Binary
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Net
Imports System.Net.Mime.MediaTypeNames
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Text

Public Class Form1

    <System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")>
    Private Shared Function DeleteObject(ByVal hObject As IntPtr) As Boolean
    End Function

    Dim udpSend As UdpClient
    Dim udpRec As UdpClient
    Dim remoteEndPoint As IPEndPoint
    Dim Init() As Byte = {2, 0, 0, 0, 0}
    Dim Disconnect() As Byte = {3, 0, 0, 0, 0}
    Dim Bmp As Bitmap
    Dim Bcopy As Bitmap
    Dim PixelCount As Integer = 0
    Dim Ndx As Integer = 0
    Dim NColor As UShort = 0
    Dim NRun As Byte = 0
    Dim BColor As Color


    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Public Structure DataHeader
        Public Control As UShort   ' 2 byte
        Public Width As UShort     ' 2 bytes
        Public Height As UShort    ' 2 bytes
        Public Extra As UInt32     ' 4 bytes
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Public Structure RLEPacket
        Public Control As UShort   ' 2 byte
        Public X As UShort         ' 2 byte
        Public Y As UShort         ' 2 byte
        Public Width As UShort     ' 2 bytes
        Public Height As UShort    ' 2 bytes
        Public Progress As UInt32  ' 4 bytes
    End Structure

    Dim Header As DataHeader
    Dim Rle As RLEPacket
    Dim XNdx As Integer = 0

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Bmp = New Bitmap(PictureBox1.Width, PictureBox1.Height, Imaging.PixelFormat.Format32bppArgb)
        Dim remoteIP As IPAddress = Dns.GetHostEntry("calculator").AddressList(0)
        remoteEndPoint = New IPEndPoint(remoteIP, 2400)
        udpSend = New UdpClient("calculator", 2400)
        udpRec = New UdpClient(2400)
        udpSend.Send(Disconnect, 5)
        While udpRec.Available > 0
            Dim remoteEP As New IPEndPoint(IPAddress.Any, 0)
            udpRec.Receive(remoteEP) ' Discard packet
        End While
        udpRec.Client.ReceiveTimeout = 1000
        udpRec.BeginReceive(AddressOf ReceiveCallback, Nothing)
        udpSend.Send(Init, 5)
    End Sub

    Function RGB565ToARGB32(rgb565 As UShort) As Color
        ' Extract RGB components from RGB565
        Dim r5 As Integer = (rgb565 >> 11) And &H1F   ' 5 bits red
        Dim g6 As Integer = (rgb565 >> 5) And &H3F    ' 6 bits green
        Dim b5 As Integer = rgb565 And &H1F           ' 5 bits blue

        ' Scale to 8-bit per channel
        Dim r8 As Integer = (r5 * 255) \ 31
        Dim g8 As Integer = (g6 * 255) \ 63
        Dim b8 As Integer = (b5 * 255) \ 31

        ' Return 32 bit ARGB color with (alpha = 255)
        Return Color.FromArgb(255, r8, g8, b8)
    End Function

    Private Sub ReceiveCallback(ar As IAsyncResult)
        Dim data As Byte() = udpRec.EndReceive(ar, remoteEndPoint)

        Header.Control = BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(0))
        If data.Length = 12 Then
            Header.Width = BitConverter.ToInt16(data, 2)
            Header.Height = BitConverter.ToInt16(data, 4)
            Header.Extra = BitConverter.ToInt32(data, 6)
            PixelCount = Header.Extra
        End If
        If Header.Control = 2 Then
            Rle.X = BitConverter.ToInt16(data, 2)
            Rle.Y = BitConverter.ToInt16(data, 4)
            Rle.Width = BitConverter.ToInt16(data, 6)
            Rle.Height = BitConverter.ToInt16(data, 8)
            Rle.Progress = BitConverter.ToInt32(data, 10)
            Ndx = Marshal.SizeOf(Rle)
            'Debug.Print("RLE.X:{0} Index:{1} RLE.Y:{2} Progress:{3}", Rle.X, Ndx, Rle.Y, Progress)
            XNdx = Rle.Progress
            While Ndx < data.Length
                NColor = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(Ndx))
                BColor = RGB565ToARGB32(NColor)
                NRun = data(Ndx + 2)
                Ndx += 3
                For I As Integer = 1 To NRun
                    Bmp.SetPixel(Rle.X + (XNdx Mod Rle.Width), Rle.Y + XNdx \ Rle.Width, BColor)
                    XNdx += 1
                Next I
                'Debug.Print("Final NDX:{0} NRun:{1} FinalX:{2}", Ndx, NRun, XNdx)
            End While
            Bcopy = Bmp.Clone()
        End If
        'Debug.Print("Received:{0} Control:{1} Count:{2} RLE.Y:{3}", data.Length, header.Control, PixelCount, Rle.Y)
        data = Nothing
        udpRec.BeginReceive(AddressOf ReceiveCallback, Nothing)
    End Sub
    Public Function ReverseBytes(value As UShort) As UShort
        ' Shift and mask to swap the two bytes
        Return CUShort(((value And &HFFUS) << 8) Or ((value And &HFF00US) >> 8))
    End Function
    Private Sub PictureBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove
        Dim dtr(2) As UShort

        dtr(0) = ReverseBytes(e.X)
        dtr(1) = ReverseBytes(e.Y)
        Dim xmitBytes As New List(Of Byte)
        If (e.Button = MouseButtons.Left) Then
            xmitBytes.Add(0)
        Else
            xmitBytes.Add(1)
        End If
        For Each i As UShort In dtr
            xmitBytes.AddRange(BitConverter.GetBytes(i))
        Next
        udpSend.Send(xmitBytes.ToArray, 5)
    End Sub

    Private Sub PictureBox1_MouseUp(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseUp
        Dim lc As MouseEventArgs = New MouseEventArgs(MouseButtons.None, e.Clicks, e.X, e.Y, e.Delta)
        PictureBox1_MouseMove(sender, lc)
    End Sub

    Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown
        PictureBox1_MouseMove(sender, e)
    End Sub

    Private Sub PictureBox1_Paint(sender As Object, e As PaintEventArgs) Handles PictureBox1.Paint
        PictureBox1.Image = CType(Bcopy, System.Drawing.Image)
    End Sub

    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        udpSend.Send(Disconnect, 5)
    End Sub
End Class
