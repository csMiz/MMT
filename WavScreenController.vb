public class WavScreenController
    Private StartAt As Single = 0.0F        '均为百分比
    Private EndAt As Single = 100.0F
    Private Reserve As New Point

    Private ReadOnly diameter As Integer = 30

    Public MouseHitState As EMouseHitState = 0

    Public Enum EMouseHitState As Byte
        NONE = 0
        LEFT = 1
        RIGHT = 2
        MIDDLE = 3
    End Enum

    Public sub SetStartPoint(value as Single)
		if value < 0.0F then
			StartAt = 0.0F
		elseif value > 99.0F then
			StartAt = 99.0F
		elseif value >= EndAt - 1.0F then
			StartAt = EndAt - 1.0F
		else
			StartAt = value
		end if
	end sub 
	
	public sub SetEndPoint(value as Single)
		if value < 1.0F then
			EndAt = 1.0F
		elseif value > 100.0F then
			EndAt = 100.0F
		elseif EndAt <= StartAt + 1.0F then
			EndAt = StartAt + 1.0F
		else
			EndAt = value
		end if
	end sub 
	
	public sub MoveBar(Delta as Single)
		dim valid as boolean = (StartAt + Delta >= 0.0F) andalso (EndAt + Delta <= 100.0F)
        If valid Then
            If MouseHitState Then
                StartAt = Reserve.X + Delta
                EndAt = Reserve.Y + Delta
            Else
                StartAt += Delta
                EndAt += Delta
            End If
        End If
    End Sub

    Public Function GetStartPoint() As Single
        Return StartAt
    End Function

    Public Function GetEndPoint() As Single
        Return EndAt
    End Function

    Public Sub DrawBar(ByRef G As Graphics)
        'draw image of bar here.

        G.FillEllipse(Brushes.LightGray, 100, 300, diameter, diameter)
        G.FillEllipse(Brushes.LightGray, 900 - diameter, 300, diameter, diameter)
        G.FillRectangle(Brushes.LightGray, 100 + 0.5F * diameter, 300, 800 - diameter, diameter)

        Dim zoom As Single = (800 - diameter) / 100
        G.FillRectangle(Brushes.Azure, 100 + 0.5F * diameter + StartAt * zoom, 300.0F, (EndAt - StartAt) * zoom, diameter)
        G.FillEllipse(Brushes.CornflowerBlue, 100 + StartAt * zoom, 300, diameter, diameter)
        G.FillEllipse(Brushes.CornflowerBlue, 100 + EndAt * zoom, 300, diameter, diameter)
    End Sub

    Public Function JudgeHit(point As MouseEventArgs) As EMouseHitState
        Dim trueX As Single = point.X * 2
        Dim trueY As Single = point.Y * 2

        Dim zoom As Single = (800 - diameter) / 100
        If trueY >= 300 And trueY <= 300 + diameter Then
            If trueX >= 100 + StartAt * zoom AndAlso trueX <= 100 + diameter + StartAt * zoom Then
                MouseHitState = EMouseHitState.LEFT
            ElseIf trueX >= 100 + EndAt * zoom AndAlso trueX <= 100 + diameter + EndAt * zoom Then
                MouseHitState = EMouseHitState.RIGHT
            ElseIf trueX >= 100 + diameter + StartAt * zoom AndAlso trueX <= 100 + EndAt * zoom Then
                MouseHitState = EMouseHitState.MIDDLE
            End If
            Reserve.X = StartAt
            Reserve.Y = EndAt
        End If
        Return MouseHitState
    End Function

    Public Sub MouseMove(ByVal Delta As Single)
        Dim zoom As Single = (800 - diameter) / 100
        Delta = Delta * 2 / zoom
        If MouseHitState = EMouseHitState.LEFT Then
            SetStartPoint(Reserve.X + Delta)
        ElseIf MouseHitState = EMouseHitState.RIGHT Then
            SetEndPoint(Reserve.Y + Delta)
        ElseIf MouseHitState = EMouseHitState.MIDDLE Then
            MoveBar(Delta)
        End If
    End Sub

End class
