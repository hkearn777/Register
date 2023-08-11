Public Class Form1
  'Register Program. 
  'Author. Howard Kearney
  'Date-Written. November 2017.
  'Description. Simplied checkbook register for my wife.
  'Change-History.
  ' 8/11/2023 hk v3 - display full path/filename of the csv data file
  ' 7/7/2018  hk v2 - Position DataGridView to last record on file
  '                 - Correct Tabbing positions
  '                 - Widen Screen so Scroll bar always shows
  '                 - Have csv data file reside in the UserProfile\Documents folder
  ' 11/1/2017 HK v1 New Code
  '----------------------------------------------------------------------------------------------------------
  Dim RegisterFile As String = "register.csv"
  Dim ProgramVersion As String = "v3"
  Dim AddedANewRow As Boolean = False
    Public Const MAKINGDEPOSIT As Short = 1
    Public Const MAKINGWITHDRAWAL As Short = 2

  Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        'before closing save the register file.
        Dim fName As String = Environment.GetEnvironmentVariable("UserProfile") & "\Documents\" & RegisterFile
        Dim cellvalue As String = ""
        Dim rowLine As String = ""
        Dim objWriter As New System.IO.StreamWriter(fName, False)
        Dim Delim As String = ","
        Dim LastRow As Integer = DataGridView1.Rows.Count - 1
        Dim LastCol As Integer = DataGridView1.Columns.Count - 1
        Dim Col As Integer = 0
        Dim Row As Integer = 0

        Try

      For Row = 0 To LastRow
        For Col = 0 To LastCol
          If Not TypeOf DataGridView1.CurrentRow.Cells.Item(Col).Value Is DBNull Then
            cellvalue = DataGridView1.Item(Col, Row).Value
            If Col > 1 Then 'format amount fields by removing leadings spaces and any commas
              cellvalue = LTrim(Str(Val(cellvalue)))
            End If
          Else
            cellvalue = ""
          End If
          'rowLine = rowLine + cellvalue
          ' if this is last field; do not add delimiter
          If Col = LastCol Then
            rowLine = rowLine + cellvalue
          Else
            rowLine = rowLine + cellvalue + Delim
          End If
        Next
        objWriter.WriteLine(rowLine)
        rowLine = ""
      Next

      objWriter.Close()
      MsgBox("Register saved, it's good to be saved.")

    Catch exc As Exception

      MessageBox.Show("Error occured while writing to the register." + exc.ToString())

    Finally
      FileClose(1)

    End Try

    Close()
  End Sub


  Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    Dim fName As String = Environment.GetEnvironmentVariable("UserProfile") & "\Documents\" & RegisterFile
    Dim RowNum As Integer = 0
    Dim NumberOfFields As Integer = 0
    Dim TextLine As String = ""
    Dim SplitLine() As String
    Dim PrevBalance As Long = 0
    Dim DepositAmount As Decimal = 0
    Dim WithdrawalAmount As Decimal = 0
    Dim NewBalanceAmount As Decimal = 0

    Text = "Register " & ProgramVersion     'puts version value onto the form title.

    If System.IO.File.Exists(fName) = True Then
      Dim objReader As New System.IO.StreamReader(fName)
      Do While objReader.Peek() <> -1
        RowNum = RowNum + 1
        TextLine = objReader.ReadLine()
        SplitLine = Split(TextLine, ",")
        NumberOfFields = SplitLine.Count
        If NumberOfFields <> 5 Then
          MsgBox("CSV must hold only 5 fields. row#" & RowNum, MsgBoxStyle.OkOnly, "Error")
          End
        End If
        If Val(SplitLine(2)) = 0 Then
          SplitLine(2) = ""
        End If
        If Val(SplitLine(2)) > 0 Then
          DepositAmount = Val(SplitLine(2))
          SplitLine(2) = FormatRegisterAmount(DepositAmount)
        End If
        If Val(SplitLine(3)) = 0 Then
          SplitLine(3) = ""
        End If
        If Val(SplitLine(3)) > 0 Then
          WithdrawalAmount = Val(SplitLine(3))
          SplitLine(3) = FormatRegisterAmount(WithdrawalAmount)
        End If
        NewBalanceAmount = Val(SplitLine(4))
        SplitLine(4) = FormatRegisterAmount(NewBalanceAmount)

        DataGridView1.Rows.Add(SplitLine)
      Loop
      objReader.Close()
    Else
      MsgBox("File " & fName & "Does Not Exist." & vbCrLf & "New file will be created at Close.")
    End If

    lblFileName.Text = fName & ", #Records " & DataGridView1.Rows.Count

    Call PositionToLastRow()

  End Sub

    Private Sub PositionToLastRow()
        Dim LastRow As Integer = DataGridView1.Rows.Count
        If LastRow > 0 Then
            DataGridView1.CurrentCell = DataGridView1.Rows(LastRow - 1).Cells(0)
        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        If MsgBox("Are you sure you want to Cancel", MsgBoxStyle.OkCancel, "Confirm Cancel Request") = vbOK Then
            Close()
        End If
    End Sub


    Private Sub btnDeposit_Click(sender As Object, e As EventArgs) Handles btnDeposit.Click
        If TextFieldsAreOkay() = True Then
            Deposit(ComputeNewBalance(MAKINGDEPOSIT, ComputePreviousBalance()))
            ClearTheTextFields()
            PositionToLastRow()
        End If
    End Sub

  Private Sub btnWithdraw_Click(sender As Object, e As EventArgs) Handles btnWithdraw.Click
        If TextFieldsAreOkay() = True Then
            Withdrawal(ComputeNewBalance(MAKINGWITHDRAWAL, ComputePreviousBalance()))
            ClearTheTextFields()
            PositionToLastRow()
        End If
    End Sub

  Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
    ClearTheTextFields()
  End Sub

  Private Sub ClearTheTextFields()
    txtDate.Text = ""
    txtDescription.Text = ""
    txtAmount.Text = ""
  End Sub

  Function TextFieldsAreOkay() As Boolean
        'validate text fields are okay
        If txtDate.TextLength = 0 Then
            MsgBox("Date must be entered.", MsgBoxStyle.OkOnly, "Error")
            Return False
        End If
        If Not IsDate(txtDate.Text) Then
            MsgBox("Date format invalid.", MsgBoxStyle.OkOnly, "Error")
            Return False
        End If
    If txtDescription.TextLength = 0 Then
      MsgBox("Description must be entered.", MsgBoxStyle.OkOnly, "Error")
      Return False
    End If
    If txtDescription.TextLength > 20 Then
      MsgBox("Description length must be <= 20 characters", MsgBoxStyle.OkOnly, "Error")
      Return False
    End If
    If txtDescription.Text.IndexOf(",") <> -1 Then
            MsgBox("Description must not have a COMMA.", MsgBoxStyle.OkOnly, "Error")
            Return False
        End If
        If txtAmount.TextLength = 0 Then
            MsgBox("Amount must be entered.", MsgBoxStyle.OkOnly, "Error")
            Return False
        End If
        Return True
  End Function

  Private Sub Withdrawal(NewBalance As Decimal)
    Dim row As String() = New String() _
       {txtDate.Text,
        txtDescription.Text,
        "",
        FormatRegisterAmount(txtAmount.Text),
        FormatRegisterAmount(LTrim(Str(NewBalance)))}
    DataGridView1.Rows.Add(row)
  End Sub

  Private Sub Deposit(NewBalance As Decimal)
    Dim row As String() = New String() _
       {txtDate.Text,
        txtDescription.Text,
        FormatRegisterAmount(txtAmount.Text),
        "",
        FormatRegisterAmount(LTrim(Str(NewBalance)))}
    DataGridView1.Rows.Add(row)
  End Sub

  Function FormatRegisterAmount(Amount As Decimal) As String
    Return String.Format("{0,10:N2}", Amount)
  End Function

  Function ComputePreviousBalance() As Decimal
    Dim row As Integer = 0
    Dim PreviousBalance As Decimal = 0.0
    row = DataGridView1.Rows.Count - 1
    If row < 0 Then 'there is no previous row
      Return 0
    End If
    PreviousBalance = Val(DataGridView1.Rows(row).Cells(4).Value)
    'MsgBox("PrevBal=" & PreviousBalance, MsgBoxStyle.OkOnly, "debug")
    Return PreviousBalance
  End Function

  Function ComputeNewBalance(ButtonPressed As Short, PreviousBalance As Decimal) As Decimal
    Dim Amount As Decimal = 0

    Dim NewBalance As Decimal = 0

    ' Compute the New Balance
    Amount = Val(txtAmount.Text)
    Select Case ButtonPressed
      Case MAKINGDEPOSIT
        NewBalance = PreviousBalance + Amount
      Case MAKINGWITHDRAWAL
        NewBalance = PreviousBalance - Amount
      Case Else
        MsgBox("Unsupported Feature", MsgBoxStyle.OkOnly, "Internal Error")
        End
    End Select
    'MsgBox("NewBal=" & NewBalance & ":Amount=" & Amount, MsgBoxStyle.OkOnly, "debug")
    Return NewBalance
  End Function

    Private Sub btnFind_Click(sender As Object, e As EventArgs) Handles btnFind.Click

    End Sub
End Class
