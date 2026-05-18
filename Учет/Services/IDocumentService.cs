using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Учет.Enums;
using Учет.Models;

namespace Учет.Services
{
    public interface IDocumentService
    {
        string Generate(Asset asset, string departmentName, DocType docType, string templatesPath);
    }
}