public class SentenceOperation

	''' <summary>
	''' 一键设置语句时值，单位为帧
	''' </summary>
	''' <param name="headBlock">语句开头的拼音块</param>
	public shared function SetSentence(headBlock as CPYBlock, startValue as single, endValue as single) as boolean
		if headBlock is nothing orelse startValue < 0 then return false
		dim count as integer = headBlock.GetArrayBlockCount()
		if startValue + count > endValue then return false
		headBlock.SetStartAbsolute(startValue)
		headBlock.SetArrayAvgLength((endValue-startValue)/count)
		return true		'之后需要检查重叠错误
	end function 
	
	
	
end class



'增加CTRL+Z的撤销功能
'public UndoStack as new Stack(Of MMTAction)

