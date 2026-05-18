using System;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Учет.Enums;
using Учет.Models;

namespace Учет.Services
{
    public class DocumentService : IDocumentService
    {
        public string Generate(Asset asset, string departmentName, DocType docType, string templatesPath)
        {
            var templatePath = Path.Combine(templatesPath, $"{docType}.docx");
            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Шаблон не найден: {templatePath}");

            var outputPath = Path.Combine(Path.GetTempPath(), $"Документ_{asset.InventoryNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.docx");
            File.Copy(templatePath, outputPath, true);

            using (var wordDoc = WordprocessingDocument.Open(outputPath, true))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;

                ReplaceText(body, "{{InventoryNumber}}", asset.InventoryNumber);
                ReplaceText(body, "{{ShortName}}", asset.ShortName);
                ReplaceText(body, "{{FullName}}", asset.FullName);
                ReplaceText(body, "{{Department}}", departmentName);
                ReplaceText(body, "{{CommissioningDate}}", asset.CommissioningDate.ToString("dd.MM.yyyy"));
                ReplaceText(body, "{{Status}}", GetStatusString(asset.Status));
                ReplaceText(body, "{{RepairCount}}", asset.RepairCount.ToString());
                ReplaceText(body, "{{CurrentDate}}", DateTime.Now.ToString("dd.MM.yyyy"));
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(outputPath) { UseShellExecute = true });
            return outputPath;
        }

        private void ReplaceText(Body body, string placeholder, string value)
        {
            foreach (var text in body.Descendants<Text>())
            {
                if (text.Text.Contains(placeholder))
                    text.Text = text.Text.Replace(placeholder, value);
            }
        }

        private string GetStatusString(int status)
        {
            return status switch
            {
                0 => "Зарегистрирован",
                1 => "В эксплуатации",
                2 => "На ремонте",
                3 => "На обслуживании",
                4 => "Просрочено",
                5 => "Списано",
                _ => "Неизвестно"
            };
        }
    }
}