Imports System.Math
Imports System.Text.RegularExpressions
Imports Miz_MMD_Tool

'里面有各类模块和类，建议拆分

Module mMath
    Private NDTable As New List(Of PointF)

    Public Interface iPointEx
        Function Copy() As iPointEx
    End Interface

    Public Sub InitNDTable()
        For i = -400 To 399
            Dim tx As Double = i / 100
            Dim tv As Double = CND(tx)
            NDTable.Add(New PointF(CSng(tx), CSng(tv)))
        Next
    End Sub

    Public Function RCND(Acc As Single) As Single
        For i = 0 To NDTable.Count - 2
            If Acc < NDTable(i + 1).Y AndAlso Acc >= NDTable(i).Y Then
                Return NDTable(i).X
            End If
        Next
        Return 3.99
    End Function

    Public Function CND(X As Double) As Double

        Dim L As Double = 0.0
        Dim K As Double = 0.0
        Dim dCND As Double = 0.0
        Const a1 As Double = 0.31938153
        Const a2 As Double = -0.356563782
        Const a3 As Double = 1.781477937
        Const a4 As Double = -1.821255978
        Const a5 As Double = 1.330274429
        L = Abs(X)
        K = 1.0 / (1.0 + 0.2316419 * L)
        dCND = 1.0 - 1.0 / Sqrt(2 * Convert.ToDouble(PI.ToString())) * Exp(-L * L / 2.0) * (a1 * K + a2 * K * K + a3 * Pow(K, 3.0) + a4 * Pow(K, 4.0) + a5 * Pow(K, 5.0))

        If (X < 0) Then
            Return 1.0 - dCND
        Else
        End If
        Return dCND

    End Function

    Public Function getpi(nums As Integer) As String
        nums = nums / 5
        Dim max As Long, result() As String
        Dim i As Long, j As Long, t, d As Long, g, k As Long, f()
        max = 18 * nums
        ReDim f(0 To max)
        ReDim result(nums)
        For i = 0 To max
            f(i) = 20000
        Next
        g = 20000
        For j = max To 1 Step -18
            t = 0
            For i = j To 1 Step -1
                t = t + f(i) * 100000
                d = 2 * i + 1
                f(i) = t - Int(t / d) * d
                t = Int(t / d) * i
            Next
            k = k + 1
            result(k) = Format(Int(g + t / 100000) Mod 100000, "00000")

            g = t Mod 100000
        Next
        Return Join(result, "")
    End Function

    Public Class Vector3
        Public X As Single = 0
        Public Y As Single = 0
        Public Z As Single = 0

        Public Sub New(Optional tx As Single = 0, Optional ty As Single = 0, Optional tz As Single = 0)
            X = tx
            Y = ty
            Z = tz

        End Sub

    End Class

    Public Class PointF3
        Implements iPointEx
        Public X As Single = 0, Y As Single = 0, Z As Single = 0

        Public Sub New(Optional tx As Single = 0, Optional ty As Single = 0, Optional tz As Single = 0)
            X = tx
            Y = ty
            Z = tz
        End Sub

        Public Sub Move(v As Vector3)
            X += v.X
            Y += v.Y
            Z += v.Z
        End Sub

        Public Function Copy() As iPointEx Implements iPointEx.Copy
            Dim r As New PointF3(X, Y, Z)
            Return r
        End Function

        Public Function GetXZ() As PointF
            Dim r As New PointF(X, Z)
            Return r
        End Function
    End Class

    Public Class BezierPenPoint
        Public StartPoint As PointF3
        Public ArmPoint1 As PointF3
        Public ReadOnly ArmPoint2 As PointF3

        Public Sub New(SP As PointF3, AP1 As PointF3)
            StartPoint = SP
            ArmPoint1 = AP1

            Dim ap2 As New PointF3
            With ap2
                .X = SP.X * 2 - AP1.X
                .Y = SP.Y * 2 - AP1.Y
                .Z = SP.Z * 2 - AP1.Z
            End With
            ArmPoint2 = ap2
        End Sub

    End Class

    Public Function Bezier(pCont As List(Of PointF3), t As Double) As PointF3
        If pCont.Count = 0 Then Return Nothing
        Dim r As New PointF3
        Dim n As Short = pCont.Count - 1
        For i = 0 To n
            r.X += PascalT(n, i) * pCont(i).X * (1 - t) ^ (n - i) * t ^ i
            r.Y += PascalT(n, i) * pCont(i).Y * (1 - t) ^ (n - i) * t ^ i
            r.Z += PascalT(n, i) * pCont(i).Z * (1 - t) ^ (n - i) * t ^ i
        Next
        Return r
    End Function

    Public Function Bezier(pCont As List(Of BezierPenPoint), t As Double) As PointF3
        If pCont.Count = 0 Then Return Nothing

        Dim listlen As New List(Of Single)
        For i = 0 To pCont.Count - 2
            Dim tbpp1 As BezierPenPoint = pCont(i)
            Dim tbpp2 As BezierPenPoint = pCont(i + 1)
            Dim tl As Single = 0
            tl += CalcDist(tbpp1.StartPoint, tbpp1.ArmPoint2)
            tl += CalcDist(tbpp1.ArmPoint2, tbpp2.ArmPoint1)
            tl += CalcDist(tbpp2.ArmPoint1, tbpp2.StartPoint)
            listlen.Add(tl)
        Next

        Dim listlen2 As New List(Of Double)
        Dim tacc As Double = 0
        Dim tsum As Double = listlen.Sum
        For i = 0 To listlen.Count - 1
            tacc += listlen(i) / tsum
            listlen2.Add(tacc)
        Next

        Dim lineindex As Short = 0
        Dim t2 As Double = 0
        For i = 0 To listlen2.Count - 1
            If t <= listlen2(i) Then
                lineindex = i
                If i = 0 Then
                    t2 = t / listlen2(i)
                Else
                    t2 = (t - listlen2(i - 1)) / (listlen2(i) - listlen2(i - 1))
                End If
                Exit For
            End If
        Next

        Dim r As New PointF3
        Dim cont2 As New List(Of PointF3)
        cont2.Add(pCont(lineindex).StartPoint)
        cont2.Add(pCont(lineindex).ArmPoint2)
        cont2.Add(pCont(lineindex + 1).ArmPoint1)
        cont2.Add(pCont(lineindex + 1).StartPoint)
        r = Bezier(cont2, t2)

        Return r
    End Function

    Public Function Bezier(pCont As List(Of BezierPenPoint), slides As Integer) As PointF3
        If pCont.Count = 0 Then Return Nothing

        Dim linenumber As Integer = slides \ 100
        Dim slidenumber As Double = (slides Mod 100) / 100

        Dim r As New PointF3
        Dim cont2 As New List(Of PointF3)
        cont2.Add(pCont(linenumber).StartPoint)
        cont2.Add(pCont(linenumber).ArmPoint2)
        cont2.Add(pCont(linenumber + 1).ArmPoint1)
        cont2.Add(pCont(linenumber + 1).StartPoint)
        r = Bezier(cont2, slidenumber)

        Return r
    End Function

    Public Function Bezier(pCont As List(Of PointF), t As Double) As PointF
        If pCont.Count = 0 Then Return Nothing
        Dim r As New PointF
        Dim n As Short = pCont.Count - 1
        For i = 0 To n
            r.X += PascalT(n, i) * pCont(i).X * (1 - t) ^ (n - i) * t ^ i
            r.Y += PascalT(n, i) * pCont(i).Y * (1 - t) ^ (n - i) * t ^ i
        Next
        Return r
    End Function

    Public Function PascalT(a As Short, t As Short) As Integer
        Dim r1 As Double = 1
        Dim r2 As Double = 1
        If t = 0 Then Return 1
        For i = 0 To t - 1
            r1 *= (a - i)
            r2 *= (i + 1)
        Next
        Return CInt(r1 / r2)
    End Function

    Public Function Slerp(V1 As VectorF4, V2 As VectorF4, t As Double) As VectorF4
        Dim result As New VectorF4
        'If V1.X = 0 AndAlso V1.Y = 0 AndAlso V1.Z = 0 AndAlso V1.W = 0 Then
        '    V1.W = 1
        'End If
        Dim cosa As Single = V1.W * V2.W + V1.X * V2.X + V1.Y * V2.Y + V1.Z * V2.Z

        If (cosa < 0.0F) Then
            V2.X = -V2.X
            V2.Y = -V2.Y
            V2.Y = -V2.Y
            V2.Z = -V2.Z
            cosa = -cosa
        End If

        Dim k0 As Single, k1 As Single

        If (cosa > 0.9995F) Then
            k0 = 1.0F - t
            k1 = t
        Else
            Dim sina As Single = (1.0F - cosa * cosa) ^ 0.5
            Dim a As Single = Atan2(sina, cosa)
            k0 = Sin((1.0F - t) * a) / sina
            k1 = Sin(t * a) / sina
        End If

        result.W = V1.W * k0 + V2.W * k1
        result.X = V1.X * k0 + V2.X * k1
        result.Y = V1.Y * k0 + V2.Y * k1
        result.Z = V1.Z * k0 + V2.Z * k1

        Return result
    End Function

    Public Function ReverseSlerp(V1 As VectorF4, V2 As VectorF4, t As Single) As VectorF4

        Dim result As New VectorF4
        '如何解决四个值均为0的情况？
        'If V1.X = 0 AndAlso V1.Y = 0 AndAlso V1.Z = 0 AndAlso V1.W = 0 Then
        '    V1.W = 1
        'End If
        Dim cosa As Single = V1.W * V2.W + V1.X * V2.X + V1.Y * V2.Y + V1.Z * V2.Z
        Dim k0 As Single, k1 As Single
        Dim sina As Single = (1.0F - cosa * cosa) ^ 0.5
        Dim a As Single = Atan2(sina, cosa)

        If (Cos(a * t) > 0.9995F) Then
            k0 = (1.0F - t) / t
            k1 = 1 / t
        Else
            k0 = Sin((1.0F - t) * a) / sina
            k1 = Sin(t * a) / sina
        End If

        result.W = -V1.W * k0 + V2.W * k1
        result.X = -V1.X * k0 + V2.X * k1
        result.Y = -V1.Y * k0 + V2.Y * k1
        result.Z = -V1.Z * k0 + V2.Z * k1

        Return result
    End Function

    Public Function CalcDist(pa As PointF3, pb As PointF3) As Single
        Dim r As Double = 0
        r = ((pa.X - pb.X) ^ 2 + (pa.Y - pb.Y) ^ 2 + (pa.Z - pb.Z) ^ 2) ^ 0.5
        Return CSng(r)
    End Function

    Public Function CalcDist(pa As PointF, pb As PointF) As Single
        Dim r = 0
        r = ((pa.X - pb.X) ^ 2 + (pa.Y - pb.Y) ^ 2) ^ 0.5
        Return r
    End Function

    Public Function Helix(a As Single) As PointF
        'Dim r As New PointF

    End Function

End Module

Module mHex
    Public Function GetByte(ByRef BContent As List(Of String), pos As Integer) As Byte
        Dim tl As Integer = pos \ 2000
        Dim tp As Integer = pos Mod 2000
        Dim r As Byte = 0
        If tl > BContent.Count - 1 Then Return r
        Dim ol As String = BContent(tl)

        If tp >= ol.Length Then Return r

        r = ol.Substring(tp, 1)

        Return r
    End Function

    Public Function GetBytes(ByRef BContent As List(Of String), start As Integer, len As Integer) As String
        Dim r As String = ""
        Dim s1 As Integer = start \ 2000
        Dim s2 As Integer = start Mod 2000

        If s2 + len - 1 < 2000 Then
            r = BContent(s1).Substring(s2, len)
        ElseIf len < 2000 Then
            Dim s3 As Integer = s2 + len - 2000
            r = BContent(s1).Substring(s2, 2000 - s2)
            r = r & BContent(s1 + 1).Substring(0, s3)

        End If

        Return r

    End Function

    'Public Function FourBytesToInt(input As String) As Integer
    '    '超过80 00 00 00会溢出
    '    Dim r As Integer = 0
    '    Dim b1 As Byte = AscW(input.Substring(0, 1))
    '    Dim b2 As Byte = AscW(input.Substring(1, 1))
    '    Dim b3 As Byte = AscW(input.Substring(2, 1))
    '    Dim b4 As Byte = AscW(input.Substring(3, 1))
    '    r = b4 * (256 ^ 3) + b3 * (256 ^ 2) + b2 * 256 + b1
    '    Return r

    'End Function

    Public Function StringTo4Bytes(input As String) As Byte()
        Dim r(3) As Byte
        r(0) = AscW(input.Substring(0, 1))
        r(1) = AscW(input.Substring(1, 1))
        r(2) = AscW(input.Substring(2, 1))
        r(3) = AscW(input.Substring(3, 1))

        Return r

    End Function

    Public Function Receive4Bytes(a As String, b As String, c As String, d As String) As Byte()
        Dim r(3) As Byte
        r(0) = HEXByte(a)
        r(1) = HEXByte(b)
        r(2) = HEXByte(c)
        r(3) = HEXByte(d)

        Return r
    End Function

    Public Function ReceiveBytes(input As String) As Byte()
        Dim r(255) As Byte
        Dim st() As String = Regex.Split(input, " ")
        If st.Length Then
            For i = 0 To st.Length - 1
                r(i) = HEXByte(st(i))
            Next
        End If
        Return r
    End Function

    Public Function CharToBytes(input As String) As Byte()
        Dim r(255) As Byte
        For i = 0 To 255
            r(i) = 0
        Next
        If input.Length Then
            For i = 0 To input.Length - 1
                r(i) = AscW(input.Substring(i, 1))
            Next
        End If
        Return r
    End Function

    Public Function HEXByte(input As String) As Byte
        input = input.ToUpper
        Dim a As Char = input.Substring(0, 1)
        Dim b As Char = input.Substring(1, 1)

        Dim c As Byte = AscW(a) - AscW("0")
        If c >= 10 Then c -= 7
        Dim d As Byte = AscW(b) - AscW("0")
        If d >= 10 Then d -= 7

        Return (c * 16 + d)
    End Function

End Module

Module mMMDPhysics

    Public Enum eColliderShape As Byte
        Cube = 0
        Sphere = 1
        'Cuboid = 2
    End Enum

    Public Class PhysicsBone
        Public BindingBone As Bone
        Public LinkedSynthesis As CollideSynthesis

        Public Sub New(tb As Bone)
            BindingBone = tb
        End Sub

    End Class

    Public Class Collider
        Public Shape As eColliderShape = eColliderShape.Cube
        Public Param As Single = 5
        Public Mass As Single = 1

        Public Sub New(tshape As eColliderShape, arg As Single, tmass As Single)
            Shape = tshape
            Param = arg
            Mass = tmass
        End Sub

    End Class

    Public Class CollideSynthesis
        Public Bones As New List(Of PhysicsBone)
        Public Colliders As New List(Of Collider)

        Public Function IsValid() As Boolean
            If Bones.Count > 0 AndAlso Colliders.Count > 0 Then Return True Else Return False
        End Function

    End Class


    Public Function PhysicsBoneMove(tb As PhysicsBone, pos As BonePoint, dir As Vector3, vel As Single) As BonePoint

        Throw New NotImplementedException
    End Function

End Module

Module mMMD

    Public Class PointB
        Public X As Byte
        Public Y As Byte

        Public Sub New(Optional tx As Byte = 0, Optional ty As Byte = 0)
            X = tx
            Y = ty
        End Sub

        Public Function Copy() As PointB
            Dim r As New PointB
            With r
                .X = X
                .Y = Y
            End With
            Return r
        End Function

    End Class

    Public Interface MMDPoint
        Function Copy() As MMDPoint
        Sub Apply(tp As MMDPoint)
		Sub ApplyMax(input as MMDPoint)
        Property Frame As Integer

    End Interface

    Public Interface MMDBoneFace
        Function Copy() As MMDBoneFace
        Sub AddPoint(tp As MMDPoint)
		Sub AddPointApplyMax(input as MMDPoint)
        Function IsEmpty() As Boolean
        Property Name As String
        Function GetPointCount() As Integer
        Function GetAt(frame As Integer) As MMDPoint

    End Interface

    Public Class Bone
        Implements MMDBoneFace
        Private MyName As String = ""
        Public PointList As List(Of BonePoint)

        Sub New()
            PointList = New List(Of BonePoint)

        End Sub

        Public Property Name As String Implements MMDBoneFace.Name
            Get
                Return MyName
            End Get
            Set(value As String)
                MyName = value
            End Set
        End Property

        Public Sub AddPoint(tp As MMDPoint) Implements MMDBoneFace.AddPoint
            If TypeOf tp Is BonePoint Then
                Dim tf As Integer = tp.Frame
                If CBool(PointList.Count) Then
                    For Each ttp As BonePoint In PointList
                        If ttp.Frame = tf Then
                            ttp.Apply(tp)
                            Exit Sub
                        End If
                    Next
                End If
                PointList.Add(CType(tp, BonePoint))
            End If
        End Sub
		
		public sub AddPointApplyMax(input as MMDPoint) Implements MMDBoneFace.AddPointApplyMax
		
			throw new NotImplementedException()
			
		end sub 

        Public Function Copy() As MMDBoneFace Implements MMDBoneFace.Copy
            Throw New NotImplementedException()
        End Function

        Public Function IsEmpty() As Boolean Implements MMDBoneFace.IsEmpty
            Return Not CBool(PointList.Count)
        End Function

        Public Function GetPointCount() As Integer Implements MMDBoneFace.GetPointCount
            Return PointList.Count
        End Function

        Public Function GetAt(frame As Integer) As MMDPoint Implements MMDBoneFace.GetAt
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class BonePoint
        Implements MMDPoint
        Public Frame As Integer = 0
        Public X As Single = 0, Y As Single = 0, Z As Single = 0
        Private RX As Single = 0, RY As Single = 0, RZ As Single = 0, RW As Single = 0
        Private SSX As Single = 0, SSY As Single = 0, SSZ As Single = 0
        Private TWXA As New PointB, TWXB As New PointB, TWYA As New PointB, TWYB As New PointB
        Private TWZA As New PointB, TWZB As New PointB, TWRA As New PointB, TWRB As New PointB



        Enum QuaParam As Byte
            QX = 0
            QY = 1
            QZ = 2
            QW = 3
        End Enum

        Public Property SX As Single
            Get
                Return SSX
            End Get
            Set(value As Single)
                SSX = value
                Call CalcQuaternion()
            End Set

        End Property
        Public Property SY As Single
            Get
                Return SSY
            End Get
            Set(value As Single)
                SSY = value
                Call CalcQuaternion()
            End Set

        End Property
        Public Property SZ As Single
            Get
                Return -SSZ
            End Get
            Set(value As Single)
                SSZ = -value
                Call CalcQuaternion()
            End Set

        End Property

        Public Sub New()
            Call DefaultTween()
        End Sub

        Public Overloads Function GetQuaternion(param As QuaParam) As Single
            Select Case param
                Case QuaParam.QX
                    Return RX
                Case QuaParam.QY
                    Return RY
                Case QuaParam.QZ
                    Return RZ
                Case QuaParam.QW
                    Return RW
            End Select
            Return 0
        End Function

        Public Overloads Function GetQuaternion() As VectorF4
            Dim r As VectorF4 = Me.QuaToV4
            Return r
        End Function

        Public Sub SetQuaternion(vector As VectorF4)
            RW = vector.W
            RX = vector.X
            RY = vector.Y
            RZ = vector.Z
            Call CalcSSRotate()
        End Sub

        Public Sub CalcQuaternion()
            'x = sin(Y/2)sin(Z/2)cos(X/2)+cos(Y/2)cos(Z/2)sin(X/2)
            'Y = sin(Y / 2)cos(Z/2)cos(X/2)+cos(Y/2)sin(Z/2)sin(X/2)
            'Z = cos(Y / 2)sin(Z/2)cos(X/2)-sin(Y/2)cos(Z/2)sin(X/2)
            'w = cos(Y / 2)cos(Z/2)cos(X/2)-sin(Y/2)sin(Z/2)sin(X/2)
            'q = ((X, Y, Z), w)

            Dim rax As Single = SSX * PI / 180
            Dim ray As Single = SSY * PI / 180
            Dim raz As Single = SSZ * PI / 180

            RX = Sin(ray / 2) * Sin(raz / 2) * Cos(rax / 2) + Cos(ray / 2) * Cos(raz / 2) * Sin(rax / 2)
            RY = Sin(ray / 2) * Cos(raz / 2) * Cos(rax / 2) + Cos(ray / 2) * Sin(raz / 2) * Sin(rax / 2)
            RZ = Cos(ray / 2) * Sin(raz / 2) * Cos(rax / 2) - Sin(ray / 2) * Cos(raz / 2) * Sin(rax / 2)
            RW = Cos(ray / 2) * Cos(raz / 2) * Cos(rax / 2) - Sin(ray / 2) * Sin(raz / 2) * Sin(rax / 2)

        End Sub

        Public Sub CalcSSRotate()   '这里有问题

            SSX = Atan2(2 * (RW * RX + RY * RZ), 1 - 2 * (RX ^ 2 + RY ^ 2))
            SSY = Asin(2 * (RW * RY - RZ * RX))
            SSZ = Atan2(2 * (RW * RZ + RZ * RY), 1 - 2 * (RY ^ 2 + RZ ^ 2))

            SSX = SSX * 180 / PI
            SSY = SSY * 180 / PI
            SSZ = SSZ * 180 / PI

        End Sub

        Private Property MMDPoint_Frame As Integer Implements MMDPoint.Frame
            Get
                Return Frame
            End Get
            Set(value As Integer)
                Frame = value
            End Set
        End Property

        Public Function Copy() As MMDPoint Implements MMDPoint.Copy
            Dim r As New BonePoint
            With r
                .X = X
                .Y = Y
                .Z = Z
                .RX = RX
                .RY = RY
                .RZ = RZ
                .RW = RW

                Call .CalcSSRotate()
            End With

            Return r
        End Function

        Public Sub Init(tx As Single, ty As Single, tz As Single, rtx As Single, rty As Single, rtz As Single, rtw As Single)
            X = tx
            Y = ty
            Z = tz
            RX = rtx
            RY = rty
            RZ = rtz
            RW = rtw

            Call CalcSSRotate()
        End Sub

        Public Sub DeltaPos(tv As Vector3)
            X += tv.X
            Y += tv.Y
            Z += tv.Z

        End Sub

        Public Sub SetPos(tp As PointF3)
            X = tp.X
            Y = tp.Y
            Z = tp.Z
        End Sub

        Public Function QuaToV4() As VectorF4
            Dim r As New VectorF4(RW, RX, RY, RZ)
            Return r
        End Function

        Public Function PosToV3() As Vector3
            Dim r As New Vector3(X, Y, Z)
            Return r
        End Function

        Public Function PosToP3() As PointF3
            Dim r As New PointF3(X, Y, Z)
            Return r
        End Function

        Public Sub SetTween(input As String)
            TWXA.X = AscW(input.Substring(0, 1))
            TWYA.X = AscW(input.Substring(1, 1))

            TWXA.Y = AscW(input.Substring(4, 1))
            TWYA.Y = AscW(input.Substring(5, 1))
            TWZA.Y = AscW(input.Substring(6, 1))
            TWRA.Y = AscW(input.Substring(7, 1))

            TWXB.X = AscW(input.Substring(8, 1))
            TWYB.X = AscW(input.Substring(9, 1))
            TWZB.X = AscW(input.Substring(10, 1))
            TWRB.X = AscW(input.Substring(11, 1))

            TWXB.Y = AscW(input.Substring(12, 1))
            TWYB.Y = AscW(input.Substring(13, 1))
            TWZB.Y = AscW(input.Substring(14, 1))
            TWRB.Y = AscW(input.Substring(15, 1))

            TWZA.X = AscW(input.Substring(17, 1))
            TWRA.X = AscW(input.Substring(18, 1))

        End Sub

        Public Function GetTweenBytes() As Byte()

            Dim r(63) As Byte
            For i = 0 To 63
                r(i) = 0
            Next

            r(0) = TWXA.X
            r(1) = TWYA.X
            r(2) = TWZA.X
            r(3) = TWRA.X

            r(4) = TWXA.Y
            r(5) = TWYA.Y
            r(6) = TWZA.Y
            r(7) = TWRA.Y

            r(8) = TWXB.X
            r(9) = TWYB.X
            r(10) = TWZB.X
            r(11) = TWRB.X

            r(12) = TWXB.Y
            r(13) = TWYB.Y
            r(14) = TWZB.Y
            r(15) = TWRB.Y

            For j = 1 To 3
                For i = 16 * j To 16 * j + 14
                    r(i) = r(i - 15)
                Next
            Next

            r(2) = HEXByte("00")
            r(3) = HEXByte("00")
            r(31) = HEXByte("90")
            r(46) = HEXByte("90")
            r(47) = HEXByte("99")
            r(61) = HEXByte("90")
            r(62) = HEXByte("99")
            r(63) = HEXByte("2D")

            Return r
        End Function

        Public Sub DefaultTween()
            TWXA = New PointB(20, 20)
            TWXB = New PointB(107, 107)
            TWYA = New PointB(20, 20)
            TWYB = New PointB(107, 107)
            TWZA = New PointB(20, 20)
            TWZB = New PointB(107, 107)
            TWRA = New PointB(20, 20)
            TWRB = New PointB(107, 107)

        End Sub

        Public Sub Apply(tp As MMDPoint) Implements MMDPoint.Apply
            Dim ttp As BonePoint = CType(tp, BonePoint)
            With ttp
                X = .X
                Y = .Y
                Z = .Z
                RX = .RX
                RY = .RY
                RZ = .RZ
                RW = .RW

                Call CalcSSRotate()
            End With
        End Sub
		
		public sub ApplyMax(input as MMDPoint) Implements MMDPoint.ApplyMax
			throw new NotImplementedException()
		end sub 
    End Class

    Public Class Face
        Implements MMDBoneFace
        Private MyName As String = ""
        Public PointList As List(Of FacePoint)

        Sub New()
            PointList = New List(Of FacePoint)

        End Sub

        Public Property Name As String Implements MMDBoneFace.Name
            Get
                Return MyName
            End Get
            Set(value As String)
                MyName = value
            End Set
        End Property

        Public Sub AddPoint(tp As MMDPoint) Implements MMDBoneFace.AddPoint
            If TypeOf tp Is FacePoint Then
                Dim tf As Integer = tp.Frame
                If CBool(PointList.Count) Then
                    For Each ttp As FacePoint In PointList
                        If ttp.Frame = tf Then
                            ttp.Apply(tp)
                            Exit Sub
                        End If
                    Next
                End If
                PointList.Add(CType(tp, FacePoint))
            End If
        End Sub
		
		public sub AddPointApplyMax(input as MMDPoint) Implements MMDBoneFace.AddPointApplyMax
			If TypeOf input Is FacePoint Then
                Dim frame As Integer = input.Frame
                If CBool(Me.PointList.Count) Then
                    For Each comparePoint As FacePoint In Me.PointList
                        If comparePoint.Frame = frame Then
                            comparePoint.ApplyMax(input)
                            Exit Sub
                        End If
                    Next
                End If
                Me.PointList.Add(CType(input, FacePoint))
            End If
		end sub 

        Public Function Copy() As MMDBoneFace Implements MMDBoneFace.Copy
            Throw New NotImplementedException
        End Function

        Public Function IsEmpty() As Boolean Implements MMDBoneFace.IsEmpty
            Return Not CBool(PointList.Count)
        End Function

        Public Function GetPointCount() As Integer Implements MMDBoneFace.GetPointCount
            Return PointList.Count
        End Function

        Public Function GetAt(frame As Integer) As MMDPoint Implements MMDBoneFace.GetAt
            Dim count = Me.GetPointCount
            If count Then
                For i = 0 To count - 1
                    Dim point As FacePoint = Me.PointList(i)
                    If point.Frame = frame Then
                        Return point
                    End If
                Next
            End If
            Return Nothing
        End Function
    End Class

    Public Class FacePoint
        Implements MMDPoint
        Public Frame As Integer = 0
        Public V As Single = 0

        Private Property MMDPoint_Frame As Integer Implements MMDPoint.Frame
            Get
                Return Frame
            End Get
            Set(value As Integer)
                Frame = value
            End Set
        End Property

        Public Function Copy() As MMDPoint Implements MMDPoint.Copy
            Dim r As New FacePoint
            With r
                .V = V
            End With

            Return r
        End Function

        Public Sub New(Optional tf As Integer = 0, Optional tv As Single = 0)
            Frame = tf
            V = tv
        End Sub

        Public Sub Init(tv As Single)
            V = tv
        End Sub

        Public Sub Apply(tp As MMDPoint) Implements MMDPoint.Apply
            Dim ttp As FacePoint = CType(tp, FacePoint)
            With ttp
                V = .V
            End With
        End Sub
		
		public sub ApplyMax(input as MMDPoint) Implements MMDPoint.ApplyMax
			Dim facePoint As FacePoint = CType(input, FacePoint)
            With facePoint
                if Me.V < .V then
					Me.V = .V
				end if 
            End With
		end sub 
		
    End Class

End Module

Public Class VectorF4
    Public W As Single = 0, X As Single = 0, Y As Single = 0, Z As Single = 0

    Public Sub New(Optional tw As Single = 0, Optional tx As Single = 0, Optional ty As Single = 0, Optional tz As Single = 0)
        W = tw
        X = tx
        Y = ty
        Z = tz
    End Sub

End Class

Public Class Vector2
    Public X As Single = 0, Y As Single = 0

    Public Sub New(Optional tx As Single = 0, Optional ty As Single = 0)
        X = tx
        Y = ty
    End Sub

End Class
