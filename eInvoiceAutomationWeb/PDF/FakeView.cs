namespace eInvoiceAutomationWeb.PDF
{
    using System;
    using System.IO;
    using System.Web.Mvc;

    public class FakeView : IView
    {
        #region IView Members

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}