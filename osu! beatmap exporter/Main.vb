Imports System.IO.Compression
Imports OsuParsers.Database
Imports OsuParsers.Decoders

Public Class Main
    Dim osupath As String
    Dim exportfoldername As String
    Dim beatmaplist As List(Of String)
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Text = "osu! beatmap exporter - Working..."
        Me.Enabled = False
        Dim scoresDb As ScoresDatabase = DatabaseDecoder.DecodeScores(osupath & "\scores.db")
        Dim osuDb As OsuDatabase = DatabaseDecoder.DecodeOsu(osupath & "\osu!.db")
        beatmaplist = Nothing
        beatmaplist = New List(Of String)
        Dim beatmaplistInfo As New List(Of String)

        If NumericUpDown1.Value = 0 Then
            For o = 1 To osuDb.Beatmaps.Count - 1
                If Not osuDb.Beatmaps(o).IsUnplayed = True Then
                    beatmaplist.Add(osuDb.Beatmaps(o).FolderName)
                    beatmaplistInfo.Add(osuDb.Beatmaps(o).LastPlayed & " | " & osuDb.Beatmaps(o).Artist & " - " & osuDb.Beatmaps(o).Title)
                End If
            Next
            exportfoldername = "_Beatmaps played at least once"
        Else
            For i = 1 To scoresDb.Scores.Count - 1
                If scoresDb.Scores(i).Item2.Count >= NumericUpDown1.Value Then
                    For o = 1 To osuDb.Beatmaps.Count - 1
                        If osuDb.Beatmaps(o).MD5Hash = scoresDb.Scores(i).Item1.ToString Then
                            beatmaplist.Add(osuDb.Beatmaps(o).FolderName)
                            Dim fillerspace As String = " replays "
                            If scoresDb.Scores(i).Item2.Count.ToString.Length = 1 Then
                                fillerspace = " replays    "
                            ElseIf scoresDb.Scores(i).Item2.Count.ToString.Length = 2 Then
                                fillerspace = " replays  "
                            ElseIf scoresDb.Scores(i).Item2.Count.ToString.Length = 3 Then
                                fillerspace = " replays"
                            End If
                            beatmaplistInfo.Add(scoresDb.Scores(i).Item2.Count & fillerspace & "| " & osuDb.Beatmaps(o).Artist & " - " & osuDb.Beatmaps(o).Title)
                        End If
                    Next
                End If
            Next
            exportfoldername = "_Beatmaps with at least " & NumericUpDown1.Value & " replays"
        End If
        beatmaplist = beatmaplist.Distinct.ToList()
        beatmaplistInfo = beatmaplistInfo.Distinct.ToList()
        TextBox1.Text = (String.Join(vbNewLine, beatmaplistInfo))
        Me.Enabled = True
        Button2.Visible = True
        Label4.Visible = True
        Label4.Text = beatmaplist.Count & " mapsets will be copied"
        RadioButton1.Visible = True
        RadioButton2.Visible = True
        Me.Text = "osu! beatmap exporter"
    End Sub

    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged
        If NumericUpDown1.Value = 0 Then
            Label1.Text = "Export all beatmaps that have been played at least once."
        Else
            Label1.Text = "Export all beatmaps with at least " & NumericUpDown1.Value & " replays."
        End If
        TextBox1.Clear()
        Button2.Visible = False
        Label4.Visible = False
        RadioButton1.Visible = False
        RadioButton2.Visible = False
    End Sub

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim processes As Process() = Process.GetProcessesByName("osu!")
        If processes.Length > 0 Then
            Dim process As Process = processes(0)
            osupath = process.MainModule.FileName.Replace("\osu!.exe", "")
            Label3.Visible = True
            Label3.Text = "Directory:" & vbNewLine & process.MainModule.FileName.Replace("\osu!.exe", "")
            Button1.Visible = True
            startbtn.Enabled = False
            Label2.Visible = True
            NumericUpDown1.Visible = True
            Label1.Visible = True
        End If
    End Sub
    Private Sub startbtn_Click(sender As Object, e As EventArgs) Handles startbtn.Click
        Dim processes As Process() = Process.GetProcessesByName("osu!")
        If processes.Length > 0 Then
            Dim process As Process = processes(0)
            osupath = process.MainModule.FileName.Replace("\osu!.exe", "")
            Label3.Visible = True
            Label3.Text = "Directory:" & vbNewLine & process.MainModule.FileName.Replace("\osu!.exe", "")
            Button1.Visible = True
            startbtn.Enabled = False
            Label2.Visible = True
            NumericUpDown1.Visible = True
            Label1.Visible = True
        Else
            If (FolderBrowserDialog1.ShowDialog() = DialogResult.OK) Then
                osupath = FolderBrowserDialog1.SelectedPath
                Label3.Visible = True
                Label3.Text = "Directory:" & vbNewLine & FolderBrowserDialog1.SelectedPath
                Button1.Visible = True
                startbtn.Enabled = False
                Label2.Visible = True
                NumericUpDown1.Visible = True
                Label1.Visible = True
            End If
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Text = "osu! beatmap exporter - Working..."
        ProgressBar1.Visible = True
        statustxt.Visible = True
        Me.Enabled = False
        ProgressBar1.Maximum = beatmaplist.Count
        ProgressBar1.Value = 0
        Dim exportfolder As String = osupath & "\" & exportfoldername
        If Not System.IO.Directory.Exists(exportfolder) Then
            System.IO.Directory.CreateDirectory(exportfolder)
        End If
        For Each beatmap In beatmaplist
            Try
                Dim SourcePath As String = osupath & "\Songs\" & beatmap.ToString
                Dim DestinationPath As String = exportfolder & "\" & beatmap.ToString
                statustxt.Text = "Copying: " & beatmap.ToString
                If RadioButton1.Checked = True Then
                    Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(SourcePath, DestinationPath)
                Else
                    ZipFile.CreateFromDirectory(SourcePath, DestinationPath & ".osz", CompressionLevel.NoCompression, False)
                End If

            Catch ex As Exception
            End Try
            ProgressBar1.Value += 1
            Application.DoEvents()
        Next
        ProgressBar1.Value = 0
        Me.Enabled = True
        statustxt.Visible = False
        ProgressBar1.Visible = False
        Me.Text = "osu! beatmap exporter"
        Process.Start("explorer.exe", "/root," & exportfolder)
    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click
        Dim webAddress As String = "https://www.youtube.com/channel/UC8COCm4lP0hsQqbFUVu9lKg"
        Process.Start(webAddress)
    End Sub
End Class