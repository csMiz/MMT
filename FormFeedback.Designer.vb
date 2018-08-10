<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormFeedback
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.tbContent = New System.Windows.Forms.TextBox()
        Me.tbUsername = New System.Windows.Forms.TextBox()
        Me.tbEmail = New System.Windows.Forms.TextBox()
        Me.btn1 = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("微软雅黑", 22.125!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label1.Location = New System.Drawing.Point(46, 59)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(151, 78)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "反馈"
        '
        'tbContent
        '
        Me.tbContent.Font = New System.Drawing.Font("微软雅黑", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.tbContent.Location = New System.Drawing.Point(59, 154)
        Me.tbContent.MaxLength = 255
        Me.tbContent.Multiline = True
        Me.tbContent.Name = "tbContent"
        Me.tbContent.Size = New System.Drawing.Size(679, 574)
        Me.tbContent.TabIndex = 1
        '
        'tbUsername
        '
        Me.tbUsername.Font = New System.Drawing.Font("微软雅黑", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.tbUsername.Location = New System.Drawing.Point(270, 769)
        Me.tbUsername.MaxLength = 31
        Me.tbUsername.Name = "tbUsername"
        Me.tbUsername.Size = New System.Drawing.Size(468, 50)
        Me.tbUsername.TabIndex = 2
        '
        'tbEmail
        '
        Me.tbEmail.Font = New System.Drawing.Font("微软雅黑", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.tbEmail.Location = New System.Drawing.Point(270, 854)
        Me.tbEmail.MaxLength = 63
        Me.tbEmail.Name = "tbEmail"
        Me.tbEmail.Size = New System.Drawing.Size(468, 50)
        Me.tbEmail.TabIndex = 3
        '
        'btn1
        '
        Me.btn1.Location = New System.Drawing.Point(367, 957)
        Me.btn1.Name = "btn1"
        Me.btn1.Size = New System.Drawing.Size(371, 154)
        Me.btn1.TabIndex = 4
        Me.btn1.Text = "提交"
        Me.btn1.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("微软雅黑", 13.875!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label2.Location = New System.Drawing.Point(114, 766)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(94, 48)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "昵称"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("微软雅黑", 13.875!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label3.Location = New System.Drawing.Point(79, 851)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(168, 48)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "电子邮箱"
        '
        'FormFeedback
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(811, 1161)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.btn1)
        Me.Controls.Add(Me.tbEmail)
        Me.Controls.Add(Me.tbUsername)
        Me.Controls.Add(Me.tbContent)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "FormFeedback"
        Me.Text = "Feedback"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents tbContent As TextBox
    Friend WithEvents tbUsername As TextBox
    Friend WithEvents tbEmail As TextBox
    Friend WithEvents btn1 As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
End Class
