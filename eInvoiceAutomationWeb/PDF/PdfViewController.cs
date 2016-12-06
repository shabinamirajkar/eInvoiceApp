
namespace eInvoiceAutomationWeb.PDF
{
    using System.Web.Mvc;

    /// <summary>
    /// Extends the controller with functionality for rendering PDF views
    /// </summary>
    /// 
    [Authorize]
    public class PdfViewController : Controller
    {
        private readonly HtmlViewRenderer htmlViewRenderer;
        private readonly StandardPdfRenderer standardPdfRenderer;

        public PdfViewController()
        {
            this.htmlViewRenderer = new HtmlViewRenderer();
            this.standardPdfRenderer = new StandardPdfRenderer();
        }

        protected ActionResult ViewPdf(string pageTitle, string viewName, object model)
        {
            // Render the view html to a string.
            string htmlText = this.htmlViewRenderer.RenderViewToString(this, viewName, model);


            string cssfile = Server.MapPath("~") + "\\Content\\Custom.css";

            System.IO.StreamReader myFile = new System.IO.StreamReader(cssfile);
            string cssText = myFile.ReadToEnd();

            myFile.Close();

            // Let the html be rendered into a PDF document through iTextSharp.
            byte[] buffer = standardPdfRenderer.Render(htmlText, pageTitle, cssText);


            // Return the PDF as a binary stream to the client.
            return new BinaryContentResult(buffer, "application/pdf");
        }

        public bool SavePdf(string pageTitle,  string viewName, string fileName, object model)
        { 
            // Render the view html to a string.
            string htmlText = this.htmlViewRenderer.RenderViewToString(this, viewName, model);


            string cssfile = Server.MapPath("~") + "\\Content\\Custom.css";

            System.IO.StreamReader myFile = new System.IO.StreamReader(cssfile);
            string cssText = myFile.ReadToEnd();

            myFile.Close();

            // Let the html be rendered into a PDF document through iTextSharp.
            byte[] buffer = standardPdfRenderer.Render(htmlText, pageTitle, cssText);


            string filepath = Server.MapPath("~") + "\\PDFDocs\\" + fileName;
            System.IO.File.WriteAllBytes(filepath, buffer);

            // Return the PDF as a binary stream to the client.
            // return new BinaryContentResult(buffer, "application/pdf");

            return true;
        }

    }
}