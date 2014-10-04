Public Class Form1

    Dim state As Integer
    Dim JudgeID As String
    Dim pth As String

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        FolderBrowserDialog1.ShowDialog()
        TextBox3.Text = FolderBrowserDialog1.SelectedPath
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If My.Computer.FileSystem.DirectoryExists(TextBox3.Text) = False Then
            Try
                My.Computer.FileSystem.CreateDirectory(TextBox3.Text)
            Catch ex As Exception
                Label4.Text = "Status: cannot create directory!"
                Exit Sub
            End Try
        End If
        pth = TextBox3.Text
        If Strings.Right(pth, 1) <> "\" Then pth += "\"
        state = 1
        Label4.Text = "Status: verifying credentials..."
        WebBrowser1.Navigate("http://acm.timus.ru/authedit.aspx")
        Button2.Enabled = False
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(ByVal sender As Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
        Select Case state
            Case 1
                Dim doc As HtmlDocument = WebBrowser1.Document
                doc.GetElementById("JudgeID").SetAttribute("value", TextBox1.Text)
                doc.GetElementById("Password").SetAttribute("value", TextBox2.Text)
                state = 2
                Dim els As HtmlElementCollection = doc.GetElementsByTagName("input")
                For Each el As HtmlElement In els
                    If el.GetAttribute("value") = "Login" Then
                        el.InvokeMember("click")
                        Exit For
                    End If
                Next
            Case 2
                Dim doc As HtmlDocument = WebBrowser1.Document
                Dim els As HtmlElementCollection = doc.GetElementsByTagName("input")
                For Each el As HtmlElement In els
                    If el.GetAttribute("value") = "Save" Then
                        state = 3
                        Label4.Text = "Status: Getting all submissions..."
                        JudgeID = Trim(Val(TextBox1.Text))
                        WebBrowser1.Navigate("http://acm.timus.ru/author.aspx?id=" + JudgeID)
                        Exit Select
                    End If
                Next
                Label4.Text = "Status: Login failed."
                Button2.Enabled = True
                state = 0
            Case 3
                MsgBox(":)")
            Case Else

        End Select

    End Sub
End Class
