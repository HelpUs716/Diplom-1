using Учет.Enums;
using Учет.Models;

namespace Учет.Services
{
    public interface IDocumentService
    {
        string Generate(Asset asset, string departmentName, DocType docType, string templatesPath);
    }
}