using AgiExperiment.AI.Domain.Data.Model;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Spire.Doc;
using Spire.Doc.Documents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace AgiExperiment.AI.Cortex.Extensions
{
    internal static class ContentExtensions
    {
        public static string ToText(this MessageAttachment attachment)
        {
            if (attachment == null)
            {
                throw new NullReferenceException("attachment");
            }

            if (attachment.ContentType == "application/pdf")
            {
                return ReadPdf(attachment.Content);
            }
            else if (attachment.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                return ReadWord(attachment.Content);
            }
            else if (attachment.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                return ReadExcel(attachment.Content);
            }

            throw new NotImplementedException("attachment.ContentType");
        }

        static string ReadPdf(byte[] pdfData)
        {
            using (var stream = new MemoryStream(pdfData))
            using (var document = PdfDocument.Open(stream))
            {
                StringBuilder sb = new StringBuilder();

                foreach (Page page in document.GetPages())
                {
                    sb.AppendLine(page.Text);
                }

                return sb.ToString();
            }
        }

        static string ReadWord(byte[] wordData)
        {
            using (var stream = new MemoryStream(wordData))
            {
                Document document = new Document(stream);
                StringBuilder sb = new StringBuilder();

                foreach (Section section in document.Sections)
                {
                    foreach (Paragraph paragraph in section.Paragraphs)
                    {
                        sb.AppendLine(paragraph.Text);
                    }
                }

                return sb.ToString();
            }
        }

        static string ReadExcel(byte[] xlsxData)
        {
            StringBuilder sb = new StringBuilder();
            using (var stream = new MemoryStream(xlsxData))
            {
                stream.Position = 0;
                XSSFWorkbook xssWorkbook = new XSSFWorkbook(stream);
                for (int i = 0; i < xssWorkbook.NumberOfSheets; i++)
                {
                    ISheet sheet = xssWorkbook.GetSheetAt(i);
                    sb.AppendLine($"Sheet: {sheet.SheetName}");
                    for (int j = sheet.FirstRowNum; j <= sheet.LastRowNum; j++)
                    {
                        IRow row = sheet.GetRow(j);
                        if (row == null) continue;
                        for (int k = row.FirstCellNum; k < row.LastCellNum; k++)
                        {
                            ICell cell = row.GetCell(k);
                            if (cell != null)
                            {
                                sb.Append(cell.ToString() + "\t");
                            }
                        }
                        sb.AppendLine();
                    }
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
    }
}
