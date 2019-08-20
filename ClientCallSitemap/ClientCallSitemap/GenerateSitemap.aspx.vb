Imports System.IO
Imports System.Net
Imports System.Threading

Public Class GenerateSitemap
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Create a request for the URL. 
        Dim request As WebRequest =
          WebRequest.Create("https://localhost:44361/SiteMapUsingCrawler/CrawlSite2")
        ' If required by the server, set the credentials.
        request.Timeout = Timeout.Infinite
        request.Credentials = CredentialCache.DefaultCredentials

        ' Get the response.
        Dim response As WebResponse = request.GetResponse()
        ' Display the status.
        LitResult.Text = "Response status: " & CType(response, HttpWebResponse).StatusDescription
        ' Get the stream containing content returned by the server.
        Dim dataStream As Stream = response.GetResponseStream()
        ' Open the stream using a StreamReader for easy access.
        Dim reader As New StreamReader(dataStream)
        ' Read the content.
        Dim responseFromServer As String = reader.ReadToEnd()

        Dim doc As XDocument = XDocument.Parse(responseFromServer)
        doc.Save(Server.MapPath("~") & "\Sitemap.xml", SaveOptions.None)

        'File.WriteAllText(Server.MapPath("~") & "\Sitemap.xml", responseFromServer)
        ' Clean up the streams and the response.
        reader.Close()
        response.Close()
    End Sub

End Class