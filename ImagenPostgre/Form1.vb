Option Explicit On
Option Strict On

Imports System.IO
Imports Npgsql

Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        If OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            'Suponiendo que tengas la imagen en un archivo, necesitaras un objeto fileStream para leer la imagen 
            'y otro FileInfo para obtener el tamaño. Además necesitarás un array de bytes para convertir la imagen 
            'antes de pasarsela a la base de datos.

            Dim imgInfo As New FileInfo(OpenFileDialog1.FileName)
            Dim imgStream As FileStream
            Dim imgByte(Convert.ToInt32(imgInfo.Length)) As Byte

            imgStream = imgInfo.OpenRead()

            ' Ahora cargamos la imagen en el array de bytes
            imgStream.Read(imgByte, 0, Convert.ToInt32(imgInfo.Length))
            imgStream.Close()

            ' Conectamos con la base de datos
            Dim BD As New BaseDeDatos()
            BD.InsertarImagen(TextBox1.Text, imgByte)

            'Cargamos la imagen en el PictureBox
            PictureBox1.Load(OpenFileDialog1.FileName)
        End If

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim BD As New BaseDeDatos()

        'Cargamos la imagen en el PictureBox desde la base de datos. Se busca por nombre del pais que hay en el textbox.
        PictureBox2.Image = BD.LeerImagen(TextBox2.Text)

    End Sub
End Class

Public Class BaseDeDatos
    Private ConexionConBD As NpgsqlConnection
    Private Orden As NpgsqlCommand
    Private Lector As NpgsqlDataReader

    Public Sub InsertarImagen(ByVal NombrePais As String, ByVal img As Byte())

        'Abrir la base de datos
        Dim strConexión As String = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                                  "192.168.0.2", "5432", "esports", "antonio", "esports")
        ConexionConBD = New NpgsqlConnection(strConexión)
        ConexionConBD.Open()

        'Creamos una consulta parametrizada. Usamos @nombre para los parámetros
        Dim Consulta As String = "INSERT INTO pais(nombre_pais, bandera_pais) VALUES (@pais, @imagen)"

        'Creamos la consulta y le asignamos los parámetros.
        Orden = New NpgsqlCommand(Consulta, ConexionConBD)
        Orden.Parameters.Add(New Npgsql.NpgsqlParameter("@imagen", NpgsqlTypes.NpgsqlDbType.Bytea))
        Orden.Parameters("@imagen").Value = img
        Orden.Parameters.Add(New Npgsql.NpgsqlParameter("@pais", NpgsqlTypes.NpgsqlDbType.Varchar))
        Orden.Parameters("@pais").Value = NombrePais

        Orden.ExecuteNonQuery()
        CerrarConexion()

    End Sub

    Public Function LeerImagen(ByVal NombrePais As String) As Image
        Dim tam As Integer = 0
        Dim img() As Byte = {}

        'Abrir la base de datos
        Dim strConexión As String = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                                  "192.168.0.2", "5432", "esports", "antonio", "esports")
        ConexionConBD = New NpgsqlConnection(strConexión)
        ConexionConBD.Open()

        'Creamos una consulta parametrizada. Usamos @nombre para los parámetros
        Dim Consulta As String = "Select bandera_pais, length(bandera_pais) as len from pais where nombre_pais = @pais"

        'Creamos la consulta y le asignamos los parámetros.
        Orden = New NpgsqlCommand(Consulta, ConexionConBD)
        Orden.Parameters.Add(New Npgsql.NpgsqlParameter("@pais", NpgsqlTypes.NpgsqlDbType.Varchar))
        Orden.Parameters("@pais").Value = NombrePais

        Lector = Orden.ExecuteReader()

        'Recogemos los datos obtenidos por la select, la imagen y su longitud.
        While Lector.Read()
            tam = CInt(Lector("len"))
            ReDim img(tam)
            img = CType(Lector("bandera_pais"), Byte())
        End While

        CerrarConexion()

        'Finalmente necesitaremos un MemoryStream para convertir la imagen a un objeto image de .Net
        Dim ms As New MemoryStream
        Dim imagen As Image

        ms.Write(img, 0, tam)
        imagen = Image.FromStream(ms)

        Return imagen
    End Function

    Public Sub CerrarConexion()
        ' Cerrar la conexión cuando ya no sea necesaria
        If (Not Lector Is Nothing) Then
            Lector.Close()
        End If
        If (Not ConexionConBD Is Nothing) Then
            ConexionConBD.Close()
        End If
    End Sub

End Class