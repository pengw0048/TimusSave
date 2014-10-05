Imports System.Security.Cryptography
Imports System.Text

Public Class Form1

    Structure Submission
        Dim ID As String, Dat As Date, Language As String, Result As String, Test As String, Link As String
    End Structure

    Dim state As Integer, prob As Integer
    Dim JudgeID As String
    Dim pth As String
    Dim status(2500) As Integer
    Dim tried As Integer, total As Integer, done As Integer
    Dim hashlist As New List(Of String)
    Dim subs(1000) As Submission, subc As Integer
    Dim postdata As Byte()
    Dim tdoc As HtmlDocument

    Private Function GenerateHash(ByVal SourceText As String) As String
        'Create an encoding object to ensure the encoding standard for the source text
        Dim Ue As New UnicodeEncoding()
        'Retrieve a byte array based on the source text
        Dim ByteSourceText() As Byte = Ue.GetBytes(SourceText)
        'Instantiate an MD5 Provider object
        Dim Md5 As New MD5CryptoServiceProvider()
        'Compute the hash value from the source
        Dim ByteHash() As Byte = Md5.ComputeHash(ByteSourceText)
        'And convert it to String format for return
        Return Convert.ToBase64String(ByteHash)
    End Function

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
        Label4.Text = "Status: Verifying credentials..."
        WebBrowser1.Navigate("http://acm.timus.ru/authedit.aspx")
        Button2.Enabled = False
        ProgressBar1.Value = 0
        ProgressBar2.Value = 0
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
                        Label4.Text = "Status: Getting problems state..."
                        JudgeID = Trim(Val(TextBox1.Text))
                        postdata = Encoding.Default.GetBytes("Action=getsubmit&JudgeID=" + TextBox1.Text + "&Password=" + TextBox2.Text)
                        For i = 1000 To 2500
                            status(i) = 1
                        Next
                        tried = 0
                        total = 0
                        WebBrowser1.Navigate("http://acm.timus.ru/author.aspx?id=" + JudgeID)
                        Exit Select
                    End If
                Next
                Label4.Text = "Status: Login failed."
                Button2.Enabled = True
                state = 0
            Case 3
                Dim doc As HtmlDocument = WebBrowser1.Document
                Dim els As HtmlElementCollection = doc.GetElementsByTagName("td")
                For Each el As HtmlElement In els
                    If el.GetAttribute("className") = "accepted" Then
                        Dim num As Integer = Val(el.FirstChild.InnerText)
                        status(num) = 2
                        tried += 1
                        total += 1
                    ElseIf el.GetAttribute("className") = "tried" Then
                        Dim num As Integer = Val(el.FirstChild.InnerText)
                        status(num) = 3
                        tried += 1
                        total += 1
                    ElseIf el.GetAttribute("className") = "empty" Then
                        total += 1
                    End If
                Next
                state = 4
                done = 0
                prob = 1000
                WebBrowser1.Navigate("about:blank")
            Case 4
                Do While prob < 2500
                    If status(prob) > 1 Then Exit Do
                    prob += 1
                Loop
                If prob = 2500 Then
                    Label4.Text = "Status: Done!"
                    ProgressBar1.Value = 100
                    Button2.Enabled = True
                    Exit Sub
                End If
                hashlist.Clear()
                subc = 0
                state = 5
                done += 1
                WebBrowser1.Navigate("http://acm.timus.ru/status.aspx?space=1&num=" + Trim(prob) + "&author=" + JudgeID + "&refresh=0&count=1000")
            Case 5
                Label4.Text = "Status: Retrieving Problem " + Trim(prob) + " " + Trim(done) + "/" + Trim(tried)
                ProgressBar1.Value = 100.0 * done / tried
                Dim doc As HtmlDocument = WebBrowser1.Document
                tdoc = doc
                Dim th As New System.Threading.Thread(AddressOf work)
                th.Start()
            Case Else

        End Select

    End Sub

    Function transRes(ByVal result As String) As String
        Select Case result.Substring(0, 3)
            Case "Com"
                Return "CE"
            Case "Wro"
                Return "WA"
            Case "Acc"
                Return "AC"
            Case "Tim"
                Return "TL"
            Case "Mem"
                Return "ML"
            Case "Run"
                Return "RE"
            Case "Out"
                Return "OL"
            Case "Res"
                Return "RF"
            Case Else
                Return result
        End Select
    End Function

    Function findExt(ByRef subm As Submission) As String
        Dim a() As String = Split(subm.Link, ".")
        Return a(UBound(a))
    End Function

    Sub work()
        Dim els As HtmlElementCollection = tdoc.GetElementsByTagName("tr")
        For Each el As HtmlElement In els
            If el.GetAttribute("className") = "even" Or el.GetAttribute("className") = "odd" Then
                For Each ch As HtmlElement In el.Children
                    Select Case ch.GetAttribute("className")
                        Case "id"
                            subs(subc).ID = ch.FirstChild.InnerText
                            subs(subc).Link = ch.FirstChild.GetAttribute("href")
                        Case "date"
                            subs(subc).Dat = ch.Children(0).InnerText + " " + ch.Children(2).InnerText
                        Case "language"
                            subs(subc).Language = ch.InnerText
                        Case "test"
                            subs(subc).Test = IIf(IsNumeric(ch.InnerText), ch.InnerText, "")
                        Case Else
                            If ch.GetAttribute("className").StartsWith("verdict") Then
                                subs(subc).Result = ch.InnerText
                            End If
                    End Select
                Next
                subc += 1
            End If
        Next
        Dim bpath As String = pth + Trim(prob)
        Try
            My.Computer.FileSystem.DeleteDirectory(bpath, FileIO.DeleteDirectoryOption.DeleteAllContents)
        Catch ex As Exception

        End Try
        My.Computer.FileSystem.CreateDirectory(bpath)
        Dim req As Net.HttpWebRequest = Nothing
        For i = 0 To subc - 1
            If CheckBox2.Checked = False Or subs(i).Result = "Accepted" Then
                req = Net.HttpWebRequest.Create(subs(i).Link)
                req.Method = "GET"
                Dim cookies As New Net.CookieContainer
                req.CookieContainer = cookies
                req.GetResponse.Close()
                Application.DoEvents()
                req = Net.HttpWebRequest.Create(subs(i).Link)
                req.Method = "POST"
                req.ContentType = "application/x-www-form-urlencoded"
                req.ContentLength = postdata.Length
                req.GetRequestStream.Write(postdata, 0, postdata.Length)
                req.Accept = "*/*"
                req.Referer = subs(i).Link
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.124 Safari/537.36"
                req.CookieContainer = cookies
                Dim resp As Net.HttpWebResponse = req.GetResponse()
                Application.DoEvents()
                Dim reader As New IO.StreamReader(resp.GetResponseStream())
                Dim ts As String = reader.ReadToEnd()
                reader.Close()
                resp.Close()
                Dim hash As String = GenerateHash(ts)
                If CheckBox1.Checked = False Or Not hashlist.Exists(Function(val As String) As Boolean
                                                                        Return val = hash
                                                                    End Function) Then
                    hashlist.Add(hash)
                    Dim fname As String = TextBox4.Text
                    fname = fname.Replace("{prob}", Trim(prob))
                    fname = fname.Replace("{id}", subs(i).ID)
                    fname = fname.Replace("{res}", transRes(subs(i).Result))
                    fname = fname.Replace("{date}", Format(subs(i).Dat, "yyyyMMdd"))
                    fname = fname.Replace("{time}", Format(subs(i).Dat, "HHmmss"))
                    fname = fname.Replace("{fmt}", findExt(subs(i)))
                    My.Computer.FileSystem.WriteAllText(bpath + "\" + fname, ts, False)
                End If
            End If
            ProgressBar2.Value = IIf(100.0 * (i + 1) / subc > 100, 100, 100.0 * (i + 1) / subc)
        Next
        prob += 1
        state = 4
        WebBrowser1.Navigate("about:blank")
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        MsgBox("Use these placeholders:" + vbCrLf + _
               "{prob}: Problem number." + vbCrLf + _
               "{id}: Submission ID." + vbCrLf + _
               "{res}: Judge verdict." + vbCrLf + _
               "{date}: Submission date (yyyyMMdd)" + vbCrLf + _
               "{time}: Submission time (HHmmss)." + vbCrLf + _
               "{fmt}: File type.")
    End Sub
End Class
