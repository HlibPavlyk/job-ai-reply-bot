using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;

namespace DjinniAIReplyBot.Application.Models.Telegram;

public static class PdfTools
{
    public static string ParsePdfToString(byte[] fileBytes)
    {
        StringBuilder textBuilder = new();

        using (var pdf = PdfDocument.Open(new MemoryStream(fileBytes)))
        {
            foreach (var page in pdf.GetPages())
            {
                textBuilder.AppendLine(page.Text);
            }
        }

        var result = textBuilder.ToString();
        if(result.Length < 10)
            throw new PdfDocumentFormatException("PDF parsing failed. Please provide a valid PDF file.");

        return result;
    }
}