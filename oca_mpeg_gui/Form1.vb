Public Class Form1

    Private Sub CheckAndAdjust()
        If IO.Path.GetExtension(InputLocation.Text).Contains(".oca") Then ExtractRButton.Checked = True Else CompressRButton.Checked = True
        If ExtractRButton.Checked Then
            OutputLocation.Text = IO.Path.GetFileNameWithoutExtension(InputLocation.Text)
        Else
            OutputLocation.Text = InputLocation.Text + ".oca"
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        OpenFileDialog1.Filter = "All Files|*.*"
        If CompressRButton.Checked Then
            OpenFileDialog1.Title = "Browse for a file to compress"
        Else
            OpenFileDialog1.Title = "Browse for a file to extract"
        End If
        If Not String.IsNullOrWhiteSpace(InputLocation.Text) Then OpenFileDialog1.FileName = IO.Path.GetFileName(InputLocation.Text)
        Dim result As DialogResult = OpenFileDialog1.ShowDialog
        If result = DialogResult.OK Then
            InputLocation.Text = OpenFileDialog1.FileName
            CheckAndAdjust()
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If CompressRButton.Checked Then
            SaveFileDialog1.Filter = "OCA File|*.oca"
            SaveFileDialog1.Title = "Browse for a location to save the compressed OCA file"
        Else
            SaveFileDialog1.Filter = "All Files|*.*"
            SaveFileDialog1.Title = "Browse for a location to save the extracted file"
        End If
        If Not String.IsNullOrWhiteSpace(OutputLocation.Text) Then SaveFileDialog1.FileName = IO.Path.GetFileName(OutputLocation.Text)
        Dim result As DialogResult = SaveFileDialog1.ShowDialog
        If result = DialogResult.OK Then OutputLocation.Text = SaveFileDialog1.FileName

    End Sub

    Private Sub StartBtn_Click(sender As Object, e As EventArgs) Handles StartBtn.Click
        StartBtn.Enabled = False
        SaveLogBtn.Enabled = False
        Dim CMD As String = String.Empty
        If CompressRButton.Checked Then
            CMD = CompressionLevel.Value.ToString() + " " + ChunkSizeTxt.Text + " """ + OutputLocation.Text + """ """ + InputLocation.Text + """"
        Else
            CMD = "d """ + OutputLocation.Text + """ """ + InputLocation.Text + """"
        End If
        Dim StartThread = New Threading.Thread(Sub() TaskThread(CMD))
        StartThread.Start()
    End Sub

    Private Sub TaskThread(CMD As String)
        Using process As New Process()
            process.StartInfo.WorkingDirectory = IO.Path.GetDirectoryName("oca_mpeg.exe")
            process.StartInfo.FileName = "oca_mpeg.exe"
            process.StartInfo.Arguments = CMD
            process.StartInfo.UseShellExecute = False
            process.StartInfo.RedirectStandardOutput = True
            process.StartInfo.RedirectStandardError = True
            process.StartInfo.CreateNoWindow = True
            process.Start()
            AddHandler process.OutputDataReceived, New DataReceivedEventHandler(AddressOf UpdateLogEventHandler)
            AddHandler process.ErrorDataReceived, New DataReceivedEventHandler(AddressOf UpdateLogEventHandler)
            process.BeginOutputReadLine()
            process.BeginErrorReadLine()
            process.WaitForExit()
            StartBtn.BeginInvoke(Sub() StartBtn.Enabled = True)
            SaveLogBtn.BeginInvoke(Sub() SaveLogBtn.Enabled = True)
        End Using
    End Sub

    Private Sub UpdateLogEventHandler(sender As Object, e As DataReceivedEventArgs)
        If Not e.Data = Nothing Then
            UpdateLog(e.Data)
        End If
    End Sub

    Private Delegate Sub UpdateLogInvoker(message As String)

    Private Sub UpdateLog(message As String)
        If Log.InvokeRequired Then
            Log.Invoke(New UpdateLogInvoker(AddressOf UpdateLog), message)
        Else
            If Not String.IsNullOrWhiteSpace(message) Then
                Log.AppendText(Date.Now().ToString() + " || " + message + vbCrLf)
                Log.SelectionStart = Log.Text.Length - 1
                Log.ScrollToCaret()
            End If
        End If
    End Sub

    Private Sub CompressRButton_CheckedChanged(sender As Object, e As EventArgs) Handles CompressRButton.CheckedChanged
        My.Settings.CompressChecked = CompressRButton.Checked
        My.Settings.Save()
    End Sub

    Private Sub ExtractRButton_CheckedChanged(sender As Object, e As EventArgs) Handles ExtractRButton.CheckedChanged
        My.Settings.ExtractChecked = ExtractRButton.Checked
        My.Settings.Save()
    End Sub

    Private Sub CompressionLevel_ValueChanged(sender As Object, e As EventArgs) Handles CompressionLevel.ValueChanged
        My.Settings.CompressionLevel = CompressionLevel.Value
        My.Settings.Save()
    End Sub

    Private Sub ChunkSizeTxt_TextChanged(sender As Object, e As EventArgs) Handles ChunkSizeTxt.TextChanged
        My.Settings.ChunkSize = ChunkSizeTxt.Text
        My.Settings.Save()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CompressRButton.Checked = My.Settings.CompressChecked
        ExtractRButton.Checked = My.Settings.ExtractChecked
        CompressionLevel.Value = My.Settings.CompressionLevel
        ChunkSizeTxt.Text = My.Settings.ChunkSize
        CompressionLevel.Minimum = 1
    End Sub

    Private Sub ClearLogButton_Click(sender As Object, e As EventArgs) Handles ClearLogBtn.Click
        Log.Text = String.Empty
    End Sub

    Private Sub SaveLogButton_Click(sender As Object, e As EventArgs) Handles SaveLogBtn.Click
        Dim SaveDialog As New SaveFileDialog With {
           .Filter = "Text file|*.txt",
           .FileName = String.Empty,
           .Title = "Browse to save the log"}
        Dim result As DialogResult = SaveDialog.ShowDialog
        If result = DialogResult.OK Then
            If Not String.IsNullOrWhiteSpace(SaveDialog.FileName) Then IO.File.WriteAllText(SaveDialog.FileName, Log.Text)
        End If
    End Sub
    Private Sub Form1_DragEnter(sender As Object, e As DragEventArgs) Handles MyBase.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        End If
    End Sub
    Private Sub Form1_DragDrop(sender As Object, e As DragEventArgs) Handles MyBase.DragDrop
        InputLocation.Text = CType(e.Data.GetData(DataFormats.FileDrop), String())(0)
        CheckAndAdjust()
    End Sub
End Class
