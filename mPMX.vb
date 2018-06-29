Module mPMX
    Public PMXName As String = ""
    Public PMXDesc As String = ""
    Public PMXName_Eng As String = ""
    Public PMXDesc_Eng As String = ""

    Public EncodeType As Byte = 0   '0-UTF16 1-UTF8
    Public UV_Addition As Byte = 0  '0-4
    Public VertexIndexSize As Byte = 1   '1,2,4
    Public TextureIndexSize As Byte = 1  '1,2,4
    Public MatIndexSize As Byte = 1
    Public BoneIndexSize As Byte = 1
    Public MofIndexSize As Byte = 1     '表情？
    Public RigidIndexSize As Byte = 1

    Public ModelVertex As New List(Of PMXPoint)



    Public Class PMXPoint
        Public Pos As PointF3
        Public Normal As Vector3
        Public UV As Vector2
        Public LinkedBones As New List(Of KeyValuePair(Of Integer, Single))
        Public EdgeSize As Single = 1

        Public Sub New(tpos As PointF3, tnorm As Vector3, tuv As Vector2, Optional tedge As Single = 1)
            Pos = tpos
            Normal = tnorm
            UV = tuv
            EdgeSize = tedge
        End Sub

    End Class

End Module
