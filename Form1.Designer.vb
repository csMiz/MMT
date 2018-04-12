<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.P = New System.Windows.Forms.PictureBox()
        Me.TB1 = New System.Windows.Forms.TextBox()
        Me.TB2 = New System.Windows.Forms.TextBox()
        CType(Me.P, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'P
        '
        Me.P.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.P.Location = New System.Drawing.Point(77, 36)
        Me.P.Name = "P"
        Me.P.Size = New System.Drawing.Size(1000, 750)
        Me.P.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.P.TabIndex = 0
        Me.P.TabStop = False
        '
        'TB1
        '
        Me.TB1.Location = New System.Drawing.Point(1147, 36)
        Me.TB1.Multiline = True
        Me.TB1.Name = "TB1"
        Me.TB1.ReadOnly = True
        Me.TB1.Size = New System.Drawing.Size(527, 697)
        Me.TB1.TabIndex = 2
        '
        'TB2
        '
        Me.TB2.AcceptsReturn = True
        Me.TB2.Location = New System.Drawing.Point(1147, 761)
        Me.TB2.Multiline = True
        Me.TB2.Name = "TB2"
        Me.TB2.Size = New System.Drawing.Size(526, 35)
        Me.TB2.TabIndex = 1
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(192.0!, 192.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ClientSize = New System.Drawing.Size(1714, 837)
        Me.Controls.Add(Me.TB2)
        Me.Controls.Add(Me.TB1)
        Me.Controls.Add(Me.P)
        Me.Name = "Form1"
        Me.Text = "Miz MMD Tool"
        CType(Me.P, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents P As PictureBox
    Friend WithEvents TB1 As TextBox
    Friend WithEvents TB2 As TextBox
End Class
