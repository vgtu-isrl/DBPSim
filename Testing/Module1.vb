Module Module1

    Sub Main()

        Dim x = 5
        If Dec(x) AndAlso Dec(x) Then
            Console.WriteLine("2x Dec" + x)
        End If

        Console.WriteLine(x)

        Console.ReadLine()


    End Sub

    Function Dec(ByRef x As Integer) As Boolean
        x -= 1
        If x < 0 Then
            Return True
        Else
            Return False
        End If
    End Function



End Module
