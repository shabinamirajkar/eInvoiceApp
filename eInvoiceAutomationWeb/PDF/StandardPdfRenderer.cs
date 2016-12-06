
namespace eInvoiceAutomationWeb.PDF
{
    using System.IO;

    using iTextSharp.text;
    using iTextSharp.text.html.simpleparser;
    using iTextSharp.text.pdf;
    using iTextSharp.text.xml.simpleparser;
    using iTextSharp.tool.xml;
    using System.Net;

    /// <summary>
    /// This class is responsible for rendering a html text string to a PDF document using the html renderer of iTextSharp.
    /// </summary>
    public class StandardPdfRenderer
    {
        private const int HorizontalMargin = 30;
        private const int VerticalMargin = 20;

        public byte[] Render(string htmlText, string pageTitle, string cssText)
        {
            byte[] renderedBuffer;

            using (var outputMemoryStream = new MemoryStream())
            {
                using (var pdfDocument = new Document(PageSize.A4.Rotate(), HorizontalMargin, HorizontalMargin, 10, VerticalMargin))
                {

                    PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDocument, outputMemoryStream);
                    pdfWriter.CloseStream = false;
                    pdfWriter.PageEvent = new PrintHeaderFooter { Title = pageTitle };
                    pdfDocument.Open();


                  //  using (var msCss = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(string.Empty)))
                    using (var msCss = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(cssText)))
                    {
                        using (var msHtml = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(htmlText)))
                        {
                            XMLWorkerHelper.GetInstance().ParseXHtml(pdfWriter, pdfDocument, msHtml, msCss);
                        }
                    }
                    pdfDocument.Close();


                    //using (var htmlViewReader = new StringReader(htmlText))
                    //{
                    //    //    //get the XMLWorkerHelper Instance
                    //    //    XMLWorkerHelper worker = XMLWorkerHelper.GetInstance();// getInstance();
                    //    //    //convert to PDF
                    //    //    worker.ParseXHtml(pdfWriter, pdfDocument, htmlViewReader);

                    //    //  //  using (var xmlWorker = new XMLWorker(pdfDocument, true))
                    //    //  //  {
                    //    //  //      xmlWorker.Parse(htmlViewReader);
                    //    // //   }


                    //    using (var htmlWorker = new  HTMLWorker(pdfDocument))
                    //    {
                    //        htmlWorker.Parse(htmlViewReader);
                    //    }
                    //}
                }

                renderedBuffer = new byte[outputMemoryStream.Position];
                outputMemoryStream.Position = 0;
                outputMemoryStream.Read(renderedBuffer, 0, renderedBuffer.Length);
            }

            return renderedBuffer;
        }
    }
}