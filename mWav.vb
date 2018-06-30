Imports System.IO

Module mWav

    Public wavefile As New cWaveFile

    Public Class cWaveFile
        Public Content As New List(Of String)
        Public Length As Long = 0
        Public Pointer As Long = 44

        Public Sub LoadWavFile()

            Dim openFile As New OpenFileDialog
            openFile.Filter = "波形声音|*.wav"
            openFile.Title = "打开"
            openFile.AddExtension = True
            openFile.AutoUpgradeEnabled = True
            If openFile.ShowDialog() = DialogResult.OK Then
                Dim tstr As FileStream = CType(openFile.OpenFile, FileStream)
                Dim r As BinaryReader = New BinaryReader(tstr)

                Dim tempcontent As Byte = 0
                Dim jmax As Integer = r.BaseStream.Length \ 10240
                Length = r.BaseStream.Length - 44

                For j = 0 To jmax
                    Dim tcline As String = ""
                    For i = 0 To 10239
                        If j = jmax Then
                            If jmax * 10240 + i >= r.BaseStream.Length Then Exit For
                        End If
                        tempcontent = r.ReadByte()
                        tcline = tcline & ChrW(tempcontent)
                    Next
                    Content.Add(tcline)
                Next

                r.Close()
                tstr.Dispose()
            Else
                Exit Sub
            End If

        End Sub

        Public Function GetValue(tx As Long) As Byte

            Dim tv1 As Integer = tx \ 10240
            Dim tv2 As Integer = tx Mod 10240
            Return AscW(Content(tv1).Substring(tv2, 1))

        End Function

        Public Function GetSampleImage(x As Integer) As List(Of Byte)
            Dim r As New List(Of Byte)

            For i = 0 To x - 1
                Dim tx As Long = CLng(44 + i * Length / x)
                Dim ty As Byte = GetValue(tx)
                r.Add(ty)
            Next

            Return r

        End Function

    End Class

End Module
