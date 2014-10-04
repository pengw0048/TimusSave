Public Class Form1

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        FolderBrowserDialog1.ShowDialog()
        TextBox3.Text = FolderBrowserDialog1.SelectedPath
    End Sub
End Class
